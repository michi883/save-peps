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
    /// the game, with spindrift blowing across it. This is the last free round
    /// and it climaxes with the game's fastest single movement: a Pep on a
    /// sled from the cornice to the valley in nine tenths of a second.
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
                "Loose powder lies in soft mounds across the slope, with a Pep-shaped hole where " +
                "someone has already tried to walk on it.");

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
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.56f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.165f, 0.66f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Sfx(0.58f, "wind"),
                        Move(0.58f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        // Wind slab: the powder blows away sideways and what it
                        // leaves behind is hard enough to stand on.
                        Move(0.66f, 0.80f, StepKind.Fly, "Powder", new Vector3(-0.34f, -0.04f, 0f),
                            ease: EaseKind.Out),
                        Move(1.46f, 0.24f, StepKind.Hide, "Powder", Vector3.zero),
                        Move(1.46f, 0.26f, StepKind.Show, "Crust", Vector3.zero),
                        Move(1.46f, 0.26f, StepKind.Hide, "Sinkhole", Vector3.zero),
                        Sfx(1.50f, "crunch"),
                        Haptic(1.52f, "light"),
                        Face(1.56f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.56f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.66f, 0.90f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.36f, 0.31f, 0.96f), amplitude: 0.19f, ease: EaseKind.Hop),
                        Meet(2.62f, 0.74f),
                        Sfx(2.68f, "reunion"),
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
                "A carved chute runs from the cornice at the top of the slope all the way down to a " +
                "deep drift where the other Pep waits.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "sled", Prop = Author.Prop("sled"), AnchorId = "Slot_1", Label = "The little sled",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.78f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.66f, 0.96f, 2.88f), amplitude: 1.05f, ease: EaseKind.Hop),
                        Sfx(0.80f, "crunch"),
                        Face(0.88f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.92f, 0.36f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0.03f, 0f)),
                        Sfx(1.32f, "glide_hiss"),
                        Haptic(1.34f, "light"),
                        Move(1.32f, 0.92f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.16f, -0.80f, -2.16f), ease: EaseKind.In),
                        Move(1.32f, 0.92f, StepKind.Fly, SceneRef.Self,
                            new Vector3(-0.16f, -0.80f, -2.16f), ease: EaseKind.In),
                        Face(1.40f, SceneRef.PepA, PepFace.Happy),
                        Move(2.24f, 0.20f, StepKind.Show, "Poof", Vector3.zero),
                        Sfx(2.26f, "poof"),
                        Resize(2.26f, 0.50f, "Poof", 1.90f, EaseKind.Out),
                        Move(2.42f, 0.34f, StepKind.Hide, "Poof", Vector3.zero),
                        Meet(2.76f, 0.72f),
                        Sfx(2.82f, "reunion"),
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
                "A band of bare ice cuts across the slope between the Peps, with a rock bollard " +
                "standing at each end of it.");

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
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.64f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.46f, 0.62f, 1.42f), amplitude: 0.56f, ease: EaseKind.Hop),
                        Sfx(0.68f, "creak"),
                        Move(0.82f, 0.16f, StepKind.Hide, "RopeLine", Vector3.zero),
                        Move(0.82f, 0.18f, StepKind.Show, "TautLine", Vector3.zero),
                        Move(0.84f, 0.40f, StepKind.Shake, "Bollard", Vector3.zero,
                            amplitude: 2.5f, ease: EaseKind.InOut),
                        Haptic(0.86f, "light"),
                        Face(0.96f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.96f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.10f, 1.00f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.54f, 0f, -0.02f), amplitude: 0.10f, ease: EaseKind.Hop),
                        Sfx(1.20f, "crunch"),
                        Meet(2.30f, 0.72f),
                        Sfx(2.36f, "reunion"),
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
