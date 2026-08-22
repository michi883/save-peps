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

        [Tooltip("Second source, looping, for the per-world ambience bed. Separate so a cue never cuts the bed.")]
        [SerializeField] private AudioSource _ambienceSource;

        [SerializeField] private Clip[] _clips;
        [SerializeField] private bool _hapticsEnabled = true;

        /// <summary>
        /// Player settings, applied by the shell from the save file. They are
        /// separate from <c>_hapticsEnabled</c>, which is the hardware verdict
        /// — a device with no vibrator switches that off permanently, and it
        /// must not look to the player as though they turned buzz off.
        /// </summary>
        public bool SoundEnabled
        {
            get => _soundEnabled;
            set
            {
                _soundEnabled = value;
                // The bed is a looping source, so it has to be muted rather
                // than simply not started again — turning sound off mid-round
                // must silence the world immediately.
                if (_ambienceSource != null) _ambienceSource.mute = !value;
            }
        }

        private bool _soundEnabled = true;

        public bool HapticsAllowed { get; set; } = true;

        private readonly Dictionary<string, Clip> _byId = new();
        private readonly List<AudioClip> _generated = new();

        private void Awake()
        {
            if (_source == null) _source = gameObject.AddComponent<AudioSource>();
            _source.playOnAwake = false;
            _source.spatialBlend = 0f;
            _source.dopplerLevel = 0f;

            if (_ambienceSource == null) _ambienceSource = gameObject.AddComponent<AudioSource>();
            _ambienceSource.playOnAwake = false;
            _ambienceSource.loop = true;
            _ambienceSource.spatialBlend = 0f;
            _ambienceSource.dopplerLevel = 0f;
            foreach (var c in _clips ?? System.Array.Empty<Clip>())
            {
                if (c != null && !string.IsNullOrEmpty(c.Id)) _byId[c.Id] = c;
            }

            // Load recorded SFX from Resources
            Dictionary<string, string> idMapping = new()
            {
                ["tap"] = "object_tap",
                ["wrong"] = "failure_sting",
                ["reunion"] = "reunion_hug",
                ["star"] = "star_earned",
                ["thud"] = "impact_soft",
                ["bonk"] = "impact_soft"
            };

            foreach (var (id, filename) in idMapping)
            {
                var clip = Resources.Load<AudioClip>($"SFX/{filename}");
                if (clip != null)
                {
                    float volume = filename switch
                    {
                        "object_tap" => 0.5f,
                        "impact_soft" => 0.7f,
                        "failure_sting" => 0.6f,
                        "reunion_hug" => 0.9f,
                        "star_earned" => 0.7f,
                        _ => 0.8f
                    };
                    _byId[id] = new Clip { Id = id, Audio = clip, Volume = volume };
                }
            }

            // Authored clips always win. The synthesized bank fills every
            // remaining choreography id, so prototype silence can never make
            // a physically readable outcome feel inert on the device.
            foreach (var (id, audio) in ToyAudioBank.Create())
            {
                if (_byId.ContainsKey(id))
                {
                    Destroy(audio);
                    continue;
                }

                _generated.Add(audio);
                _byId[id] = new Clip
                {
                    Id = id,
                    Audio = audio,
                    // Beds sit under everything by design; the mix is the
                    // difference between atmosphere and interference.
                    Volume = id.StartsWith("amb_") ? 0.55f
                        : id is "whoosh" or "slide" or "wind" or "steam" ? 0.62f
                        : 0.82f,
                };
            }
        }

        private void OnDestroy()
        {
            CancelInvoke();
            foreach (var clip in _generated)
            {
                if (clip != null) Destroy(clip);
            }
        }

        public void Play(string id)
        {
            if (!SoundEnabled) return;
            if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out var clip) || clip.Audio == null) return;
            _source.PlayOneShot(clip.Audio, clip.Volume);
        }

        /// <summary>
        /// Swaps the looping bed a world plays under everything else.
        ///
        /// The bed is most of what makes a trench sound different from a
        /// rooftop when the cues themselves are the same handful of knocks and
        /// chimes, and it is the cheapest of the identity levers: one
        /// synthesized loop per world rather than a re-recorded sfx set.
        /// </summary>
        public void SetAmbience(string id, float volume)
        {
            if (_ambienceSource == null) return;

            if (string.IsNullOrEmpty(id) || !_byId.TryGetValue(id, out var clip) || clip.Audio == null)
            {
                _ambienceSource.Stop();
                _ambienceSource.clip = null;
                return;
            }

            _ambienceSource.volume = Mathf.Clamp01(volume) * clip.Volume;
            if (_ambienceSource.clip == clip.Audio && _ambienceSource.isPlaying) return;

            _ambienceSource.clip = clip.Audio;
            _ambienceSource.mute = !SoundEnabled;
            _ambienceSource.Play();
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
            if (!_hapticsEnabled || !HapticsAllowed || string.IsNullOrEmpty(strength)) return;

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
