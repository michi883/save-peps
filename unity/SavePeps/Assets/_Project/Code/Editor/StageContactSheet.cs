using System.IO;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEngine;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Renders every rescue's opening frame to a PNG, in its own world's light
    /// and framing.
    ///
    /// This exists because of the specific failure the twelve-world revamp was
    /// fixing: the old catalogue's problem was invisible in any individual
    /// asset and only obvious when you put the rounds side by side. A contact
    /// sheet is the cheapest possible version of the acceptance test the brief
    /// actually asks for — hide the HUD, look at one frame, and know which
    /// round it is.
    ///
    /// It is not a substitute for the device pass (AGENTS §6): it uses the
    /// authored framing but not the phone's aspect, gamma or panel. It is what
    /// catches a Pep standing inside a wall before anyone plugs a phone in.
    ///
    ///   Tools > Save Peps > Render Stage Contact Sheet
    ///
    /// From the command line this is the one batchmode tool that must NOT be
    /// given -nographics: that forces a Null GfxDevice, every camera draw is a
    /// no-op, and all 36 PNGs come back as uniform 0xCDCDCD (uninitialised
    /// render-target memory) with no error in the log.
    /// </summary>
    public static class StageContactSheet
    {
        private const int Width = 540;
        private const int Height = 1140;   // the Pixel 4's 1080x2280, halved

        [MenuItem("Tools/Save Peps/Render Stage Contact Sheet")]
        public static void Render()
        {
            var outputDir = Environment("SAVEPEPS_SHEET_DIR") ?? "Temp/StageSheet";
            Directory.CreateDirectory(outputDir);

            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath);
            if (catalog == null)
            {
                Debug.LogError($"[SavePeps] No catalogue at {ContentPaths.CatalogPath}.");
                return;
            }

            var rig = new GameObject("SheetRig");
            var cam = rig.AddComponent<Camera>();
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 40f;

            var sunGo = new GameObject("SheetSun");
            var sun = sunGo.AddComponent<Light>();
            sun.type = LightType.Directional;
            sun.shadows = LightShadows.None;

            var fillGo = new GameObject("SheetFill");
            var fill = fillGo.AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.shadows = LightShadows.None;

            var rendered = 0;
            try
            {
                for (var number = 1; number <= catalog.RoundCount; number++)
                {
                    var round = catalog.Round(number);
                    for (var slot = 0; slot < RoundDefinition.RescuesPerRound; slot++)
                    {
                        var rescue = round?.RescueAt(slot);
                        if (rescue?.Environment == null) continue;

                        var path = Path.Combine(outputDir, $"{rescue.Id}_{rescue.Verb}.png");
                        Shoot(rescue, cam, sun, fill, path);
                        rendered++;
                    }
                }
            }
            finally
            {
                Object.DestroyImmediate(rig);
                Object.DestroyImmediate(sunGo);
                Object.DestroyImmediate(fillGo);
                RenderSettings.fog = false;
            }

            Debug.Log($"[SavePeps] Contact sheet: {rendered} stages written to {Path.GetFullPath(outputDir)}.");
        }

        private static void Shoot(RescueDefinition rescue, Camera cam, Light sun, Light fill, string path)
        {
            var stage = (GameObject)PrefabUtility.InstantiatePrefab(rescue.Environment);
            var spawned = new System.Collections.Generic.List<GameObject> { stage };

            try
            {
                var anchors = new System.Collections.Generic.Dictionary<string, Transform>();
                foreach (var t in stage.GetComponentsInChildren<Transform>(includeInactive: true))
                {
                    anchors[t.name] = t;
                }

                Place(rescue.PepAPrefab, anchors, rescue.PepAAnchor, spawned);
                Place(rescue.PepBPrefab, anchors, rescue.PepBAnchor, spawned);
                foreach (var obj in rescue.Objects ?? System.Array.Empty<RescueObject>())
                {
                    if (obj != null) Place(obj.Prop, anchors, obj.AnchorId, spawned);
                }

                ApplyAtmosphere(stage.GetComponent<DioramaAtmosphere>(), cam, sun, fill);

                var rt = new RenderTexture(Width, Height, 24, RenderTextureFormat.ARGB32)
                {
                    antiAliasing = 4,
                };
                cam.targetTexture = rt;
                cam.Render();
                cam.Render();

                var previous = RenderTexture.active;
                RenderTexture.active = rt;
                var shot = new Texture2D(Width, Height, TextureFormat.RGB24, mipChain: false);
                shot.ReadPixels(new Rect(0, 0, Width, Height), 0, 0);
                shot.Apply();
                RenderTexture.active = previous;

                File.WriteAllBytes(path, shot.EncodeToPNG());

                cam.targetTexture = null;
                RenderTexture.active = null;
                Object.DestroyImmediate(shot);
                rt.Release();
                Object.DestroyImmediate(rt);
            }
            finally
            {
                foreach (var go in spawned)
                {
                    if (go != null) Object.DestroyImmediate(go);
                }
            }
        }

        private static void Place(GameObject prefab, System.Collections.Generic.IReadOnlyDictionary<string, Transform> anchors,
            string anchorId, System.Collections.Generic.List<GameObject> spawned)
        {
            if (prefab == null || string.IsNullOrEmpty(anchorId)) return;
            if (!anchors.TryGetValue(anchorId, out var anchor)) return;

            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.SetParent(anchor, worldPositionStays: false);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            spawned.Add(instance);
        }

        private static void ApplyAtmosphere(DioramaAtmosphere a, Camera cam, Light sun, Light fill)
        {
            if (a == null)
            {
                Debug.LogWarning("[SavePeps] A stage has no atmosphere; the sheet frame will be wrong.");
                return;
            }

            a.Framing(out var position, out var rotation, out var fov);
            cam.transform.SetPositionAndRotation(position, rotation);
            cam.fieldOfView = fov;
            cam.backgroundColor = a.Sky;

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = a.AmbientSky;
            RenderSettings.ambientEquatorColor = a.AmbientEquator;
            RenderSettings.ambientGroundColor = a.AmbientGround;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = a.Fog;
            RenderSettings.fogDensity = a.UseFog ? a.FogDensity : 0f;
            RenderSettings.fog = a.UseFog;

            sun.color = a.SunColor;
            sun.intensity = a.SunIntensity;
            sun.transform.rotation = Quaternion.Euler(a.SunAngles);

            fill.color = a.FillColor;
            fill.intensity = a.FillIntensity;
            fill.transform.rotation = Quaternion.Euler(a.FillAngles);
        }

        private static string Environment(string key)
        {
            var value = System.Environment.GetEnvironmentVariable(key);
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}
