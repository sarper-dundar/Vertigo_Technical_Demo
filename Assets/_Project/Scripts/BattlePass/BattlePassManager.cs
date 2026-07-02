using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class BattlePassManager : MonoBehaviour
    {
        [Header("Data")]
        [SerializeField] private List<LevelData> levels = new();
        [SerializeField] private int currentPlayerLevel = 5;
        [SerializeField] private int currentXP = 60;
        [SerializeField] private int xpPerLevel = 200;
        [SerializeField] private int advanceCost = 20;
        [SerializeField] private int costIncrement = 5;

        [Header("UI References")]
        [SerializeField] private Transform contentParent;
        [SerializeField] private GameObject levelColumnPrefab;
        [SerializeField] private CurrencyManager currencyManager;

        [Header("Progress Road")]
        [SerializeField] private ProgressRoad progressRoad;

        [Header("XP Bar (Header)")]
        [SerializeField] private Image xpBarFill;
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private TextMeshProUGUI levelNumberText;

        [Header("Progress Endpoint")]
        [SerializeField] private GameObject endpointPrefab;
        [SerializeField] private float progressYOffset = 0f;

        [Header("Ultimate Reward")]
        [SerializeField] private GameObject ultimateRewardPrefab;
        [SerializeField] private Sprite ultimateDotSprite;
        [SerializeField] private RewardItemData ultimateRewardData;

        [Header("Popups")]
        [SerializeField] private RewardClaimPopup claimPopup;
        [SerializeField] private LevelUpConfirmPopup levelUpPopup;
        [SerializeField] private InsufficientFundsPopup insufficientFundsPopup;

        [Header("Effects")]
        [SerializeField] private CurrencyFlyEffect currencyFlyEffect;

        [Header("Edge Indicators")]
        [SerializeField] private ScrollEdgeIndicator edgeIndicator;

        private readonly List<LevelColumn> _spawnedColumns = new();
        private ProgressEndpoint _activeEndpoint;
        private UltimateReward _ultimateReward;

        public int CurrentPlayerLevel => currentPlayerLevel;

        private int MaxLevel => levels.Count > 0 ? levels[levels.Count - 1].level : 0;

        private IEnumerator Start()
        {
            ComputeStates();
            SpawnColumns();
            SpawnUltimateReward();
            UpdateXPBar();
            UpdateEdgeIndicator();

            yield return null;

            RectTransform ultimateDot = _ultimateReward != null ? _ultimateReward.DotTransform : null;
            progressRoad.SpawnSegments(_spawnedColumns, contentParent, currentPlayerLevel, levels, ultimateDot);

            if (_ultimateReward != null)
                _ultimateReward.transform.SetAsLastSibling();

            if (!IsMaxLevel())
                SpawnEndpoint();
        }

        private void ComputeStates()
        {
            foreach (var level in levels)
            {
                if (level.freeReward.state != RewardState.Claimed)
                    level.freeReward.state = ComputeState(level.level);
                if (level.premiumReward.state != RewardState.Claimed)
                    level.premiumReward.state = ComputeState(level.level);
            }
        }

        private RewardState ComputeState(int level)
        {
            if (level > currentPlayerLevel)
                return RewardState.Locked;
            if (level == currentPlayerLevel)
                return RewardState.Claimable;
            return RewardState.Unlocked;
        }

        private void SpawnColumns()
        {
            foreach (var level in levels)
            {
                var columnObj = Instantiate(levelColumnPrefab, contentParent);
                var column = columnObj.GetComponent<LevelColumn>();
                column.Initialize(level, this);
                _spawnedColumns.Add(column);
            }
        }

        private void SpawnUltimateReward()
        {
            if (ultimateRewardPrefab == null || levels.Count == 0)
                return;

            var obj = Instantiate(ultimateRewardPrefab, contentParent);
            _ultimateReward = obj.GetComponent<UltimateReward>();

            if (_ultimateReward == null)
                return;

            _ultimateReward.Initialize(ultimateRewardData, ultimateDotSprite);
            _ultimateReward.SetLocked(!IsMaxLevel());
        }

        private void UpdateEdgeIndicator()
        {
            if (edgeIndicator == null)
                return;

            if (ultimateRewardData != null)
                edgeIndicator.SetEndRewardIcon(ultimateRewardData.icon);

            edgeIndicator.SetNextLevel(currentPlayerLevel + 1, IsMaxLevel());

            int index = GetCurrentLevelIndex();
            if (index >= 0 && index < _spawnedColumns.Count)
                edgeIndicator.SetCurrentLevelTarget(_spawnedColumns[index].LevelDotTransform);
        }

        private bool IsMaxLevel()
        {
            return currentPlayerLevel >= MaxLevel;
        }

        public void ClaimReward(RewardNode node)
        {
            if (node.Data.state != RewardState.Claimable && node.Data.state != RewardState.Unlocked)
                return;

            node.Data.state = RewardState.Claimed;
            node.UpdateVisuals();

            if (claimPopup != null)
                claimPopup.Show(node.Data, levels, currencyManager);
        }

        public void RequestBuyLevel()
        {
            if (IsMaxLevel())
                return;

            if (currencyManager != null && currencyManager.Diamonds < advanceCost)
            {
                if (insufficientFundsPopup != null)
                    insufficientFundsPopup.Show(advanceCost, currencyManager.Diamonds);
                return;
            }

            if (levelUpPopup != null)
            {
                int xpNeeded = xpPerLevel - currentXP;
                levelUpPopup.Show(advanceCost, xpNeeded, this);
            }
        }

        public void ConfirmBuyLevel()
        {
            if (IsMaxLevel())
                return;

            if (currencyManager == null || !currencyManager.SpendDiamonds(advanceCost))
                return;

            DestroyEndpoint();

            if (currencyFlyEffect != null)
            {
                currencyFlyEffect.PlayXP(() =>
                {
                    StartCoroutine(AnimateLevelUp());
                });
            }
            else
            {
                ApplyLevelUp();
            }
        }

        private IEnumerator AnimateLevelUp()
        {
            float fillDuration = 0.5f;
            float elapsed = 0f;
            float startFill = xpBarFill != null ? xpBarFill.fillAmount : 0f;

            while (elapsed < fillDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fillDuration;
                if (xpBarFill != null)
                    xpBarFill.fillAmount = Mathf.Lerp(startFill, 1f, t);
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            ApplyLevelUp();
        }

        private void ApplyLevelUp()
        {
            int previousLevel = currentPlayerLevel;
            currentPlayerLevel++;
            currentXP = 0;
            advanceCost += costIncrement;

            ComputeStates();

            foreach (var column in _spawnedColumns)
            {
                column.RefreshVisuals();
                column.UpdateDotBackground(currentPlayerLevel);
            }

            UpdateXPBar();
            UpdateEdgeIndicator();

            if (IsMaxLevel() && _ultimateReward != null)
                _ultimateReward.SetLocked(false);

            int prevSegIndex = progressRoad.GetSegmentIndexForLevel(previousLevel, levels);
            int newSegIndex = progressRoad.GetSegmentIndexForLevel(currentPlayerLevel, levels);

            if (prevSegIndex >= 0)
            {
                progressRoad.AnimateFillTo(prevSegIndex, 1f, () =>
                {
                    if (!IsMaxLevel() && newSegIndex >= 0)
                    {
                        progressRoad.AnimateFillTo(newSegIndex, 0.5f, () => SpawnEndpoint());
                    }
                    else
                    {
                        SpawnEndpoint();
                    }
                });
            }
            else
            {
                RectTransform uDot = _ultimateReward != null ? _ultimateReward.DotTransform : null;
                progressRoad.SpawnSegments(_spawnedColumns, contentParent, currentPlayerLevel, levels, uDot);

                if (!IsMaxLevel())
                    SpawnEndpoint();
            }
        }

        private void SpawnEndpoint()
        {
            if (endpointPrefab == null || progressRoad == null)
                return;

            var pos = progressRoad.FillEndPoint;
            var obj = Instantiate(endpointPrefab, contentParent);
            _activeEndpoint = obj.GetComponent<ProgressEndpoint>();
            _activeEndpoint.Initialize(this);
            _activeEndpoint.SetCost(advanceCost);
            _activeEndpoint.SetPosition(pos.x, pos.y + progressYOffset);
        }

        private void DestroyEndpoint()
        {
            if (_activeEndpoint != null)
            {
                Destroy(_activeEndpoint.gameObject);
                _activeEndpoint = null;
            }
        }

        private int GetCurrentLevelIndex()
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].level == currentPlayerLevel)
                    return i;
            }
            return -1;
        }

        private void UpdateXPBar()
        {
            if (xpBarFill != null)
                xpBarFill.fillAmount = (float)currentXP / xpPerLevel;
            if (xpText != null)
                xpText.text = $"{currentXP}/{xpPerLevel}";
            if (levelNumberText != null)
                levelNumberText.text = currentPlayerLevel.ToString();
        }
    }
}
