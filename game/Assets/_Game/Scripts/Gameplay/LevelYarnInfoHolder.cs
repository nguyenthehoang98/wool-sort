using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    // 1) Định nghĩa struct để lưu từng cặp Color + Count
    [System.Serializable]
    public struct ColorCountEntry
    {
        public YarnColor color;
        public int count;
    }

    [System.Serializable]
    public struct LayerCountEntry
    {
        public int layerIndex;
        public int count;
    }

    public class LevelYarnInfoHolder : MonoBehaviour
    {
        [Header("Box Spawn Thresholds (% remaining pieces)")]
        [Tooltip("Phải có đúng 4 mốc, số nguyên và tổng 4 giá trị = 100%")]
        public int[] boxSpawnThresholds = new int[4] { 20, 40, 15, 25 };

        public int totalYarnPartCount;
        public int totalYarnDecorations;
        public int totalYarnSpecialDecor;
        public List<YarnPart> allBaseYarnParts; // các yarn part sẵn có (layer 1)

        [Header("Các màu Random cho inner layer (Color + Count)")]
        [Tooltip("Mỗi entry gồm một màu và số lượng inner-layer cần random dùng màu đó.")]
        public List<ColorCountEntry> randomColorEntries = new List<ColorCountEntry>();

        // --- hai list debug sẽ được tự động cập nhật ---
        public List<LayerCountEntry> layerCountEntries = new List<LayerCountEntry>();
        public List<ColorCountEntry> colorCountEntries = new List<ColorCountEntry>();

        public int GetTotalYarnPartCount(List<YarnPart> outerParts)
        {
            int count = 0;
            foreach (var part in outerParts)
                count += 1 + part.totalInnerCount; // 1 là bản thân part ngoài cùng
            return count;
        }

        /// <summary>
        /// Cập nhật hai list layerCountEntries và colorCountEntries.
        /// Gọi sau khi thiết lập level hoặc bất kỳ khi nào randomColorEntries/allBaseYarnParts thay đổi.
        /// </summary>
        public void UpdateDebugInfo()
        {
            layerCountEntries.Clear();
            colorCountEntries.Clear();

            if (allBaseYarnParts == null || allBaseYarnParts.Count == 0)
                return;

            // 1) Tính theo layer dựa trên totalInnerCount
            int baseCount = allBaseYarnParts.Count;
            layerCountEntries.Add(new LayerCountEntry { layerIndex = 1, count = baseCount });

            int maxInner = allBaseYarnParts.Max(p => p.totalInnerCount);
            for (int i = 1; i <= maxInner; i++)
            {
                int cnt = allBaseYarnParts.Count(p => p.totalInnerCount >= i);
                layerCountEntries.Add(new LayerCountEntry { layerIndex = i + 1, count = cnt });
            }

            // 2) Tính theo color
            var dict = new Dictionary<YarnColor, int>();

            // 2a) Outer
            foreach (var p in allBaseYarnParts)
            {
                dict.TryAdd(p.initColor, 0);
                dict[p.initColor]++;
            }

            // 2b) Fixed-inner
            foreach (var p in allBaseYarnParts)
            {
                if (p.totalInnerCount > 0)
                {
                    for (int j = 0; j < p.innerColorTypes.Count; j++)
                    {
                        if (p.innerColorTypes[j] == InnerColorType.Fixed)
                        {
                            var c = p.innerFixedColors[j];
                            dict.TryAdd(c, 0);
                            dict[c]++;
                        }
                    }
                }
            }

            // 2c) Random-inner (từ randomColorEntries)
            foreach (var e in randomColorEntries)
            {
                dict.TryAdd(e.color, 0);
                dict[e.color] += e.count;
            }

            // chuyển thành list
            foreach (var kv in dict)
                colorCountEntries.Add(new ColorCountEntry { color = kv.Key, count = kv.Value });
        }

        /// <summary>
        /// Unity sẽ gọi OnValidate() mỗi khi sửa trong Inspector.
        /// Do đó ta có thể cập nhật ngay debug info nếu có thay đổi.
        /// </summary>
        private void OnValidate()
        {
            UpdateDebugInfo();
        }
    }
}