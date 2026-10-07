using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _Game.Scripts;
using _Game.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Extract ảnh level thành ma trận color ID (28x28 ô = 1 YarnColor).
/// Vì pixel ảnh gốc hay lệch màu len (nén/gamma/lấy mẫu), mỗi ô được duyệt
/// tìm màu GẦN NHẤT trong dataset (các màu level đang dùng, chuẩn màu lấy
/// từ ThreadMat._MainColor). Runtime JamImage đọc file này thay vì đọc ảnh.
/// Mở: Tools / Puzzle / Extract ảnh thành ma trận màu
/// File ra: Assets/Resources/LevelColorMatrix/{LevelName}.asset
/// </summary>
public sealed class LevelImageMatrixTool : EditorWindow
{
    private const string DatabasePath = "Assets/_Game/Configs/YarnColorDatabase.asset";
    private const string LevelsFolder = "Assets/Resources/Levels";
    private const string MatrixFolder = "Assets/Resources/LevelColorMatrix";
    private const int Grid = 28;

    private Vector2 scroll;
    private string report = "Kéo ảnh vào + chọn level rồi Extract, hoặc Extract tất cả levels.";

    private Texture2D sourceTexture;
    private int selectedLevelIndex;
    private List<string> levelNames = new List<string>();

    [MenuItem("Tools/Puzzle/5. Extract ảnh thành ma trận màu", false, 5)]
    private static void Open() => GetWindow<LevelImageMatrixTool>("Extract ma trận màu");

    private void OnEnable()
    {
        RefreshLevelNames();
    }

    private void RefreshLevelNames()
    {
        levelNames = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(n => n)
            .ToList();
        selectedLevelIndex = Mathf.Clamp(selectedLevelIndex, 0, Mathf.Max(0, levelNames.Count - 1));
    }

    private void OnGUI()
    {
        GUILayout.Label("EXTRACT ẢNH -> MATRIX COLOR ID", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Mỗi ảnh được extract thành lưới 28x28, mỗi ô = 1 YarnColor gần nhất\n" +
            "(pixel không khớp 100% dataset sẽ được duyệt tìm màu gần nhất).\n" +
            "Runtime JamImage ưu tiên đọc file này, không đọc trực tiếp ảnh nữa.",
            MessageType.Info);

        EditorGUILayout.LabelField("Extract 1 level (kéo ảnh + chọn level)", EditorStyles.boldLabel);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Ảnh nguồn", sourceTexture, typeof(Texture2D), false);
        if (GUILayout.Button("Tải lại danh sách level"))
            RefreshLevelNames();
        if (levelNames.Count > 0)
            selectedLevelIndex = EditorGUILayout.Popup("Level", selectedLevelIndex, levelNames.ToArray());
        else
            EditorGUILayout.HelpBox("Không tìm thấy prefab nào trong " + LevelsFolder, MessageType.Warning);

        if (GUILayout.Button("Extract level đã chọn", GUILayout.Height(32)))
        {
            if (sourceTexture == null || levelNames.Count == 0)
            {
                report = "[LỖI] Chưa kéo ảnh vào hoặc chưa có level nào.";
            }
            else
            {
                ExtractSingle(levelNames[selectedLevelIndex], sourceTexture);
            }
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Extract tất cả levels", GUILayout.Height(32)))
        {
                if (EditorUtility.DisplayDialog("Extract ảnh?",
                "Tạo/ghi đè ma trận color ID cho mọi level trong Assets/Resources/Levels?",
                "Extract luôn", "Hủy"))
                ExtractAll();
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void ExtractSingle(string levelName, Texture2D tex)
    {
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        if (db == null)
        {
            report = "[LỖI] Không thấy Database: " + DatabasePath;
            return;
        }
        if (!AssetDatabase.IsValidFolder(MatrixFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "LevelColorMatrix");
        try
        {
            report = ExtractWithTexture(levelName, tex, db);
        }
        catch (Exception ex)
        {
            report = "[LỖI] " + levelName + ": " + ex.Message;
            Debug.LogException(ex);
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log(report);
    }

    private void ExtractAll()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== EXTRACT LEVEL IMAGE TO MATRIX ===");

        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        if (db == null)
        {
            report = "[LỖI] Không thấy Database: " + DatabasePath;
            return;
        }
        if (!AssetDatabase.IsValidFolder(MatrixFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "LevelColorMatrix");

        var levelPaths = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p)
            .ToList();

        int ok = 0, fail = 0;
        foreach (var path in levelPaths)
        {
            string levelName = Path.GetFileNameWithoutExtension(path);
            try
            {
                sb.AppendLine(ExtractOne(levelName, db));
                ok++;
            }
            catch (Exception ex)
            {
                sb.AppendLine("[LỖI] " + levelName + ": " + ex.Message);
                Debug.LogException(ex);
                fail++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        sb.AppendLine($"Xong: {ok} OK, {fail} lỗi.");
        report = sb.ToString();
        Debug.Log(report);
    }

    private static string ExtractOne(string levelName, YarnColorDatabase db)
    {
        // Ảnh gốc (đúng thứ tự fallback của JamImage.Initialize)
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.jpg");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/Level_{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/LevelImages/{levelName}.png");
        if (tex == null) return "[LỖI] " + levelName + ": thiếu ảnh Resources/Images/" + levelName + ".png.";
        return ExtractWithTexture(levelName, tex, db);
    }

    private static string ExtractWithTexture(string levelName, Texture2D tex, YarnColorDatabase db)
    {
        var sb = new StringBuilder();

        // 1. Dataset = các màu level đang dùng (đúng nguồn runtime JamImage.Initialize dùng)
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Levels/{levelName}.prefab");
        if (prefab == null) return "[LỖI] " + levelName + ": không thấy prefab.";
        var holder = prefab.GetComponent<LevelYarnInfoHolder>();
        if (holder == null) holder = prefab.GetComponentInChildren<LevelYarnInfoHolder>(true);

        var available = new List<YarnColor>();
        var yarnCounts = new Dictionary<YarnColor, int>();
        if (holder != null && holder.colorCountEntries != null && holder.colorCountEntries.Count > 0)
        {
            foreach (var e in holder.colorCountEntries)
                if (e.count > 0 && !available.Contains(e.color))
                {
                    available.Add(e.color);
                    yarnCounts[e.color] = e.count;
                }
        }
        else
        {
            foreach (var p in prefab.GetComponentsInChildren<YarnPart>(true))
                if (!available.Contains(p.initColor))
                {
                    available.Add(p.initColor);
                    yarnCounts[p.initColor] = 0;
                }
        }
        if (available.Count == 0) return "[LỖI] " + levelName + ": level không có màu nào.";
        if (tex == null) return "[LỖI] " + levelName + ": chưa kéo ảnh vào.";
        if (tex == null) return "[LỖI] " + levelName + ": thiếu ảnh Resources/Images/" + levelName + ".png.";

        // 3. Ảnh phải đọc được pixel -> tự bật Read/Write
        string texPath = AssetDatabase.GetAssetPath(tex);
        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null && !importer.isReadable)
        {
            importer.isReadable = true;
            importer.SaveAndReimport();
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
            sb.AppendLine("  (đã tự bật Read/Write cho ảnh)");
        }

        // 4. Duyệt từng ô, pixel không khớp 100% thì tìm màu gần nhất trong dataset
        // (dùng helper chung LevelColorMatrixUtility để tool window tự viết sau này
        // cho cùng 1 kết quả).
        var ids = new int[Grid * Grid];
        int w = tex.width, h = tex.height;
        for (int cy = 0; cy < Grid; cy++)
            for (int cx = 0; cx < Grid; cx++)
            {
                int sx = Mathf.Clamp(Mathf.RoundToInt((cx + 0.5f) / Grid * w), 0, w - 1);
                int sy = Mathf.Clamp(Mathf.RoundToInt((cy + 0.5f) / Grid * h), 0, h - 1);
                var sample = tex.GetPixel(sx, sy);
                var best = LevelColorMatrixUtility.FindClosest(sample, available, db);
                ids[cy * Grid + cx] = (int)best;
            }

        // 4b. Khử nhiễu: ô lẻ loi giữa vùng 1 màu khác thì đổi theo số đông
        // (tránh pixel lẻ như trong ảnh test). Chạy 2 lượt trên snapshot.
        int denoised = 0;
        for (int pass = 0; pass < 2; pass++)
        {
            var snap = (int[])ids.Clone();
            bool changed = false;
            for (int cy = 0; cy < Grid; cy++)
                for (int cx = 0; cx < Grid; cx++)
                {
                    var mine = (YarnColor)snap[cy * Grid + cx];
                    var counts = new Dictionary<YarnColor, int>();
                    int neighbors = 0;
                    for (int oy = -1; oy <= 1; oy++)
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            if (ox == 0 && oy == 0) continue;
                            int nx = cx + ox, ny = cy + oy;
                            if (nx < 0 || ny < 0 || nx >= Grid || ny >= Grid) continue;
                            var nc = (YarnColor)snap[ny * Grid + nx];
                            if (!counts.ContainsKey(nc)) counts[nc] = 0;
                            counts[nc]++;
                            neighbors++;
                        }
                    int same = counts.TryGetValue(mine, out var s) ? s : 0;
                    YarnColor other = YarnColor.None;
                    int otherCount = 0;
                    foreach (var kv in counts)
                        if (kv.Key != mine && kv.Value > otherCount) { other = kv.Key; otherCount = kv.Value; }
                    if (same <= 1 && otherCount >= neighbors - 1 && neighbors > 0)
                    {
                        ids[cy * Grid + cx] = (int)other;
                        denoised++;
                        changed = true;
                    }
                }
            if (!changed) break;
        }
        if (denoised > 0) sb.AppendLine($"  (đã khử {denoised} ô nhiễu lẻ)");

        // 4c. Gom thành entries sparse: mỗi màu 1 entry (màu + màu hiển thị
        // chuẩn + danh sách ô) — file nhẹ, Inspector đọc được màu nào ở đâu.
        var builtEntries = LevelColorMatrixUtility.BuildEntries(ids, Grid, db);
        var ownerCounts = builtEntries.ToDictionary(e => e.colorId, e => e.cells != null ? e.cells.Count : 0);

        // 5. Lưu file matrix
        string assetPath = $"{MatrixFolder}/{levelName}.asset";
        var matrix = AssetDatabase.LoadAssetAtPath<LevelColorMatrix>(assetPath);
        if (matrix == null)
        {
            matrix = ScriptableObject.CreateInstance<LevelColorMatrix>();
            AssetDatabase.CreateAsset(matrix, assetPath);
        }
        matrix.levelName = levelName;
        matrix.gridSize = Grid;
        matrix.entries = builtEntries;
        EditorUtility.SetDirty(matrix);

        // 6. Báo cáo: ô/box đúng công thức runtime (3 sợi = 1 box)
        sb.AppendLine("[OK] " + levelName + " -> " + assetPath + $" ({builtEntries.Count} màu)");
        foreach (var kv in ownerCounts.OrderByDescending(kv => kv.Value))
        {
            int yarn = yarnCounts.TryGetValue(kv.Key, out var y) ? y : 0;
            int boxes = Math.Max(1, Mathf.CeilToInt(yarn / 3f));
            int batch = Math.Max(1, Mathf.CeilToInt(kv.Value / (float)boxes));
            string warn = kv.Value == 0 ? " [CẢNH BÁO: 0 ô - màu này bắn lên sẽ rỗng]" : "";
            sb.AppendLine($"  - {kv.Key}: {kv.Value} ô / {yarn} sợi = {boxes} box (~{batch} ô/box){warn}");
        }
        foreach (var kv in yarnCounts)
            if (!ownerCounts.ContainsKey(kv.Key))
                sb.AppendLine($"  - {kv.Key}: 0 ô / {kv.Value} sợi [CẢNH BÁO: màu này bắn lên sẽ rỗng]");
        return sb.ToString();
    }
}
