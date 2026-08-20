using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    public static class RoundElevenRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r31 = BuildPropel(overwrite, log);
            var r32 = BuildNurture(overwrite, log);
            var r33 = BuildBridge(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_11.asset", overwrite, log, out var round))
            {
                round.Number = 11;
                round.Rescues = new[] { r31, r32, r33 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildPropel(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r31_propel.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r31", "chime", "Signal conveyor.", Difficulty.Medium,
                ReasoningKind.Activation, "Diorama_Factory",
                "Automated foundry machinery responds to acoustic frequencies; a resonant bell triggers the conveyor.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_1", Label = "The craft scissors",
                    Quip = "Cannot spin gears.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "snip"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Quip = "Pillow jams soft cog.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Prop("bell"), AnchorId = "Slot_3", Label = "The golden bell",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "ring"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.2f, -1.3f), amplitude: 0.3f, ease: EaseKind.Hop),
                        Move(0.6f, 0.6f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 8f, ease: EaseKind.InOut),
                        Move(0.65f, 0.65f, StepKind.Fly, "ConveyorBelt",
                            new Vector3(0f, 0.05f, 0f), amplitude: 0f, ease: EaseKind.Out),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.25f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.85f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(1.95f, 0.75f),
                        Sfx(2.0f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildNurture(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r32_nurture.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r32", "quench", "Cool iron gears.", Difficulty.Medium,
                ReasoningKind.Temperature, "Diorama_Factory",
                "Overheating foundry gears jam the main drawbridge; cold water quenches the hot mechanism.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "watering_can", Prop = Prop("watering_can"), AnchorId = "Slot_1", Label = "The blue watering can",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "splash"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Rotate(0.6f, 0.75f, "GearAssembly", new Vector3(0f, 0f, 180f), EaseKind.Out),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.35f, 0.6f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.85f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(1.95f, 0.75f),
                        Sfx(2.0f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "mirror", Prop = Prop("mirror"), AnchorId = "Slot_2", Label = "The shiny mirror",
                    Quip = "Dazzles the machine.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_3", Label = "The dog bone",
                    Quip = "Robots do not eat bones.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildBridge(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r33_bridge.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r33", "span", "Span molten vat.", Difficulty.Surprising,
                ReasoningKind.Crossing, "Diorama_Factory",
                "A wide gap over the molten metal furnace requires a sturdy platform to span the distance.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_1", Label = "The electric fan",
                    Quip = "Blown off industrial duct.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "plank", Prop = Prop("plank"), AnchorId = "Slot_2", Label = "The wooden plank",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "thud"),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.15f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_3", Label = "The yellow umbrella",
                    Quip = "Melts in factory steam.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
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
