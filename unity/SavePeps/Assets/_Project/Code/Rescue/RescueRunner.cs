using System;
using System.Collections;
using System.Collections.Generic;
using SavePeps.Core;
using UnityEngine;

namespace SavePeps.Rescue
{
    /// <summary>
    /// Runs one rescue: builds the diorama, wires the taps, plays outcomes,
    /// and owns the win / fail / retry loop.
    ///
    /// The loop is deliberately tiny, because the whole game is one tap. The
    /// only genuinely delicate part is retry, which must put the scene back
    /// exactly — see <see cref="AnimTarget"/> for why that is cheap here.
    ///
    /// The runner knows nothing about rounds, progression or the catalogue.
    /// It is handed one rescue at a time by <c>GameFlow</c> and reports the
    /// result; keeping it ignorant of what comes next is what lets the same
    /// component serve the game, the editor preview, and the Gauntlet.
    /// </summary>
    public sealed class RescueRunner : MonoBehaviour
    {
        [Tooltip("Optional. Played at boot when nothing else drives this runner — the P1 slice, the editor preview.")]
        [SerializeField] private RescueDefinition _rescue;

        [Tooltip("Off when a GameFlow owns the sequencing.")]
        [SerializeField] private bool _autoPlayOnStart = true;

        [SerializeField] private TapRouter _tapRouter;
        [SerializeField] private ChoreographyPlayer _player;
        [SerializeField] private RescueHud _hud;
        [SerializeField] private Feedback _feedback;

        private readonly Dictionary<string, AnimTarget> _targets = new();
        private GameObject _diorama;
        private Pep _pepA, _pepB;
        private Transform _meetAnchor;
        private RescueObject _tapped;
        private int _attempts;
        private bool _solved;

        /// <summary>Raised when the Peps are reunited. True if solved first tap.</summary>
        public event Action<bool> OnSolved;

        /// <summary>The rescue currently staged, or null between rescues.</summary>
        public RescueDefinition Current => _rescue;

        private void Awake()
        {
            if (_player != null) _player.OnEvent += HandleEvent;
            if (_tapRouter != null) _tapRouter.OnTap += HandleTap;
        }

        private void Start()
        {
            if (!_autoPlayOnStart) return;

            if (_rescue == null)
            {
                Debug.LogError("[SavePeps] RescueRunner is set to auto-play but has no rescue assigned.");
                return;
            }

            Load(_rescue);
        }

        private void OnDestroy()
        {
            if (_player != null) _player.OnEvent -= HandleEvent;
            if (_tapRouter != null) _tapRouter.OnTap -= HandleTap;
        }

        // -------------------------------------------------------------------
        // Staging
        // -------------------------------------------------------------------

        /// <summary>
        /// Stages a rescue, replacing whatever was there. Safe to call
        /// repeatedly — this is the seam the round loop drives.
        /// </summary>
        public void Load(RescueDefinition rescue)
        {
            if (rescue == null)
            {
                Debug.LogError("[SavePeps] RescueRunner.Load was given no rescue.");
                return;
            }

            Teardown();
            _rescue = rescue;
            Build();
        }

        /// <summary>
        /// Clears the staged rescue. Every per-rescue field is reset here
        /// rather than in Build, so that a half-built rescue (a missing
        /// diorama, a bad anchor) cannot leave a previous one's Peps or
        /// targets behind to be animated by the next tap.
        /// </summary>
        public void Teardown()
        {
            StopAllCoroutines();
            if (_player != null) _player.Stop();

            if (_diorama != null) Destroy(_diorama);
            _diorama = null;

            _targets.Clear();
            _rescue = null;
            _pepA = null;
            _pepB = null;
            _meetAnchor = null;
            _tapped = null;
            _attempts = 0;
            _solved = false;

            if (_tapRouter != null) _tapRouter.InputEnabled = false;
        }

        private void Build()
        {
            _diorama = Instantiate(_rescue.Environment, transform);
            _diorama.name = "Diorama";

            var anchors = new Dictionary<string, Transform>();
            foreach (var t in _diorama.GetComponentsInChildren<Transform>(includeInactive: true))
            {
                anchors[t.name] = t;
            }

            _meetAnchor = Look(anchors, _rescue.MeetAnchor);
            _pepA = SpawnPep(anchors, _rescue.PepAAnchor, _rescue.PepAPrefab, "PepA");
            _pepB = SpawnPep(anchors, _rescue.PepBAnchor, _rescue.PepBPrefab, "PepB");
            if (_pepA != null && _pepB != null)
            {
                _pepA.SetPartner(_pepB.transform);
                _pepB.SetPartner(_pepA.transform);
            }

            foreach (var obj in _rescue.Objects)
            {
                if (obj?.Prop == null) continue;
                var anchor = Look(anchors, obj.AnchorId);
                if (anchor == null) continue;

                var prop = Instantiate(obj.Prop, anchor);
                prop.name = obj.Id;
                prop.transform.localPosition = Vector3.zero;
                prop.transform.localRotation = Quaternion.identity;

                var tappable = prop.GetComponentInChildren<Tappable>();
                if (tappable != null) tappable.ObjectId = obj.Id;

                var target = prop.GetComponentInChildren<AnimTarget>();
                if (target != null) _targets[obj.Id] = target;
            }

            // Named fx and scenery the choreography can move.
            foreach (var target in _diorama.GetComponentsInChildren<AnimTarget>(includeInactive: true))
            {
                _targets.TryAdd(target.transform.parent != null ? target.transform.parent.name : target.name, target);
            }

            _hud?.Show(_rescue.Goal);
            ResetScene();
        }

        private Pep SpawnPep(IReadOnlyDictionary<string, Transform> anchors, string anchorId, GameObject prefab, string label)
        {
            var anchor = Look(anchors, anchorId);
            if (anchor == null || prefab == null) return null;

            var pep = Instantiate(prefab, anchor).GetComponent<Pep>();
            if (pep == null) return null;
            pep.name = label;
            pep.transform.localPosition = Vector3.zero;
            return pep;
        }

        private static Transform Look(IReadOnlyDictionary<string, Transform> anchors, string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if (anchors.TryGetValue(id, out var t)) return t;
            Debug.LogWarning($"[SavePeps] Anchor '{id}' is not in this diorama.");
            return null;
        }

        // -------------------------------------------------------------------
        // The one tap
        // -------------------------------------------------------------------

        private void HandleTap(string objectId)
        {
            if (_rescue == null || _solved) return;

            var obj = Array.Find(_rescue.Objects, o => o != null && o.Id == objectId);
            if (obj == null) return;

            _tapped = obj;
            _attempts++;
            _tapRouter.InputEnabled = false;

            _feedback?.Tap();
            _hud?.ClearQuip();

            _player.Play(obj.Steps, Resolve);
            StartCoroutine(FinishAfter(obj));
        }

        private IEnumerator FinishAfter(RescueObject obj)
        {
            yield return new WaitForSeconds(obj.Duration);

            if (_rescue.IsCorrect(obj)) Win();
            else Fail(obj);
        }

        private void Win()
        {
            _solved = true;
            var firstTap = _attempts == 1;
            _feedback?.Reunion();
            _hud?.ShowResult(firstTap ? "Perfect!" : "Together again!");
            OnSolved?.Invoke(firstTap);
        }

        private void Fail(RescueObject obj)
        {
            _feedback?.Wrong();
            _hud?.ShowQuip(obj.Quip, Retry);
        }

        /// <summary>
        /// Puts the scene back exactly as it started. The brief's test —
        /// wrong, retry, wrong, retry, correct — has to behave identically
        /// every time, so this resets state rather than reloading the scene.
        /// </summary>
        public void Retry()
        {
            ResetScene();
            _hud?.ClearQuip();
        }

        private void ResetScene()
        {
            StopAllCoroutines();
            _player.Stop();

            foreach (var target in _targets.Values)
            {
                if (target != null) target.ResetToRest();
            }

            _pepA?.ResetToRest();
            _pepB?.ResetToRest();

            _tapped = null;
            _tapRouter.InputEnabled = true;
        }

        // -------------------------------------------------------------------
        // Choreography plumbing
        // -------------------------------------------------------------------

        private AnimTarget Resolve(string reference)
        {
            if (string.IsNullOrEmpty(reference)) return null;

            return reference switch
            {
                SceneRef.Self => _tapped != null && _targets.TryGetValue(_tapped.Id, out var self) ? self : null,
                SceneRef.PepA => _pepA?.Target,
                SceneRef.PepB => _pepB?.Target,
                _ => _targets.GetValueOrDefault(reference),
            };
        }

        private void HandleEvent(OutcomeStep step)
        {
            switch (step.Kind)
            {
                case StepKind.Face:
                    if (Enum.TryParse<PepFace>(step.Param, ignoreCase: true, out var face))
                    {
                        var who = step.Target switch
                        {
                            SceneRef.PepA => _pepA,
                            SceneRef.PepB => _pepB,
                            _ => null,
                        };
                        if (who != null) who.SetFace(face);
                        else { _pepA?.SetFace(face); _pepB?.SetFace(face); }
                    }
                    break;

                case StepKind.Sfx:
                    _feedback?.Play(step.Param);
                    break;

                case StepKind.Haptic:
                    _feedback?.Haptic(step.Param);
                    break;

                case StepKind.Meet:
                    StartCoroutine(PlayReunion(step.Duration));
                    break;
            }
        }

        /// <summary>
        /// The emotional payoff, and the most-watched animation in the game.
        /// Both Peps drop their idle, run to the meet anchor, and land on
        /// happy faces.
        /// </summary>
        private IEnumerator PlayReunion(float duration)
        {
            if (_pepA == null || _pepB == null || _meetAnchor == null) yield break;

            _pepA.SetIdle(false);
            _pepB.SetIdle(false);
            _pepA.SetFace(PepFace.Happy);
            _pepB.SetFace(PepFace.Happy);

            // Take the Peps off the choreography player and drive their
            // animated transforms directly, starting from wherever the
            // outcome left them — a Pep that just hopped across a plank must
            // not snap back to its anchor to begin the reunion.
            var aTarget = _pepA.Target.transform;
            var bTarget = _pepB.Target.transform;
            _player.Release(_pepA.Target);
            _player.Release(_pepB.Target);

            var aStart = aTarget.position;
            var bStart = bTarget.position;
            var meet = _meetAnchor.position;

            // Stop a body-width short of each other rather than intersecting.
            // 0.22 was too close on device — they read as one blob at the
            // moment the reunion is supposed to land.
            var offset = (aStart - bStart).normalized * 0.34f;
            var aEnd = meet + offset;
            var bEnd = meet - offset;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Easing.Evaluate(EaseKind.Hop, Mathf.Clamp01(elapsed / duration));
                // A little vertical skip so they bound rather than slide.
                var skip = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)) * 0.09f;
                aTarget.position = Vector3.Lerp(aStart, aEnd, t) + Vector3.up * skip;
                bTarget.position = Vector3.Lerp(bStart, bEnd, t) + Vector3.up * skip;
                yield return null;
            }

            _pepA.SetFace(PepFace.Love);
            _pepB.SetFace(PepFace.Love);
            _feedback?.Haptic("success");
        }
    }
}
