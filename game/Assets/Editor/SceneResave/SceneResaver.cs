#if UNITY_EDITOR
namespace _Game.EditorTools
{
    using System.IO;
    using System.Linq;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;

    /// <summary>
    /// Mở và lưu lại scene để Unity re-serialize PrefabInstance.
    ///
    /// Dùng khi scene đã "stale" so với prefab: Unity báo lỗi
    /// "Prefab mismatch: The instance object in the scene is referencing a
    /// corresponding source object in the Prefab of a different type" mỗi lần mở
    /// scene (thường do đổi kiểu field trong prefab mà không re-serialize scene).
    ///
    /// Menu: `Tools/Resave Scenes`
    /// </summary>
    public static class SceneResaver
    {
        private const string ScenesFolder = "Assets/Scenes";

        [MenuItem("Tools/Resave Scenes", priority = 30)]
        public static void ResaveAllScenes()
        {
            var paths = Directory.Exists(ScenesFolder)
                ? Directory.GetFiles(ScenesFolder, "*.unity", SearchOption.AllDirectories)
                    .Select(p => p.Replace('\\', '/'))
                    .OrderBy(p => p)
                    .ToArray()
                : System.Array.Empty<string>();

            if (paths.Length == 0)
            {
                Debug.LogWarning("[SceneResaver] Không tìm thấy scene nào trong " + ScenesFolder);
                return;
            }

            Resave(paths);
            if (Application.isBatchMode) EditorApplication.Exit(0);
        }

        /// <summary>Mở + lưu lại từng scene trong danh sách (dùng để chạy bằng -executeMethod).</summary>
        public static void Resave(string[] paths)
        {
            foreach (var path in paths)
            {
                if (!File.Exists(path)) continue;
                var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
                EditorSceneManager.SaveScene(scene, path);
                Debug.Log($"[SceneResaver] Đã lưu lại {path}");
            }
            AssetDatabase.Refresh();
        }
    }
}
#endif
