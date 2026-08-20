using System;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.Progression
{
    /// <summary>One compact, reusable entry in the round picker.</summary>
    public sealed class RoundPickerItem : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _panel;
        [SerializeField] private Text _roundLabel;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Image[] _dots;

        private Action<int> _onSelected;

        public int RoundNumber { get; private set; }
        public RoundAccess AccessState { get; private set; }
        public bool Interactable => _button != null && _button.interactable;

        private void Awake()
        {
            if (_button != null) _button.onClick.AddListener(Select);
        }

        public void Configure(int number, RoundDefinition round, SaveData save, RoundAccess access,
            Action<int> onSelected)
        {
            RoundNumber = number;
            AccessState = access;
            _onSelected = onSelected;
            name = $"Round_{number:00}";

            if (_roundLabel != null) _roundLabel.text = $"ROUND {number}";

            var progress = RoundProgress.Read(round, save);
            if (_statusLabel != null)
            {
                _statusLabel.text = access switch
                {
                    RoundAccess.SubscriptionLocked => "PEPS UNLIMITED",
                    RoundAccess.ProgressLocked => "LOCKED",
                    _ when progress.IsUnplayed => "NEW",
                    _ when progress.IsPerfect => "PERFECT",
                    _ when progress.IsComplete => "COMPLETE",
                    _ => $"{progress.Solved} / {Mathf.Max(1, progress.Total)}",
                };
            }

            if (_button != null)
            {
                // A premium entry remains tappable so the existing paywall
                // event can own that path. Sequentially locked free content
                // is informational and cannot be skipped.
                _button.interactable = access is RoundAccess.Playable or RoundAccess.SubscriptionLocked;
            }

            if (_panel != null)
            {
                _panel.color = access switch
                {
                    RoundAccess.Playable when progress.IsUnplayed => Hex("DFF4E9"),
                    RoundAccess.Playable when progress.IsPerfect => Hex("FFF0C2"),
                    RoundAccess.Playable => Hex("F7F3E8"),
                    RoundAccess.SubscriptionLocked => Hex("E9E0F2"),
                    _ => Hex("D9D5DD"),
                };
            }

            PaintDots(round, save, access == RoundAccess.Playable ? 1f : 0.42f);
        }

        /// <summary>Also used by PlayMode tests to exercise the real item callback.</summary>
        public void Select()
        {
            if (_button != null && !_button.interactable) return;
            _onSelected?.Invoke(RoundNumber);
        }

        private void PaintDots(RoundDefinition round, SaveData save, float visibility)
        {
            for (var i = 0; i < (_dots?.Length ?? 0); i++)
            {
                var dot = _dots[i];
                if (dot == null) continue;

                var mark = save?.MarkFor(round?.RescueAt(i)?.Id) ?? Mark.None;
                dot.color = mark switch
                {
                    Mark.Star => new Color(1f, 0.71f, 0.24f, visibility),
                    Mark.Check => new Color(1f, 0.71f, 0.24f, visibility * 0.58f),
                    _ => new Color(0.24f, 0.20f, 0.33f, visibility * 0.22f),
                };
                dot.rectTransform.localScale = Vector3.one * (mark == Mark.Star ? 1.22f : 1f);
            }
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
