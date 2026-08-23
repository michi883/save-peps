using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 6 — Storm rooftop.** *World rule: the wind has a direction and
    /// it is taking things. Anything loose is already leaving, and there is
    /// nothing underneath.*
    ///
    /// A narrow roof on a shaft that drops out of frame, under the first
    /// near-black sky in the game, with rain crossing the whole picture
    /// and a lightning flash on a long duty cycle. The umbrella that glided a
    /// canyon and sheltered a terrace is now a liability, and the round says so
    /// in its first wrong answer.
    ///
    /// Only-here rescue: **r17**, planting the rod so the next strike has
    /// somewhere better to go. It is the only rescue in the game where the
    /// answer does not remove the hazard — it redirects it.
    /// </summary>
    public static class RoundSixRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r16 = BuildPin(overwrite, log);
            var r17 = BuildGround(overwrite, log);
            var r18 = BuildChute(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_06.asset", overwrite, log, out var round))
            {
                round.Number = 6;
                round.Rescues = new[] { r16, r17, r18 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildPin(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r16_pin.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r16", "pin", "Tame the tarp.", Difficulty.Medium,
                ReasoningKind.Airflow, "Diorama_Storm_Tarp",
                "A tarp held down on one side only cracks and whips across the rooftop walkway between " +
                "the Peps.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_1",
                    Label = "The orange umbrella",
                    Quip = "Inside out. Then gone.",
                    Duration = 2.6f,
                    Steps = PropGags.Umbrella(),
                },
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_2", Label = "The coil of rope",
                    Quip = "The wind untied it.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.54f, 0.23f, 1.50f), amplitude: 0.46f, ease: EaseKind.Hop),
                        Sfx(0.66f, "wind"),
                        Move(0.66f, 0.90f, StepKind.Shake, "Tarp", Vector3.zero,
                            amplitude: 14f, ease: EaseKind.InOut),
                        Move(0.70f, 0.86f, StepKind.Shake, "TarpCorner", Vector3.zero,
                            amplitude: 20f, ease: EaseKind.InOut),
                        Move(1.58f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(-0.80f, 0.60f, -0.40f), ease: EaseKind.In),
                        Face(0.90f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "sandbag", Prop = Author.Prop("sandbag"), AnchorId = "Slot_3", Label = "The sandbag",
                    Duration = 2.8f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.54f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.34f, 0.07f, 0.72f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Sfx(0.56f, "thud"),
                        Haptic(0.58f, "light"),
                        // The tarp's flap and the taut tarp are two objects.
                        // Choreography can add motion to an idle but never
                        // remove one, so going still is a swap.
                        VisibilitySwap(0.72f, "Tarp", "TautTarp"),
                        VisibilitySwap(0.74f, "TarpCorner", "PinnedCorner"),
                        Sfx(0.80f, "creak"),
                        Face(0.88f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.88f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.02f, 0.88f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.04f, 0f, 1.58f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(2.02f, 0.54f),
                        Sfx(2.08f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildGround(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r17_ground.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r17", "ground", "Stop the lightning.", Difficulty.Surprising,
                ReasoningKind.Signal, "Diorama_Storm_Mast",
                "Lightning keeps finding the metal walkway between the Peps, and there are scorch " +
                "marks where it has already hit.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "lightning_rod", Prop = Author.Prop("lightning_rod"), AnchorId = "Slot_1",
                    Label = "The lightning rod",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.92f, 0.12f, 1.78f), amplitude: 0.62f, ease: EaseKind.Hop),
                        Sfx(0.60f, "clank"),
                        Move(0.72f, 0.14f, StepKind.Show, "Arc", Vector3.zero),
                        Move(0.76f, 0.14f, StepKind.Show, "Strike", Vector3.zero),
                        Sfx(0.76f, "zap"),
                        Impact(0.78f, 0.72f),
                        Haptic(0.80f, "medium"),
                        Atmosphere(0.80f, 0.62f, "grounded"),
                        Ambient(0.82f, 0.58f, "StormLightning", 0.05f),
                        Move(0.86f, 0.52f, StepKind.Shake, "Mast", Vector3.zero,
                            amplitude: 7f, ease: EaseKind.InOut),
                        Move(0.90f, 0.16f, StepKind.Show, "GroundPulseNear", Vector3.zero),
                        Move(1.00f, 0.16f, StepKind.Show, "GroundPulseMid", Vector3.zero),
                        Move(1.10f, 0.16f, StepKind.Show, "GroundPulseFar", Vector3.zero),
                        Sfx(1.10f, "ratchet"),
                        Move(1.10f, 0.52f, StepKind.Spin, "Relay", Vector3.zero,
                            amplitude: 460f, ease: EaseKind.InOut),
                        Move(1.02f, 0.16f, StepKind.Hide, "Strike", Vector3.zero),
                        Move(1.16f, 0.18f, StepKind.Hide, "Arc", Vector3.zero),
                        VisibilitySwap(1.34f, "LiveGrid", "SafeGrid"),
                        VisibilitySwap(1.44f, "ServiceBridgeLocked", "ServiceBridgeOpen"),
                        Move(1.46f, 0.20f, StepKind.Show, "SignalBeacons", Vector3.zero),
                        Resize(1.46f, 0.36f, "SignalBeacons", 1.16f, EaseKind.Back),
                        Move(1.50f, 0.20f, StepKind.Hide, "Scorch", Vector3.zero),
                        Sfx(1.48f, "chime"),
                        Impact(1.50f, 0.58f),
                        Haptic(1.52f, "medium"),
                        Face(1.56f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.56f, SceneRef.PepB, PepFace.Happy),
                        Move(1.68f, 0.44f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.30f, 0.02f, 0.58f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Move(2.10f, 0.42f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.27f, 0.01f, 0.52f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Move(2.18f, 0.42f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.10f, 0f, -0.32f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Meet(2.74f, 0.58f),
                        Sfx(2.80f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "lantern", Prop = Author.Prop("lantern"), AnchorId = "Slot_2", Label = "The cage lantern",
                    Quip = "The gale blew it out.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.24f, 0.33f, 1.06f), amplitude: 0.44f, ease: EaseKind.Hop),
                        Face(0.68f, SceneRef.PepA, PepFace.Hopeful),
                        Sfx(0.86f, "wind"),
                        Move(0.86f, 0.80f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 22f, ease: EaseKind.InOut),
                        Move(1.70f, 0.60f, StepKind.Hide, SceneRef.Self, Vector3.zero),
                        Face(1.76f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "plank", Prop = Author.Prop("plank"), AnchorId = "Slot_3", Label = "The wooden plank",
                    Quip = "Wet wood. Still a spark.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.02f, 0.03f, 1.48f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Sfx(0.62f, "thud"),
                        Move(0.92f, 0.14f, StepKind.Show, "Arc", Vector3.zero),
                        Sfx(0.94f, "zap"),
                        Move(1.10f, 0.24f, StepKind.Hide, "Arc", Vector3.zero),
                        Move(0.98f, 0.70f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 9f, ease: EaseKind.InOut),
                        Face(1.02f, SceneRef.PepA, PepFace.Panic),
                        Face(1.02f, SceneRef.PepB, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildChute(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r18_chute.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r18", "chute", "Slide down safely.", Difficulty.Medium,
                ReasoningKind.Momentum, "Diorama_Storm_Gutter",
                "A locked storm cistern towers over three disconnected roof islands. Folded spillway " +
                "panels stand upright while the other Pep waits at the lowest landing basin.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "It drank the whole roof.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.64f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.52f, 0.07f, 2.04f), amplitude: 0.52f, ease: EaseKind.Hop),
                        Sfx(0.66f, "splash"),
                        Resize(0.70f, 0.80f, SceneRef.Self, 0.74f, EaseKind.Out),
                        Move(0.70f, 0.80f, StepKind.Fly, SceneRef.Self, new Vector3(0f, -0.03f, 0f)),
                        Face(0.92f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "plank", Prop = Author.Prop("plank"), AnchorId = "Slot_2", Label = "The wooden plank",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.54f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.87f, 0.75f, 2.06f), amplitude: 0.92f, ease: EaseKind.Hop),
                        Rotate(0.42f, 0.24f, SceneRef.Self, new Vector3(-18f, -8f, 0f), EaseKind.Back),
                        Sfx(0.56f, "thud"),
                        Impact(0.58f, 0.46f),
                        Haptic(0.58f, "medium"),

                        // The plank completes the release linkage. Rainwater
                        // tips the cistern, pulls both shutter chains and sends
                        // a visible flood front through the whole roof.
                        Move(0.58f, 0.54f, StepKind.Spin, "DrainWheel", Vector3.zero,
                            amplitude: 620f, ease: EaseKind.InOut),
                        Sfx(0.66f, "ratchet"),
                        Move(0.66f, 0.50f, StepKind.Fly, "SpillwayChains",
                            new Vector3(0f, 0.32f, 0f), ease: EaseKind.In),
                        Rotate(0.72f, 0.58f, "StormTank", new Vector3(0f, 0f, -48f), EaseKind.In),
                        Move(0.72f, 0.58f, StepKind.Fly, "StormTank",
                            new Vector3(-0.16f, -0.18f, -0.10f), ease: EaseKind.In),
                        Move(0.78f, 0.12f, StepKind.Show, "WorldFlash", Vector3.zero),
                        Sfx(0.80f, "zap"),
                        Move(0.82f, 0.12f, StepKind.Show, "FloodFront", Vector3.zero),
                        Move(0.82f, 0.66f, StepKind.Fly, "FloodFront",
                            new Vector3(0.62f, -0.54f, -2.44f), ease: EaseKind.In),
                        Move(0.86f, 0.46f, StepKind.Shake, "LockedRoofWorld", Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Move(0.88f, 0.58f, StepKind.FlyOff, "StormDebris",
                            new Vector3(1.72f, 0.74f, -0.72f), ease: EaseKind.In),
                        Sfx(1.04f, "rumble"),
                        Move(1.06f, 0.14f, StepKind.Hide, "WorldFlash", Vector3.zero),

                        // WORLD STATE CHANGE: a vertical, broken rooftop is
                        // replaced by one continuous diagonal spillway. The
                        // storm remains; its force has become the route.
                        Haptic(1.26f, "heavy"),
                        Impact(1.26f, 1.38f),
                        Atmosphere(1.26f, 0.72f, "stormflow"),
                        Ambient(1.26f, 0.62f, "StormRain", 0.38f),
                        Ambient(1.26f, 0.58f, "StormLightning", 0.05f),
                        VisibilitySwap(1.26f, "LockedRoofWorld", "SpillwayWorld"),
                        Move(1.28f, 0.16f, StepKind.Show, "TorrentField", Vector3.zero),
                        Move(1.34f, 0.18f, StepKind.Show, "SafetyLights", Vector3.zero),
                        Resize(1.34f, 0.34f, "SafetyLights", 1.18f, EaseKind.Back),
                        Move(1.30f, 0.34f, StepKind.Shake, "SpillwayWorld", Vector3.zero,
                            amplitude: 2.5f, ease: EaseKind.InOut),
                        Move(1.50f, 0.12f, StepKind.Hide, "FloodFront", Vector3.zero),
                        Sfx(1.34f, "splash"),
                        Face(1.38f, SceneRef.PepA, PepFace.Happy),
                        Face(1.38f, SceneRef.PepB, PepFace.Hopeful),

                        // Pep A rides the placed plank through three long
                        // spillway legs, then catches air into the basin.
                        Sfx(1.56f, "glide_hiss"),
                        Move(1.56f, 0.34f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.26f, -0.22f, -0.68f), ease: EaseKind.InOut),
                        Move(1.56f, 0.34f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.26f, -0.22f, -0.68f), ease: EaseKind.InOut),
                        Sfx(1.90f, "splash"),
                        Move(1.90f, 0.34f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.30f, -0.25f, -0.70f), ease: EaseKind.InOut),
                        Move(1.90f, 0.34f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.30f, -0.25f, -0.70f), ease: EaseKind.InOut),
                        Sfx(2.24f, "glide_hiss"),
                        Move(2.24f, 0.32f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.25f, -0.18f, -0.56f), ease: EaseKind.In),
                        Move(2.24f, 0.32f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.25f, -0.18f, -0.56f), ease: EaseKind.In),
                        Move(2.54f, 0.30f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0.12f, -0.08f, -0.22f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Move(2.54f, 0.30f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.12f, -0.08f, -0.22f), amplitude: 0.28f, ease: EaseKind.Hop),
                        Move(2.66f, 0.14f, StepKind.Show, "LandingSpray", Vector3.zero),
                        Resize(2.66f, 0.38f, "LandingSpray", 1.45f, EaseKind.Out),
                        Sfx(2.84f, "splash"),
                        Impact(2.84f, 0.62f),
                        Haptic(2.84f, "success"),
                        Move(3.10f, 0.16f, StepKind.Hide, "LandingSpray", Vector3.zero),
                        Meet(3.02f, 0.50f),
                        Sfx(3.08f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_3", Label = "The grey stone",
                    Quip = "It kept going. Alone.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.08f, 0.07f, 2.16f), amplitude: 0.56f, ease: EaseKind.Hop),
                        Sfx(0.62f, "thud"),
                        Move(0.68f, 0.72f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.06f, -0.14f, 0.86f), ease: EaseKind.In),
                        Sfx(1.42f, "clunk"),
                        Move(1.44f, 0.86f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.20f, -1.30f, 0.42f), ease: EaseKind.In),
                        Face(1.50f, SceneRef.PepB, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
