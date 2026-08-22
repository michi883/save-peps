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
                "Black water — or a black hole in the floor, there is no way to tell — lies between the " +
                "Peps, with an empty lamp hook on the wall.");

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
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.38f, 0.32f, 1.50f), amplitude: 0.46f, ease: EaseKind.Hop),
                        Sfx(0.68f, "click"),
                        Move(0.70f, 0.44f, StepKind.Shake, "LampHook", Vector3.zero,
                            amplitude: 4f, ease: EaseKind.InOut),
                        // The single biggest state change in the game: the
                        // whole stage stops being a void and becomes a room.
                        Move(0.86f, 0.50f, StepKind.Hide, "Darkness", Vector3.zero),
                        Move(0.86f, 0.50f, StepKind.Show, "LitPool", Vector3.zero),
                        Move(1.00f, 0.44f, StepKind.Show, "Shallows", Vector3.zero),
                        Sfx(1.04f, "crystal"),
                        Haptic(1.06f, "light"),
                        Face(1.44f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.44f, SceneRef.PepB, PepFace.Hopeful),
                        Sfx(1.64f, "splash"),
                        Move(1.60f, 0.92f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0.32f, 0f, 0.78f), amplitude: 0.13f, ease: EaseKind.Hop),
                        Meet(2.62f, 0.74f),
                        Sfx(2.68f, "reunion"),
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
                "A seam of crystal runs the length of the cave roof, and a curtain of loose rock seals " +
                "the tunnel where the other Pep waits.");

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
                    Duration = 3.5f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.66f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0f, 0.66f, 1.54f), amplitude: 0.58f, ease: EaseKind.Hop),
                        Sfx(0.70f, "crystal"),
                        Haptic(0.72f, "light"),
                        Move(0.72f, 0.90f, StepKind.Shake, "CrystalVein", Vector3.zero,
                            amplitude: 3f, ease: EaseKind.InOut),
                        // The ring travels: the ripple is shown at the near end
                        // and flown along the seam, so the cave visibly answers
                        // rather than merely lighting up.
                        Move(0.86f, 0.16f, StepKind.Show, "VeinRing", Vector3.zero),
                        Move(0.88f, 0.86f, StepKind.Fly, "VeinRing", new Vector3(0f, 0f, 0.90f),
                            ease: EaseKind.Out),
                        Resize(0.88f, 0.86f, "VeinRing", 1.45f, EaseKind.Out),
                        Move(1.76f, 0.30f, StepKind.Hide, "VeinRing", Vector3.zero),
                        Sfx(1.74f, "rumble"),
                        Move(1.72f, 0.36f, StepKind.Shake, "RockCurtain", Vector3.zero,
                            amplitude: 7f, ease: EaseKind.InOut),
                        Move(2.06f, 0.26f, StepKind.Show, "Dust", Vector3.zero),
                        Resize(2.08f, 0.60f, "Dust", 1.80f, EaseKind.Out),
                        Move(2.08f, 0.24f, StepKind.Hide, "RockCurtain", Vector3.zero),
                        Move(2.72f, 0.34f, StepKind.Hide, "Dust", Vector3.zero),
                        Face(2.14f, SceneRef.PepA, PepFace.Hopeful),
                        Face(2.14f, SceneRef.PepB, PepFace.Hopeful),
                        Move(2.24f, 0.74f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.31f, -0.26f, -1.08f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.98f, 0.50f),
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
                "A loaded ore cart sits on the rail across the tunnel, held by one wooden wedge under " +
                "its front wheel.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pickaxe", Prop = Author.Prop("pickaxe"), AnchorId = "Slot_1", Label = "The miner's pick",
                    Duration = 3.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.60f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.36f, 0.18f, 1.06f), amplitude: 0.40f, ease: EaseKind.Hop),
                        Move(0.62f, 0.36f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 24f, ease: EaseKind.InOut),
                        Sfx(0.82f, "chip"),
                        Move(0.98f, 0.36f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 24f, ease: EaseKind.InOut),
                        Sfx(1.18f, "chip"),
                        Haptic(1.20f, "medium"),
                        Move(1.22f, 0.60f, StepKind.FlyOff, "Chock",
                            new Vector3(-0.36f, 0.06f, -0.28f), ease: EaseKind.In),
                        Sfx(1.30f, "rumble"),
                        Move(1.34f, 0.96f, StepKind.Fly, "MineCart",
                            new Vector3(0f, 0f, 1.42f), ease: EaseKind.In),
                        Move(1.34f, 0.70f, StepKind.Shake, "Rail", Vector3.zero,
                            amplitude: 1.2f, ease: EaseKind.InOut),
                        Face(1.42f, SceneRef.PepA, PepFace.Hopeful),
                        Face(1.42f, SceneRef.PepB, PepFace.Hopeful),
                        Move(1.66f, 0.82f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(-0.34f, 0f, -0.90f), amplitude: 0.14f, ease: EaseKind.Hop),
                        Meet(2.56f, 0.74f),
                        Sfx(2.62f, "reunion"),
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
