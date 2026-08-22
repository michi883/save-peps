using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 5 — Tidewater docks.** *World rule: everything floats or sinks,
    /// and the water is going somewhere whether you like it or not.*
    ///
    /// The first world where the ground is not solid. Every stage stands in
    /// open water that runs off the edge of the picture, everything with a
    /// hull bobs at rest, and both of the round's wrong answers per rescue are
    /// heavy or loose — the two ways the sea takes something away from you.
    ///
    /// Only-here rescue: **r15**, tying the buoy upstream and letting the
    /// current swing it across. It is the only rescue solved by a force that
    /// was already moving before the player arrived.
    /// </summary>
    public static class RoundFiveRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r13 = BuildBail(overwrite, log);
            var r14 = BuildPaddle(overwrite, log);
            var r15 = BuildDrift(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_05.asset", overwrite, log, out var round))
            {
                round.Number = 5;
                round.Rescues = new[] { r13, r14, r15 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildBail(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r13_bail.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r13", "bail", "Lift the punt.", Difficulty.Easy,
                ReasoningKind.Buoyancy, "Diorama_Tide_Punt",
                "A punt lies swamped between two jetties, its gunwale sitting below the decking, full " +
                "of green water.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "bucket", Prop = Author.Prop("bucket"), AnchorId = "Slot_1", Label = "The wooden bucket",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.62f, 0.125f, 1.72f), amplitude: 0.46f, ease: EaseKind.Hop),
                        Sfx(0.66f, "splash"),
                        Rotate(0.66f, 0.30f, SceneRef.Self, new Vector3(0f, 0f, -52f)),
                        Rotate(0.96f, 0.30f, SceneRef.Self, new Vector3(0f, 0f, 52f)),
                        Sfx(1.02f, "splash"),
                        Resize(0.70f, 0.70f, "Bilge", 0.15f, EaseKind.In),
                        Move(0.70f, 0.70f, StepKind.Fly, "Bilge", new Vector3(0f, -0.06f, 0f)),
                        Move(1.40f, 0.20f, StepKind.Hide, "Bilge", Vector3.zero),
                        Move(1.28f, 0.62f, StepKind.Fly, "Punt", new Vector3(0f, 0.22f, 0f),
                            ease: EaseKind.Back),
                        Haptic(1.32f, "light"),
                        Face(1.38f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.38f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.62f, 0.52f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.52f, -0.03f, 0.82f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Move(2.14f, 0.48f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.18f, 0.03f, 0.65f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.68f, 0.72f),
                        Sfx(2.74f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_2", Label = "The grey stone",
                    Quip = "Now it sits lower.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.44f, 0.125f, 1.82f), amplitude: 0.48f, ease: EaseKind.Hop),
                        Sfx(0.64f, "splash"),
                        Move(0.66f, 0.62f, StepKind.Fly, "Punt", new Vector3(0f, -0.06f, 0f)),
                        Resize(0.66f, 0.62f, "Bilge", 1.20f, EaseKind.Out),
                        Face(0.84f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_3",
                    Label = "The orange balloon",
                    Quip = "One end up. One down.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.28f, 0.145f, 1.28f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Sfx(0.62f, "boing"),
                        Rotate(0.66f, 0.80f, "Punt", new Vector3(15f, 0f, 0f)),
                        Move(0.66f, 0.80f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.16f, 0f)),
                        Sfx(1.20f, "splash"),
                        Face(1.24f, SceneRef.PepA, PepFace.Panic),
                        Face(1.36f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildPaddle(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r14_paddle.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r14", "paddle", "Row across the bay.", Difficulty.Medium,
                ReasoningKind.Momentum, "Diorama_Tide_Channel",
                "A log raft is moored at the near dock with nothing aboard to move it, and the far " +
                "dock is out of jumping range.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "weight", Prop = Author.Prop("weight"), AnchorId = "Slot_1", Label = "The iron weight",
                    Quip = "Down. Not across.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.50f, 0.09f, 1.73f), amplitude: 0.44f, ease: EaseKind.Hop),
                        Sfx(0.60f, "splash"),
                        Move(0.60f, 0.70f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0f, -0.24f, 0f), ease: EaseKind.In),
                        Move(0.62f, 0.62f, StepKind.Shake, "Raft", Vector3.zero,
                            amplitude: 3f, ease: EaseKind.InOut),
                        Face(0.86f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "oar", Prop = Author.Prop("oar"), AnchorId = "Slot_2", Label = "The wooden oar",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.56f, 0.065f, 1.63f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Sfx(0.60f, "thud"),
                        Face(0.66f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.70f, 0.56f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.44f, -0.16f, 0.70f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Move(1.20f, 0.18f, StepKind.Hide, "Mooring", Vector3.zero),
                        Sfx(1.24f, "creak"),
                        Move(1.28f, 1.30f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 22f, ease: EaseKind.InOut),
                        Move(1.28f, 1.30f, StepKind.Fly, "Raft",
                            new Vector3(0.30f, 0f, 1.00f), ease: EaseKind.InOut),
                        Move(1.28f, 1.30f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.30f, 0f, 1.00f), ease: EaseKind.InOut),
                        Move(1.28f, 1.30f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.30f, 0f, 1.00f), ease: EaseKind.InOut),
                        Move(2.58f, 0.42f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.06f, 0.16f, 0.07f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(2.74f, 0.72f),
                        Sfx(2.80f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_3",
                    Label = "The orange balloon",
                    Quip = "No wind. No progress.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.16f, 0.115f, 1.21f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Sfx(0.62f, "boing"),
                        Move(0.66f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 8f, ease: EaseKind.InOut),
                        Move(0.70f, 1.00f, StepKind.Shake, "Raft", Vector3.zero,
                            amplitude: 1.5f, ease: EaseKind.InOut),
                        Face(0.90f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.80f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildDrift(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r15_drift.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r15", "drift", "Ride the current.", Difficulty.Surprising,
                ReasoningKind.Crossing, "Diorama_Tide_Current",
                "A fast channel runs left to right between two docks, and there is a mooring post on " +
                "the upstream side of it.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "plank", Prop = Author.Prop("plank"), AnchorId = "Slot_1", Label = "The wooden plank",
                    Quip = "The sea took it.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.52f, 0.045f, 1.76f), amplitude: 0.44f, ease: EaseKind.Hop),
                        Sfx(0.62f, "splash"),
                        Face(0.68f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.72f, 0.95f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(1.40f, 0f, 0.24f), ease: EaseKind.In),
                        Face(1.74f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "net", Prop = Author.Prop("net"), AnchorId = "Slot_2", Label = "The landing net",
                    Quip = "It caught only water.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.02f, 0.105f, 1.98f), amplitude: 0.46f, ease: EaseKind.Hop),
                        Sfx(0.66f, "splash"),
                        Move(0.68f, 0.90f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 14f, ease: EaseKind.InOut),
                        Move(1.60f, 0.70f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.86f, 0f, 0.14f), ease: EaseKind.In),
                        Face(0.94f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "buoy", Prop = Author.Prop("buoy"), AnchorId = "Slot_3", Label = "The mooring buoy",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.84f, 0.045f, 1.16f), amplitude: 0.48f, ease: EaseKind.Hop),
                        Sfx(0.64f, "splash"),
                        Move(0.78f, 0.22f, StepKind.Show, "Swing", Vector3.zero),
                        Sfx(0.80f, "creak"),
                        Face(0.86f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.92f, 0.52f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.28f, -0.09f, -0.30f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Move(1.48f, 1.00f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0.80f, 0f, 0.50f), amplitude: 0.26f, ease: EaseKind.InOut),
                        Move(1.48f, 1.00f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.80f, 0.06f, 0.50f), amplitude: 0.26f, ease: EaseKind.InOut),
                        Move(1.50f, 0.98f, StepKind.Shake, "Current", Vector3.zero,
                            amplitude: 2f, ease: EaseKind.InOut),
                        Move(2.50f, 0.44f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.10f, 0.09f, 0.10f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Haptic(2.52f, "light"),
                        Meet(2.72f, 0.72f),
                        Sfx(2.78f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
