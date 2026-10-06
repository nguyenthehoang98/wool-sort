using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _Game.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Bộ tool cho shader dissolve riêng (WoolSort/YarnDissolve):
/// 1. Migrate ThreadMat_* sang shader mới (giữ nguyên tên property nên
///    màu/texture cũ được giữ lại; property lạ của Amplify sẽ bị rớt —
///    revert được bằng git).
/// 2. Quét trục wipe từng mesh trong level (trục cao nhất + span),
///    báo model suy biến (phẳng/rỗng).
/// </summary>
public static class YarnDissolveTools
{
    private const string ShaderPath = "Assets/_Game/Shader/YarnDissolve.shader";
    private const string MaterialsFolder = "Assets/_Game/Materials";
    private const string LevelsFolder = "Assets/Resources/Levels";

    [MenuItem("Tools/Puzzle/Chuyển ThreadMat sang YarnDissolve", false, 14)]
    private static void MigrateThreadMats()
    {
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(ShaderPath);
        if (shader == null)
        {
            EditorUtility.DisplayDialog("Chuyển shader", "Không thấy shader: " + ShaderPath, "OK");
            return;
        }
            if (!EditorUtility.DisplayDialog("Chuyển ThreadMat?",
            "Chuyển mọi ThreadMat_*.mat sang shader WoolSort/YarnDissolve.\n" +
            "Màu/texture (tên property giống nhau) được giữ lại.\n" +
            "Muốn về cũ thì git checkout lại thư mục Materials.",
            "Migrate luôn", "Hủy"))
            return;

        var guids = AssetDatabase.FindAssets("t:Material", new[] { MaterialsFolder });
        int ok = 0;
        var sb = new StringBuilder();
        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string name = Path.GetFileNameWithoutExtension(path);
            if (!name.StartsWith("ThreadMat_")) continue;
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (mat == null) continue;
            if (mat.shader != shader)
            {
                mat.shader = shader;
                EditorUtility.SetDirty(mat);
                sb.AppendLine("[ĐỔI] " + name);
                ok++;
            }
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        string report = $"Đã migrate {ok} ThreadMat sang YarnDissolve.\n" + sb;
        Debug.Log(report);
        EditorUtility.DisplayDialog("Chuyển shader", $"Đã chuyển {ok} material. Xem Console để chi tiết.", "OK");
    }

    [MenuItem("Tools/Puzzle/Quét trục mesh len", false, 15)]
    private static void ScanMeshAxes()
    {
        var levelPaths = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p)
            .ToList();

        var sb = new StringBuilder();
        sb.AppendLine("=== TRỤC WIPE TỪNG MESH (cao nhất = X/Y/Z, span object-space) ===");
        int degenerate = 0;

        foreach (var path in levelPaths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            var parts = go.GetComponentsInChildren<YarnPart>(true);
            var seen = new HashSet<string>();
            foreach (var p in parts)
            {
                var mf = p.GetComponent<MeshFilter>();
                var mesh = mf != null ? mf.sharedMesh : null;
                if (mesh == null || !seen.Add(mesh.name)) continue;
                Vector3 size = mesh.bounds.size;
                string axis = size.y >= size.x && size.y >= size.z ? "Y"
                    : size.x >= size.z ? "X" : "Z";
                float span = axis == "Y" ? size.y : axis == "X" ? size.x : size.z;
                bool bad = span < 0.0001f;
                if (bad) degenerate++;
                sb.AppendLine($"{Path.GetFileNameWithoutExtension(path)}/{mesh.name}: trục {axis} span={span:F4} size=({size.x:F3},{size.y:F3},{size.z:F3})"
                    + (bad ? " [SUY BIẾN: mesh rỗng/phẳng]" : ""));
            }
        }

        sb.AppendLine(degenerate == 0
            ? "OK: không có mesh suy biến."
            : $"CẢNH BÁO: {degenerate} mesh suy biến (rỗng/phẳng) — wipe trên đó sẽ pop, cần artist sửa mesh.");
        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Quét trục mesh",
            degenerate == 0 ? "Mọi mesh đều có trục wipe hợp lệ. Xem Console để chi tiết."
            : $"{degenerate} mesh suy biến! Xem Console để chi tiết.", "OK");
    }
}
