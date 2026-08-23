using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 11 — Foundry floor.** *World rule: the machine is already
    /// running and it will not wait for you. Feed it, cool it, or jam it.*
    ///
    /// The loudest and busiest world, and the only one lit from underneath —
    /// a crucible trough runs through the deck, so the ambient ground colour
    /// is molten gold and every silhouette has a hot rim. Conveyor slats
    /// scroll, a piston hammers on a beat and steam vents on its own clock
    /// before the player has touched anything, which is the round's whole
    /// argument: the world was here first.
    ///
    /// Only-here rescue: **r32**, quenching a river of molten metal into a
    /// black bridge. Water is a gardening tool in round three and a fire
    /// hazard in round eight; here it is the only thing in the building that
    /// can make a floor.
    /// </summary>
    public static class RoundElevenRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r31 = BuildFeed(overwrite, log);
            var r32 = BuildQuench(overwrite, log);
            var r33 = BuildJam(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_11.asset", overwrite, log, out var round))
            {
                round.Number = 11;
                round.Rescues = new[] { r31, r32, r33 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildFeed(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r31_feed.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r31", "feed", "Open the gate.", Difficulty.Medium,
                ReasoningKind.Activation, "Diorama_Forge_Conveyor",
                "A conveyor runs empty through a scanner arch to a shuttered gate, and the shutter " +
                "only lifts for something on the belt.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "crate", Prop = Author.Prop("crate"), AnchorId = "Slot_1", Label = "The wooden crate",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.48f, 0.065f, 0.60f), amplitude: 0.36f, ease: EaseKind.Hop),
                        Sfx(0.60f, "thud"),
                        Move(0.64f, 1.10f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0f, 1.50f), ease: EaseKind.Linear),
                        Move(1.16f, 0.24f, StepKind.Show, "ScanBeam", Vector3.zero),
                        Sfx(1.20f, "chime"),
                        Move(1.44f, 0.30f, StepKind.Hide, "ScanBeam", Vector3.zero),
                        Sfx(1.50f, "clank"),
                        Haptic(1.52f, "medium"),
                        Move(1.52f, 0.66f, StepKind.Fly, "ShutterGate",
                            new Vector3(0f, 0.66f, 0f), ease: EaseKind.InOut),
                        Move(1.74f, 0.60f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0f, 0f, 0.50f), ease: EaseKind.Linear),
                        Face(1.60f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.60f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.72f, 1.00f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.38f, 0f, 2.00f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(2.74f, 0.72f),
                        Sfx(2.80f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Quip = "Now it is a pancake.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.48f, 0.065f, 0.60f), amplitude: 0.36f, ease: EaseKind.Hop),
                        Sfx(0.60f, "poof"),
                        Move(0.64f, 0.80f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0f, 1.06f), ease: EaseKind.Linear),
                        Sfx(1.44f, "clank"),
                        Resize(1.46f, 0.40f, SceneRef.Self, 0.42f, EaseKind.In),
                        Move(1.46f, 0.40f, StepKind.Fly, SceneRef.Self, new Vector3(0f, -0.03f, 0f)),
                        Face(1.52f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Author.Prop("watering_can"), AnchorId = "Slot_3",
                    Label = "The blue watering can",
                    Quip = "Wet belt. No grip.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.30f, 0.92f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Rotate(0.62f, 0.34f, SceneRef.Self, new Vector3(0f, 0f, -44f)),
                        Sfx(0.66f, "splash"),
                        Move(0.98f, 0.90f, StepKind.Shake, "Belt", Vector3.zero,
                            amplitude: 2.2f, ease: EaseKind.InOut),
                        Sfx(1.02f, "hiss"),
                        Face(1.10f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildQuench(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r32_quench.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r32", "quench", "Cool the spill.", Difficulty.Surprising,
                ReasoningKind.Temperature, "Diorama_Forge_Spill",
                "A tipped ladle has poured a river of glowing metal across the only walkway between " +
                "the two decks.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "plank", Prop = Author.Prop("plank"), AnchorId = "Slot_1", Label = "The wooden plank",
                    Quip = "Wood, briefly.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.50f, 0.03f, 1.38f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Sfx(0.62f, "sizzle"),
                        Face(0.68f, SceneRef.PepA, PepFace.Hopeful),
                        Resize(0.86f, 0.60f, SceneRef.Self, 0.18f, EaseKind.In),
                        Move(1.46f, 0.30f, StepKind.Hide, SceneRef.Self, Vector3.zero),
                        Face(1.52f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Author.Prop("watering_can"), AnchorId = "Slot_2",
                    Label = "The blue watering can",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.46f, 0.30f, 1.46f), amplitude: 0.44f, ease: EaseKind.Hop),
                        Rotate(0.64f, 0.34f, SceneRef.Self, new Vector3(0f, 0f, -46f)),
                        Sfx(0.70f, "sizzle"),
                        Haptic(0.72f, "medium"),
                        Move(0.76f, 0.26f, StepKind.Show, "Steam", Vector3.zero),
                        Resize(0.78f, 0.80f, "Steam", 2.10f, EaseKind.Out),
                        Move(0.78f, 0.80f, StepKind.Fly, "Steam", new Vector3(0f, 0.34f, 0f)),
                        Move(1.06f, 0.24f, StepKind.Hide, "Spill", Vector3.zero),
                        Move(1.06f, 0.26f, StepKind.Show, "Crust", Vector3.zero),
                        Move(1.60f, 0.40f, StepKind.Hide, "Steam", Vector3.zero),
                        Face(1.36f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.36f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.62f, 1.00f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.34f, 0f, 1.88f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(2.70f, 0.74f),
                        Sfx(2.76f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_3", Label = "The caged electric fan",
                    Quip = "Hotter. Much hotter.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Resize(0.30f, 0.80f, "Spill", 1.14f, EaseKind.Out),
                        Move(0.30f, 0.80f, StepKind.Fly, "Spill", new Vector3(0f, 0.02f, 0f)),
                        Sfx(0.34f, "hiss"),
                        Face(0.52f, SceneRef.PepA, PepFace.Panic),
                        Move(0.60f, 0.70f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, -0.20f), amplitude: 0.10f, ease: EaseKind.Hop),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildJam(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r33_jam.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r33", "jam", "Stop the piston.", Difficulty.Medium,
                ReasoningKind.Momentum, "Diorama_Forge_Piston",
                "A massive press hammers the floor over a smelting abyss, blocking the route to the high " +
                "tower. Jamming its linkage trips emergency brakes and swings down the gantry skywalk.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_1", Label = "The coil of rope",
                    Quip = "The ram won.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.64f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.50f, 0.60f, 1.32f), amplitude: 0.56f, ease: EaseKind.Hop),
                        Sfx(0.68f, "creak"),
                        Move(0.70f, 0.50f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 16f, ease: EaseKind.InOut),
                        Sfx(1.22f, "clank"),
                        Move(1.24f, 0.30f, StepKind.Hide, SceneRef.Self, Vector3.zero),
                        Face(1.30f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "magnet", Prop = Author.Prop("magnet"), AnchorId = "Slot_2",
                    Label = "The horseshoe magnet",
                    Quip = "Now it hammers too.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.46f, 0.62f, 1.32f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Sfx(0.66f, "snap_on"),
                        Move(0.70f, 0.44f, StepKind.Drop, SceneRef.Self, new Vector3(0f, -0.30f, 0f)),
                        Move(1.14f, 0.44f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.30f, 0f),
                            ease: EaseKind.Out),
                        Sfx(1.16f, "clank"),
                        Move(1.58f, 0.44f, StepKind.Drop, SceneRef.Self, new Vector3(0f, -0.30f, 0f)),
                        Face(0.94f, SceneRef.PepB, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "wrench", Prop = Author.Prop("wrench"), AnchorId = "Slot_3", Label = "The steel spanner",
                    Duration = 3.55f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.40f, 0.765f, 1.18f), amplitude: 0.58f, ease: EaseKind.Hop),
                        Sfx(0.58f, "clank"),
                        Haptic(0.60f, "heavy"),
                        Impact(0.60f, 1.8f),
                        Move(0.60f, 0.36f, StepKind.Shake, "Linkage", Vector3.zero,
                            amplitude: 14f, ease: EaseKind.InOut),
                        Move(0.60f, 0.24f, StepKind.Show, "Sparks", Vector3.zero),
                        Resize(0.62f, 0.44f, "Sparks", 2.20f, EaseKind.Out),
                        VisibilitySwap(0.64f, "ActivePressComplex", "TransformedForgeWorld"),
                        Sfx(0.66f, "hiss"),
                        Move(0.66f, 0.30f, StepKind.Show, "SteamBurst", Vector3.zero),
                        Resize(0.68f, 0.50f, "SteamBurst", 1.80f, EaseKind.Out),
                        Sfx(1.10f, "thud"),
                        Haptic(1.12f, "medium"),
                        Impact(1.12f, 0.8f),
                        Sfx(1.22f, "snap_on"),
                        Move(1.24f, 0.30f, StepKind.Hide, "Sparks", Vector3.zero),
                        Move(1.40f, 0.40f, StepKind.Hide, "SteamBurst", Vector3.zero),
                        Face(1.26f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.26f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.42f, 1.14f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.62f, 0.34f, 2.12f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(2.60f, 0.85f),
                        Sfx(2.66f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
