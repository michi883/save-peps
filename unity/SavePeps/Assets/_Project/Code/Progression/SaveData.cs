using System;
using System.Collections.Generic;
using UnityEngine;

namespace SavePeps.Progression
{
    /// <summary>
    /// How well a rescue was solved. Progression never depends on this — the
    /// brief is explicit that a wrong tap costs nothing but the star — so it
    /// exists purely so a player who nails one first time gets told so.
    /// </summary>
    public enum Mark
    {
        /// <summary>Never solved.</summary>
        None = 0,
        /// <summary>Solved, but not on the first tap.</summary>
        Check = 1,
        /// <summary>Solved first tap.</summary>
        Star = 2,
    }

    /// <summary>
    /// Everything that survives an app restart.
    ///
    /// Deliberately absent: entitlement. RevenueCat's CustomerInfo is the only
    /// source of truth for whether a player is subscribed, and its own cache
    /// already covers offline launches. Writing "is subscribed" into a
    /// user-writable JSON file in persistentDataPath would be both a
    /// correctness bug and a paywall anyone can defeat with a text editor.
    ///
    /// Serialised by JsonUtility, which cannot handle a Dictionary — hence the
    /// parallel key/value lists and the callback pair below. The runtime works
    /// against the dictionary; the lists exist only at the file boundary.
    /// </summary>
    [Serializable]
    public sealed class SaveData : ISerializationCallbackReceiver
    {
        /// <summary>Bumped only when a migration is needed. See <see cref="SaveStore"/>.</summary>
        public int SchemaVersion = CurrentSchemaVersion;

        public const int CurrentSchemaVersion = 1;

        public int HighestUnlockedRound = 1;
        /// <summary>Used only to keep random Play from immediately repeating itself.</summary>
        public int LastPlayedRound;
        public int TotalRescuesSolved;
        public bool SoundMuted;
        public bool HapticsOff;
        public long FirstRunUtc;

        [SerializeField] private List<string> _markIds = new();
        [SerializeField] private List<int> _markValues = new();

        [NonSerialized] private Dictionary<string, Mark> _marks = new();

        public IReadOnlyDictionary<string, Mark> Marks => _marks;

        public Mark MarkFor(string rescueId) =>
            rescueId != null && _marks.TryGetValue(rescueId, out var m) ? m : Mark.None;

        /// <summary>
        /// Records a solve. A replay can upgrade a Check to a Star but never
        /// downgrades a Star — the player already proved they knew it, and
        /// taking the star back for replaying is a punishment this game does
        /// not otherwise deal in.
        /// </summary>
        public void RecordSolved(string rescueId, bool firstTap)
        {
            if (string.IsNullOrEmpty(rescueId)) return;

            var earned = firstTap ? Mark.Star : Mark.Check;
            var existing = MarkFor(rescueId);
            if (existing == Mark.None) TotalRescuesSolved++;
            if (earned > existing) _marks[rescueId] = earned;
        }

        public void UnlockThrough(int round)
        {
            if (round > HighestUnlockedRound) HighestUnlockedRound = round;
        }

        public static SaveData Fresh() => new()
        {
            SchemaVersion = CurrentSchemaVersion,
            HighestUnlockedRound = 1,
            FirstRunUtc = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        };

        // -------------------------------------------------------------------
        // Dictionary <-> parallel lists, only at the file boundary
        // -------------------------------------------------------------------

        public void OnBeforeSerialize()
        {
            _markIds.Clear();
            _markValues.Clear();
            foreach (var pair in _marks)
            {
                _markIds.Add(pair.Key);
                _markValues.Add((int)pair.Value);
            }
        }

        /// <summary>
        /// Rebuilds the dictionary defensively: a hand-edited or truncated
        /// file can arrive with mismatched list lengths or a duplicate id, and
        /// neither should be worth a crash on launch.
        /// </summary>
        public void OnAfterDeserialize()
        {
            _marks = new Dictionary<string, Mark>();
            var count = Mathf.Min(_markIds?.Count ?? 0, _markValues?.Count ?? 0);
            for (var i = 0; i < count; i++)
            {
                var id = _markIds[i];
                if (string.IsNullOrEmpty(id)) continue;
                var value = (Mark)Mathf.Clamp(_markValues[i], 0, (int)Mark.Star);
                _marks[id] = value;
            }

            if (HighestUnlockedRound < 1) HighestUnlockedRound = 1;
            if (LastPlayedRound < 0) LastPlayedRound = 0;
            if (TotalRescuesSolved < 0) TotalRescuesSolved = 0;
        }
    }
}
