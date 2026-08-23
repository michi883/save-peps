using System;
using System.Collections.Generic;
using UnityEngine;

namespace SavePeps.Core
{
    /// <summary>
    /// Tiny synthesized toy sounds used until (and as a fallback after) final
    /// recorded assets arrive. The authored choreography already names its
    /// sounds; generating this bank once at boot makes those cues audible on
    /// every device without adding an asset-production dependency.
    /// </summary>
    internal static class ToyAudioBank
    {
        private const int SampleRate = 44100;
        private delegate float Sample(float time, int index);

        /// <summary>A looping sample: absolute time, sample index, and loop progress 0..1.</summary>
        private delegate float LoopSample(float time, int index, float progress);

        public static Dictionary<string, AudioClip> Create() => new()
        {
            ["tap"] = Make("tap", 0.09f, (t, i) =>
                Envelope(t, 0.09f, 0.003f, 2.5f) *
                (Tone(t, Mathf.Lerp(1050f, 620f, t / 0.09f)) * 0.78f + Noise(i) * 0.12f)),

            ["wrong"] = Make("wrong", 0.34f, (t, _) =>
                BellPulse(t, 0f, 0.18f, 330f) * 0.55f + BellPulse(t, 0.13f, 0.20f, 247f) * 0.48f),

            // Mastery punctuation comes after the reunion. A first-tap star
            // gets the brighter three-note glint; a retry check lands softly
            // and positively rather than sounding like a lesser win.
            ["star"] = Make("star", 0.58f, (t, _) =>
                BellPulse(t, 0f, 0.24f, 784f) * 0.26f +
                BellPulse(t, 0.10f, 0.30f, 988f) * 0.30f +
                BellPulse(t, 0.23f, 0.34f, 1319f) * 0.25f),

            ["check"] = Make("check", 0.36f, (t, _) =>
                BellPulse(t, 0f, 0.22f, 659f) * 0.25f +
                BellPulse(t, 0.10f, 0.25f, 784f) * 0.25f),

            // It starts when Meet begins: two anticipation notes lead into a
            // warm chord near physical contact rather than congratulating the
            // player before the Peps have actually reached one another.
            ["reunion"] = Make("reunion", 1.12f, (t, _) =>
                BellPulse(t, 0.02f, 0.24f, 523.25f) * 0.28f +
                BellPulse(t, 0.25f, 0.28f, 659.25f) * 0.30f +
                BellPulse(t, 0.57f, 0.52f, 523.25f) * 0.34f +
                BellPulse(t, 0.57f, 0.52f, 659.25f) * 0.30f +
                BellPulse(t, 0.57f, 0.52f, 783.99f) * 0.32f),

            ["slide"] = Make("slide", 0.42f, (t, i) =>
                Envelope(t, 0.42f, 0.02f, 1.4f) *
                (Noise(i / 7) * 0.22f + Tone(t, Mathf.Lerp(185f, 120f, t / 0.42f)) * 0.12f)),

            ["whoosh"] = Make("whoosh", 0.50f, (t, i) =>
                Mathf.Sin(Mathf.Clamp01(t / 0.50f) * Mathf.PI) *
                (Noise(i / 13) * 0.30f + Tone(t, 95f + t * 90f) * 0.05f)),

            ["thud"] = Make("thud", 0.26f, (t, i) =>
                Envelope(t, 0.26f, 0.002f, 3.2f) *
                (Tone(t, Mathf.Lerp(135f, 52f, t / 0.26f)) * 0.74f + Noise(i) * 0.10f)),

            ["bonk"] = Make("bonk", 0.42f, (t, _) =>
                Envelope(t, 0.42f, 0.002f, 2.6f) *
                (Tone(t, Mathf.Lerp(540f, 170f, t / 0.42f)) * 0.62f + Tone(t, 820f) * 0.12f)),

            ["boing"] = Make("boing", 0.62f, (t, _) =>
                Envelope(t, 0.62f, 0.004f, 1.8f) *
                Tone(t, 185f + Mathf.Sin(t * 17f) * 75f) * 0.72f),

            ["bell"] = Make("bell", 0.78f, (t, _) =>
                BellPulse(t, 0f, 0.76f, 784f) * 0.64f + BellPulse(t, 0.02f, 0.70f, 1175f) * 0.22f),

            ["chime"] = Make("chime", 0.72f, (t, _) =>
                BellPulse(t, 0f, 0.34f, 659f) * 0.36f +
                BellPulse(t, 0.14f, 0.38f, 784f) * 0.38f +
                BellPulse(t, 0.30f, 0.40f, 988f) * 0.38f),

            ["click"] = Make("click", 0.11f, (t, i) =>
                Envelope(t, 0.11f, 0.001f, 4f) * (Noise(i) * 0.34f + Tone(t, 1250f) * 0.24f)),

            ["pop"] = Make("pop", 0.17f, (t, i) =>
                Envelope(t, 0.17f, 0.001f, 3.5f) *
                (Tone(t, Mathf.Lerp(580f, 155f, t / 0.17f)) * 0.52f + Noise(i) * 0.18f)),

            ["poof"] = Make("poof", 0.35f, (t, i) =>
                Envelope(t, 0.35f, 0.012f, 2f) * (Noise(i / 9) * 0.30f + Tone(t, 120f) * 0.08f)),

            ["splash"] = Make("splash", 0.48f, (t, i) =>
                Envelope(t, 0.48f, 0.006f, 1.9f) *
                (Noise(i / 5) * 0.25f + Tone(t, 260f + Mathf.Sin(t * 32f) * 80f) * 0.14f)),

            ["snip"] = Make("snip", 0.19f, (t, i) =>
                (Envelope(t, 0.07f, 0.001f, 4f) +
                 (t > 0.085f ? Envelope(t - 0.085f, 0.09f, 0.001f, 4f) : 0f)) *
                (Noise(i) * 0.24f + Tone(t, 1450f) * 0.18f)),

            ["sigh"] = Make("sigh", 0.72f, (t, i) =>
                Mathf.Sin(Mathf.Clamp01(t / 0.72f) * Mathf.PI) *
                (Noise(i / 21) * 0.18f + Tone(t, Mathf.Lerp(310f, 145f, t / 0.72f)) * 0.08f)),

            // ---------------------------------------------------------------
            // World cues.
            //
            // Twelve worlds that all answer a tap with the same four knocks
            // sound like one world with twelve wallpapers. These are the
            // per-world half of the vocabulary — a ratchet for the clockwork
            // courtyard, a sonar ping for the trench, a servo for orbit — and
            // they cost a few lines each rather than an asset pipeline.
            // ---------------------------------------------------------------

            /// Clockwork: a brass pawl skipping over teeth.
            ["ratchet"] = Make("ratchet", 0.40f, (t, i) =>
            {
                var tick = Mathf.Repeat(t, 0.055f);
                return Envelope(tick, 0.03f, 0.0005f, 4f) * Envelope(t, 0.40f, 0.01f, 1.2f) * 4f *
                       (Noise(i) * 0.22f + Tone(t, 900f) * 0.16f);
            }),

            /// Heavy machinery: a struck iron plate.
            ["clank"] = Make("clank", 0.46f, (t, _) =>
                BellPulse(t, 0f, 0.44f, 196f) * 0.46f + BellPulse(t, 0.005f, 0.24f, 523f) * 0.20f +
                BellPulse(t, 0.01f, 0.14f, 1490f) * 0.10f),

            /// Pressure release: steam, a suit vent, a quenched casting.
            ["hiss"] = Make("hiss", 0.62f, (t, i) =>
                Envelope(t, 0.62f, 0.03f, 1.5f) * (Noise(i / 3) * 0.30f + Noise(i) * 0.10f)),

            /// Water hitting something far too hot.
            ["sizzle"] = Make("sizzle", 0.78f, (t, i) =>
                Envelope(t, 0.78f, 0.015f, 1.1f) *
                (Noise(i) * 0.26f + Noise(i / 11) * 0.14f + Tone(t, 1900f) * 0.04f)),

            /// A long gust across an exposed edge.
            ["wind"] = Make("wind", 1.10f, (t, i) =>
                Mathf.Sin(Mathf.Clamp01(t / 1.10f) * Mathf.PI) *
                (Noise(i / 29) * 0.34f + Noise(i / 7) * 0.10f)),

            /// Something very large moving, felt more than heard.
            ["rumble"] = Make("rumble", 1.05f, (t, i) =>
                Mathf.Sin(Mathf.Clamp01(t / 1.05f) * Mathf.PI) *
                (Tone(t, 41f) * 0.52f + Tone(t, 62f) * 0.24f + Noise(i / 37) * 0.12f)),

            /// A single drop into standing water, with the cave's tail on it.
            ["drip"] = Make("drip", 0.44f, (t, _) =>
                Envelope(t, 0.44f, 0.001f, 3.4f) * Tone(t, Mathf.Lerp(620f, 1450f, Mathf.Clamp01(t / 0.12f))) * 0.42f),

            /// Struck crystal: a pure, long, slightly detuned ring.
            ["crystal"] = Make("crystal", 1.30f, (t, _) =>
                BellPulse(t, 0f, 1.28f, 1046.5f) * 0.34f + BellPulse(t, 0.03f, 1.10f, 1568f) * 0.20f +
                BellPulse(t, 0.06f, 0.90f, 2093f) * 0.12f),

            /// The cave answering a shape it does not like: flat, dusty, dead.
            ["clunk"] = Make("clunk", 0.34f, (t, i) =>
                Envelope(t, 0.34f, 0.002f, 3f) * (Tone(t, 128f) * 0.55f + Noise(i / 5) * 0.18f)),

            /// Pick into rock.
            ["chip"] = Make("chip", 0.22f, (t, i) =>
                Envelope(t, 0.22f, 0.001f, 4.2f) * (Noise(i) * 0.34f + Tone(t, 780f) * 0.22f)),

            /// A rising column of bubbles.
            ["bubble"] = Make("bubble", 0.52f, (t, _) =>
            {
                var pop = Mathf.Repeat(t, 0.11f);
                return Envelope(pop, 0.05f, 0.002f, 3f) * Envelope(t, 0.52f, 0.01f, 0.8f) * 3f *
                       Tone(pop, Mathf.Lerp(280f, 900f, pop / 0.05f)) * 0.5f;
            }),

            /// Deep-water ping, and the reason the trench feels large.
            ["sonar"] = Make("sonar", 1.20f, (t, _) =>
                BellPulse(t, 0f, 0.30f, 660f) * 0.34f + BellPulse(t, 0.42f, 0.34f, 660f) * 0.16f +
                BellPulse(t, 0.80f, 0.38f, 660f) * 0.08f),

            /// Orbit: a small electric actuator, dry and close.
            ["servo"] = Make("servo", 0.44f, (t, i) =>
                Envelope(t, 0.44f, 0.02f, 1.6f) *
                (Tone(t, 220f + Mathf.Sin(t * 120f) * 40f) * 0.24f + Noise(i / 15) * 0.10f)),

            /// A short puff of cold gas — the only push available in vacuum.
            ["thrust"] = Make("thrust", 0.55f, (t, i) =>
                Envelope(t, 0.55f, 0.008f, 2.2f) * (Noise(i / 2) * 0.32f + Tone(t, 150f) * 0.08f)),

            /// Magnet meeting steel.
            ["snap_on"] = Make("snap_on", 0.30f, (t, _) =>
                Envelope(t, 0.30f, 0.001f, 3.6f) *
                (Tone(t, Mathf.Lerp(320f, 90f, t / 0.30f)) * 0.62f + Tone(t, 1180f) * 0.14f)),

            /// High voltage arriving.
            ["zap"] = Make("zap", 0.42f, (t, i) =>
                Envelope(t, 0.42f, 0.0005f, 3f) *
                (Noise(i) * 0.34f + Tone(t, 60f) * 0.28f + Tone(t, 4200f) * 0.10f)),

            /// Neon starting up: a buzz that settles into a hum.
            ["neon"] = Make("neon", 0.90f, (t, i) =>
            {
                var strike = t < 0.28f ? Noise(i) * 0.30f * (1f - t / 0.28f) : 0f;
                return strike + Envelope(t, 0.90f, 0.02f, 0.6f) * Tone(t, 120f) * 0.20f;
            }),

            /// City transit going past at speed.
            ["transit"] = Make("transit", 0.95f, (t, i) =>
                Mathf.Sin(Mathf.Clamp01(t / 0.95f) * Mathf.PI) *
                (Noise(i / 5) * 0.24f + Tone(t, Mathf.Lerp(180f, 320f, t / 0.95f)) * 0.16f)),

            /// Compacted snow underfoot.
            ["crunch"] = Make("crunch", 0.30f, (t, i) =>
                Envelope(t, 0.30f, 0.004f, 2.4f) * (Noise(i / 2) * 0.30f + Noise(i) * 0.12f)),

            /// A loaded snow or ice shelf splitting before it releases.
            ["crack"] = Make("crack", 0.48f, (t, i) =>
                Envelope(t, 0.48f, 0.001f, 2.8f) *
                (Noise(i) * 0.28f + Noise(i / 5) * 0.18f +
                 Tone(t, Mathf.Lerp(420f, 72f, t / 0.48f)) * 0.24f)),

            /// A runner on snow.
            ["glide_hiss"] = Make("glide_hiss", 0.90f, (t, i) =>
                Mathf.Sin(Mathf.Clamp01(t / 0.90f) * Mathf.PI) * (Noise(i / 4) * 0.26f)),

            /// Rope, canvas or timber taking load.
            ["creak"] = Make("creak", 0.66f, (t, i) =>
                Envelope(t, 0.66f, 0.05f, 1.3f) *
                (Tone(t, 190f + Mathf.Sin(t * 26f) * 55f) * 0.28f + Noise(i / 23) * 0.08f)),

            /// A hull, a raft, a rooftop tank: something big and hollow.
            ["gong"] = Make("gong", 1.40f, (t, _) =>
                BellPulse(t, 0f, 1.38f, 110f) * 0.42f + BellPulse(t, 0.02f, 1.10f, 164f) * 0.22f +
                BellPulse(t, 0.05f, 0.80f, 277f) * 0.12f),

            // ---------------------------------------------------------------
            // Ambience beds.
            //
            // Looping, quiet, and deliberately featureless — they set a room
            // tone rather than play a part. Every one is built from periodic
            // terms with a whole number of cycles across the loop length, so
            // the seam is inaudible without any crossfade machinery.
            // ---------------------------------------------------------------

            ["amb_garden"] = Loop("amb_garden", 4f, (t, i, p) =>
                Breathe(p, 3) * 0.16f * Noise(i / 41) + Breathe(p, 1) * 0.05f * Tone(t, 210f)),

            ["amb_clock"] = Loop("amb_clock", 4f, (t, i, p) =>
            {
                var tick = Mathf.Repeat(t, 0.5f);
                return Envelope(tick, 0.045f, 0.001f, 4f) * (Noise(i) * 0.16f + Tone(t, 1400f) * 0.10f) +
                       Breathe(p, 2) * 0.06f * Tone(t, 96f);
            }),

            ["amb_weather"] = Loop("amb_weather", 4f, (t, i, p) =>
                Breathe(p, 2) * 0.20f * Noise(i / 17) + Breathe(p, 5) * 0.08f * Noise(i / 3)),

            ["amb_canyon"] = Loop("amb_canyon", 4f, (t, i, p) =>
                Breathe(p, 1) * 0.24f * Noise(i / 53) + Breathe(p, 3) * 0.10f * Noise(i / 11)),

            ["amb_tide"] = Loop("amb_tide", 4f, (t, i, p) =>
                Breathe(p, 2) * 0.22f * Noise(i / 31) + Mathf.Sin(p * Mathf.PI * 2f) * 0.05f * Tone(t, 74f)),

            ["amb_storm"] = Loop("amb_storm", 4f, (t, i, p) =>
                0.26f * Noise(i / 2) * (0.7f + 0.3f * Breathe(p, 3)) + Breathe(p, 1) * 0.10f * Tone(t, 48f)),

            ["amb_cave"] = Loop("amb_cave", 4f, (t, i, p) =>
            {
                var drop = Mathf.Repeat(t, 1.3333f);
                return Envelope(drop, 0.30f, 0.001f, 3.4f) * Tone(drop, 900f) * 0.16f +
                       Breathe(p, 1) * 0.09f * Tone(t, 58f);
            }),

            ["amb_peak"] = Loop("amb_peak", 4f, (t, i, p) =>
                Breathe(p, 1) * 0.28f * Noise(i / 61) + Breathe(p, 4) * 0.07f * Noise(i / 9)),

            ["amb_abyss"] = Loop("amb_abyss", 4f, (t, i, p) =>
                Breathe(p, 1) * 0.10f * Tone(t, 52f) + Breathe(p, 2) * 0.07f * Tone(t, 78.5f) +
                Breathe(p, 6) * 0.06f * Noise(i / 71)),

            ["amb_orbit"] = Loop("amb_orbit", 4f, (t, i, p) =>
                0.07f * Tone(t, 64f) + 0.04f * Tone(t, 96f) + Breathe(p, 3) * 0.05f * Noise(i / 83)),

            ["amb_forge"] = Loop("amb_forge", 4f, (t, i, p) =>
            {
                var beat = Mathf.Repeat(t, 1f);
                return Envelope(beat, 0.16f, 0.002f, 2.6f) * (Tone(beat, 88f) * 0.24f + Noise(i) * 0.08f) +
                       0.10f * Tone(t, 55f) + Breathe(p, 2) * 0.08f * Noise(i / 13);
            }),

            ["amb_neon"] = Loop("amb_neon", 4f, (t, i, p) =>
                0.07f * Tone(t, 82.4f) + 0.05f * Tone(t, 123.5f) + 0.04f * Tone(t, 164.8f) +
                Breathe(p, 2) * 0.09f * Noise(i / 47)),
        };

        private static AudioClip Make(string id, float duration, Sample sample)
        {
            var count = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[count];
            var peak = 0f;
            for (var i = 0; i < count; i++)
            {
                data[i] = Mathf.Clamp(sample(i / (float)SampleRate, i), -1f, 1f);
                peak = Mathf.Max(peak, Mathf.Abs(data[i]));
            }

            var gain = peak > 0.86f ? 0.86f / peak : 1f;
            if (!Mathf.Approximately(gain, 1f))
            {
                for (var i = 0; i < data.Length; i++) data[i] *= gain;
            }

            var clip = AudioClip.Create($"Toy_{id}", count, 1, SampleRate, stream: false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// A bed clip. Peak normalisation is skipped on purpose — a bed is
        /// mixed by its source volume, and normalising each one would erase
        /// the intended difference between a near-silent orbit and a loud
        /// foundry.
        /// </summary>
        private static AudioClip Loop(string id, float duration, LoopSample sample)
        {
            var count = Mathf.CeilToInt(duration * SampleRate);
            var data = new float[count];
            for (var i = 0; i < count; i++)
            {
                data[i] = Mathf.Clamp(sample(i / (float)SampleRate, i, i / (float)count), -1f, 1f);
            }

            var clip = AudioClip.Create($"Toy_{id}", count, 1, SampleRate, stream: false);
            clip.SetData(data, 0);
            return clip;
        }

        /// <summary>
        /// A seamless swell: <paramref name="cycles"/> whole periods across the
        /// loop, so the last sample joins the first without a click.
        /// </summary>
        private static float Breathe(float progress, int cycles) =>
            0.5f + 0.5f * Mathf.Sin(progress * Mathf.PI * 2f * cycles);

        private static float BellPulse(float time, float start, float duration, float frequency)
        {
            var local = time - start;
            if (local < 0f || local >= duration) return 0f;
            var decay = Mathf.Exp(-local * (5f / duration));
            return decay * (Tone(local, frequency) + Tone(local, frequency * 2.01f) * 0.34f +
                            Tone(local, frequency * 3.97f) * 0.12f);
        }

        private static float Envelope(float time, float duration, float attack, float curve)
        {
            if (time < 0f || time >= duration) return 0f;
            var inGain = attack <= 0f ? 1f : Mathf.Clamp01(time / attack);
            return inGain * Mathf.Pow(1f - time / duration, curve);
        }

        private static float Tone(float time, float frequency) => Mathf.Sin(time * frequency * Mathf.PI * 2f);

        private static float Noise(int index)
        {
            unchecked
            {
                var n = (uint)index * 747796405u + 2891336453u;
                n = ((n >> ((int)(n >> 28) + 4)) ^ n) * 277803737u;
                n = (n >> 22) ^ n;
                return (n / (float)uint.MaxValue) * 2f - 1f;
            }
        }
    }
}
