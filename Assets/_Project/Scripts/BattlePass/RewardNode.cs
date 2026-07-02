using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class RewardNode : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Image frameImage;
        [SerializeField] private Image iconImage;
        [SerializeField] private Image lockOverlay;
        [SerializeField] private Image checkmarkOverlay;
        [SerializeField] private Image glowEffect;
        [SerializeField] private GameObject notificationBadge;

        [Header("Reward Info")]
        [SerializeField] private TextMeshProUGUI rewardNameText;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private Image currencyIconImage;

        [Header("State Colors")]
        [SerializeField] private Color lockedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color claimedColor = new Color(0.7f, 0.7f, 0.7f, 1f);

        private RewardItemData _data;
        private BattlePassManager _manager;
        private bool _isPremium;

        public RewardItemData Data => _data;
        public bool IsPremium => _isPremium;

        public void Initialize(RewardItemData data, BattlePassManager manager, bool isPremium = false)
        {
            _data = data;
            _manager = manager;
            _isPremium = isPremium;

            if (data.icon != null)
                iconImage.sprite = data.icon;

            if (data.frameSprite != null)
                frameImage.sprite = data.frameSprite;

            if (rewardNameText != null)
                rewardNameText.text = data.rewardName;

            if (amountText != null)
                amountText.text = data.amount > 1 ? data.amount.ToString() : "";

            if (currencyIconImage != null && data.currencyIcon != null)
                currencyIconImage.sprite = data.currencyIcon;
            else if (currencyIconImage != null)
                currencyIconImage.gameObject.SetActive(false);

            var button = GetComponentInChildren<Button>();
            if (button != null)
            {
                if (isPremium)
                {
                    button.interactable = false;
                }
                else
                {
                    button.onClick.AddListener(OnNodeClicked);
                }
            }

            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            bool showLock = _isPremium || _data.state == RewardState.Locked;
            lockOverlay.gameObject.SetActive(showLock);
            checkmarkOverlay.gameObject.SetActive(!_isPremium && _data.state == RewardState.Claimed);
            glowEffect.gameObject.SetActive(!_isPremium && _data.state == RewardState.Claimable);

            bool showBadge = !_isPremium && (_data.state == RewardState.Claimable || _data.state == RewardState.Unlocked);
            if (notificationBadge != null)
                notificationBadge.SetActive(showBadge);

            switch (_data.state)
            {
                case RewardState.Locked:
                    frameImage.color = lockedColor;
                    iconImage.color = lockedColor;
                    break;

                case RewardState.Unlocked:
                case RewardState.Claimable:
                    frameImage.color = normalColor;
                    iconImage.color = normalColor;
                    break;

                case RewardState.Claimed:
                    frameImage.color = claimedColor;
                    iconImage.color = claimedColor;
                    break;
            }
        }

        public void OnNodeClicked()
        {
            if (_data.state == RewardState.Claimable || _data.state == RewardState.Unlocked)
                _manager.ClaimReward(this);
        }
    }
}
