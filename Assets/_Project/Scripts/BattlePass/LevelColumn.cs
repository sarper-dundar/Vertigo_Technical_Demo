using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class LevelColumn : MonoBehaviour
    {
        [Header("Reward Nodes")]
        [SerializeField] private RewardNode premiumNode;
        [SerializeField] private RewardNode freeNode;

        [Header("Level Indicator")]
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image levelDot;

        public RewardNode PremiumNode => premiumNode;
        public RewardNode FreeNode => freeNode;

        [Header("Dot Backgrounds")]
        [SerializeField] private Sprite acquiredDotSprite;
        [SerializeField] private Sprite unacquiredDotSprite;

        public RectTransform LevelDotTransform => levelDot != null ? levelDot.rectTransform : null;

        private LevelData _levelData;

        public void Initialize(LevelData levelData, BattlePassManager manager)
        {
            _levelData = levelData;
            levelText.text = levelData.level.ToString();

            premiumNode.Initialize(levelData.premiumReward, manager, isPremium: true);
            freeNode.Initialize(levelData.freeReward, manager, isPremium: false);
            UpdateDotBackground(manager.CurrentPlayerLevel);
        }

        public void RefreshVisuals()
        {
            premiumNode.UpdateVisuals();
            freeNode.UpdateVisuals();
        }

        public void UpdateDotBackground(int currentPlayerLevel)
        {
            if (levelDot == null)
                return;

            levelDot.sprite = _levelData.level <= currentPlayerLevel
                ? acquiredDotSprite
                : unacquiredDotSprite;
        }

        public void HideRewards()
        {
            premiumNode.gameObject.SetActive(false);
            freeNode.gameObject.SetActive(false);
        }
    }
}
