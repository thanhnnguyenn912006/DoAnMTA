using TMPro;
using UnityEngine;
using UnityEngine.UI;
using SurvivorGame.Player;

namespace SurvivorGame.UI
{
    /// <summary>
    /// Hiển thị thanh kinh nghiệm và cấp độ hiện tại, lắng nghe sự kiện từ PlayerExperience.
    /// </summary>
    public class ExpBarUI : MonoBehaviour
    {
        [Header("Tham chiếu dữ liệu")]
        [SerializeField] private PlayerExperience playerExperience;

        [Header("Thành phần UI")]
        [SerializeField] private Slider expSlider;
        [SerializeField] private TMP_Text levelText;

        private void Awake()
        {
            if (playerExperience == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    playerExperience = playerObj.GetComponent<PlayerExperience>();
                }
            }
        }

        private void OnEnable()
        {
            if (playerExperience != null)
            {
                playerExperience.OnExpChanged += HandleExpChanged;
            }
        }

        private void OnDisable()
        {
            if (playerExperience != null)
            {
                playerExperience.OnExpChanged -= HandleExpChanged;
            }
        }

        private void Start()
        {
            if (playerExperience != null)
            {
                HandleExpChanged(
                    playerExperience.CurrentExp,
                    playerExperience.ExpToNextLevel,
                    playerExperience.CurrentLevel);
            }
        }

        private void HandleExpChanged(int currentExp, int expToNextLevel, int currentLevel)
        {
            if (expSlider != null)
            {
                expSlider.minValue = 0f;
                expSlider.maxValue = 1f;
                expSlider.value = expToNextLevel > 0 ? (float)currentExp / expToNextLevel : 0f;
            }

            if (levelText != null)
            {
                levelText.text = $"LV {currentLevel}";
            }
        }
    }
}