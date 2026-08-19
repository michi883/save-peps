using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.Progression
{
    /// <summary>
    /// Three rescues, played in order.
    ///
    /// A round is authored rather than sampled. Save Pip drew random rounds
    /// from a pool, which was right for endless Reddit play and is wrong here:
    /// a paywall at "round 11" needs a stable round 11, the difficulty ramp
    /// needs hand-tuning, and a two-minute demo video needs the first ninety
    /// seconds to be identical every single time.
    /// </summary>
    [CreateAssetMenu(menuName = "Peps/Round", fileName = "Round")]
    public sealed class RoundDefinition : ScriptableObject
    {
        /// <summary>Every round is three rescues. The brief fixes this.</summary>
        public const int RescuesPerRound = 3;

        [Tooltip("1-based. Shown to the player as 'Round 4'.")]
        public int Number = 1;

        [Tooltip("Exactly three, played in this order.")]
        public RescueDefinition[] Rescues = new RescueDefinition[RescuesPerRound];

        public RescueDefinition RescueAt(int index) =>
            Rescues != null && index >= 0 && index < Rescues.Length ? Rescues[index] : null;

        /// <summary>
        /// Save Pip's round-composition rules, kept as warnings rather than
        /// errors. Two rescues in a round that want the same kind of thinking
        /// make the round feel like one puzzle asked twice — but the late
        /// rounds may legitimately break the difficulty rule, so this must not
        /// be able to fail a build.
        /// </summary>
        private void OnValidate()
        {
            Number = Mathf.Max(1, Number);

            if (Rescues == null || Rescues.Length == 0) return;

            if (Rescues.Length != RescuesPerRound)
            {
                Debug.LogWarning(
                    $"[SavePeps] Round {Number} has {Rescues.Length} rescues; a round is exactly {RescuesPerRound}.",
                    this);
            }

            for (var i = 0; i < Rescues.Length; i++)
            {
                var a = Rescues[i];
                if (a == null) continue;

                for (var j = i + 1; j < Rescues.Length; j++)
                {
                    var b = Rescues[j];
                    if (b == null) continue;

                    if (a == b)
                    {
                        Debug.LogWarning($"[SavePeps] Round {Number} plays '{a.Id}' twice.", this);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(a.Verb) && a.Verb == b.Verb)
                    {
                        Debug.LogWarning(
                            $"[SavePeps] Round {Number}: '{a.Id}' and '{b.Id}' share the verb '{a.Verb}' — " +
                            "the round will read as the same puzzle twice.", this);
                    }

                    var correctA = a.Correct?.Id;
                    var correctB = b.Correct?.Id;
                    if (!string.IsNullOrEmpty(correctA) && correctA == correctB)
                    {
                        Debug.LogWarning(
                            $"[SavePeps] Round {Number}: '{a.Id}' and '{b.Id}' are both solved by '{correctA}' — " +
                            "the second one is answerable without looking.", this);
                    }
                }
            }
        }
    }
}
