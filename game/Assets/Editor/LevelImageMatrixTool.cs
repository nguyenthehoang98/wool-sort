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

    private LevelColorMatrix cachedMatrix;
    private string cachedLevel;

    [MenuItem("Tools/Puzzle/5. Extract ảnh thành ma trận màu", false, 5)]
    private static void Open() => GetWindow<LevelImageMatrixTool>("Extract ma trận màu");

    private void OnEnable()
    {
        RefreshLevelNames();
        AutoLoadSourceTexture();
    }

    private void RefreshLevelNames()
    {
        levelNames = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(Path.GetFileNameWithoutExtension)
            .OrderBy(n => n)
            .ToList();
        selectedLevelIndex = Mathf.Clamp(selectedLevelIndex, 0, Mathf.Max(0, levelNames.Count - 1));
        cachedLevel = null; // ép nạp lại preview
    }

    private void OnGUI()
    {
        GUILayout.Label("EXTRACT ẢNH -> MATRIX COLOR ID", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Mỗi ảnh được extract thành lưới 28x28, mỗi ô = 1 YarnColor gần nhất\n" +
            "(pixel không khớp 100% dataset sẽ được duyệt tìm màu gần nhất).\n" +
            "Runtime JamImage ưu tiên đọc file này, không đọc trực tiếp ảnh nữa.",
            MessageType.Info);

        EditorGUILayout.BeginHorizontal();

        // TRÁI: chọn level + lưới preview
        EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(330));
        GUILayout.Label("Ảnh (lưới matrix)", EditorStyles.boldLabel);
        if (levelNames.Count > 0)
        {
            int pick = EditorGUILayout.Popup("Level", selectedLevelIndex, levelNames.ToArray());
            if (pick != selectedLevelIndex)
            {
                selectedLevelIndex = pick;
                AutoLoadSourceTexture();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("Không tìm thấy prefab nào trong " + LevelsFolder, MessageType.Warning);
        }
        DrawMatrixPreview();
        EditorGUILayout.EndVertical();

        // PHẢI: extract
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Extract", EditorStyles.boldLabel);
        sourceTexture = (Texture2D)EditorGUILayout.ObjectField("Ảnh nguồn", sourceTexture, typeof(Texture2D), false);
        EditorGUILayout.HelpBox("Đổi level bên trái sẽ tự nạp ảnh Resources/Images/{Level}.png (nếu có). Không có thì kéo ảnh vào tay.", MessageType.None);
        if (GUILayout.Button("Tải lại danh sách level"))
            RefreshLevelNames();

        if (GUILayout.Button("Extract level đã chọn", GUILayout.Height(36)))
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

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    /// <summary>Đổi level -> tự nạp ảnh nguồn đúng thứ tự fallback của JamImage (khỏi kéo tay).</summary>
    private void AutoLoadSourceTexture()
    {
        if (levelNames.Count == 0) return;
        string levelName = levelNames[Mathf.Clamp(selectedLevelIndex, 0, levelNames.Count - 1)];
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.jpg");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/Level_{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/LevelImages/{levelName}.png");
        if (tex != null) sourceTexture = tex;
    }

    private void DrawMatrixPreview()
    {
        if (levelNames.Count == 0)
        {
            EditorGUILayout.HelpBox("Không tìm thấy prefab nào trong " + LevelsFolder, MessageType.Warning);
            return;
        }
        string levelName = levelNames[Mathf.Clamp(selectedLevelIndex, 0, levelNames.Count - 1)];
        // Đổi level hoặc vừa extract xong thì nạp lại asset.
        if (cachedLevel != levelName)
        {
            cachedLevel = levelName;
            cachedMatrix = AssetDatabase.LoadAssetAtPath<LevelColorMatrix>($"{MatrixFolder}/{levelName}.asset");
        }
        if (cachedMatrix == null || !cachedMatrix.IsValid)
        {
            EditorGUILayout.HelpBox($"Chưa có matrix cho {levelName} (hoặc matrix hỏng) — bấm Extract để tạo.", MessageType.None);
            return;
        }
        int g = cachedMatrix.gridSize;
        GUILayout.Label($"{levelName}: {g}x{g} = {cachedMatrix.TotalCellCount} ô, {cachedMatrix.entries.Count} màu", EditorStyles.miniLabel);
        const float cell = 10f;
        float size = g * cell;
        Rect r = GUILayoutUtility.GetRect(size, size, GUILayout.Width(size), GUILayout.Height(size));
        EditorGUI.DrawRect(r, Color.black);
        for (int y = 0; y < g; y++)
        {
            for (int x = 0; x < g; x++)
            {
                var c = cachedMatrix.GetDisplayColor(x, y);
                // y = 0 là hàng dưới trong data -> lật lại cho giống ảnh nhìn từ trên xuống.
                float px = r.x + x * cell;
                float py = r.y + (g - 1 - y) * cell;
                EditorGUI.DrawRect(new Rect(px + 0.5f, py + 0.5f, cell - 1f, cell - 1f), c);
            }
        }
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
        cachedLevel = null; // ép vẽ lại preview với matrix mới
        Debug.Log(report);
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

        // 3. Ảnh phải đọc pixel CHÍNH XÁC: readable + không nén (nén DXT làm lem
        // màu giữa các block -> vừa sai màu vừa nhòe hình) + tắt mipmap.
        string texPath = AssetDatabase.GetAssetPath(tex);
        var importer = AssetImporter.GetAtPath(texPath) as TextureImporter;
        if (importer != null)
        {
            bool dirty = false;
            if (!importer.isReadable) { importer.isReadable = true; dirty = true; }
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                dirty = true;
            }
            if (importer.mipmapEnabled) { importer.mipmapEnabled = false; dirty = true; }
            if (dirty)
            {
                importer.SaveAndReimport();
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(texPath);
                sb.AppendLine("  (đã ép import: Read/Write + Uncompressed + tắt mipmap)");
            }
        }

        // 3b. Ảnh không vuông -> cắt vuông giữa trước khi chia lưới (tránh ép tỉ lệ làm méo hình).
        int w = tex.width, h = tex.height;
        int side = Mathf.Min(w, h);
        int cropX = (w - side) / 2, cropY = (h - side) / 2;
        if (w != h) sb.AppendLine($"  (ảnh {w}x{h} không vuông -> cắt vuông giữa {side}x{side})");

        // 4. Duyệt từng ô: TRUNG BÌNH cả block rồi mới tìm màu gần nhất trong dataset
        // (lấy 1 điểm giữa ô dễ rơi vào hạt nhiễu/gradient của ảnh AI -> sai màu cả ô).
        var ids = new int[Grid * Grid];
        for (int cy = 0; cy < Grid; cy++)
        {
            for (int cx = 0; cx < Grid; cx++)
            {
                int x0 = cropX + Mathf.FloorToInt((float)cx / Grid * side);
                int x1 = cropX + Mathf.CeilToInt((float)(cx + 1) / Grid * side);
                int y0 = cropY + Mathf.FloorToInt((float)cy / Grid * side);
                int y1 = cropY + Mathf.CeilToInt((float)(cy + 1) / Grid * side);
                x0 = Mathf.Clamp(x0, 0, w - 1); x1 = Mathf.Clamp(x1, x0 + 1, w);
                y0 = Mathf.Clamp(y0, 0, h - 1); y1 = Mathf.Clamp(y1, y0 + 1, h);
                var px = tex.GetPixels(x0, y0, x1 - x0, y1 - y0);
                float r = 0f, g = 0f, b = 0f;
                for (int i = 0; i < px.Length; i++) { r += px[i].r; g += px[i].g; b += px[i].b; }
                float inv = 1f / Mathf.Max(1, px.Length);
                var sample = new Color(r * inv, g * inv, b * inv, 1f);
                var best = LevelColorMatrixUtility.FindClosest(sample, available, db);
                ids[cy * Grid + cx] = (int)best;
            }
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
        matrix.InvalidateLookup(); // entries mới -> xóa cache lookup cũ trong session
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
