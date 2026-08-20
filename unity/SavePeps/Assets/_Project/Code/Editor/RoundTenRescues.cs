using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    public static class RoundTenRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r28 = BuildWarm(overwrite, log);
            var r29 = BuildShield(overwrite, log);
            var r30 = BuildBalance(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_10.asset", overwrite, log, out var round))
            {
                round.Number = 10;
                round.Rescues = new[] { r28, r29, r30 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildWarm(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r28_warm.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r28", "warm", "Defrost airlock.", Difficulty.Medium,
                ReasoningKind.Temperature, "Diorama_Space",
                "Extreme cosmic cold freezes the orbital airlock solid; focused warmth will thaw the mechanism.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Prop("hair_dryer"), AnchorId = "Slot_1", Label = "The hair dryer",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Move(0.6f, 0.65f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 5f, ease: EaseKind.InOut),
                        Resize(0.7f, 0.65f, "AirlockHatch", 0.1f, EaseKind.In),
                        Face(0.8f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.35f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.85f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(1.95f, 0.75f),
                        Sfx(2.0f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "plank", Prop = Prop("plank"), AnchorId = "Slot_2", Label = "The wooden plank",
                    Quip = "Frozen solid to the ice.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Prop("bell"), AnchorId = "Slot_3", Label = "The golden bell",
                    Quip = "Chimes freeze in orbit.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "ring"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildShield(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r29_shield.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r29", "shield", "Shield cosmic dust.", Difficulty.Medium,
                ReasoningKind.Shelter, "Diorama_Space",
                "A micrometeorite storm pelts the satellite pathway; a sturdy canopy provides protection.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_1", Label = "The heavy stone",
                    Quip = "Heavy stone drops away.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Quip = "Stardust shreds pillow.",
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
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_3", Label = "The yellow umbrella",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.25f, -1.3f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Face(0.7f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.75f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.85f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildBalance(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r30_balance.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r30", "stabilize", "Balance solar wing.", Difficulty.Surprising,
                ReasoningKind.Counterweight, "Diorama_Space",
                "The solar platform counterweight requires precise mass to align with the receiving airlock.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "rope", Prop = Prop("rope"), AnchorId = "Slot_1", Label = "The coiled rope",
                    Quip = "Tangled in zero gravity.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_2", Label = "The heavy stone",
                    Duration = 3.0f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.2f, 1.35f), amplitude: 0.3f, ease: EaseKind.Hop),
                        Sfx(0.6f, "thud"),
                        Move(0.65f, 0.65f, StepKind.Fly, "SolarWing",
                            new Vector3(0f, 0.36f, 0f), amplitude: 0f, ease: EaseKind.Out),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.35f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(2.05f, 0.75f),
                        Sfx(2.1f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Prop("watering_can"), AnchorId = "Slot_3", Label = "The blue watering can",
                    Quip = "Water floats away.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "splash"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
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
