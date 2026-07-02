using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class InsufficientFundsPopup : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button closeButton;
        [SerializeField] private GameObject panel;

        [Header("Buy Buttons")]
        [SerializeField] private Button buyButton1;
        [SerializeField] private int buyAmount1 = 10;
        [SerializeField] private Button buyButton2;
        [SerializeField] private int buyAmount2 = 30;
        [SerializeField] private Button buyButton3;
        [SerializeField] private int buyAmount3 = 80;

        [Header("References")]
        [SerializeField] private CurrencyFlyEffect currencyFlyEffect;
        [SerializeField] private CurrencyManager currencyManager;

        private void Awake()
        {
            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (buyButton1 != null)
                buyButton1.onClick.AddListener(() => OnBuy(buyAmount1));
            if (buyButton2 != null)
                buyButton2.onClick.AddListener(() => OnBuy(buyAmount2));
            if (buyButton3 != null)
                buyButton3.onClick.AddListener(() => OnBuy(buyAmount3));
        }

        public void Show(int cost, int current)
        {
            panel.SetActive(true);

            if (messageText != null)
                messageText.text = $"You need {cost} diamonds but only have {current}.\nYou need {cost - current} more!";
        }

        private void OnBuy(int amount)
        {
            if (currencyFlyEffect != null && currencyManager != null)
            {
                currencyFlyEffect.Play(CurrencyType.Diamond, amount, () =>
                {
                    currencyManager.AddDiamonds(amount);
                });
            }
            else if (currencyManager != null)
            {
                currencyManager.AddDiamonds(amount);
            }

            Hide();
        }

        public void Hide()
        {
            panel.SetActive(false);
        }
    }
}
