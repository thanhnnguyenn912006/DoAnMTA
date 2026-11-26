using System;
using UnityEngine;

namespace SurvivorGame.Systems
{
    public enum SoundId
    {
        EnemyHit,
        PlayerHit,
        EnemyDeath,
        LevelUp,
        UpgradeSelect,
        BossSpawn,
        BossDefeat,
        Victory,
        GameOver,
        ButtonClick,
        Dash,
        PickupHeal,
        PickupMagnet,
        WeaponEvolve,
        BossWarning,
        ExpPickup,
        ChestOpen
    }

    /// <summary>
    /// Tổng hợp sẵn các AudioClip hiệu ứng chiến đấu và nhạc nền bằng thuật toán sóng âm (Procedural Audio),
    /// giúp game có đầy đủ âm thanh ngay cả khi chưa gán file âm thanh ngoài.
    /// </summary>
    public static class ProceduralSfxFactory
    {
        private const int SampleRate = 44100;

        public static AudioClip CreateSfx(SoundId id)
        {
            return id switch
            {
                SoundId.EnemyHit => CreateSweep("SFX_EnemyHit", 0.065f, 540f, 240f, 0.35f, 2.2f, 0.15f, 101),
                SoundId.PlayerHit => CreateSweep("SFX_PlayerHit", 0.18f, 175f, 58f, 0.55f, 1.8f, 0.35f, 102),
                SoundId.EnemyDeath => CreateSweep("SFX_EnemyDeath", 0.12f, 340f, 85f, 0.42f, 2.0f, 0.45f, 103),
                SoundId.LevelUp => CreateArpeggio("SFX_LevelUp", new[] { 523.25f, 659.25f, 783.99f, 1046.50f }, 0.105f, 0.45f),
                SoundId.UpgradeSelect => CreateArpeggio("SFX_UpgradeSelect", new[] { 659.25f, 987.77f }, 0.075f, 0.40f),
                SoundId.BossSpawn => CreateBossSiren("SFX_BossSpawn", 0.85f, 0.50f),
                SoundId.BossDefeat => CreateArpeggio("SFX_BossDefeat", new[] { 392.00f, 523.25f, 659.25f, 783.99f, 1046.50f }, 0.11f, 0.50f),
                SoundId.Victory => CreateArpeggio("SFX_Victory", new[] { 523.25f, 659.25f, 783.99f, 987.77f, 1046.50f }, 0.13f, 0.50f),
                SoundId.GameOver => CreateArpeggio("SFX_GameOver", new[] { 329.63f, 293.66f, 246.94f, 196.00f }, 0.18f, 0.45f),
                SoundId.ButtonClick => CreateSweep("SFX_ButtonClick", 0.045f, 760f, 520f, 0.30f, 2.5f, 0.05f, 110),
                SoundId.Dash => CreateSweep("SFX_Dash", 0.11f, 680f, 220f, 0.38f, 2.0f, 0.35f, 111),
                SoundId.PickupHeal => CreateArpeggio("SFX_PickupHeal", new[] { 523.25f, 659.25f, 783.99f }, 0.065f, 0.42f),
                SoundId.PickupMagnet => CreateSweep("SFX_PickupMagnet", 0.24f, 260f, 920f, 0.42f, 1.3f, 0.08f, 112),
                SoundId.WeaponEvolve => CreateArpeggio("SFX_WeaponEvolve", new[] { 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f }, 0.09f, 0.50f),
                SoundId.BossWarning => CreateBossSiren("SFX_BossWarning", 0.65f, 0.45f),
                SoundId.ExpPickup => CreateSweep("SFX_ExpPickup", 0.048f, 880f, 1320f, 0.28f, 2.4f, 0f, 113),
                SoundId.ChestOpen => CreateArpeggio("SFX_ChestOpen", new[] { 523.25f, 659.25f, 783.99f, 1046.50f, 1174.66f, 1567.98f }, 0.075f, 0.48f),
                _ => CreateSweep("SFX_Default", 0.05f, 440f, 320f, 0.3f, 2f, 0f, 100)
            };
        }

        /// <summary>
        /// Tạo vòng lặp nhạc nền 8 giây tông tối êm dịu (A minor) phù hợp bối cảnh sinh tồn.
        /// </summary>
        public static AudioClip CreateAmbientBgm()
        {
            const float duration = 8f;
            int totalSamples = Mathf.RoundToInt(SampleRate * duration);
            float[] data = new float[totalSamples];

            // Hợp âm nền nhẹ: Am -> F -> C -> G (mỗi ô nhịp 2 giây)
            float[] bassNotes = { 110.00f, 87.31f, 130.81f, 98.00f };
            float[] arpNotes =
            {
                220.00f, 261.63f, 329.63f, 261.63f, // Am
                174.61f, 220.00f, 261.63f, 220.00f, // F
                261.63f, 329.63f, 392.00f, 329.63f, // C
                196.00f, 246.94f, 293.66f, 246.94f  // G
            };

            float bassPhase = 0f;
            float arpPhase = 0f;

            for (int i = 0; i < totalSamples; i++)
            {
                float time = (float)i / SampleRate;

                int barIndex = Mathf.Clamp((int)(time / 2f), 0, bassNotes.Length - 1);
                float bassFreq = bassNotes[barIndex];
                bassPhase += bassFreq / SampleRate;
                float bassSample = Mathf.Sin(2f * Mathf.PI * bassPhase) * 0.20f;

                int stepIndex = Mathf.Clamp((int)(time / 0.5f), 0, arpNotes.Length - 1);
                float stepLocalT = (time % 0.5f) / 0.5f;
                float arpEnv = Mathf.Sin(Mathf.PI * Mathf.Clamp01(stepLocalT)) * Mathf.Pow(1f - stepLocalT, 0.6f);
                arpPhase += arpNotes[stepIndex] / SampleRate;
                float arpSample = TriangleWave(arpPhase) * 0.11f * arpEnv;

                data[i] = Mathf.Clamp(bassSample + arpSample, -0.9f, 0.9f);
            }

            ApplyEdgeFade(data, 256);

            AudioClip clip = AudioClip.Create("BGM_ProceduralLoop", totalSamples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateSweep(
            string clipName,
            float duration,
            float startFreq,
            float endFreq,
            float amplitude,
            float decayPower,
            float noiseMix,
            int seed)
        {
            int samples = Mathf.Max(64, Mathf.RoundToInt(SampleRate * duration));
            float[] data = new float[samples];
            System.Random rng = new System.Random(seed);
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float freq = Mathf.Lerp(startFreq, endFreq, t);
                phase += freq / SampleRate;

                float tone = TriangleWave(phase);
                float noise = (float)(rng.NextDouble() * 2.0 - 1.0);
                float env = Mathf.Pow(1f - t, decayPower);

                data[i] = Mathf.Lerp(tone, noise, noiseMix) * amplitude * env;
            }

            ApplyEdgeFade(data, 64);

            AudioClip clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArpeggio(string clipName, float[] frequencies, float noteDuration, float amplitude)
        {
            int samplesPerNote = Mathf.Max(128, Mathf.RoundToInt(SampleRate * noteDuration));
            int totalSamples = samplesPerNote * frequencies.Length;
            float[] data = new float[totalSamples];

            for (int n = 0; n < frequencies.Length; n++)
            {
                float freq = frequencies[n];
                float phase = 0f;
                int offset = n * samplesPerNote;

                for (int i = 0; i < samplesPerNote; i++)
                {
                    float t = (float)i / samplesPerNote;
                    phase += freq / SampleRate;

                    float attack = Mathf.Clamp01(t / 0.08f);
                    float release = Mathf.Pow(1f - t, 1.35f);
                    float wave = Mathf.Sin(2f * Mathf.PI * phase) * 0.7f + TriangleWave(phase * 2f) * 0.3f;

                    data[offset + i] = wave * amplitude * attack * release;
                }
            }

            ApplyEdgeFade(data, 64);

            AudioClip clip = AudioClip.Create(clipName, totalSamples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateBossSiren(string clipName, float duration, float amplitude)
        {
            int samples = Mathf.Max(256, Mathf.RoundToInt(SampleRate * duration));
            float[] data = new float[samples];
            float phase = 0f;

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / samples;
                float pulse = Mathf.Sin(t * Mathf.PI * 4f) * 0.5f + 0.5f;
                float freq = Mathf.Lerp(95f, 175f, pulse);
                phase += freq / SampleRate;

                float wave = Mathf.Sin(2f * Mathf.PI * phase) * 0.65f + TriangleWave(phase * 0.5f) * 0.35f;
                float env = Mathf.Sin(Mathf.PI * t);
                data[i] = wave * amplitude * env;
            }

            ApplyEdgeFade(data, 128);

            AudioClip clip = AudioClip.Create(clipName, samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static float TriangleWave(float phase)
        {
            float frac = phase - Mathf.Floor(phase + 0.5f);
            return 2f * Mathf.Abs(2f * frac) - 1f;
        }

        private static void ApplyEdgeFade(float[] data, int fadeSamples)
        {
            int count = Mathf.Min(fadeSamples, data.Length / 2);
            for (int i = 0; i < count; i++)
            {
                float factor = (float)i / count;
                data[i] *= factor;
                data[data.Length - 1 - i] *= factor;
            }
        }
    }
}
