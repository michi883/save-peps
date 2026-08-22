using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 1 — Garden.** *World rule: simple things do simple jobs. A flat
    /// thing spans, a loud thing wakes, a sharp thing cuts.*
    ///
    /// The one round the revamp deliberately left alone. It is the tutorial,
    /// and its three rescues already teach three different kinds of thinking —
    /// span a gap, act on an intermediary, remove an obstruction — in three
    /// different spatial compositions. What changed is underneath: it now has
    /// a world of its own rather than lending its dioramas to rounds four,
    /// five and seven.
    ///
    /// Only-here rescue: **r01**, the plank across the brook. Every later
    /// world takes this away — the canyon is too wide, the sea takes it, the
    /// city is a block across — and the joke only works because it worked here
    /// first.
    /// </summary>
    public static class RoundOneRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r01 = BuildBridge(overwrite, log);
            var r02 = BuildWake(overwrite, log);
            var r03 = BuildPrune(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_01.asset", overwrite, log, out var round))
            {
                round.Number = 1;
                round.Rescues = new[] { r01, r02, r03 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildBridge(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r01_brook.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r01", "bridge", "Cross the brook.", Difficulty.Easy,
                ReasoningKind.Crossing, "Diorama_Garden_Brook",
                "The Peps lean toward each other from opposite banks of a narrow blue brook.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "plank", Prop = Author.Prop("plank"), AnchorId = "Slot_1", Label = "The wooden plank",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.7f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.02f, 1.25f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.68f, "thud"),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.9f, 0.95f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Haptic(0.95f, "light"),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Author.Prop("bell"), AnchorId = "Slot_2", Label = "The brass bell",
                    Quip = "The brook is awake.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "bell"),
                        Move(0f, 0.9f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 16f, ease: EaseKind.InOut),
                        Move(0.2f, 0.8f, StepKind.Shake, "Water", Vector3.zero,
                            amplitude: 2.5f, ease: EaseKind.InOut),
                        Face(0.25f, SceneRef.PepA, PepFace.Panic),
                        Face(0.25f, SceneRef.PepB, PepFace.Panic),
                        Move(0.45f, 0.45f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(-0.12f, 0f, -0.08f), amplitude: 0.08f, ease: EaseKind.Hop),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_3",
                    Label = "The orange balloon",
                    Quip = "Up is not across.",
                    Duration = 2.5f,
                    Steps = PropGags.Balloon(),
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildWake(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r02_wake.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r02", "wake", "Wake the helper.", Difficulty.Easy,
                ReasoningKind.Activation, "Diorama_Garden_Gate",
                "A sleeping toy helper slumps beside a lever while a gate keeps the Peps apart diagonally.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "Still asleep.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.95f, 0.25f, 1.48f), amplitude: 0.38f, ease: EaseKind.Hop),
                        Sfx(0.65f, "poof"),
                        Resize(0.65f, 0.25f, SceneRef.Self, 0.82f, EaseKind.Hop),
                        Move(0.68f, 0.55f, StepKind.Drop, "Helper", new Vector3(0f, -0.045f, 0f)),
                        Move(0.72f, 0.8f, StepKind.Fly, "Zzz", new Vector3(0.03f, 0.16f, 0f)),
                        Face(0.8f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bone", Prop = Author.Prop("bone"), AnchorId = "Slot_2", Label = "The white dog bone",
                    Quip = "Good bone. Wrong helper.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.01f, 0.24f, 1.48f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(0.62f, "bonk"),
                        Move(0.62f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.12f, -0.12f, -0.18f), amplitude: 0.20f, ease: EaseKind.Hop),
                        Move(0.62f, 0.68f, StepKind.Shake, "Helper", Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Move(0.68f, 0.65f, StepKind.Fly, "Zzz", new Vector3(0.08f, 0.06f, 0f)),
                        Face(0.76f, SceneRef.PepA, PepFace.Worried),
                        Face(0.76f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Author.Prop("bell"), AnchorId = "Slot_3", Label = "The brass bell",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.47f, 0.26f, 1.22f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(0.53f, "bell"),
                        Haptic(0.55f, "light"),
                        Move(0.52f, 0.55f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 18f, ease: EaseKind.InOut),
                        Move(0.64f, 0.12f, StepKind.Hide, "SleepMask", Vector3.zero),
                        Move(0.64f, 0.12f, StepKind.Hide, "Zzz", Vector3.zero),
                        Move(0.74f, 0.48f, StepKind.Hop, "Helper",
                            new Vector3(0.12f, 0f, -0.03f), amplitude: 0.10f, ease: EaseKind.Hop),
                        Sfx(0.95f, "click"),
                        Move(0.95f, 0.55f, StepKind.Fly, "Gate",
                            new Vector3(0f, 0.64f, 0f), ease: EaseKind.InOut),
                        Face(1.02f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.02f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.42f, 0.75f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(0.40f, 0f, -0.94f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.30f, 0.70f),
                        Sfx(2.35f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildPrune(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r03_prune.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r03", "prune", "Clear the vines.", Difficulty.Medium,
                ReasoningKind.Cutting, "Diorama_Garden_Trellis",
                "One Pep is visible behind a thick vertical trellis of crossed green vines; their partner waits in front-left.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "watering_can", Prop = Author.Prop("watering_can"), AnchorId = "Slot_1",
                    Label = "The blue watering can",
                    Quip = "Too much water.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.73f, 0.24f, 1.62f), amplitude: 0.32f, ease: EaseKind.Hop),
                        Sfx(0.66f, "splash"),
                        Rotate(0.64f, 0.38f, SceneRef.Self, new Vector3(0f, 0f, -42f)),
                        Resize(0.72f, 0.70f, "Vines", 1.24f, EaseKind.Back),
                        Move(0.72f, 0.70f, StepKind.Fly, "Vines", new Vector3(0f, 0.10f, 0f),
                            ease: EaseKind.Back),
                        Face(0.84f, SceneRef.PepB, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Author.Prop("scissors"), AnchorId = "Slot_2",
                    Label = "The purple-handled scissors",
                    Duration = 3.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.25f, 0.25f, 1.65f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(0.58f, "snip"),
                        Move(0.55f, 0.55f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 16f, ease: EaseKind.InOut),
                        Sfx(0.88f, "snip"),
                        Move(0.90f, 0.16f, StepKind.Hide, "Vines", Vector3.zero),
                        Haptic(0.92f, "medium"),
                        Face(1.02f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.02f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.15f, 0.72f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.34f, 0f, -0.66f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.02f, 0.72f),
                        Sfx(2.08f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_3", Label = "The caged electric fan",
                    Quip = "Leaves up. Roots stuck.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.0f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 7f, ease: EaseKind.InOut),
                        Move(0.18f, 1.0f, StepKind.Shake, "Vines", Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Face(0.32f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.18f, SceneRef.PepB, PepFace.Worried),
                        Move(0.38f, 0.65f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.13f, 0f, -0.10f)),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
