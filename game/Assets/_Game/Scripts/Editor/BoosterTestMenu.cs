// BoosterTestMenu.cs — menu test nhanh trong Editor (chỉ Play mode mới tác dụng).
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    public static class BoosterTestMenu
    {
        private const string Root = "Tools/Booster/";

        [MenuItem(Root + "Set All = 5 (có hàng)")]
        private static void Set5() => SetAll(5);

        [MenuItem(Root + "Set All = 0 (test popup mua)")]
        private static void Set0() => SetAll(0);

        [MenuItem(Root + "Preset: Add=2 Clean=0 Magic=2")]
        private static void Preset() => SetEach(2, 0, 2);

        [MenuItem(Root + "Popup/AddSlot")]
        private static void P1() => MoreBoosterPopupUI.Show(BoosterType.AddSlot);
        [MenuItem(Root + "Popup/Clean")]
        private static void P2() => MoreBoosterPopupUI.Show(BoosterType.Clean);
        [MenuItem(Root + "Popup/MagicBox")]
        private static void P3() => MoreBoosterPopupUI.Show(BoosterType.MagicBox);
        [MenuItem(Root + "Popup/Close")]
        private static void PClose() => MoreBoosterPopupUI.CloseCurrent();

        [MenuItem(Root + "Refresh BoosterBar")]
        private static void Refresh()
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Booster", "Bấm Play rồi hãy dùng menu này.", "OK");
                return;
            }
            var bar = Object.FindObjectOfType<BoosterBarUI>();
            if (bar != null) bar.Refresh();
        }

        private static void SetAll(int v) => SetEach(v, v, v);

        private static void SetEach(int a, int c, int m)
        {
            if (!Application.isPlaying)
            {
                EditorUtility.DisplayDialog("Booster", "Bấm Play rồi hãy set số lượng.", "OK");
                return;
            }
            GameData.Instance.SetAllCounts(a, c, m);
            var bar = Object.FindObjectOfType<BoosterBarUI>();
            if (bar != null) bar.Refresh();
            Debug.Log($"[BoosterTestMenu] counts = {a}/{c}/{m}");
        }
    }
}
#endif
