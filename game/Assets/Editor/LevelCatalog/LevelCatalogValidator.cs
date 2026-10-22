#if UNITY_EDITOR
namespace _Game.EditorTools.LevelCatalog
{
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using _Game.Scripts.LevelData;

    /// <summary>
    /// Kiểm tra tính toàn vẹn của LevelCatalog trước khi build.
    ///
    /// Menu: `Tools/Level/Validate Level Catalog`
    /// Build: chặn build nếu catalog có lỗi mức Error.
    /// </summary>
    public static class LevelCatalogValidator
    {
        private const string CatalogAssetPath = "Assets/Resources/LevelCatalog.asset";

        [MenuItem("Tools/Level/Validate Level Catalog", priority = 21)]
        public static void ValidateMenu()
        {
            var catalog = Load();
            if (catalog == null)
            {
                EditorUtility.DisplayDialog("Level Catalog",
                    $"Không có {CatalogAssetPath}.\nHãy chạy `Tools/Level/Build Level Catalog` trước.",
                    "OK");
                return;
            }

            var issues = catalog.Validate();
            int errors = issues.Count(i => i.severity == LevelCatalogSeverity.Error);
            int warnings = issues.Count(i => i.severity == LevelCatalogSeverity.Warning);

            foreach (var issue in issues)
            {
                if (issue.severity == LevelCatalogSeverity.Error) Debug.LogError($"[LevelCatalog] {issue}");
                else Debug.LogWarning($"[LevelCatalog] {issue}");
            }

            if (errors == 0)
            {
                Debug.Log($"[LevelCatalog] OK — {catalog.TotalLevels} màn, " +
                          $"{catalog.PlayableCount} chơi được, {warnings} cảnh báo.");
                EditorUtility.DisplayDialog("Level Catalog",
                    $"OK — {catalog.TotalLevels} màn ({catalog.PlayableCount} chơi được), {warnings} cảnh báo.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog("Level Catalog",
                    $"{errors} lỗi, {warnings} cảnh báo.\nXem Console để biết chi tiết.",
                    "OK");
            }
        }

        public static LevelCatalog Load()
        {
            return AssetDatabase.LoadAssetAtPath<LevelCatalog>(CatalogAssetPath);
        }

        /// <summary>Chặn build khi catalog sai.</summary>
        public class BuildGate : IPreprocessBuildWithReport
        {
            public int callbackOrder => 0;

            public void OnPreprocessBuild(BuildReport report)
            {
                var catalog = Load();
                if (catalog == null)
                {
                    Debug.LogError("[LevelCatalog] Không có LevelCatalog.asset — build bị chặn. " +
                                   "Chạy `Tools/Level/Build Level Catalog`.");
                    throw new System.Exception("LevelCatalog missing");
                }

                var errors = catalog.Validate()
                    .Where(i => i.severity == LevelCatalogSeverity.Error)
                    .ToList();

                if (errors.Count > 0)
                {
                    foreach (var e in errors) Debug.LogError($"[LevelCatalog] {e}");
                    throw new System.Exception(
                        $"LevelCatalog có {errors.Count} lỗi — build bị chặn. " +
                        "Chạy `Tools/Level/Build Level Catalog` rồi validate lại.");
                }
            }
        }
    }
}
#endif
