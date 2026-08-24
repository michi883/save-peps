using UnityEngine;

namespace SavePeps.Progression
{
    /// <summary>
    /// The ordered list of rounds, and the one number that decides where the
    /// paywall sits.
    ///
    /// <see cref="FreeRoundCount"/> is a config value rather than a constant on
    /// purpose (decision D3). The brief fixes the gate at round 11, which makes
    /// 30 polished rescues a hard floor before the paywall is even reachable.
    /// If content slips in release week, moving the gate is a one-field change
    /// here instead of a code change — cheap insurance that costs nothing to
    /// carry.
    /// </summary>
    [CreateAssetMenu(menuName = "Peps/Catalog", fileName = "Catalog")]
    public sealed class Catalog : ScriptableObject
    {
        public const int DefaultFreeRoundCount = 10;

        [Tooltip("Rounds in play order. Index 0 is round 1.")]
        public RoundDefinition[] Rounds = new RoundDefinition[0];

        [Tooltip("Rounds playable without the full-game unlock. Keep this at 10 for release.")]
        [Min(1)]
        public int FreeRoundCount = DefaultFreeRoundCount;

        public int RoundCount => Rounds?.Length ?? 0;

        /// <summary>Rounds are 1-based everywhere the player can see them.</summary>
        public RoundDefinition Round(int number) =>
            Rounds != null && number >= 1 && number <= Rounds.Length ? Rounds[number - 1] : null;

        public bool Exists(int number) => Round(number) != null;

        /// <summary>True if this round sits behind the lifetime unlock.</summary>
        public bool IsPaid(int number) => number > FreeRoundCount;

        private void OnValidate()
        {
            FreeRoundCount = Mathf.Max(1, FreeRoundCount);

            if (Rounds == null) return;

            // The round number on each asset is what the HUD prints, so a
            // mismatch between list order and Number shows up as the game
            // lying about where the player is.
            for (var i = 0; i < Rounds.Length; i++)
            {
                var round = Rounds[i];
                if (round == null)
                {
                    Debug.LogWarning($"[SavePeps] Catalog slot {i + 1} is empty.", this);
                    continue;
                }

                if (round.Number != i + 1)
                {
                    Debug.LogWarning(
                        $"[SavePeps] '{round.name}' is at catalog position {i + 1} but numbered {round.Number}.",
                        this);
                }
            }

            // FreeRoundCount may intentionally exceed RoundCount while the
            // catalogue is still being authored. The product boundary stays
            // at round 11; missing future free rounds are not premium content.
        }
    }
}
