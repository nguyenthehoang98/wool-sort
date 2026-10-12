#if UNITY_EDITOR
// BoosterDebugPanel.cs — harness test thủ công tạm thời (chỉ tồn tại trong Editor).
//
// Cách dùng:
//   1. Mở scene Gameplay, tạo GameObject "BoosterDebug", gắn component này.
//   2. Bấm Play. Panel hiện góc trái màn hình Game.
//   3. Không cần UI/shop/xây dựng thêm: gọi thẳng BoosterManager/BoxManager.
//   4. Xóa GameObject (hoặc cả file) trước khi build: file không được biên dịch vào player.
//
// Khác unit test ở chỗ: panel chạy đường chạy thật (coroutine, animation, prefab),
// hợp để kiểm tra cảm quan Clean/MagicBox; còn kiểm tra hồi quy thì dùng PlayMode tests.

using _Game.Scripts.Gameplay;
using UnityEngine;

namespace _Game.Scripts.Utils
{
    public sealed class BoosterDebugPanel : MonoBehaviour
    {
        [Tooltip("Tắt để giấu panel mà không cần gỡ component.")]
        public bool showPanel = true;

        private Rect windowRect = new Rect(10, 10, 360, 380);
        private string lastResult = "Chưa chạy thao tác nào.";

        private void OnGUI()
        {
            if (!showPanel) return;
            // Luôn vẽ để tự chẩn đoán: nếu manager thiếu thì hiện hướng dẫn thay vì biến mất.
            windowRect = GUILayout.Window(GetInstanceID(), windowRect, DrawWindow, "[DEBUG] Booster Test");
        }

        private void DrawWindow(int id)
        {
            GUI.skin.label.fontSize = 20;
            GUI.skin.button.fontSize = 24;
            GUI.skin.box.fontSize = 18;

            var bm = BoosterManager.Instance;
            var gm = GameManager.Instance;
            var wam = WaitingAreaManager.Instance;

            if (bm == null || gm == null)
            {
                GUILayout.Label("Chờ manager...");
                GUILayout.Label($"GameManager: {(gm != null ? "OK" : "THIẾU")}");
                GUILayout.Label($"BoosterManager: {(bm != null ? "OK" : "THIẾU")}");
                GUILayout.Label("Yêu cầu: đang Play scene Gameplay,");
                GUILayout.Label("các manager đã Awake (xem Console");
                GUILayout.Label("có lỗi NRE nào không).");
                GUI.DragWindow();
                return;
            }

            GUI.skin.label.fontSize = 20;
            GUI.skin.button.fontSize = 24;
            GUI.skin.box.fontSize = 18;

            GUILayout.Label($"Level={gm.CurrentLevel}");
            if (wam != null)
            {
                var slots = wam.GetWaitingSlots();
                int filled = 0;
                foreach (var s in slots) if (s != null && s.IsFilled) filled++;
                GUILayout.Label($"Waiting: {filled}/{slots.Count}");
            }

            GUILayout.Space(6);
            foreach (BoosterType t in System.Enum.GetValues(typeof(BoosterType)))
            {
                if (GUILayout.Button($"Dùng {t}", GUILayout.Height(64))) Run(() =>
                {
                    if (!bm.CanUseBooster(t)) return $"Từ chối {t} (điều kiện không đạt).";
                    bm.UseBooster(t);
                    return $"Đã gọi UseBooster({t}).";
                });
            }

            GUILayout.Space(6);
            GUILayout.Label(lastResult, GUI.skin.box);
            GUI.DragWindow();
        }

        private void Run(System.Func<string> action)
        {
            try
            {
                lastResult = action();
            }
            catch (System.Exception e)
            {
                lastResult = "LỖI: " + e.GetType().Name + " — " + e.Message;
                Debug.LogException(e);
            }
        }
    }
}
#endif
