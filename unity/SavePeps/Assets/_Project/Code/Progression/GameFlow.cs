using System;
using System.Collections;
using SavePeps.Monetization;
using SavePeps.Rescue;
using UnityEngine;

namespace SavePeps.Progression
{
    /// <summary>
    /// Sequences the game: rescue, rescue, rescue, round complete, next round.
    ///
    /// This is the only component that knows about progression, and it holds
    /// the entire gating rule (<see cref="CanPlay"/>). Keeping that in one
    /// readable method matters more than it looks — a paywall that is checked
    /// in three places is a paywall with three chances to be wrong, and the
    /// failure mode is either giving the game away or blocking a paying
    /// customer.
    /// </summary>
    public sealed class GameFlow : MonoBehaviour
    {
        [Header("Content")]
        [SerializeField] private Catalog _catalog;

        [Header("Scene")]
        [SerializeField] private RescueRunner _runner;
        [SerializeField] private RescueHud _hud;
        [SerializeField] private RoundCompleteCard _card;

        [Tooltip("Anything implementing IEntitlementService: the fake in the editor, RevenueCat on device.")]
        [SerializeField] private MonoBehaviour _entitlementSource;

        [Header("Pacing")]
        [Tooltip("Seconds the reunion is left on screen before the next rescue drops in.")]
        [SerializeField, Range(0.5f, 5f)] private float _winDwell = 2.4f;

        private IEntitlementService _entitlements;
        private SaveData _save;
        private int _roundNumber;
        private int _rescueIndex;
        private bool _awaitingEntitlement;

        /// <summary>Raised with the round the player just tried to reach.</summary>
        public event Action<int> OnPaywallRequested;

        /// <summary>Raised when the player finishes the last authored round.</summary>
        public event Action OnCatalogComplete;

        public SaveData Save => _save;
        public int CurrentRound => _roundNumber;

        // -------------------------------------------------------------------
        // Boot
        // -------------------------------------------------------------------

        private void Awake()
        {
            _entitlements = _entitlementSource as IEntitlementService;
            if (_entitlementSource != null && _entitlements == null)
            {
                Debug.LogError(
                    $"[SavePeps] '{_entitlementSource.GetType().Name}' is wired as the entitlement source but does " +
                    "not implement IEntitlementService. Paid rounds will stay locked.");
            }

            _save = SaveStore.Load();
        }

        private void OnEnable()
        {
            if (_runner != null) _runner.OnSolved += HandleSolved;
            if (_entitlements != null) _entitlements.Changed += HandleEntitlementChanged;
        }

        private void OnDisable()
        {
            if (_runner != null) _runner.OnSolved -= HandleSolved;
            if (_entitlements != null) _entitlements.Changed -= HandleEntitlementChanged;
        }

        private void Start()
        {
            _entitlements?.Initialise();

            if (_catalog == null || _catalog.RoundCount == 0)
            {
                Debug.LogError("[SavePeps] GameFlow has no catalog. Nothing to play.");
                return;
            }

            // Resume where they left off, but never past what has actually been
            // authored — a save from a build with more rounds must not strand
            // the player on a round that does not exist.
            var resume = Mathf.Clamp(_save.HighestUnlockedRound, 1, _catalog.RoundCount);
            PlayRound(resume);
        }

        /// <summary>
        /// Persisting on pause as well as on completion: Android can kill a
        /// backgrounded app without further warning, and losing a finished
        /// round to that is exactly the kind of small betrayal that gets a
        /// game uninstalled.
        /// </summary>
        private void OnApplicationPause(bool paused)
        {
            if (paused) Persist();
        }

        private void OnApplicationQuit() => Persist();

        private void Persist() => SaveStore.Save(_save);

        // -------------------------------------------------------------------
        // The gate — the only access rule in the game
        // -------------------------------------------------------------------

        public bool CanPlay(int round) =>
            Access.CanPlay(_catalog, round, _save.HighestUnlockedRound, IsSubscribed);

        private bool IsSubscribed => _entitlements is { IsSubscribed: true };

        // -------------------------------------------------------------------
        // Rounds
        // -------------------------------------------------------------------

        public void PlayRound(int number)
        {
            if (!_catalog.Exists(number))
            {
                OnCatalogComplete?.Invoke();
                _hud?.SetVisible(false);
                _card?.ShowOutOfContent();
                return;
            }

            if (!CanPlay(number))
            {
                // Locked rather than unpurchased is a bug, not a sales moment:
                // only surface the paywall when the subscription is genuinely
                // what stands between the player and the round.
                if (Access.IsPaywalled(_catalog, number, _save.HighestUnlockedRound, IsSubscribed))
                {
                    _awaitingEntitlement = true;
                    OnPaywallRequested?.Invoke(number);
                }
                else
                {
                    Debug.LogWarning($"[SavePeps] Round {number} is not unlocked yet.");
                }
                return;
            }

            _awaitingEntitlement = false;
            _roundNumber = number;
            _rescueIndex = 0;
            _card?.Hide();
            StartRescue();
        }

        private void StartRescue()
        {
            var round = _catalog.Round(_roundNumber);
            var rescue = round?.RescueAt(_rescueIndex);
            if (rescue == null)
            {
                Debug.LogError($"[SavePeps] Round {_roundNumber} has no rescue at index {_rescueIndex}.");
                return;
            }

            _hud?.SetVisible(true);
            RefreshDots();
            _runner.Load(rescue);
        }

        private void HandleSolved(bool firstTap)
        {
            var rescue = _runner.Current;
            if (rescue != null) _save.RecordSolved(rescue.Id, firstTap);

            // Persist per rescue, not per round: the reunion is the moment the
            // player feels they earned something, and it should survive a kill
            // three seconds later.
            Persist();
            RefreshDots();

            StartCoroutine(AdvanceAfterDwell());
        }

        private IEnumerator AdvanceAfterDwell()
        {
            yield return new WaitForSeconds(_winDwell);

            _rescueIndex++;
            if (_rescueIndex < RoundDefinition.RescuesPerRound &&
                _catalog.Round(_roundNumber)?.RescueAt(_rescueIndex) != null)
            {
                StartRescue();
            }
            else
            {
                CompleteRound();
            }
        }

        private void CompleteRound()
        {
            _save.UnlockThrough(_roundNumber + 1);
            Persist();

            // The diorama deliberately stays: the card washes over the scene
            // the player just solved, and that reunion is most of the reward.
            // Tearing it down here left the card floating on an empty
            // background, which read as a broken screen on device. The next
            // rescue's Load() clears it.
            _hud?.SetVisible(false);

            var round = _catalog.Round(_roundNumber);
            var marks = new Mark[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < marks.Length; i++)
            {
                marks[i] = _save.MarkFor(round?.RescueAt(i)?.Id);
            }

            if (_card != null) _card.Show(_roundNumber, marks, Continue, ReplayRound);
            else Continue();
        }

        /// <summary>Advances past the round-complete card.</summary>
        public void Continue() => PlayRound(_roundNumber + 1);

        /// <summary>The whole of the replay story — no level select, per PLAN §8.</summary>
        public void ReplayRound() => PlayRound(_roundNumber);

        /// <summary>
        /// A purchase or restore finished. If the player was stopped at the
        /// paywall, take them straight into the round they were reaching for —
        /// making someone tap through to the thing they just paid for is a
        /// small insult at the worst possible moment.
        /// </summary>
        private void HandleEntitlementChanged()
        {
            if (!_awaitingEntitlement || !IsSubscribed) return;
            PlayRound(_roundNumber + 1);
        }

        private void RefreshDots()
        {
            if (_hud == null) return;

            _hud.SetRound(_roundNumber, _rescueIndex, RoundDefinition.RescuesPerRound);

            var round = _catalog.Round(_roundNumber);
            var states = new DotState[RoundDefinition.RescuesPerRound];
            for (var i = 0; i < states.Length; i++)
            {
                var mark = _save.MarkFor(round?.RescueAt(i)?.Id);
                states[i] = mark switch
                {
                    Mark.Star => DotState.Star,
                    Mark.Check => DotState.Check,
                    _ => i == _rescueIndex ? DotState.Current : DotState.Upcoming,
                };
            }

            _hud.SetDots(states);
        }
    }
}
