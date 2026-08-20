using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    public static class RoundTwelveRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r34 = BuildBounce(overwrite, log);
            var r35 = BuildSnip(overwrite, log);
            var r36 = BuildFloat(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_12.asset", overwrite, log, out var round))
            {
                round.Number = 12;
                round.Rescues = new[] { r34, r35, r36 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildBounce(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r34_bounce.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r34", "bounce", "Cushion neon leap.", Difficulty.Medium,
                ReasoningKind.Activation, "Diorama_Neon",
                "A dramatic leap from a skyscraper ledge needs a soft, bouncy pad for a joyful landing.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "plank", Prop = Prop("plank"), AnchorId = "Slot_1", Label = "The wooden plank",
                    Quip = "Hard wooden clatter.",
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
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Duration = 3.0f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, -0.15f, 1.35f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "poof"),
                        Face(0.7f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.75f, 0.45f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0f, -0.15f, 0.62f), amplitude: 0.2f, ease: EaseKind.Hop),
                        Sfx(1.2f, "boing"),
                        Move(1.25f, 0.55f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0f, 0.15f, 0.5f), amplitude: 0.3f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_3", Label = "The heavy stone",
                    Quip = "Smashes rooftop skylight.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSnip(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r35_snip.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r35", "snip", "Cut cyber cable.", Difficulty.Medium,
                ReasoningKind.Cutting, "Diorama_Neon",
                "Sparks flicker from tangled cables blocking the fire escape; precision shears clear the obstruction.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_1", Label = "The craft scissors",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "snip"),
                        Resize(0.7f, 0.45f, "SkylineCable", 0.05f, EaseKind.In),
                        Face(0.8f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.15f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "leaf", Prop = Prop("leaf"), AnchorId = "Slot_2", Label = "The green leaf",
                    Quip = "Leaf cannot sever wire.",
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
                    Id = "hair_dryer", Prop = Prop("hair_dryer"), AnchorId = "Slot_3", Label = "The hair dryer",
                    Quip = "Overheats the transformer.",
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

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildFloat(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r36_float.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r36", "float", "Soar neon skyline.", Difficulty.Surprising,
                ReasoningKind.Crossing, "Diorama_Neon",
                "The grand finale: soaring high above the city skyline on a bright festive balloon to reunite forever!");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "bell", Prop = Prop("bell"), AnchorId = "Slot_1", Label = "The golden bell",
                    Quip = "Sinks into neon puddle.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "ring"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Prop("rope"), AnchorId = "Slot_2", Label = "The coiled rope",
                    Quip = "Rope drifts downstream.",
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
                    Id = "balloon", Prop = Prop("balloon"), AnchorId = "Slot_3", Label = "The bright balloon",
                    Duration = 3.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Face(0.7f, SceneRef.PepA, PepFace.Happy),
                        Move(0.75f, 1.2f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0f, 0.35f, 0.95f), amplitude: 0.45f, ease: EaseKind.Hop),
                        Meet(2.05f, 0.85f),
                        Sfx(2.1f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
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
