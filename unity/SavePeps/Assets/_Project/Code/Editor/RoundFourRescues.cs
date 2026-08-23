using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 4 — Windrock canyon.** *World rule: the gap is vertical as well
    /// as horizontal, the far rim is higher than the near one, and the air is
    /// going somewhere.*
    ///
    /// The round the old catalogue got most wrong: it was called Canyon and
    /// contained one canyon rescue plus two borrowed garden scenes. All three
    /// now stand on the two mesas, and all three are about mass and moving air
    /// rather than about crossing — nothing here is solved by laying something
    /// flat across the hole, and the plank is not even offered.
    ///
    /// Only-here rescue: **r12**, pulling the rock spire over to make a
    /// bridge out of the landscape itself. It needs a chasm with something
    /// standing in it, which exists in exactly one world.
    /// </summary>
    public static class RoundFourRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r10 = BuildGlide(overwrite, log);
            var r11 = BuildPlumb(overwrite, log);
            var r12 = BuildTopple(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_04.asset", overwrite, log, out var round))
            {
                round.Number = 4;
                round.Rescues = new[] { r10, r11, r12 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildGlide(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r10_glide.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r10", "glide", "Cross the chasm.", Difficulty.Medium,
                ReasoningKind.Airflow, "Diorama_Canyon_Updraft",
                "Warm air rises visibly out of the chasm between two red mesas; the far rim stands " +
                "higher than the near one.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_1", Label = "The caged electric fan",
                    Quip = "Blew the dust about.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Move(0.20f, 0.90f, StepKind.Shake, "Thermal", Vector3.zero,
                            amplitude: 9f, ease: EaseKind.InOut),
                        Face(0.40f, SceneRef.PepA, PepFace.Panic),
                        Move(0.46f, 0.70f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0f, -0.24f)),
                        Move(1.30f, 0.60f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0f, 0.06f),
                            ease: EaseKind.InOut),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_2",
                    Label = "The orange umbrella",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.42f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.56f, 0.35f, 0.78f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Sfx(0.42f, "pop"),
                        Move(0.42f, 0.24f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 160f),
                        Resize(0.42f, 0.24f, SceneRef.Self, 1.12f, EaseKind.Back),
                        // Only the compact thermal pocket answers. Nothing on
                        // either rim changes: this is the local baseline.
                        Resize(0.58f, 0.34f, "Thermal", 1.22f, EaseKind.Back),
                        Move(0.58f, 0.34f, StepKind.Shake, "Thermal", Vector3.zero,
                            amplitude: 2.5f, ease: EaseKind.InOut),
                        Sfx(0.66f, "wind"),
                        Move(0.66f, 0.78f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0.20f, 0.24f, 1.40f), amplitude: 0.46f, ease: EaseKind.InOut),
                        Move(0.66f, 0.78f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.20f, 0.24f, 1.40f), amplitude: 0.46f, ease: EaseKind.InOut),
                        Face(0.82f, SceneRef.PepA, PepFace.Happy),
                        Face(0.82f, SceneRef.PepB, PepFace.Hopeful),
                        Haptic(1.42f, "light"),
                        Meet(1.66f, 0.68f),
                        Sfx(1.72f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_3", Label = "The grey stone",
                    Quip = "Straight down. Quickly.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.56f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.16f, 1.02f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Face(0.60f, SceneRef.PepA, PepFace.Worried),
                        Move(0.62f, 0.90f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0f, -1.70f, 0.06f), ease: EaseKind.In),
                        Sfx(1.66f, "rumble"),
                        Face(1.72f, SceneRef.PepB, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildPlumb(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r11_plumb.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r11", "plumb", "Stabilise the cableway.", Difficulty.Medium,
                ReasoningKind.Counterweight, "Diorama_Canyon_Cablecar",
                "A diagonal cableway spans both height and chasm. Its cables sag, both towers flex and " +
                "the suspended car swings sideways in a broad crosswind.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "feather", Prop = Author.Prop("feather"), AnchorId = "Slot_1", Label = "The white feather",
                    Quip = "The wind took it.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "wind"),
                        Move(0f, 0.50f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.20f, 0.30f, 0.60f), amplitude: 0.30f, ease: EaseKind.Hop),
                        Face(0.56f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.60f, 1.10f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.90f, 0.95f, 0.30f), ease: EaseKind.In),
                        Face(1.60f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_2", Label = "The coil of rope",
                    Quip = "Now it swings harder.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.46f, 0.60f, 1.42f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Sfx(0.64f, "creak"),
                        Move(0.66f, 1.10f, StepKind.Shake, "Basket", Vector3.zero,
                            amplitude: 17f, ease: EaseKind.InOut),
                        Move(0.66f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 17f, ease: EaseKind.InOut),
                        Face(0.90f, SceneRef.PepA, PepFace.Panic),
                        Face(1.10f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "weight", Prop = Author.Prop("weight"), AnchorId = "Slot_3", Label = "The iron weight",
                    Duration = 3.45f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.50f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.76f, 0.18f, 0.54f), amplitude: 0.48f, ease: EaseKind.Hop),
                        Sfx(0.52f, "clank"),
                        Haptic(0.54f, "medium"),
                        Impact(0.54f, 0.45f),
                        // One mass changes an entire suspended system: cradle
                        // drops, both towers take load, slack cables pull taut,
                        // and the crosswind and car settle together.
                        Move(0.54f, 0.42f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, -0.22f, 0f), ease: EaseKind.In),
                        Move(0.54f, 0.42f, StepKind.Fly, "CounterweightRig",
                            new Vector3(0f, -0.16f, 0f), ease: EaseKind.In),
                        Rotate(0.56f, 0.38f, "NearTower", new Vector3(0f, 0f, -3.5f), EaseKind.InOut),
                        Rotate(0.60f, 0.38f, "FarTower", new Vector3(0f, 0f, 3.5f), EaseKind.InOut),
                        Move(0.62f, 0.34f, StepKind.Shake, "SlackCable", Vector3.zero,
                            amplitude: 4.5f, ease: EaseKind.InOut),
                        Ambient(0.56f, 0.62f, "BasketSwing", 0f),
                        Ambient(0.56f, 0.62f, "CableWind", 0.12f),
                        VisibilitySwap(1.00f, "SlackCable", "TautCable"),
                        VisibilitySwap(1.04f, "Basket", "SteadyCar"),
                        Sfx(1.04f, "ratchet"),
                        Move(1.04f, 0.24f, StepKind.Shake, "TautCable", Vector3.zero,
                            amplitude: 2.2f, ease: EaseKind.InOut),
                        Face(1.06f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.10f, 0.44f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.16f, 0.39f, 0.40f), amplitude: 0.22f, ease: EaseKind.Hop),
                        Face(1.32f, SceneRef.PepB, PepFace.Hopeful),
                        Sfx(1.56f, "slide"),
                        Move(1.56f, 0.98f, StepKind.Fly, "SteadyCar",
                            new Vector3(0.92f, 0.18f, 1.05f), ease: EaseKind.InOut),
                        Move(1.56f, 0.98f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.92f, 0.18f, 1.05f), ease: EaseKind.InOut),
                        Move(2.56f, 0.34f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(-0.02f, -0.33f, 0.12f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.94f, 0.46f),
                        Sfx(3.00f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildTopple(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r12_topple.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r12", "topple", "Reshape the canyon.", Difficulty.Surprising,
                ReasoningKind.Momentum, "Diorama_Canyon_Spire",
                "A leaning monolith and two tall hoodoos break the canyon skyline. Faults run from " +
                "both rims into the rock mass, with the Peps stranded at opposite diagonal corners.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "grapple", Prop = Author.Prop("grapple"), AnchorId = "Slot_1", Label = "The grappling hook",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.52f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.52f, 1.37f, 1.49f), amplitude: 0.78f, ease: EaseKind.Hop),
                        Move(0.46f, 0.16f, StepKind.Show, "GrappleLine", Vector3.zero),
                        Sfx(0.52f, "clank"),
                        Haptic(0.54f, "light"),
                        Impact(0.54f, 0.45f),
                        Move(0.54f, 0.22f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 8f, ease: EaseKind.InOut),
                        Sfx(0.68f, "creak"),
                        Face(0.70f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.68f, 0.24f, StepKind.Shake, "Spire", Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Move(0.72f, 0.18f, StepKind.Show, "FaultCracks", Vector3.zero),
                        Rotate(0.74f, 0.64f, "Spire", new Vector3(58f, 0f, 52f), EaseKind.In),
                        Rotate(0.82f, 0.44f, "RimCrownNear", new Vector3(0f, 0f, -24f), EaseKind.In),
                        Rotate(0.88f, 0.42f, "RimCrownFar", new Vector3(0f, 0f, 22f), EaseKind.In),
                        Move(0.92f, 0.52f, StepKind.FlyOff, "RockfallNear",
                            new Vector3(0.28f, -1.34f, 0.34f), ease: EaseKind.In),
                        Move(0.98f, 0.48f, StepKind.FlyOff, "RockfallFar",
                            new Vector3(-0.32f, -1.52f, -0.20f), ease: EaseKind.In),
                        Sfx(1.12f, "crunch"),
                        Sfx(1.36f, "rumble"),
                        Haptic(1.36f, "heavy"),
                        Impact(1.36f, 1.45f),
                        VisibilitySwap(1.36f, "Spire", "FallenSpan"),
                        VisibilitySwap(1.38f, "RimCrownNear", "AfterRimNear"),
                        VisibilitySwap(1.40f, "RimCrownFar", "AfterRimFar"),
                        Move(1.36f, 0.08f, StepKind.Show, "SpireDust", Vector3.zero),
                        Resize(1.36f, 0.66f, "SpireDust", 2.15f, EaseKind.Out),
                        Move(1.38f, 0.30f, StepKind.Shake, "FallenSpan", Vector3.zero,
                            amplitude: 5f, ease: EaseKind.InOut),
                        Move(1.42f, 0.18f, StepKind.Show, "RockStepNear", Vector3.zero),
                        Move(1.50f, 0.18f, StepKind.Show, "RockStepMid", Vector3.zero),
                        Move(1.58f, 0.18f, StepKind.Show, "RockStepFar", Vector3.zero),
                        Face(1.44f, SceneRef.PepB, PepFace.Happy),
                        Move(1.72f, 0.35f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.25f, 0.04f, 0.38f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Move(2.05f, 0.34f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.24f, 0.06f, 0.37f), amplitude: 0.19f, ease: EaseKind.Hop),
                        Move(2.36f, 0.34f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.23f, 0.07f, 0.36f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Move(2.66f, 0.32f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.20f, 0.07f, 0.31f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Move(2.10f, 0.18f, StepKind.Hide, "SpireDust", Vector3.zero),
                        Meet(3.00f, 0.55f),
                        Sfx(3.04f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Author.Prop("scissors"), AnchorId = "Slot_2",
                    Label = "The purple-handled scissors",
                    Quip = "Rock. Meet scissors.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.44f, 0.42f, 1.08f), amplitude: 0.50f, ease: EaseKind.Hop),
                        Sfx(0.64f, "clunk"),
                        Move(0.64f, 0.66f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 15f, ease: EaseKind.InOut),
                        Move(1.34f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.12f, -0.30f, -0.34f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Face(0.92f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Author.Prop("fan"), AnchorId = "Slot_3", Label = "The caged electric fan",
                    Quip = "The mountain was fine.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0f, 1.10f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Move(0.22f, 0.90f, StepKind.Shake, "Spire", Vector3.zero,
                            amplitude: 0.6f, ease: EaseKind.InOut),
                        Move(0.30f, 0.30f, StepKind.Show, "SpireDust", Vector3.zero),
                        Resize(0.32f, 0.80f, "SpireDust", 1.60f, EaseKind.Out),
                        Move(1.30f, 0.40f, StepKind.Hide, "SpireDust", Vector3.zero),
                        Face(0.90f, SceneRef.PepB, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
