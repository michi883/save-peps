using System;
using System.Collections;
using UnityEngine;

namespace SavePeps.Monetization
{
    /// <summary>
    /// Android RevenueCat adapter for the one full-game non-consumable.
    /// CustomerInfo is the only access authority; neither a successful store
    /// callback nor a local save flag can unlock content by itself.
    /// </summary>
    [RequireComponent(typeof(Purchases))]
    public sealed class RevenueCatEntitlementService : Purchases.UpdatedCustomerInfoListener,
        IEntitlementService, IFullGameStore
    {
        [SerializeField] private RevenueCatSettings _settings;

        private Purchases _purchases;
        private Purchases.Package _lifetimePackage;
        private bool _initialised;

        public event Action Changed;
        public event Action StoreChanged;
        public event Action<FullGameStoreResult> ActionFinished;

        public bool HasFullGame { get; private set; }
        public string LocalizedPrice => _lifetimePackage?.StoreProduct?.PriceString;
        public bool ProductReady => _lifetimePackage != null && !string.IsNullOrWhiteSpace(LocalizedPrice);
        public bool Busy { get; private set; }

        public void Initialise()
        {
            if (_initialised) return;
            _initialised = true;

#if UNITY_ANDROID && !UNITY_EDITOR
            // Purchases creates its native wrapper in Start. GameFlow can call
            // us from its own Start in either order, so wait one frame before
            // configuring or asking the wrapper for anything.
            StartCoroutine(InitialiseAndroid());
#else
            Debug.Log("[SavePeps] RevenueCat device service is idle outside an Android player.");
#endif
        }

#if UNITY_ANDROID && !UNITY_EDITOR
        private IEnumerator InitialiseAndroid()
        {
            yield return null;

            _purchases = GetComponent<Purchases>();
            if (_purchases == null)
            {
                Debug.LogError("[SavePeps] RevenueCat Purchases component is missing.");
                StoreChanged?.Invoke();
                yield break;
            }

            var development = Debug.isDebugBuild;
            var apiKey = _settings != null ? _settings.ApiKeyFor(development) : null;
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                var target = development ? "Test Store" : "Google Play";
                Debug.LogError($"[SavePeps] RevenueCat {target} public SDK key is not configured.");
                StoreChanged?.Invoke();
                yield break;
            }

            _purchases.useRuntimeSetup = true;
            _purchases.listener = this;
            var configuration = Purchases.PurchasesConfiguration.Builder.Init(apiKey).Build();
            _purchases.Configure(configuration);
            _purchases.SetLogLevel(development ? Purchases.LogLevel.Debug : Purchases.LogLevel.Warn);

            RefreshCustomerInfo();
            RefreshProduct();
        }
#endif

        public void RefreshProduct()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_purchases == null)
            {
                StoreChanged?.Invoke();
                return;
            }

            _purchases.GetOfferings((offerings, error) =>
            {
                if (error != null)
                {
                    Debug.LogWarning($"[SavePeps] RevenueCat offerings failed: {error.Message}");
                    _lifetimePackage = null;
                    StoreChanged?.Invoke();
                    return;
                }

                var candidate = offerings?.Current?.Lifetime;
                if (candidate?.StoreProduct?.Identifier != StoreProducts.Lifetime)
                {
                    var found = candidate?.StoreProduct?.Identifier ?? "none";
                    Debug.LogError(
                        $"[SavePeps] Current RevenueCat offering must contain lifetime product " +
                        $"'{StoreProducts.Lifetime}', but found '{found}'.");
                    _lifetimePackage = null;
                }
                else
                {
                    _lifetimePackage = candidate;
                    Debug.Log($"[SavePeps] Full-game product ready at {LocalizedPrice}.");
                }

                StoreChanged?.Invoke();
            });
#else
            StoreChanged?.Invoke();
#endif
        }

        public void PurchaseFullGame()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_purchases == null || !ProductReady || Busy)
            {
                ActionFinished?.Invoke(FullGameStoreResult.Failed);
                return;
            }

            SetBusy(true);
            _purchases.PurchasePackage(_lifetimePackage, result =>
            {
                SetBusy(false);
                if (result == null)
                {
                    Debug.LogWarning("[SavePeps] RevenueCat purchase returned no result.");
                    ActionFinished?.Invoke(FullGameStoreResult.Failed);
                    return;
                }

                if (result.UserCancelled)
                {
                    ActionFinished?.Invoke(FullGameStoreResult.Cancelled);
                    return;
                }

                if (result.Error != null)
                {
                    Debug.LogWarning($"[SavePeps] RevenueCat purchase failed: {result.Error.Message}");
                    ActionFinished?.Invoke(FullGameStoreResult.Failed);
                    return;
                }

                Apply(result.CustomerInfo);
                if (HasFullGame)
                {
                    ActionFinished?.Invoke(FullGameStoreResult.Purchased);
                }
                else
                {
                    Debug.LogError(
                        $"[SavePeps] Purchase completed without entitlement '{Entitlements.FullGame}'. " +
                        "Check the RevenueCat product attachment.");
                    ActionFinished?.Invoke(FullGameStoreResult.Failed);
                }
            });
#else
            ActionFinished?.Invoke(FullGameStoreResult.Failed);
#endif
        }

        public void RestoreFullGame()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            if (_purchases == null || Busy)
            {
                ActionFinished?.Invoke(FullGameStoreResult.Failed);
                return;
            }

            SetBusy(true);
            _purchases.RestorePurchases((info, error) =>
            {
                SetBusy(false);
                if (error != null)
                {
                    Debug.LogWarning($"[SavePeps] RevenueCat restore failed: {error.Message}");
                    ActionFinished?.Invoke(FullGameStoreResult.Failed);
                    return;
                }

                Apply(info);
                ActionFinished?.Invoke(HasFullGame
                    ? FullGameStoreResult.Restored
                    : FullGameStoreResult.NoPurchaseFound);
            });
#else
            ActionFinished?.Invoke(FullGameStoreResult.Failed);
#endif
        }

        /// <summary>SDK listener: fires for refreshed, purchased, and restored CustomerInfo.</summary>
        public override void CustomerInfoReceived(Purchases.CustomerInfo customerInfo) => Apply(customerInfo);

        private void RefreshCustomerInfo()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
            _purchases.GetCustomerInfo((info, error) =>
            {
                if (error != null)
                {
                    // Offline is not fatal. RevenueCat normally returns its
                    // cached CustomerInfo; the ten free rounds remain usable
                    // if no usable response exists.
                    Debug.LogWarning($"[SavePeps] RevenueCat customer info failed: {error.Message}");
                    return;
                }

                Apply(info);
            });
#endif
        }

        private void Apply(Purchases.CustomerInfo info)
        {
            var active = info != null
                         && info.Entitlements?.Active != null
                         && info.Entitlements.Active.ContainsKey(Entitlements.FullGame);

            if (active == HasFullGame) return;
            HasFullGame = active;
            Debug.Log($"[SavePeps] Full-game entitlement active: {HasFullGame}.");
            Changed?.Invoke();
        }

        private void SetBusy(bool value)
        {
            if (Busy == value) return;
            Busy = value;
            StoreChanged?.Invoke();
        }
    }
}
