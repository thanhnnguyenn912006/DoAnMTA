using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Systems;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Kết nối 2 thanh Slider chỉnh âm lượng Nhạc nền (BGM) và Hiệu ứng (SFX) với AudioManager.
    /// </summary>
    public class VolumeSettingsUI : MonoBehaviour
    {
        [Header("Thanh chỉnh Nhạc nền (BGM)")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private TMP_Text bgmValueText;

        [Header("Thanh chỉnh Hiệu ứng (SFX)")]
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TMP_Text sfxValueText;

        private void OnEnable()
        {
            SyncSlidersFromAudioManager();

            if (bgmSlider != null) bgmSlider.onValueChanged.AddListener(HandleBgmChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(HandleSfxChanged);
        }

        private void OnDisable()
        {
            if (bgmSlider != null) bgmSlider.onValueChanged.RemoveListener(HandleBgmChanged);
            if (sfxSlider != null) sfxSlider.onValueChanged.RemoveListener(HandleSfxChanged);
        }

        private void SyncSlidersFromAudioManager()
        {
            float bgm = AudioManager.Instance != null
                ? AudioManager.Instance.BgmVolume
                : PlayerPrefs.GetFloat(AudioManager.BgmVolumeKey, 0.45f);

            float sfx = AudioManager.Instance != null
                ? AudioManager.Instance.SfxVolume
                : PlayerPrefs.GetFloat(AudioManager.SfxVolumeKey, 0.8f);

            if (bgmSlider != null) bgmSlider.SetValueWithoutNotify(bgm);
            if (sfxSlider != null) sfxSlider.SetValueWithoutNotify(sfx);

            UpdateLabels(bgm, sfx);
        }

        private void HandleBgmChanged(float value)
        {
            AudioManager.Instance?.SetBgmVolume(value);
            if (bgmValueText != null)
            {
                bgmValueText.text = $"NHẠC NỀN: {Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private void HandleSfxChanged(float value)
        {
            AudioManager.Instance?.SetSfxVolume(value);
            AudioManager.Instance?.PlayButtonClick();
            if (sfxValueText != null)
            {
                sfxValueText.text = $"HIỆU ỨNG: {Mathf.RoundToInt(value * 100f)}%";
            }
        }

        private void UpdateLabels(float bgm, float sfx)
        {
            if (bgmValueText != null)
            {
                bgmValueText.text = $"NHẠC NỀN: {Mathf.RoundToInt(bgm * 100f)}%";
            }

            if (sfxValueText != null)
            {
                sfxValueText.text = $"HIỆU ỨNG: {Mathf.RoundToInt(sfx * 100f)}%";
            }
        }
    }
}