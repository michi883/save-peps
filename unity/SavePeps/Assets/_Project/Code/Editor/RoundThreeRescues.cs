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

            Author.Stage(rescue, "r07", "thaw", "Melt the ice.", Difficulty.Easy,
                ReasoningKind.Temperature, "Diorama_Weather_Frost",
                "Snow is falling on the top terrace and one Pep peers out of a shell of faceted ice.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "Ice stays frozen.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.72f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.74f, 0.77f, 2.66f), amplitude: 0.78f, ease: EaseKind.Hop),
                        Sfx(0.74f, "poof"),
                        Move(0.76f, 0.60f, StepKind.Shake, "IceShell", Vector3.zero,
                            amplitude: 2f, ease: EaseKind.InOut),
                        Face(0.92f, SceneRef.PepB, PepFace.Worried),
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
                        Move(0f, 0.95f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 17f, ease: EaseKind.InOut),
                        Move(0.24f, 0.85f, StepKind.Shake, "IceShell", Vector3.zero,
                            amplitude: 2.5f, ease: EaseKind.InOut),
                        Face(0.30f, SceneRef.PepB, PepFace.Panic),
                        Face(0.42f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Author.Prop("hair_dryer"), AnchorId = "Slot_3",
                    Label = "The warm hair dryer",
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.77f, 1.92f), amplitude: 0.60f, ease: EaseKind.Hop),
                        Sfx(0.66f, "whoosh"),
                        Move(0.66f, 0.90f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Resize(0.74f, 0.80f, "IceShell", 0.06f, EaseKind.In),
                        Move(0.74f, 0.80f, StepKind.Fly, "IceShell", new Vector3(0f, -0.16f, 0f),
                            ease: EaseKind.In),
                        Move(1.50f, 0.22f, StepKind.Show, "MeltPuddle", Vector3.zero),
                        Sfx(1.52f, "splash"),
                        Haptic(1.54f, "light"),
                        Face(1.58f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.58f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.72f, 0.72f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.38f, 0f, -0.28f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Meet(2.54f, 0.70f),
                        Sfx(2.60f, "reunion"),
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
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.12f, 0.65f, 1.74f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Rotate(0.64f, 0.38f, SceneRef.Self, new Vector3(0f, 0f, -44f)),
                        Sfx(0.68f, "splash"),
                        Resize(0.80f, 0.85f, "Plant", 2.40f, EaseKind.Back),
                        Move(0.80f, 0.85f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, 0.24f, 0f), ease: EaseKind.Back),
                        Haptic(0.90f, "light"),
                        Face(1.02f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.02f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.72f, 0.80f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.40f, -0.13f, 0.42f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(2.56f, 0.72f),
                        Sfx(2.62f, "reunion"),
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

            Author.Stage(rescue, "r09", "shelter", "Stay out of rain.", Difficulty.Medium,
                ReasoningKind.Shelter, "Diorama_Weather_Downpour",
                "A low grey cloud parks over one Pep and rains on them while their partner watches from " +
                "under a dry awning.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_1",
                    Label = "The orange umbrella",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.78f, 0.63f, 1.74f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Sfx(0.64f, "pop"),
                        Move(0.64f, 0.36f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 170f),
                        Resize(0.64f, 0.36f, SceneRef.Self, 1.18f, EaseKind.Back),
                        Move(0.96f, 0.30f, StepKind.Hide, "Rain", Vector3.zero),
                        Move(1.02f, 0.80f, StepKind.FlyOff, "Cloud",
                            new Vector3(0.70f, 0.34f, 0.30f), ease: EaseKind.In),
                        Haptic(1.06f, "light"),
                        Face(1.12f, SceneRef.PepB, PepFace.Hopeful),
                        Face(1.12f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.42f, 0.78f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.28f, 0f, -0.44f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.38f, 0.72f),
                        Sfx(2.44f, "reunion"),
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
