using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// The scalability round. It uses the same data timeline and generated
    /// toy vocabulary for temperature, biological growth, and weather — no
    /// rescue-specific runtime component or new step kind is involved.
    /// </summary>
    public static class RoundThreeRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r07 = BuildThaw(overwrite, log);
            var r08 = BuildGrow(overwrite, log);
            var r09 = BuildShelter(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_03.asset", overwrite, log, out var round))
            {
                round.Number = 3;
                round.Rescues = new[] { r07, r08, r09 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildThaw(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r07_thaw.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r07", "thaw", "Melt the ice.", Difficulty.Easy,
                ReasoningKind.Temperature, "Diorama_Thaw",
                "A mint Pep peers through a faceted ice shell while their coral partner waits diagonally across a snowy toy field.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "The ice is comfortable now.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.68f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.70f, 0.20f, 1.82f), amplitude: 0.36f, ease: EaseKind.Hop),
                        Sfx(0.68f, "poof"),
                        Resize(0.66f, 0.24f, SceneRef.Self, 0.82f, EaseKind.Hop),
                        Move(0.72f, 0.62f, StepKind.Shake, "IceShell", Vector3.zero,
                            amplitude: 3f, ease: EaseKind.InOut),
                        Face(0.82f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Prop("hair_dryer"), AnchorId = "Slot_2",
                    Label = "The coral hair dryer",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.15f, 0.16f, 1.38f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Move(0.60f, 0.72f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        Sfx(0.64f, "whoosh"),
                        Move(0.76f, 0.18f, StepKind.Hide, "IceShell", Vector3.zero),
                        Move(0.78f, 0.16f, StepKind.Show, "MeltPuddle", Vector3.zero),
                        Haptic(0.80f, "light"),
                        Face(0.84f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.84f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.12f, 0.72f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.22f, 0f, -0.34f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Meet(2.02f, 0.72f),
                        Sfx(2.07f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Prop("bell"), AnchorId = "Slot_3", Label = "The brass bell",
                    Quip = "Very awake. Still frozen.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.10f, 0.20f, 1.62f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(0.60f, "bell"),
                        Move(0.58f, 0.70f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 17f, ease: EaseKind.InOut),
                        Move(0.68f, 0.72f, StepKind.Shake, "IceShell", Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        Face(0.78f, SceneRef.PepA, PepFace.Worried),
                        Face(0.78f, SceneRef.PepB, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildGrow(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r08_grow.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r08", "grow", "Reach the ledge.", Difficulty.Medium,
                ReasoningKind.Growth, "Diorama_Grow",
                "A mint Pep balances on a tiny potted flower below the high terrace where their coral partner waits.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_1",
                    Label = "The red-handled scissors",
                    Quip = "Remarkably tidy. Considerably shorter.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.74f, 0.22f, 1.64f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(0.66f, "snip"),
                        Move(0.64f, 0.48f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 15f, ease: EaseKind.InOut),
                        Move(0.78f, 0.16f, StepKind.Hide, "Plant", Vector3.zero),
                        Move(0.80f, 0.42f, StepKind.Drop, SceneRef.PepB, new Vector3(0f, -0.06f, 0f)),
                        Face(0.84f, SceneRef.PepB, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Prop("balloon"), AnchorId = "Slot_2", Label = "The orange balloon",
                    Quip = "The ledge was not in the clouds.",
                    Duration = 2.7f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "boing"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.06f, 0.26f, 1.62f), amplitude: 0.36f, ease: EaseKind.Hop),
                        Face(0.50f, SceneRef.PepB, PepFace.Hopeful),
                        Move(0.66f, 0.68f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.48f, 0f)),
                        Move(0.66f, 0.68f, StepKind.Fly, SceneRef.PepB, new Vector3(0f, 0.48f, 0f)),
                        Face(1.02f, SceneRef.PepB, PepFace.Panic),
                        Move(1.38f, 0.86f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.25f, 0.95f, 0f), ease: EaseKind.In),
                        Move(1.38f, 0.86f, StepKind.FlyOff, SceneRef.PepB,
                            new Vector3(0.25f, 0.95f, 0f), ease: EaseKind.In),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Prop("watering_can"), AnchorId = "Slot_3",
                    Label = "The blue watering can",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.64f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.34f, 0.22f, 1.35f), amplitude: 0.32f, ease: EaseKind.Hop),
                        Rotate(0.58f, 0.34f, SceneRef.Self, new Vector3(0f, 0f, -42f)),
                        Sfx(0.62f, "splash"),
                        Resize(0.70f, 0.72f, "Plant", 1.80f, EaseKind.Back),
                        Move(0.70f, 0.72f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, 0.155f, 0f), ease: EaseKind.InOut),
                        Haptic(0.78f, "light"),
                        Face(0.90f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.90f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.46f, 0.66f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.29f, 0f, -0.30f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(2.20f, 0.70f),
                        Sfx(2.25f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildShelter(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r09_shelter.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r09", "shelter", "Stay out of rain.", Difficulty.Medium,
                ReasoningKind.Shelter, "Diorama_Rain",
                "One Pep waits beneath a tiny awning while rain falls from a side cloud around their huddled partner.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_1",
                    Label = "The orange umbrella",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.72f, 0.38f, 1.72f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Rotate(0.10f, 0.52f, SceneRef.Self, new Vector3(0f, 165f, 0f), EaseKind.Back),
                        Move(0.70f, 0.14f, StepKind.Hide, "Rain", Vector3.zero),
                        Move(0.72f, 0.78f, StepKind.FlyOff, "Cloud",
                            new Vector3(0.64f, 0.18f, 0.10f), ease: EaseKind.In),
                        Sfx(0.76f, "whoosh"),
                        Face(0.86f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.86f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.40f, 0.70f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.38f, 0f, -0.58f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Meet(2.20f, 0.70f),
                        Sfx(2.25f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_2", Label = "The caged electric fan",
                    Quip = "Now it is raining sideways.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.06f, 0.18f, 1.52f), amplitude: 0.32f, ease: EaseKind.Hop),
                        Move(0.58f, 1.0f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 7f, ease: EaseKind.InOut),
                        Rotate(0.64f, 0.66f, "Rain", new Vector3(0f, 0f, 28f)),
                        Move(0.64f, 0.66f, StepKind.Fly, "Rain", new Vector3(-0.34f, 0f, -0.12f)),
                        Face(0.78f, SceneRef.PepA, PepFace.Panic),
                        Move(0.82f, 0.52f, StepKind.Fly, SceneRef.PepA, new Vector3(-0.12f, 0f, -0.08f)),
                    },
                },
                new RescueObject
                {
                    Id = "mirror", Prop = Prop("mirror"), AnchorId = "Slot_3", Label = "The framed hand mirror",
                    Quip = "Bright idea. Persistent weather.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.34f, 0.20f, 1.30f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Rotate(0.54f, 0.34f, SceneRef.Self, new Vector3(0f, 48f, 0f), EaseKind.Back),
                        Sfx(0.62f, "chime"),
                        Move(0.68f, 0.72f, StepKind.Shake, "Cloud", Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        Face(0.80f, SceneRef.PepA, PepFace.Worried),
                        Face(0.80f, SceneRef.PepB, PepFace.Worried),
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
