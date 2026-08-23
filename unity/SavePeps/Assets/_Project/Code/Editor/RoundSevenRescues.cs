using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// **Round 7 — Crystal cave.** *World rule: you cannot see and you cannot
    /// reach. Make light, make the right sound, or move the rock — and the
    /// cave answers back.*
    ///
    /// This was the worst round in the old catalogue: three rescues borrowed
    /// wholesale from rounds two and three, one of them a note-for-note repeat
    /// of "melt the ice". It is now the only enclosed world in the game — rock
    /// walls, a ceiling of stalactites, a near-black sky, dense fog and a
    /// lantern-warm key light — and none of its three answers appears anywhere
    /// else in the catalogue.
    ///
    /// Only-here rescue: **r20**, striking the tuned crystal so the ceiling
    /// vein rings and the rock curtain shivers apart. It is the game's only
    /// puzzle about *pitch*, and the bell — right in round one — is the near
    /// miss.
    /// </summary>
    public static class RoundSevenRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r19 = BuildKindle(overwrite, log);
            var r20 = BuildRing(overwrite, log);
            var r21 = BuildHew(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_07.asset", overwrite, log, out var round))
            {
                round.Number = 7;
                round.Rescues = new[] { r19, r20, r21 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildKindle(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r19_kindle.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r19", "kindle", "Light the cave.", Difficulty.Medium,
                ReasoningKind.Reflection, "Diorama_Cave_Dark",
                "One small black pocket in the floor lies between two nearby Peps, with an empty lamp " +
                "hook beside it.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "mirror", Prop = Author.Prop("mirror"), AnchorId = "Slot_1", Label = "The hand mirror",
                    Quip = "No light to bounce.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.26f, 0.26f, 0.84f), amplitude: 0.38f, ease: EaseKind.Hop),
                        Move(0.66f, 0.90f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 300f,
                            ease: EaseKind.InOut),
                        Sfx(0.70f, "drip"),
                        Face(0.94f, SceneRef.PepA, PepFace.Worried),
                        Face(1.30f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "lantern", Prop = Author.Prop("lantern"), AnchorId = "Slot_2", Label = "The cage lantern",
                    Duration = 2.8f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.54f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.34f, 0.27f, 1.36f), amplitude: 0.38f, ease: EaseKind.Hop),
                        Sfx(0.56f, "click"),
                        Move(0.56f, 0.30f, StepKind.Shake, "LampHook", Vector3.zero,
                            amplitude: 3f, ease: EaseKind.InOut),
                        // LOCAL EVENT: one compact patch changes atomically.
                        VisibilitySwap(0.72f, "Darkness", "LitPool"),
                        Sfx(0.76f, "crystal"),
                        Haptic(0.78f, "light"),
                        Face(0.88f, SceneRef.PepA, PepFace.Hopeful),
                        Face(0.88f, SceneRef.PepB, PepFace.Hopeful),
                        Sfx(1.02f, "splash"),
                        Move(1.00f, 0.66f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.30f, 0f, 0.58f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(1.82f, 0.58f),
                        Sfx(1.88f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "pickaxe", Prop = Author.Prop("pickaxe"), AnchorId = "Slot_3", Label = "The miner's pick",
                    Quip = "Swing. Miss. Swing.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.28f, 0.20f, 1.24f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Move(0.62f, 0.42f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 26f, ease: EaseKind.InOut),
                        Sfx(0.86f, "clunk"),
                        Move(1.06f, 0.42f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 26f, ease: EaseKind.InOut),
                        Sfx(1.30f, "clunk"),
                        Face(1.34f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildRing(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r20_ring.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r20", "ring", "Ring the crystal.", Difficulty.Surprising,
                ReasoningKind.Resonance, "Diorama_Cave_Vein",
                "A three-part crystal seam runs the length of the cave roof, above a broad rock shutter " +
                "that has folded the only route shut.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Author.Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "The cave said nothing.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.33f, 0.86f, 1.28f), amplitude: 0.66f, ease: EaseKind.Hop),
                        Sfx(0.68f, "poof"),
                        Move(0.70f, 0.60f, StepKind.Shake, "CrystalVein", Vector3.zero,
                            amplitude: 0.6f, ease: EaseKind.InOut),
                        Move(1.32f, 0.80f, StepKind.Drop, SceneRef.Self, new Vector3(0f, -0.80f, 0f)),
                        Face(0.96f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "bell", Prop = Author.Prop("bell"), AnchorId = "Slot_2", Label = "The brass bell",
                    Quip = "Wrong note. Just dust.",
                    Duration = 2.6f,
                    Steps = new[]
                    {
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.28f, 0.51f, 1.23f), amplitude: 0.50f, ease: EaseKind.Hop),
                        Sfx(0.62f, "bell"),
                        Move(0.62f, 0.60f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 18f, ease: EaseKind.InOut),
                        Sfx(0.98f, "clunk"),
                        Move(1.00f, 0.24f, StepKind.Show, "Dust", Vector3.zero),
                        Resize(1.02f, 0.60f, "Dust", 1.30f, EaseKind.Out),
                        Move(1.70f, 0.44f, StepKind.Hide, "Dust", Vector3.zero),
                        Face(1.06f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "chime_crystal", Prop = Author.Prop("chime_crystal"), AnchorId = "Slot_3",
                    Label = "The tuned crystal",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.56f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.78f, 1.48f), amplitude: 0.58f, ease: EaseKind.Hop),
                        Sfx(0.58f, "crystal"),
                        Haptic(0.60f, "medium"),
                        Move(0.60f, 0.78f, StepKind.Shake, "CrystalVein", Vector3.zero,
                            amplitude: 3.4f, ease: EaseKind.InOut),

                        // SYSTEM EVENT: the note travels through three roof
                        // sections before the landscape can unfold.
                        Move(0.66f, 0.10f, StepKind.Show, "VeinPulseNear", Vector3.zero),
                        Resize(0.66f, 0.30f, "VeinPulseNear", 1.16f, EaseKind.Out),
                        Sfx(0.68f, "chime"),
                        Move(0.86f, 0.10f, StepKind.Show, "VeinPulseMid", Vector3.zero),
                        Resize(0.86f, 0.30f, "VeinPulseMid", 1.18f, EaseKind.Out),
                        Sfx(0.88f, "chime"),
                        Move(1.06f, 0.10f, StepKind.Show, "VeinPulseFar", Vector3.zero),
                        Resize(1.06f, 0.30f, "VeinPulseFar", 1.20f, EaseKind.Out),
                        Sfx(1.08f, "crystal"),
                        Ambient(0.82f, 0.60f, "CaveDrips", 0.34f),
                        Atmosphere(0.92f, 0.70f, "resonant"),
                        Move(1.14f, 0.38f, StepKind.Shake, "SealedPassage", Vector3.zero,
                            amplitude: 7f, ease: EaseKind.InOut),
                        Move(1.24f, 0.12f, StepKind.Show, "Dust", Vector3.zero),
                        Move(1.24f, 0.46f, StepKind.FlyOff, "Dust",
                            new Vector3(0f, 0.34f, -0.12f), ease: EaseKind.In),
                        Sfx(1.26f, "rumble"),
                        VisibilitySwap(1.50f, "SealedPassage", "OpenPassage"),
                        Move(1.52f, 0.12f, StepKind.Show, "RouteLights", Vector3.zero),
                        Resize(1.52f, 0.30f, "RouteLights", 1.18f, EaseKind.Back),
                        Impact(1.52f, 0.68f),
                        Haptic(1.54f, "medium"),
                        Move(1.58f, 0.12f, StepKind.Hide, "VeinPulseNear", Vector3.zero),
                        Move(1.62f, 0.12f, StepKind.Hide, "VeinPulseMid", Vector3.zero),
                        Move(1.66f, 0.12f, StepKind.Hide, "VeinPulseFar", Vector3.zero),
                        Face(1.58f, SceneRef.PepA, PepFace.Happy),
                        Face(1.58f, SceneRef.PepB, PepFace.Hopeful),

                        // Four clear steps climb the newly created route.
                        Move(1.68f, 0.32f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.28f, 0.04f, 0.44f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Move(1.98f, 0.32f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.24f, 0.07f, 0.44f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Move(2.28f, 0.32f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.20f, 0.10f, 0.42f), amplitude: 0.17f, ease: EaseKind.Hop),
                        Move(2.58f, 0.30f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(-0.02f, 0.11f, 0.38f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(2.96f, 0.54f),
                        Sfx(3.02f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildHew(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r21_hew.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Author.Stage(rescue, "r21", "hew", "Free the mine cart.", Difficulty.Medium,
                ReasoningKind.Momentum, "Diorama_Cave_Cart",
                "A loaded ore cart points at a resonant gate in a cavern sealed by two enormous rock " +
                "masses. One wooden wedge holds the whole chain still.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pickaxe", Prop = Author.Prop("pickaxe"), AnchorId = "Slot_1", Label = "The miner's pick",
                    Duration = 3.6f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.42f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.35f, 0.12f, 0.54f), amplitude: 0.34f, ease: EaseKind.Hop),
                        Move(0.42f, 0.22f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 24f, ease: EaseKind.InOut),
                        Sfx(0.48f, "chip"),
                        Haptic(0.50f, "medium"),
                        Move(0.50f, 0.34f, StepKind.FlyOff, "Chock",
                            new Vector3(-0.42f, 0.18f, -0.26f), ease: EaseKind.In),
                        Sfx(0.58f, "rumble"),
                        Move(0.52f, 0.30f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.48f, 0.18f, 0.42f), amplitude: 0.24f, ease: EaseKind.Hop),

                        // The Pep rides the released cart through three rail
                        // legs. The cart, not the pickaxe, strikes the cave's
                        // resonator and causes the world change.
                        Move(0.80f, 0.28f, StepKind.Fly, "MineCart",
                            new Vector3(0.12f, 0f, 0.42f), ease: EaseKind.In),
                        Move(0.80f, 0.28f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.12f, 0f, 0.42f), ease: EaseKind.In),
                        Move(0.80f, 0.72f, StepKind.Shake, "Rail", Vector3.zero,
                            amplitude: 1.6f, ease: EaseKind.InOut),
                        Move(1.06f, 0.28f, StepKind.Fly, "MineCart",
                            new Vector3(-0.20f, 0f, 0.40f), ease: EaseKind.InOut),
                        Move(1.06f, 0.28f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(-0.20f, 0f, 0.40f), ease: EaseKind.InOut),
                        Move(1.32f, 0.26f, StepKind.Fly, "MineCart",
                            new Vector3(0.12f, 0f, 0.34f), ease: EaseKind.In),
                        Move(1.32f, 0.26f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0.12f, 0f, 0.34f), ease: EaseKind.In),
                        Sfx(1.46f, "clank"),
                        Move(1.46f, 0.42f, StepKind.Spin, "TuningGate", Vector3.zero,
                            amplitude: 420f, ease: EaseKind.InOut),
                        Impact(1.48f, 0.72f),
                        Haptic(1.48f, "medium"),
                        Sfx(1.52f, "crystal"),

                        // WORLD EVENT: resonance splits both sealed halves,
                        // replaces the mine rail with a crystalline causeway,
                        // and reveals an animated geode cathedral.
                        Move(1.54f, 0.36f, StepKind.Shake, "SealLeft", Vector3.zero,
                            amplitude: 8f, ease: EaseKind.InOut),
                        Move(1.54f, 0.36f, StepKind.Shake, "SealRight", Vector3.zero,
                            amplitude: 8f, ease: EaseKind.InOut),
                        Move(1.58f, 0.32f, StepKind.Fly, "SealLeft",
                            new Vector3(-0.24f, 0.16f, 0f), ease: EaseKind.In),
                        Move(1.58f, 0.32f, StepKind.Fly, "SealRight",
                            new Vector3(0.24f, 0.16f, 0f), ease: EaseKind.In),
                        Sfx(1.64f, "rumble"),
                        VisibilitySwap(1.90f, "SealLeft", "GeodeLeft"),
                        VisibilitySwap(1.90f, "SealRight", "GeodeRight"),
                        VisibilitySwap(1.90f, "Rail", "CrystalRoute"),
                        Move(1.90f, 0.10f, StepKind.Show, "GeodeHeart", Vector3.zero),
                        Resize(1.90f, 0.36f, "GeodeHeart", 1.12f, EaseKind.Back),
                        Move(1.92f, 0.10f, StepKind.Show, "CrystalFall", Vector3.zero),
                        Move(1.92f, 0.56f, StepKind.FlyOff, "CrystalFall",
                            new Vector3(0f, -0.82f, -0.20f), ease: EaseKind.In),
                        Atmosphere(1.90f, 0.78f, "geode"),
                        Ambient(1.90f, 0.62f, "CaveDrips", 0.12f),
                        Ambient(1.90f, 0.48f, "CaveGeode", 1f),
                        Impact(1.90f, 1.42f),
                        Haptic(1.92f, "heavy"),
                        Sfx(1.94f, "crystal"),
                        Move(1.96f, 0.34f, StepKind.Shake, "CrystalRoute", Vector3.zero,
                            amplitude: 2.4f, ease: EaseKind.InOut),
                        Face(1.94f, SceneRef.PepA, PepFace.Happy),
                        Face(1.94f, SceneRef.PepB, PepFace.Happy),

                        // The cart ride becomes a longer crystal-road finish.
                        Move(2.08f, 0.34f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.16f, 0.06f, 0.08f), amplitude: 0.22f, ease: EaseKind.Hop),
                        Move(2.40f, 0.34f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.10f, 0.06f, 0f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Move(2.42f, 0.32f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.08f, 0f, -0.06f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Move(2.50f, 0.12f, StepKind.Hide, "CrystalFall", Vector3.zero),
                        Meet(2.84f, 0.56f),
                        Sfx(2.90f, "reunion"),
                        Impact(2.92f, 0.54f),
                        Haptic(2.94f, "success"),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Author.Prop("rope"), AnchorId = "Slot_2", Label = "The coil of rope",
                    Quip = "Now it is tied down.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.62f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.44f, 0.26f, 1.16f), amplitude: 0.42f, ease: EaseKind.Hop),
                        Sfx(0.66f, "creak"),
                        Move(0.68f, 0.70f, StepKind.Shake, "MineCart", Vector3.zero,
                            amplitude: 2.5f, ease: EaseKind.InOut),
                        Face(0.92f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Author.Prop("stone"), AnchorId = "Slot_3", Label = "The grey stone",
                    Quip = "Now it is very stuck.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.30f, 0.10f, 1.32f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Sfx(0.64f, "clunk"),
                        Move(0.66f, 0.66f, StepKind.Shake, "MineCart", Vector3.zero,
                            amplitude: 1.6f, ease: EaseKind.InOut),
                        Face(0.90f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }
    }
}
