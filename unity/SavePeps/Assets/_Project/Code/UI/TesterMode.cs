using System;
using System.Collections;
using SavePeps.Monetization;
using SavePeps.Progression;
using SavePeps.Rescue;
using UnityEngine;
using UnityEngine.UI;

namespace SavePeps.UI
{
    /// <summary>
    /// A clean development mode layered over the ordinary game flow.
    ///
    /// Tester Tools answers three questions with zero fluff:
    /// 1. Where do I want to go? (GO TO: Round & Rescue -> PLAY RESCUE)
    /// 2. Do I want to test Free or Peps Unlimited? (ACCESS: FREE | PEPS UNLIMITED)
    /// 3. Do I want to erase my progress? (PROFILE: CLEAR ALL PROGRESS)
    /// </summary>
    public sealed class TesterMode : MonoBehaviour
    {
        private static readonly HomeSecretTap[] SecretSequence =
        {
            HomeSecretTap.Heart,
            HomeSecretTap.GreenPep,
            HomeSecretTap.PinkPep,
            HomeSecretTap.Heart,
            HomeSecretTap.GreenPep,
            HomeSecretTap.PinkPep,
            HomeSecretTap.Heart,
        };

        [Header("Active indicator")]
        [SerializeField] private GameObject _indicatorRoot;
        [SerializeField] private Button _indicatorButton;
        [SerializeField] private Text _indicatorLabel;

        [Header("Panel")]
        [SerializeField] private GameObject _root;
        [SerializeField] private CanvasGroup _group;
        [SerializeField] private Button _closeButton;

        [Header("Go To")]
        [SerializeField] private Button[] _roundButtons = Array.Empty<Button>();
        [SerializeField] private Button[] _rescueButtons = Array.Empty<Button>();
        [SerializeField] private Button _playRescueButton;
        [SerializeField] private Text _goToSelectionSummary;

        [Header("Access")]
        [SerializeField] private Button _freeButton;
        [SerializeField] private Button _unlimitedButton;
        [SerializeField] private Text _freeLabel;
        [SerializeField] private Text _unlimitedLabel;

        [Header("Profile")]
        [SerializeField] private Button _clearProgressButton;
        [SerializeField] private Text _clearProgressLabel;
        [SerializeField] private Button _cancelClearButton;

        [Header("Runtime")]
        [SerializeField] private GameFlow _flow;
        [SerializeField] private GameMenu _menu;
        [SerializeField] private RescueRunner _runner;
        [SerializeField] private FakeEntitlementService _fakeEntitlements;

        private static readonly Color Selected = new(1f, 0.71f, 0.24f, 1f); // Gold #FFB53E
        private static readonly Color Ordinary = new(1f, 0.98f, 0.93f, 1f); // Cream #FFFBEE
        private static readonly Color InactiveAccess = new(0.94f, 0.89f, 0.82f, 1f); // Muted cream #F0E3D2
        private static readonly Color ActiveAccess = new(0.36f, 0.80f, 0.68f, 1f); // Mint #5CCCAE
        private static readonly Color NormalDestructive = new(0.96f, 0.91f, 0.91f, 1f);
        private static readonly Color ConfirmDestructive = new(0.88f, 0.33f, 0.33f, 1f); // Alert Coral #E05353
        private static readonly Color Ink = new(0.24f, 0.20f, 0.33f, 1f); // #3D3354

        private int _selectedRound = 1;
        private int _selectedRescue = 0; // 0-indexed
        private int _secretIndex;
        private bool _active;
        private bool _busy;
        private bool _confirmingClear;
        private float _confirmExpiresTime;
        private float _nextPaint;

        /// <summary>True only in the editor or a player built with Development Build enabled.</summary>
        public static bool Available => Application.isEditor || Debug.isDebugBuild;

        /// <summary>Session-only; deliberately false on every process start.</summary>
        public bool Active => Available && _active;

        public bool Visible => Active && _root != null && _root.activeSelf;
        public int SelectedRound => _selectedRound;
        public int SelectedRescueIndex => _selectedRescue;
        public string HomePlayLabel => "PLAY";

        public bool TryGetPlayTarget(out int round, out int rescueIndex)
        {
            round = _selectedRound;
            rescueIndex = _selectedRescue;
            return Active && _flow?.Catalog != null && _flow.Catalog.Exists(round) &&
                   _flow.Catalog.Round(round)?.RescueAt(rescueIndex) != null;
        }

        private void Awake()
        {
            _active = false;
            _secretIndex = 0;
            _confirmingClear = false;
            SetVisible(_root, false);
            SetVisible(_indicatorRoot, false);
            _menu?.SetTesterSecretInputEnabled(Available);

            if (!Available)
            {
                enabled = false;
                return;
            }

            _indicatorButton?.onClick.AddListener(Open);
            _closeButton?.onClick.AddListener(RequestClose);
            _playRescueButton?.onClick.AddListener(PlayRescue);
            _freeButton?.onClick.AddListener(() => SetSubscribed(false));
            _unlimitedButton?.onClick.AddListener(() => SetSubscribed(true));
            _clearProgressButton?.onClick.AddListener(HandleClearProgressClicked);
            _cancelClearButton?.onClick.AddListener(CancelClearProgress);

            for (var i = 0; i < _roundButtons.Length; i++)
            {
                var roundNumber = i + 1;
                _roundButtons[i]?.onClick.AddListener(() => SelectRound(roundNumber));
            }

            for (var i = 0; i < _rescueButtons.Length; i++)
            {
                var rescueIndex = i;
                _rescueButtons[i]?.onClick.AddListener(() => SelectRescue(rescueIndex));
            }

            Paint();
        }

        private void OnEnable()
        {
            if (_menu != null) _menu.OnHomeSecretTapped += HandleSecretTap;
            if (_fakeEntitlements != null) _fakeEntitlements.Changed += Paint;
        }

        private void OnDisable()
        {
            if (_menu != null) _menu.OnHomeSecretTapped -= HandleSecretTap;
            if (_fakeEntitlements != null) _fakeEntitlements.Changed -= Paint;
        }

        private void Update()
        {
            if (_confirmingClear && Time.unscaledTime > _confirmExpiresTime)
            {
                _confirmingClear = false;
                Paint();
            }

            if (Time.unscaledTime < _nextPaint) return;
            _nextPaint = Time.unscaledTime + 0.25f;
            Paint();
        }

        private void HandleSecretTap(HomeSecretTap tap)
        {
            if (!Available || _busy) return;

            if (tap == SecretSequence[_secretIndex])
            {
                _secretIndex++;
                if (_secretIndex < SecretSequence.Length) return;

                _secretIndex = 0;
                if (Active) Deactivate();
                else Activate();
                return;
            }

            _secretIndex = tap == HomeSecretTap.Heart ? 1 : 0;
        }

        private void Activate()
        {
            _active = true;
            if (_flow?.Catalog != null && _flow.CurrentRound > 0 && _flow.Catalog.Exists(_flow.CurrentRound))
            {
                _selectedRound = _flow.CurrentRound;
                _selectedRescue = Mathf.Clamp(_flow.CurrentRescueIndex, 0, RoundDefinition.RescuesPerRound - 1);
            }

            SetVisible(_indicatorRoot, true);
            Debug.Log("[SavePeps] Tester Mode active. Restrictions removed.");
        }

        private void Deactivate()
        {
            _active = false;
            _busy = false;
            _confirmingClear = false;
            HidePanel();
            SetVisible(_indicatorRoot, false);
            _runner?.SuspendInput(false);
            _flow?.EndTesterSession();
            Debug.Log("[SavePeps] User Mode active. Normal navigation and gating restored.");
        }

        public void Open()
        {
            if (!Active || _busy || _flow == null) return;

            _confirmingClear = false;
            _runner?.SuspendInput(true);
            SetVisible(_indicatorRoot, false);
            SetVisible(_root, true);
            if (_group != null)
            {
                _group.alpha = 1f;
                _group.interactable = true;
                _group.blocksRaycasts = true;
            }
            Paint();
            Debug.Log("[SavePeps] Tester controls opened.");
        }

        public void RequestClose()
        {
            if (!Visible || _busy) return;
            StartCoroutine(CloseRoutine());
        }

        private IEnumerator CloseRoutine()
        {
            _busy = true;
            _confirmingClear = false;
            HidePanel();
            yield return null;
            _runner?.SuspendInput(false);
            _busy = false;
        }

        public void PlayRescue()
        {
            if (!Active || _busy) return;
            HidePanel();
            _runner?.SuspendInput(false);
            _flow?.TesterPlay(_selectedRound, _selectedRescue);
            Paint();
        }

        public bool SelectTarget(int round, int rescueIndex)
        {
            if (!Active || _flow?.Catalog == null || !_flow.Catalog.Exists(round) ||
                rescueIndex < 0 || rescueIndex >= RoundDefinition.RescuesPerRound)
            {
                return false;
            }

            _selectedRound = round;
            _selectedRescue = rescueIndex;
            Paint();
            return true;
        }

        private void SelectRound(int number) => SelectTarget(number, 0);

        private void SelectRescue(int index) => SelectTarget(_selectedRound, index);

        private void SetSubscribed(bool subscribed)
        {
            if (!Active || _fakeEntitlements == null) return;
            _fakeEntitlements.SetSubscribed(subscribed);
            Debug.Log($"[SavePeps] Tester entitlement set to: {(subscribed ? "Peps Unlimited" : "Free")}.");
            Paint();
        }

        private void HandleClearProgressClicked()
        {
            if (!Active || _busy) return;

            if (!_confirmingClear)
            {
                _confirmingClear = true;
                _confirmExpiresTime = Time.unscaledTime + 5f;
                Paint();
                return;
            }

            // Confirmed destructive reset
            _confirmingClear = false;
            _flow?.TesterApplyProfile(TesterProfilePreset.Fresh);
            HidePanel();
            _runner?.SuspendInput(false);
            Debug.Log("[SavePeps] Tester cleared all progress. Profile is now fresh.");
            Paint();
        }

        private void CancelClearProgress()
        {
            _confirmingClear = false;
            Paint();
        }

        private void Paint()
        {
            if (!Available) return;

            if (_indicatorLabel != null) _indicatorLabel.text = "TESTER";

            var catalog = _flow?.Catalog;
            for (var i = 0; i < _roundButtons.Length; i++)
            {
                var button = _roundButtons[i];
                if (button == null) continue;
                var exists = catalog != null && catalog.Exists(i + 1);
                button.interactable = exists;
                PaintButton(button, exists && i + 1 == _selectedRound ? Selected : Ordinary);
            }

            for (var i = 0; i < _rescueButtons.Length; i++)
            {
                var button = _rescueButtons[i];
                if (button == null) continue;
                PaintButton(button, i == _selectedRescue ? Selected : Ordinary);
            }

            if (_goToSelectionSummary != null)
            {
                var rescueDef = _flow?.Catalog?.Round(_selectedRound)?.RescueAt(_selectedRescue);
                var rescueName = rescueDef != null ? $" · {rescueDef.Id}" : string.Empty;
                _goToSelectionSummary.text = $"ROUND {_selectedRound} · RESCUE {_selectedRescue + 1}{rescueName}";
            }

            var isSubscribed = _fakeEntitlements is { IsSubscribed: true };
            if (_freeButton != null)
            {
                _freeButton.interactable = _fakeEntitlements != null;
                PaintButton(_freeButton, !isSubscribed ? ActiveAccess : InactiveAccess);
            }
            if (_freeLabel != null)
            {
                _freeLabel.color = !isSubscribed ? Ink : new Color(Ink.r, Ink.g, Ink.b, 0.75f);
            }

            if (_unlimitedButton != null)
            {
                _unlimitedButton.interactable = _fakeEntitlements != null;
                PaintButton(_unlimitedButton, isSubscribed ? ActiveAccess : InactiveAccess);
            }
            if (_unlimitedLabel != null)
            {
                _unlimitedLabel.color = isSubscribed ? Ink : new Color(Ink.r, Ink.g, Ink.b, 0.75f);
            }

            if (_clearProgressButton != null)
            {
                PaintButton(_clearProgressButton, _confirmingClear ? ConfirmDestructive : NormalDestructive);
            }
            if (_clearProgressLabel != null)
            {
                _clearProgressLabel.text = _confirmingClear ? "CONFIRM: ERASE ALL PROGRESS" : "CLEAR ALL PROGRESS";
                _clearProgressLabel.color = _confirmingClear ? Color.white : new Color(0.55f, 0.20f, 0.20f, 1f);
            }

            if (_cancelClearButton != null)
            {
                SetVisible(_cancelClearButton.gameObject, _confirmingClear);
            }
        }

        private static void PaintButton(Button button, Color color)
        {
            if (button?.targetGraphic != null) button.targetGraphic.color = color;
        }

        private void HidePanel()
        {
            if (_group != null)
            {
                _group.alpha = 0f;
                _group.interactable = false;
                _group.blocksRaycasts = false;
            }
            SetVisible(_root, false);
            SetVisible(_indicatorRoot, Active);
        }

        private static void SetVisible(GameObject target, bool visible)
        {
            if (target != null && target.activeSelf != visible) target.SetActive(visible);
        }
    }
}
