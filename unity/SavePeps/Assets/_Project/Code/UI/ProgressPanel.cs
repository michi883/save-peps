using System;
using System.Collections;
using System.Collections.Generic;
using SavePeps.Core;
using SavePeps.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.UI
{
    /// <summary>
    /// The ★/✓ collection, read back to the player.
    ///
    /// It adds no second progression system: every number here is derived from
    /// the marks already in the save, so the shelf can never disagree with the
    /// HUD. Three tiles summarise, and one row per round shows the actual
    /// three shapes — the same geometry as the HUD and the completion card,
    /// because a star the player earned should look like a star everywhere.
    /// </summary>
    public sealed class ProgressPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _panel;

        [Header("Summary")]
        [SerializeField] private RectTransform[] _tiles;
        [SerializeField] private Text _roundsValue;
        [SerializeField] private Text _perfectValue;
        [SerializeField] private Text _starsValue;

        [Header("Rounds")]
        [SerializeField] private RectTransform _content;
        [SerializeField] private ProgressRow _rowTemplate;
        [SerializeField] private ScrollRect _scroll;

        [SerializeField] private Button _backButton;
        [SerializeField] private Feedback _feedback;

        /// <summary>Title, ribbon and the three tiles, measured from the shelf's top edge.</summary>
        public const float HeaderHeight = 470f;

        private const float RowHeight = 118f;
        private const float RowSpacing = 14f;
        private const float FooterHeight = 220f;
        private const float MinViewport = 300f;
        private const float MaxViewport = 1240f;

        private readonly List<ProgressRow> _rows = new();
        private Action _onBack;
        private Coroutine _motion;
        private bool _busy;

        public bool Visible => _root != null && _root.activeSelf;
        public IReadOnlyList<ProgressRow> Rows => _rows;

        private void Awake()
        {
            if (_backButton != null) _backButton.onClick.AddListener(RequestClose);
            Hide();
        }

        public void Show(Catalog catalog, SaveData save, bool hasFullGame, Action onBack)
        {
            _onBack = onBack;
            _busy = false;

            var roundsDone = 0;
            var perfect = 0;
            var stars = 0;
            var total = catalog != null ? catalog.RoundCount : 0;

            BuildRows(catalog, save, hasFullGame, ref roundsDone, ref perfect, ref stars);

            if (_roundsValue != null) _roundsValue.text = $"{roundsDone}/{total}";
            if (_perfectValue != null) _perfectValue.text = perfect.ToString();
            if (_starsValue != null) _starsValue.text = stars.ToString();

            SetVisible(true);
            if (_scroll != null) _scroll.verticalNormalizedPosition = 1f;
            if (_motion != null) StopCoroutine(_motion);
            _motion = StartCoroutine(RevealRoutine());
        }

        public void RequestClose()
        {
            if (_busy || !Visible)
            {
                _onBack?.Invoke();
                return;
            }

            _busy = true;
            _feedback?.Tap();
            if (_motion != null) StopCoroutine(_motion);
            _motion = StartCoroutine(DismissRoutine());
        }

        public void Hide()
        {
            if (_motion != null) StopCoroutine(_motion);
            _motion = null;
            _busy = false;
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
            if (_panel != null) _panel.localScale = Vector3.one;
            SetVisible(false);
        }

        private void BuildRows(Catalog catalog, SaveData save, bool hasFullGame,
            ref int roundsDone, ref int perfect, ref int stars)
        {
            foreach (var row in _rows)
            {
                if (row != null) Destroy(row.gameObject);
            }
            _rows.Clear();

            if (_rowTemplate == null || catalog == null || _content == null) return;

            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                var round = catalog.Round(number);
                var progress = RoundProgress.Read(round, save);
                if (progress.IsComplete) roundsDone++;
                if (progress.IsPerfect) perfect++;
                stars += progress.Stars;

                var access = Access.State(catalog, number, save?.HighestUnlockedRound ?? 1, hasFullGame);
                var row = Instantiate(_rowTemplate, _content);
                row.gameObject.SetActive(true);
                row.Configure(number, round, save, access);
                _rows.Add(row);
            }

            ResizeContent(catalog.RoundCount);
        }

        /// <summary>
        /// The list is short and fixed-width, so a layout group would only add
        /// a rebuild pass. Rows are stacked directly, and the shelf itself is
        /// then cut to the number of rounds — a three-round catalogue inside a
        /// full-height panel was mostly empty cream, and a twelve-round one
        /// still has to scroll.
        /// </summary>
        private void ResizeContent(int count)
        {
            if (_content == null) return;

            for (var i = 0; i < _rows.Count; i++)
            {
                var rect = (RectTransform)_rows[i].transform;
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(0f, rect.offsetMin.y);
                rect.offsetMax = new Vector2(0f, rect.offsetMax.y);
                rect.sizeDelta = new Vector2(0f, RowHeight);
                rect.anchoredPosition = new Vector2(0f, -i * (RowHeight + RowSpacing));
            }

            var height = Mathf.Max(1f, count * RowHeight + Mathf.Max(0, count - 1) * RowSpacing);
            _content.sizeDelta = new Vector2(_content.sizeDelta.x, height);
            _content.anchoredPosition = Vector2.zero;

            var viewport = Mathf.Clamp(height, MinViewport, MaxViewport);
            if (_scroll != null)
            {
                var scrollRect = (RectTransform)_scroll.transform;
                scrollRect.sizeDelta = new Vector2(scrollRect.sizeDelta.x, viewport);
            }
            if (_panel != null)
            {
                _panel.sizeDelta = new Vector2(_panel.sizeDelta.x, HeaderHeight + viewport + FooterHeight);
            }
        }

        private IEnumerator RevealRoutine()
        {
            yield return UIPop.In(_panel, _group, 0.24f, 0.90f);

            // The three tiles land one after another. It costs nothing and it
            // is the difference between a summary appearing and a summary
            // being dealt out.
            foreach (var tile in _tiles ?? Array.Empty<RectTransform>())
            {
                if (tile == null) continue;
                StartCoroutine(UIPop.Punch(tile, 1.16f));
                yield return new WaitForSecondsRealtime(0.07f);
            }

            _motion = null;
        }

        private IEnumerator DismissRoutine()
        {
            yield return UIPop.Out(_panel, _group, 0.14f, 0.94f);
            Hide();
            _onBack?.Invoke();
        }

        private void SetVisible(bool visible)
        {
            if (_root != null && _root.activeSelf != visible) _root.SetActive(visible);
        }
    }
}
