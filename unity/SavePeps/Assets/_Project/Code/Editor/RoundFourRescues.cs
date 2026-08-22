using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 4 — Windrock canyon.** *World rule: the gap is vertical as well
    /// as horizontal, the far rim is higher than the near one, and the air is
    /// going somewhere.*
    ///
    /// The round the old catalogue got most wrong: it was called Canyon and
    /// contained one canyon rescue plus two borrowed garden scenes. All three
    /// now stand on the two mesas, and all three are about mass and moving air
    /// rather than about crossing — nothing here is solved by laying something
    /// flat across the hole, and the plank is not even offered.
    ///
    /// Only-here rescue: **r12**, pulling the rock spire over to make a
    /// bridge out of the landscape itself. It needs a chasm with something
    /// standing in it, which exists in exactly one world.
    /// </summary>
    public static class RoundFourRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r10 = BuildGlide(overwrite, log);
            var r11 = BuildPlumb(overwrite, log);
            var r12 = BuildTopple(overwrite, log);

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

            Author.Stage(rescue, "r10", "glide", "Cross the chasm.", Difficulty.Medium,
                ReasoningKind.Airflow, "Diorama_Canyon_Updraft",
                "Warm air rises visibly out of the chasm between two red mesas; the far rim stands " +
                "higher than the near one.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_1", Label = "The caged electric fan",
                    Quip = "Blew the dust about.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Move(0.20f, 0.90f, StepKind.Shake, "Thermal", Vector3.zero,
                            amplitude: 9f, ease: EaseKind.InOut),
                        Face(0.40f, SceneRef.PepA, PepFace.Panic),
                        Move(0.46f, 0.70f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0f, -0.24f)),
                        Move(1.30f, 0.60f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0f, 0.06f),
                            ease: EaseKind.InOut),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_2",
                    Label = "The orange umbrella",
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.46f, 0.37f, 0.78f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(0.58f, "pop"),
                        Move(0.58f, 0.32f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 180f),
                        Resize(0.58f, 0.32f, SceneRef.Self, 1.22f, EaseKind.Back),
                        Sfx(0.92f, "wind"),
                        Move(0.94f, 1.15f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0f, 0.24f, 1.28f), amplitude: 0.58f, ease: EaseKind.InOut),
                        Move(0.94f, 1.15f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.24f, 1.28f), amplitude: 0.58f, ease: EaseKind.InOut),
                        Move(0.94f, 1.00f, StepKind.Fly, "Thermal", new Vector3(0f, 0.10f, 0f)),
                        Face(1.00f, SceneRef.PepA, PepFace.Happy),
                        Face(1.00f, SceneRef.PepB, PepFace.Hopeful),
                        Haptic(2.02f, "light"),
                        Meet(2.36f, 0.74f),
                        Sfx(2.42f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_3", Label = "The grey stone",
                    Quip = "Straight down. Quickly.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.56f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.16f, 1.02f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Face(0.60f, SceneRef.PepA, PepFace.Worried),
                        Move(0.62f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0f, -1.70f, 0.06f), ease: EaseKind.In),
                        Sfx(1.66f, "rumble"),
                        Face(1.72f, SceneRef.PepB, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildPlumb(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r11_plumb.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r11", "plumb", "Stop the swinging.", Difficulty.Medium,
                ReasoningKind.Counterweight, "Diorama_Canyon_Cablecar",
                "A wooden car hangs from a cable over the chasm and swings too wildly in the wind to " +
                "step into.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "feather", Prop = Author.Prop("feather"), AnchorId = "Slot_1", Label = "The white feather",
                    Quip = "The wind took it.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "wind"),
                        Move(0f, 0.50f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.20f, 0.30f, 0.60f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Face(0.56f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.60f, 1.10f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.90f, 0.95f, 0.30f), ease: EaseKind.In),
                        Face(1.60f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_2", Label = "The coil of rope",
                    Quip = "Now it swings harder.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.46f, 0.60f, 1.42f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Sfx(0.64f, "creak"),
                        Move(0.66f, 1.10f, StepKind.Shake, "Basket", Vector3.zero,
                            amplitude: 17f, ease: EaseKind.InOut),
                        Move(0.66f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 17f, ease: EaseKind.InOut),
                        Face(0.90f, SceneRef.PepA, PepFace.Panic),
                        Face(1.10f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "weight", Prop = Author.Prop("weight"), AnchorId = "Slot_3", Label = "The iron weight",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.24f, 1.09f), amplitude: 0.44f, ease: EaseKind.Hop),
                        Sfx(0.62f, "creak"),
                        Haptic(0.64f, "medium"),
                        // The swinging car and the still one are two objects.
                        // An additive delta can add motion but never cancel an
                        // idle, so settling is a swap rather than a damping.
                        Move(0.86f, 0.14f, StepKind.Hide, "Basket", Vector3.zero),
                        Move(0.86f, 0.14f, StepKind.Show, "SteadyCar", Vector3.zero),
                        Face(1.02f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.06f, 0.60f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(-0.02f, 0.39f, 0.75f), amplitude: 0.24f, ease: EaseKind.Hop),
                        Sfx(1.70f, "ratchet"),
                        Move(1.70f, 0.80f, StepKind.Fly, "SteadyCar",
                            new Vector3(0f, 0.14f, 0.62f), ease: EaseKind.InOut),
                        Move(1.70f, 0.80f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0f, 0.14f, 0.62f), ease: EaseKind.InOut),
                        Move(1.70f, 0.80f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0.14f, 0.62f), ease: EaseKind.InOut),
                        Move(2.52f, 0.48f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(-0.02f, -0.29f, 0.13f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(2.72f, 0.72f),
                        Sfx(2.78f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildTopple(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r12_topple.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r12", "topple", "Bring down the spire.", Difficulty.Surprising,
                ReasoningKind.Momentum, "Diorama_Canyon_Spire",
                "A thin finger of rock stands in the chasm, taller than either rim, with the Peps on " +
                "opposite sides of it.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "grapple", Prop = Author.Prop("grapple"), AnchorId = "Slot_1", Label = "The grappling hook",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.46f, 0.74f, 1.36f), amplitude: 0.72f, ease: EaseKind.Hop),
                        Sfx(0.66f, "clank"),
                        Move(0.70f, 0.42f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 8f, ease: EaseKind.InOut),
                        Sfx(0.86f, "creak"),
                        Rotate(0.86f, 0.44f, "Spire", new Vector3(16f, 0f, 0f), EaseKind.In),
                        Face(0.94f, SceneRef.PepA, PepFace.Hopeful),
                        Sfx(1.30f, "rumble"),
                        Haptic(1.32f, "medium"),
                        Move(1.32f, 0.14f, StepKind.Hide, "Spire", Vector3.zero),
                        Move(1.32f, 0.14f, StepKind.Show, "FallenSpan", Vector3.zero),
                        Move(1.32f, 0.20f, StepKind.Show, "SpireDust", Vector3.zero),
                        Resize(1.34f, 0.70f, "SpireDust", 2.10f, EaseKind.Out),
                        Move(2.04f, 0.36f, StepKind.Hide, "SpireDust", Vector3.zero),
                        Move(1.72f, 0.86f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.42f, 0.24f, 1.48f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(2.66f, 0.74f),
                        Sfx(2.72f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Author.Prop("scissors"), AnchorId = "Slot_2",
                    Label = "The purple-handled scissors",
                    Quip = "Rock. Meet scissors.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.44f, 0.42f, 1.08f), amplitude: 0.50f, ease: EaseKind.Hop),
                        Sfx(0.64f, "clunk"),
                        Move(0.64f, 0.66f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 15f, ease: EaseKind.InOut),
                        Move(1.34f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.12f, -0.30f, -0.34f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Face(0.92f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_3", Label = "The caged electric fan",
                    Quip = "The mountain was fine.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Move(0.22f, 0.90f, StepKind.Shake, "Spire", Vector3.zero,
                            amplitude: 0.6f, ease: EaseKind.InOut),
                        Move(0.30f, 0.30f, StepKind.Show, "SpireDust", Vector3.zero),
                        Resize(0.32f, 0.80f, "SpireDust", 1.60f, EaseKind.Out),
                        Move(1.30f, 0.40f, StepKind.Hide, "SpireDust", Vector3.zero),
                        Face(0.90f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
