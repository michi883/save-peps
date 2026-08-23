using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 8 — Snowpeak slopes.** *World rule: everything rolls downhill.
    /// Your job is to aim it, ride it, or hold on.*
    ///
    /// One steep wedge running corner to corner, the only diagonal ground in
    /// the game, with spindrift blowing across it. Its escalation grows from
    /// one packed drift, through a slope-wide banked course, to an avalanche
    /// that replaces the broken peak with a new route to the runout.
    ///
    /// Only-here rescue: **r22**, packing loose powder into a walkable wind
    /// slab. Heat makes it worse and water makes it worse; only moving air
    /// helps, which inverts everything round three taught.
    /// </summary>
    public static class RoundEightRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r22 = BuildCrust(overwrite, log);
            var r23 = BuildSled(overwrite, log);
            var r24 = BuildTraverse(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_08.asset", overwrite, log, out var round))
            {
                round.Number = 8;
                round.Rescues = new[] { r22, r23, r24 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildCrust(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r22_crust.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r22", "crust", "Firm up the snow.", Difficulty.Surprising,
                ReasoningKind.Airflow, "Diorama_Peak_Powder",
                "One compact pocket of loose powder lies between two nearby Peps, with a small " +
                "sinkhole where someone has already tried to cross it.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Author.Prop("hair_dryer"), AnchorId = "Slot_1",
                    Label = "The warm hair dryer",
                    Quip = "Now it is soup.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.46f, 0.32f, 0.96f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Move(0.62f, 0.90f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Resize(0.70f, 0.80f, "Powder", 0.80f, EaseKind.Out),
                        Move(0.70f, 0.80f, StepKind.Fly, "Powder", new Vector3(0f, -0.05f, 0f)),
                        Resize(0.74f, 0.76f, "Sinkhole", 1.45f, EaseKind.Out),
                        Move(0.90f, 0.70f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, -0.09f, 0f)),
                        Face(0.96f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Author.Prop("watering_can"), AnchorId = "Slot_2",
                    Label = "The blue watering can",
                    Quip = "Ice. Very fast ice.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.46f, 0.32f, 0.96f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Rotate(0.62f, 0.34f, SceneRef.Self, new Vector3(0f, 0f, -44f)),
                        Sfx(0.66f, "splash"),
                        Face(0.88f, SceneRef.PepA, PepFace.Hopeful),
                        Sfx(1.30f, "glide_hiss"),
                        Move(1.28f, 0.80f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.08f, -0.08f, -0.34f), ease: EaseKind.Out),
                        Face(1.36f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_3", Label = "The caged electric fan",
                    Duration = 2.8f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.48f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.02f, 0.42f, 1.08f), amplitude: 0.28f, ease: EaseKind.Hop),
                        Sfx(0.50f, "wind"),
                        Move(0.50f, 0.72f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),

                        // LOCAL EVENT: one drift pocket is blown aside and
                        // atomically becomes a walkable wind slab.
                        Move(0.58f, 0.52f, StepKind.Fly, "Powder", new Vector3(-0.28f, 0.02f, 0f),
                            ease: EaseKind.Out),
                        VisibilitySwap(1.08f, "Powder", "Crust"),
                        Move(1.08f, 0.01f, StepKind.Hide, "Sinkhole", Vector3.zero),
                        Sfx(1.10f, "crunch"),
                        Haptic(1.12f, "light"),
                        Face(1.18f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.18f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.26f, 0.70f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.36f, 0.25f, 0.72f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(2.04f, 0.56f),
                        Sfx(2.10f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSled(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r23_sled.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r23", "sled", "Get down the slope.", Difficulty.Medium,
                ReasoningKind.Momentum, "Diorama_Peak_Chute",
                "Seven blocked sections and three course gates interrupt the full mountain run. " +
                "The sled can configure them into one continuous banked route.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "sled", Prop = Author.Prop("sled"), AnchorId = "Slot_1", Label = "The little sled",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.52f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.38f, 1.03f, 2.86f), amplitude: 0.84f, ease: EaseKind.Hop),
                        Sfx(0.54f, "crunch"),
                        Move(0.54f, 0.22f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0.02f, 0f)),
                        Face(0.60f, SceneRef.PepA, PepFace.Hopeful),

                        // SYSTEM EVENT: the start mechanism releases three
                        // gates in sequence before the blocked landscape can
                        // become a continuous banked course.
                        Move(0.62f, 0.24f, StepKind.FlyOff, "StartGate",
                            new Vector3(0f, 0.20f, 0f), ease: EaseKind.In),
                        Sfx(0.64f, "click"),
                        Move(0.76f, 0.24f, StepKind.FlyOff, "CourseGateHigh",
                            new Vector3(0.28f, 0.04f, 0f), ease: EaseKind.In),
                        Move(0.88f, 0.24f, StepKind.FlyOff, "CourseGateMid",
                            new Vector3(-0.28f, 0.04f, 0f), ease: EaseKind.In),
                        Move(1.00f, 0.24f, StepKind.FlyOff, "CourseGateLow",
                            new Vector3(0.28f, 0.04f, 0f), ease: EaseKind.In),
                        Move(0.78f, 0.42f, StepKind.Shake, "ClosedRun", Vector3.zero,
                            amplitude: 3.2f, ease: EaseKind.InOut),
                        Sfx(1.04f, "rumble"),
                        VisibilitySwap(1.16f, "ClosedRun", "BankedRun"),
                        Move(1.16f, 0.01f, StepKind.Show, "TrailFlags", Vector3.zero),
                        Move(1.16f, 0.50f, StepKind.FlyOff, "Drift",
                            new Vector3(0.42f, 0.16f, -0.20f), ease: EaseKind.In),
                        Atmosphere(1.16f, 0.60f, "banked"),
                        Ambient(1.16f, 0.48f, "PeakSpindrift", 0.54f),
                        Impact(1.16f, 0.62f),
                        Haptic(1.18f, "medium"),

                        // A four-leg S-turn proves that the landscape, not
                        // just the prop, has changed how the Pep travels.
                        Sfx(1.30f, "glide_hiss"),
                        Move(1.30f, 0.38f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.38f, -0.24f, -0.62f), ease: EaseKind.InOut),
                        Move(1.30f, 0.38f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.38f, -0.24f, -0.62f), ease: EaseKind.InOut),
                        Move(1.62f, 0.38f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.62f, -0.22f, -0.64f), ease: EaseKind.InOut),
                        Move(1.62f, 0.38f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.62f, -0.22f, -0.64f), ease: EaseKind.InOut),
                        Move(1.94f, 0.38f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.58f, -0.22f, -0.62f), ease: EaseKind.InOut),
                        Move(1.94f, 0.38f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.58f, -0.22f, -0.62f), ease: EaseKind.InOut),
                        Move(2.26f, 0.38f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.24f, -0.14f, -0.50f), ease: EaseKind.InOut),
                        Move(2.26f, 0.38f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.24f, -0.14f, -0.50f), ease: EaseKind.InOut),
                        Face(1.38f, SceneRef.PepA, PepFace.Happy),
                        Move(2.50f, 0.10f, StepKind.Show, "RunSpray", Vector3.zero),
                        Sfx(2.52f, "poof"),
                        Resize(2.52f, 0.34f, "RunSpray", 1.34f, EaseKind.Out),
                        Move(2.84f, 0.16f, StepKind.Hide, "RunSpray", Vector3.zero),
                        Meet(2.94f, 0.58f),
                        Sfx(3.00f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_2", Label = "The grey stone",
                    Quip = "It went. Alone.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.70f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.30f, 0.96f, 2.76f), amplitude: 1.00f, ease: EaseKind.Hop),
                        Sfx(0.72f, "crunch"),
                        Move(0.76f, 0.84f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.16f, -0.86f, -2.30f), ease: EaseKind.In),
                        Sfx(1.58f, "thud"),
                        Face(1.62f, SceneRef.PepB, PepFace.Panic),
                        Face(1.72f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_3",
                    Label = "The orange umbrella",
                    Quip = "Wrong way. Very wrong.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "pop"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.10f, 0.81f, 2.38f), amplitude: 0.90f, ease: EaseKind.Hop),
                        Move(0.66f, 0.34f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 160f),
                        Sfx(0.98f, "wind"),
                        Move(1.00f, 0.86f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0f, 0.08f, 0.28f), ease: EaseKind.Out),
                        Move(1.00f, 0.86f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0.08f, 0.28f), ease: EaseKind.Out),
                        Face(1.06f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildTraverse(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r24_traverse.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r24", "traverse", "Follow the rope.", Difficulty.Medium,
                ReasoningKind.Crossing, "Diorama_Peak_Traverse",
                "A slack safety line spans two isolated summit shelves above a fractured cornice. " +
                "Tensioning it can release the whole loaded mountainside.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pickaxe", Prop = Author.Prop("pickaxe"), AnchorId = "Slot_1", Label = "The miner's pick",
                    Quip = "Ice wins. Ice always wins.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.30f, 0.50f, 1.42f), amplitude: 0.52f, ease: EaseKind.Hop),
                        Move(0.64f, 0.38f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 24f, ease: EaseKind.InOut),
                        Sfx(0.86f, "chip"),
                        Move(0.88f, 0.60f, StepKind.Shake, "IceBand", Vector3.zero,
                            amplitude: 1.4f, ease: EaseKind.InOut),
                        Move(1.10f, 0.38f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 24f, ease: EaseKind.InOut),
                        Sfx(1.34f, "chip"),
                        Face(1.38f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_2", Label = "The coil of rope",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.48f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-1.08f, 0.94f, 2.08f), amplitude: 0.72f, ease: EaseKind.Hop),
                        Sfx(0.50f, "creak"),
                        VisibilitySwap(0.60f, "RopeLine", "TautLine"),
                        Move(0.60f, 0.34f, StepKind.Shake, "Bollard", Vector3.zero,
                            amplitude: 3.8f, ease: EaseKind.InOut),
                        Haptic(0.62f, "medium"),
                        Face(0.68f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.68f, SceneRef.PepB, PepFace.Hopeful),

                        // The first Pep uses the taut line to reach the far
                        // anchor. Its arrival loads the release arm.
                        Sfx(0.72f, "glide_hiss"),
                        Move(0.70f, 0.46f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(1.06f, 0.14f, 0.40f), ease: EaseKind.InOut),
                        Move(0.98f, 0.30f, StepKind.Fly, "TensionArm",
                            new Vector3(-0.18f, -0.20f, -0.04f), ease: EaseKind.Back),
                        Move(1.02f, 0.01f, StepKind.Show, "FaultCracks", Vector3.zero),
                        Sfx(1.04f, "crack"),
                        Move(1.04f, 0.36f, StepKind.Shake, "CorniceSlab", Vector3.zero,
                            amplitude: 5.5f, ease: EaseKind.InOut),

                        // WORLD EVENT: the cornice shears, the avalanche runs
                        // across the frame, and the broken summit is replaced
                        // by a broad traversable fan from top to bottom.
                        Move(1.22f, 0.58f, StepKind.Fly, "CorniceSlab",
                            new Vector3(-0.12f, -0.68f, -1.20f), ease: EaseKind.In),
                        Rotate(1.22f, 0.58f, "CorniceSlab", new Vector3(0f, 0f, 18f), EaseKind.In),
                        Move(1.24f, 0.01f, StepKind.Show, "AvalancheFront", Vector3.zero),
                        Move(1.24f, 0.58f, StepKind.Fly, "AvalancheFront",
                            new Vector3(0f, -0.72f, -1.78f), ease: EaseKind.In),
                        Move(1.28f, 0.54f, StepKind.Fly, "MountainDebris",
                            new Vector3(0.08f, -0.42f, -1.14f), ease: EaseKind.In),
                        Move(1.30f, 0.42f, StepKind.Spin, "MountainDebris", Vector3.zero,
                            amplitude: 280f, ease: EaseKind.In),
                        Sfx(1.26f, "rumble"),
                        Ambient(1.28f, 0.50f, "PeakSpindrift", 0.18f),
                        Ambient(1.28f, 0.50f, "PeakFlag", 1f),
                        VisibilitySwap(1.80f, "LockedPeakWorld", "AvalancheWorld"),
                        Move(1.80f, 0.01f, StepKind.Hide, "CorniceSlab", Vector3.zero),
                        Move(1.80f, 0.01f, StepKind.Hide, "AvalancheFront", Vector3.zero),
                        Move(1.80f, 0.01f, StepKind.Hide, "IceBand", Vector3.zero),
                        Move(1.80f, 0.01f, StepKind.Hide, "TensionArm", Vector3.zero),
                        Move(1.80f, 0.01f, StepKind.Hide, "FaultCracks", Vector3.zero),
                        Move(1.80f, 0.01f, StepKind.Hide, "MountainDebris", Vector3.zero),
                        Move(1.80f, 0.01f, StepKind.Show, "SummitBeacons", Vector3.zero),
                        Atmosphere(1.80f, 0.72f, "avalanche"),
                        Impact(1.80f, 1.48f),
                        Haptic(1.82f, "heavy"),
                        Sfx(1.84f, "crunch"),
                        Face(1.88f, SceneRef.PepA, PepFace.Happy),
                        Face(1.88f, SceneRef.PepB, PepFace.Happy),

                        // Both Peps ride the route the mountain just made.
                        Move(1.96f, 0.42f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.42f, -0.35f, -0.82f), ease: EaseKind.InOut),
                        Move(1.96f, 0.42f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(-0.42f, -0.35f, -0.82f), ease: EaseKind.InOut),
                        Move(2.34f, 0.42f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.10f, -0.33f, -0.68f), ease: EaseKind.In),
                        Move(2.34f, 0.42f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0.10f, -0.33f, -0.68f), ease: EaseKind.In),
                        Move(2.54f, 0.10f, StepKind.Show, "RunoutSpray", Vector3.zero),
                        Resize(2.56f, 0.34f, "RunoutSpray", 1.42f, EaseKind.Out),
                        Sfx(2.58f, "poof"),
                        Move(2.92f, 0.14f, StepKind.Hide, "RunoutSpray", Vector3.zero),
                        Meet(2.90f, 0.58f),
                        Sfx(2.96f, "reunion"),
                        Impact(2.94f, 0.56f),
                        Haptic(2.96f, "success"),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_3", Label = "The soft pillow",
                    Quip = "It slid off without me.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.33f, 0.94f), amplitude: 0.44f, ease: EaseKind.Hop),
                        Sfx(0.66f, "poof"),
                        Face(0.74f, SceneRef.PepA, PepFace.Hopeful),
                        Sfx(1.10f, "glide_hiss"),
                        Move(1.10f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.92f, -0.18f, -0.34f), ease: EaseKind.In),
                        Face(1.60f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
