using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Round two changes the subject and the motion again: lure an animal
    /// sideways, operate a vertical counterweight, then redirect light around
    /// a corner. None is a disguised crossing puzzle.
    /// </summary>
    public static class RoundTwoRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r04 = BuildDistract(overwrite, log);
            var r05 = BuildBalance(overwrite, log);
            var r06 = BuildReflect(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_02.asset", overwrite, log, out var round))
            {
                round.Number = 2;
                round.Rescues = new[] { r04, r05, r06 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildDistract(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r04_distract.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r04", "distract", "Distract the guard.", Difficulty.Easy,
                ReasoningKind.Luring, "Diorama_Guard",
                "A large toy dog sits in a diagonal garden opening between a foreground Pep and their partner behind the hedge.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_1", Label = "The orange umbrella",
                    Quip = "The guard appreciates the shade.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.34f, 1.36f), amplitude: 0.32f, ease: EaseKind.Hop),
                        Rotate(0.10f, 0.52f, SceneRef.Self, new Vector3(0f, 160f, 0f), EaseKind.Back),
                        Move(0.66f, 0.45f, StepKind.Drop, "Guard", new Vector3(0f, -0.045f, 0f)),
                        Face(0.76f, SceneRef.PepB, PepFace.Worried),
                        Sfx(0.82f, "sigh"),
                    },
                },
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_2", Label = "The white dog bone",
                    Duration = 3.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.52f, 0.05f, 1.18f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Sfx(0.62f, "thud"),
                        Move(0.64f, 0.72f, StepKind.Hop, "Guard",
                            new Vector3(0.60f, 0f, 0.08f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Face(0.78f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.78f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.38f, 0.72f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(0.47f, 0f, -0.90f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Haptic(1.42f, "light"),
                        Meet(2.20f, 0.70f),
                        Sfx(2.25f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_3", Label = "The caged electric fan",
                    Quip = "Excellent ear flaps. Impeccable guarding.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.0f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 7f, ease: EaseKind.InOut),
                        Move(0.18f, 0.72f, StepKind.Shake, "Guard", Vector3.zero,
                            amplitude: 8f, ease: EaseKind.InOut),
                        Move(0.28f, 0.45f, StepKind.Fly, "Guard", new Vector3(-0.07f, 0f, 0f)),
                        Move(0.80f, 0.42f, StepKind.Fly, "Guard", new Vector3(0.07f, 0f, 0f),
                            ease: EaseKind.InOut),
                        Face(0.42f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.15f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildBalance(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r05_balance.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r05", "balance", "Balance the lift.", Difficulty.Medium,
                ReasoningKind.Counterweight, "Diorama_Lift",
                "One Pep waits on a raised deck while the other stands below on a yellow lift connected to an empty high tray.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_1", Label = "The heavy cracked stone",
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.72f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-1.01f, 0.62f, 1.66f), amplitude: 0.50f, ease: EaseKind.Hop),
                        Sfx(0.72f, "thud"),
                        Haptic(0.74f, "medium"),
                        Move(0.76f, 0.72f, StepKind.Drop, "Counterweight",
                            new Vector3(0f, -0.55f, 0f), ease: EaseKind.InOut),
                        Move(0.76f, 0.72f, StepKind.Fly, "LiftPlatform",
                            new Vector3(0f, 0.36f, 0f), ease: EaseKind.InOut),
                        Move(0.76f, 0.72f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, 0.36f, 0f), ease: EaseKind.InOut),
                        Rotate(0.76f, 0.72f, "Pulley", new Vector3(0f, 0f, 200f), EaseKind.InOut),
                        Face(1.02f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.02f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.56f, 0.72f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.38f, -0.17f, -0.48f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Meet(2.38f, 0.72f),
                        Sfx(2.44f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Quip = "A counterweight with excellent lumbar support.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.68f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.03f, 0.58f, 1.66f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Sfx(0.68f, "boing"),
                        Resize(0.66f, 0.24f, SceneRef.Self, 0.72f, EaseKind.Hop),
                        Move(0.70f, 0.34f, StepKind.Drop, "Counterweight", new Vector3(0f, -0.06f, 0f)),
                        Move(1.06f, 0.34f, StepKind.Fly, "Counterweight", new Vector3(0f, 0.06f, 0f),
                            ease: EaseKind.Back),
                        Face(0.78f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.32f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Prop("balloon"), AnchorId = "Slot_3", Label = "The orange balloon",
                    Quip = "That is the opposite of counterweight.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.52f, 0.60f, 1.38f), amplitude: 0.36f, ease: EaseKind.Hop),
                        Sfx(0.64f, "boing"),
                        Move(0.68f, 0.82f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.40f, 0f)),
                        Move(0.68f, 0.82f, StepKind.Fly, "Counterweight", new Vector3(0f, 0.32f, 0f)),
                        Move(0.68f, 0.82f, StepKind.Drop, "LiftPlatform", new Vector3(0f, -0.12f, 0f)),
                        Move(0.68f, 0.82f, StepKind.Drop, SceneRef.PepB, new Vector3(0f, -0.12f, 0f)),
                        Rotate(0.68f, 0.82f, "Pulley", new Vector3(0f, 0f, -120f), EaseKind.InOut),
                        Face(0.82f, SceneRef.PepB, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildReflect(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r06_reflect.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r06", "reflect", "Bounce the beam.", Difficulty.Medium,
                ReasoningKind.Reflection, "Diorama_Beam",
                "A yellow lamp beam stops at an empty pedestal while a dark sensor beside the background Pep keeps a coral gate closed.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_1", Label = "The heavy cracked stone",
                    Quip = "A flawless demonstration of blocking light.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.52f, 0.05f, 1.25f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Sfx(0.62f, "thud"),
                        Move(0.62f, 0.14f, StepKind.Hide, "BeamIn", Vector3.zero),
                        Move(0.74f, 0.65f, StepKind.Shake, "LightGate", Vector3.zero,
                            amplitude: 3f, ease: EaseKind.InOut),
                        Face(0.82f, SceneRef.PepA, PepFace.Worried),
                        Face(0.82f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_2", Label = "The orange umbrella",
                    Quip = "Perfect shade. Sensors prefer light.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.48f, 0.20f, 1.25f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Rotate(0.10f, 0.52f, SceneRef.Self, new Vector3(0f, 150f, 0f), EaseKind.Back),
                        Move(0.62f, 0.14f, StepKind.Hide, "BeamIn", Vector3.zero),
                        Face(0.78f, SceneRef.PepA, PepFace.Worried),
                        Face(0.78f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "mirror", Prop = Prop("mirror"), AnchorId = "Slot_3", Label = "The framed hand mirror",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.02f, 0.15f, 0.93f), amplitude: 0.28f, ease: EaseKind.Hop),
                        Rotate(0.52f, 0.34f, SceneRef.Self, new Vector3(0f, 42f, 0f), EaseKind.Back),
                        Sfx(0.66f, "chime"),
                        Move(0.66f, 0.12f, StepKind.Show, "BeamBounce", Vector3.zero),
                        Move(0.72f, 0.12f, StepKind.Show, "SensorGlow", Vector3.zero),
                        Haptic(0.75f, "light"),
                        Move(0.84f, 0.56f, StepKind.Fly, "LightGate",
                            new Vector3(0f, 0.66f, 0f), ease: EaseKind.InOut),
                        Face(0.94f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.94f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.42f, 0.72f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.46f, 0f, -0.84f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.26f, 0.70f),
                        Sfx(2.32f, "reunion"),
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
