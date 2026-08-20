using System;
using UnityEngine;

namespace SavePeps.Rescue
{
    public enum Difficulty { Easy = 0, Medium = 1, Surprising = 2 }

    /// <summary>
    /// The physical idea the player has to recognise, independent of the
    /// environment or the outcome verb.
    ///
    /// Verb uniqueness did not catch the first catalogue's real repetition:
    /// bridge, dam, ferry, swing, lift and glide were six words for the same
    /// "move a Pep across a horizontal gap" decision. Keeping the reasoning
    /// kind as authored data lets validation guard the distinction that
    /// actually matters to the player.
    /// </summary>
    public enum ReasoningKind
    {
        Crossing = 0,
        Activation = 1,
        Cutting = 2,
        Luring = 3,
        Counterweight = 4,
        Reflection = 5,
        Temperature = 6,
        Growth = 7,
        Shelter = 8,
    }

    /// <summary>
    /// One of the three things the player can tap.
    ///
    /// Save Pip's house rule holds: within a rescue exactly one object saves,
    /// and every wrong object fails *funny* with a one-line quip. A wrong
    /// answer is entertainment, not punishment.
    /// </summary>
    [Serializable]
    public sealed class RescueObject
    {
        [Tooltip("Stable id: plank, balloon, fan.")]
        public string Id;

        [Tooltip("Prop prefab. Instantiated under the named anchor.")]
        public GameObject Prop;

        [Tooltip("Anchor in the diorama this prop sits on, e.g. Slot_1.")]
        public string AnchorId;

        [Tooltip("Screen-reader label.")]
        public string Label;

        [Tooltip("Wrong objects only: the Try Again caption. Dry, short, never scolding.")]
        [TextArea]
        public string Quip;

        [Tooltip("Seconds from tap until the result appears. 2.0-3.6 keeps outcomes paced like a gag.")]
        public float Duration = 2.5f;

        public OutcomeStep[] Steps;
    }

    /// <summary>
    /// A whole rescue as data.
    ///
    /// The goal from PLAN.md: adding a rescue means filling this in and
    /// dragging prefabs, never writing C#. That is what made 106 rescues
    /// tractable in Save Pip and it is the only way 36 fit in this schedule.
    /// </summary>
    [CreateAssetMenu(menuName = "Peps/Rescue", fileName = "Rescue")]
    public sealed class RescueDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string Id;

        [Tooltip("The rescue verb — the way the player has to think. Unique across the catalog.")]
        public string Verb;

        [Tooltip("The physical reasoning structure. Adjacent rescues must differ even when their verbs do.")]
        public ReasoningKind Reasoning;

        [Tooltip("2-4 words. What the Peps need, never how. Shown over the scene.")]
        public string Goal;

        public Difficulty Difficulty = Difficulty.Easy;

        [Tooltip("Scene description for screen readers and design review.")]
        [TextArea]
        public string SceneDescription;

        [Header("Staging")]
        [Tooltip("The diorama prefab: geometry, anchors, and the fixed camera framing.")]
        public GameObject Environment;

        [Tooltip("The two Peps are separate prefabs: distinct silhouettes matter more than distinct colours.")]
        public GameObject PepAPrefab;
        public GameObject PepBPrefab;

        [Tooltip("Anchor names inside the diorama where each Pep starts.")]
        public string PepAAnchor = "Anchor_PepA";
        public string PepBAnchor = "Anchor_PepB";

        [Tooltip("Where the reunion plays.")]
        public string MeetAnchor = "Anchor_Meet";

        [Header("Choices")]
        [Tooltip("Exactly three. Exactly one correct.")]
        public RescueObject[] Objects = new RescueObject[3];

        [Tooltip("Index into Objects of the one that reunites the Peps.")]
        public int CorrectIndex;

        public RescueObject Correct =>
            Objects != null && CorrectIndex >= 0 && CorrectIndex < Objects.Length
                ? Objects[CorrectIndex]
                : null;

        public bool IsCorrect(RescueObject obj) => obj != null && obj == Correct;

        /// <summary>
        /// The structural rules, checked in the inspector so a broken rescue
        /// is caught while authoring rather than during a demo. The full
        /// validator suite is P2; this is the subset that would otherwise
        /// produce a silently unplayable scene.
        /// </summary>
        private void OnValidate()
        {
            if (Objects is { Length: > 0 } && Objects.Length != 3)
            {
                Debug.LogWarning($"[SavePeps] '{name}' has {Objects.Length} objects; a rescue must offer exactly three.", this);
            }

            if (Objects != null) CorrectIndex = Mathf.Clamp(CorrectIndex, 0, Mathf.Max(0, Objects.Length - 1));

            foreach (var o in Objects ?? Array.Empty<RescueObject>())
            {
                if (o == null) continue;
                o.Duration = Mathf.Clamp(o.Duration, 1f, 6f);
                if (!IsCorrect(o) && string.IsNullOrWhiteSpace(o.Quip))
                {
                    Debug.LogWarning($"[SavePeps] '{name}': wrong object '{o.Id}' needs a quip — failures must land as jokes.", this);
                }
            }
        }
    }
}
