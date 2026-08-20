using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Round eight delivers surprising multi-step climaxes: aerostatic lift
    /// ascent, aerodynamic watercraft propulsion, and high-wire zipline reunion.
    /// </summary>
    public static class RoundEightRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r22 = BuildElevate(overwrite, log);
            var r23 = BuildPropel(overwrite, log);
            var r24 = BuildZipline(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_08.asset", overwrite, log, out var round))
            {
                round.Number = 8;
                round.Rescues = new[] { r22, r23, r24 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildElevate(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r22_elevate.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r22", "elevate", "Float the lift.", Difficulty.Medium,
                ReasoningKind.Counterweight, "Diorama_Lift",
                "The mechanical lift is stuck below; an aerostatic balloon can provide direct upward buoyancy.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_1", Label = "The heavy stone",
                    Quip = "Heavy stone sinks.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.6f, "thud"),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "balloon", Prop = Prop("balloon"), AnchorId = "Slot_2", Label = "The orange balloon",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.35f, 1.35f), amplitude: 0.3f, ease: EaseKind.Hop),
                        Move(0.7f, 0.75f, StepKind.Fly, "LiftPlatform", new Vector3(0f, 0.45f, 0f), ease: EaseKind.InOut),
                        Move(0.7f, 0.75f, StepKind.Drop, "Counterweight", new Vector3(0f, -0.45f, 0f), ease: EaseKind.InOut),
                        Rotate(0.7f, 0.75f, "Pulley", new Vector3(0f, 0f, -180f), EaseKind.InOut),
                        Face(0.85f, SceneRef.PepA, PepFace.Happy),
                        Move(1.35f, 0.55f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.45f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Prop("rope"), AnchorId = "Slot_3", Label = "The coil of rope",
                    Quip = "Needs upward pull.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildPropel(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r23_propel.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r23", "propel", "Drive the flow.", Difficulty.Surprising,
                ReasoningKind.Activation, "Diorama_Brook",
                "Strong localized airflow can create rapid water currents to bridge the gap.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_1", Label = "The electric fan",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.1f, 0.95f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Move(0.6f, 0.8f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 8f, ease: EaseKind.InOut),
                        Move(0.65f, 0.65f, StepKind.Shake, "Water", Vector3.zero, amplitude: 3f, ease: EaseKind.InOut),
                        Face(0.85f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.15f, 0.7f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(1.85f, 0.75f),
                        Sfx(1.9f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Quip = "Damp and heavy.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.05f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.6f, "splash"),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "plank", Prop = Prop("plank"), AnchorId = "Slot_3", Label = "The wooden plank",
                    Quip = "Drifted downstream.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, -0.05f, -1.3f), amplitude: 0.22f, ease: EaseKind.Hop),
                        Sfx(0.55f, "splash"),
                        Move(0.6f, 0.85f, StepKind.FlyOff, SceneRef.Self, new Vector3(0.8f, 0f, 0f), ease: EaseKind.In),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildZipline(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r24_zipline.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r24", "zipline", "Zip the canyon.", Difficulty.Surprising,
                ReasoningKind.Crossing, "Diorama_Canyon",
                "A tense cable strung across the canyon gorge creates a thrilling zipline descent.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "scissors", Prop = Prop("scissors"), AnchorId = "Slot_1", Label = "The shears",
                    Quip = "Cuts the connection.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Rotate(0.55f, 0.25f, SceneRef.Self, new Vector3(0f, 0f, 45f)),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "umbrella", Prop = Prop("umbrella"), AnchorId = "Slot_2", Label = "The orange umbrella",
                    Quip = "No wind today.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "pop"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.15f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Face(0.65f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "rope", Prop = Prop("rope"), AnchorId = "Slot_3", Label = "The sturdy rope",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.25f, -1.3f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.55f, "click"),
                        Face(0.65f, SceneRef.PepA, PepFace.Happy),
                        Move(0.7f, 0.85f, StepKind.Arc, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.18f, ease: EaseKind.InOut),
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
