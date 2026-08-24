using System;
using System.IO;
using UnityEngine;

namespace SavePeps.Progression
{
    /// <summary>
    /// Loads and persists <see cref="SaveData"/> as JSON in
    /// <c>Application.persistentDataPath</c>.
    ///
    /// A versioned file rather than PlayerPrefs: it can be inspected, copied
    /// off a device, unit-tested and migrated, none of which PlayerPrefs makes
    /// pleasant. Android Auto Backup carries this file across reinstall and
    /// device change, and that is the entire cloud-save story — no backend,
    /// per the brief.
    ///
    /// Writes are atomic (temp file, then swap) because the realistic failure
    /// is not disk corruption, it is the process being killed mid-write when
    /// the player backgrounds the app. A half-written save that parses as
    /// valid JSON but has lost the last round is worse than either extreme.
    /// </summary>
    public static class SaveStore
    {
        private const string FileName = "save.json";
        private const string TempSuffix = ".tmp";

        private static string Path => System.IO.Path.Combine(Application.persistentDataPath, FileName);
        private static string TempPath => Path + TempSuffix;

        /// <summary>
        /// Reads the save, or returns a fresh one. Never throws and never
        /// returns null: a player whose file is unreadable gets a new game,
        /// not a crash loop on the splash screen.
        /// </summary>
        public static SaveData Load()
        {
            var data = TryRead(Path);

            // The main file is missing or unreadable but a temp survives:
            // we were killed between the delete and the move. The temp is the
            // newer of the two by construction, so prefer it.
            if (data == null && File.Exists(TempPath))
            {
                data = TryRead(TempPath);
                if (data != null) Debug.LogWarning("[SavePeps] Recovered save from an interrupted write.");
            }

            if (data == null) return SaveData.Fresh();

            return Migrate(data);
        }

        private static SaveData TryRead(string path)
        {
            try
            {
                if (!File.Exists(path)) return null;
                var json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;

                // JsonUtility returns an object with defaults rather than null
                // for structurally valid but wrong JSON, so the version field
                // doubles as a sanity check that this is one of our files.
                var data = JsonUtility.FromJson<SaveData>(json);
                if (data == null || data.SchemaVersion <= 0) return null;
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SavePeps] Save at '{path}' is unreadable ({e.Message}). Starting fresh.");
                return null;
            }
        }

        /// <summary>
        /// Forward-compatible by design: JsonUtility ignores unknown fields and
        /// defaults missing ones, so a save written by a newer build still
        /// loads in an older one. This gate is only for changes that need real
        /// work — renaming a rescue id, say.
        /// </summary>
        private static SaveData Migrate(SaveData data)
        {
            if (data.SchemaVersion == SaveData.CurrentSchemaVersion) return data;

            if (data.SchemaVersion > SaveData.CurrentSchemaVersion)
            {
                // Downgrade: keep what parsed and carry on rather than wiping
                // a full-game owner's progress because they rolled back a build.
                Debug.LogWarning(
                    $"[SavePeps] Save is schema {data.SchemaVersion}, newer than this build's " +
                    $"{SaveData.CurrentSchemaVersion}. Loading it as-is.");
                return data;
            }

            // No migrations exist yet — v1 is the first shipped schema.
            data.SchemaVersion = SaveData.CurrentSchemaVersion;
            return data;
        }

        /// <summary>Writes the save. Returns false if it could not be persisted.</summary>
        public static bool Save(SaveData data)
        {
            if (data == null) return false;

            try
            {
                var json = JsonUtility.ToJson(data, prettyPrint: true);

                File.WriteAllText(TempPath, json);

                // File.Replace is not available on every Android backing store,
                // so this is delete-then-move. The window between the two is
                // the reason Load() knows how to recover from a stray temp.
                if (File.Exists(Path)) File.Delete(Path);
                File.Move(TempPath, Path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SavePeps] Could not write the save: {e.Message}");
                return false;
            }
        }

        /// <summary>Test and QA hook: removes the save entirely.</summary>
        public static void Delete()
        {
            try
            {
                if (File.Exists(Path)) File.Delete(Path);
                if (File.Exists(TempPath)) File.Delete(TempPath);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SavePeps] Could not delete the save: {e.Message}");
            }
        }
    }
}
