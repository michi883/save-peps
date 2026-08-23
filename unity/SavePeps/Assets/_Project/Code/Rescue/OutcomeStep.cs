using System;
using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// What a step does. Transform kinds expand into keyframes; event kinds
    /// fire once and are dispatched to the runner.
    ///
    /// The transform kinds are Save Pip's choreography vocabulary, ported name
    /// for name. They are not arbitrary — each one is a piece of physical
    /// comedy that earned its place across 106 rescues, and having them as
    /// named kinds rather than raw keyframes is what keeps authoring fast.
    /// </summary>
    public enum StepKind
    {
        /// <summary>Straight glide by Delta.</summary>
        Fly = 0,
        /// <summary>Arc to Delta with the midpoint lifted by Amplitude.</summary>
        Arc = 1,
        /// <summary>Two little hops to Delta — a Pep's walk.</summary>
        Hop = 2,
        /// <summary>Gravity fall by Delta with a small settle bounce.</summary>
        Drop = 3,
        /// <summary>Indignant wiggle in place; Amplitude is degrees of roll.</summary>
        Shake = 4,
        /// <summary>Spin in place; Amplitude is degrees about Y.</summary>
        Spin = 5,
        /// <summary>Reveal immediately, then apply Delta/rotation/scale over Duration.</summary>
        Show = 6,
        /// <summary>Hide immediately.</summary>
        Hide = 7,
        /// <summary>Drift off and vanish — balloons, birds, dignity.</summary>
        FlyOff = 8,

        // --- event kinds: no keyframes, dispatched by the runner ---

        /// <summary>Swap a Pep's face. Param is the face name.</summary>
        Face = 100,
        /// <summary>Play a sound. Param is the clip id.</summary>
        Sfx = 101,
        /// <summary>Fire a haptic. Param is light|medium|heavy|success.</summary>
        Haptic = 102,
        /// <summary>Hand both Peps to the shared reunion animation.</summary>
        Meet = 103,
        /// <summary>Atomically hide Target and reveal the AnimTarget named by Param.</summary>
        VisibilitySwap = 104,
        /// <summary>Kick the camera. Amplitude is the impact strength.</summary>
        Impact = 105,
        /// <summary>Blend to the environment outcome mood named by Param.</summary>
        Atmosphere = 106,
        /// <summary>Blend the named ambient control to Scale activity over Duration.</summary>
        Ambient = 107,
    }

    /// <summary>
    /// One keyframe, expressed as a delta from the target's rest pose.
    /// Mirrors the Frame type in Save Pip's choreo.ts.
    /// </summary>
    [Serializable]
    public sealed class Frame
    {
        public Vector3 Position;
        public Vector3 Rotation;
        public float Scale = 1f;
        /// <summary>-1 leaves alpha untouched at this keyframe.</summary>
        public float Alpha = -1f;

        public static Frame Rest => new();
    }

    /// <summary>
    /// One beat of an outcome: what moves, when, for how long, and to where.
    ///
    /// Times are absolute seconds from the tap — not a graph, not a state
    /// machine. A flat sorted list is readable at a glance and trivial to
    /// validate, which matters when there are ~108 of these to review.
    /// </summary>
    [Serializable]
    public sealed class OutcomeStep
    {
        [Tooltip("Seconds after the tap.")]
        public float At;

        [Tooltip("Seconds this step runs for. Event kinds ignore it.")]
        public float Duration = 0.5f;

        public StepKind Kind = StepKind.Fly;

        [Tooltip("$self, $pepA, $pepB, $object, or the name of an anchor or fx object in the diorama.")]
        public string Target = SceneRef.Self;

        [Tooltip("Movement relative to the target's rest pose, in diorama-local space.")]
        public Vector3 Delta;

        [Tooltip("Rotation delta in degrees.")]
        public Vector3 EulerDelta;

        [Tooltip("Scale multiplier at the end of the step. 1 leaves it alone.")]
        public float Scale = 1f;

        [Tooltip("Kind-specific magnitude: arc lift, shake degrees, hop height, spin degrees.")]
        public float Amplitude;

        public EaseKind Ease = EaseKind.Out;

        [Tooltip("Face, sfx, haptic, atmosphere cue, or reveal target, depending on Kind.")]
        public string Param;

        public bool IsEvent => (int)Kind >= 100;

        /// <summary>When this step is finished, in seconds after the tap.</summary>
        public float EndTime => At + (IsEvent ? 0f : Duration);
    }

    /// <summary>
    /// The reserved target names. Anything else is looked up by object name
    /// inside the diorama, and the content validator checks that it resolves —
    /// so a typo fails at author time rather than during a demo.
    /// </summary>
    public static class SceneRef
    {
        /// <summary>The tapped object itself.</summary>
        public const string Self = "$self";
        public const string PepA = "$pepA";
        public const string PepB = "$pepB";
        /// <summary>Both Peps together.</summary>
        public const string Peps = "$peps";
        public const string Camera = "$camera";

        public static bool IsReserved(string target) =>
            target is Self or PepA or PepB or Peps or Camera;
    }
}
