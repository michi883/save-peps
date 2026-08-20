using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Round six challenges optical redirection, storm shelter traversal,
    /// and soft cushion physics over dramatic vertical drops.
    /// </summary>
    public static class RoundSixRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r16 = BuildDeflect(overwrite, log);
            var r17 = BuildCover(overwrite, log);
            var r18 = BuildCushion(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_06.asset", overwrite, log, out var round))
            {
                round.Number = 6;
                round.Rescues = new[] { r16, r17, r18 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildDeflect(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r16_deflect.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r16", "deflect", "Aim the beam.", Difficulty.Medium,
                ReasoningKind.Reflection, "Diorama_Beam",
                "A light beam travels horizontally across the night diorama; aiming it into the sensor unlocks the light gate.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "mirror", Prop = Prop("mirror"), AnchorId = "Slot_1", Label = "The framed mirror",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.3f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.3f, SceneRef.Self, new Vector3(0f, 45f, 0f), EaseKind.Back),
                        Sfx(0.65f, "chime"),
                        Move(0.7f, 0.35f, StepKind.Fly, "SensorGlow", new Vector3(0f, 0.1f, 0f)),
                        Move(0.8f, 0.65f, StepKind.Fly, "LightGate", new Vector3(0f, -0.55f, 0f), ease: EaseKind.InOut),
                        Sfx(0.9f, "click"),
                        Face(0.95f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.2f, 0.6f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_2", Label = "The orange umbrella",
                    Quip = "Blocks the sensor.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.2f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.3f, SceneRef.Self, new Vector3(0f, 90f, 0f)),
                        Face(0.75f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_3", Label = "The soft pillow",
                    Quip = "Pillows absorb beams.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.28f, ease: EaseKind.Hop),
                        Sfx(0.6f, "poof"),
                        Face(0.7f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildCover(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r17_cover.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r17", "cover", "Brave the rain.", Difficulty.Medium,
                ReasoningKind.Shelter, "Diorama_Rain",
                "Rain lashes the walkway between the separated Peps; a sturdy shelter lets Pep A cross safely.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "leaf", Prop = Prop("leaf"), AnchorId = "Slot_1", Label = "The broad leaf",
                    Quip = "Tears in heavy rain.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Move(0.6f, 0.4f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 5f),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_2", Label = "The orange umbrella",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Rotate(0f, 0.35f, SceneRef.Self, new Vector3(0f, 180f, 0f), EaseKind.Back),
                        Move(0.1f, 0.5f, StepKind.Fly, SceneRef.Self, new Vector3(-0.45f, 0.45f, 0.75f)),
                        Face(0.6f, SceneRef.PepA, PepFace.Happy),
                        Move(0.7f, 0.85f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Move(0.7f, 0.85f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.45f, 0f, 0.37f), ease: EaseKind.InOut),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_3", Label = "The shears",
                    Quip = "Does not stop raindrops.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.3f, SceneRef.Self, new Vector3(0f, 0f, 45f)),
                        Face(0.7f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildCushion(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r18_cushion.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r18", "cushion", "Soft landing.", Difficulty.Surprising,
                ReasoningKind.Activation, "Diorama_Canyon",
                "A perilous drop into the canyon can be turned into a safe bouncy landing pad.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "plank", Prop = Prop("plank"), AnchorId = "Slot_1", Label = "The wooden plank",
                    Quip = "Hard wooden landing.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, -0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.6f, "thud"),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_2", Label = "The dog bone",
                    Quip = "Ouch! Hard bone.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, -0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.55f, "thud"),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_3", Label = "The soft pillow",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, -0.15f, -1.3f), amplitude: 0.35f, ease: EaseKind.Hop),
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
