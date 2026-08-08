using System;
using System.Collections.Generic;
using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// One step in flight. Holds its own keyframes (already expanded from the
    /// step's <see cref="StepKind"/>) and answers "what is your delta right
    /// now" — including after it has finished, where it holds its final frame
    /// so later steps stack on top of it rather than snapping back.
    /// </summary>
    public sealed class MoveInstance
    {
        private readonly Frame[] _frames;
        private readonly float _start;
        private readonly float _duration;
        private readonly EaseKind _ease;

        public MoveInstance(Frame[] frames, float start, float duration, EaseKind ease)
        {
            _frames = frames;
            _start = start;
            _duration = Mathf.Max(0.0001f, duration);
            _ease = ease;
        }

        public Frame Evaluate(float now)
        {
            var t = Mathf.Clamp01((now - _start) / _duration);
            if (t <= 0f) return RestFrame;

            var eased = Easing.Evaluate(_ease, t);

            // Keyframes are evenly spaced: rest, then each via, then to.
            var segments = _frames.Length;
            var scaled = eased * segments;
            var index = Mathf.Min(Mathf.FloorToInt(scaled), segments - 1);
            var local = scaled - index;

            var from = index == 0 ? RestFrame : _frames[index - 1];
            var to = _frames[index];
            return Lerp(from, to, local);
        }

        private static readonly Frame RestFrame = new();

        private static Frame Lerp(Frame a, Frame b, float t) => new()
        {
            Position = Vector3.LerpUnclamped(a.Position, b.Position, t),
            Rotation = Vector3.LerpUnclamped(a.Rotation, b.Rotation, t),
            Scale = Mathf.LerpUnclamped(a.Scale, b.Scale, t),
            // An unset alpha inherits the previous keyframe's, so a step can
            // move something without disturbing a fade another step owns.
            Alpha = b.Alpha < 0f ? a.Alpha : (a.Alpha < 0f ? b.Alpha : Mathf.LerpUnclamped(a.Alpha, b.Alpha, t)),
        };
    }

    /// <summary>
    /// Plays an outcome: a flat list of <see cref="OutcomeStep"/> scheduled in
    /// absolute seconds from the tap.
    ///
    /// Transforms composite *additively*, which is the whole point. A Pep
    /// hopping onto a plank and then riding it across is two independent
    /// steps, not one hand-computed path — so authoring a rescue stays a
    /// matter of describing beats rather than solving motion.
    /// </summary>
    public sealed class ChoreographyPlayer : MonoBehaviour
    {
        private readonly Dictionary<AnimTarget, List<MoveInstance>> _live = new();
        private readonly List<(OutcomeStep step, float at)> _pendingEvents = new();

        private float _clock;
        private bool _running;

        /// <summary>Fired when an event-kind step comes due.</summary>
        public event Action<OutcomeStep> OnEvent;

        public bool IsRunning => _running;

        /// <summary>
        /// Begins an outcome. <paramref name="resolve"/> maps a step's target
        /// name to a live <see cref="AnimTarget"/>; unresolved targets are
        /// skipped loudly rather than failing silently.
        /// </summary>
        public void Play(IReadOnlyList<OutcomeStep> steps, Func<string, AnimTarget> resolve)
        {
            Stop();

            foreach (var step in steps)
            {
                if (step.IsEvent)
                {
                    _pendingEvents.Add((step, step.At));
                    continue;
                }

                var target = resolve(step.Target);
                if (target == null)
                {
                    Debug.LogWarning($"[SavePeps] Step targets '{step.Target}', which is not in this scene.");
                    continue;
                }

                if (!_live.TryGetValue(target, out var list))
                {
                    list = new List<MoveInstance>();
                    _live[target] = list;
                }

                list.Add(new MoveInstance(Expand(step), step.At, step.Duration, step.Ease));
            }

            _pendingEvents.Sort((a, b) => a.at.CompareTo(b.at));
            _clock = 0f;
            _running = true;
        }

        /// <summary>
        /// Hands a target back to whoever asked for it, leaving it exactly
        /// where the choreography left it.
        ///
        /// Needed because a finished step keeps holding its final frame and
        /// rewriting the transform every update. Without releasing first, a
        /// scripted sequence like the reunion and the player would fight over
        /// the same transform and the player would win.
        /// </summary>
        public void Release(AnimTarget target)
        {
            if (target != null) _live.Remove(target);
        }

        /// <summary>
        /// Halts everything and returns every touched target to rest. This is
        /// the retry path, and it must be exact — see <see cref="AnimTarget"/>.
        /// </summary>
        public void Stop()
        {
            foreach (var target in _live.Keys)
            {
                if (target != null) target.ResetToRest();
            }

            _live.Clear();
            _pendingEvents.Clear();
            _running = false;
            _clock = 0f;
        }

        private void Update()
        {
            if (!_running) return;

            _clock += Time.deltaTime;

            while (_pendingEvents.Count > 0 && _pendingEvents[0].at <= _clock)
            {
                var (step, _) = _pendingEvents[0];
                _pendingEvents.RemoveAt(0);
                OnEvent?.Invoke(step);
            }

            foreach (var (target, moves) in _live)
            {
                if (target != null) target.Accumulate(moves, _clock);
            }
        }

        // -------------------------------------------------------------------
        // Step kind → keyframes.
        //
        // Ported from Save Pip's choreo.ts factories. Each is a small piece of
        // physical comedy that earned its place across 106 rescues; keeping
        // them as named kinds rather than raw keyframes is what makes a rescue
        // cheap to author.
        // -------------------------------------------------------------------

        private static Frame[] Expand(OutcomeStep s)
        {
            var d = s.Delta;
            var rot = s.EulerDelta;
            var scale = s.Scale;

            switch (s.Kind)
            {
                case StepKind.Fly:
                    return new[] { new Frame { Position = d, Rotation = rot, Scale = scale } };

                case StepKind.Arc:
                {
                    var lift = s.Amplitude <= 0f ? 0.4f : s.Amplitude;
                    return new[]
                    {
                        new Frame { Position = d * 0.5f + Vector3.up * lift, Rotation = rot * 0.5f, Scale = scale },
                        new Frame { Position = d, Rotation = rot, Scale = scale },
                    };
                }

                case StepKind.Hop:
                {
                    // Two little skips rather than a glide. Reads as walking
                    // without needing a walk cycle, which is why Save Pip's
                    // characters never needed one.
                    var h = s.Amplitude <= 0f ? 0.18f : s.Amplitude;
                    return new[]
                    {
                        new Frame { Position = d * 0.25f + Vector3.up * h, Scale = scale },
                        new Frame { Position = d * 0.5f, Scale = scale },
                        new Frame { Position = d * 0.75f + Vector3.up * h, Scale = scale },
                        new Frame { Position = d, Rotation = rot, Scale = scale },
                    };
                }

                case StepKind.Drop:
                {
                    var overshoot = s.Amplitude <= 0f ? 0.06f : s.Amplitude;
                    return new[]
                    {
                        new Frame { Position = d + Vector3.down * overshoot, Rotation = rot, Scale = scale },
                        new Frame { Position = d, Rotation = rot, Scale = scale },
                    };
                }

                case StepKind.Shake:
                {
                    var a = s.Amplitude <= 0f ? 7f : s.Amplitude;
                    return new[]
                    {
                        new Frame { Position = d, Rotation = new Vector3(0f, 0f, -a), Scale = scale },
                        new Frame { Position = d, Rotation = new Vector3(0f, 0f, a), Scale = scale },
                        new Frame { Position = d, Rotation = new Vector3(0f, 0f, -a * 0.6f), Scale = scale },
                        new Frame { Position = d, Rotation = Vector3.zero, Scale = scale },
                    };
                }

                case StepKind.Spin:
                {
                    var deg = Mathf.Approximately(s.Amplitude, 0f) ? 360f : s.Amplitude;
                    return new[] { new Frame { Position = d, Rotation = new Vector3(0f, deg, 0f), Scale = scale } };
                }

                case StepKind.Show:
                    return new[] { new Frame { Position = d, Rotation = rot, Scale = scale, Alpha = 1f } };

                case StepKind.Hide:
                    return new[] { new Frame { Position = d, Rotation = rot, Scale = scale, Alpha = 0f } };

                case StepKind.FlyOff:
                    return new[] { new Frame { Position = d, Rotation = rot, Scale = scale, Alpha = 0f } };

                default:
                    Debug.LogWarning($"[SavePeps] StepKind {s.Kind} has no expansion; treating it as a hold.");
                    return new[] { new Frame() };
            }
        }
    }
}
