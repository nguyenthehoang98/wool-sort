using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Gameplay;
using UnityEngine;

namespace _Game.Scripts.Utils
{
    public static class LogUtils
    {
        /// <summary>
        /// Log các priority list chỉ khi list không rỗng.
        /// </summary>
        public static void LogNonEmptyPriorityLists<T>(Dictionary<YarnColor, List<T>> priLists)
        {
            Log("---- DEBUG priLists (chưa xử lý skip) ----", Color.green);

            foreach (var kv in priLists)
            {
                var color = kv.Key;
                var list  = kv.Value;

                if (list == null || list.Count == 0)
                    continue;

                // Chuyển từng phần tử của list thành chuỗi
                var priStrings = list.Select(p => p.ToString()).ToArray();
                Debug.Log($"Color={color} → Priorities=[{string.Join(", ", priStrings)}]");
            }

           Log("------------------------", Color.green);
        }
        
        /// <summary>
        /// Log toàn bộ contents của piecesByColor sau khi BuildAllPartData(), in ra số lượng và danh sách priority.
        /// </summary>
        public static void LogPiecesByColor(Dictionary<YarnColor, List<YarnPartInfo>> piecesByColor)
        {
            Log("---- DEBUG: Load Level | piecesByColor after BuildAllPartData() ----", Color.magenta);

            foreach (var kv in piecesByColor)
            {
                var color = kv.Key;
                var list  = kv.Value;

                if (list == null || list.Count == 0)
                    continue;

                var priorities = list
                    .Select(info => info.Priority)
                    .OrderBy(x => x)
                    .ToArray();

                Debug.Log($"Color={color} | Count={list.Count} | Priorities=[{string.Join(", ", priorities)}]");
            }

           Log("-------------------------------------------------------------", Color.magenta);
        }
        
        /// <summary>
        /// Log dictionary dưới dạng một chuỗi "key1:val1, key2:val2, …".
        /// </summary>
        public static void LogDictionaryInline<TKey, TValue>(Dictionary<TKey, TValue> dict, Color color)
        {
            if (dict == null || dict.Count == 0)
            {
                Log("<Empty Dictionary>", color);
                return;
            }

            string body = string.Join(", ",
                dict.Select(kv => $"{kv.Key}:{kv.Value}")
            );
            Log($"[Dictionary Color Sum] → {{ {body} }}", color);
        }
        
        /// <summary>
        /// Chuyển UnityEngine.Color thành mã HEX (RRGGBB).
        /// </summary>
        private static string ToHex(Color color)
        {
            return ColorUtility.ToHtmlStringRGB(color);
        }

        /// <summary>
        /// Log bình thường với màu tuỳ chọn.
        /// </summary>
        public static void Log(string message, Color color)
        {
            string hex = ToHex(color);
            Debug.Log($"<color=#{hex}>{message}</color>");
        }

        /// <summary>
        /// Log warning với màu tuỳ chọn.
        /// </summary>
        public static void LogWarning(string message, Color color)
        {
            string hex = ToHex(color);
            Debug.LogWarning($"<color=#{hex}>{message}</color>");
        }

        /// <summary>
        /// Log error với màu tuỳ chọn.
        /// </summary>
        public static void LogError(string message, Color color)
        {
            string hex = ToHex(color);
            Debug.LogError($"<color=#{hex}>{message}</color>");
        }

        /// <summary>
        /// Log có tag (ví dụ "AI", "NETWORK") để dễ filter, kèm màu tuỳ chọn.
        /// </summary>
        public static void LogTag(string tag, string message, Color color)
        {
            string hex = ToHex(color);
            Debug.Log($"<color=#{hex}>[{tag}] {message}</color>");
        }
        
        
        /// <summary>
        /// In từng phần tử của list, mỗi phần tử 1 dòng.
        /// </summary>
        public static void LogList<T>(IEnumerable<T> list, Color color)
        {
            if (list == null)
            {
                Log("<List is null>", color);
                return;
            }

            var items = list.ToList();
            if (items.Count == 0)
            {
                Log("<Empty List>", color);
                return;
            }

            for (int i = 0; i < items.Count; i++)
            {
                Log($"[{i}] {items[i]}", color);
            }
        }

        /// <summary>
        /// In toàn bộ list trên 1 dòng: [item0, item1, …].
        /// </summary>
        public static void LogListInline<T>(IEnumerable<T> list, Color color)
        {
            if (list == null)
            {
                Log("<List is null>", color);
                return;
            }

            var items = list.ToList();
            if (items.Count == 0)
            {
                Log("<Empty List>", color);
                return;
            }

            string body = string.Join(", ", items);
            Log($"List → [ {body} ]", color);
        }

    }
}