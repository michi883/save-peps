using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// The authoring surface for a rescue: the usual fields, plus the two
    /// things that decide whether authoring is a two-hour job or a two-day one
    /// — being able to watch any outcome in one click, and being told what is
    /// wrong before it is watched.
    ///
    /// The validation panel is deliberately always visible rather than hidden
    /// behind a button. A step aimed at a misspelled target does nothing at
    /// all when played, so "it looked fine when I watched it" is not evidence;
    /// the panel is the only thing that catches that class of mistake.
    /// </summary>
    [CustomEditor(typeof(RescueDefinition))]
    public sealed class RescueDefinitionEditor : Editor
    {
        private ContentValidator.Report _report;
        private int _dirtyCount = -1;

        private void OnEnable() => Revalidate();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Revalidating on every repaint would walk the environment prefab
            // continuously; the dirty counter changes exactly when the asset
            // does, which is the only time the answer can change.
            var dirty = EditorUtility.GetDirtyCount(target);
            if (dirty != _dirtyCount) Revalidate();

            EditorGUILayout.Space(8);
            DrawPreview();
            EditorGUILayout.Space(4);
            DrawValidation();
        }

        private void Revalidate()
        {
            _report = ContentValidator.Validate((RescueDefinition)target);
            _dirtyCount = EditorUtility.GetDirtyCount(target);
        }

        // -------------------------------------------------------------------

        private void DrawPreview()
        {
            var rescue = (RescueDefinition)target;

            EditorGUILayout.LabelField("Preview", EditorStyles.boldLabel);

            using (new EditorGUI.DisabledScope(rescue.Environment == null))
            {
                if (GUILayout.Button("Play This Rescue", GUILayout.Height(28)))
                {
                    // -1: stage it and hand over control, rather than playing
                    // an outcome. This is the "let me actually try it" case.
                    RescuePlayback.PlaySingle(rescue, -1);
                }

                var objects = rescue.Objects ?? System.Array.Empty<RescueObject>();
                using (new EditorGUILayout.HorizontalScope())
                {
                    for (var i = 0; i < objects.Length; i++)
                    {
                        var obj = objects[i];
                        var label = obj == null || string.IsNullOrEmpty(obj.Id) ? $"Slot {i + 1}" : obj.Id;
                        if (i == rescue.CorrectIndex) label += "  ●";

                        using (new EditorGUI.DisabledScope(obj == null))
                        {
                            if (GUILayout.Button(label, GUILayout.Height(24)))
                            {
                                RescuePlayback.PlaySingle(rescue, i);
                            }
                        }
                    }
                }

                EditorGUILayout.LabelField(
                    "● is the answer. Buttons enter play mode and run that outcome.",
                    EditorStyles.miniLabel);
            }
        }

        private void DrawValidation()
        {
            if (_report == null) return;

            if (_report.Errors.Count == 0 && _report.Warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("This rescue is valid.", MessageType.Info);
                return;
            }

            foreach (var error in _report.Errors)
            {
                EditorGUILayout.HelpBox(error, MessageType.Error);
            }

            foreach (var warning in _report.Warnings)
            {
                EditorGUILayout.HelpBox(warning, MessageType.Warning);
            }

            EditorGUILayout.LabelField(
                "Catalogue-wide rules (unique verbs, round composition) are checked by Tools > Save Peps > Validate Content.",
                EditorStyles.miniLabel);
        }
    }
}
