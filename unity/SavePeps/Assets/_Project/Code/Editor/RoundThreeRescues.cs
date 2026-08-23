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
    /// Three spatial scales, three terraces, three skies. This is the only
    /// round whose stages deliberately carry different atmospheres — frost
    /// pale and blue around one local shell, full sun and gold over a diagonal
    /// living causeway, then a frame-filling downpour whose drainage changes
    /// every terrace — because "the weather changed the world" is exactly
    /// what the round is teaching. Every answer is a *field* applied to a
    /// place: heat, water, cover.
    ///
    /// Only-here rescue: **r09**, the rain. Wind visibly drives a wheel and
    /// sluice before the banks, torrent, route and atmosphere change state;
    /// both wrong answers still move the weather rather than the obstacle.
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

            // R3.2 LANDSCAPE: watering one root builds a traversable living
            // stair diagonally across all three terrace elevations.
            Author.Stage(rescue, "r08", "sprout", "Reach the ledge.", Difficulty.Medium,
                ReasoningKind.Growth, "Diorama_Weather_Bloom",
                "A dry basin lies at the near foot of the hill. One Pep stands on its tiny potted " +
                "flower while their partner waits on the opposite high ledge.");

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
                            new Vector3(0.82f, 0.38f, 1.08f), amplitude: 0.55f, ease: EaseKind.Hop),
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
                    Duration = 3.45f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.52f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.08f, 0.55f, 1.08f), amplitude: 0.52f, ease: EaseKind.Hop),
                        Rotate(0.50f, 0.32f, SceneRef.Self, new Vector3(0f, 0f, -44f)),
                        Sfx(0.56f, "splash"),
                        Move(0.66f, 0.30f, StepKind.Shake, "RootHeave", Vector3.zero,
                            amplitude: 5.5f, ease: EaseKind.InOut),
                        Move(0.70f, 0.34f, StepKind.Shake, "Plant", Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),

                        // The local seedling rises first. Then the ground
                        // heaves and a full route grows away from it.
                        Resize(0.84f, 0.36f, "Plant", 1.55f, EaseKind.Back),
                        Move(0.84f, 0.36f, StepKind.Fly, "Plant",
                            new Vector3(0f, 0.06f, 0f), ease: EaseKind.Back),
                        Move(0.84f, 0.36f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, 0.09f, 0f), ease: EaseKind.Back),
                        // The tool has done its job; clear the foreground so
                        // the stage-sized causeway owns the rest of the beat.
                        Move(0.92f, 0.44f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(1.18f, 0.28f, -0.34f), ease: EaseKind.In),
                        Resize(0.92f, 0.44f, "RootHeave", 1.24f, EaseKind.Back),
                        Move(0.92f, 0.44f, StepKind.Fly, "RootHeave",
                            new Vector3(0f, 0.055f, 0f), ease: EaseKind.Back),
                        Sfx(1.02f, "crunch"),
                        Impact(1.04f, 0.68f),
                        Haptic(1.06f, "medium"),

                        Move(1.00f, 0.46f, StepKind.Show, "VineSpine",
                            new Vector3(0f, 0.14f, 0f), ease: EaseKind.Back),
                        Move(1.14f, 0.34f, StepKind.Show, "VineStep1",
                            new Vector3(0f, 0.14f, 0f), ease: EaseKind.Back),
                        Sfx(1.20f, "boing"),
                        Move(1.28f, 0.34f, StepKind.Show, "VineStep2",
                            new Vector3(0f, 0.14f, 0f), ease: EaseKind.Back),
                        Move(1.42f, 0.34f, StepKind.Show, "VineStep3",
                            new Vector3(0f, 0.14f, 0f), ease: EaseKind.Back),
                        Move(1.56f, 0.40f, StepKind.Show, "VineCrown",
                            new Vector3(0f, 0.14f, 0f), ease: EaseKind.Back),
                        Resize(1.56f, 0.40f, "VineCrown", 1.15f, EaseKind.Back),
                        Haptic(1.72f, "medium"),
                        Face(1.72f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.72f, SceneRef.PepA, PepFace.Hopeful),

                        // Pep B uses the new landscape as four distinct steps,
                        // climbing the full diagonal rather than riding one
                        // uniformly scaled platform.
                        Move(1.82f, 0.27f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.10f, 0f, 0.33f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Move(2.07f, 0.29f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.21f, 0.13f, 0.38f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Move(2.34f, 0.29f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.23f, 0.14f, 0.37f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Move(2.61f, 0.31f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.18f, 0.03f, 0.30f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(2.96f, 0.40f),
                        Sfx(3.02f, "reunion"),
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

            // R3.3 WORLD EVENT: the umbrella catches the gale, drives the
            // drainage works, reshapes every terrace and reveals a new route.
            Author.Stage(rescue, "r09", "shelter", "Stay out of rain.", Difficulty.Medium,
                ReasoningKind.Shelter, "Diorama_Weather_Downpour",
                "A dark tempest has mud-choked all three terraces. The Peps occupy opposite corners " +
                "of a swollen torrent with no route between them.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_1",
                    Label = "The orange umbrella",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.92f, 0.78f, 2.62f), amplitude: 0.82f, ease: EaseKind.Hop),
                        Sfx(0.58f, "pop"),
                        Move(0.58f, 0.28f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 210f),
                        Resize(0.58f, 0.28f, SceneRef.Self, 1.28f, EaseKind.Back),
                        Face(0.66f, SceneRef.PepB, PepFace.Hopeful),
                        Face(0.66f, SceneRef.PepA, PepFace.Hopeful),

                        // The opened canopy catches the crosswind beside the
                        // wheel. One visible mechanism initiates the whole
                        // environmental chain: wheel -> gate -> drainage.
                        Sfx(0.76f, "wind"),
                        Move(0.74f, 0.46f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Move(0.78f, 0.62f, StepKind.Spin, "DrainWheel", Vector3.zero,
                            amplitude: 540f, ease: EaseKind.InOut),
                        Sfx(0.86f, "ratchet"),
                        Move(0.86f, 0.54f, StepKind.Fly, "SluiceGate",
                            new Vector3(0f, 0.34f, 0f), ease: EaseKind.Back),
                        Impact(0.92f, 0.58f),
                        Haptic(0.94f, "medium"),

                        Move(1.04f, 0.30f, StepKind.Shake, "StormBankLeft", Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        Move(1.08f, 0.30f, StepKind.Shake, "StormBankRight", Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        Sfx(1.10f, "rumble"),
                        Ambient(1.08f, 0.30f, "FloodCurrent", 0f),
                        Move(1.14f, 0.68f, StepKind.FlyOff, "StormDebris",
                            new Vector3(-0.20f, -0.22f, -1.30f), ease: EaseKind.In),
                        Move(1.18f, 0.72f, StepKind.FlyOff, "Awning",
                            new Vector3(-1.25f, 0.44f, -0.24f), ease: EaseKind.In),

                        // Every large system now changes state across the
                        // frame: air, cloud, light, terrain and water.
                        Ambient(1.16f, 0.48f, "Rainfall", 0f),
                        Ambient(1.16f, 0.50f, "StormGusts", 0f),
                        Ambient(1.16f, 0.48f, "StormCloud", 0f),
                        Ambient(1.16f, 0.62f, "StormTrees", 0f),
                        Ambient(1.16f, 0.54f, "StormVane", 0.05f),
                        Atmosphere(1.18f, 0.98f, "sunbreak"),
                        Move(1.22f, 0.88f, StepKind.FlyOff, "Cloud",
                            new Vector3(1.65f, 0.72f, 0.30f), ease: EaseKind.In),

                        VisibilitySwap(1.38f, "StormBankLeft", "ClearBankLeft"),
                        Move(1.38f, 0.50f, StepKind.Fly, "ClearBankLeft",
                            new Vector3(0.10f, 0.12f, 0f), ease: EaseKind.Back),
                        VisibilitySwap(1.46f, "StormBankRight", "ClearBankRight"),
                        Move(1.46f, 0.50f, StepKind.Fly, "ClearBankRight",
                            new Vector3(-0.10f, 0.12f, 0f), ease: EaseKind.Back),
                        Sfx(1.46f, "crunch"),
                        Impact(1.46f, 1.30f),
                        Haptic(1.48f, "heavy"),
                        VisibilitySwap(1.56f, "Rain", "Sunbeams"),
                        Resize(1.56f, 0.46f, "Sunbeams", 1.15f, EaseKind.Back),
                        VisibilitySwap(1.64f, "FloodStream", "RiverThread"),
                        Sfx(1.66f, "splash"),

                        // The route rises out of the drained torrent from far
                        // shelf to near shelf, then the cleared world blooms.
                        Move(1.58f, 0.34f, StepKind.Show, "CausewayStep1",
                            new Vector3(0f, 0.16f, 0f), ease: EaseKind.Back),
                        Move(1.68f, 0.34f, StepKind.Show, "CausewayStep2",
                            new Vector3(0f, 0.16f, 0f), ease: EaseKind.Back),
                        Move(1.78f, 0.34f, StepKind.Show, "CausewayStep3",
                            new Vector3(0f, 0.16f, 0f), ease: EaseKind.Back),
                        Move(1.88f, 0.34f, StepKind.Show, "CausewayStep4",
                            new Vector3(0f, 0.16f, 0f), ease: EaseKind.Back),
                        Move(1.72f, 0.38f, StepKind.Show, "SunGlow", new Vector3(0f, 0.06f, 0f),
                            ease: EaseKind.Back),
                        Resize(1.72f, 0.38f, "SunGlow", 1.28f, EaseKind.Back),
                        Move(1.90f, 0.38f, StepKind.Show, "MeadowBurst",
                            new Vector3(0f, 0.12f, 0f), ease: EaseKind.Back),
                        Resize(1.90f, 0.38f, "MeadowBurst", 1.12f, EaseKind.Back),
                        Move(2.04f, 0.42f, StepKind.Show, "Rainbow",
                            new Vector3(0f, 0.08f, 0f), ease: EaseKind.Back),
                        Resize(2.04f, 0.42f, "Rainbow", 1.12f, EaseKind.Back),
                        Sfx(2.08f, "chime"),
                        Impact(2.10f, 0.42f),
                        Haptic(2.10f, "success"),

                        // Pep B and the shelter now traverse the newly made
                        // route from the far high corner to Pep A below.
                        Move(2.12f, 0.27f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.10f, 0.02f, -0.40f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Move(2.12f, 0.27f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.10f, 0.02f, -0.40f), ease: EaseKind.InOut),
                        Move(2.37f, 0.27f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.20f, -0.27f, -0.42f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Move(2.37f, 0.27f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.20f, -0.27f, -0.42f), ease: EaseKind.InOut),
                        Move(2.62f, 0.27f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.22f, 0f, -0.42f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Move(2.62f, 0.27f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.22f, 0f, -0.42f), ease: EaseKind.InOut),
                        Move(2.87f, 0.29f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.24f, -0.26f, -0.44f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Move(2.87f, 0.29f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.24f, -0.26f, -0.44f), ease: EaseKind.InOut),
                        Meet(3.18f, 0.35f),
                        Sfx(3.22f, "reunion"),
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
                            new Vector3(0.48f, 0.62f, 2.24f), amplitude: 0.68f, ease: EaseKind.Hop),
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
