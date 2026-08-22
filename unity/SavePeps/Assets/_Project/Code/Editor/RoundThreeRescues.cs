using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 3 — Weather terrace.** *World rule: you never touch a Pep or an
    /// obstacle. You change the state of the air over them, and the world
    /// changes back.*
    ///
    /// One hillside, three terraces, three skies. This is the only round whose
    /// three stages deliberately carry different atmospheres — frost pale and
    /// blue on the top shelf, full sun and gold on the middle, a grey
    /// downpour on the terrace below — because "the weather changed" is
    /// exactly what the round is teaching. Every answer is a *field* applied
    /// to a place: heat, water, cover.
    ///
    /// Only-here rescue: **r09**, the rain. The cloud is a character with a
    /// position, and both wrong answers move the weather rather than the Peps.
    /// </summary>
    public static class RoundThreeRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r07 = BuildThaw(overwrite, log);
            var r08 = BuildSprout(overwrite, log);
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

            // R3.1 INTRODUCE: Melt Ice — simple, restrained local temperature change
            Author.Stage(rescue, "r07", "thaw", "Melt the ice.", Difficulty.Easy,
                ReasoningKind.Temperature, "Diorama_Weather_Frost",
                "Snow is falling on the top terrace and one Pep peers out of a shell of faceted ice.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "Ice stays frozen.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.74f, 0.77f, 2.66f), amplitude: 0.75f, ease: EaseKind.Hop),
                        Sfx(0.68f, "poof"),
                        Move(0.70f, 0.55f, StepKind.Shake, "IceShell", Vector3.zero,
                            amplitude: 2f, ease: EaseKind.InOut),
                        Face(0.85f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Author.Prop("bell"), AnchorId = "Slot_2", Label = "The brass bell",
                    Quip = "Awake. Still frozen.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "bell"),
                        Move(0f, 0.90f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 16f, ease: EaseKind.InOut),
                        Move(0.20f, 0.80f, StepKind.Shake, "IceShell", Vector3.zero,
                            amplitude: 2.5f, ease: EaseKind.InOut),
                        Face(0.28f, SceneRef.PepB, PepFace.Panic),
                        Face(0.40f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Author.Prop("hair_dryer"), AnchorId = "Slot_3",
                    Label = "The warm hair dryer",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        // 1. Arc from Slot 3 to IceShell
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.50f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.77f, 1.92f), amplitude: 0.50f, ease: EaseKind.Hop),
                        // 2. Warm blast & melt
                        Sfx(0.50f, "whoosh"),
                        Move(0.50f, 0.45f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 4.5f, ease: EaseKind.InOut),
                        Resize(0.55f, 0.48f, "IceShell", 0.05f, EaseKind.In),
                        Move(0.55f, 0.48f, StepKind.Fly, "IceShell", new Vector3(0f, -0.16f, 0f), ease: EaseKind.In),
                        // 3. Melt puddle reveals
                        Move(1.05f, 0.18f, StepKind.Show, "MeltPuddle", Vector3.zero),
                        Sfx(1.08f, "splash"),
                        Haptic(1.10f, "light"),
                        Face(1.12f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.12f, SceneRef.PepA, PepFace.Hopeful),
                        // 4. Freed Pep B hops to Pep A
                        Move(1.22f, 0.50f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.38f, 0f, -0.28f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(1.75f, 0.70f),
                        Sfx(1.80f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSprout(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r08_sprout.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            // R3.2 EXPAND: Reach Ledge — multi-step plant growth elevating Pep B to upper terrace
            Author.Stage(rescue, "r08", "sprout", "Reach the ledge.", Difficulty.Medium,
                ReasoningKind.Growth, "Diorama_Weather_Bloom",
                "Full sun on the middle terrace. One Pep stands on a tiny potted flower, far below the " +
                "snow shelf where their partner waits.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "scissors", Prop = Author.Prop("scissors"), AnchorId = "Slot_1",
                    Label = "The purple-handled scissors",
                    Quip = "Neat trim. Too short.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.78f, 0.47f, 1.74f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Sfx(0.68f, "snip"),
                        Resize(0.70f, 0.50f, "Plant", 0.55f, EaseKind.Out),
                        Move(0.70f, 0.50f, StepKind.Fly, SceneRef.PepB, new Vector3(0f, -0.08f, 0f)),
                        Face(0.86f, SceneRef.PepB, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Author.Prop("watering_can"), AnchorId = "Slot_2",
                    Label = "The blue watering can",
                    Duration = 3.0f,
                    Steps = new[]
                    {
                        // 1. Watering can arcs from Slot 2 over to the flower pot
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.12f, 0.65f, 1.74f), amplitude: 0.55f, ease: EaseKind.Hop),
                        // 2. Can tilts & pours water
                        Rotate(0.55f, 0.35f, SceneRef.Self, new Vector3(0f, 0f, -44f)),
                        Sfx(0.60f, "splash"),
                        // 3. Flower pot shakes & responds
                        Move(0.75f, 0.30f, StepKind.Shake, "Plant", Vector3.zero, amplitude: 5f),
                        // 4. Plant shoots up & expands into flower platform elevator
                        Resize(0.88f, 0.82f, "Plant", 2.35f, EaseKind.Back),
                        Move(0.88f, 0.82f, StepKind.Fly, "Plant", new Vector3(0f, 0.16f, 0f), ease: EaseKind.Back),
                        // 5. Pep B rises on the blossom elevator
                        Move(0.88f, 0.82f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, 0.24f, 0f), ease: EaseKind.Back),
                        Haptic(1.10f, "medium"),
                        Face(1.15f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.15f, SceneRef.PepA, PepFace.Hopeful),
                        // 6. Pep B hops onto upper terrace shelf to reunite
                        Move(1.78f, 0.56f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.40f, -0.12f, 0.44f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(2.35f, 0.65f),
                        Sfx(2.40f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_3",
                    Label = "The orange balloon",
                    Quip = "Still out of reach.",
                    Duration = 2.5f,
                    Steps = PropGags.Balloon(),
                },
            };

            rescue.CorrectIndex = 1;
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

            // R3.3 CLIMAX: Tempest to Rainbow — World transformed from raging storm to sunny rainbow
            Author.Stage(rescue, "r09", "shelter", "Stay out of rain.", Difficulty.Medium,
                ReasoningKind.Shelter, "Diorama_Weather_Downpour",
                "A dark tempest lashes the hillside while one Pep is stranded across the swollen stream.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_1",
                    Label = "The orange umbrella",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        // 1. Umbrella arcs from Slot 1 high across to stranded Pep B
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.78f, 0.63f, 1.74f), amplitude: 0.58f, ease: EaseKind.Hop),
                        // 2. Umbrella snaps open & spins in the updraft
                        Sfx(0.65f, "pop"),
                        Move(0.65f, 0.35f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 180f),
                        Resize(0.65f, 0.35f, SceneRef.Self, 1.22f, EaseKind.Back),
                        // 3. Updraft carries Umbrella & Pep B across the flooded torrent
                        Move(0.92f, 0.88f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.18f, -0.46f), amplitude: 0.32f, ease: EaseKind.InOut),
                        Move(0.92f, 0.88f, StepKind.Arc, SceneRef.PepB,
                            new Vector3(-0.45f, 0.00f, -0.46f), amplitude: 0.32f, ease: EaseKind.InOut),
                        Face(1.15f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.15f, SceneRef.PepA, PepFace.Hopeful),
                        // 4. WORLD CLIMAX TRANSFORMATION: Storm breaks into sunshine and rainbow!
                        Move(1.38f, 0.22f, StepKind.Hide, "Rain", Vector3.zero),
                        Move(1.40f, 0.95f, StepKind.FlyOff, "Cloud",
                            new Vector3(1.15f, 0.60f, 0.40f), ease: EaseKind.In),
                        Resize(1.50f, 0.60f, "FloodStream", 0.04f, EaseKind.In),
                        Move(1.60f, 0.35f, StepKind.Show, "SunGlow", Vector3.zero),
                        Move(1.65f, 0.70f, StepKind.Show, "Rainbow", Vector3.zero),
                        Resize(1.65f, 0.70f, "Rainbow", 1.00f, EaseKind.Back),
                        Haptic(1.70f, "success"),
                        // 5. Climactic Reunion under the Rainbow
                        Meet(2.78f, 0.72f),
                        Sfx(2.84f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_2", Label = "The caged electric fan",
                    Quip = "Rain is sideways now.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Rotate(0.24f, 0.70f, "Rain", new Vector3(0f, 0f, 34f)),
                        Move(0.24f, 0.70f, StepKind.Fly, "Rain", new Vector3(-0.52f, 0f, -0.20f)),
                        Move(0.28f, 0.70f, StepKind.Fly, "Cloud", new Vector3(-0.46f, 0f, -0.18f)),
                        Face(0.62f, SceneRef.PepA, PepFace.Panic),
                        Move(1.10f, 0.60f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(-0.14f, 0f, -0.18f), amplitude: 0.10f, ease: EaseKind.Hop),
                    },
                },
                new RescueObject
                {
                    Id = "leaf", Prop = Author.Prop("leaf"), AnchorId = "Slot_3", Label = "The broad green leaf",
                    Quip = "One leaf. Whole storm.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.68f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.34f, 0.55f, 1.36f), amplitude: 0.48f, ease: EaseKind.Hop),
                        Face(0.74f, SceneRef.PepB, PepFace.Hopeful),
                        Move(0.90f, 0.50f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 9f, ease: EaseKind.InOut),
                        Resize(1.06f, 0.42f, SceneRef.Self, 0.30f, EaseKind.In),
                        Sfx(1.10f, "poof"),
                        Face(1.44f, SceneRef.PepB, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
