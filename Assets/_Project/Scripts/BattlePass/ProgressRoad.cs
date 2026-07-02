using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.BattlePass
{
    public class ProgressRoad : MonoBehaviour
    {
        [SerializeField] private GameObject segmentPrefab;
        [SerializeField] private Transform segmentParent;
        [SerializeField] private float barHeight = 16f;
        [SerializeField] private float fillAnimDuration = 0.6f;

        [Header("Fill Head")]
        [SerializeField] private RectTransform fillHead;
        [SerializeField] private Vector2 fillHeadOffset = Vector2.zero;

        private readonly List<GameObject> _segmentObjects = new();
        private readonly List<Image> _segmentFills = new();
        private readonly List<float> _segmentStartX = new();
        private readonly List<float> _segmentEndX = new();
        private readonly List<float> _segmentY = new();

        private Vector2 _fillEndPoint;
        public Vector2 FillEndPoint => _fillEndPoint;

        private Transform _contentParent;

        private void UpdateFillHead()
        {
            if (fillHead == null || _contentParent == null)
                return;

            // convert from content-local to world, then to fillHead's parent-local
            Vector3 worldPos = _contentParent.TransformPoint(new Vector3(_fillEndPoint.x, _fillEndPoint.y, 0));
            Vector3 localPos = fillHead.parent.InverseTransformPoint(worldPos);
            fillHead.anchoredPosition = new Vector2(localPos.x + fillHeadOffset.x, localPos.y + fillHeadOffset.y);
        }

        private void LateUpdate()
        {
            UpdateFillHead();
        }

        public void SpawnSegments(List<LevelColumn> columns, Transform contentParent, int currentLevel, List<LevelData> levels, RectTransform ultimateDot = null)
        {
            foreach (var obj in _segmentObjects)
                Destroy(obj);
            _segmentObjects.Clear();
            _segmentFills.Clear();
            _segmentStartX.Clear();
            _segmentEndX.Clear();
            _segmentY.Clear();

            _fillEndPoint = Vector2.zero;
            _contentParent = contentParent;

            for (int i = 0; i < columns.Count - 1; i++)
            {
                var dotA = columns[i].LevelDotTransform;
                var dotB = columns[i + 1].LevelDotTransform;

                float ax = contentParent.InverseTransformPoint(dotA.position).x;
                float bx = contentParent.InverseTransformPoint(dotB.position).x;
                float y = contentParent.InverseTransformPoint(dotA.position).y;

                var segObj = Instantiate(segmentPrefab, segmentParent);
                segObj.transform.SetAsFirstSibling();
                _segmentObjects.Add(segObj);

                var segRect = segObj.GetComponent<RectTransform>();
                segRect.anchorMin = new Vector2(0, 0.5f);
                segRect.anchorMax = new Vector2(0, 0.5f);
                segRect.pivot = new Vector2(0, 0.5f);
                segRect.anchoredPosition = new Vector2(ax, y);
                segRect.sizeDelta = new Vector2(bx - ax, barHeight);

                var fillImage = segObj.transform.Find("Fill").GetComponent<Image>();
                _segmentFills.Add(fillImage);
                _segmentStartX.Add(ax);
                _segmentEndX.Add(bx);
                _segmentY.Add(y);

                int levelAtThisIndex = levels[i].level;
                float fillAmount;

                if (levelAtThisIndex < currentLevel)
                    fillAmount = 1f;
                else if (levelAtThisIndex == currentLevel)
                    fillAmount = 0.5f;
                else
                    fillAmount = 0f;

                fillImage.fillAmount = fillAmount;

                if (levelAtThisIndex == currentLevel)
                {
                    float fillX = ax + (bx - ax) * fillAmount;
                    _fillEndPoint = new Vector2(fillX, y);
                }
            }

            if (ultimateDot != null && columns.Count > 0)
            {
                var lastDot = columns[columns.Count - 1].LevelDotTransform;
                float ax = contentParent.InverseTransformPoint(lastDot.position).x;
                float bx = contentParent.InverseTransformPoint(ultimateDot.position).x;
                float y = contentParent.InverseTransformPoint(lastDot.position).y;

                var segObj = Instantiate(segmentPrefab, segmentParent);
                segObj.transform.SetAsFirstSibling();
                _segmentObjects.Add(segObj);

                var segRect = segObj.GetComponent<RectTransform>();
                segRect.anchorMin = new Vector2(0, 0.5f);
                segRect.anchorMax = new Vector2(0, 0.5f);
                segRect.pivot = new Vector2(0, 0.5f);
                segRect.anchoredPosition = new Vector2(ax, y);
                segRect.sizeDelta = new Vector2(bx - ax, barHeight);

                var fillImage = segObj.transform.Find("Fill").GetComponent<Image>();
                _segmentFills.Add(fillImage);
                _segmentStartX.Add(ax);
                _segmentEndX.Add(bx);
                _segmentY.Add(y);

                int lastLevel = levels[levels.Count - 1].level;
                float fill = lastLevel < currentLevel ? 1f : 0f;
                fillImage.fillAmount = fill;
            }

            UpdateFillHead();
        }

        public void AnimateFillTo(int segmentIndex, float targetFill, Action onComplete)
        {
            if (segmentIndex < 0 || segmentIndex >= _segmentFills.Count)
            {
                onComplete?.Invoke();
                return;
            }

            float ax = _segmentStartX[segmentIndex];
            float bx = _segmentEndX[segmentIndex];
            float y = _segmentY[segmentIndex];

            StartCoroutine(AnimateFillRoutine(segmentIndex, targetFill, ax, bx, y, onComplete));
        }

        private IEnumerator AnimateFillRoutine(int index, float targetFill, float ax, float bx, float y, Action onComplete)
        {
            var fill = _segmentFills[index];
            float startFill = fill.fillAmount;
            float elapsed = 0f;

            while (elapsed < fillAnimDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fillAnimDuration;
                float current = Mathf.Lerp(startFill, targetFill, t);
                fill.fillAmount = current;
                _fillEndPoint = new Vector2(ax + (bx - ax) * current, y);
                UpdateFillHead();
                yield return null;
            }

            fill.fillAmount = targetFill;
            _fillEndPoint = new Vector2(ax + (bx - ax) * targetFill, y);
            UpdateFillHead();
            onComplete?.Invoke();
        }

        public int GetSegmentIndexForLevel(int level, List<LevelData> levels)
        {
            for (int i = 0; i < levels.Count; i++)
            {
                if (levels[i].level == level)
                    return i;
            }
            return -1;
        }
    }
}
