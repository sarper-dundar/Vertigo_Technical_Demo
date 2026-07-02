using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace VertigoDemo.BattlePass
{
    [RequireComponent(typeof(Button))]
    public class ProgressEndpoint : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI costText;

        private RectTransform _rect;
        private BattlePassManager _manager;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            GetComponent<Button>().onClick.AddListener(OnClicked);

            var layout = gameObject.AddComponent<LayoutElement>();
            layout.ignoreLayout = true;

            _rect.anchorMin = new Vector2(0, 0.5f);
            _rect.anchorMax = new Vector2(0, 0.5f);
            _rect.pivot = new Vector2(0.5f, 0.5f);
        }

        public void Initialize(BattlePassManager manager)
        {
            _manager = manager;
        }

        public void SetCost(int cost)
        {
            if (costText != null)
                costText.text = cost.ToString();
        }

        public void SetPosition(float x, float y)
        {
            _rect.anchoredPosition = new Vector2(x, y);
        }

        private void OnClicked()
        {
            _manager.RequestBuyLevel();
        }
    }
}
