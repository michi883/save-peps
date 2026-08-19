using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Draws a diorama's anchors in the scene view.
    ///
    /// Anchors are empty GameObjects, which means they are invisible exactly
    /// when they matter most — while dressing a new diorama and deciding where
    /// the Peps stand and where the three objects sit. Without this, placing a
    /// slot is done by typing numbers and then entering play mode to see where
    /// they landed.
    ///
    /// Recognised by name rather than by a marker component, so it works on
    /// the existing prefabs untouched and adds nothing to the runtime.
    /// </summary>
    [InitializeOnLoad]
    public static class AnchorGizmos
    {
        private const string MenuPath = "Tools/Save Peps/Show Anchor Gizmos";
        private const string PrefKey = "SavePeps.ShowAnchorGizmos";

        private static readonly Color PepAColor = new(1f, 0.46f, 0.38f);
        private static readonly Color PepBColor = new(0.18f, 0.77f, 0.71f);
        private static readonly Color MeetColor = new(1f, 0.71f, 0.24f);
        private static readonly Color SlotColor = new(0.85f, 0.85f, 0.95f);
        private static readonly Color MoverColor = new(0.44f, 0.75f, 0.89f);

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(PrefKey, true);
            set => EditorPrefs.SetBool(PrefKey, value);
        }

        static AnchorGizmos()
        {
            SceneView.duringSceneGui += OnSceneGui;
            EditorApplication.delayCall += () => Menu.SetChecked(MenuPath, Enabled);
        }

        [MenuItem(MenuPath)]
        private static void Toggle()
        {
            Enabled = !Enabled;
            Menu.SetChecked(MenuPath, Enabled);
            SceneView.RepaintAll();
        }

        private static void OnSceneGui(SceneView view)
        {
            if (!Enabled || Application.isPlaying) return;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                Draw(stage.prefabContentsRoot.transform);
                return;
            }

            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded) return;

            foreach (var root in scene.GetRootGameObjects())
            {
                Draw(root.transform);
            }
        }

        private static void Draw(Transform root)
        {
            foreach (var t in root.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                if (!Classify(t.name, out var color, out var radius)) continue;

                var position = t.position;
                Handles.color = color;
                Handles.DrawWireDisc(position, Vector3.up, radius);

                // A stalk as well as a disc: at the diorama's fixed camera
                // pitch a flat disc is nearly edge-on and easy to miss.
                Handles.DrawLine(position, position + Vector3.up * (radius * 1.6f));

                Handles.Label(position + Vector3.up * (radius * 1.8f), t.name, Style(color));
            }
        }

        private static bool Classify(string name, out Color color, out float radius)
        {
            radius = 0.11f;

            switch (name)
            {
                case "Anchor_PepA": color = PepAColor; return true;
                case "Anchor_PepB": color = PepBColor; return true;
                case "Anchor_Meet": color = MeetColor; radius = 0.14f; return true;
            }

            if (name.StartsWith("Slot_", System.StringComparison.Ordinal))
            {
                color = SlotColor;
                return true;
            }

            // A mover is scenery choreography can reach; seeing which parts of
            // a diorama those are is half of knowing what an outcome can do.
            if (name == "Water" || name == "Movers")
            {
                color = MoverColor;
                radius = 0.2f;
                return name == "Water";
            }

            color = default;
            return false;
        }

        private static GUIStyle Style(Color color) => new(EditorStyles.miniLabel)
        {
            normal = { textColor = color },
        };
    }
}
