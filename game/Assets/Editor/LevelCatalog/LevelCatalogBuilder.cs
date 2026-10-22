#if UNITY_EDITOR
namespace _Game.EditorTools.LevelCatalog
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;
    using _Game.Scripts.LevelData;

    /// <summary>
    /// Sinh `Assets/Resources/LevelCatalog.asset` từ dữ liệu gốc.
    ///
    /// Dữ liệu gốc lấy từ `tools/level_reference.json` (sinh bằng
    /// `python3 tools/extract_level_reference.py`) — tool KHÔNG phụ thuộc đường dẫn
    /// máy cụ thể, nên build lại catalog ở máy khác vẫn ra kết quả giống nhau.
    ///
    /// Prefab màn lấy từ `Assets/Resources/Levels/Level*.prefab` (đã có trong target).
    ///
    /// Menu: `Tools/Level/Build Level Catalog`
    /// </summary>
    public static class LevelCatalogBuilder
    {
        private const string CatalogAssetPath = "Assets/Resources/LevelCatalog.asset";
        private const string LevelsFolder = "Assets/Resources/Levels";

        /// <summary>Cách xếp thứ tự chơi khi sinh catalog.</summary>
        public enum OrderPolicy
        {
            /// <summary>
            /// Thứ tự tuần tự theo id prefab (Level1, Level2, ...) — khớp với hành vi
            /// runtime hiện tại của target. Mặc định.
            /// </summary>
            Sequential = 0,

            /// <summary>
            /// Theo đúng danh sách `levelOrder` của dự án gốc (88 mức xáo trộn).
            /// Chỉ giữ những mức có prefab thật trong target.
            /// </summary>
            ReferenceOrder = 1,
        }

        [Serializable]
        private class RefLevel
        {
            public int id;
            public int modeType;
        }

        [Serializable]
        private class RefOrder
        {
            public int index;
            public int levelId;
        }

        [Serializable]
        private class RefData
        {
            public string source;
            public List<RefLevel> levels = new List<RefLevel>();
            public List<RefOrder> order = new List<RefOrder>();
        }

        [MenuItem("Tools/Level/Build Level Catalog", priority = 20)]
        public static void BuildAndSave()
        {
            var catalog = Build(OrderPolicy.Sequential);
            if (catalog == null) return;
            Save(catalog);
            // Chạy bằng `-executeMethod` thì Unity đứng yên chờ → phải tự thoát.
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        /// <summary>Sinh catalog (chưa ghi asset) theo policy.</summary>
        public static LevelCatalog Build(OrderPolicy policy)
        {
            string jsonPath = FindReferenceJson();
            RefData data = new RefData();
            if (jsonPath != null)
            {
                try
                {
                    data = JsonUtility.FromJson<RefData>(File.ReadAllText(jsonPath));
                }
                catch (Exception e)
                {
                    Debug.LogError($"[LevelCatalog] Không parse được {jsonPath}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[LevelCatalog] Không tìm thấy tools/level_reference.json — " +
                                 "chỉ dùng prefab trong Resources/Levels, độ khó để mặc định Normal.");
            }

            var modeById = new Dictionary<int, LevelModeType>();
            if (data?.levels != null)
                foreach (var l in data.levels)
                    modeById[l.id] = (LevelModeType)l.modeType;

            var refOrderByLevelId = new Dictionary<int, int>();
            if (data?.order != null)
                foreach (var o in data.order)
                    if (!refOrderByLevelId.ContainsKey(o.levelId)) refOrderByLevelId[o.levelId] = o.index;

            var prefabByLevelId = FindLevelPrefabs();

            var defs = new List<LevelDefinition>();
            if (policy == OrderPolicy.ReferenceOrder && data?.order != null && data.order.Count > 0)
            {
                int order = 0;
                foreach (var o in data.order)
                {
                    prefabByLevelId.TryGetValue(o.levelId, out var prefab);
                    if (prefab == null) continue; // chưa có asset -> bỏ khỏi danh sách chơi
                    defs.Add(MakeDef(++order, o.levelId, modeById, refOrderByLevelId, prefab));
                }
            }
            else
            {
                int order = 0;
                foreach (var kv in prefabByLevelId.OrderBy(k => k.Key))
                    defs.Add(MakeDef(++order, kv.Key, modeById, refOrderByLevelId, kv.Value));
            }

            var catalog = ScriptableObject.CreateInstance<LevelCatalog>();
            catalog.name = "LevelCatalog";
            catalog.SetLevels(
                defs,
                jsonPath != null ? $"tools/level_reference.json ({data?.source})" : "prefabs-only",
                DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture));
            return catalog;
        }

        /// <summary>Ghi catalog ra asset trong Resources (để runtime load được).</summary>
        public static void Save(LevelCatalog catalog)
        {
            var existing = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogAssetPath);
            if (existing == null)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(CatalogAssetPath));
                AssetDatabase.CreateAsset(catalog, CatalogAssetPath);
                catalog = AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogAssetPath);
            }
            else
            {
                existing.SetLevels(
                    catalog.Levels.Select(l => l.Clone()).ToList(),
                    catalog.GeneratedFrom,
                    catalog.GeneratedAtUtc);
                EditorUtility.SetDirty(existing);
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var issues = catalog.Validate();
            int errors = issues.Count(i => i.severity == LevelCatalogSeverity.Error);
            Debug.Log($"[LevelCatalog] Đã lưu {CatalogAssetPath}: " +
                      $"{catalog.TotalLevels} màn ({catalog.PlayableCount} chơi được), {errors} lỗi.");
            foreach (var issue in issues)
                Debug.LogWarning($"[LevelCatalog] {issue}");
        }

        private static LevelDefinition MakeDef(int orderIndex, int levelId,
                                              Dictionary<int, LevelModeType> modeById,
                                              Dictionary<int, int> refOrderByLevelId,
                                              GameObject prefab)
        {
            modeById.TryGetValue(levelId, out var mode);
            // -1 = màn này không nằm trong danh sách order của dự án gốc
            // (index trong GeneralConfig.levelOrder chạy từ 1 nên 0 là giá trị hợp lệ).
            int refOrder = refOrderByLevelId.TryGetValue(levelId, out var ro) ? ro : -1;
            return new LevelDefinition
            {
                orderIndex = orderIndex,
                levelId = levelId,
                referenceOrderIndex = refOrder,
                modeType = mode,
                prefab = prefab,
                seed = LevelDefinition.DefaultSeed(levelId),
            };
        }

        /// <summary>Tìm prefab màn trong Resources/Levels, key = số trong tên `Level{n}`.</summary>
        private static Dictionary<int, GameObject> FindLevelPrefabs()
        {
            var map = new Dictionary<int, GameObject>();
            if (!Directory.Exists(LevelsFolder)) return map;
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder }))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab == null) continue;
                var name = Path.GetFileNameWithoutExtension(path);
                if (!name.StartsWith("Level")) continue;
                if (!int.TryParse(name.Substring("Level".Length), out int id)) continue;
                map[id] = prefab;
            }
            return map;
        }

        /// <summary>Tìm `tools/level_reference.json` (repo root, hoặc trong project root).</summary>
        public static string FindReferenceJson()
        {
            string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
            string[] candidates =
            {
                Path.GetFullPath(Path.Combine(projectRoot, "..", "tools", "level_reference.json")),
                Path.Combine(projectRoot, "tools", "level_reference.json"),
            };
            foreach (var c in candidates)
                if (File.Exists(c)) return c;
            return null;
        }
    }
}
#endif
