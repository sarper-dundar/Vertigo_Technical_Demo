using System;
using System.Collections;
using UnityEngine;
using TMPro;

namespace VertigoDemo.BattlePass
{
    public class CurrencyManager : MonoBehaviour
    {
        [Header("Starting Values")]
        [SerializeField] private int startingGold = 3403;
        [SerializeField] private int startingDiamonds = 18;

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI diamondText;

        [Header("Count Animation")]
        [SerializeField] private float countDuration = 0.5f;

        private int _gold;
        private int _diamonds;

        public int Gold => _gold;
        public int Diamonds => _diamonds;

        public event Action OnCurrencyChanged;

        private Coroutine _goldCountRoutine;
        private Coroutine _diamondCountRoutine;

        private void Awake()
        {
            _gold = startingGold;
            _diamonds = startingDiamonds;
            UpdateUI();
        }

        public void AddGold(int amount)
        {
            int from = _gold;
            _gold += amount;
            OnCurrencyChanged?.Invoke();
            AnimateCount(goldText, from, _gold, ref _goldCountRoutine);
        }

        public void AddDiamonds(int amount)
        {
            int from = _diamonds;
            _diamonds += amount;
            OnCurrencyChanged?.Invoke();
            AnimateCount(diamondText, from, _diamonds, ref _diamondCountRoutine);
        }

        public bool SpendDiamonds(int amount)
        {
            if (_diamonds < amount)
                return false;

            int from = _diamonds;
            _diamonds -= amount;
            OnCurrencyChanged?.Invoke();
            AnimateCount(diamondText, from, _diamonds, ref _diamondCountRoutine);
            return true;
        }

        private void AnimateCount(TextMeshProUGUI text, int from, int to, ref Coroutine routine)
        {
            if (text == null)
                return;

            if (routine != null)
                StopCoroutine(routine);

            routine = StartCoroutine(CountRoutine(text, from, to));
        }

        private IEnumerator CountRoutine(TextMeshProUGUI text, int from, int to)
        {
            float elapsed = 0f;

            while (elapsed < countDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / countDuration;
                int current = (int)Mathf.Lerp(from, to, t);
                text.text = current.ToString("N0");
                yield return null;
            }

            text.text = to.ToString("N0");
        }

        private void UpdateUI()
        {
            if (goldText != null)
                goldText.text = _gold.ToString("N0");
            if (diamondText != null)
                diamondText.text = _diamonds.ToString("N0");
        }
    }
}
