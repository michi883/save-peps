using System;
using UnityEngine;

namespace SavePeps.Monetization
{
    /// <summary>
    /// The real thing, on device.
    ///
    /// Guarded by the SAVEPEPS_REVENUECAT scripting define so the project
    /// compiles whether or not the SDK is installed — the define is added
    /// once `purchases-unity` is in the project. Without that guard, pulling
    /// the SDK in becomes a blocking, all-or-nothing step, and it is on the
    /// critical path for the Play clock.
    ///
    /// Note that RevenueCat does not run in the Editor at all: in-editor this
    /// component logs and reports not-subscribed, and
    /// <see cref="FakeEntitlementService"/> is what the game actually uses
    /// there.
    /// </summary>
    public sealed class RevenueCatEntitlementService : MonoBehaviour, IEntitlementService
    {
        public event Action Changed;

        public bool IsSubscribed { get; private set; }

        public void Initialise()
        {
#if SAVEPEPS_REVENUECAT && UNITY_ANDROID && !UNITY_EDITOR
            var purchases = GetComponent<Purchases>();
            if (purchases == null)
            {
                Debug.LogError("[SavePeps] No Purchases component alongside RevenueCatEntitlementService.");
                return;
            }

            purchases.SetLogLevel(Purchases.LogLevel.Warn);
            purchases.GetCustomerInfo((info, error) =>
            {
                if (error != null)
                {
                    // Offline or store unavailable is not fatal: the SDK
                    // serves a cached entitlement, and the free rounds do not
                    // depend on this call succeeding.
                    Debug.LogWarning($"[SavePeps] RevenueCat customer info failed: {error.message}");
                    return;
                }

                Apply(info);
            });
#else
            Debug.Log("[SavePeps] RevenueCat is unavailable here; treating the player as not subscribed.");
            IsSubscribed = false;
#endif
        }

#if SAVEPEPS_REVENUECAT
        /// <summary>SDK callback: fires on purchase, restore, renewal and lapse.</summary>
        public void OnCustomerInfoUpdated(Purchases.CustomerInfo info) => Apply(info);

        private void Apply(Purchases.CustomerInfo info)
        {
            var active = info != null
                         && info.Entitlements.Active.ContainsKey(Entitlements.PepsUnlimited);

            if (active == IsSubscribed) return;
            IsSubscribed = active;
            Changed?.Invoke();
        }
#endif
    }
}
