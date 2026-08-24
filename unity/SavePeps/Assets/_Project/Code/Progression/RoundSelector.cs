using System.Collections.Generic;
using UnityEngine;

namespace SavePeps.Progression
{
    /// <summary>
    /// Chooses one useful playable round without turning the game into a
    /// recommendation system. The last round is removed when an alternative
    /// exists; new and unfinished rounds receive a small weight; everything
    /// else is ordinary random choice.
    /// </summary>
    public static class RoundSelector
    {
        public static List<int> Available(Catalog catalog, SaveData save, bool hasFullGame)
        {
            var result = new List<int>();
            if (catalog == null || save == null) return result;

            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                if (Access.CanPlay(catalog, number, save.HighestUnlockedRound, hasFullGame))
                {
                    result.Add(number);
                }
            }

            return result;
        }

        /// <param name="roll">A value in [0,1], injected so selection is deterministic in tests.</param>
        public static int Choose(Catalog catalog, SaveData save, bool hasFullGame, float roll)
        {
            var candidates = Available(catalog, save, hasFullGame);
            if (candidates.Count == 0) return 0;

            if (candidates.Count > 1 && save.LastPlayedRound > 0)
            {
                candidates.Remove(save.LastPlayedRound);
            }

            if (candidates.Count == 0) return save.LastPlayedRound;
            if (candidates.Count == 1) return candidates[0];

            var weights = new float[candidates.Count];
            var totalWeight = 0f;
            var newestSequential = Mathf.Clamp(save.HighestUnlockedRound, 1, catalog.RoundCount);

            for (var i = 0; i < candidates.Count; i++)
            {
                var number = candidates[i];
                var progress = RoundProgress.Read(catalog.Round(number), save);
                var weight = 1f;

                if (!progress.IsPerfect) weight += 2f;
                if (progress.IsUnplayed && number == newestSequential) weight += 2f;

                weights[i] = weight;
                totalWeight += weight;
            }

            var threshold = Mathf.Clamp01(roll) * totalWeight;
            var accumulated = 0f;
            for (var i = 0; i < candidates.Count; i++)
            {
                accumulated += weights[i];
                if (threshold < accumulated) return candidates[i];
            }

            return candidates[candidates.Count - 1];
        }
    }
}
