namespace _Game.Scripts.Meta
{
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Nút mở popup Daily Reward. Gán ref popup trong Inspector; nếu chưa có thì tự tìm trong
    /// scene, cuối cùng load từ Resources ("DailyRewardPopup") rồi instantiate.</summary>
    [RequireComponent(typeof(Button))]
    public class DailyRewardButton : MonoBehaviour
    {
        public DailyRewardPopup popup;

        private void Awake()
        {
            var btn = GetComponent<Button>();
            if (btn != null)
            {
                btn.onClick.RemoveListener(Open);
                btn.onClick.AddListener(Open);
            }
        }

        public void Open()
        {
            if (popup == null) popup = FindFirstObjectByType<DailyRewardPopup>(FindObjectsInactive.Include);

            if (popup == null)
            {
                var prefab = Resources.Load<GameObject>("DailyRewardPopup");
                if (prefab != null)
                {
                    var canvas = GetComponentInParent<Canvas>();
                    var parent = canvas != null ? canvas.transform : transform.root;
                    var go = Instantiate(prefab, parent, false);
                    go.name = "DailyRewardPopup";
                    popup = go.GetComponent<DailyRewardPopup>();
                }
                else
                {
                    Debug.LogWarning("[DailyRewardButton] Không có DailyRewardPopup trong scene lẫn Resources.");
                }
            }

            if (popup != null) popup.Show();
        }
    }
}
