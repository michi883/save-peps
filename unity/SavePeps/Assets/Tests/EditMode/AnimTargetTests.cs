using NUnit.Framework;
using SavePeps.Core;
using SavePeps.EditorTools;
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

        [Test]
        public void OutcomeVisibilityDoesNotOverwriteTheAuthoredRestState()
        {
            var root = new GameObject("Effect");
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(root.transform, false);
            var renderer = visual.GetComponent<Renderer>();
            var target = root.AddComponent<AnimTarget>();

            try
            {
                target.SetVisibleAtRest(true);
                target.SetVisible(false);
                Assert.IsFalse(renderer.enabled);
                target.ResetToRest();
                Assert.IsTrue(renderer.enabled, "Retry should restore a normally visible target.");

                target.SetVisibleAtRest(false);
                target.SetVisible(true);
                Assert.IsTrue(renderer.enabled);
                target.ResetToRest();
                Assert.IsFalse(renderer.enabled, "Retry should re-hide an outcome reveal.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void OutcomeVisibilitySurvivesConcurrentTransformAnimation()
        {
            var root = new GameObject("IncomingState");
            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.transform.SetParent(root.transform, false);
            var renderer = visual.GetComponent<Renderer>();
            var target = root.AddComponent<AnimTarget>();
            var move = new MoveInstance(
                new[] { new Frame { Position = Vector3.up } },
                0f, 1f, EaseKind.Out, StepKind.Fly);

            try
            {
                target.SetVisibleAtRest(false);
                target.SetVisible(true);
                target.Accumulate(new[] { move }, 0.5f);

                Assert.IsTrue(renderer.enabled,
                    "An incoming visibility-swap twin must stay visible while it also moves.");

                target.ResetToRest();
                Assert.IsFalse(renderer.enabled, "Retry must clear the outcome visibility override.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlyOffStaysVisibleUntilTheFlightFinishes()
        {
            var move = new MoveInstance(
                new[] { new Frame { Position = Vector3.right, Alpha = 0f } },
                0f, 1f, EaseKind.Out, StepKind.FlyOff);

            Assert.Less(move.Evaluate(0.5f).Alpha, 0f,
                "An opaque toy must remain drawn while it is flying away.");
            Assert.AreEqual(0f, move.Evaluate(1f).Alpha, 0.0001f,
                "It should disappear only at the destination.");
        }

        [Test]
        public void AmbientControlSettlesAndResetRestoresItsAuthoredPhase()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var motion = root.AddComponent<AmbientMotion>()
                .Configure(AmbientMode.Sway, 20f, 1f, Vector3.forward, phase: 0.25f,
                    controlId: "TestSway");

            try
            {
                motion.ResetControl();
                Assert.AreEqual("TestSway", motion.ControlId);
                Assert.Greater(Quaternion.Angle(Quaternion.identity, root.transform.localRotation), 5f);

                motion.SetActivity(0f, 0f);
                Assert.Less(Quaternion.Angle(Quaternion.identity, root.transform.localRotation), 0.01f,
                    "A stopped sway should settle to its authored rest pose.");

                motion.ResetControl();
                Assert.Greater(Quaternion.Angle(Quaternion.identity, root.transform.localRotation), 5f,
                    "Retry should restore the loop's authored phase and activity.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void FlickerAmountIsTheFractionOfTheCycleSpentOff()
        {
            var dark = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var lit = GameObject.CreatePrimitive(PrimitiveType.Cube);

            try
            {
                dark.AddComponent<AmbientMotion>()
                    .Configure(AmbientMode.Flicker, 0.8f, 1f, Vector3.up, phase: 0.2f)
                    .ResetControl();
                lit.AddComponent<AmbientMotion>()
                    .Configure(AmbientMode.Flicker, 0.8f, 1f, Vector3.up, phase: 0.9f)
                    .ResetControl();

                Assert.IsFalse(dark.GetComponent<Renderer>().enabled);
                Assert.IsTrue(lit.GetComponent<Renderer>().enabled);
            }
            finally
            {
                Object.DestroyImmediate(dark);
                Object.DestroyImmediate(lit);
            }
        }

        [Test]
        public void HeavyIsARealHapticStrength()
        {
            Assert.IsTrue(Feedback.SupportsHaptic("heavy"));
            Assert.IsFalse(Feedback.SupportsHaptic("mystery"));
        }

        [Test]
        public void OffsetClonesStepsInsteadOfMutatingTheSourceSequence()
        {
            var source = Steps.Sfx(0.25f, "click");
            var shifted = EscalationAuthoring.Offset(1f, source);

            Assert.AreEqual(0.25f, source.At, 0.0001f);
            Assert.AreEqual(1.25f, shifted[0].At, 0.0001f);
            Assert.AreNotSame(source, shifted[0]);
        }
    }
}
