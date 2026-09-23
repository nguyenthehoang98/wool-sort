using UnityEngine;

namespace _Game.Scripts
{
    public class CameraZAdjuster : MonoBehaviour
    {
        [Tooltip("Khoảng cách âm Z mặc định cho aspect ratio chuẩn (ví dụ: -10)")]
        public float defaultZ = -10f;

        [Tooltip("Aspect ratio chuẩn, ví dụ 9:16")]
        public float defaultAspect = 9f / 16f;

        private void Start()
        {
            float currentAspect = (float)Screen.width / Screen.height;

            // Tính tỷ lệ điều chỉnh khoảng cách
            float aspectRatioFactor = defaultAspect / currentAspect;

            // Không bao giờ tiến gần hơn (chỉ lùi xa thêm nếu cần)
            float distanceFactor = Mathf.Max(1f, aspectRatioFactor);

            // Lùi xa hơn về Z âm (giữ đúng hướng nhìn)
            float newZ = defaultZ * distanceFactor;

            // Chỉ đổi Z
            Vector3 pos = transform.position;
            transform.position = new Vector3(pos.x, pos.y, newZ);
        }
    }
}