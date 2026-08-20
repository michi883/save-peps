using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    public static class RoundNineRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r25 = BuildSignal(overwrite, log);
            var r26 = BuildDistract(overwrite, log);
            var r27 = BuildSever(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_09.asset", overwrite, log, out var round))
            {
                round.Number = 9;
                round.Rescues = new[] { r25, r26, r27 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildSignal(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r25_signal.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r25", "steer", "Steer deep glow.", Difficulty.Medium,
                ReasoningKind.Reflection, "Diorama_Ocean",
                "Deep underwater currents require a focused reflection to trigger the optical beacon.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_1", Label = "The soft pillow",
                    Quip = "Pillow absorbs the beam.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.6f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "mirror", Prop = Prop("mirror"), AnchorId = "Slot_2", Label = "The shiny mirror",
                    Duration = 3.0f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.4f, SceneRef.Self, new Vector3(0f, -42f, 0f), EaseKind.InOut),
                        Move(0.85f, 0.6f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.85f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Face(0.9f, SceneRef.PepA, PepFace.Hopeful),
                        Meet(1.95f, 0.75f),
                        Sfx(2.0f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_3", Label = "The dog bone",
                    Quip = "Dogs love it, beam ignores.",
                    Duration = 2.2f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.6f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildDistract(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r26_distract.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r26", "tempt", "Tempt deep angler.", Difficulty.Medium,
                ReasoningKind.Luring, "Diorama_Ocean",
                "A watchful abyssal sea-creature guards the tunnel entrance; a savory bone will lure it away.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "bone", Prop = Prop("bone"), AnchorId = "Slot_1", Label = "The tasty bone",
                    Duration = 3.0f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.6f, 0.25f, 1.4f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "thud"),
                        Move(0.8f, 0.8f, StepKind.Hop, "Anemone",
                            new Vector3(0.6f, 0f, 0.6f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Face(0.85f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.4f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(2.05f, 0.75f),
                        Sfx(2.1f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_2", Label = "The craft scissors",
                    Quip = "Creature snapped shears.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.55f, "snip"),
                        Face(0.65f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_3", Label = "The yellow umbrella",
                    Quip = "Spooked the creature.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildSever(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r27_sever.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r27", "slice", "Slice abyss kelp.", Difficulty.Surprising,
                ReasoningKind.Cutting, "Diorama_Ocean",
                "Tangled deep-sea kelp coils tightly around the hatch; sharp shears will slice it cleanly.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_1", Label = "The electric fan",
                    Quip = "Breeze tangled the kelp.",
                    Duration = 2.4f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "leaf", Prop = Prop("leaf"), AnchorId = "Slot_2", Label = "The green leaf",
                    Quip = "Leaf cannot cut kelp.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_3", Label = "The craft scissors",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "snip"),
                        Resize(0.7f, 0.45f, "ReefGate", 0.05f, EaseKind.In),
                        Face(0.8f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.15f, 0.65f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.95f), amplitude: 0.18f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static void Stage(RescueDefinition rescue, string id, string verb, string goal,
            Difficulty difficulty, ReasoningKind reasoning, string environment, string description)
        {
            rescue.Id = id;
            rescue.Verb = verb;
            rescue.Goal = goal;
            rescue.Difficulty = difficulty;
            rescue.Reasoning = reasoning;
            rescue.SceneDescription = description;
            rescue.Environment = Load<GameObject>($"{ContentPaths.EnvironmentDir}/{environment}.prefab");
            rescue.PepAPrefab = Load<GameObject>($"{ContentPaths.CharacterDir}/Pep_A.prefab");
            rescue.PepBPrefab = Load<GameObject>($"{ContentPaths.CharacterDir}/Pep_B.prefab");
            rescue.PepAAnchor = "Anchor_PepA";
            rescue.PepBAnchor = "Anchor_PepB";
            rescue.MeetAnchor = "Anchor_Meet";
        }

        private static GameObject Prop(string id) => Load<GameObject>($"{ContentPaths.PropDir}/{id}.prefab");

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.LogError($"[SavePeps] Missing asset: {path}");
            return asset;
        }
    }
}
