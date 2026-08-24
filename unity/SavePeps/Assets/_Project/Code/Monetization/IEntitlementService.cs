using System;

namespace SavePeps.Monetization
{
    /// <summary>
    /// Whether the player can reach the paid rounds.
    ///
    /// This interface exists because of one hard constraint: the RevenueCat
    /// SDK does not run in the Unity Editor. Without an abstraction, every
    /// change to gating or the paywall would cost a full device deploy to
    /// test. With it, effectively all of that work happens at editor speed
    /// against <see cref="FakeEntitlementService"/>, and the real SDK is only
    /// needed to verify the purchase paths themselves.
    /// </summary>
    public interface IEntitlementService
    {
        /// <summary>True while RevenueCat reports the full-game entitlement as active.</summary>
        bool HasFullGame { get; }

        /// <summary>Fires when entitlement changes — purchase, restore, or a refreshed receipt.</summary>
        event Action Changed;

        /// <summary>Begins fetching state. Safe to call once at boot.</summary>
        void Initialise();
    }

    public static class Entitlements
    {
        /// <summary>The single entitlement id, as configured in RevenueCat.</summary>
        public const string FullGame = "save_peps_pro";
    }

    public static class StoreProducts
    {
        /// <summary>The single Google Play non-consumable attached to the full-game entitlement.</summary>
        public const string Lifetime = "lifetime";
    }
}
