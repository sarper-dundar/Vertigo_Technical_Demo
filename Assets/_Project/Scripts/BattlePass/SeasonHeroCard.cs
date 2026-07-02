using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    [RequireComponent(typeof(UnityEngine.UI.RectMask2D))]
    public class SeasonHeroCard : MonoBehaviour
    {
        [Header("Season Info")]
        [SerializeField] private TextMeshProUGUI seasonNumberText;
        [SerializeField] private TextMeshProUGUI characterNameText;
        [SerializeField] private TextMeshProUGUI rarityText;
        [SerializeField] private Image characterImage;

        [Header("Badges")]
        [SerializeField] private TextMeshProUGUI premiumMultiplierText;
        [SerializeField] private GameObject premiumBadge;

        [Header("Action")]
        [SerializeField] private Button getButton;
        [SerializeField] private TextMeshProUGUI priceText;
        [SerializeField] private GameObject discountBanner;

        public void Initialize(string seasonNumber, string characterName, string rarity, Sprite characterSprite)
        {
            if (seasonNumberText != null)
                seasonNumberText.text = $"SEASON {seasonNumber}";

            if (characterNameText != null)
                characterNameText.text = characterName.ToUpper();

            if (rarityText != null)
                rarityText.text = rarity.ToUpper();

            if (characterImage != null && characterSprite != null)
                characterImage.sprite = characterSprite;
        }

        public void SetPremiumMultiplier(int multiplier)
        {
            if (premiumMultiplierText != null)
                premiumMultiplierText.text = $"PREMIUM x{multiplier}";

            if (premiumBadge != null)
                premiumBadge.SetActive(multiplier > 1);
        }

        public void SetPrice(string price, bool hasDiscount)
        {
            if (priceText != null)
                priceText.text = price;

            if (discountBanner != null)
                discountBanner.SetActive(hasDiscount);
        }
    }
}
