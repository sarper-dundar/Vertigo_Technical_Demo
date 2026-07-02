using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class LevelUpConfirmPopup : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private Image costCurrencyIcon;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private GameObject panel;

        private BattlePassManager _manager;

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(OnConfirm);
            if (cancelButton != null)
                cancelButton.onClick.AddListener(Hide);
        }

        public void Show(int diamondCost, int xpNeeded, BattlePassManager manager)
        {
            _manager = manager;
            panel.SetActive(true);

            if (descriptionText != null)
                descriptionText.text = $"You will buy {xpNeeded} XP to level up.\nAre you sure?";
            if (costText != null)
                costText.text = diamondCost.ToString();
        }

        private void OnConfirm()
        {
            _manager.ConfirmBuyLevel();
            Hide();
        }

        public void Hide()
        {
            panel.SetActive(false);
        }
    }
}
