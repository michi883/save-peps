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

        public static Dictionary<string, AudioClip> Create() => new()
        {
            ["tap"] = Make("tap", 0.09f, (t, i) =>
                Envelope(t, 0.09f, 0.003f, 2.5f) *
                (Tone(t, Mathf.Lerp(1050f, 620f, t / 0.09f)) * 0.78f + Noise(i) * 0.12f)),

            ["wrong"] = Make("wrong", 0.34f, (t, _) =>
                BellPulse(t, 0f, 0.18f, 330f) * 0.55f + BellPulse(t, 0.13f, 0.20f, 247f) * 0.48f),

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
