namespace _Game.Scripts.Meta
{
    using UnityEngine;

    /// <summary>
    /// Nút bánh răng mở popup cài đặt. Gắn vào object có Image (icon bánh răng) + Button.
    /// Popup được tìm trong scene (kể cả khi đang tắt) nên không cần kéo tham chiếu.
    /// </summary>
    public class SettingsButton : MonoBehaviour
    {
        private void Awake()
        {
            var btn = GetComponent<UnityEngine.UI.Button>();
            if (btn != null)
            {
                // không RemoveAllListeners để giữ listener gắn thêm trong Inspector
                btn.onClick.AddListener(OpenPopup);
            }
        }

        public static void OpenPopup()
        {
            SettingsPopup.Open();
        }
    }
}
