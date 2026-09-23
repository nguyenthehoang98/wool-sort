using System.Diagnostics;
using System.Globalization;
using System.IO;
using UnityEngine;

namespace _KIT
{
    /// <summary>
    /// Chụp ảnh màn hình (Camera orthographic)
    /// </summary>
    public class KitScreenshot : MonoBehaviour
    {
        [SerializeField] private Vector2 size;
        [SerializeField] private Vector2 position;
        [SerializeField] private Texture2D screenShot;

        private Camera mainCamera;
        private float ratio;

        private void Start()
        {
            mainCamera = Camera.main;
            ratio = Screen.width / (float)Screen.height;
        }

#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Texture2D screenShot = Capture();
                
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenshotName("ANDROID+", size.x.ToString(CultureInfo.InvariantCulture), size.y.ToString(CultureInfo.InvariantCulture));

                if (!Directory.Exists(Application.persistentDataPath + "/../screenshots/"))
                    Directory.CreateDirectory(Application.persistentDataPath + "/../screenshots/");

                File.WriteAllBytes(filename, bytes);
                //Debug.Log(string.Format("Took screenshot to: {0}", filename));

                //if (OpenFileDirecoty)
                {
                    Process.Start(Application.persistentDataPath + "/../screenshots/");
                }
            }
        }
#endif

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(position, size);
        }
#endif
        
        Vector2 ConvertWorldToPixelSize(Vector2 worldSize)
        {
            // Tạo hai điểm trong thế giới tương ứng với kích thước của hình chữ nhật
            Vector3 worldTopLeft = new Vector3(0, 0, 0);
            Vector3 worldBottomRight = new Vector3(worldSize.x, -worldSize.y, 0);

            // Chuyển đổi các điểm này sang tọa độ màn hình
            Vector3 screenTopLeft = mainCamera.WorldToScreenPoint(worldTopLeft);
            Vector3 screenBottomRight = mainCamera.WorldToScreenPoint(worldBottomRight);

            // Tính toán kích thước trong pixel
            Vector2 pixelSize = new Vector2(screenBottomRight.x - screenTopLeft.x, screenTopLeft.y - screenBottomRight.y);
            return pixelSize;
        }
        
        public Texture2D Capture()
        {
            Vector2 pixelSize = ConvertWorldToPixelSize(size);
            int width = (int)pixelSize.x;
            int height = (int)pixelSize.y;
            TextureFormat textForm = TextureFormat.RGB24;
            RenderTexture rt = new RenderTexture(width, height, 24);
            mainCamera.targetTexture = rt;
            screenShot = new Texture2D(width, height, textForm, false);
            mainCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenShot.Apply();
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            return screenShot;
        }

        public Texture2D Capture(Rect rect)
        {
            int width = (int)rect.width;
            int height = (int)rect.height;
            TextureFormat textForm = TextureFormat.RGB24;
            RenderTexture rt = new RenderTexture(width, height, 24);
            mainCamera.targetTexture = rt;
            screenShot = new Texture2D(width, height, textForm, false);
            mainCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(rect.x, rect.y, width, height), 0, 0);
            screenShot.Apply();
            Camera.main.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            return screenShot;
        }
        
        private string ScreenshotName(string platform, string width, string height)
        {
            return string.Format("{0}/../screenshots/" + "_" + platform + "screen_{1}x{2}_{3}.png", Application.persistentDataPath, width, height, System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
        }
    }
}