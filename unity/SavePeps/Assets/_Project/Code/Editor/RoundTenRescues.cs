using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 10 — Orbital station.** *World rule: nothing falls and nothing
    /// stops. Every push lasts forever, and you have to push off something.*
    ///
    /// The only world with no ground: three hull sections hanging in a
    /// starfield with real gaps between them, one hard white key light and
    /// almost no fill, so the shadow side is black. Its choreography is the
    /// opposite of every other round's — arcs become straight lines, easing
    /// becomes linear, and things that are launched simply keep going.
    ///
    /// Only-here rescue: **r28**, one puff of cold gas. It is the only rescue
    /// solved by adding velocity to a Pep rather than by changing the world
    /// between them, and it only works where there is nothing to slow them
    /// down.
    /// </summary>
    public static class RoundTenRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r28 = BuildPush(overwrite, log);
            var r29 = BuildAttract(overwrite, log);
            var r30 = BuildSeal(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_10.asset", overwrite, log, out var round))
            {
                round.Number = 10;
                round.Rescues = new[] { r28, r29, r30 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildPush(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r28_push.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r28", "push", "Stop the drifting.", Difficulty.Medium,
                ReasoningKind.Momentum, "Diorama_Orbit_Drift",
                "One Pep hangs turning slowly in the gap between two hull sections with a cut tether " +
                "trailing behind them and nothing within reach.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_1", Label = "The coil of rope",
                    Quip = "Nobody holds the end.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "servo"),
                        Move(0f, 0.86f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.32f, 0.42f, 1.10f), ease: EaseKind.Linear),
                        Move(0.88f, 1.20f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 240f,
                            ease: EaseKind.Linear),
                        Move(0.88f, 1.20f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.16f, 0.20f, 0.36f), ease: EaseKind.Linear),
                        Face(1.02f, SceneRef.PepA, PepFace.Worried),
                        Face(1.40f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "thruster", Prop = Author.Prop("thruster"), AnchorId = "Slot_2",
                    Label = "The gas thruster",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "servo"),
                        // Straight lines and linear easing: with nothing to
                        // slow it, a tossed object in this world travels the
                        // way it left, exactly.
                        Move(0f, 0.90f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.38f, 0.61f, 1.38f), ease: EaseKind.Linear),
                        Face(0.94f, SceneRef.PepA, PepFace.Hopeful),
                        Sfx(1.06f, "thrust"),
                        Haptic(1.08f, "medium"),
                        Move(1.10f, 1.30f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.02f, -0.50f, 0.78f), ease: EaseKind.Linear),
                        Move(1.10f, 1.30f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.02f, -0.50f, 0.78f), ease: EaseKind.Linear),
                        Move(1.14f, 0.34f, StepKind.Hide, "Adrift", Vector3.zero),
                        Move(1.20f, 0.44f, StepKind.Hide, "Tether", Vector3.zero),
                        Face(2.30f, SceneRef.PepA, PepFace.Happy),
                        Meet(2.62f, 0.74f),
                        Sfx(2.68f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_3",
                    Label = "The orange umbrella",
                    Quip = "No air to catch.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.80f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.02f, 0.53f, 1.74f), ease: EaseKind.Linear),
                        Sfx(0.84f, "pop"),
                        Move(0.84f, 0.36f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 170f),
                        Resize(0.84f, 0.36f, SceneRef.Self, 1.20f, EaseKind.Back),
                        Face(1.24f, SceneRef.PepA, PepFace.Hopeful),
                        // The joke is the absence: nothing at all happens for a
                        // full second, and then the hope drains out of the face.
                        Face(2.10f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildAttract(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r29_attract.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r29", "attract", "Pull them back.", Difficulty.Surprising,
                ReasoningKind.Magnetism, "Diorama_Orbit_Tumble",
                "One Pep is tumbling away in open space above the station, wearing a steel-backed pack, " +
                "already drifting beyond reach of the station handrail.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "magnet", Prop = Author.Prop("magnet"), AnchorId = "Slot_1",
                    Label = "The horseshoe magnet",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "servo"),
                        // Magnet flies to the crane gantry electromagnet coil
                        Move(0f, 0.86f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.36f, 0.57f, 1.69f), ease: EaseKind.Linear),
                        Sfx(0.88f, "snap_on"),
                        Haptic(0.90f, "heavy"),
                        Impact(0.90f, 0.85f),
                        Face(0.92f, SceneRef.PepA, PepFace.Hopeful),

                        // Crane gantry engages and pivots toward tumbling Pep
                        Move(0.92f, 0.38f, StepKind.Spin, "Gantry", Vector3.zero, amplitude: 18f, ease: EaseKind.Out),

                        // Intense magnetic flux field illuminates across the orbital gap
                        Move(0.95f, 0.25f, StepKind.Show, "MagneticFlux", Vector3.zero),
                        Sfx(0.96f, "zap"),

                        // Orbital debris field polarizes and deflects
                        Move(1.00f, 0.90f, StepKind.Fly, "DebrisField",
                            new Vector3(0.20f, 0.10f, -0.22f), ease: EaseKind.Out),

                        // Tumbling Pep B and pack are captured by the flux field and reeled in
                        Move(1.02f, 1.18f, StepKind.Fly, "Backpack",
                            new Vector3(-1.00f, -0.97f, -1.29f), ease: EaseKind.InOut),
                        Move(1.02f, 1.18f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(-1.00f, -0.97f, -1.29f), ease: EaseKind.InOut),
                        Face(1.20f, SceneRef.PepB, PepFace.Happy),

                        // Flux field disengages as Pep B reaches the station deck
                        Move(2.20f, 0.28f, StepKind.Hide, "MagneticFlux", Vector3.zero),

                        Meet(2.65f, 0.74f),
                        Sfx(2.70f, "reunion"),
                        Haptic(2.70f, "success"),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Author.Prop("bell"), AnchorId = "Slot_2", Label = "The brass bell",
                    Quip = "Space took the sound.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.70f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.30f, 0.30f, 1.10f), ease: EaseKind.Linear),
                        Move(0.72f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 22f, ease: EaseKind.InOut),
                        Face(0.90f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.86f, SceneRef.PepA, PepFace.Worried),
                        Sfx(1.90f, "click"),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_3",
                    Label = "The orange balloon",
                    Quip = "Vacuum. Zero balloons.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Move(0f, 0.62f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.10f, 0.42f, 1.30f), ease: EaseKind.Linear),
                        Resize(0.64f, 0.26f, SceneRef.Self, 1.70f, EaseKind.Out),
                        Sfx(0.90f, "pop"),
                        Move(0.90f, 0.18f, StepKind.Hide, SceneRef.Self, Vector3.zero),
                        Face(0.96f, SceneRef.PepA, PepFace.Panic),
                        Face(1.62f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSeal(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r30_seal.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r30", "seal", "Shut the airlock.", Difficulty.Medium,
                ReasoningKind.Airflow, "Diorama_Orbit_Airlock",
                "A catastrophic airlock breach is expelling atmosphere into the vacuum, tearing loose " +
                "station panels and preventing any crossing between the disaligned modules.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "mirror", Prop = Author.Prop("mirror"), AnchorId = "Slot_1", Label = "The hand mirror",
                    Quip = "The draught took it.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Move(0f, 0.76f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.22f, 0.21f, 1.61f), ease: EaseKind.Linear),
                        Sfx(0.78f, "hiss"),
                        Move(0.80f, 0.40f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 20f, ease: EaseKind.InOut),
                        Move(1.20f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.80f, 0.60f, -1.20f), ease: EaseKind.Linear),
                        Face(1.26f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_2", Label = "The grey stone",
                    Quip = "Straight through. Clang.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Move(0f, 0.80f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.58f, 0.21f, 1.61f), ease: EaseKind.Linear),
                        Sfx(0.84f, "clank"),
                        Move(0.84f, 0.80f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0f, 0f, 0.60f), ease: EaseKind.In),
                        Move(0.86f, 0.70f, StepKind.Shake, "BreachCollar", Vector3.zero,
                            amplitude: 7f, ease: EaseKind.InOut),
                        Face(0.98f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_3", Label = "The soft pillow",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "servo"),
                        // Phase 1: Pillow flies toward the high-velocity decompression maw
                        Move(0f, 0.78f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.18f, 0.21f, 1.95f), ease: EaseKind.Linear),

                        // Phase 2: Suction pulls pillow into breach collar and seals it tight
                        Sfx(0.80f, "poof"),
                        Resize(0.80f, 0.22f, SceneRef.Self, 0.65f, EaseKind.Out),
                        Sfx(0.82f, "thud"),
                        Sfx(0.84f, "hiss"),
                        Impact(0.84f, 1.50f),
                        Haptic(0.84f, "heavy"),
                        Move(0.82f, 0.25f, StepKind.Shake, "BreachCollar", Vector3.zero, amplitude: 14f),
                        Move(0.84f, 0.18f, StepKind.Hide, "DecompressionVortex", Vector3.zero),

                        // Phase 3: WORLD EVENT TRANSFORMATION
                        // Breached station transforms: modules dock, pressurized concourse links the modules
                        VisibilitySwap(0.96f, "BreachedStationWorld", "DockedStationWorld"),
                        Atmosphere(0.96f, 0.75f, "orbit_docked"),
                        Sfx(0.96f, "clank"),
                        Sfx(1.00f, "servo"),
                        Sfx(1.06f, "snap_on"),
                        Impact(0.96f, 1.40f),
                        Haptic(0.96f, "heavy"),
                        Move(0.96f, 0.40f, StepKind.Fly, "DockedStationWorld",
                            new Vector3(0f, 0.02f, 0f), ease: EaseKind.Out),

                        // Phase 4: Longest zero-g orbital traversal across the illuminated concourse
                        Face(1.38f, SceneRef.PepA, PepFace.Happy),
                        Face(1.38f, SceneRef.PepB, PepFace.Happy),
                        Move(1.45f, 1.10f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.42f, 0f, 0.55f), ease: EaseKind.InOut),
                        Move(1.45f, 1.10f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(-0.55f, -0.14f, -0.53f), ease: EaseKind.InOut),

                        // Phase 5: Climax Reunion inside the central concourse
                        Meet(2.65f, 0.80f),
                        Sfx(2.70f, "reunion"),
                        Haptic(2.70f, "success"),
                        Impact(2.70f, 0.60f),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
