using SavePeps.Progression;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.UI
{
    /// <summary>
    /// One round on the progress shelf: its number, what it is worth, and the
    /// three marks themselves. Deliberately not a button — choosing a round
    /// already has a screen, and making this one do both jobs is how a small
    /// game grows two ways to do the same thing.
    /// </summary>
    public sealed class ProgressRow : MonoBehaviour
    {
        [SerializeField] private Image _panel;
        [SerializeField] private Text _label;
        [SerializeField] private Text _status;
        [SerializeField] private MasteryMarkGraphic[] _marks;

        public int RoundNumber { get; private set; }

        public void Configure(int number, RoundDefinition round, SaveData save, RoundAccess access)
        {
            RoundNumber = number;
            name = $"Progress_{number:00}";

            if (_label != null) _label.text = $"ROUND {number}";

            var progress = RoundProgress.Read(round, save);
            var locked = !progress.IsComplete && (access is RoundAccess.ProgressLocked or RoundAccess.FullGameLocked);

            if (_status != null)
            {
                _status.text = progress.IsPerfect ? "PERFECT"
                    : progress.IsComplete ? "SAVED"
                    : access switch
                    {
                        RoundAccess.FullGameLocked => "FULL GAME",
                        RoundAccess.ProgressLocked => "NOT YET",
                        _ when progress.IsUnplayed => "UNPLAYED",
                        _ => $"{progress.Solved} OF {Mathf.Max(1, progress.Total)}",
                    };
            }

            if (_panel != null)
            {
                _panel.color = progress.IsPerfect
                    ? Hex("FFF0C2")
                    : locked ? Hex("E4E0E8") : Hex("FFFBEE");
            }

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
                view.canvasRenderer.SetAlpha(locked ? 0.45f : 1f);
            }
        }

        private static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var color);
            return color;
        }
    }
}
