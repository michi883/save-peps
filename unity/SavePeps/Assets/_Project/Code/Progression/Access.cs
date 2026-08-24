namespace SavePeps.Progression
{
    public enum RoundAccess
    {
        Missing = 0,
        Playable = 1,
        ProgressLocked = 2,
        FullGameLocked = 3,
    }

    /// <summary>
    /// Whether a round can be played, as a pure function.
    ///
    /// This is the entire paywall. It lives here rather than inside
    /// <see cref="GameFlow"/> so that it can be tested exhaustively without a
    /// scene, a MonoBehaviour or a store — the boundary cases (the last free
    /// round, the first paid one, an entitlement refresh mid-session) are
    /// exactly the ones that are painful to reach by hand on a device, and
    /// they are the ones that decide whether the game is given away or a
    /// paying customer is locked out.
    /// </summary>
    public static class Access
    {
        /// <summary>
        /// The one authoritative access state used by gameplay and the round
        /// picker. Full-game owners may choose any authored round immediately;
        /// everyone else follows the free linear unlock and meets the
        /// purchase gate only beyond <see cref="Catalog.FreeRoundCount"/>.
        /// </summary>
        public static RoundAccess State(Catalog catalog, int round, int highestUnlocked, bool hasFullGame)
        {
            if (catalog == null || !catalog.Exists(round)) return RoundAccess.Missing;
            if (hasFullGame) return RoundAccess.Playable;
            if (catalog.IsPaid(round)) return RoundAccess.FullGameLocked;
            return round <= highestUnlocked ? RoundAccess.Playable : RoundAccess.ProgressLocked;
        }

        public static bool CanPlay(Catalog catalog, int round, int highestUnlocked, bool hasFullGame)
            => State(catalog, round, highestUnlocked, hasFullGame) == RoundAccess.Playable;

        /// <summary>
        /// True when an authored premium round would become playable through
        /// the full-game unlock. This intentionally ignores sequential
        /// progress: owners may enter any existing round immediately.
        /// </summary>
        public static bool IsPaywalled(Catalog catalog, int round, int highestUnlocked, bool hasFullGame) =>
            State(catalog, round, highestUnlocked, hasFullGame) == RoundAccess.FullGameLocked;
    }
}
