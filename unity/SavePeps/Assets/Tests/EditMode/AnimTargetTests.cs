using NUnit.Framework;
using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.Tests
{
    public sealed class AnimTargetTests
    {
        [Test]
        public void HiddenRestTargetsReturnHiddenAfterBeingShown()
        {
            var root = new GameObject("Effect");
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(root.transform, false);
            var renderer = visual.GetComponent<Renderer>();
            var target = root.AddComponent<AnimTarget>();

            try
            {
                target.SetVisibleAtRest(false);
                Assert.IsFalse(renderer.enabled, "A reflected beam must begin hidden.");

                target.SetVisibleAtRest(true);
                Assert.IsTrue(renderer.enabled, "The target should still be capable of becoming visible.");

                target.SetVisibleAtRest(false);
                target.ResetToRest();
                Assert.IsFalse(renderer.enabled, "Retry must hide an effect that was authored hidden at rest.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }
    }
}
