using System;

namespace SavePeps.Monetization
{
    public enum FullGameStoreResult
    {
        Purchased,
        Restored,
        Cancelled,
        NoPurchaseFound,
        Failed,
    }

    /// <summary>
    /// The small store surface needed by Save Peps. Access remains owned by
    /// <see cref="IEntitlementService"/>; this interface only drives the
    /// player-initiated purchase UI.
    /// </summary>
    public interface IFullGameStore
    {
        string LocalizedPrice { get; }
        bool ProductReady { get; }
        bool Busy { get; }

        event Action StoreChanged;
        event Action<FullGameStoreResult> ActionFinished;

        void RefreshProduct();
        void PurchaseFullGame();
        void RestoreFullGame();
    }
}
