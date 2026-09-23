using UnityEngine;

namespace _Game.Scripts
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class BackgroundScaler : MonoBehaviour
    {
        public Camera backgroundCamera;
        private bool scaled = false;

        void Start()
        {
            // Đảm bảo chỉ scale khi Play
            if (Application.isPlaying)
            {
                ScaleToFit();
            }
        }

        void ScaleToFit()
        {
            if (backgroundCamera == null || !backgroundCamera.orthographic) return;

            SpriteRenderer sr = GetComponent<SpriteRenderer>();
            if (sr.sprite == null) return;

            // Reset scale
            transform.localScale = Vector3.one;

            // Kích thước gốc sprite
            float spriteWidth = sr.sprite.rect.width / sr.sprite.pixelsPerUnit;
            float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;

            // Kích thước màn hình (world units)
            float worldHeight = backgroundCamera.orthographicSize * 2f;
            float worldWidth = worldHeight * Screen.width / (float)Screen.height;

            float scaleX = worldWidth / spriteWidth;
            float scaleY = worldHeight / spriteHeight;

            float finalScale = Mathf.Max(scaleX, scaleY);
            transform.localScale = new Vector3(finalScale, finalScale, 1f);

#if UNITY_EDITOR
            Debug.Log($"[BackgroundScaler] SpriteSize: ({spriteWidth:0.00}, {spriteHeight:0.00}), ScreenWorld: ({worldWidth:0.00}, {worldHeight:0.00}), FinalScale: {finalScale:0.00}");
#endif
        }
    }
}