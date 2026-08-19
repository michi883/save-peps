using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Terse constructors for <see cref="OutcomeStep"/>.
    ///
    /// These exist so an authored outcome reads as a timeline — one line per
    /// beat, times in a column — rather than as a wall of object initialisers.
    /// Until the custom inspector lands (PLAN §5.3) this is the authoring
    /// surface, and legibility here is the difference between reviewing a gag
    /// and re-deriving it.
    /// </summary>
    public static class Steps
    {
        public static OutcomeStep Move(float at, float dur, StepKind kind, string target,
            Vector3 delta, float amplitude = 0f, EaseKind ease = EaseKind.Out) => new()
        {
            At = at, Duration = dur, Kind = kind, Target = target,
            Delta = delta, Amplitude = amplitude, Ease = ease, Scale = 1f,
        };

        public static OutcomeStep Face(float at, string target, PepFace face) => new()
        {
            At = at, Kind = StepKind.Face, Target = target, Param = face.ToString(), Scale = 1f,
        };

        public static OutcomeStep Sfx(float at, string id) => new()
        {
            At = at, Kind = StepKind.Sfx, Target = SceneRef.Self, Param = id, Scale = 1f,
        };

        public static OutcomeStep Haptic(float at, string strength) => new()
        {
            At = at, Kind = StepKind.Haptic, Target = SceneRef.Self, Param = strength, Scale = 1f,
        };

        public static OutcomeStep Meet(float at, float dur) => new()
        {
            At = at, Duration = dur, Kind = StepKind.Meet, Target = SceneRef.Peps, Scale = 1f,
        };
    }

    /// <summary>
    /// Wrong-answer outcomes that belong to a prop rather than to a rescue.
    ///
    /// A fan blows things over wherever it is tapped, so authoring that once
    /// and reusing it across lineups is not a shortcut — it is the thing that
    /// makes a prop feel like it has a personality. The player learns what a
    /// balloon does, and the joke lands faster the second time.
    ///
    /// Every gag here is written relative to <c>$self</c> and the Peps, never
    /// to a slot position, so the same steps work from any anchor.
    /// </summary>
    public static class PropGags
    {
        /// <summary>Excellent breeze, entirely the wrong direction.</summary>
        public static OutcomeStep[] Fan() => new[]
        {
            Steps.Sfx(0.05f, "whoosh"),
            Steps.Move(0.0f, 1.2f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                amplitude: 6f, ease: EaseKind.InOut),
            Steps.Face(0.3f, SceneRef.PepA, PepFace.Panic),
            Steps.Move(0.35f, 0.7f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0f, -0.30f)),
            Steps.Move(1.25f, 0.5f, StepKind.Fly, SceneRef.PepA, new Vector3(0f, 0f, 0.07f),
                ease: EaseKind.InOut),
        };

        /// <summary>Further apart, vertically. Needs the balloon at Slot_3.</summary>
        public static OutcomeStep[] Balloon() => new[]
        {
            Steps.Move(0.0f, 0.6f, StepKind.Arc, SceneRef.Self,
                new Vector3(0.45f, 0.34f, -0.68f), amplitude: 0.3f, ease: EaseKind.Hop),
            Steps.Face(0.35f, SceneRef.PepB, PepFace.Panic),
            Steps.Sfx(0.6f, "boing"),
            Steps.Move(0.7f, 0.6f, StepKind.Fly, SceneRef.PepB, new Vector3(0f, 0.45f, 0f)),
            Steps.Move(0.7f, 0.6f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.45f, 0f)),
            Steps.Move(1.35f, 0.9f, StepKind.FlyOff, SceneRef.PepB,
                new Vector3(0.22f, 0.85f, 0f), ease: EaseKind.In),
            Steps.Move(1.35f, 0.9f, StepKind.FlyOff, SceneRef.Self,
                new Vector3(0.22f, 0.85f, 0f), ease: EaseKind.In),
        };

        /// <summary>
        /// Pops open, catches the wind, and leaves without anybody. Written
        /// entirely in self-relative deltas so it can sit at any slot — which
        /// is why it can serve as the near-miss in more than one lineup.
        /// </summary>
        public static OutcomeStep[] Umbrella() => new[]
        {
            Steps.Sfx(0.05f, "pop"),
            Steps.Move(0.0f, 0.4f, StepKind.Spin, SceneRef.Self, Vector3.zero, amplitude: 160f),
            Steps.Move(0.1f, 0.35f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.18f, 0f)),
            Steps.Face(0.5f, SceneRef.PepA, PepFace.Worried),
            Steps.Face(0.5f, SceneRef.PepB, PepFace.Worried),
            Steps.Sfx(0.6f, "whoosh"),
            Steps.Move(0.55f, 1.1f, StepKind.FlyOff, SceneRef.Self,
                new Vector3(0.9f, 0.55f, -0.4f), ease: EaseKind.In),
        };
    }
}
