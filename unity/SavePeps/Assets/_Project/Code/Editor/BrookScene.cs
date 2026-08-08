using SavePeps.Core;
using SavePeps.Rescue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Builds the P1 vertical slice: the Brook rescue asset and the Game scene
    /// that plays it.
    ///
    ///   Tools > Save Peps > Build Vertical Slice
    ///
    /// The rescue is authored here as code only because it is the first one
    /// and the inspector tooling is P2. The data it produces is an ordinary
    /// RescueDefinition asset — exactly what a designer would fill in by hand.
    /// </summary>
    public static class BrookScene
    {
        private const string Root = "Assets/_Project";
        private const string RescuePath = Root + "/Content/Rescues/r01_brook.asset";
        private const string ScenePath = Root + "/Scenes/Game.unity";

        [MenuItem("Tools/Save Peps/Build Vertical Slice")]
        public static void Build()
        {
            var rescue = BuildRescue();
            BuildScene(rescue);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SavePeps] Vertical slice built: r01_brook + Game scene.");
        }

        // -------------------------------------------------------------------
        // The rescue
        // -------------------------------------------------------------------

        private static RescueDefinition BuildRescue()
        {
            var rescue = AssetDatabase.LoadAssetAtPath<RescueDefinition>(RescuePath);
            if (rescue == null)
            {
                rescue = ScriptableObject.CreateInstance<RescueDefinition>();
                AssetDatabase.CreateAsset(rescue, RescuePath);
            }

            rescue.Id = "r01";
            rescue.Verb = "bridge";
            rescue.Goal = "Bring them together.";
            rescue.Difficulty = Difficulty.Easy;
            rescue.SceneDescription =
                "Two Peps stand on opposite banks of a small brook, leaning toward each other.";

            rescue.Environment = Load<GameObject>($"{Root}/Art/Environments/Diorama_Brook.prefab");
            rescue.PepAPrefab = Load<GameObject>($"{Root}/Art/Characters/Pep_A.prefab");
            rescue.PepBPrefab = Load<GameObject>($"{Root}/Art/Characters/Pep_B.prefab");
            rescue.PepAAnchor = "Anchor_PepA";
            rescue.PepBAnchor = "Anchor_PepB";
            rescue.MeetAnchor = "Anchor_Meet";

            var plank = Load<GameObject>($"{Root}/Art/Props/plank.prefab");
            var fan = Load<GameObject>($"{Root}/Art/Props/fan.prefab");
            var balloon = Load<GameObject>($"{Root}/Art/Props/balloon.prefab");

            rescue.Objects = new[]
            {
                // ---- correct: the plank bridges the brook -----------------
                new RescueObject
                {
                    Id = "plank", Prop = plank, AnchorId = "Slot_1", Label = "The wooden plank",
                    Duration = 2.9f,
                    Steps = new[]
                    {
                        Sfx(0.03f, "slide"),
                        Move(0.0f, 0.7f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.42f, 0.02f, 1.25f), amplitude: 0.35f, ease: EaseKind.Hop),
                        Sfx(0.68f, "thud"),
                        Face(0.75f, SceneRef.PepA, PepFace.Hopeful),
                        Move(0.9f, 0.95f, StepKind.Hop, SceneRef.PepA,
                            new Vector3(0f, 0f, 1.12f), amplitude: 0.16f, ease: EaseKind.Hop),
                        Haptic(0.95f, "light"),
                        Meet(1.9f, 0.75f),
                        Sfx(1.95f, "reunion"),
                    },
                },

                // ---- wrong: the fan blows the wrong way -------------------
                new RescueObject
                {
                    Id = "fan", Prop = fan, AnchorId = "Slot_2", Label = "The electric fan",
                    Quip = "Excellent breeze. Entirely the wrong direction.",
                    Duration = 2.3f,
                    Steps = new[]
                    {
                        Sfx(0.05f, "whoosh"),
                        Move(0.0f, 1.2f, StepKind.Shake, SceneRef.Self, Vector3.zero,
                            amplitude: 6f, ease: EaseKind.InOut),
                        Face(0.3f, SceneRef.PepA, PepFace.Panic),
                        Move(0.35f, 0.7f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0f, 0f, -0.30f)),
                        Move(1.25f, 0.5f, StepKind.Fly, SceneRef.PepA,
                            new Vector3(0f, 0f, 0.07f), ease: EaseKind.InOut),
                    },
                },

                // ---- wrong: the balloon makes the gap worse ---------------
                new RescueObject
                {
                    Id = "balloon", Prop = balloon, AnchorId = "Slot_3", Label = "The red balloon",
                    Quip = "Now they are even further apart. Vertically.",
                    Duration = 2.5f,
                    Steps = new[]
                    {
                        Move(0.0f, 0.6f, StepKind.Arc, SceneRef.Self,
                            new Vector3(0.45f, 0.34f, -0.68f), amplitude: 0.3f, ease: EaseKind.Hop),
                        Face(0.35f, SceneRef.PepB, PepFace.Panic),
                        Sfx(0.6f, "boing"),
                        Move(0.7f, 0.6f, StepKind.Fly, SceneRef.PepB, new Vector3(0f, 0.45f, 0f)),
                        Move(0.7f, 0.6f, StepKind.Fly, SceneRef.Self, new Vector3(0f, 0.45f, 0f)),
                        Move(1.35f, 0.9f, StepKind.FlyOff, SceneRef.PepB,
                            new Vector3(0.22f, 0.85f, 0f), ease: EaseKind.In),
                        Move(1.35f, 0.9f, StepKind.FlyOff, SceneRef.Self,
                            new Vector3(0.22f, 0.85f, 0f), ease: EaseKind.In),
                    },
                },
            };

            rescue.CorrectIndex = 0;
            EditorUtility.SetDirty(rescue);
            return rescue;
        }

        private static OutcomeStep Move(float at, float dur, StepKind kind, string target,
            Vector3 delta, float amplitude = 0f, EaseKind ease = EaseKind.Out) => new()
        {
            At = at, Duration = dur, Kind = kind, Target = target,
            Delta = delta, Amplitude = amplitude, Ease = ease, Scale = 1f,
        };

        private static OutcomeStep Face(float at, string target, PepFace face) => new()
        {
            At = at, Kind = StepKind.Face, Target = target, Param = face.ToString(), Scale = 1f,
        };

        private static OutcomeStep Sfx(float at, string id) => new()
        {
            At = at, Kind = StepKind.Sfx, Target = SceneRef.Self, Param = id, Scale = 1f,
        };

        private static OutcomeStep Haptic(float at, string strength) => new()
        {
            At = at, Kind = StepKind.Haptic, Target = SceneRef.Self, Param = strength, Scale = 1f,
        };

        private static OutcomeStep Meet(float at, float dur) => new()
        {
            At = at, Duration = dur, Kind = StepKind.Meet, Target = SceneRef.Peps, Scale = 1f,
        };

        // -------------------------------------------------------------------
        // The scene
        // -------------------------------------------------------------------

        private static void BuildScene(RescueDefinition rescue)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Fixed camera, low FOV, tilted down: the tilt-shift toy read from
            // design/palette.md. Framing is tuned for portrait 9:19.5 and
            // wants a look on the actual device — see the note in PLAN.md.
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 30f;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 40f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = Hex("B8E6F5");   // sky, from the palette
            // 40° reads as a toy on a table. The first device build used 55°
            // and looked like a floor plan — the tilt is what sells the
            // diorama, and past about 45° the objects lose their silhouettes.
            const float pitch = 40f, distance = 6.3f;
            camGo.transform.position = new Vector3(
                0f,
                0.1f + distance * Mathf.Sin(pitch * Mathf.Deg2Rad),
                -distance * Mathf.Cos(pitch * Mathf.Deg2Rad));
            camGo.transform.rotation = Quaternion.Euler(pitch, 0f, 0f);
            camGo.AddComponent<AudioListener>();

            var lightGo = new GameObject("Sun");
            var light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = Hex("FFF3CE");
            light.intensity = 1.15f;
            // No realtime shadows: blob shadows are cheaper and read better at
            // this scale (design/palette.md).
            light.shadows = LightShadows.None;
            lightGo.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = Hex("B8E6F5");
            RenderSettings.ambientEquatorColor = Hex("F7F3E8");
            RenderSettings.ambientGroundColor = Hex("E8DCC8");

            var hud = BuildHud(out var hudComponent);

            var game = new GameObject("Game");
            var player = game.AddComponent<ChoreographyPlayer>();
            var router = game.AddComponent<TapRouter>();
            var feedback = game.AddComponent<Feedback>();
            var runner = game.AddComponent<RescueRunner>();

            Wire(router, "_camera", cam);
            Wire(runner, "_rescue", rescue);
            Wire(runner, "_tapRouter", router);
            Wire(runner, "_player", player);
            Wire(runner, "_hud", hudComponent);
            Wire(runner, "_feedback", feedback);

            hudComponent.SetRound(1, 0, 3);

            EditorSceneManager.SaveScene(scene, ScenePath);
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(ScenePath, true) };
            Debug.Log($"[SavePeps] Scene saved to {ScenePath} and set as the only build scene.");
            _ = hud;
        }

        /// <summary>
        /// Minimal HUD. The P1 question is whether any of this competes with
        /// the scene, so everything is small, low-contrast and pinned to the
        /// edges — it should be possible to forget it is there.
        /// </summary>
        private static GameObject BuildHud(out RescueHud hud)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            // Without an EventSystem a Canvas renders fine and no button ever
            // fires — which on device looked exactly like a broken Try Again
            // rather than like missing plumbing.
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

            var canvasGo = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2280f);
            scaler.matchWidthOrHeight = 1f;

            var ink = Hex("3D3354");

            var roundLabel = Text(canvasGo.transform, "RoundLabel", font, 34, ink,
                new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(900f, 50f));
            roundLabel.color = new Color(ink.r, ink.g, ink.b, 0.65f);

            var dots = new Image[3];
            for (var i = 0; i < 3; i++)
            {
                var dotGo = new GameObject($"Dot_{i}", typeof(Image));
                dotGo.transform.SetParent(canvasGo.transform, false);
                var rt = dotGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(22f, 22f);
                rt.anchoredPosition = new Vector2((i - 1) * 40f, -128f);
                dots[i] = dotGo.GetComponent<Image>();
            }

            var goal = Text(canvasGo.transform, "Goal", font, 46, ink,
                new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(950f, 64f));

            // The tray only exists after a wrong answer.
            var tray = new GameObject("Tray", typeof(RectTransform));
            tray.transform.SetParent(canvasGo.transform, false);
            var trayRt = tray.GetComponent<RectTransform>();
            trayRt.anchorMin = trayRt.anchorMax = new Vector2(0.5f, 0f);
            trayRt.sizeDelta = new Vector2(1000f, 300f);
            trayRt.anchoredPosition = new Vector2(0f, 220f);

            var quip = Text(tray.transform, "Quip", font, 40, ink,
                new Vector2(0.5f, 1f), new Vector2(0f, -30f), new Vector2(940f, 90f));

            var buttonGo = new GameObject("Retry", typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(tray.transform, false);
            var buttonRt = buttonGo.GetComponent<RectTransform>();
            buttonRt.anchorMin = buttonRt.anchorMax = new Vector2(0.5f, 0f);
            buttonRt.sizeDelta = new Vector2(460f, 116f);
            buttonRt.anchoredPosition = new Vector2(0f, 40f);
            buttonGo.GetComponent<Image>().color = Hex("FFB53E");
            var buttonLabel = Text(buttonGo.transform, "Label", font, 44, Hex("3D3354"),
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(440f, 100f));
            buttonLabel.text = "Try again";

            // High enough to clear the diorama: at the centre it lands right
            // on top of the reunion, which is the one moment the player
            // should be looking at the characters and not at text.
            var stamp = Text(canvasGo.transform, "ResultStamp", font, 92, Hex("FF7660"),
                new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(900f, 140f));

            hud = canvasGo.AddComponent<RescueHud>();
            Wire(hud, "_roundLabel", roundLabel);
            Wire(hud, "_goal", goal);
            Wire(hud, "_tray", tray);
            Wire(hud, "_quip", quip);
            Wire(hud, "_retryButton", buttonGo.GetComponent<Button>());
            Wire(hud, "_resultStamp", stamp);

            var so = new SerializedObject(hud);
            var dotsProp = so.FindProperty("_dots");
            dotsProp.arraySize = dots.Length;
            for (var i = 0; i < dots.Length; i++)
            {
                dotsProp.GetArrayElementAtIndex(i).objectReferenceValue = dots[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            tray.SetActive(false);
            return canvasGo;
        }

        private static Text Text(Transform parent, string name, Font font, int size, Color color,
            Vector2 anchor, Vector2 position, Vector2 size2)
        {
            var go = new GameObject(name, typeof(Text));
            go.transform.SetParent(parent, false);
            var t = go.GetComponent<Text>();
            t.font = font;
            t.fontSize = size;
            t.color = color;
            t.alignment = TextAnchor.MiddleCenter;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = anchor;
            rt.anchoredPosition = position;
            rt.sizeDelta = size2;
            return t;
        }

        private static void Wire(Object target, string field, Object value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[SavePeps] {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }

            prop.objectReferenceValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        private static T Load<T>(string path) where T : Object
        {
            var asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null) Debug.LogError($"[SavePeps] Missing asset: {path}");
            return asset;
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
