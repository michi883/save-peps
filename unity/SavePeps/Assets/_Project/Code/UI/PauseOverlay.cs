using System;
using System.Collections;
using SavePeps.Core;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.UI
{
    /// <summary>
    /// The way out of a rescue, and the only screen in the game that exists
    /// purely to navigate.
    ///
    /// It is a bottom sheet rather than a full screen on purpose: the diorama
    /// stays visible above it, so leaving gameplay never feels like leaving
    /// the game. Settings live inline as two toggles rather than behind a
    /// sixth destination — sound and haptics are the whole of what there is to
    /// configure, and a screen that holds two switches is a hierarchy the game
    /// does not need.
    /// </summary>
    public sealed class PauseOverlay : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _sheet;

        [Tooltip("Full-screen dim behind the sheet. Tapping it resumes.")]
        [SerializeField] private Button _scrim;

        [Header("Actions")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _progressButton;
        [SerializeField] private Button _chooseButton;
        [SerializeField] private Button _homeButton;
        [SerializeField] private Button _testerToolsButton;

        [Header("Settings")]
        [SerializeField] private Button _soundToggle;
        [SerializeField] private Image _soundPanel;
        [SerializeField] private Text _soundLabel;
        [SerializeField] private Button _hapticsToggle;
        [SerializeField] private Image _hapticsPanel;
        [SerializeField] private Text _hapticsLabel;

        [SerializeField] private Feedback _feedback;

        private static readonly Color On = new(0.36f, 0.80f, 0.68f, 1f);
        // Grey rather than a paler cream: an off toggle has to read as off
        // against the sheet it sits on, not merely as a lighter shade of it.
        private static readonly Color Off = new(0.78f, 0.76f, 0.73f, 1f);

        private Action _onResume;
        private Action _onProgress;
        private Action _onChooseRound;
        private Action _onHome;
        private Action _onSettingsChanged;
        private Action _onTesterTools;
        private SaveData _save;
        private Coroutine _motion;
        private float _sheetRestY;
        private bool _restCaptured;
        private bool _busy;

        public bool Visible => _root != null && _root.activeSelf;

        private void Awake()
        {
            CaptureRest();
            if (_scrim != null) _scrim.onClick.AddListener(RequestClose);
            if (_resumeButton != null) _resumeButton.onClick.AddListener(RequestClose);
            if (_progressButton != null) _progressButton.onClick.AddListener(() => Leave(() => _onProgress?.Invoke()));
            if (_chooseButton != null) _chooseButton.onClick.AddListener(() => Leave(() => _onChooseRound?.Invoke()));
            if (_homeButton != null) _homeButton.onClick.AddListener(() => Leave(() => _onHome?.Invoke()));
            if (_testerToolsButton != null) _testerToolsButton.onClick.AddListener(() => Leave(() => _onTesterTools?.Invoke()));
            if (_soundToggle != null) _soundToggle.onClick.AddListener(ToggleSound);
            if (_hapticsToggle != null) _hapticsToggle.onClick.AddListener(ToggleHaptics);
            Hide();
        }

        public void Show(SaveData save, bool testerActive, Action onResume, Action onProgress, Action onChooseRound,
            Action onHome, Action onSettingsChanged, Action onTesterTools = null)
        {
            _save = save;
            _onResume = onResume;
            _onProgress = onProgress;
            _onChooseRound = onChooseRound;
            _onHome = onHome;
            _onSettingsChanged = onSettingsChanged;
            _onTesterTools = onTesterTools;

            if (_testerToolsButton != null)
            {
                _testerToolsButton.gameObject.SetActive(testerActive && _onTesterTools != null);
            }

            PaintSettings();
            SetVisible(true);
            _busy = false;

            if (_motion != null) StopCoroutine(_motion);
            _motion = StartCoroutine(RiseRoutine());
        }

        public void Show(SaveData save, Action onResume, Action onProgress, Action onChooseRound,
            Action onHome, Action onSettingsChanged) =>
            Show(save, testerActive: false, onResume, onProgress, onChooseRound, onHome, onSettingsChanged, onTesterTools: null);

        /// <summary>Resume, whether that came from Android Back, the scrim, or the button.</summary>
        public void RequestClose() => Leave(() => _onResume?.Invoke());

        /// <summary>
        /// Reads the sheet's authored resting height once, before anything
        /// restores it. GameFlow calls <see cref="Hide"/> from its own Awake
        /// and component Awake order is not guaranteed, so this must not be
        /// left to Awake alone — see the same note in <c>GameMenu</c>.
        /// </summary>
        private void CaptureRest()
        {
            if (_restCaptured) return;
            _restCaptured = true;
            if (_sheet != null) _sheetRestY = _sheet.anchoredPosition.y;
        }

        public void Hide()
        {
            CaptureRest();
            if (_motion != null) StopCoroutine(_motion);
            _motion = null;
            _busy = false;
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
            if (_sheet != null)
            {
                _sheet.anchoredPosition = new Vector2(_sheet.anchoredPosition.x, _sheetRestY);
                _sheet.localScale = Vector3.one;
            }
            SetVisible(false);
        }

        private void Leave(Action action)
        {
            if (_busy || !Visible)
            {
                action?.Invoke();
                return;
            }

            _busy = true;
            _feedback?.Tap();
            if (_motion != null) StopCoroutine(_motion);
            _motion = StartCoroutine(SinkRoutine(action));
        }

        private void ToggleSound()
        {
            if (_save == null) return;
            _save.SoundMuted = !_save.SoundMuted;
            // Unmuting should be audible immediately; muting obviously cannot
            // be, so the haptic carries the acknowledgement instead.
            _onSettingsChanged?.Invoke();
            _feedback?.Tap();
            PaintSettings();
            Bounce(_soundPanel);
        }

        private void ToggleHaptics()
        {
            if (_save == null) return;
            _save.HapticsOff = !_save.HapticsOff;
            _onSettingsChanged?.Invoke();
            _feedback?.Tap();
            PaintSettings();
            Bounce(_hapticsPanel);
        }

        private void Bounce(Graphic target)
        {
            if (target == null || !isActiveAndEnabled) return;
            StartCoroutine(UIPop.Punch((RectTransform)target.transform, 1.10f));
        }

        private void PaintSettings()
        {
            var sound = _save == null || !_save.SoundMuted;
            var haptics = _save == null || !_save.HapticsOff;

            if (_soundPanel != null) _soundPanel.color = sound ? On : Off;
            if (_soundLabel != null) _soundLabel.text = sound ? "SOUND ON" : "SOUND OFF";
            if (_hapticsPanel != null) _hapticsPanel.color = haptics ? On : Off;
            if (_hapticsLabel != null) _hapticsLabel.text = haptics ? "BUZZ ON" : "BUZZ OFF";
        }

        private IEnumerator RiseRoutine()
        {
            var hidden = _sheetRestY - 900f;
            if (_sheet != null) _sheet.anchoredPosition = new Vector2(_sheet.anchoredPosition.x, hidden);
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = true;
            }

            var elapsed = 0f;
            const float duration = 0.26f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var linear = Mathf.Clamp01(elapsed / duration);
                var t = Easing.Evaluate(EaseKind.Back, linear);
                if (_sheet != null)
                {
                    _sheet.anchoredPosition = new Vector2(_sheet.anchoredPosition.x,
                        Mathf.LerpUnclamped(hidden, _sheetRestY, t));
                }
                if (_group != null) _group.alpha = Easing.Evaluate(EaseKind.Out, Mathf.Clamp01(linear * 2.2f));
                yield return null;
            }

            if (_sheet != null) _sheet.anchoredPosition = new Vector2(_sheet.anchoredPosition.x, _sheetRestY);
            if (_group != null)
            {
                _group.alpha = 1f;
                _group.interactable = true;
                _group.blocksRaycasts = true;
            }
            _motion = null;
        }

        private IEnumerator SinkRoutine(Action action)
        {
            if (_group != null) _group.interactable = false;
            var startY = _sheet != null ? _sheet.anchoredPosition.y : 0f;
            var target = _sheetRestY - 900f;

            var elapsed = 0f;
            const float duration = 0.16f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.In, Mathf.Clamp01(elapsed / duration));
                if (_sheet != null)
                {
                    _sheet.anchoredPosition = new Vector2(_sheet.anchoredPosition.x,
                        Mathf.Lerp(startY, target, t));
                }
                if (_group != null) _group.alpha = 1f - t;
                yield return null;
            }

            Hide();
            action?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (_root != null && _root.activeSelf != visible) _root.SetActive(visible);
        }
    }
}
