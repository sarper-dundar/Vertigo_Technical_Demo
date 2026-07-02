using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class RewardClaimPopup : MonoBehaviour
    {
        [Header("Free Reward Display")]
        [SerializeField] private Image rewardIcon;
        [SerializeField] private TextMeshProUGUI rewardInfoText;

        [Header("Premium Rewards ScrollView")]
        [SerializeField] private Transform premiumRewardContent;
        [SerializeField] private GameObject premiumPreviewPrefab;

        [Header("Buttons")]
        [SerializeField] private Button gatherButton;
        [SerializeField] private Button activateTicketButton;

        [Header("Panel")]
        [SerializeField] private GameObject panel;

        [Header("Currency Fly")]
        [SerializeField] private CurrencyFlyEffect currencyFlyEffect;

        private RewardItemData _claimedReward;
        private CurrencyManager _currencyManager;
        private readonly List<GameObject> _spawnedMinis = new();

        private void Awake()
        {
            if (gatherButton != null)
                gatherButton.onClick.AddListener(OnGather);
            if (activateTicketButton != null)
                activateTicketButton.onClick.AddListener(Hide);
        }

        public void Show(RewardItemData claimedReward, List<LevelData> allLevels, CurrencyManager currencyManager)
        {
            _claimedReward = claimedReward;
            _currencyManager = currencyManager;
            panel.SetActive(true);

            if (rewardIcon != null && claimedReward.icon != null)
                rewardIcon.sprite = claimedReward.icon;

            if (rewardInfoText != null)
            {
                string info = claimedReward.amount > 1
                    ? $"{claimedReward.amount} {claimedReward.rewardName}"
                    : claimedReward.rewardName;
                rewardInfoText.text = info;
            }

            SpawnPremiumMinis(allLevels);
        }

        private void SpawnPremiumMinis(List<LevelData> allLevels)
        {
            foreach (var obj in _spawnedMinis)
                Destroy(obj);
            _spawnedMinis.Clear();

            if (premiumRewardContent == null || premiumPreviewPrefab == null)
                return;

            foreach (var level in allLevels)
            {
                var reward = level.premiumReward;
                var obj = Instantiate(premiumPreviewPrefab, premiumRewardContent);
                _spawnedMinis.Add(obj);

                var icon = obj.transform.Find("Icon");
                if (icon != null)
                {
                    var img = icon.GetComponent<Image>();
                    if (img != null && reward.icon != null)
                        img.sprite = reward.icon;
                }
            }
        }

        private void OnGather()
        {
            if (currencyFlyEffect != null && _claimedReward != null &&
                _claimedReward.currencyType != CurrencyType.None)
            {
                Hide();
                currencyFlyEffect.Play(_claimedReward.currencyType, _claimedReward.amount, () =>
                {
                    if (_currencyManager == null)
                        return;

                    switch (_claimedReward.currencyType)
                    {
                        case CurrencyType.Gold:
                            _currencyManager.AddGold(_claimedReward.amount);
                            break;
                        case CurrencyType.Diamond:
                            _currencyManager.AddDiamonds(_claimedReward.amount);
                            break;
                    }
                });
            }
            else
            {
                Hide();
            }
        }

        public void Hide()
        {
            panel.SetActive(false);
        }
    }
}
