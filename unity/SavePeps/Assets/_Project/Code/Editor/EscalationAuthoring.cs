using System.Collections.Generic;
using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Reusable choreography helpers tailored for the Introduce -> Expand -> Climax
    /// progression across all 12 rounds.
    ///
    /// Provides standardised timing bands, chained multi-target reactions,
    /// cascading pulses, and environment reactions.
    /// </summary>
    public static class EscalationAuthoring
    {
        public const float IntroduceMinDuration = 2.0f;
        public const float IntroduceMaxDuration = 2.5f;

        public const float ExpandMinDuration = 2.6f;
        public const float ExpandMaxDuration = 3.1f;

        public const float ClimaxMinDuration = 3.2f;
        public const float ClimaxMaxDuration = 3.6f;

        /// <summary>
        /// Offsets an array of OutcomeSteps by a time delta, enabling modular sub-sequences.
        /// </summary>
        public static OutcomeStep[] Offset(float timeOffset, params OutcomeStep[] steps)
        {
            if (steps == null) return System.Array.Empty<OutcomeStep>();
            var result = new OutcomeStep[steps.Length];
            for (var i = 0; i < steps.Length; i++)
            {
                var s = Clone(steps[i]);
                if (s == null) continue;
                s.At += timeOffset;
                result[i] = s;
            }
            return result;
        }

        /// <summary>
        /// Combines multiple step arrays into a single chronological sequence.
        /// </summary>
        public static OutcomeStep[] Combine(params OutcomeStep[][] stepGroups)
        {
            var list = new List<OutcomeStep>();
            if (stepGroups == null) return list.ToArray();
            foreach (var group in stepGroups)
            {
                if (group == null) continue;
                foreach (var step in group)
                {
                    var copy = Clone(step);
                    if (copy != null) list.Add(copy);
                }
            }
            list.Sort((a, b) => a.At.CompareTo(b.At));
            return list.ToArray();
        }

        /// <summary>
        /// Creates a cascading wave of motion across multiple targets with staggered start times.
        /// Useful for opening louvres, cascading gears, tilting foliage, or unfolding bridges.
        /// </summary>
        public static OutcomeStep[] Cascade(
            float startAt,
            float interval,
            float stepDur,
            StepKind kind,
            string[] targets,
            Vector3 delta,
            EaseKind ease = EaseKind.Out)
        {
            if (targets == null || targets.Length == 0) return System.Array.Empty<OutcomeStep>();
            var steps = new OutcomeStep[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                steps[i] = Steps.Move(
                    startAt + (i * interval),
                    stepDur,
                    kind,
                    targets[i],
                    delta,
                    ease: ease);
            }
            return steps;
        }

        /// <summary>
        /// Creates a rotating cascade across multiple targets (e.g. interlocking gears, tumbling panels).
        /// </summary>
        public static OutcomeStep[] CascadeRotate(
            float startAt,
            float interval,
            float stepDur,
            string[] targets,
            Vector3 euler,
            EaseKind ease = EaseKind.InOut)
        {
            if (targets == null || targets.Length == 0) return System.Array.Empty<OutcomeStep>();
            var steps = new OutcomeStep[targets.Length];
            for (var i = 0; i < targets.Length; i++)
            {
                steps[i] = Steps.Rotate(
                    startAt + (i * interval),
                    stepDur,
                    targets[i],
                    euler,
                    ease: ease);
            }
            return steps;
        }

        /// <summary>
        /// Generates a shudder/shake on a target (e.g. machinery kick, structural release, steam burst).
        /// </summary>
        public static OutcomeStep Shudder(float at, float dur, string target, float amplitude = 3.5f) =>
            Steps.Move(at, dur, StepKind.Shake, target, Vector3.zero, amplitude: amplitude, ease: EaseKind.InOut);

        private static OutcomeStep Clone(OutcomeStep source) => source == null ? null : new OutcomeStep
        {
            At = source.At,
            Duration = source.Duration,
            Kind = source.Kind,
            Target = source.Target,
            Delta = source.Delta,
            EulerDelta = source.EulerDelta,
            Scale = source.Scale,
            Amplitude = source.Amplitude,
            Ease = source.Ease,
            Param = source.Param,
        };
    }
}
