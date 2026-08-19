using System.Collections.Generic;
using System.IO;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Seeds round one: three rescues staged in the Brook diorama, the round
    /// that orders them, and the catalogue.
    ///
    /// **Seeding creates; it does not overwrite.** An asset that already exists
    /// is left exactly as it is. That rule matters now that rescues are
    /// authored in the inspector: this file was how the first three rescues
    /// came into being, but the assets on disk are the source of truth the
    /// moment anybody edits one, and a generator that silently reasserts itself
    /// would quietly discard tuning work. Re-seeding is available, but it asks
    /// first and says what it is about to destroy.
    ///
    /// The catalogue is protected for a sharper reason: <c>FreeRoundCount</c> is
    /// the release-week lever from decision D3. Setting it to 10 for launch and
    /// then having a tool reset it to the development value is precisely the
    /// kind of silent regression that ships a game with its paywall in the
    /// wrong place.
    ///
    /// All three rescues share one diorama. That is the architecture working as
    /// PLAN §6 intends — eight dioramas host thirty-six rescues — and what has
    /// to differ is the *reasoning*: bridge the gap, remove the water, ride the
    /// water.
    /// </summary>
    public static class BrookRescues
    {
        /// <summary>Records what a seed run touched, so it can say so afterwards.</summary>
        public sealed class SeedLog
        {
            public readonly List<string> Written = new();
            public readonly List<string> Kept = new();

            public override string ToString()
            {
                var parts = new List<string>();
                if (Written.Count > 0) parts.Add("wrote " + string.Join(", ", Written));
                if (Kept.Count > 0) parts.Add("kept " + string.Join(", ", Kept));
                return parts.Count == 0 ? "nothing to do" : string.Join("; ", parts);
            }
        }

        // -------------------------------------------------------------------
        // Menu
        // -------------------------------------------------------------------

        [MenuItem("Tools/Save Peps/Seed Round One Content")]
        public static void SeedFromMenu()
        {
            var log = new SeedLog();
            Seed(overwrite: false, log);
            Debug.Log($"[SavePeps] Seed: {log}.");
        }

        [MenuItem("Tools/Save Peps/Danger/Re-seed Round One Content (discards edits)")]
        public static void ReseedFromMenu()
        {
            var existing = new List<string>();
            foreach (var path in new[]
                     {
                         $"{ContentPaths.RescueDir}/r01_brook.asset",
                         $"{ContentPaths.RescueDir}/r02_dam.asset",
                         $"{ContentPaths.RescueDir}/r03_ferry.asset",
                         $"{ContentPaths.RoundDir}/Round_01.asset",
                         ContentPaths.CatalogPath,
                     })
            {
                if (AssetDatabase.LoadAssetAtPath<ScriptableObject>(path) != null) existing.Add(Path.GetFileName(path));
            }

            // A batch or CI run has nobody to answer the dialog; the explicit
            // choice of this menu item is the confirmation in that case.
            if (!Application.isBatchMode && existing.Count > 0 && !EditorUtility.DisplayDialog(
                    "Re-seed round one?",
                    "This overwrites the authored assets with the versions defined in code, " +
                    "discarding any inspector edits:\n\n" +
                    string.Join("\n", existing) +
                    "\n\nThe catalogue's FreeRoundCount will be reset to the development value.",
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

        /// <summary>
        /// Creates any missing round-one content. With <paramref name="overwrite"/>
        /// set, rewrites it all from code instead.
        /// </summary>
        public static Catalog Seed(bool overwrite, SeedLog log = null)
        {
            log ??= new SeedLog();

            Directory.CreateDirectory(ContentPaths.RescueDir);
            Directory.CreateDirectory(ContentPaths.RoundDir);

            var r01 = BuildBridge(overwrite, log);
            var r02 = BuildDam(overwrite, log);
            var r03 = BuildFerry(overwrite, log);

            if (Claim<RoundDefinition>($"{ContentPaths.RoundDir}/Round_01.asset", overwrite, log, out var round1))
            {
                round1.Number = 1;
                round1.Rescues = new[] { r01, r02, r03 };
                EditorUtility.SetDirty(round1);
            }

            if (Claim<Catalog>(ContentPaths.CatalogPath, overwrite, log, out var catalog))
            {
                catalog.Rounds = new[] { round1 };
                // Ten free rounds is the shipping value (D3), but only one
                // round exists today. Leaving it at 10 would make the paywall
                // unreachable and untestable for the whole content sprint, so
                // the seed carries the honest number and release week sets it.
                catalog.FreeRoundCount = 1;
                EditorUtility.SetDirty(catalog);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return catalog;
        }

        /// <summary>
        /// Gets the asset at <paramref name="path"/>, creating it if absent.
        /// Returns true when the caller should write content into it — which is
        /// only when it was just created, or when overwriting was asked for.
        /// </summary>
        private static bool Claim<T>(string path, bool overwrite, SeedLog log, out T asset)
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

        // -------------------------------------------------------------------
        // r01 — bridge the gap
        // -------------------------------------------------------------------

        private static RescueDefinition BuildBridge(bool overwrite, SeedLog log)
        {
            if (!Claim<RescueDefinition>($"{ContentPaths.RescueDir}/r01_brook.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r01", "bridge", "Bring them together.", Difficulty.Easy,
                "Two Peps stand on opposite banks of a small brook, leaning toward each other.");

            rescue.Objects = new[]
            {
                // ---- correct: the plank bridges the brook -----------------
                new RescueObject
                {
                    Id = "plank", Prop = Prop("plank"), AnchorId = "Slot_1", Label = "The wooden plank",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0.0f, 0.7f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.02f, 1.25f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.68f, "thud"),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.9f, 0.95f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Haptic(0.95f, "light"),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },

                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_2", Label = "The electric fan",
                    Quip = "Excellent breeze. Entirely the wrong direction.",
                    Duration = 2.3f,
                    Steps = PropGags.Fan(),
                },

                new RescueObject
                {
                    Id = "balloon", Prop = Prop("balloon"), AnchorId = "Slot_3", Label = "The red balloon",
                    Quip = "Now they are even further apart. Vertically.",
                    Duration = 2.5f,
                    Steps = PropGags.Balloon(),
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        // -------------------------------------------------------------------
        // r02 — remove the water
        // -------------------------------------------------------------------

        private static RescueDefinition BuildDam(bool overwrite, SeedLog log)
        {
            if (!Claim<RescueDefinition>($"{ContentPaths.RescueDir}/r02_dam.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r02", "dam", "Get them across.", Difficulty.Easy,
                "Two Peps face each other across a running brook. A heavy stone sits on the near bank.");

            rescue.Objects = new[]
            {
                // ---- correct: dam the brook and it drains ------------------
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_2", Label = "The heavy stone",
                    Duration = 3.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        // Aimed from Slot_2 to the middle of the brook. Deltas
                        // are slot-relative, so moving a prop between slots
                        // means re-aiming whatever it does.
                        Move(0.0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, -0.02f, 1.35f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Sfx(0.6f, "splash"),
                        Haptic(0.62f, "medium"),
                        // The brook is a mover, so draining it is one step
                        // against the diorama rather than a bespoke script.
                        Move(0.72f, 0.8f, StepKind.Fly, "Water",
                            new Vector3(0f, -0.075f, 0f), ease: EaseKind.InOut),
                        Face(0.85f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.2f, 0.95f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Haptic(1.25f, "light"),
                        Meet(2.25f, 0.75f),
                        Sfx(2.3f, "reunion"),
                    },
                },

                new RescueObject
                {
                    Id = "balloon", Prop = Prop("balloon"), AnchorId = "Slot_3", Label = "The red balloon",
                    Quip = "Airborne. Still on opposite banks.",
                    Duration = 2.5f,
                    Steps = PropGags.Balloon(),
                },

                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_1", Label = "The orange umbrella",
                    Quip = "Beautifully dry. Still two banks.",
                    Duration = 2.3f,
                    Steps = PropGags.Umbrella(),
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        // -------------------------------------------------------------------
        // r03 — ride the water
        // -------------------------------------------------------------------

        private static RescueDefinition BuildFerry(bool overwrite, SeedLog log)
        {
            if (!Claim<RescueDefinition>($"{ContentPaths.RescueDir}/r03_ferry.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r03", "ferry", "Reach the far bank.", Difficulty.Medium,
                "Two Peps across a brook. A broad flat leaf lies on the near bank, big enough to stand on.");

            rescue.Objects = new[]
            {
                // ---- correct: the leaf is a raft --------------------------
                new RescueObject
                {
                    Id = "leaf", Prop = Prop("leaf"), AnchorId = "Slot_3", Label = "The broad leaf",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        // From the far bank, sweeping across to the water in
                        // front of Pep A. The long arc is worth having: it
                        // shows the leaf is a thing that floats before anyone
                        // is asked to stand on it.
                        Move(0.0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, -0.085f, -1.55f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Sfx(0.65f, "splash"),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        // Aboard, then adrift. Two independent steps rather
                        // than one hand-computed path — the additive model is
                        // what makes "stand on the thing, then move with the
                        // thing" composable.
                        Move(0.9f, 0.5f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, -0.06f, 0.37f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Move(1.5f, 0.9f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0f, 0.55f), ease: EaseKind.InOut),
                        Move(1.5f, 0.9f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.55f), ease: EaseKind.InOut),
                        Haptic(1.55f, "light"),
                        Move(2.45f, 0.45f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0.06f, 0.18f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(2.95f, 0.6f),
                        Sfx(3.0f, "reunion"),
                    },
                },

                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_2", Label = "The electric fan",
                    Quip = "The current runs that way. The breeze does not.",
                    Duration = 2.3f,
                    Steps = PropGags.Fan(),
                },

                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_1", Label = "The orange umbrella",
                    Quip = "It made the crossing. Alone.",
                    Duration = 2.3f,
                    Steps = PropGags.Umbrella(),
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        /// <summary>Everything a Brook rescue shares: diorama, Peps, anchors.</summary>
        private static void Stage(RescueDefinition rescue, string id, string verb, string goal,
            Difficulty difficulty, string description)
        {
            rescue.Id = id;
            rescue.Verb = verb;
            rescue.Goal = goal;
            rescue.Difficulty = difficulty;
            rescue.SceneDescription = description;

            rescue.Environment = Load<GameObject>($"{ContentPaths.EnvironmentDir}/Diorama_Brook.prefab");
            rescue.PepAPrefab = Load<GameObject>($"{ContentPaths.CharacterDir}/Pep_A.prefab");
            rescue.PepBPrefab = Load<GameObject>($"{ContentPaths.CharacterDir}/Pep_B.prefab");
            rescue.PepAAnchor = "Anchor_PepA";
            rescue.PepBAnchor = "Anchor_PepB";
            rescue.MeetAnchor = "Anchor_Meet";
        }

        private static GameObject Prop(string id) => Load<GameObject>($"{ContentPaths.PropDir}/{id}.prefab");

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.LogError($"[SavePeps] Missing asset: {path}");
            return asset;
        }
    }
}
