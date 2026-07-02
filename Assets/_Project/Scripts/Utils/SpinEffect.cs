using UnityEngine;

namespace VertigoDemo.Utils
{
    public class SpinEffect : MonoBehaviour
    {
        [SerializeField] private float degreesPerSecond = 60f;
        [SerializeField] private bool randomizeStart = true;

        private void OnEnable()
        {
            if (randomizeStart)
                transform.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
        }

        private void Update()
        {
            transform.Rotate(0, 0, degreesPerSecond * Time.deltaTime);
        }
    }
}
