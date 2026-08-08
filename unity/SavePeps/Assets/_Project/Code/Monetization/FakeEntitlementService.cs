using System;
using UnityEngine;

namespace SavePeps.Monetization
{
    /// <summary>
    /// Editor and test stand-in for RevenueCat.
    ///
    /// Deliberately able to be *wrong* on demand: the states worth testing are
    /// not-subscribed, subscribed, and lapsed-mid-session, and the last one is
    /// the one real devices reach at inconvenient moments. Being able to
    /// trigger it from the inspector is the difference between handling it and
    /// hoping.
    /// </summary>
    public sealed class FakeEntitlementService : MonoBehaviour, IEntitlementService
    {
        [SerializeField] private bool _subscribed;

        public event Action Changed;

        public bool IsSubscribed => _subscribed;

        public void Initialise() =>
            Debug.Log($"[SavePeps] Fake entitlements active (subscribed: {_subscribed}). No store involved.");

        /// <summary>Flip entitlement at runtime to exercise gating both ways.</summary>
        public void SetSubscribed(bool value)
        {
            if (_subscribed == value) return;
            _subscribed = value;
            Changed?.Invoke();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) Changed?.Invoke();
        }
    }
}
