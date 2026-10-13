// BoosterQuickTest.cs
// Setup data + panel test nhanh booster (Play là dùng được ngay, không cần kéo ref).
// - Áp số lượng ban đầu cho 3 loại khi Play (để test Noti/Plus/Lock).
// - Phím tắt: 1/2/3 dùng booster, Shift+1/2/3 mở popup mua, F1=set 5, F2=set 0, F3=toggle lock.
// - Panel OnGUI góc trái: nút bấm to cho từng loại + hiện count + mở popup.
// Đặt 1 object "BoosterQuickTest" trong scene Gameplay (đã thêm sẵn trong .unity).

using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    public sealed class BoosterQuickTest : MonoBehaviour
    {
        [Header("Số lượng lúc Play (đổi trong Inspector rồi bấm Play)")]
        [Min(0)] public int startAddSlot = 2;
        [Min(0)] public int startClean = 0;
        [Min(0)] public int startMagic = 2;

        [Header("Giả lập khóa (test state Lock)")]
        [Tooltip("Bật = ép level xuống 1 và đẩy unlock Clean lên 99 để thấy nhánh Lock. " +
            "Tắt = level 1 unlock hết (mặc định).")]
        public bool simulateLockedClean = false;

        [Header("Panel debug")]
        public bool showPanel = true;
        public bool enableHotkeys = true;
        [Range(0f, 1f)] public float panelScale = 1f;

        private void Awake()
        {
            ApplyStartData();
        }

        private void OnValidate()
        {
            if (Application.isPlaying) ApplyStartData();
        }

        public void ApplyStartData()
        {
            if (GameData.Instance == null) return;
            GameData.Instance.SetAllCounts(startAddSlot, startClean, startMagic, notify: false);
            if (simulateLockedClean)
            {
                GameData.Instance.SetLevel(1, notify: false);
                if (Configs.LIST_BOOSTER_UNLOCK.Count >= 3)
                    Configs.LIST_BOOSTER_UNLOCK[(int)BoosterType.Clean] = 99;
            }
            else
            {
                GameData.Instance.SetLevel(1, notify: false);
                // Trả unlock về mặc định {1,1,1} nếu đang bị test đẩy lên.
                if (Configs.LIST_BOOSTER_UNLOCK.Count >= 3 &&
                    Configs.LIST_BOOSTER_UNLOCK[(int)BoosterType.Clean] > 1)
                    Configs.LIST_BOOSTER_UNLOCK[(int)BoosterType.Clean] = 1;
            }
            GameConst.RaiseChanged();
            RefreshBar();
        }

        private void Update()
        {
            if (!enableHotkeys || !Application.isPlaying) return;
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (Input.GetKeyDown(KeyCode.Alpha1)) { if (shift) MoreBoosterPopupUI.Show(BoosterType.AddSlot); else Use(BoosterType.AddSlot); }
            if (Input.GetKeyDown(KeyCode.Alpha2)) { if (shift) MoreBoosterPopupUI.Show(BoosterType.Clean); else Use(BoosterType.Clean); }
            if (Input.GetKeyDown(KeyCode.Alpha3)) { if (shift) MoreBoosterPopupUI.Show(BoosterType.MagicBox); else Use(BoosterType.MagicBox); }
            if (Input.GetKeyDown(KeyCode.F1)) SetAll(5);
            if (Input.GetKeyDown(KeyCode.F2)) SetAll(0);
            if (Input.GetKeyDown(KeyCode.F3)) ToggleLock();
        }

        // OnGUI removed – UI now provided by real UI components.
        // Retained for potential debug when needed (enable via #if UNITY_EDITOR).
#if UNITY_EDITOR
        private void OnGUI()
        {
            // Debug panel placeholder – disabled in production.
        }
#endif

        // ---- API cho nút bấm / menu Tools ----
        public void SetAll(int v)
        {
            startAddSlot = startClean = startMagic = v;
            GameData.Instance.SetAllCounts(v, v, v);
            RefreshBar();
            Debug.Log($"[BoosterQuickTest] Set all = {v}");
        }

        public void ToggleLock()
        {
            simulateLockedClean = !simulateLockedClean;
            ApplyStartData();
            Debug.Log($"[BoosterQuickTest] simulateLockedClean={simulateLockedClean}");
        }

        public void Use(BoosterType type)
        {
            // Mô phỏng đúng flow click nút: trừ kho rồi gọi manager.
            if (BoosterManager.Instance == null)
            {
                Debug.LogWarning("[BoosterQuickTest] Thiếu BoosterManager trong scene.");
                return;
            }
            if (!BoosterManager.Instance.CanUseBooster(type))
            {
                Debug.Log($"[BoosterQuickTest] {type} bị từ chối bởi CanUseBooster.");
                return;
            }
            if (GameData.Instance.GetCount(type) <= 0)
            {
                MoreBoosterPopupUI.Show(type);
                return;
            }
            GameData.Instance.AddCount(type, -1, "use", "quick_test");
            BoosterManager.Instance.UseBooster(type);
        }

        public void RefreshBar()
        {
            var bar = FindObjectOfType<BoosterBarUI>();
            if (bar != null) bar.Refresh();
            else Debug.LogWarning("[BoosterQuickTest] Không thấy BoosterBarUI trong scene.");
        }

        [ContextMenu("Apply Start Data")]
        private void CtxApply() => ApplyStartData();
        [ContextMenu("Set All 5")]
        private void Ctx5() => SetAll(5);
        [ContextMenu("Set All 0 (test popup)")]
        private void Ctx0() => SetAll(0);
    }
}
