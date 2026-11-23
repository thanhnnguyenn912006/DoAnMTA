using System.Collections.Generic;
using UnityEngine;

namespace SurvivorGame.Systems
{
    /// <summary>
    /// Quản lý âm thanh tập trung (Singleton + DontDestroyOnLoad):
    /// Phát nhạc nền lặp, bộ SFX đa kênh có chống nghẽn tiếng và lưu âm lượng qua PlayerPrefs.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        public const string BgmVolumeKey = "Survivor_BgmVolume";
        public const string SfxVolumeKey = "Survivor_SfxVolume";

        public static AudioManager Instance { get; private set; }

        [Header("File âm thanh tùy chỉnh (để trống = dùng âm thanh tự tổng hợp)")]
        [SerializeField] private AudioClip customBgmClip;
        [SerializeField] private AudioClip customEnemyHitClip;
        [SerializeField] private AudioClip customPlayerHitClip;
        [SerializeField] private AudioClip customEnemyDeathClip;
        [SerializeField] private AudioClip customLevelUpClip;
        [SerializeField] private AudioClip customUpgradeSelectClip;
        [SerializeField] private AudioClip customBossSpawnClip;
        [SerializeField] private AudioClip customBossDefeatClip;
        [SerializeField] private AudioClip customVictoryClip;
        [SerializeField] private AudioClip customGameOverClip;
        [SerializeField] private AudioClip customButtonClickClip;

        [Header("Cấu hình bộ phát")]
        [SerializeField, Range(0f, 1f)] private float defaultBgmVolume = 0.45f;
        [SerializeField, Range(0f, 1f)] private float defaultSfxVolume = 0.8f;
        [SerializeField, Range(2, 12)] private int sfxChannelCount = 6;

        private readonly Dictionary<SoundId, AudioClip> _clips = new Dictionary<SoundId, AudioClip>();
        private readonly Dictionary<SoundId, float> _lastPlayedTimes = new Dictionary<SoundId, float>();

        private AudioSource _bgmSource;
        private AudioSource[] _sfxSources;
        private int _nextSfxChannel;

        public float BgmVolume { get; private set; }
        public float SfxVolume { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            BgmVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(BgmVolumeKey, defaultBgmVolume));
            SfxVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(SfxVolumeKey, defaultSfxVolume));

            SetupAudioSources();
            InitializeClips();
            StartBgm();
        }

        private void SetupAudioSources()
        {
            _bgmSource = gameObject.AddComponent<AudioSource>();
            _bgmSource.loop = true;
            _bgmSource.playOnAwake = false;
            _bgmSource.volume = BgmVolume;

            _sfxSources = new AudioSource[sfxChannelCount];
            for (int i = 0; i < sfxChannelCount; i++)
            {
                AudioSource src = gameObject.AddComponent<AudioSource>();
                src.loop = false;
                src.playOnAwake = false;
                _sfxSources[i] = src;
            }
        }

        private void InitializeClips()
        {
            RegisterClip(SoundId.EnemyHit, customEnemyHitClip);
            RegisterClip(SoundId.PlayerHit, customPlayerHitClip);
            RegisterClip(SoundId.EnemyDeath, customEnemyDeathClip);
            RegisterClip(SoundId.LevelUp, customLevelUpClip);
            RegisterClip(SoundId.UpgradeSelect, customUpgradeSelectClip);
            RegisterClip(SoundId.BossSpawn, customBossSpawnClip);
            RegisterClip(SoundId.BossDefeat, customBossDefeatClip);
            RegisterClip(SoundId.Victory, customVictoryClip);
            RegisterClip(SoundId.GameOver, customGameOverClip);
            RegisterClip(SoundId.ButtonClick, customButtonClickClip);
        }

        private void RegisterClip(SoundId id, AudioClip customClip)
        {
            _clips[id] = customClip != null ? customClip : ProceduralSfxFactory.CreateSfx(id);
        }

        private void StartBgm()
        {
            AudioClip bgmClip = customBgmClip != null ? customBgmClip : ProceduralSfxFactory.CreateAmbientBgm();
            if (_bgmSource != null && bgmClip != null)
            {
                _bgmSource.clip = bgmClip;
                _bgmSource.volume = BgmVolume;
                _bgmSource.Play();
            }
        }

        /// <summary>
        /// Phát một hiệu ứng âm thanh với tùy chọn giới hạn tần suất (chống nghẽn tiếng) và biến thiên cao độ nhẹ.
        /// </summary>
        public void PlaySfx(
            SoundId id,
            float volumeScale = 1f,
            float minInterval = 0f,
            float pitchJitter = 0f)
        {
            if (SfxVolume <= 0.001f || !_clips.TryGetValue(id, out AudioClip clip) || clip == null)
            {
                return;
            }

            float now = Time.unscaledTime;
            if (minInterval > 0f && _lastPlayedTimes.TryGetValue(id, out float lastTime) && now - lastTime < minInterval)
            {
                return;
            }
            _lastPlayedTimes[id] = now;

            AudioSource channel = _sfxSources[_nextSfxChannel];
            _nextSfxChannel = (_nextSfxChannel + 1) % _sfxSources.Length;

            channel.pitch = pitchJitter > 0f ? 1f + UnityEngine.Random.Range(-pitchJitter, pitchJitter) : 1f;
            channel.PlayOneShot(clip, Mathf.Clamp01(SfxVolume * volumeScale));
        }

        public void PlayButtonClick()
        {
            PlaySfx(SoundId.ButtonClick, 0.85f, 0.04f, 0.04f);
        }

        public void SetBgmVolume(float value)
        {
            BgmVolume = Mathf.Clamp01(value);
            if (_bgmSource != null)
            {
                _bgmSource.volume = BgmVolume;
            }

            PlayerPrefs.SetFloat(BgmVolumeKey, BgmVolume);
            PlayerPrefs.Save();
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = Mathf.Clamp01(value);
            PlayerPrefs.SetFloat(SfxVolumeKey, SfxVolume);
            PlayerPrefs.Save();
        }
    }
}