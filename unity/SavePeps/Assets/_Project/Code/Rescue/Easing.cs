using UnityEngine;

namespace SavePeps.Rescue
{
    public enum EaseKind
    {
        Out = 0,
        In = 1,
        InOut = 2,
        Linear = 3,
        Back = 4,
        Hop = 5,
    }

    /// <summary>
    /// CSS-compatible cubic-bezier easing.
    ///
    /// These are the exact curves Save Pip's outcomes were tuned against. The
    /// timing of a gag is most of whether it is funny, so the curves are
    /// ported rather than approximated with Unity's built-in ease names —
    /// `Out` here is cubic-bezier(0.22, 0.9, 0.35, 1), which is noticeably
    /// snappier than a plain SmoothStep and is why objects land with weight.
    /// </summary>
    public static class Easing
    {
        // Matches the EASES table in Save Pip's choreo.ts.
        private static readonly CubicBezier OutCurve = new(0.22f, 0.9f, 0.35f, 1f);
        private static readonly CubicBezier InCurve = new(0.55f, 0.06f, 0.68f, 0.19f);
        private static readonly CubicBezier InOutCurve = new(0.42f, 0f, 0.58f, 1f);
        private static readonly CubicBezier BackCurve = new(0.34f, 1.56f, 0.64f, 1f);
        private static readonly CubicBezier HopCurve = new(0.45f, 0f, 0.55f, 1f);

        /// <summary>Maps linear progress 0..1 to eased progress.</summary>
        public static float Evaluate(EaseKind kind, float t)
        {
            t = Mathf.Clamp01(t);
            return kind switch
            {
                EaseKind.Linear => t,
                EaseKind.Out => OutCurve.Evaluate(t),
                EaseKind.In => InCurve.Evaluate(t),
                EaseKind.InOut => InOutCurve.Evaluate(t),
                EaseKind.Back => BackCurve.Evaluate(t),
                EaseKind.Hop => HopCurve.Evaluate(t),
                _ => t,
            };
        }
    }

    /// <summary>
    /// A CSS `cubic-bezier(x1, y1, x2, y2)` curve: control points are
    /// (0,0), (x1,y1), (x2,y2), (1,1). Progress comes in as x and the curve
    /// answers y, so we solve x(s) = t for the parameter s before reading y.
    /// </summary>
    public readonly struct CubicBezier
    {
        private readonly float _x1, _y1, _x2, _y2;

        public CubicBezier(float x1, float y1, float x2, float y2)
        {
            _x1 = x1; _y1 = y1; _x2 = x2; _y2 = y2;
        }

        public float Evaluate(float t)
        {
            // Linear curves need no solving, and this is the common case for
            // the identity-ish presets.
            if (Mathf.Approximately(_x1, _y1) && Mathf.Approximately(_x2, _y2)) return t;
            if (t <= 0f) return 0f;
            if (t >= 1f) return 1f;
            return SampleY(SolveForX(t));
        }

        private static float A(float a1, float a2) => 1f - 3f * a2 + 3f * a1;
        private static float B(float a1, float a2) => 3f * a2 - 6f * a1;
        private static float C(float a1) => 3f * a1;

        private static float Sample(float s, float a1, float a2) =>
            ((A(a1, a2) * s + B(a1, a2)) * s + C(a1)) * s;

        private static float Slope(float s, float a1, float a2) =>
            3f * A(a1, a2) * s * s + 2f * B(a1, a2) * s + C(a1);

        private float SampleY(float s) => Sample(s, _y1, _y2);

        /// <summary>
        /// Newton-Raphson, falling back to bisection where the curve is flat
        /// enough that Newton stalls (which happens on strong ease-in curves).
        /// </summary>
        private float SolveForX(float x)
        {
            var s = x;
            for (var i = 0; i < 8; i++)
            {
                var slope = Slope(s, _x1, _x2);
                if (Mathf.Abs(slope) < 1e-6f) break;
                var error = Sample(s, _x1, _x2) - x;
                if (Mathf.Abs(error) < 1e-6f) return s;
                s -= error / slope;
            }

            float low = 0f, high = 1f;
            s = x;
            for (var i = 0; i < 20; i++)
            {
                var value = Sample(s, _x1, _x2);
                if (Mathf.Abs(value - x) < 1e-6f) break;
                if (value > x) high = s; else low = s;
                s = (low + high) * 0.5f;
            }

            return s;
        }
    }
}
