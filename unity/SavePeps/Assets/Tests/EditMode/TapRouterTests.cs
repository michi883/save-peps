using System.Reflection;
using NUnit.Framework;
using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.Tests
{
    public sealed class TapRouterTests
    {
        [Test]
        public void OverlappingHitboxesChooseTheVisualNearestTheTap()
        {
            var cameraObject = new GameObject("Test Camera");
            var routerObject = new GameObject("Router");
            var intendedObject = new GameObject("Bone");
            var frontObject = new GameObject("Fan");

            try
            {
                cameraObject.tag = "MainCamera";
                cameraObject.transform.position = new Vector3(0f, 0f, -10f);
                var camera = cameraObject.AddComponent<Camera>();
                var router = routerObject.AddComponent<TapRouter>();
                typeof(TapRouter).GetField("_camera", BindingFlags.Instance | BindingFlags.NonPublic)
                    ?.SetValue(router, camera);

                intendedObject.transform.position = new Vector3(0f, 0f, 2f);
                var intendedCollider = intendedObject.AddComponent<BoxCollider>();
                intendedCollider.center = new Vector3(0.3f, 0f, 0f);
                intendedCollider.size = new Vector3(0.2f, 0.5f, 0.5f);
                var intended = intendedObject.AddComponent<Tappable>();
                intended.ObjectId = "bone";
                var intendedVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(intendedVisual.GetComponent<Collider>());
                intendedVisual.transform.SetParent(intendedObject.transform, false);
                intendedVisual.transform.localScale = Vector3.one * 0.2f;

                // This generous collider is the only direct ray hit, while
                // its visible model is clearly off to the side. That
                // reproduces the device layout that used to let the fan steal
                // a tap visibly centred on the bone.
                frontObject.transform.position = new Vector3(0.3f, 0f, 0f);
                frontObject.AddComponent<BoxCollider>().size = new Vector3(1f, 0.5f, 0.5f);
                frontObject.AddComponent<Tappable>().ObjectId = "fan";
                var frontVisual = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Object.DestroyImmediate(frontVisual.GetComponent<Collider>());
                frontVisual.transform.SetParent(frontObject.transform, false);
                frontVisual.transform.localScale = Vector3.one * 0.2f;
                Physics.SyncTransforms();

                var screen = camera.WorldToScreenPoint(intendedVisual.transform.position);
                var pick = typeof(TapRouter).GetMethod("Pick", BindingFlags.Instance | BindingFlags.NonPublic);
                Assert.IsNotNull(pick);
                var selected = (Tappable)pick.Invoke(router, new object[] { (Vector2)screen });

                Assert.AreSame(intended, selected,
                    "A nearer overlapping collider must not steal a tap centred on another choice.");
            }
            finally
            {
                Object.DestroyImmediate(frontObject);
                Object.DestroyImmediate(intendedObject);
                Object.DestroyImmediate(routerObject);
                Object.DestroyImmediate(cameraObject);
            }
        }
    }
}
