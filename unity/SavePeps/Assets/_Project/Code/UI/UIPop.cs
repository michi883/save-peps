using System.Collections;
using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.UI
{
    /// <summary>
    /// The one motion vocabulary every piece of UI in the game speaks.
    ///
    /// The 3D layer earned its feel from a shared set of curves; the layer on
    /// top of it kept inventing its own fade per panel, which is most of why
    /// the shell read as a form and the diorama read as a toy. These four
    /// routines are that fix: overshoot in, snap out, punch, and press — all
    /// on <see cref="Easing"/>'s ported curves, all on unscaled time so a
    /// paused or slowed scene never makes the interface feel broken.
    /// </summary>
    public static class UIPop
    {
        public const float InDuration = 0.22f;
        public const float OutDuration = 0.12f;

        /// <summary>
        /// Puts a panel in its pre-entrance pose. Separate from <see cref="In"/>
        /// so a caller can stage the pose on the same frame it activates the
        /// object — a one-frame flash of the full-size panel is the classic
        /// way a pop-in ends up looking like a glitch instead of a bounce.
        /// </summary>
        public static void Prepare(RectTransform rect, CanvasGroup group, float from = 0.78f, float tilt = 0f)
        {
            if (group != null)
            {
                group.alpha = 0f;
                group.interactable = false;
                group.blocksRaycasts = true;
            }

            if (rect == null) return;
            rect.localScale = Vector3.one * from;
            rect.localRotation = Quaternion.Euler(0f, 0f, tilt);
        }

        /// <summary>
        /// Overshoots to full size. The alpha runs faster than the scale so
        /// the panel is legible before it stops moving, which is what makes a
        /// 0.22 s entrance feel instant rather than sluggish.
        /// </summary>
        public static IEnumerator In(RectTransform rect, CanvasGroup group,
            float duration = InDuration, float from = 0.78f, float tilt = 0f)
        {
            Prepare(rect, group, from, tilt);

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var linear = Mathf.Clamp01(elapsed / duration);
                var scale = Easing.Evaluate(EaseKind.Back, linear);
                if (rect != null)
                {
                    rect.localScale = Vector3.one * Mathf.LerpUnclamped(from, 1f, scale);
                    rect.localRotation = Quaternion.Euler(0f, 0f, Mathf.LerpUnclamped(tilt, 0f, scale));
                }
                if (group != null) group.alpha = Easing.Evaluate(EaseKind.Out, Mathf.Clamp01(linear * 1.9f));
                yield return null;
            }

            Settle(rect, group);
        }

        /// <summary>Leaves quickly and slightly smaller — a dismissal, not a fade.</summary>
        public static IEnumerator Out(RectTransform rect, CanvasGroup group,
            float duration = OutDuration, float to = 0.90f)
        {
            if (group != null)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            var startScale = rect != null ? rect.localScale.x : 1f;
            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.In, Mathf.Clamp01(elapsed / duration));
                if (rect != null) rect.localScale = Vector3.one * Mathf.Lerp(startScale, to, t);
                if (group != null) group.alpha = 1f - t;
                yield return null;
            }

            if (group != null) group.alpha = 0f;
        }

        /// <summary>A short squash-and-spring, for something that just changed.</summary>
        public static IEnumerator Punch(RectTransform rect, float peak = 1.14f, float rest = 1f)
        {
            if (rect == null) yield break;

            var elapsed = 0f;
            const float up = 0.14f;
            while (elapsed < up)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.Back, Mathf.Clamp01(elapsed / up));
                rect.localScale = Vector3.one * Mathf.LerpUnclamped(rest * 0.86f, rest * peak, t);
                yield return null;
            }

            elapsed = 0f;
            const float down = 0.16f;
            while (elapsed < down)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.Out, Mathf.Clamp01(elapsed / down));
                rect.localScale = Vector3.one * Mathf.Lerp(rest * peak, rest, t);
                yield return null;
            }

            rect.localScale = Vector3.one * rest;
        }

        /// <summary>Eases a panel to a quieter resting size, without hiding it.</summary>
        public static IEnumerator Settle(RectTransform rect, CanvasGroup group,
            float scale, float alpha, float duration = 0.34f)
        {
            var fromScale = rect != null ? rect.localScale.x : 1f;
            var fromAlpha = group != null ? group.alpha : 1f;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.InOut, Mathf.Clamp01(elapsed / duration));
                if (rect != null) rect.localScale = Vector3.one * Mathf.Lerp(fromScale, scale, t);
                if (group != null) group.alpha = Mathf.Lerp(fromAlpha, alpha, t);
                yield return null;
            }

            if (rect != null) rect.localScale = Vector3.one * scale;
            if (group != null) group.alpha = alpha;
        }

        private static void Settle(RectTransform rect, CanvasGroup group)
        {
            if (rect != null)
            {
                rect.localScale = Vector3.one;
                rect.localRotation = Quaternion.identity;
            }

            if (group == null) return;
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }
}
