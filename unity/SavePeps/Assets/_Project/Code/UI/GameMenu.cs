using System;
using System.Collections;
using System.Collections.Generic;
using SavePeps.Core;
using SavePeps.Rescue;
using SavePeps.UI;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Progression
{
    /// <summary>The three title-tableau taps used by the development-only Tester Mode switch.</summary>
    public enum HomeSecretTap
    {
        Heart,
        GreenPep,
        PinkPep,
    }

    /// <summary>
    /// The navigation shell outside a rescue: one home screen and one compact
    /// picker. Gameplay still lives in the single generated Game scene, so
    /// moving between menu and rescue is an instant overlay transition rather
    /// than a scene load or a second progression system.
    ///
    /// Home stays two choices wide on purpose. What it gained in this pass is
    /// life rather than options: the title lands with a bounce, the couple
    /// keep celebrating at each other under a beating heart, and one small
    /// earned line doubles as personality and as the way into progress.
    /// </summary>
    public sealed class GameMenu : MonoBehaviour
    {
        [Header("Home")]
        [SerializeField] private GameObject _homeRoot;
        [SerializeField] private RectTransform _homeTitle;
        [SerializeField] private CanvasGroup _homeTitleGroup;
        [SerializeField] private Button _playButton;
        [SerializeField] private Text _playLabel;
        [SerializeField] private Button _chooseButton;
        [SerializeField] private Button _statButton;
        [SerializeField] private Text _statLabel;
        [SerializeField] private Button _secretHeartButton;
        [SerializeField] private Button _secretGreenPepButton;
        [SerializeField] private Button _secretPinkPepButton;
        [SerializeField] private GameObject _homeDiorama;
        [SerializeField] private Transform _homeHeart;
        [SerializeField] private Pep _homePepA;
        [SerializeField] private Pep _homePepB;

        [Header("Round picker")]
        [SerializeField] private GameObject _pickerRoot;
        [SerializeField] private Transform _pickerContent;
        [SerializeField] private RoundPickerItem _itemTemplate;
        [SerializeField] private Button _backButton;

        [Header("Feel")]
        [SerializeField] private Feedback _feedback;

        private readonly List<RoundPickerItem> _items = new();
        private Action _onPlay;
        private Action _onChoose;
        private Action _onStats;
        private Action _onBack;
        private Action<int> _onRoundSelected;
        private Coroutine _homeLife;
        private Vector3 _heartRest;
        private Vector3 _dioramaRestPosition;
        private Vector3 _dioramaRestScale = Vector3.one;
        private bool _restCaptured;
        private bool _transitioning;

        /// <summary>Raised only from the three invisible title-tableau tap areas.</summary>
        public event Action<HomeSecretTap> OnHomeSecretTapped;

        public bool HomeVisible => _homeRoot != null && _homeRoot.activeSelf;
        public bool PickerVisible => _pickerRoot != null && _pickerRoot.activeSelf;
        public IReadOnlyList<RoundPickerItem> Items => _items;

        private void Awake()
        {
            if (_playButton != null) _playButton.onClick.AddListener(HandlePlay);
            if (_chooseButton != null) _chooseButton.onClick.AddListener(HandleChoose);
            if (_statButton != null) _statButton.onClick.AddListener(HandleStats);
            if (_secretHeartButton != null)
                _secretHeartButton.onClick.AddListener(() => HandleSecretTap(HomeSecretTap.Heart));
            if (_secretGreenPepButton != null)
                _secretGreenPepButton.onClick.AddListener(() => HandleSecretTap(HomeSecretTap.GreenPep));
            if (_secretPinkPepButton != null)
                _secretPinkPepButton.onClick.AddListener(() => HandleSecretTap(HomeSecretTap.PinkPep));
            if (_backButton != null) _backButton.onClick.AddListener(HandleBack);
            CaptureRest();
            Hide();
        }

        /// <summary>
        /// Reads the authored rest pose of the title tableau, once, before
        /// anything restores it.
        ///
        /// This cannot live only in Awake. <c>GameFlow.Awake</c> calls
        /// <see cref="Hide"/> on this component, and Unity gives no ordering
        /// guarantee between two components' Awake methods — so Hide can run
        /// first, restore a rest pose that has not been read yet, and write
        /// default zeroes over the authored transform. The symptom was a heart
        /// that sat on the tabletop between the Peps' feet no matter what the
        /// scene said, with the scene file and the runtime disagreeing.
        /// </summary>
        private void CaptureRest()
        {
            if (_restCaptured) return;
            _restCaptured = true;
            if (_homeHeart != null) _heartRest = _homeHeart.localPosition;
            if (_homeDiorama == null) return;
            _dioramaRestPosition = _homeDiorama.transform.localPosition;
            _dioramaRestScale = _homeDiorama.transform.localScale;
        }

        public void ShowHome(Action onPlay, Action onChoose, Action onStats, string statLine,
            string playLabel = "PLAY")
        {
            _onPlay = onPlay;
            _onChoose = onChoose;
            _onStats = onStats;
            if (_playLabel != null) _playLabel.text = string.IsNullOrEmpty(playLabel) ? "PLAY" : playLabel;
            if (_statLabel != null) _statLabel.text = statLine ?? string.Empty;
            if (_statButton != null) _statButton.gameObject.SetActive(!string.IsNullOrEmpty(statLine));

            SetVisible(_pickerRoot, false);
            RestoreHomeDiorama();
            SetVisible(_homeDiorama, true);
            PrepareHomePeps();
            Reveal(_homeRoot);
            StartHomeLife();
        }

        public void ShowPicker(Catalog catalog, SaveData save, bool subscribed, bool showHomeDiorama,
            bool bypassAccess, Action<int> onRoundSelected, Action onBack)
        {
            _onRoundSelected = onRoundSelected;
            _onBack = onBack;
            BuildItems(catalog, save, subscribed, bypassAccess);
            SetVisible(_homeRoot, false);
            RestoreHomeDiorama();
            SetVisible(_homeDiorama, showHomeDiorama);
            StopHomeLife();
            Reveal(_pickerRoot);
        }

        public void Hide()
        {
            StopAllCoroutines();
            _homeLife = null;
            _transitioning = false;
            // Hide can land mid-duck. Without this the tableau keeps the
            // half-played exit pose and the next visit to home opens on a
            // title screen that is sunk and slightly too small.
            RestoreHomeDiorama();
            SetVisible(_homeRoot, false);
            SetVisible(_pickerRoot, false);
            SetVisible(_homeDiorama, false);
        }

        /// <summary>Android Back, routed to whichever surface is up.</summary>
        public void RequestBack()
        {
            if (PickerVisible) HandleBack();
        }

        /// <summary>
        /// The secret hit areas do not exist as interactive objects in a production player.
        /// Tester Mode enables them at boot only in the editor or a Development Build.
        /// </summary>
        public void SetTesterSecretInputEnabled(bool enabled)
        {
            SetVisible(_secretHeartButton != null ? _secretHeartButton.gameObject : null, enabled);
            SetVisible(_secretGreenPepButton != null ? _secretGreenPepButton.gameObject : null, enabled);
            SetVisible(_secretPinkPepButton != null ? _secretPinkPepButton.gameObject : null, enabled);
        }

        private void BuildItems(Catalog catalog, SaveData save, bool subscribed, bool bypassAccess)
        {
            foreach (var item in _items)
            {
                if (item == null) continue;
                item.gameObject.SetActive(false);
                Destroy(item.gameObject);
            }
            _items.Clear();

            if (_itemTemplate == null || catalog == null) return;
            ResizePickerContent(catalog.RoundCount);

            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                var item = Instantiate(_itemTemplate, _pickerContent);
                item.gameObject.SetActive(true);
                var access = bypassAccess
                    ? (subscribed || !catalog.IsPaid(number) ? RoundAccess.Playable : RoundAccess.SubscriptionLocked)
                    : Access.State(catalog, number, save.HighestUnlockedRound, subscribed);
                item.Configure(number, catalog.Round(number), save, access, HandleRoundSelected);
                _items.Add(item);
            }
        }

        private void ResizePickerContent(int itemCount)
        {
            if (_pickerContent is not RectTransform rect) return;
            var grid = _pickerContent.GetComponent<GridLayoutGroup>();
            if (grid == null) return;

            var columns = grid.constraint == GridLayoutGroup.Constraint.FixedColumnCount
                ? Mathf.Max(1, grid.constraintCount)
                : 2;
            var rows = Mathf.CeilToInt(itemCount / (float)columns);
            var height = grid.padding.top + grid.padding.bottom +
                         rows * grid.cellSize.y + Mathf.Max(0, rows - 1) * grid.spacing.y;
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, Mathf.Max(1210f, height));
            rect.anchoredPosition = Vector2.zero;
        }

        private void HandlePlay()
        {
            if (_transitioning) return;
            _feedback?.Tap();
            StopHomeLife();
            // The tableau ducks out under the fading panel, so Play hands the
            // camera to the first diorama rather than cutting between two.
            if (_homeDiorama != null) StartCoroutine(DuckHomeDiorama());
            Dismiss(_homeRoot, _onPlay);
        }

        private void HandleChoose()
        {
            if (_transitioning) return;
            _feedback?.Tap();
            StopHomeLife();
            Dismiss(_homeRoot, _onChoose);
        }

        private void HandleStats()
        {
            if (_transitioning) return;
            _feedback?.Tap();
            StopHomeLife();
            Dismiss(_homeRoot, _onStats);
        }

        private void HandleSecretTap(HomeSecretTap tap)
        {
            if (!HomeVisible || _transitioning) return;
            OnHomeSecretTapped?.Invoke(tap);
        }

        private void HandleBack()
        {
            if (_transitioning) return;
            _feedback?.Tap();
            Dismiss(_pickerRoot, _onBack);
        }

        private void HandleRoundSelected(int number)
        {
            if (_transitioning) return;
            _feedback?.Tap();
            _onRoundSelected?.Invoke(number);
        }

        private void PrepareHomePeps()
        {
            if (_homePepA == null || _homePepB == null) return;
            _homePepA.SetPartner(_homePepB.transform);
            _homePepB.SetPartner(_homePepA.transform);
            _homePepA.SetIdle(true);
            _homePepB.SetIdle(true);
            _homePepA.SetFace(PepFace.Love);
            _homePepB.SetFace(PepFace.Love);
        }

        private void RestoreHomeDiorama()
        {
            CaptureRest();
            if (_homeDiorama != null)
            {
                _homeDiorama.transform.localPosition = _dioramaRestPosition;
                _homeDiorama.transform.localScale = _dioramaRestScale;
            }

            if (_homeHeart == null) return;
            _homeHeart.localPosition = _heartRest;
            _homeHeart.localScale = Vector3.one;
            _homeHeart.localRotation = Quaternion.identity;
        }

        private void StartHomeLife()
        {
            StopHomeLife();
            if (!isActiveAndEnabled) return;
            _homeLife = StartCoroutine(HomeLifeRoutine());
        }

        private void StopHomeLife()
        {
            if (_homeLife != null) StopCoroutine(_homeLife);
            _homeLife = null;
            if (_homeHeart == null) return;
            _homeHeart.localPosition = _heartRest;
            _homeHeart.localScale = Vector3.one;
            _homeHeart.localRotation = Quaternion.identity;
        }

        /// <summary>
        /// The title screen's whole personality budget: a beating heart, and
        /// the couple taking turns celebrating at each other. Both reuse
        /// animation that already exists, so this costs one coroutine and no
        /// new art.
        /// </summary>
        private IEnumerator HomeLifeRoutine()
        {
            if (_homeTitle != null)
            {
                yield return UIPop.In(_homeTitle, _homeTitleGroup, 0.34f, 0.72f, -3.5f);
            }

            var clock = 0f;
            var beat = 1.4f;
            var turn = 0;
            while (true)
            {
                clock += Time.unscaledDeltaTime;
                if (_homeHeart != null)
                {
                    // Two-stage beat rather than a sine: a heart that pulses
                    // twice and rests reads as alive, one that breathes reads
                    // as a loading indicator.
                    var cycle = Mathf.Repeat(clock, 1.15f);
                    var pump = Mathf.Exp(-cycle * 5.2f) * Mathf.Sin(cycle * 34f);
                    _homeHeart.localScale = Vector3.one * (1f + pump * 0.16f);
                    _homeHeart.localPosition = _heartRest + Vector3.up * (Mathf.Sin(clock * 1.5f) * 0.035f);
                    _homeHeart.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(clock * 1.1f) * 5f);
                }

                if (clock >= beat)
                {
                    beat = clock + 3.4f;
                    var who = turn++ % 2 == 0 ? _homePepA : _homePepB;
                    if (who != null)
                    {
                        who.BeginCelebrate();
                        StartCoroutine(ReturnToIdle(who, 1.15f));
                    }
                }

                yield return null;
            }
        }

        private static IEnumerator ReturnToIdle(Pep pep, float after)
        {
            yield return new WaitForSecondsRealtime(after);
            if (pep == null) yield break;
            pep.SetIdle(true);
            pep.SetFace(PepFace.Love);
        }

        private IEnumerator DuckHomeDiorama()
        {
            var target = _homeDiorama.transform;
            var start = target.localPosition;
            var startScale = target.localScale;
            var elapsed = 0f;
            const float duration = 0.22f;
            while (elapsed < duration && _homeDiorama != null && _homeDiorama.activeSelf)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.In, Mathf.Clamp01(elapsed / duration));
                target.localPosition = start + new Vector3(0f, -0.55f * t, 0f);
                target.localScale = startScale * Mathf.Lerp(1f, 0.9f, t);
                yield return null;
            }

            target.localPosition = start;
            target.localScale = startScale;
        }

        private void Reveal(GameObject root)
        {
            if (_homeLife != null) StopCoroutine(_homeLife);
            _homeLife = null;
            _transitioning = false;
            if (root == null) return;
            root.SetActive(true);
            StartCoroutine(RevealRoutine(root));
        }

        private void Dismiss(GameObject root, Action action)
        {
            if (root == null)
            {
                action?.Invoke();
                return;
            }

            _transitioning = true;
            StartCoroutine(DismissRoutine(root, action));
        }

        private IEnumerator DismissRoutine(GameObject root, Action action)
        {
            var group = root.GetComponent<CanvasGroup>();
            var rect = root.transform as RectTransform;
            if (group != null)
            {
                group.interactable = false;
                group.blocksRaycasts = false;
            }

            var elapsed = 0f;
            const float duration = 0.13f;
            while (elapsed < duration && root != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.In, Mathf.Clamp01(elapsed / duration));
                if (group != null) group.alpha = 1f - t;
                if (rect != null) rect.localScale = Vector3.one * Mathf.Lerp(1f, 0.985f, t);
                yield return null;
            }

            SetVisible(root, false);
            _transitioning = false;
            action?.Invoke();
        }

        private static IEnumerator RevealRoutine(GameObject root)
        {
            var group = root.GetComponent<CanvasGroup>();
            var rect = root.transform as RectTransform;
            if (group == null)
            {
                yield break;
            }

            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
            if (rect != null) rect.localScale = Vector3.one * 0.97f;

            var elapsed = 0f;
            const float duration = 0.18f;
            while (elapsed < duration && root != null)
            {
                elapsed += Time.unscaledDeltaTime;
                var t = Easing.Evaluate(EaseKind.Out, Mathf.Clamp01(elapsed / duration));
                group.alpha = t;
                if (rect != null) rect.localScale = Vector3.one * Mathf.Lerp(0.97f, 1f, t);
                yield return null;
            }

            if (root == null) yield break;
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;
            if (rect != null) rect.localScale = Vector3.one;
        }

        private static void SetVisible(GameObject target, bool visible)
        {
            if (target != null && target.activeSelf != visible) target.SetActive(visible);
        }
    }
}
