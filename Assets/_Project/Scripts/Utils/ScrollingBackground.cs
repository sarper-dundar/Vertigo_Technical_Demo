using UnityEngine;
using UnityEngine.UI;

namespace VertigoDemo.Utils
{
    [RequireComponent(typeof(RawImage))]
    public class ScrollingBackground : MonoBehaviour
    {
        [SerializeField] private Vector2 scrollSpeed = new Vector2(0f, -1f);

        private RawImage _rawImage;
        private Vector2 _offset;

        private void Awake()
        {
            _rawImage = GetComponent<RawImage>();
        }

        private void Update()
        {
            _offset += scrollSpeed * Time.deltaTime;
            _offset.x %= 1f;
            _offset.y %= 1f;
            _rawImage.uvRect = new Rect(_offset, _rawImage.uvRect.size);
        }
    }
}
