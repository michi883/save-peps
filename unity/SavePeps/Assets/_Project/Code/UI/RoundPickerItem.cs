using System;
using SavePeps.UI;
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
        [SerializeField] private MasteryMarkGraphic[] _marks;

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
                    RoundAccess.FullGameLocked => "FULL GAME",
                    RoundAccess.ProgressLocked => "LOCKED",
                    _ when progress.IsUnplayed => "NEW",
                    _ when progress.IsPerfect => "PERFECT",
                    _ when progress.IsComplete => "COMPLETE",
                    _ => $"{progress.Solved} / {Mathf.Max(1, progress.Total)}",
                };
            }

            if (_button != null)
            {
                // Premium tiles are calls to the one unlock screen. Only
                // progression-locked and missing rounds reject the tap.
                _button.interactable = access is RoundAccess.Playable or RoundAccess.FullGameLocked;
            }

            if (_panel != null)
            {
                _panel.color = access switch
                {
                    RoundAccess.Playable when progress.IsUnplayed => Hex("DFF4E9"),
                    RoundAccess.Playable when progress.IsPerfect => Hex("FFF0C2"),
                    RoundAccess.Playable => Hex("F7F3E8"),
                    RoundAccess.FullGameLocked => Hex("E9E0F2"),
                    _ => Hex("D9D5DD"),
                };
            }

            PaintMarks(round, save, access == RoundAccess.Playable);
        }

        /// <summary>Also used by PlayMode tests to exercise the real item callback.</summary>
        public void Select()
        {
            if (_button != null && !_button.interactable) return;
            _onSelected?.Invoke(RoundNumber);
        }

        private void PaintMarks(RoundDefinition round, SaveData save, bool playable)
        {
            for (var i = 0; i < (_marks?.Length ?? 0); i++)
            {
                var view = _marks[i];
                if (view == null) continue;

                var mark = save?.MarkFor(round?.RescueAt(i)?.Id) ?? Mark.None;
                view.SetState(mark switch
                {
                    Mark.Star => MasteryMarkState.Star,
                    Mark.Check => MasteryMarkState.Check,
                    _ => MasteryMarkState.Empty,
                });
                view.canvasRenderer.SetAlpha(playable ? 1f : 0.42f);
            }
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
