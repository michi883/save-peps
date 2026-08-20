using System.Collections.Generic;
using System.IO;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Assembles the catalogue from the per-environment seeders.
    ///
    /// The safety rule from the round-one work still holds and is the reason
    /// this is not simply "write the catalogue": **seeding may add content, but
    /// it must never destroy authored values.** Adding a new round to the
    /// catalogue is additive and expected; resetting <c>FreeRoundCount</c> is
    /// not, because that field is the release-week paywall lever from decision
    /// D3 and a tool that silently moved it would ship the gate in the wrong
    /// place.
    /// </summary>
    public static class ContentSeeder
    {
        /// <summary>Records what a seed run touched, so it can say so afterwards.</summary>
        public sealed class SeedLog
        {
            public readonly List<string> Written = new();
            public readonly List<string> Kept = new();
            public readonly List<string> Extended = new();

            public override string ToString()
            {
                var parts = new List<string>();
                if (Written.Count > 0) parts.Add("wrote " + string.Join(", ", Written));
                if (Extended.Count > 0) parts.Add("extended " + string.Join(", ", Extended));
                if (Kept.Count > 0) parts.Add("kept " + string.Join(", ", Kept));
                return parts.Count == 0 ? "nothing to do" : string.Join("; ", parts);
            }
        }

        // -------------------------------------------------------------------
        // Menu
        // -------------------------------------------------------------------

        [MenuItem("Tools/Save Peps/Seed Content")]
        public static void SeedFromMenu()
        {
            var log = new SeedLog();
            Seed(overwrite: false, log);
            Debug.Log($"[SavePeps] Seed: {log}.");
        }

        [MenuItem("Tools/Save Peps/Danger/Re-seed All Content (discards edits)")]
        public static void ReseedFromMenu()
        {
            // A batch or CI run has nobody to answer the dialog; choosing this
            // menu item is itself the confirmation in that case.
            if (!Application.isBatchMode && !EditorUtility.DisplayDialog(
                    "Re-seed all content?",
                    "This overwrites every authored rescue, round and the catalogue with the versions " +
                    "defined in code, discarding any inspector edits.\n\n" +
                    "The catalogue's FreeRoundCount will be reset to the development value.",
                    "Overwrite", "Cancel"))
            {
                return;
            }

            var log = new SeedLog();
            Seed(overwrite: true, log);
            Debug.LogWarning($"[SavePeps] Re-seeded: {log}.");
        }

        // -------------------------------------------------------------------
        // Seeding
        // -------------------------------------------------------------------

        public static Catalog Seed(bool overwrite, SeedLog log = null)
        {
            log ??= new SeedLog();

            Directory.CreateDirectory(ContentPaths.RescueDir);
            Directory.CreateDirectory(ContentPaths.RoundDir);

            MigrateLegacyRescuePath("r02_dam", "r02_wake", log);
            MigrateLegacyRescuePath("r03_ferry", "r03_free", log);
            MigrateLegacyRescuePath("r04_swing", "r04_distract", log);
            MigrateLegacyRescuePath("r05_lift", "r05_balance", log);
            MigrateLegacyRescuePath("r06_glide", "r06_reflect", log);

            var rounds = new[]
            {
                RoundOneRescues.SeedRound(overwrite, log),
                RoundTwoRescues.SeedRound(overwrite, log),
                RoundThreeRescues.SeedRound(overwrite, log),
            };

            var catalog = EnsureCatalog(rounds, overwrite, log);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return catalog;
        }

        /// <summary>
        /// Keeps stable asset GUIDs and therefore save/catalogue references
        /// while replacing the first prototype's crossing-specific filenames.
        /// This is a move, not a recreation; inspector history survives it.
        /// </summary>
        private static void MigrateLegacyRescuePath(string oldName, string newName, SeedLog log)
        {
            var oldPath = $"{ContentPaths.RescueDir}/{oldName}.asset";
            var newPath = $"{ContentPaths.RescueDir}/{newName}.asset";
            if (AssetDatabase.LoadAssetAtPath<RescueDefinition>(oldPath) == null ||
                AssetDatabase.LoadAssetAtPath<RescueDefinition>(newPath) != null)
            {
                return;
            }

            var error = AssetDatabase.MoveAsset(oldPath, newPath);
            if (!string.IsNullOrEmpty(error))
            {
                Debug.LogError($"[SavePeps] Could not rename {oldName} to {newName}: {error}");
                return;
            }

            log.Extended.Add($"{oldName}->{newName}");
        }

        /// <summary>
        /// Puts every seeded round into the catalogue in order, adding any that
        /// are missing and leaving everything else — order, extra rounds,
        /// FreeRoundCount — exactly as authored.
        /// </summary>
        private static Catalog EnsureCatalog(RoundDefinition[] rounds, bool overwrite, SeedLog log)
        {
            if (Claim<Catalog>(ContentPaths.CatalogPath, overwrite, log, out var catalog))
            {
                catalog.Rounds = rounds;
                // The free block is a product rule, not a count of whatever
                // happens to be authored today. Keeping it at ten means a new
                // round 4 cannot silently become premium during production.
                catalog.FreeRoundCount = Catalog.DefaultFreeRoundCount;
                EditorUtility.SetDirty(catalog);
                return catalog;
            }

            var existing = new List<RoundDefinition>(catalog.Rounds ?? System.Array.Empty<RoundDefinition>());
            var added = false;

            foreach (var round in rounds)
            {
                if (round == null || existing.Contains(round)) continue;
                existing.Add(round);
                log.Extended.Add(round.name);
                added = true;
            }

            if (!added) return catalog;

            existing.Sort((a, b) => (a?.Number ?? 0).CompareTo(b?.Number ?? 0));
            catalog.Rounds = existing.ToArray();
            EditorUtility.SetDirty(catalog);
            return catalog;
        }

        /// <summary>
        /// Gets the asset at <paramref name="path"/>, creating it if absent.
        /// Returns true when the caller should write content into it — which is
        /// only when it was just created, or when overwriting was asked for.
        /// </summary>
        public static bool Claim<T>(string path, bool overwrite, SeedLog log, out T asset)
            where T : ScriptableObject
        {
            asset = AssetDatabase.LoadAssetAtPath<T>(path);
            var name = Path.GetFileNameWithoutExtension(path);

            if (asset != null)
            {
                if (!overwrite)
                {
                    log.Kept.Add(name);
                    return false;
                }

                log.Written.Add(name);
                return true;
            }

            asset = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(asset, path);
            log.Written.Add(name);
            return true;
        }
    }
}
