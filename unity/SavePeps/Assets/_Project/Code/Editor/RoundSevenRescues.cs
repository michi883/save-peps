using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Round seven tests multi-step chain reactions: thermal defrosting,
    /// rapid botanical bridging, and strategic canine distraction.
    /// </summary>
    public static class RoundSevenRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r19 = BuildUnfreeze(overwrite, log);
            var r20 = BuildNourish(overwrite, log);
            var r21 = BuildEntice(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_07.asset", overwrite, log, out var round))
            {
                round.Number = 7;
                round.Rescues = new[] { r19, r20, r21 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildUnfreeze(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r19_unfreeze.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r19", "unfreeze", "Melt the frost.", Difficulty.Medium,
                ReasoningKind.Temperature, "Diorama_Thaw",
                "A thick layer of glacial frost encases Pep B on the snow field; direct heat is required.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "bell", Prop = Prop("bell"), AnchorId = "Slot_1", Label = "The brass bell",
                    Quip = "Chimes in the cold.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "bell"),
                        Move(0f, 0.6f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 12f, ease: EaseKind.InOut),
                        Move(0.2f, 0.35f, StepKind.Shake, "IceShell", Vector3.zero, amplitude: 2f),
                        Face(0.6f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Prop("hair_dryer"), AnchorId = "Slot_2", Label = "The hair dryer",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.32f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.3f, SceneRef.Self, new Vector3(0f, 45f, 0f)),
                        Sfx(0.65f, "slide"),
                        Resize(0.7f, 0.6f, "IceShell", 0.05f, EaseKind.In),
                        Move(0.75f, 0.5f, StepKind.Fly, "MeltPuddle", new Vector3(0f, 0.02f, 0f)),
                        Face(0.9f, SceneRef.PepB, PepFace.Happy),
                        Move(1.15f, 0.65f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(0f, 0f, -0.65f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_3", Label = "The electric fan",
                    Quip = "Just made it colder.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 6f, ease: EaseKind.InOut),
                        Face(0.35f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildNourish(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r20_nourish.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r20", "nourish", "Water the vine.", Difficulty.Medium,
                ReasoningKind.Growth, "Diorama_Grow",
                "A dormant vine seed sits in the garden gap between both Peps awaiting hydration.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_1", Label = "The shears",
                    Quip = "Trimmed too early.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.25f, SceneRef.Self, new Vector3(0f, 0f, 45f)),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_2", Label = "The treat bone",
                    Quip = "Plants don't eat bones.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.1f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.6f, "thud"),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Prop("watering_can"), AnchorId = "Slot_3", Label = "The blue watering can",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.35f, -1.3f), amplitude: 0.32f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.35f, SceneRef.Self, new Vector3(0f, 0f, -40f), EaseKind.InOut),
                        Sfx(0.7f, "splash"),
                        Resize(0.75f, 0.7f, "Plant", 2.1f, EaseKind.Back),
                        Face(0.85f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.25f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildEntice(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r21_entice.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r21", "entice", "Lure the pup.", Difficulty.Surprising,
                ReasoningKind.Luring, "Diorama_Guard",
                "A lively toy pup guards the passage; a tasty treat can lure it away from the gateway.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_1", Label = "The juicy bone",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.18f, 1.35f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "thud"),
                        Move(0.7f, 0.75f, StepKind.FlyOff, "Guard", new Vector3(0.9f, 0f, 0.6f), ease: EaseKind.In),
                        Sfx(0.85f, "boing"),
                        Face(0.95f, SceneRef.PepA, PepFace.Happy),
                        Move(1.2f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Prop("bell"), AnchorId = "Slot_2", Label = "The brass bell",
                    Quip = "Guard barks along.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "bell"),
                        Move(0f, 0.6f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 14f, ease: EaseKind.InOut),
                        Move(0.2f, 0.35f, StepKind.Hop, "Guard", new Vector3(0f, 0.15f, 0f)),
                        Face(0.6f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "leaf", Prop = Prop("leaf"), AnchorId = "Slot_3", Label = "The broad leaf",
                    Quip = "Not dog food.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.1f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static void Stage(RescueDefinition rescue, string id, string verb, string goal,
            Difficulty difficulty, ReasoningKind reasoning, string environment, string description)
        {
            rescue.Id = id;
            rescue.Verb = verb;
            rescue.Goal = goal;
            rescue.Difficulty = difficulty;
            rescue.Reasoning = reasoning;
            rescue.SceneDescription = description;
            rescue.Environment = Load<GameObject>($"{ContentPaths.EnvironmentDir}/{environment}.prefab");
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
