using System.Collections.Generic;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Plays rescues on demand from the editor: one outcome from the
    /// inspector, or every outcome in the catalogue back to back.
    ///
    /// This is the heir to Save Pip's <c>?p=c17&amp;v=2</c> dev shortcut, which
    /// is the single reason 106 rescues were authorable at all. The cost of
    /// checking a gag has to be one click; if it is "enter play mode, play
    /// three rescues, get to the one you changed", nobody checks, and the
    /// catalogue quietly fills with outcomes that have never been watched.
    ///
    /// Two implementation notes that are not obvious:
    ///
    /// - It drives the **real Game scene**, not a preview scene. A preview
    ///   that stages things differently from the game is a preview that lies,
    ///   and the whole point is to trust what you just watched.
    /// - Requests go through <see cref="SessionState"/> because entering play
    ///   mode triggers a domain reload, which wipes static fields. SessionState
    ///   survives it, so the queue is rebuilt on the other side.
    /// </summary>
    [InitializeOnLoad]
    public static class RescuePlayback
    {
        public const string GameScenePath = ContentPaths.GameScenePath;
        public const string CatalogPath = ContentPaths.CatalogPath;

        private const string ModeKey = "SavePeps.Playback.Mode";
        private const string RescueKey = "SavePeps.Playback.Rescue";
        private const string OutcomeKey = "SavePeps.Playback.Outcome";

        /// <summary>Seconds to let a freshly staged diorama settle before tapping.</summary>
        private const double Settle = 0.35;
        /// <summary>Extra seconds held after a wrong outcome, so the quip can be read.</summary>
        private const double WrongHold = 1.0;
        /// <summary>Extra seconds after a correct outcome, so the reunion can be watched.</summary>
        private const double RightHold = 1.8;

        /// <summary>One outcome of one rescue, as scheduled by the gauntlet.</summary>
        public readonly struct Beat
        {
            public readonly RescueDefinition Rescue;
            public readonly int Outcome;

            public Beat(RescueDefinition rescue, int outcome) { Rescue = rescue; Outcome = outcome; }

            public bool IsCorrect => Rescue != null && Rescue.CorrectIndex == Outcome;
        }

        private enum Phase { Idle, Stage, Tap, Hold }

        private static List<Beat> _queue;
        private static int _index;
        private static Phase _phase;
        private static double _deadline;
        private static bool _ticking;

        static RescuePlayback()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        // -------------------------------------------------------------------
        // Public entry points
        // -------------------------------------------------------------------

        public static bool IsRunning => _queue != null && _index < _queue.Count;

        /// <summary>Human-readable progress, for the Gauntlet window.</summary>
        public static string Status
        {
            get
            {
                if (!IsRunning) return SessionState.GetString(ModeKey, string.Empty) == string.Empty
                    ? "Idle."
                    : "Waiting for play mode…";

                var beat = _queue[_index];
                var obj = Objects(beat.Rescue);
                var name = beat.Outcome < obj.Length && obj[beat.Outcome] != null ? obj[beat.Outcome].Id : "?";
                var mark = beat.Rescue.CorrectIndex == beat.Outcome ? "correct" : "wrong";
                return $"{_index + 1}/{_queue.Count} — {beat.Rescue.Id} · {name} ({mark})";
            }
        }

        public static float Progress => _queue is { Count: > 0 } ? (float)_index / _queue.Count : 0f;

        /// <summary>
        /// Stages one rescue and optionally taps one of its objects.
        /// An outcome of -1 stages the scene and leaves the tap to you.
        /// </summary>
        public static void PlaySingle(RescueDefinition rescue, int outcome)
        {
            if (rescue == null) return;
            if (!EnsureGameScene()) return;

            SessionState.SetString(ModeKey, "single");
            SessionState.SetString(RescueKey, AssetDatabase.GetAssetPath(rescue));
            SessionState.SetInt(OutcomeKey, outcome);

            if (EditorApplication.isPlaying) Begin();
            else EditorApplication.isPlaying = true;
        }

        /// <summary>Every outcome of every rescue in the catalogue, unattended.</summary>
        public static void RunGauntlet()
        {
            if (!EnsureGameScene()) return;

            SessionState.SetString(ModeKey, "gauntlet");
            SessionState.SetString(RescueKey, string.Empty);
            SessionState.SetInt(OutcomeKey, -1);

            if (EditorApplication.isPlaying) Begin();
            else EditorApplication.isPlaying = true;
        }

        /// <summary>
        /// How many outcomes a full gauntlet run is, and roughly how long it
        /// takes. Shown before the run because "watch every outcome" is only a
        /// reasonable thing to ask if you know whether it is four minutes or
        /// forty.
        /// </summary>
        public static (int Beats, double Seconds) EstimateGauntlet()
        {
            var queue = GauntletOrder();
            if (queue == null) return (0, 0d);

            var total = 0d;
            foreach (var beat in queue)
            {
                var objects = Objects(beat.Rescue);
                if (beat.Outcome < 0 || beat.Outcome >= objects.Length || objects[beat.Outcome] == null) continue;

                var correct = beat.Rescue.CorrectIndex == beat.Outcome;
                total += Settle + objects[beat.Outcome].Duration + (correct ? RightHold : WrongHold);
            }

            return (queue.Count, total);
        }

        public static void Stop()
        {
            SessionState.SetString(ModeKey, string.Empty);
            _queue = null;
            _index = 0;
            _phase = Phase.Idle;
            StopTicking();
        }

        // -------------------------------------------------------------------
        // Play-mode lifecycle
        // -------------------------------------------------------------------

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            switch (state)
            {
                case PlayModeStateChange.EnteredPlayMode:
                    if (SessionState.GetString(ModeKey, string.Empty) != string.Empty) Begin();
                    break;

                case PlayModeStateChange.ExitingPlayMode:
                    Stop();
                    break;
            }
        }

        private static void Begin()
        {
            var mode = SessionState.GetString(ModeKey, string.Empty);
            _queue = mode == "gauntlet" ? GauntletOrder() : BuildSingleQueue();
            _index = 0;
            _phase = Phase.Stage;

            if (_queue == null || _queue.Count == 0)
            {
                Debug.LogWarning("[SavePeps] Nothing to play.");
                Stop();
                return;
            }

            // The flow would otherwise stage round 1 over the top of whatever
            // is being previewed, and advance past it on a correct answer.
            var flow = Object.FindFirstObjectByType<GameFlow>();
            if (flow != null) flow.enabled = false;

            StartTicking();
        }

        private static List<Beat> BuildSingleQueue()
        {
            var path = SessionState.GetString(RescueKey, string.Empty);
            var rescue = AssetDatabase.LoadAssetAtPath<RescueDefinition>(path);
            if (rescue == null) return null;

            return new List<Beat> { new(rescue, SessionState.GetInt(OutcomeKey, -1)) };
        }

        /// <summary>
        /// Wrong outcomes first, correct one last, for every rescue in
        /// catalogue order. Ending each rescue on the reunion is not
        /// decoration — a review pass is looking for outcomes that leave the
        /// scene wrong, and the reunion is where that shows up.
        /// </summary>
        /// <summary>
        /// Exactly what a full gauntlet run will play, in order. Public so the
        /// ordering rule can be asserted in a test rather than confirmed by
        /// watching four minutes of animation.
        /// </summary>
        public static List<Beat> GauntletOrder()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(CatalogPath);
            if (catalog == null)
            {
                Debug.LogError($"[SavePeps] No catalogue at {CatalogPath}.");
                return null;
            }

            var beats = new List<Beat>();
            var seen = new HashSet<RescueDefinition>();

            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                var round = catalog.Round(number);
                foreach (var rescue in round?.Rescues ?? System.Array.Empty<RescueDefinition>())
                {
                    if (rescue == null || !seen.Add(rescue)) continue;

                    var objects = Objects(rescue);
                    for (var i = 0; i < objects.Length; i++)
                    {
                        if (i != rescue.CorrectIndex) beats.Add(new Beat(rescue, i));
                    }

                    if (rescue.CorrectIndex >= 0 && rescue.CorrectIndex < objects.Length)
                    {
                        beats.Add(new Beat(rescue, rescue.CorrectIndex));
                    }
                }
            }

            return beats;
        }

        // -------------------------------------------------------------------
        // The ticker
        // -------------------------------------------------------------------

        private static void StartTicking()
        {
            if (_ticking) return;
            _ticking = true;
            EditorApplication.update += Tick;
        }

        private static void StopTicking()
        {
            if (!_ticking) return;
            _ticking = false;
            EditorApplication.update -= Tick;
        }

        private static void Tick()
        {
            if (!EditorApplication.isPlaying || !IsRunning)
            {
                if (!EditorApplication.isPlaying) Stop();
                return;
            }

            var runner = Object.FindFirstObjectByType<RescueRunner>();
            var router = Object.FindFirstObjectByType<TapRouter>();
            if (runner == null || router == null)
            {
                Debug.LogError("[SavePeps] The Game scene has no RescueRunner — cannot preview.");
                Stop();
                return;
            }

            var beat = _queue[_index];
            var now = EditorApplication.timeSinceStartup;

            switch (_phase)
            {
                case Phase.Stage:
                    runner.Load(beat.Rescue);
                    // A staged rescue with no tap requested is the "let me
                    // play it myself" case: hand it over and get out of the way.
                    if (beat.Outcome < 0)
                    {
                        _index = _queue.Count;
                        StopTicking();
                        SessionState.SetString(ModeKey, string.Empty);
                        return;
                    }

                    _deadline = now + Settle;
                    _phase = Phase.Tap;
                    break;

                case Phase.Tap:
                    if (now < _deadline) return;

                    var objects = Objects(beat.Rescue);
                    if (beat.Outcome >= objects.Length || objects[beat.Outcome] == null)
                    {
                        Advance();
                        return;
                    }

                    var obj = objects[beat.Outcome];
                    router.SimulateTap(obj.Id);

                    var correct = beat.Rescue.CorrectIndex == beat.Outcome;
                    _deadline = now + obj.Duration + (correct ? RightHold : WrongHold);
                    _phase = Phase.Hold;
                    break;

                case Phase.Hold:
                    if (now < _deadline) return;
                    Advance();
                    break;
            }
        }

        private static void Advance()
        {
            _index++;
            _phase = Phase.Stage;

            if (_index < _queue.Count) return;

            Debug.Log("[SavePeps] Playback finished.");
            SessionState.SetString(ModeKey, string.Empty);
            StopTicking();
        }

        // -------------------------------------------------------------------
        // Helpers
        // -------------------------------------------------------------------

        private static RescueObject[] Objects(RescueDefinition rescue) =>
            rescue.Objects ?? System.Array.Empty<RescueObject>();

        /// <summary>
        /// Opens the Game scene if it is not already the active one, giving the
        /// user a chance to save whatever they had open first.
        /// </summary>
        private static bool EnsureGameScene()
        {
            if (EditorApplication.isPlaying) return true;
            if (SceneManager.GetActiveScene().path == GameScenePath) return true;

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return false;

            EditorSceneManager.OpenScene(GameScenePath, OpenSceneMode.Single);
            return true;
        }
    }
}
