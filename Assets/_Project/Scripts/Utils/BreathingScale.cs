using UnityEngine;

namespace VertigoDemo.Utils
{
    public class BreathingScale : MonoBehaviour
    {
        [SerializeField] private float scaleAmount = 0.15f;
        [SerializeField] private float speed = 2f;
        [SerializeField] private bool randomizeStart = true;

        private Vector3 _baseScale;
        private float _offset;

        private void OnEnable()
        {
            _baseScale = transform.localScale;
            _offset = randomizeStart ? Random.Range(0f, Mathf.PI * 2f) : 0f;
        }

        private void OnDisable()
        {
            transform.localScale = _baseScale;
        }

        private void Update()
        {
            float t = (Mathf.Sin(Time.time * speed + _offset) + 1f) * 0.5f;
            float s = 1f + t * scaleAmount;
            transform.localScale = _baseScale * s;
        }
    }
}
