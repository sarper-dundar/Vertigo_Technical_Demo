using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class ScrollEdgeIndicator : MonoBehaviour
    {
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private GameObject leftArrow;
        [SerializeField] private GameObject rightArrow;

        [Header("End Reward Preview (right edge)")]
        [SerializeField] private GameObject endRewardPreview;
        [SerializeField] private Image endRewardIcon;

        [Header("Next Level Preview (left side)")]
        [SerializeField] private GameObject nextLevelPreview;
        [SerializeField] private TextMeshProUGUI nextLevelText;

        [Tooltip("How close to the edge (0-1) before hiding the arrow")]
        [SerializeField] private float edgeThreshold = 0.02f;

        private RectTransform _currentLevelTarget;
        private bool _isMaxLevel;

        public void SetEndRewardIcon(Sprite icon)
        {
            if (endRewardIcon != null && icon != null)
                endRewardIcon.sprite = icon;
        }

        public void SetNextLevel(int level, bool isMaxLevel)
        {
            _isMaxLevel = isMaxLevel;

            if (nextLevelText != null)
                nextLevelText.text = level.ToString();
        }

        public void SetCurrentLevelTarget(RectTransform target)
        {
            _currentLevelTarget = target;
        }

        private void Update()
        {
            if (scrollRect == null)
                return;

            float pos = scrollRect.horizontalNormalizedPosition;

            bool atLeft = pos <= edgeThreshold;
            bool atRight = pos >= 1f - edgeThreshold;

            if (leftArrow != null)
                leftArrow.SetActive(!atLeft);

            if (rightArrow != null)
                rightArrow.SetActive(!atRight);

            if (endRewardPreview != null)
                endRewardPreview.SetActive(!atRight);

            if (nextLevelPreview != null)
            {
                bool show = false;
                if (!_isMaxLevel && _currentLevelTarget != null && viewport != null)
                {
                    // show only when the current level dot has scrolled off the left side
                    float localX = viewport.InverseTransformPoint(_currentLevelTarget.position).x;
                    show = localX < viewport.rect.xMin;
                }
                nextLevelPreview.SetActive(show);
            }
        }
    }
}
