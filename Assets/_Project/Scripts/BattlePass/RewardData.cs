using UnityEngine;

namespace VertigoDemo.BattlePass
{
    public enum RewardState
    {
        Locked,
        Unlocked,
        Claimable,
        Claimed
    }

    public enum CurrencyType
    {
        None,
        Gold,
        Diamond
    }

    [System.Serializable]
    public class RewardItemData
    {
        public string rewardName;
        public Sprite icon;
        public Sprite frameSprite;
        public int amount = 1;
        public CurrencyType currencyType;
        public Sprite currencyIcon;

        [HideInInspector]
        public RewardState state;
    }

    [System.Serializable]
    public class LevelData
    {
        public int level;
        public RewardItemData freeReward;
        public RewardItemData premiumReward;
    }
}
