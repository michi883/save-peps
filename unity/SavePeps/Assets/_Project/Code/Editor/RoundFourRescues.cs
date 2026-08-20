using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Round four explores aerodynamic drift, acoustic calming, and precise
    /// hedge clearing. Introduces the Canyon chasm with thermal gliding.
    /// </summary>
    public static class RoundFourRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r10 = BuildGlide(overwrite, log);
            var r11 = BuildSoothe(overwrite, log);
            var r12 = BuildSever(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_04.asset", overwrite, log, out var round))
            {
                round.Number = 4;
                round.Rescues = new[] { r10, r11, r12 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildGlide(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r10_glide.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r10", "glide", "Cross the chasm.", Difficulty.Easy,
                ReasoningKind.Crossing, "Diorama_Canyon",
                "A wide rocky canyon separates the Peps, with a gentle updraft rising from below.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_1", Label = "The electric fan",
                    Quip = "Blew the canyon dust.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.8f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 8f, ease: EaseKind.InOut),
                        Face(0.2f, SceneRef.PepA, PepFace.Panic),
                        Move(0.3f, 0.5f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0f, -0.15f)),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_2", Label = "The orange umbrella",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Rotate(0f, 0.4f, SceneRef.Self, new Vector3(0f, 180f, 0f), EaseKind.Back),
                        Move(0.1f, 0.5f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.25f, 0f)),
                        Move(0.45f, 0.9f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Move(0.45f, 0.9f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0f, 1.12f), ease: EaseKind.InOut),
                        Sfx(1.35f, "thud"),
                        Face(1.4f, SceneRef.PepA, PepFace.Happy),
                        Meet(1.8f, 0.75f),
                        Sfx(1.85f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_3", Label = "The round stone",
                    Quip = "Dropped straight down.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.35f, -0.35f, 0.4f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Sfx(0.6f, "thud"),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                        Face(0.65f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSoothe(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r11_soothe.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r11", "soothe", "Calm the gate.", Difficulty.Medium,
                ReasoningKind.Luring, "Diorama_Wake",
                "A grumpy sleeping helper blocks the garden passage between the Peps.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Duration = 2.8f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.52f, 0.08f, 1.55f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "poof"),
                        Move(0.7f, 0.45f, StepKind.Shake, "Helper", Vector3.zero, amplitude: 2f, ease: EaseKind.InOut),
                        Move(0.85f, 0.65f, StepKind.Fly, "Gate", new Vector3(0f, -0.42f, 0f), ease: EaseKind.InOut),
                        Sfx(1.1f, "click"),
                        Face(1.15f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.2f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.85f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Prop("bell"), AnchorId = "Slot_2", Label = "The brass bell",
                    Quip = "Woke the grumpy gate.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "bell"),
                        Move(0f, 0.6f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 12f, ease: EaseKind.InOut),
                        Move(0.25f, 0.4f, StepKind.Hop, "Helper", new Vector3(0f, 0.15f, 0f), amplitude: 0.12f),
                        Face(0.4f, SceneRef.PepA, PepFace.Panic),
                        Face(0.4f, SceneRef.PepB, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_3", Label = "The dog bone",
                    Quip = "Helper is fast asleep.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.35f, 0.05f, 0.45f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.6f, "thud"),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSever(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r12_sever.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r12", "sever", "Clear the hedge.", Difficulty.Medium,
                ReasoningKind.Cutting, "Diorama_Vines",
                "Dense tangled brambles choke the garden archway between the Peps.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Prop("hair_dryer"), AnchorId = "Slot_1", Label = "The hair dryer",
                    Quip = "Warm thorns still prick.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.38f, 0.15f, 1.42f), amplitude: 0.28f, ease: EaseKind.Hop),
                        Move(0.55f, 0.8f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 5f, ease: EaseKind.InOut),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Prop("rope"), AnchorId = "Slot_2", Label = "The coil of rope",
                    Quip = "Even more tangled.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.35f, 0.15f, 1.35f), amplitude: 0.3f, ease: EaseKind.Hop),
                        Sfx(0.55f, "thud"),
                        Move(0.6f, 0.4f, StepKind.Shake, "Vines", Vector3.zero, amplitude: 3f),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_3", Label = "The garden shears",
                    Duration = 2.8f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.38f, 0.28f, -1.25f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Rotate(0.5f, 0.3f, SceneRef.Self, new Vector3(0f, 0f, 45f), EaseKind.Back),
                        Sfx(0.65f, "click"),
                        Resize(0.7f, 0.45f, "Vines", 0.01f, EaseKind.In),
                        Move(0.75f, 0.45f, StepKind.Drop, "Vines", new Vector3(0f, -0.4f, 0f)),
                        Face(0.85f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.1f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(1.8f, 0.75f),
                        Sfx(1.85f, "reunion"),
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
