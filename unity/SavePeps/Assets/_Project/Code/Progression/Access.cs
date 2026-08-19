namespace SavePeps.Progression
{
    /// <summary>
    /// Whether a round can be played, as a pure function.
    ///
    /// This is the entire paywall. It lives here rather than inside
    /// <see cref="GameFlow"/> so that it can be tested exhaustively without a
    /// scene, a MonoBehaviour or a store — the boundary cases (the last free
    /// round, the first paid one, a subscription lapsing mid-session) are
    /// exactly the ones that are painful to reach by hand on a device, and
    /// they are the ones that decide whether the game is given away or a
    /// paying customer is locked out.
    /// </summary>
    public static class Access
    {
        public static bool CanPlay(Catalog catalog, int round, int highestUnlocked, bool subscribed)
        {
            if (catalog == null || !catalog.Exists(round)) return false;
            if (round > highestUnlocked) return false;
            return !catalog.IsPaid(round) || subscribed;
        }

        /// <summary>
        /// True when the *only* thing standing between the player and this
        /// round is the subscription. Distinguishing this from "locked" is what
        /// stops the paywall being shown for a round they simply have not
        /// reached yet — a sales pitch at the wrong moment reads as a bug.
        /// </summary>
        public static bool IsPaywalled(Catalog catalog, int round, int highestUnlocked, bool subscribed) =>
            catalog != null &&
            catalog.Exists(round) &&
            round <= highestUnlocked &&
            catalog.IsPaid(round) &&
            !subscribed;
    }
}
