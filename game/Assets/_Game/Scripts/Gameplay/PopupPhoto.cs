using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    // Chụp ảnh gameplay (camera ortho) làm ảnh tròn trong Win/Lose popup.
    public static class PopupPhoto
    {
        private const int PHOTO_SIZE = 512;

        public static Sprite Capture(Camera cam)
        {
            if (cam == null) cam = Camera.main;
            if (cam == null || Screen.width <= 0 || Screen.height <= 0) return null;

                        // Capture directly to a square render texture to avoid extra cropping.
            var rtSquare = RenderTexture.GetTemporary(PHOTO_SIZE, PHOTO_SIZE, 24);
            var prevTex = cam.targetTexture;
            var prevActive = RenderTexture.active;
            Texture2D square = null;
            try
            {
                cam.targetTexture = rtSquare;
                cam.Render();
                cam.targetTexture = prevTex;
                RenderTexture.active = rtSquare;
                square = new Texture2D(PHOTO_SIZE, PHOTO_SIZE, TextureFormat.RGB24, false);
                square.ReadPixels(new Rect(0, 0, PHOTO_SIZE, PHOTO_SIZE), 0, 0);
                square.Apply();
                ApplyCircularMask(square);
            }
            finally
            {
                RenderTexture.active = prevActive;
                RenderTexture.ReleaseTemporary(rtSquare);
            }
            if (square == null) return null;
            return Sprite.Create(square, new Rect(0, 0, PHOTO_SIZE, PHOTO_SIZE), new Vector2(0.5f, 0.5f));
        }

        // Cắt góc trong suốt để ảnh vuông vừa khung tròn.
        private static void ApplyCircularMask(Texture2D tex)
        {
            int s = tex.width;
            float r = s * 0.5f;
            var pixels = tex.GetPixels32();
            for (int y = 0; y < s; y++)
            {
                float dy = y + 0.5f - r;
                for (int x = 0; x < s; x++)
                {
                    float dx = x + 0.5f - r;
                    if (dx * dx + dy * dy > r * r)
                        pixels[y * s + x].a = 0;
                }
            }
            tex.SetPixels32(pixels);
            tex.Apply();
        }
    }
}
