using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Watch every outcome in the catalogue, back to back, unattended.
    ///
    /// PLAN §5.3's argument for this is a scheduling one: a polish pass over
    /// 36 rescues is 108 outcomes, and reaching each one by hand costs more
    /// than watching it does. Automating the *reaching* is what turns a day
    /// into an hour, and it is the only practical way to catch the bug class
    /// that matters most here — an outcome that leaves the scene subtly wrong
    /// for the next one (PLAN R7).
    ///
    /// It deliberately does not assert anything. A human is watching; the
    /// automated version of this is the PlayMode reset test.
    /// </summary>
    public sealed class RescueGauntletWindow : EditorWindow
    {
        private ContentValidator.Report _report;
        private Vector2 _scroll;

        [MenuItem("Tools/Save Peps/Rescue Gauntlet")]
        public static void Open()
        {
            var window = GetWindow<RescueGauntletWindow>("Gauntlet");
            window.minSize = new Vector2(360f, 280f);
            window.Validate();
        }

        private void OnEnable() => EditorApplication.update += RepaintWhileRunning;
        private void OnDisable() => EditorApplication.update -= RepaintWhileRunning;

        private void RepaintWhileRunning()
        {
            if (RescuePlayback.IsRunning) Repaint();
        }

        private void OnGUI()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(RescuePlayback.CatalogPath);
            if (catalog == null)
            {
                EditorGUILayout.HelpBox($"No catalogue at {RescuePlayback.CatalogPath}.", MessageType.Error);
                return;
            }

            DrawSummary(catalog);
            EditorGUILayout.Space(6);
            DrawControls();
            EditorGUILayout.Space(6);
            DrawReport();
        }

        // -------------------------------------------------------------------

        private static void DrawSummary(Catalog catalog)
        {
            var rescues = 0;
            for (var i = 1; i <= catalog.RoundCount; i++)
            {
                foreach (var rescue in catalog.Round(i)?.Rescues ?? System.Array.Empty<RescueDefinition>())
                {
                    if (rescue != null) rescues++;
                }
            }

            EditorGUILayout.LabelField("Catalogue", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"{catalog.RoundCount} rounds · {rescues} rescues · " +
                                       $"{catalog.FreeRoundCount} free");

            var (beats, seconds) = RescuePlayback.EstimateGauntlet();
            EditorGUILayout.LabelField($"Full run: {beats} outcomes, about {Mins(seconds)}.");
        }

        private void DrawControls()
        {
            if (RescuePlayback.IsRunning)
            {
                var rect = EditorGUILayout.GetControlRect(false, 22f);
                EditorGUI.ProgressBar(rect, RescuePlayback.Progress, RescuePlayback.Status);

                if (GUILayout.Button("Stop", GUILayout.Height(26)))
                {
                    RescuePlayback.Stop();
                    EditorApplication.isPlaying = false;
                }

                return;
            }

            EditorGUILayout.LabelField(RescuePlayback.Status, EditorStyles.miniLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Run Gauntlet", GUILayout.Height(30)))
                {
                    RescuePlayback.RunGauntlet();
                }

                if (GUILayout.Button("Validate", GUILayout.Height(30), GUILayout.Width(90)))
                {
                    Validate();
                }
            }

            EditorGUILayout.LabelField(
                "Runs in the Game scene. Entering play mode is expected.",
                EditorStyles.miniLabel);
        }

        private void DrawReport()
        {
            if (_report == null) return;

            if (_report.Ok && _report.Warnings.Count == 0)
            {
                EditorGUILayout.HelpBox("Content is valid.", MessageType.Info);
                return;
            }

            using var scope = new EditorGUILayout.ScrollViewScope(_scroll);
            _scroll = scope.scrollPosition;

            foreach (var error in _report.Errors) EditorGUILayout.HelpBox(error, MessageType.Error);
            foreach (var warning in _report.Warnings) EditorGUILayout.HelpBox(warning, MessageType.Warning);
        }

        private void Validate()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(RescuePlayback.CatalogPath);
            _report = ContentValidator.Validate(catalog);
        }

        private static string Mins(double seconds) =>
            seconds < 90d ? $"{seconds:F0}s" : $"{seconds / 60d:F0} min";
    }
}
