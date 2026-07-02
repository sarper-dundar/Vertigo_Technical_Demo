using UnityEngine;

namespace VertigoDemo.Utils
{
    public class VerticalSweep : MonoBehaviour
    {
        [SerializeField] private float sweepDuration = 0.6f;
        [SerializeField] private float pauseMin = 1.5f;
        [SerializeField] private float pauseMax = 4f;
        [SerializeField] private float startY = 200f;
        [SerializeField] private float endY = -200f;

        private RectTransform _rect;
        private float _timer;
        private float _currentPause;
        private bool _sweeping;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _currentPause = Random.Range(pauseMin, pauseMax);
            _timer = Random.Range(0f, _currentPause);
            _sweeping = false;
            SetY(startY);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (!_sweeping)
            {
                if (_timer >= _currentPause)
                {
                    _sweeping = true;
                    _timer = 0f;
                }
                return;
            }

            float t = _timer / sweepDuration;
            if (t >= 1f)
            {
                SetY(startY);
                _sweeping = false;
                _timer = 0f;
                _currentPause = Random.Range(pauseMin, pauseMax);
                return;
            }

            SetY(Mathf.Lerp(startY, endY, t));
        }

        private void SetY(float y)
        {
            var pos = _rect.anchoredPosition;
            pos.y = y;
            _rect.anchoredPosition = pos;
        }
    }
}
