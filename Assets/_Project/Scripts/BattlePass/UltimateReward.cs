using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class UltimateReward : MonoBehaviour
    {
        [SerializeField] private Image rewardIcon;
        [SerializeField] private Image rewardFrame;
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private Image levelDot;

        [SerializeField] private float layoutWidth = 600f;

        public RectTransform DotTransform => levelDot != null ? levelDot.rectTransform : null;

        public void Initialize(RewardItemData data, Sprite dotSprite)
        {
            var layout = gameObject.GetComponent<LayoutElement>();
            if (layout == null)
                layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = layoutWidth;

            if (rewardIcon != null && data.icon != null)
                rewardIcon.sprite = data.icon;

            if (rewardFrame != null && data.frameSprite != null)
                rewardFrame.sprite = data.frameSprite;

            if (rewardNameText != null)
                rewardNameText.text = data.rewardName;

            if (levelDot != null && dotSprite != null)
                levelDot.sprite = dotSprite;
        }

        public void SetLocked(bool locked)
        {
            Color tint = locked ? new Color(0.45f, 0.45f, 0.45f, 1f) : Color.white;

            if (rewardIcon != null)
                rewardIcon.color = tint;
            if (rewardFrame != null)
                rewardFrame.color = tint;
            if (rewardNameText != null)
                rewardNameText.color = locked ? new Color(0.6f, 0.6f, 0.6f, 1f) : Color.white;
        }
    }
}
