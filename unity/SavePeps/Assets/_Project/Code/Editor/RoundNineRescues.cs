using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 9 — Deep ocean trench.** *World rule: down is slow and up is
    /// free. Nothing falls, everything drifts, and sound travels as deep resonance.*
    ///
    /// Escalation arc:
    /// - **R9.1 (Local Event):** Trapped in soft sediment, a released air bubble
    ///   column creates a localized vertical elevator lift up to the high shelf.
    /// - **R9.2 (Landscape/Ecosystem Event):** A sunken galleon wreck guarded by a
    ///   predatory deep-sea anglerfish. A drifting bioluminescent jelly lures the
    ///   creature away in a wide sweeping arc, awakening the dormant coral reef and
    ///   illuminating the interior deck passage for a dual traversal through the hull.
    /// - **R9.3 (World Event):** A roaring hydrothermal vent chimney blasts violent
    ///   updraft plumes and torrential cross-currents across a massive abyss chasm.
    ///   Plunging an iron mooring ballast directly into the vent throat caps the
    ///   hydraulic pressure, transforming the turbulent trench into a glowing,
    ///   calm laminar thermal highway with blossoming vent flora and an epic
    ///   weightless oceanic reunion crossing.
    /// </summary>
    public static class RoundNineRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r25 = BuildRise(overwrite, log);
            var r26 = BuildBeckon(overwrite, log);
            var r27 = BuildMoor(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_09.asset", overwrite, log, out var round))
            {
                round.Number = 9;
                round.Rescues = new[] { r25, r26, r27 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildRise(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r25_rise.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r25", "rise", "Rise from the floor.", Difficulty.Easy,
                ReasoningKind.Buoyancy, "Diorama_Abyss_Floor",
                "One Pep stands in the silt at the bottom of the trench; the other waits on a shelf far " +
                "up the wall.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_1", Label = "The grey stone",
                    Quip = "Down is not the way.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.86f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.64f, 0.09f, 1.48f), amplitude: 0.26f, ease: EaseKind.InOut),
                        Sfx(0.88f, "thud"),
                        Move(0.92f, 0.80f, StepKind.Fly, SceneRef.PepB, new Vector3(0f, -0.055f, 0f),
                            ease: EaseKind.InOut),
                        Resize(0.92f, 0.80f, "SiltCloud", 1.45f, EaseKind.Out),
                        Face(1.04f, SceneRef.PepB, PepFace.Worried),
                        Face(1.28f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Author.Prop("bell"), AnchorId = "Slot_2", Label = "The brass bell",
                    Quip = "Water eats the sound.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 1.05f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 13f, ease: EaseKind.InOut),
                        Sfx(0.30f, "clunk"),
                        Move(0.36f, 0.90f, StepKind.Shake, "SiltCloud", Vector3.zero,
                            amplitude: 1.2f, ease: EaseKind.InOut),
                        Face(0.52f, SceneRef.PepB, PepFace.Worried),
                        Face(1.14f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bubble_shell", Prop = Author.Prop("bubble_shell"), AnchorId = "Slot_3",
                    Label = "The air-filled shell",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "bubble"),
                        Move(0f, 0.90f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.24f, 0.09f, 1.74f), amplitude: 0.28f, ease: EaseKind.InOut),
                        Move(0.94f, 0.42f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 34f,
                            ease: EaseKind.Out),
                        Sfx(0.98f, "bubble"),
                        Move(1.04f, 0.30f, StepKind.Show, "Lift", Vector3.zero),
                        Haptic(1.06f, "light"),
                        Face(1.10f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.10f, SceneRef.PepA, PepFace.Hopeful),
                        // Slow, even, buoyant ascent with no terrestrial gravity.
                        Move(1.14f, 1.10f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, 0.70f, 0f), ease: EaseKind.InOut),
                        Move(1.14f, 1.10f, StepKind.Fly, "Lift",
                            new Vector3(0f, 0.34f, 0f), ease: EaseKind.InOut),
                        Move(2.24f, 0.42f, StepKind.Hide, "Lift", Vector3.zero),
                        Move(2.26f, 0.62f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(-0.54f, -0.03f, 0.22f), ease: EaseKind.InOut),
                        Meet(2.90f, 0.68f),
                        Sfx(2.96f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildBeckon(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r26_beckon.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r26", "beckon", "Move the angler.", Difficulty.Medium,
                ReasoningKind.Luring, "Diorama_Abyss_Wreck",
                "An anglerfish hangs in the mouth of a sunken hull, its own little lamp swinging, and " +
                "the tunnel behind it is the only way through.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "glow_jelly", Prop = Author.Prop("glow_jelly"), AnchorId = "Slot_1",
                    Label = "The glowing jelly",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "bubble"),
                        Move(0f, 0.86f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.18f, 0.54f, 0.90f), amplitude: 0.24f, ease: EaseKind.InOut),
                        Face(0.88f, SceneRef.PepA, PepFace.Hopeful),
                        Sfx(0.88f, "sonar"),
                        Move(0.88f, 0.50f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 5f, ease: EaseKind.InOut),
                        Move(0.92f, 0.42f, StepKind.Fly, "AnglerLure",
                            new Vector3(-0.16f, 0.08f, 0.16f), ease: EaseKind.InOut),
                        Move(1.05f, 1.10f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.95f, 0.25f, -0.65f), ease: EaseKind.InOut),
                        Move(1.10f, 1.10f, StepKind.Fly, "Angler",
                            new Vector3(-0.85f, 0.30f, -0.55f), ease: EaseKind.InOut),
                        Move(1.10f, 1.10f, StepKind.Fly, "AnglerLure",
                            new Vector3(-0.85f, 0.30f, -0.55f), ease: EaseKind.InOut),
                        Move(2.20f, 0.55f, StepKind.FlyOff, "Angler",
                            new Vector3(-0.70f, 0.20f, -0.40f), ease: EaseKind.In),
                        Move(2.20f, 0.55f, StepKind.FlyOff, "AnglerLure",
                            new Vector3(-0.70f, 0.20f, -0.40f), ease: EaseKind.In),
                        // Ecosystem response: as the angler leaves, the lit passage awakens.
                        Move(1.55f, 0.01f, StepKind.Show, "LitWreckPassage", Vector3.zero),
                        Sfx(1.58f, "bubble"),
                        Haptic(1.58f, "light"),
                        Face(1.62f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.62f, SceneRef.PepB, PepFace.Hopeful),
                        // Dual traversal through the illuminated galleon ribcage.
                        Move(1.72f, 0.96f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.32f, 0f, 0.60f), ease: EaseKind.InOut),
                        Move(1.72f, 0.96f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(-0.22f, 0f, -1.32f), ease: EaseKind.InOut),
                        Meet(2.72f, 0.72f),
                        Sfx(2.78f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "net", Prop = Author.Prop("net"), AnchorId = "Slot_2", Label = "The landing net",
                    Quip = "It swam right through.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.86f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.38f, 0.45f, 1.60f), amplitude: 0.26f, ease: EaseKind.InOut),
                        Sfx(0.88f, "whoosh"),
                        Move(0.90f, 0.70f, StepKind.Shake, "Angler", Vector3.zero,
                            amplitude: 10f, ease: EaseKind.InOut),
                        Move(1.62f, 0.50f, StepKind.Hide, SceneRef.Self, Vector3.zero),
                        Face(0.98f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Author.Prop("scissors"), AnchorId = "Slot_3",
                    Label = "The purple-handled scissors",
                    Quip = "The angler kept them.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.84f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.06f, 0.45f, 1.36f), amplitude: 0.24f, ease: EaseKind.InOut),
                        Sfx(0.86f, "snip"),
                        Move(0.88f, 0.60f, StepKind.Shake, "Angler", Vector3.zero,
                            amplitude: 12f, ease: EaseKind.InOut),
                        Sfx(1.44f, "clunk"),
                        Move(1.46f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.10f, 0.20f, 0.44f), ease: EaseKind.InOut),
                        Face(1.50f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildMoor(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r27_moor.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r27", "moor", "Beat the current.", Difficulty.Surprising,
                ReasoningKind.Crossing, "Diorama_Abyss_Current",
                "A race of cold water runs the length of the trench between two shelves, fast enough " +
                "that anything loose is already gone.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "leaf", Prop = Author.Prop("leaf"), AnchorId = "Slot_1", Label = "The broad green leaf",
                    Quip = "Gone. Instantly.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.34f, 0.20f, 1.20f), amplitude: 0.22f, ease: EaseKind.InOut),
                        Face(0.60f, SceneRef.PepA, PepFace.Hopeful),
                        Sfx(0.68f, "whoosh"),
                        Move(0.68f, 0.72f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(1.60f, 0.35f, 0.18f), ease: EaseKind.In),
                        Face(1.44f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "weight", Prop = Author.Prop("weight"), AnchorId = "Slot_2", Label = "The iron weight",
                    Duration = 3.55f,
                    Steps = new[]
                    {
                        // 1. Plunge: The heavy iron ballast arcs high across the trench toward the roaring vent spire.
                        Move(0f, 0.70f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.40f, 0.32f, 1.42f), amplitude: 0.38f, ease: EaseKind.Hop),
                        // 2. Hydraulic Impact: Capping the vent throat under massive back-pressure.
                        Sfx(0.72f, "thud"),
                        Sfx(0.73f, "clank"),
                        Impact(0.72f, 1.50f),
                        Haptic(0.72f, "heavy"),
                        Move(0.74f, 0.22f, StepKind.Shake, "VentChimney", Vector3.zero,
                            amplitude: 6.0f, ease: EaseKind.InOut),
                        Sfx(0.76f, "hiss"),
                        Resize(0.76f, 0.20f, "TurbulentPlume", 0.05f, EaseKind.Out),
                        Move(0.76f, 0.20f, StepKind.Shake, "CrossCurrentRace", Vector3.zero,
                            amplitude: 4.0f, ease: EaseKind.InOut),
                        // 3. WORLD EVENT TRANSFORMATION: Seismic pressure wave forces submerged basalt monoliths
                        // to heave upward out of the abyss pit, locking together into a monumental stepped causeway!
                        VisibilitySwap(0.98f, "LockedChasmWorld", "TransformedChasmWorld"),
                        Move(0.98f, 0.01f, StepKind.Hide, "TurbulentPlume", Vector3.zero),
                        Move(0.98f, 0.01f, StepKind.Hide, "CrossCurrentRace", Vector3.zero),
                        Move(0.98f, 0.01f, StepKind.Hide, "VentChimney", Vector3.zero),
                        Move(0.98f, 0.01f, StepKind.Hide, SceneRef.Self, Vector3.zero),
                        Atmosphere(0.98f, 0.70f, "abyss_bloom"),
                        Sfx(0.98f, "quake"),
                        Sfx(1.02f, "sonar"),
                        Impact(0.98f, 1.00f),
                        Haptic(0.98f, "heavy"),
                        Face(1.30f, SceneRef.PepA, PepFace.Happy),
                        Face(1.30f, SceneRef.PepB, PepFace.Happy),
                        Sfx(1.38f, "water_rise"),
                        // 4. Stepped traversal across the newly elevated basalt monolith causeway.
                        // Pep A hops across left monoliths toward the central keystone:
                        Move(1.45f, 0.32f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.17f, 0.00f, 0.04f), ease: EaseKind.Hop),
                        Move(1.80f, 0.34f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.36f, 0.04f, 0.08f), ease: EaseKind.Hop),
                        Move(2.16f, 0.36f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.56f, 0.06f, 0.12f), ease: EaseKind.Hop),
                        // Pep B hops across right monoliths toward the central keystone:
                        Move(1.45f, 0.32f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.11f, 0.00f, -0.04f), ease: EaseKind.Hop),
                        Move(1.80f, 0.34f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.30f, 0.04f, -0.08f), ease: EaseKind.Hop),
                        Move(2.16f, 0.36f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.50f, 0.06f, -0.12f), ease: EaseKind.Hop),
                        // 5. Climax reunion at the central volcanic keystone.
                        Meet(2.60f, 0.85f),
                        Sfx(2.65f, "reunion"),
                        Impact(2.62f, 0.80f),
                        Haptic(2.65f, "success"),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_3",
                    Label = "The orange balloon",
                    Quip = "Up, then away.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.10f, 0.42f, 1.24f), amplitude: 0.24f, ease: EaseKind.InOut),
                        Sfx(0.62f, "bubble"),
                        Face(0.68f, SceneRef.PepB, PepFace.Hopeful),
                        Move(0.74f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.15f, 1.40f, 0f), ease: EaseKind.In),
                        Face(1.68f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
