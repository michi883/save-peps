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
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.58f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.34f, 0.07f, 0.72f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Sfx(0.60f, "thud"),
                        Haptic(0.62f, "medium"),
                        // The tarp's flap and the taut tarp are two objects.
                        // Choreography can add motion to an idle but never
                        // remove one, so going still is a swap.
                        Move(0.80f, 0.14f, StepKind.Hide, "Tarp", Vector3.zero),
                        Move(0.80f, 0.14f, StepKind.Hide, "TarpCorner", Vector3.zero),
                        Move(0.80f, 0.16f, StepKind.Show, "TautTarp", Vector3.zero),
                        Move(0.80f, 0.16f, StepKind.Show, "PinnedCorner", Vector3.zero),
                        Sfx(0.96f, "creak"),
                        Face(1.02f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.02f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.16f, 1.02f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.04f, 0f, 1.58f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(2.36f, 0.72f),
                        Sfx(2.42f, "reunion"),
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
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.64f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.88f, 0.02f, 1.24f), amplitude: 0.46f, ease: EaseKind.Hop),
                        Sfx(0.66f, "clank"),
                        Move(0.90f, 0.18f, StepKind.Show, "Arc", Vector3.zero),
                        Move(0.94f, 0.16f, StepKind.Show, "Strike", Vector3.zero),
                        Sfx(0.94f, "zap"),
                        Haptic(0.96f, "medium"),
                        Move(1.12f, 0.26f, StepKind.Hide, "Strike", Vector3.zero),
                        Move(1.20f, 0.30f, StepKind.Hide, "Arc", Vector3.zero),
                        Move(1.24f, 0.60f, StepKind.Shake, "Mast", Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Move(1.30f, 0.24f, StepKind.Hide, "Scorch", Vector3.zero),
                        Face(1.36f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.36f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.52f, 0.86f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.22f, 0f, 0.94f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Move(1.52f, 0.86f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.24f, 0f, -0.78f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.62f, 0.74f),
                        Sfx(2.68f, "reunion"),
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
                "A steep empty gutter runs from this roof down to a lower annex where the other Pep " +
                "shelters.");

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
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.36f, 0.07f, 2.12f), amplitude: 0.60f, ease: EaseKind.Hop),
                        Rotate(0.60f, 0.34f, SceneRef.Self, new Vector3(28f, 0f, 0f)),
                        Sfx(0.68f, "thud"),
                        Face(0.80f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.90f, 0.60f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.32f, 0.05f, 1.22f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Sfx(1.52f, "glide_hiss"),
                        Move(1.52f, 0.62f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.06f, -0.13f, 1.10f), ease: EaseKind.In),
                        Move(2.10f, 0.20f, StepKind.Show, "Spray", Vector3.zero),
                        Sfx(2.12f, "splash"),
                        Haptic(2.14f, "light"),
                        Move(2.34f, 0.34f, StepKind.Hide, "Spray", Vector3.zero),
                        Meet(2.70f, 0.74f),
                        Sfx(2.76f, "reunion"),
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
