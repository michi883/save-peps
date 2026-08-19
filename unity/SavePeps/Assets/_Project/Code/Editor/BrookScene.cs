using SavePeps.Core;
using SavePeps.Monetization;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SavePeps.EditorTools
{
    /// <summary>
    /// Builds the Game scene: camera, lighting, HUD, round-complete card, and
    /// the components that run a rescue.
    ///
    ///   Tools > Save Peps > Build Game Scene
    ///
    /// This rebuilds the *scene* and nothing else. It reads the catalogue off
    /// disk and wires it in; it never writes content. Keeping that boundary
    /// sharp is what makes the scene safe to regenerate after a UI change now
    /// that rescues are authored in the inspector — see
    /// <see cref="BrookRescues"/> for the content side.
    /// </summary>
    public static class BrookScene
    {
        private const string ScenePath = ContentPaths.GameScenePath;

        [MenuItem("Tools/Save Peps/Build Game Scene")]
        public static void BuildGameScene()
        {
            if (AssetDatabase.LoadAssetAtPath<Catalog>(ContentPaths.CatalogPath) == null)
            {
                Debug.LogError(
                    $"[SavePeps] No catalogue at {ContentPaths.CatalogPath}. " +
                    "Run Tools > Save Peps > Seed Round One Content first.");
                return;
            }

            BuildScene(ContentPaths.CatalogPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SavePeps] Game scene rebuilt. Content untouched.");
        }

        // -------------------------------------------------------------------
        // The scene
        // -------------------------------------------------------------------

        private static void BuildScene(string catalogPath)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // Loaded *after* the new scene, never before. Opening a scene
            // unloads unused assets, and an asset held only by a local
            // variable is exactly that — the reference survives as a destroyed
            // object that assigns as null, so the scene silently ends up with
            // no catalogue and the game boots to "nothing to play".
            var catalog = AssetDatabase.LoadAssetAtPath<Catalog>(catalogPath);

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
            BuildRoundCard(hud.transform, out var cardComponent);

            var game = new GameObject("Game");
            var player = game.AddComponent<ChoreographyPlayer>();
            var router = game.AddComponent<TapRouter>();
            var feedback = game.AddComponent<Feedback>();
            var runner = game.AddComponent<RescueRunner>();

            Wire(router, "_camera", cam);
            Wire(runner, "_tapRouter", router);
            Wire(runner, "_player", player);
            Wire(runner, "_hud", hudComponent);
            Wire(runner, "_feedback", feedback);
            // GameFlow owns sequencing now; the runner is handed one rescue
            // at a time rather than playing a fixed asset at boot.
            WireBool(runner, "_autoPlayOnStart", false);

            // The editor stand-in for RevenueCat. The real SDK does not run in
            // the editor at all, so this is what makes the gating path
            // testable without a device deploy - swapped for
            // RevenueCatEntitlementService in the Android build (P4).
            var entitlements = game.AddComponent<FakeEntitlementService>();

            var flow = game.AddComponent<GameFlow>();
            Wire(flow, "_catalog", catalog);
            Wire(flow, "_runner", runner);
            Wire(flow, "_hud", hudComponent);
            Wire(flow, "_card", cardComponent);
            Wire(flow, "_entitlementSource", entitlements);

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

            // Everything the HUD owns hangs off one container so the round
            // card can hide it wholesale. On device the label, the goal and
            // the result stamp all bled through the card's wash at once.
            var hudRoot = new GameObject("HudRoot", typeof(RectTransform));
            hudRoot.transform.SetParent(canvasGo.transform, false);
            Stretch(hudRoot.GetComponent<RectTransform>());

            var roundLabel = Text(hudRoot.transform, "RoundLabel", font, 34, ink,
                new Vector2(0.5f, 1f), new Vector2(0f, -70f), new Vector2(900f, 50f));
            roundLabel.color = new Color(ink.r, ink.g, ink.b, 0.65f);

            var dots = new Image[3];
            for (var i = 0; i < 3; i++)
            {
                var dotGo = new GameObject($"Dot_{i}", typeof(Image));
                dotGo.transform.SetParent(hudRoot.transform, false);
                dotGo.GetComponent<Image>().sprite = Circle();
                var rt = dotGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 1f);
                rt.sizeDelta = new Vector2(22f, 22f);
                rt.anchoredPosition = new Vector2((i - 1) * 40f, -128f);
                dots[i] = dotGo.GetComponent<Image>();
            }

            var goal = Text(hudRoot.transform, "Goal", font, 46, ink,
                new Vector2(0.5f, 1f), new Vector2(0f, -196f), new Vector2(950f, 64f));

            // The tray only exists after a wrong answer.
            var tray = new GameObject("Tray", typeof(RectTransform));
            tray.transform.SetParent(hudRoot.transform, false);
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
            var stamp = Text(hudRoot.transform, "ResultStamp", font, 92, Hex("FF7660"),
                new Vector2(0.5f, 1f), new Vector2(0f, -330f), new Vector2(900f, 140f));

            hud = canvasGo.AddComponent<RescueHud>();
            Wire(hud, "_root", hudRoot);
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

        /// <summary>
        /// The round-complete card. It sits inside the same canvas as the HUD
        /// and covers it, because it is a beat rather than a screen — the
        /// player should feel the round land and then be back in a diorama,
        /// not navigate anywhere.
        /// </summary>
        private static void BuildRoundCard(Transform canvas, out RoundCompleteCard card)
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var ink = Hex("3D3354");

            // The component lives on an always-active holder and toggles a
            // child panel. Putting it on the panel itself would mean Awake
            // never runs while the card is hidden, and the first Show would
            // switch the object on only for Awake's Hide to switch it back
            // off — a card that never appears, from code that looks correct.
            var holder = new GameObject("RoundComplete", typeof(RectTransform));
            holder.transform.SetParent(canvas, false);
            Stretch(holder.GetComponent<RectTransform>());

            var root = new GameObject("Panel", typeof(Image));
            root.transform.SetParent(holder.transform, false);
            Stretch(root.GetComponent<RectTransform>());
            // A wash rather than a blackout: the solved diorama stays visible
            // underneath, which is most of the reward.
            // 0.88 washed the scene out almost entirely on device, which
            // defeats the point of keeping it. This is light enough to read
            // the reunion through and still hold dark text.
            root.GetComponent<Image>().color = new Color(0.97f, 0.95f, 0.91f, 0.74f);

            var title = Text(root.transform, "Title", font, 68, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 300f), new Vector2(940f, 100f));

            var subtitle = Text(root.transform, "Subtitle", font, 40, ink,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 210f), new Vector2(940f, 70f));
            subtitle.color = new Color(ink.r, ink.g, ink.b, 0.7f);

            var dots = new Image[3];
            for (var i = 0; i < dots.Length; i++)
            {
                var dotGo = new GameObject($"CardDot_{i}", typeof(Image));
                dotGo.transform.SetParent(root.transform, false);
                var rt = dotGo.GetComponent<RectTransform>();
                rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
                rt.sizeDelta = new Vector2(44f, 44f);
                rt.anchoredPosition = new Vector2((i - 1) * 90f, 100f);
                dots[i] = dotGo.GetComponent<Image>();
                dots[i].sprite = Circle();
            }

            var continueGo = new GameObject("Continue", typeof(Image), typeof(Button));
            continueGo.transform.SetParent(root.transform, false);
            var continueRt = continueGo.GetComponent<RectTransform>();
            continueRt.anchorMin = continueRt.anchorMax = new Vector2(0.5f, 0.5f);
            continueRt.sizeDelta = new Vector2(520f, 124f);
            continueRt.anchoredPosition = new Vector2(0f, -60f);
            continueGo.GetComponent<Image>().color = Hex("FFB53E");
            var continueLabel = Text(continueGo.transform, "Label", font, 46, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(500f, 110f));
            continueLabel.text = "Continue";

            // Replay is a link, not a button: it is the rarer intent and
            // should not compete with Continue for the thumb.
            var replayGo = new GameObject("Replay", typeof(Image), typeof(Button));
            replayGo.transform.SetParent(root.transform, false);
            var replayRt = replayGo.GetComponent<RectTransform>();
            replayRt.anchorMin = replayRt.anchorMax = new Vector2(0.5f, 0.5f);
            replayRt.sizeDelta = new Vector2(420f, 96f);
            replayRt.anchoredPosition = new Vector2(0f, -190f);
            replayGo.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0f);
            var replayLabel = Text(replayGo.transform, "Label", font, 36, ink,
                new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(400f, 90f));
            replayLabel.text = "Replay round";
            replayLabel.color = new Color(ink.r, ink.g, ink.b, 0.6f);

            card = holder.AddComponent<RoundCompleteCard>();
            Wire(card, "_root", root);
            Wire(card, "_title", title);
            Wire(card, "_subtitle", subtitle);
            Wire(card, "_continueButton", continueGo.GetComponent<Button>());
            Wire(card, "_continueLabel", continueLabel);
            Wire(card, "_replayButton", replayGo.GetComponent<Button>());

            var so = new SerializedObject(card);
            var dotsProp = so.FindProperty("_dots");
            dotsProp.arraySize = dots.Length;
            for (var i = 0; i < dots.Length; i++)
            {
                dotsProp.GetArrayElementAtIndex(i).objectReferenceValue = dots[i];
            }
            so.ApplyModifiedPropertiesWithoutUndo();

            root.SetActive(false);
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

        /// <summary>
        /// A round sprite for the progress dots. A bare Image with no sprite
        /// draws a square, which is what shipped to the device — "three dots"
        /// rendered as three little boxes. Knob is a built-in filled circle, so
        /// this costs no asset.
        /// </summary>
        private static Sprite Circle() =>
            UnityEditor.AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void WireBool(Object target, string field, bool value)
        {
            var so = new SerializedObject(target);
            var prop = so.FindProperty(field);
            if (prop == null)
            {
                Debug.LogError($"[SavePeps] {target.GetType().Name} has no serialized field '{field}'.");
                return;
            }

            prop.boolValue = value;
            so.ApplyModifiedPropertiesWithoutUndo();
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

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }
    }
}
