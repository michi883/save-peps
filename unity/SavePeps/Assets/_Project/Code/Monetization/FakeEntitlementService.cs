using System;
using UnityEngine;

namespace SavePeps.Monetization
{
    /// <summary>
    /// Editor and test stand-in for RevenueCat.
    ///
    /// Deliberately able to be *wrong* on demand: the states worth testing are
    /// free, owned, and a refreshed receipt that removes access. Being able to
    /// trigger each state in the editor keeps purchase gating testable without
    /// a device deployment.
    /// </summary>
    public sealed class FakeEntitlementService : MonoBehaviour, IEntitlementService, IFullGameStore
    {
        [SerializeField] private bool _hasFullGame;
        private FullGameStoreResult _nextPurchaseResult = FullGameStoreResult.Purchased;
        private bool _restorableFullGame;

        public event Action Changed;
        public event Action StoreChanged;
        public event Action<FullGameStoreResult> ActionFinished;

        public bool HasFullGame => _hasFullGame;
        public string LocalizedPrice => "TEST PRICE";
        public bool ProductReady => true;
        public bool Busy => false;

        public void Initialise() =>
            Debug.Log($"[SavePeps] Fake full-game unlock active: {_hasFullGame}. No store involved.");

        /// <summary>Flip entitlement at runtime to exercise gating both ways.</summary>
        public void SetFullGameUnlocked(bool value)
        {
            if (_hasFullGame == value) return;
            _hasFullGame = value;
            Changed?.Invoke();
        }

        public void RefreshProduct() => StoreChanged?.Invoke();

        public void PurchaseFullGame()
        {
            var result = _nextPurchaseResult;
            _nextPurchaseResult = FullGameStoreResult.Purchased;
            if (result == FullGameStoreResult.Purchased)
            {
                _restorableFullGame = true;
                SetFullGameUnlocked(true);
            }
            ActionFinished?.Invoke(result);
        }

        public void RestoreFullGame()
        {
            if (_restorableFullGame) SetFullGameUnlocked(true);
            ActionFinished?.Invoke(_restorableFullGame
                ? FullGameStoreResult.Restored
                : FullGameStoreResult.NoPurchaseFound);
        }

        /// <summary>One-shot failure/cancellation seam for purchase UI tests.</summary>
        public void SetNextPurchaseResult(FullGameStoreResult result) => _nextPurchaseResult = result;

        /// <summary>Models store ownership that the current CustomerInfo has not recovered yet.</summary>
        public void SetRestorableFullGame(bool value) => _restorableFullGame = value;

        private void OnValidate()
        {
            if (Application.isPlaying) Changed?.Invoke();
        }
    }
}
