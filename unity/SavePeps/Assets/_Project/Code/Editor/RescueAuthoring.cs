using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// The two lines every rescue seeder repeated twelve times over: staging a
    /// rescue's identity, and loading a prop by id.
    ///
    /// Worth collapsing now that the environment name is per rescue rather
    /// than per round — thirty-six distinct stages meant thirty-six copies of
    /// the same eight assignments, and a divergence between two of them would
    /// have been a silent staging bug rather than a compile error.
    /// </summary>
    internal static class Author
    {
        internal static void Stage(RescueDefinition rescue, string id, string verb, string goal,
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

        internal static GameObject Prop(string id) =>
            Load<GameObject>($"{ContentPaths.PropDir}/{id}.prefab");

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.LogError($"[SavePeps] Missing asset: {path}");
            return asset;
        }
    }
}
