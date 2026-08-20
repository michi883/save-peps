using System;
using System.Collections;
using System.Collections.Generic;
using SavePeps.Core;
using SavePeps.Rescue;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Progression
{
    /// <summary>
    /// The entire navigation shell: one home panel and one compact picker.
    /// Gameplay still lives in the single generated Game scene, so moving
    /// between menu and rescue is an instant overlay transition rather than a
    /// scene load or a second progression system.
    /// </summary>
    public sealed class GameMenu : MonoBehaviour
    {
        [Header("Home")]
        [SerializeField] private GameObject _homeRoot;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _chooseButton;
        [SerializeField] private GameObject _homeDiorama;
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
        private Action _onBack;
        private Action<int> _onRoundSelected;

        public bool HomeVisible => _homeRoot != null && _homeRoot.activeSelf;
        public bool PickerVisible => _pickerRoot != null && _pickerRoot.activeSelf;
        public IReadOnlyList<RoundPickerItem> Items => _items;

        private void Awake()
        {
            if (_playButton != null) _playButton.onClick.AddListener(HandlePlay);
            if (_chooseButton != null) _chooseButton.onClick.AddListener(HandleChoose);
            if (_backButton != null) _backButton.onClick.AddListener(HandleBack);
            Hide();
        }

        public void ShowHome(Action onPlay, Action onChoose)
        {
            _onPlay = onPlay;
            _onChoose = onChoose;
            SetVisible(_pickerRoot, false);
            SetVisible(_homeDiorama, true);
            PrepareHomePeps();
            Reveal(_homeRoot);
        }

        public void ShowPicker(Catalog catalog, SaveData save, bool subscribed, bool showHomeDiorama,
            Action<int> onRoundSelected, Action onBack)
        {
            _onRoundSelected = onRoundSelected;
            _onBack = onBack;
            BuildItems(catalog, save, subscribed);
            SetVisible(_homeRoot, false);
            SetVisible(_homeDiorama, showHomeDiorama);
            Reveal(_pickerRoot);
        }

        public void Hide()
        {
            StopAllCoroutines();
            SetVisible(_homeRoot, false);
            SetVisible(_pickerRoot, false);
            SetVisible(_homeDiorama, false);
        }

        private void BuildItems(Catalog catalog, SaveData save, bool subscribed)
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
                var access = Access.State(catalog, number, save.HighestUnlockedRound, subscribed);
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
            _feedback?.Tap();
            _onPlay?.Invoke();
        }

        private void HandleChoose()
        {
            _feedback?.Tap();
            _onChoose?.Invoke();
        }

        private void HandleBack()
        {
            _feedback?.Tap();
            _onBack?.Invoke();
        }

        private void HandleRoundSelected(int number)
        {
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

        private void Reveal(GameObject root)
        {
            StopAllCoroutines();
            if (root == null) return;
            root.SetActive(true);
            StartCoroutine(RevealRoutine(root));
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
