using System.Collections.Generic;
using UnityEngine;

namespace SavePeps.Core
{
    /// <summary>
    /// Sound and haptics, at the fidelity P1 needs to judge feel and no more.
    /// The real audio library is P5; temporary clips are fine here, but the
    /// *timing* is not temporary — a tap that answers instantly is most of
    /// why a one-tap toy feels good.
    /// </summary>
    public sealed class Feedback : MonoBehaviour
    {
        [System.Serializable]
        public sealed class Clip
        {
            public string Id;
            public AudioClip Audio;
            [Range(0f, 1f)] public float Volume = 1f;
        }

        [SerializeField] private AudioSource _source;
        [SerializeField] private Clip[] _clips;
        [SerializeField] private bool _hapticsEnabled = true;

        private readonly Dictionary<string, Clip> _byId = new();

        private void Awake()
        {
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            foreach (var c in _clips ?? System.Array.Empty<Clip>())
            {
                if (c != null && !string.IsNullOrEmpty(c.Id)) _byId[c.Id] = c;
            }
        }

        public void Play(string id)
        {
            if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out var clip) || clip.Audio == null) return;
            _source.PlayOneShot(clip.Audio, clip.Volume);
        }

        public void Tap()
        {
            Play("tap");
            Haptic("light");
        }

        public void Wrong()
        {
            Play("wrong");
            Haptic("medium");
        }

        public void Reunion()
        {
            Play("reunion");
            Haptic("success");
        }

        /// <summary>
        /// Android's VibrationEffect, not <c>Handheld.Vibrate</c> — the latter
        /// is a single blunt 500ms buzz with no intensity control, which reads
        /// as an error notification rather than as a tap.
        /// </summary>
        public void Haptic(string strength)
        {
            if (!_hapticsEnabled || string.IsNullOrEmpty(strength)) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            var (ms, amplitude) = strength.ToLowerInvariant() switch
            {
                "light"   => (12L, 60),
                "medium"  => (24L, 140),
                "success" => (18L, 200),
                _         => (12L, 60),
            };

            try
            {
                using var player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                using var activity = player.GetStatic<AndroidJavaObject>("currentActivity");
                using var vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");
                if (vibrator == null || !vibrator.Call<bool>("hasVibrator")) return;

                if (AndroidApiLevel >= 26)
                {
                    using var effectClass = new AndroidJavaClass("android.os.VibrationEffect");
                    using var effect = effectClass.CallStatic<AndroidJavaObject>("createOneShot", ms, amplitude);
                    vibrator.Call("vibrate", effect);
                }
                else
                {
                    vibrator.Call("vibrate", ms);
                }

                if (strength == "success")
                {
                    // Two quick beats: the reunion should feel like a
                    // heartbeat rather than a notification.
                    Invoke(nameof(SecondBeat), 0.09f);
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[SavePeps] Haptics unavailable: {e.Message}");
                _hapticsEnabled = false;
            }
#endif
        }

        private void SecondBeat() => Haptic("light");

#if UNITY_ANDROID && !UNITY_EDITOR
        private static int AndroidApiLevel
        {
            get
            {
                using var version = new AndroidJavaClass("android.os.Build$VERSION");
                return version.GetStatic<int>("SDK_INT");
            }
        }
#endif
    }
}
