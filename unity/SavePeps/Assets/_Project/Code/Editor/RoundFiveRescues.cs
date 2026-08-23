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
    /// Only-here rescue: **r15**, floating the buoy into a latch that releases
    /// the harbor barrage. The resulting tide changes
    /// the state of the whole world before it carries anyone anywhere.
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
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.48f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.62f, 0.125f, 1.72f), amplitude: 0.46f, ease: EaseKind.Hop),
                        Sfx(0.50f, "splash"),
                        Rotate(0.50f, 0.22f, SceneRef.Self, new Vector3(0f, 0f, -52f)),
                        Rotate(0.72f, 0.22f, SceneRef.Self, new Vector3(0f, 0f, 52f)),
                        Sfx(0.88f, "splash"),
                        Resize(0.54f, 0.52f, "Bilge", 0.12f, EaseKind.In),
                        Move(0.54f, 0.52f, StepKind.Fly, "Bilge", new Vector3(0f, -0.06f, 0f)),
                        Move(1.08f, 0.12f, StepKind.Hide, "Bilge", Vector3.zero),
                        Move(0.92f, 0.44f, StepKind.Fly, "Punt", new Vector3(0f, 0.16f, 0f),
                            ease: EaseKind.Back),
                        Haptic(0.96f, "light"),
                        Face(1.02f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.02f, SceneRef.PepB, PepFace.Hopeful),
                        // One hull rises in one pocket of water. The sea,
                        // docks and wider silhouette do not react.
                        Move(1.24f, 0.42f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.52f, -0.01f, 0.62f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(1.86f, 0.54f),
                        Sfx(1.92f, "reunion"),
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

            Author.Stage(rescue, "r14", "paddle", "Navigate the lock.", Difficulty.Medium,
                ReasoningKind.Momentum, "Diorama_Tide_Channel",
                "A log raft waits below a two-gate lock. The lower and upper gates are shut, the lock " +
                "water sits low, and the far landing lies beyond the raised chamber.");

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
                    Duration = 3.45f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.50f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-1.26f, 0.50f, 1.43f), amplitude: 0.52f, ease: EaseKind.Hop),
                        Sfx(0.50f, "clank"),
                        Move(0.50f, 0.34f, StepKind.Spin, SceneRef.Self, Vector3.zero,
                            amplitude: 210f, ease: EaseKind.InOut),
                        Move(0.50f, 0.38f, StepKind.Spin, "Capstan", Vector3.zero,
                            amplitude: 210f, ease: EaseKind.InOut),
                        Sfx(0.64f, "ratchet"),
                        Rotate(0.66f, 0.34f, "LowerGateLeft", new Vector3(0f, -76f, 0f), EaseKind.InOut),
                        Rotate(0.68f, 0.34f, "LowerGateRight", new Vector3(0f, 76f, 0f), EaseKind.InOut),
                        Move(0.72f, 0.12f, StepKind.Hide, "Mooring", Vector3.zero),
                        Face(0.74f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.78f, 0.40f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.61f, -0.13f, 0.10f), amplitude: 0.16f, ease: EaseKind.Hop),
                        // The chamber itself changes level before navigation
                        // continues through the second mechanical gate.
                        Sfx(0.98f, "splash"),
                        VisibilitySwap(0.98f, "LockWaterLow", "LockWaterHigh"),
                        Move(0.98f, 0.38f, StepKind.Fly, "Raft", new Vector3(0f, 0.14f, 0f),
                            ease: EaseKind.Back),
                        Move(0.98f, 0.38f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0.14f, 0f),
                            ease: EaseKind.Back),
                        Move(0.98f, 0.38f, StepKind.Fly, "LevelMarker", new Vector3(0f, 0.32f, 0f),
                            ease: EaseKind.Back),
                        Rotate(1.22f, 0.36f, "UpperGateLeft", new Vector3(0f, -76f, 0f), EaseKind.InOut),
                        Rotate(1.24f, 0.36f, "UpperGateRight", new Vector3(0f, 76f, 0f), EaseKind.InOut),
                        Move(1.30f, 0.16f, StepKind.Show, "Wake", Vector3.zero),
                        Sfx(1.36f, "splash"),
                        Move(1.36f, 0.58f, StepKind.Fly, "Raft",
                            new Vector3(0.16f, 0f, 0.75f), ease: EaseKind.InOut),
                        Move(1.36f, 0.58f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.16f, 0f, 0.75f), ease: EaseKind.InOut),
                        Sfx(1.94f, "splash"),
                        Move(1.94f, 0.62f, StepKind.Fly, "Raft",
                            new Vector3(0.45f, 0f, 0.93f), ease: EaseKind.InOut),
                        Move(1.94f, 0.62f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.45f, 0f, 0.93f), ease: EaseKind.InOut),
                        Move(2.60f, 0.28f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.08f, -0.01f, 0.18f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(2.92f, 0.48f),
                        Sfx(2.98f, "reunion"),
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

            Author.Stage(rescue, "r15", "drift", "Release the tide.", Difficulty.Surprising,
                ReasoningKind.Crossing, "Diorama_Tide_Current",
                "At extreme low tide, broad mudflats strand two boats and a broken dock across the bay. " +
                "A full-width barrage seals the horizon above a buoyancy release wheel.");

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
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.52f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.22f, 0.28f, 2.56f), amplitude: 0.56f, ease: EaseKind.Hop),
                        Sfx(0.52f, "splash"),
                        // The buoy rises under the latch. It does not carry a
                        // Pep; it releases the force that transforms the bay.
                        Move(0.52f, 0.30f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0.24f, 0f), ease: EaseKind.Back),
                        Move(0.56f, 0.38f, StepKind.Spin, "TideWheel", Vector3.zero,
                            amplitude: -240f, ease: EaseKind.InOut),
                        Move(0.60f, 0.34f, StepKind.Fly, "GateChains",
                            new Vector3(0f, 0.34f, 0f), ease: EaseKind.In),
                        Sfx(0.66f, "ratchet"),
                        Rotate(0.68f, 0.52f, "TideGate", new Vector3(-82f, 0f, 0f), EaseKind.InOut),
                        Move(0.76f, 0.12f, StepKind.Show, "SurgeFront", Vector3.zero),
                        Move(0.76f, 0.76f, StepKind.Fly, "SurgeFront",
                            new Vector3(0f, 0f, -2.52f), ease: EaseKind.In),
                        Sfx(0.88f, "rumble"),
                        Haptic(0.94f, "heavy"),
                        Impact(0.94f, 1.15f),
                        Atmosphere(0.94f, 0.70f, "high_tide"),

                        // WORLD STATE CHANGE: mud disappears under a rising
                        // sea; both stranded boats refloat in different
                        // directions; the collapsed harbor becomes a moving
                        // pontoon route; a full-screen current starts.
                        VisibilitySwap(0.96f, "LowTideWorld", "HighTideWorld"),
                        VisibilitySwap(0.98f, "StrandedBoatLeft", "FloatingBoatLeft"),
                        VisibilitySwap(1.00f, "StrandedBoatRight", "FloatingBoatRight"),
                        VisibilitySwap(1.02f, "CollapsedDock", "TideRaft"),
                        Move(0.98f, 0.46f, StepKind.Fly, "HighTideWorld",
                            new Vector3(0f, 0.13f, 0f), ease: EaseKind.Back),
                        Move(1.00f, 0.54f, StepKind.Fly, "FloatingBoatLeft",
                            new Vector3(-0.12f, 0.20f, 0.22f), ease: EaseKind.Back),
                        Move(1.02f, 0.56f, StepKind.Fly, "FloatingBoatRight",
                            new Vector3(0.16f, 0.21f, -0.28f), ease: EaseKind.Back),
                        Move(1.02f, 0.48f, StepKind.Fly, "TideRaft",
                            new Vector3(0f, 0.18f, 0f), ease: EaseKind.Back),
                        Move(1.02f, 0.48f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0f, 0.07f, 0f), ease: EaseKind.Back),
                        Move(1.02f, 0.16f, StepKind.Show, "CurrentField", Vector3.zero),
                        Face(1.12f, SceneRef.PepA, PepFace.Happy),
                        Face(1.12f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.54f, 0.12f, StepKind.Hide, "SurgeFront", Vector3.zero),

                        // The new tide carries a substantial piece of the
                        // harbor through three directional legs, with Pep A
                        // riding the environment rather than the answer prop.
                        Sfx(1.54f, "splash"),
                        Move(1.54f, 0.42f, StepKind.Fly, "TideRaft",
                            new Vector3(0.48f, 0.02f, 0.62f), ease: EaseKind.InOut),
                        Move(1.54f, 0.42f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.48f, 0.02f, 0.62f), ease: EaseKind.InOut),
                        Sfx(1.96f, "splash"),
                        Move(1.96f, 0.40f, StepKind.Fly, "TideRaft",
                            new Vector3(0.48f, -0.01f, 0.62f), ease: EaseKind.InOut),
                        Move(1.96f, 0.40f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.48f, -0.01f, 0.62f), ease: EaseKind.InOut),
                        Sfx(2.36f, "splash"),
                        Move(2.36f, 0.34f, StepKind.Fly, "TideRaft",
                            new Vector3(0.22f, 0f, 0.48f), ease: EaseKind.InOut),
                        Move(2.36f, 0.34f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.22f, 0f, 0.48f), ease: EaseKind.InOut),
                        Move(2.70f, 0.28f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, -0.02f, 0.12f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Meet(2.98f, 0.56f),
                        Sfx(3.04f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
