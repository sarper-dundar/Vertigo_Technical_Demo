using UnityEngine;

namespace VertigoDemo.Utils
{
    public class HorizontalSweep : MonoBehaviour
    {
        [SerializeField] private float sweepDuration = 0.6f;
        [SerializeField] private float pauseDuration = 2f;
        [SerializeField] private float startX = -200f;
        [SerializeField] private float endX = 200f;

        private RectTransform _rect;
        private float _timer;
        private bool _sweeping;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            _timer = 0f;
            _sweeping = false;
            SetX(startX);
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            if (!_sweeping)
            {
                if (_timer >= pauseDuration)
                {
                    _sweeping = true;
                    _timer = 0f;
                }
                return;
            }

            float t = _timer / sweepDuration;
            if (t >= 1f)
            {
                SetX(startX);
                _sweeping = false;
                _timer = 0f;
                return;
            }

            SetX(Mathf.Lerp(startX, endX, t));
        }

        private void SetX(float x)
        {
            var pos = _rect.anchoredPosition;
            pos.x = x;
            _rect.anchoredPosition = pos;
        }
    }
}
