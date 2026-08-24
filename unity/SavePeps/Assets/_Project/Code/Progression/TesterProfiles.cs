using System;

namespace SavePeps.Progression
{
    /// <summary>Representative local-player states used only by Tester Mode.</summary>
    public enum TesterProfilePreset
    {
        Fresh = 0,
        Partial = 1,
        AllCompleted = 2,
        AllPerfect = 3,
    }

    /// <summary>
    /// Builds deterministic QA profiles through the same <see cref="SaveData"/>
    /// API as gameplay. Entitlement is deliberately absent: RevenueCat (or the
    /// development fake) remains the independent source of full-game ownership.
    /// </summary>
    public static class TesterProfiles
    {
        public static SaveData Create(Catalog catalog, TesterProfilePreset preset)
        {
            var save = SaveData.Fresh();
            if (catalog == null) return save;

            switch (preset)
            {
                case TesterProfilePreset.Partial:
                    ApplyPartial(catalog, save);
                    break;

                case TesterProfilePreset.AllCompleted:
                    ApplyEveryMark(catalog, save, firstTap: false);
                    break;

                case TesterProfilePreset.AllPerfect:
                    ApplyEveryMark(catalog, save, firstTap: true);
                    break;
            }

            return save;
        }

        /// <summary>
        /// Removes only sequential progression locks. Existing marks, settings,
        /// timestamps and last-played history remain untouched, and paid rounds
        /// still pass through the normal entitlement rule.
        /// </summary>
        public static void UnlockAll(Catalog catalog, SaveData save)
        {
            if (catalog == null || save == null) return;
            save.UnlockThrough(catalog.RoundCount);
        }

        private static void ApplyPartial(Catalog catalog, SaveData save)
        {
            // A realistic interrupted playthrough: round 1 is complete with a
            // mix of marks, and round 2 has two rescues solved. Round 3 must
            // remain locked because round 2 is not complete.
            var first = catalog.Round(1);
            for (var i = 0; i < RoundDefinition.RescuesPerRound; i++)
            {
                var rescue = first?.RescueAt(i);
                if (rescue != null) save.RecordSolved(rescue.Id, firstTap: i != 1);
            }

            if (catalog.RoundCount < 2) return;

            save.UnlockThrough(2);
            var second = catalog.Round(2);
            for (var i = 0; i < Math.Min(2, RoundDefinition.RescuesPerRound); i++)
            {
                var rescue = second?.RescueAt(i);
                if (rescue != null) save.RecordSolved(rescue.Id, firstTap: i == 1);
            }
            save.LastPlayedRound = 2;
        }

        private static void ApplyEveryMark(Catalog catalog, SaveData save, bool firstTap)
        {
            for (var number = 1; number <= catalog.RoundCount; number++)
            {
                var round = catalog.Round(number);
                foreach (var rescue in round?.Rescues ?? Array.Empty<SavePeps.Rescue.RescueDefinition>())
                {
                    if (rescue != null) save.RecordSolved(rescue.Id, firstTap);
                }
            }

            save.UnlockThrough(catalog.RoundCount);
            save.LastPlayedRound = catalog.RoundCount;
        }
    }
}
