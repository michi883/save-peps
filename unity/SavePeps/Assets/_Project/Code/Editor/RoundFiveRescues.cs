using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

using static SavePeps.EditorTools.Steps;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Round five introduces aquatic flotation, heavy counterweight balancing,
    /// and botanical temperature control.
    /// </summary>
    public static class RoundFiveRescues
    {
        public static RoundDefinition SeedRound(bool overwrite, ContentSeeder.SeedLog log)
        {
            var r13 = BuildFerry(overwrite, log);
            var r14 = BuildDrop(overwrite, log);
            var r15 = BuildChill(overwrite, log);

            if (ContentSeeder.Claim<RoundDefinition>(
                    $"{ContentPaths.RoundDir}/Round_05.asset", overwrite, log, out var round))
            {
                round.Number = 5;
                round.Rescues = new[] { r13, r14, r15 };
                EditorUtility.SetDirty(round);
            }

            return round;
        }

        private static RescueDefinition BuildFerry(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r13_ferry.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r13", "ferry", "Ride the stream.", Difficulty.Medium,
                ReasoningKind.Crossing, "Diorama_Brook",
                "The brook flows swiftly between the Peps. A buoyant raft could ferry Pep A downstream.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "leaf", Prop = Prop("leaf"), AnchorId = "Slot_1", Label = "The broad green leaf",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, -0.08f, 0.65f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.6f, "splash"),
                        Face(0.65f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.7f, 0.45f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.35f), amplitude: 0.15f, ease: EaseKind.Hop),
                        Move(1.15f, 0.7f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.75f), ease: EaseKind.InOut),
                        Move(1.15f, 0.7f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0f, 0.75f), ease: EaseKind.InOut),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_2", Label = "The round stone",
                    Quip = "Stones don't float.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, -0.1f, 1.35f), amplitude: 0.2f, ease: EaseKind.Hop),
                        Sfx(0.6f, "splash"),
                        Move(0.65f, 0.3f, StepKind.Shake, "Water", Vector3.zero, amplitude: 2f),
                        Face(0.7f, SceneRef.PepA, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "plank", Prop = Prop("plank"), AnchorId = "Slot_3", Label = "The wooden plank",
                    Quip = "Floated straight away.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, -0.05f, -1.3f), amplitude: 0.22f, ease: EaseKind.Hop),
                        Sfx(0.55f, "splash"),
                        Move(0.6f, 0.9f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.8f, 0f, 0f), ease: EaseKind.In),
                        Face(0.7f, SceneRef.PepA, PepFace.Panic),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildDrop(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r14_drop.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r14", "drop", "Lower the lift.", Difficulty.Medium,
                ReasoningKind.Counterweight, "Diorama_Lift",
                "Pep B is trapped high on an elevated lift platform awaiting counterweight activation.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "balloon", Prop = Prop("balloon"), AnchorId = "Slot_1", Label = "The orange balloon",
                    Quip = "Floats the wrong way.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.7f, StepKind.Fly, SceneRef.Self,
                            new Vector3(0f, 0.8f, 0f), ease: EaseKind.Out),
                        Face(0.4f, SceneRef.PepB, PepFace.Worried),
                    },
                },
                new RescueObject
                {
                    Id = "pillow", Prop = Prop("pillow"), AnchorId = "Slot_2", Label = "The soft pillow",
                    Quip = "Too light for the scale.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.25f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Sfx(0.6f, "poof"),
                        Move(0.65f, 0.35f, StepKind.Shake, "Counterweight", Vector3.zero, amplitude: 1.5f),
                        Face(0.75f, SceneRef.PepB, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "stone", Prop = Prop("stone"), AnchorId = "Slot_3", Label = "The heavy stone",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0f, 0.65f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.45f, -1.3f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.65f, "thud"),
                        Move(0.7f, 0.75f, StepKind.Drop, "Counterweight", new Vector3(0f, -0.45f, 0f), ease: EaseKind.InOut),
                        Move(0.7f, 0.75f, StepKind.Fly, "LiftPlatform", new Vector3(0f, -0.45f, 0f), ease: EaseKind.InOut),
                        Rotate(0.7f, 0.75f, "Pulley", new Vector3(0f, 0f, 180f), EaseKind.InOut),
                        Face(0.85f, SceneRef.PepB, PepFace.Happy),
                        Move(1.35f, 0.55f, StepKind.Hop, SceneRef.PepB,
                            new Vector3(0f, 0f, -0.45f), amplitude: 0.12f, ease: EaseKind.Hop),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },
            };

            rescue.CorrectIndex = 2;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static RescueDefinition BuildChill(bool overwrite, ContentSeeder.SeedLog log)
        {
            if (!ContentSeeder.Claim<RescueDefinition>(
                    $"{ContentPaths.RescueDir}/r15_chill.asset", overwrite, log, out var rescue))
            {
                return rescue;
            }

            Stage(rescue, "r15", "chill", "Cool the sprout.", Difficulty.Surprising,
                ReasoningKind.Temperature, "Diorama_Grow",
                "A wilting magical sprout overheats under intense sun; it needs refreshing cooling to grow.");

            rescue.Objects = new[]
            {
                new RescueObject
                {
                    Id = "hair_dryer", Prop = Prop("hair_dryer"), AnchorId = "Slot_1", Label = "The hair dryer",
                    Quip = "Way too hot for sprout.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.15f, 1.25f), amplitude: 0.28f, ease: EaseKind.Hop),
                        Resize(0.65f, 0.4f, "Plant", 0.65f, EaseKind.In),
                        Face(0.75f, SceneRef.PepA, PepFace.Panic),
                    },
                },
                new RescueObject
                {
                    Id = "fan", Prop = Prop("fan"), AnchorId = "Slot_2", Label = "The electric fan",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "whoosh"),
                        Move(0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(-0.45f, 0.12f, 1.35f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Move(0.6f, 0.8f, StepKind.Shake, SceneRef.Self, Vector3.zero, amplitude: 6f, ease: EaseKind.InOut),
                        Resize(0.75f, 0.75f, "Plant", 1.85f, EaseKind.Back),
                        Face(0.85f, SceneRef.PepA, PepFace.Hopeful),
                        Move(1.3f, 0.6f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.85f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },
                new RescueObject
                {
                    Id = "watering_can", Prop = Prop("watering_can"), AnchorId = "Slot_3", Label = "The blue watering can",
                    Quip = "Water is boiling hot.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "splash"),
                        Move(0f, 0.55f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.15f, -1.3f), amplitude: 0.25f, ease: EaseKind.Hop),
                        Move(0.6f, 0.45f, StepKind.Shake, "Plant", Vector3.zero, amplitude: 3f),
                        Face(0.7f, SceneRef.PepA, PepFace.Worried),
                    },
                },
            };

            rescue.CorrectIndex = 1;
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
