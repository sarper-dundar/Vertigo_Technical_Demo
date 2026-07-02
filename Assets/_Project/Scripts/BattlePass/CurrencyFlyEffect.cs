using System;
using System.Collections;
using UnityEngine;

namespace VertigoDemo.BattlePass
{
    public class CurrencyFlyEffect : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject coinIconPrefab;
        [SerializeField] private GameObject diamondIconPrefab;
        [SerializeField] private GameObject xpIconPrefab;

        [Header("Targets")]
        [SerializeField] private RectTransform goldTarget;
        [SerializeField] private RectTransform diamondTarget;
        [SerializeField] private RectTransform xpBarTarget;

        [Header("Spawn")]
        [SerializeField] private RectTransform spawnPoint;
        [SerializeField] private Canvas rootCanvas;

        [Header("Settings")]
        [SerializeField] private int iconCount = 8;
        [SerializeField] private float flyDuration = 0.6f;
        [SerializeField] private float spawnSpread = 80f;
        [SerializeField] private float staggerDelay = 0.05f;

        public void Play(CurrencyType type, int amount, Action onComplete = null)
        {
            GameObject prefab;
            RectTransform target;

            switch (type)
            {
                case CurrencyType.Gold:
                    prefab = coinIconPrefab;
                    target = goldTarget;
                    break;
                case CurrencyType.Diamond:
                    prefab = diamondIconPrefab;
                    target = diamondTarget;
                    break;
                default:
                    onComplete?.Invoke();
                    return;
            }

            if (prefab == null || target == null || spawnPoint == null)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(SpawnAndFly(prefab, target, onComplete));
        }

        public void PlayXP(Action onComplete = null)
        {
            if (xpIconPrefab == null || xpBarTarget == null || spawnPoint == null)
            {
                onComplete?.Invoke();
                return;
            }

            StartCoroutine(SpawnAndFly(xpIconPrefab, xpBarTarget, onComplete));
        }

        private IEnumerator SpawnAndFly(GameObject prefab, RectTransform target, Action onComplete)
        {
            var wait = new WaitForSeconds(staggerDelay);
            int remaining = iconCount;

            for (int i = 0; i < iconCount; i++)
            {
                var obj = Instantiate(prefab, rootCanvas != null ? rootCanvas.transform : transform);
                var rect = obj.GetComponent<RectTransform>();

                Vector2 randomOffset = UnityEngine.Random.insideUnitCircle * spawnSpread;
                rect.position = spawnPoint.position + (Vector3)randomOffset;

                StartCoroutine(FlyIcon(rect, target, () => remaining--));
                yield return wait;
            }

            while (remaining > 0)
                yield return null;

            onComplete?.Invoke();
        }

        private IEnumerator FlyIcon(RectTransform icon, RectTransform target, Action onDone)
        {
            Vector3 startPos = icon.position;
            float elapsed = 0f;

            while (elapsed < flyDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flyDuration;
                float ease = t * t;
                icon.position = Vector3.Lerp(startPos, target.position, ease);
                float scale = 1f - ease * 0.5f;
                icon.localScale = Vector3.one * scale;
                yield return null;
            }

            Destroy(icon.gameObject);
            onDone?.Invoke();
        }
    }
}
