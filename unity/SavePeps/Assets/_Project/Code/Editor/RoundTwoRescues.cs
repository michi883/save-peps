using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 2 — Clockwork courtyard.** *World rule: nothing moves until a
    /// linkage moves it. You never act on a Pep; you act on a machine that
    /// acts on the world.*
    ///
    /// Every solution here is put *into* a mechanism — mass into a tray, a cog
    /// onto a shaft, a mirror onto a pedestal — and the result arrives second
    /// hand, through gearing. That indirection is the round's grammar and it
    /// belongs to no other world: the canyon and the peak answer immediately,
    /// the foundry's machines are already running whether you help or not.
    ///
    /// Only-here rescue: **r05**, fitting the missing cog. It is the one
    /// puzzle in the game whose answer is a spare part, and it can only exist
    /// somewhere made of gears.
    /// </summary>
    public static class RoundTwoRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r04 = BuildHoist(overwrite, log);
            var r05 = BuildMesh(overwrite, log);
            var r06 = BuildReflect(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_02.asset", overwrite, log, out var round))
            {
                round.Number = 2;
                round.Rescues = new[] { r04, r05, r06 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildHoist(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r04_hoist.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r04", "hoist", "Raise the platform.", Difficulty.Easy,
                ReasoningKind.Counterweight, "Diorama_Clock_Pulley",
                "One Pep waits on a high wooden deck; the other stands on a lift platform in the pit, " +
                "roped over a pulley to an empty tray.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "Fluff is not mass.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.96f, 0.63f, 1.56f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Sfx(0.66f, "poof"),
                        Move(0.68f, 0.60f, StepKind.Fly, "Counterweight", new Vector3(0f, -0.025f, 0f)),
                        Move(0.70f, 0.60f, StepKind.Shake, "Pulley", Vector3.zero,
                            amplitude: 3f, ease: EaseKind.InOut),
                        Face(0.86f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_2", Label = "The grey stone",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.08f, 0.63f, 1.56f), amplitude: 0.50f, ease: EaseKind.Hop),
                        Sfx(0.62f, "thud"),
                        Haptic(0.64f, "medium"),
                        Move(0.66f, 0.70f, StepKind.Fly, "Counterweight",
                            new Vector3(0f, -0.22f, 0f), ease: EaseKind.InOut),
                        Move(0.66f, 0.70f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, -0.22f, 0f), ease: EaseKind.InOut),
                        Move(0.66f, 0.70f, StepKind.Spin, "Pulley", Vector3.zero, amplitude: 180f,
                            ease: EaseKind.InOut),
                        Sfx(0.70f, "ratchet"),
                        Move(0.66f, 0.70f, StepKind.Fly, "LiftPlatform",
                            new Vector3(0f, 0.16f, 0f), ease: EaseKind.InOut),
                        Move(0.66f, 0.70f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, 0.16f, 0f), ease: EaseKind.InOut),
                        Face(0.80f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.80f, SceneRef.PepB, PepFace.Hopeful),
                        Sfx(1.36f, "click"),
                        Move(1.42f, 0.78f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.40f, -0.02f, -0.53f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.32f, 0.72f),
                        Sfx(2.38f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Author.Prop("balloon"), AnchorId = "Slot_3",
                    Label = "The orange balloon",
                    Quip = "Now it tips farther.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.52f, 0.63f, 1.28f), amplitude: 0.45f, ease: EaseKind.Hop),
                        Sfx(0.60f, "boing"),
                        Move(0.64f, 0.72f, StepKind.Fly, "Counterweight",
                            new Vector3(0f, 0.20f, 0f), ease: EaseKind.InOut),
                        Move(0.64f, 0.72f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0.20f, 0f), ease: EaseKind.InOut),
                        Move(0.64f, 0.72f, StepKind.Fly, "LiftPlatform",
                            new Vector3(0f, -0.14f, 0f), ease: EaseKind.InOut),
                        Move(0.64f, 0.72f, StepKind.Fly, SceneRef.PepB,
                            new Vector3(0f, -0.14f, 0f), ease: EaseKind.InOut),
                        Face(0.78f, SceneRef.PepB, PepFace.Panic),
                        Face(0.90f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildMesh(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r05_mesh.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r05", "mesh", "Turn the gears.", Difficulty.Medium,
                ReasoningKind.Activation, "Diorama_Clock_Gearwall",
                "A wall of brass gearing stands dead with one bare shaft between two cogs, and a " +
                "portcullis holds the far Pep behind it.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "gear", Prop = Author.Prop("gear"), AnchorId = "Slot_1", Label = "The brass cog",
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.68f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.56f, 0.51f, 2.20f), amplitude: 0.62f, ease: EaseKind.Hop),
                        Sfx(0.66f, "clank"),
                        Haptic(0.68f, "medium"),
                        Move(0.72f, 0.90f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: -260f,
                            ease: EaseKind.Out),
                        Move(0.72f, 0.90f, StepKind.Spin, "GearTrain", Vector3.zero, amplitude: 240f,
                            ease: EaseKind.Out),
                        Sfx(0.78f, "ratchet"),
                        Move(0.74f, 0.86f, StepKind.Spin, "Governor", Vector3.zero, amplitude: 620f,
                            ease: EaseKind.Out),
                        Move(0.74f, 0.86f, StepKind.Fly, "Governor", new Vector3(0f, 0.05f, 0f)),
                        Move(1.10f, 0.72f, StepKind.Fly, "Portcullis",
                            new Vector3(0f, 0.62f, 0f), ease: EaseKind.InOut),
                        Face(1.16f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.16f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.60f, 0.78f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.36f, 0f, -1.02f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Meet(2.48f, 0.72f),
                        Sfx(2.54f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_2", Label = "The coil of rope",
                    Quip = "It only made a knot.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.32f, 0.51f, 2.20f), amplitude: 0.60f, ease: EaseKind.Hop),
                        Sfx(0.68f, "clunk"),
                        Move(0.70f, 0.62f, StepKind.Shake, "GearTrain", Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        Resize(0.72f, 0.55f, SceneRef.Self, 0.72f, EaseKind.Back),
                        Face(0.90f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "wrench", Prop = Author.Prop("wrench"), AnchorId = "Slot_3", Label = "The steel spanner",
                    Quip = "Wrong size. Wrong hole.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.64f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.12f, 0.51f, 1.92f), amplitude: 0.55f, ease: EaseKind.Hop),
                        Sfx(0.66f, "clank"),
                        Move(0.66f, 0.50f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 14f, ease: EaseKind.InOut),
                        Move(1.18f, 0.70f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.06f, -0.48f, -0.30f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Sfx(1.84f, "clunk"),
                        Face(1.20f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildReflect(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r06_reflect.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r06", "reflect", "Bounce the beam.", Difficulty.Medium,
                ReasoningKind.Reflection, "Diorama_Clock_Optics",
                "A lamp beam ends on an empty pedestal while a dark sensor keeps the iris gate shut.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "magnet", Prop = Author.Prop("magnet"), AnchorId = "Slot_1", Label = "The horseshoe magnet",
                    Quip = "Brass is not magnetic.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.96f, 0.30f, 1.56f), amplitude: 0.48f, ease: EaseKind.Hop),
                        Move(0.68f, 0.85f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 11f, ease: EaseKind.InOut),
                        Sfx(0.72f, "clunk"),
                        Face(0.92f, SceneRef.PepA, PepFace.Worried),
                        Face(1.30f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Author.Prop("umbrella"), AnchorId = "Slot_2",
                    Label = "The orange umbrella",
                    Quip = "Nice shade. No light.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "pop"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.42f, 0.42f, 1.30f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Move(0.60f, 0.34f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 150f),
                        Move(0.94f, 0.30f, StepKind.Hide, "BeamIn", Vector3.zero),
                        Face(1.06f, SceneRef.PepA, PepFace.Worried),
                        Face(1.06f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "mirror", Prop = Author.Prop("mirror"), AnchorId = "Slot_3", Label = "The hand mirror",
                    Duration = 3.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.64f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.02f, 0.13f, 0.93f), amplitude: 0.38f, ease: EaseKind.Hop),
                        Rotate(0.60f, 0.34f, SceneRef.Self, new Vector3(0f, 44f, 0f)),
                        Sfx(0.66f, "click"),
                        Move(0.86f, 0.20f, StepKind.Show, "BeamBounce", Vector3.zero),
                        Move(0.98f, 0.22f, StepKind.Show, "SensorGlow", Vector3.zero),
                        Sfx(1.00f, "chime"),
                        Haptic(1.02f, "light"),
                        Move(1.10f, 0.60f, StepKind.Spin, "IrisGate", Vector3.zero, amplitude: 92f,
                            ease: EaseKind.InOut),
                        Resize(1.10f, 0.60f, "IrisGate", 0.16f, EaseKind.InOut),
                        Face(1.18f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.18f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.52f, 0.78f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.42f, 0f, -1.00f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.40f, 0.72f),
                        Sfx(2.46f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
