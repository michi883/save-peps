using System;
using System.Collections;
using SavePeps.Core;
using SavePeps.Monetization;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.UI
{
    /// <summary>
    /// A deliberately small purchase surface: what unlocks, the store's own
    /// localized price, one purchase action, and restore. It never invents a
    /// price or treats a cancelled Google Play sheet as an error.
    /// </summary>
    public sealed class FullGameUnlockPanel : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private RectTransform _card;
        [SerializeField] private Button _scrim;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _purchaseButton;
        [SerializeField] private Text _purchaseLabel;
        [SerializeField] private Button _restoreButton;
        [SerializeField] private Text _restoreLabel;
        [SerializeField] private Text _statusLabel;
        [SerializeField] private Feedback _feedback;

        private IFullGameStore _store;
        private Action _onDismiss;
        private Coroutine _motion;
        private bool _leaving;
        private bool _refreshReturned;

        public bool Visible => _root != null && _root.activeSelf;
        public string PrimaryLabel => _purchaseLabel != null ? _purchaseLabel.text : null;
        public string Status => _statusLabel != null ? _statusLabel.text : null;

        private void Awake()
        {
            if (_scrim != null) _scrim.onClick.AddListener(RequestClose);
            if (_closeButton != null) _closeButton.onClick.AddListener(RequestClose);
            if (_purchaseButton != null) _purchaseButton.onClick.AddListener(Purchase);
            if (_restoreButton != null) _restoreButton.onClick.AddListener(Restore);
            Hide();
        }

        public void Show(IFullGameStore store, Action onDismiss)
        {
            DetachStore();
            _store = store;
            _onDismiss = onDismiss;
            _refreshReturned = false;
            AttachStore();

            if (_statusLabel != null) _statusLabel.text = string.Empty;
            SetVisible(true);
            _leaving = false;
            Paint();

            if (_motion != null) StopCoroutine(_motion);
            _motion = StartCoroutine(UIPop.In(_card, _group, from: 0.82f, tilt: -1.5f));

            if (_store != null && !_store.ProductReady)
            {
                _store.RefreshProduct();
            }
        }

        public void RequestClose()
        {
            if (!Visible || _leaving) return;
            _leaving = true;
            _feedback?.Tap();
            if (_motion != null) StopCoroutine(_motion);
            _motion = StartCoroutine(Leave());
        }

        public void Hide()
        {
            if (_motion != null) StopCoroutine(_motion);
            _motion = null;
            _leaving = false;
            DetachStore();
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
            if (_card != null)
            {
                _card.localScale = Vector3.one;
                _card.localRotation = Quaternion.identity;
            }
            SetVisible(false);
        }

        private void Purchase()
        {
            if (_store == null || !_store.ProductReady || _store.Busy) return;
            _feedback?.Tap();
            if (_statusLabel != null) _statusLabel.text = string.Empty;
            _store.PurchaseFullGame();
            Paint();
        }

        private void Restore()
        {
            if (_store == null || _store.Busy) return;
            _feedback?.Tap();
            if (_statusLabel != null) _statusLabel.text = string.Empty;
            _store.RestoreFullGame();
            Paint();
        }

        private void HandleStoreChanged()
        {
            if (_store != null && !_store.Busy) _refreshReturned = true;
            Paint();
        }

        private void HandleActionFinished(FullGameStoreResult result)
        {
            switch (result)
            {
                case FullGameStoreResult.Purchased:
                case FullGameStoreResult.Restored:
                    // GameFlow reacts to CustomerInfo and immediately starts
                    // the pending round. Hide is a safe fallback for a result
                    // arriving after that transition.
                    DismissImmediately();
                    break;
                case FullGameStoreResult.Cancelled:
                    RequestClose();
                    break;
                case FullGameStoreResult.NoPurchaseFound:
                    SetStatus("No purchase found for this Google Play account.");
                    break;
                default:
                    SetStatus("Couldn’t complete that. Please try again.");
                    break;
            }

            PaintButtons();
        }

        private void Paint()
        {
            PaintButtons();
            if (_statusLabel == null || !string.IsNullOrEmpty(_statusLabel.text)) return;

            if (_store == null)
            {
                _statusLabel.text = "Purchases aren’t available right now.";
            }
            else if (!_store.ProductReady && _refreshReturned)
            {
                _statusLabel.text = "Couldn’t reach the store. Please try again.";
            }
        }

        private void PaintButtons()
        {
            var busy = _store is { Busy: true };
            var ready = _store is { ProductReady: true };

            if (_purchaseButton != null) _purchaseButton.interactable = ready && !busy;
            if (_restoreButton != null) _restoreButton.interactable = _store != null && !busy;
            if (_closeButton != null) _closeButton.interactable = !busy;

            if (_purchaseLabel != null)
            {
                _purchaseLabel.text = busy
                    ? "One moment…"
                    : ready
                        ? $"Unlock Full Game · {_store.LocalizedPrice}"
                        : "Loading price…";
            }
            if (_restoreLabel != null) _restoreLabel.text = busy ? "Checking…" : "Restore Purchase";
        }

        private void SetStatus(string message)
        {
            if (_statusLabel != null) _statusLabel.text = message;
        }

        private IEnumerator Leave()
        {
            yield return UIPop.Out(_card, _group, to: 0.92f);
            DismissImmediately();
        }

        private void DismissImmediately()
        {
            var dismissed = _onDismiss;
            Hide();
            dismissed?.Invoke();
        }

        private void AttachStore()
        {
            if (_store == null) return;
            _store.StoreChanged += HandleStoreChanged;
            _store.ActionFinished += HandleActionFinished;
        }

        private void DetachStore()
        {
            if (_store != null)
            {
                _store.StoreChanged -= HandleStoreChanged;
                _store.ActionFinished -= HandleActionFinished;
            }
            _store = null;
        }

        private void SetVisible(bool visible)
        {
            if (_root != null) _root.SetActive(visible);
        }
    }
}
