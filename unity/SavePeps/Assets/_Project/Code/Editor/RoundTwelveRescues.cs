using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 12 — Neon skyline.** *World rule: the city moves for you if you
    /// catch it at the right moment. Everything is fast, lit, and a long way
    /// up.*
    ///
    /// Three rooftops at three heights, a transit beam crossing the frame, and
    /// the most vertical composition in the game. The last round is also the
    /// only one whose final rescue answers a question the first round asked:
    /// the balloon that floated uselessly away from the brook in `r01` — "Up is
    /// not across" — is the answer here, because in a city it is exactly both.
    ///
    /// Only-here rescue: **r35**, hooking a moving tram. It is the only rescue
    /// whose obstacle is *timing*, and the only one where the wrong answers
    /// fail for being too slow rather than too weak.
    /// </summary>
    public static class RoundTwelveRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r34 = BuildPower(overwrite, log);
            var r35 = BuildBoard(overwrite, log);
            var r36 = BuildSoar(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_12.asset", overwrite, log, out var round))
            {
                round.Number = 12;
                round.Rescues = new[] { r34, r35, r36 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildPower(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r34_power.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r34", "power", "Light the sign.", Difficulty.Medium,
                ReasoningKind.Signal, "Diorama_Neon_Sign",
                "A dead sign hangs over an alley so dark it might be a hole, with one empty socket in " +
                "its frame.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "neon_tube", Prop = Author.Prop("neon_tube"), AnchorId = "Slot_1",
                    Label = "The neon tube",
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.72f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.86f, 0.955f, 1.61f), amplitude: 0.82f, ease: EaseKind.Hop),
                        Sfx(0.74f, "click"),
                        Move(0.76f, 0.34f, StepKind.Shake, "Socket", Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        Sfx(0.94f, "neon"),
                        Haptic(0.96f, "medium"),
                        Move(0.98f, 0.36f, StepKind.Show, "SignGlow", Vector3.zero),
                        Move(1.00f, 0.34f, StepKind.Shake, "SignFrame", Vector3.zero,
                            amplitude: 2f, ease: EaseKind.InOut),
                        Move(1.16f, 0.40f, StepKind.Hide, "AlleyDark", Vector3.zero),
                        Move(1.30f, 0.36f, StepKind.Show, "FireEscape", Vector3.zero),
                        Sfx(1.34f, "chime"),
                        Face(1.42f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.42f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.56f, 1.04f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.38f, 0.62f, 1.22f), amplitude: 0.20f, ease: EaseKind.Hop),
                        Meet(2.68f, 0.74f),
                        Sfx(2.74f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Author.Prop("scissors"), AnchorId = "Slot_2",
                    Label = "The purple-handled scissors",
                    Quip = "Darker than before.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.14f, 0.905f, 1.61f), amplitude: 0.76f, ease: EaseKind.Hop),
                        Sfx(0.70f, "snip"),
                        Sfx(0.74f, "zap"),
                        Move(0.74f, 0.40f, StepKind.Shake, "SignFrame", Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Resize(0.78f, 0.70f, "AlleyDark", 1.22f, EaseKind.Out),
                        Face(0.94f, SceneRef.PepA, PepFace.Panic),
                        Face(1.06f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Author.Prop("hair_dryer"), AnchorId = "Slot_3",
                    Label = "The warm hair dryer",
                    Quip = "One spark. Then nothing.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.905f, 1.35f), amplitude: 0.74f, ease: EaseKind.Hop),
                        Sfx(0.72f, "zap"),
                        Move(0.72f, 0.22f, StepKind.Show, "SignGlow", Vector3.zero),
                        Move(0.96f, 0.30f, StepKind.Hide, "SignGlow", Vector3.zero),
                        Move(0.98f, 0.50f, StepKind.Shake, "SignFrame", Vector3.zero,
                            amplitude: 3f, ease: EaseKind.InOut),
                        Face(1.06f, SceneRef.PepB, PepFace.Worried),
                        Face(1.60f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildBoard(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r35_board.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r35", "board", "Catch the tram.", Difficulty.Surprising,
                ReasoningKind.Momentum, "Diorama_Neon_Transit",
                "A tram runs past the roof edge on a loop, far too fast to step onto, and the other " +
                "Pep is stranded on the tower it passes.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_1",
                    Label = "The orange balloon",
                    Quip = "Too slow. Far too slow.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.14f, 0.255f, 0.42f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(0.62f, "boing"),
                        Move(0.66f, 0.80f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0.30f, 0.10f),
                            ease: EaseKind.Out),
                        Move(0.66f, 0.80f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.30f, 0.10f),
                            ease: EaseKind.Out),
                        Sfx(1.10f, "transit"),
                        Face(1.16f, SceneRef.PepA, PepFace.Panic),
                        Move(1.48f, 0.70f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, -0.30f, -0.10f),
                            ease: EaseKind.In),
                        Move(1.48f, 0.70f, StepKind.Fly, SceneRef.Self, new Vector3(0f, -0.30f, -0.10f),
                            ease: EaseKind.In),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Quip = "The tram did not care.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.34f, 0.66f, 1.72f), amplitude: 0.62f, ease: EaseKind.Hop),
                        Sfx(0.62f, "transit"),
                        Move(0.64f, 0.80f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(-0.86f, -0.30f, -0.40f), ease: EaseKind.Out),
                        Face(0.78f, SceneRef.PepA, PepFace.Worried),
                        Face(1.60f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "zip_grip", Prop = Author.Prop("zip_grip"), AnchorId = "Slot_3",
                    Label = "The trolley grip",
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.50f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.28f, 0.255f, 0.14f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Face(0.56f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.62f, 0.62f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0.10f, 0.66f, 1.10f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Move(0.62f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.10f, 0.66f, 1.10f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Sfx(1.24f, "transit"),
                        Haptic(1.26f, "medium"),
                        Move(1.26f, 0.20f, StepKind.Show, "RailSpark", Vector3.zero),
                        Move(1.48f, 0.30f, StepKind.Hide, "RailSpark", Vector3.zero),
                        // Fast and eased in: the whole point is that the city
                        // is doing the work and it is not slowing down for you.
                        Move(1.28f, 0.56f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.40f, -0.04f, 0.20f), ease: EaseKind.In),
                        Move(1.28f, 0.56f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0.40f, -0.04f, 0.20f), ease: EaseKind.In),
                        Face(1.32f, SceneRef.PepA, PepFace.Happy),
                        Move(1.88f, 0.26f, StepKind.Hide, SceneRef.Self, Vector3.zero),
                        Meet(2.60f, 0.72f),
                        Sfx(2.66f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSoar(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r36_soar.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r36", "soar", "Fly the skyline.", Difficulty.Surprising,
                ReasoningKind.Crossing, "Diorama_Neon_Skyline",
                "The last gap: a whole lit city block between this rooftop and the antenna tower where " +
                "the other Pep is waiting.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "plank", Prop = Author.Prop("plank"), AnchorId = "Slot_1", Label = "The wooden plank",
                    Quip = "Short by a whole city.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.60f, 0.66f, 1.70f), amplitude: 0.68f, ease: EaseKind.Hop),
                        Sfx(0.68f, "thud"),
                        Face(0.76f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.30f, SceneRef.PepA, PepFace.Worried),
                        Move(1.34f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.10f, -1.40f, 0.40f), ease: EaseKind.In),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_2",
                    Label = "The orange balloon",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.14f, 0.905f, 1.74f), amplitude: 0.72f, ease: EaseKind.Hop),
                        Sfx(0.66f, "boing"),
                        Face(0.72f, SceneRef.PepA, PepFace.Happy),
                        Haptic(0.76f, "light"),
                        // The whole game's longest single movement, and the
                        // answer to round one's "Up is not across". Here it is
                        // both, which is the only reason the city is the last
                        // world.
                        Move(0.80f, 1.40f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(-0.62f, 0.655f, 0.96f), amplitude: 0.95f, ease: EaseKind.InOut),
                        Move(0.80f, 1.40f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.62f, 0.655f, 0.96f), amplitude: 0.95f, ease: EaseKind.InOut),
                        Move(1.20f, 0.30f, StepKind.Show, "Fireworks", Vector3.zero),
                        Sfx(1.24f, "chime"),
                        Resize(1.24f, 1.00f, "Fireworks", 2.20f, EaseKind.Out),
                        Move(2.26f, 0.44f, StepKind.Hide, "Fireworks", Vector3.zero),
                        Face(2.20f, SceneRef.PepB, PepFace.Love),
                        Meet(2.84f, 0.72f),
                        Sfx(2.90f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_3", Label = "The coil of rope",
                    Quip = "Tied to nothing at all.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.28f, 0.86f, 1.44f), amplitude: 0.66f, ease: EaseKind.Hop),
                        Sfx(0.66f, "creak"),
                        Move(0.68f, 0.90f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 12f, ease: EaseKind.InOut),
                        Face(0.86f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.62f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
