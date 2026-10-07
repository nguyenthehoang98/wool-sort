using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using _Game.Scripts;
using _Game.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Extract ảnh level thành ma trận color ID (lưới chuẩn DefaultGrid, mỗi ô = 1 YarnColor).
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
    private const int Grid = LevelColorMatrix.DefaultGrid;

    private Vector2 scroll;
    private Vector2 reportScroll;
    private Vector2 jsonScroll;
    private bool selectAllJson;
    private string report = "Kéo ảnh vào + chọn level rồi Extract, hoặc Extract tất cả levels.";

    private Texture2D sourceTexture;
    private TextAsset jsonAsset;
    private string jsonText = "";
    private bool showDataset = true;
    private int selectedLevelIndex;
    private List<string> levelNames = new List<string>();

    private LevelColorMatrix cachedMatrix;
    private string cachedLevel;

    // Studio tự tạo panel, không còn menu riêng.
    private bool initialized;

    /// <summary>Gọi 1 lần khi Studio mở tab (thay OnEnable cũ).</summary>
    public void InitPanel()
    {
        if (initialized) return;
        initialized = true;
        RefreshLevelNames();
        AutoLoadSourceTexture();
    }

    public void DrawPanel() => OnGUI();

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
            "Mỗi ảnh được extract thành lưới chuẩn (mỗi ô = 1 YarnColor gần nhất)\n" +
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
        GUILayout.Space(4);
        if (GUILayout.Button(showDataset ? "Ẩn Dataset ▲" : "Hiện Dataset (màu gốc level) ▼", GUILayout.Height(26)))
            showDataset = !showDataset;
        if (showDataset)
            DrawDatasetPanel();
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

        GUILayout.Space(4);

        GUILayout.Space(8);
        GUILayout.Label("JSON tay (màu phụ / shading)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Dán JSON hoặc kéo TextAsset .json vào để import ma trận đã vẽ tay.\n" +
            "matrix[0] = HÀNG TRÊN CÙNG (nhìn như ảnh), matrix cuối = hàng dưới.\n" +
            "palette = màu IN RA (hex), owners = màu GỐC để matching.\n" +
            "VD: \"4\" hiển thị #E028AA đậm nhưng owners[\"4\"]=\"Pink\" -> bắn Pink vẫn in ra đậm.",
            MessageType.Info);
        jsonAsset = (TextAsset)EditorGUILayout.ObjectField("File JSON", jsonAsset, typeof(TextAsset), false);
        if (jsonAsset != null && GUILayout.Button("Nạp text từ file JSON"))
            jsonText = jsonAsset.text;
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Nội dung JSON", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Chọn tất cả", GUILayout.Width(85)))
        {
            selectAllJson = true;
            GUI.FocusControl("JsonInput");
        }
        if (GUILayout.Button("Dán", GUILayout.Width(55)))
            jsonText = EditorGUIUtility.systemCopyBuffer ?? "";
        if (GUILayout.Button("Xóa", GUILayout.Width(55)))
        {
            jsonText = "";
            jsonAsset = null;
            GUI.FocusControl(null);
        }
        EditorGUILayout.EndHorizontal();
        GUI.SetNextControlName("JsonInput");
        jsonScroll = EditorGUILayout.BeginScrollView(jsonScroll, GUILayout.Height(200));
        jsonText = EditorGUILayout.TextArea(jsonText ?? "", GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
        // Unity mặc định không ăn Ctrl+A trong TextArea -> tự xử lý.
        if (Event.current != null && Event.current.type == EventType.KeyDown
            && Event.current.control && Event.current.keyCode == KeyCode.A
            && GUI.GetNameOfFocusedControl() == "JsonInput")
        {
            var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null) te.SelectAll();
            Event.current.Use();
        }
        if (selectAllJson && GUI.GetNameOfFocusedControl() == "JsonInput")
        {
            var te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            if (te != null) te.SelectAll();
            selectAllJson = false;
            Repaint();
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Import JSON -> level đã chọn", GUILayout.Height(32)))
        {
            if (levelNames.Count == 0)
                report = "[LỖI] Chưa có level nào.";
            else if (string.IsNullOrWhiteSpace(jsonText))
                report = "[LỖI] JSON trống.";
            else
                ImportJson(levelNames[selectedLevelIndex], jsonText);
        }
        if (GUILayout.Button("Export matrix -> JSON", GUILayout.Height(32)))
        {
            if (levelNames.Count == 0)
                report = "[LỖI] Chưa có level nào.";
            else
                ExportJson(levelNames[selectedLevelIndex]);
        }
        EditorGUILayout.EndHorizontal();
        if (levelNames.Count > 0)
        {
            string lv = levelNames[Mathf.Clamp(selectedLevelIndex, 0, levelNames.Count - 1)];
            GUILayout.Label($"File asset ghi đè: {MatrixFolder}/{lv}.asset (Import/Extract tự lưu đè)", EditorStyles.miniLabel);
        }
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy JSON mẫu", GUILayout.Height(28)))
        {
            jsonText = BuildSampleJson();
            EditorGUIUtility.systemCopyBuffer = jsonText;
            report = $"[OK] Đã copy JSON mẫu {Grid}x{Grid} (4 màu: Green/Orange/Pink/Pink-shading) vào clipboard + nạp vào ô text. Sửa matrix/owners rồi bấm Import.";
        }
        if (GUILayout.Button("Nạp mẫu vào ô text", GUILayout.Height(28)))
        {
            jsonText = BuildSampleJson();
            report = "[OK] Đã nạp JSON mẫu vào ô text. Sửa rồi bấm Import (chưa copy clipboard).";
        }
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Vá màu thiếu vào matrix (lấy ô từ màu nhiều nhất)", GUILayout.Height(30)))
        {
            if (levelNames.Count == 0)
                report = "[LỖI] Chưa có level nào.";
            else
                FixMissingColors(levelNames[Mathf.Clamp(selectedLevelIndex, 0, levelNames.Count - 1)]);
        }
        if (GUILayout.Button("Xuất ảnh PNG 784x784 (sửa ở PTS)", GUILayout.Height(30)))
        {
            if (levelNames.Count == 0)
                report = "[LỖI] Chưa có level nào.";
            else
                ExportPng(levelNames[Mathf.Clamp(selectedLevelIndex, 0, levelNames.Count - 1)]);
        }

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        // Trong Studio: ô báo cáo cuộn riêng cố định, scroll ngoài của Studio lo phần còn lại.
        GUILayout.Label("Báo cáo:", EditorStyles.miniBoldLabel);
        reportScroll = EditorGUILayout.BeginScrollView(reportScroll, GUILayout.Height(180));
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
        GUILayout.Label($"{levelName}: {g}x{g} = {cachedMatrix.TotalCellCount} ô, {cachedMatrix.GetColors().Count} màu", EditorStyles.miniLabel);
        float cell = Mathf.Max(2f, 320f / g);
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

    /// <summary>
    /// Hiện dataset màu GỐC của level đang chọn (đúng nguồn runtime dùng để matching).
    /// Sửa dataset = sửa prefab (holder.colorCountEntries / YarnPart.initColor), KHÔNG phải sửa matrix.
    /// </summary>
    private void DrawDatasetPanel()
    {
        if (levelNames.Count == 0) return;
        string levelName = levelNames[Mathf.Clamp(selectedLevelIndex, 0, levelNames.Count - 1)];
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Levels/{levelName}.prefab");
        if (prefab == null)
        {
            EditorGUILayout.HelpBox("Không thấy prefab " + levelName, MessageType.Warning);
            return;
        }
        var holder = prefab.GetComponent<LevelYarnInfoHolder>();
        if (holder == null) holder = prefab.GetComponentInChildren<LevelYarnInfoHolder>(true);

        var available = new List<YarnColor>();
        var yarnCounts = new Dictionary<YarnColor, int>();
        string src;
        if (holder != null && holder.colorCountEntries != null && holder.colorCountEntries.Count > 0)
        {
            src = "holder.colorCountEntries";
            foreach (var e in holder.colorCountEntries)
                if (e.count > 0 && !available.Contains(e.color))
                {
                    available.Add(e.color);
                    yarnCounts[e.color] = e.count;
                }
        }
        else
        {
            src = "YarnPart.initColor (holder trống)";
            foreach (var p in prefab.GetComponentsInChildren<YarnPart>(true))
            {
                if (!available.Contains(p.initColor)) { available.Add(p.initColor); yarnCounts[p.initColor] = 0; }
                yarnCounts[p.initColor]++;
            }
        }

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label($"Dataset {levelName} ({available.Count} màu gốc, nguồn: {src})", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Đây là các màu gameplay được phép. Extract/Import chỉ được map về các màu này.\n" +
            "Muốn thêm/bớt màu -> sửa PREFAB (holder.colorCountEntries), rồi Extract lại.",
            MessageType.None);
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        foreach (var c in available)
        {
            EditorGUILayout.BeginHorizontal();
            var sw = LevelColorMatrixUtility.RefColor(db, c);
            sw.a = 1f;
            var r = GUILayoutUtility.GetRect(18, 18, GUILayout.Width(18), GUILayout.Height(18));
            EditorGUI.DrawRect(r, sw);
            int yarn = yarnCounts.TryGetValue(c, out var y) ? y : 0;
            GUILayout.Label($"{c} - {yarn} sợi (#{ColorUtility.ToHtmlStringRGB(sw)})");
            EditorGUILayout.EndHorizontal();
        }
        if (available.Count == 0)
            EditorGUILayout.HelpBox("Level chưa có màu nào.", MessageType.Warning);
        EditorGUILayout.EndVertical();
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

        // 1. Dataset = các màu level đang dùng (đúng nguồn runtime JamImage.Initialize dùng).
        // Tự đếm lại + lưu holder trước (chống stale).
        if (!RefreshHolderCounts(levelName, out var available, out var yarnCounts, sb))
            return "[LỖI] " + levelName + ": level không có màu nào.";
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
        // GIỮ MÀU GỐC: display từng ô = màu ảnh trung bình thật, owner = màu dataset gần nhất.
        var ids = new int[Grid * Grid];
        var shots = new Color[Grid * Grid];
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
                shots[cy * Grid + cx] = sample;
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

        // 4c. Mảng phẳng: owner từng ô = màu dataset gần nhất (để matching),
        // display từng ô = màu ảnh gốc (để in lên đẹp, không ép về màu chuẩn).
        var flatOwners = new YarnColor[Grid * Grid];
        var flatShows = new Color[Grid * Grid];
        var ownerCounts = new Dictionary<YarnColor, int>();
        for (int i = 0; i < ids.Length; i++)
        {
            var c = (YarnColor)ids[i];
            flatOwners[i] = c;
            var show = shots[i];
            show.a = 1f;
            flatShows[i] = show;
            if (!ownerCounts.ContainsKey(c)) ownerCounts[c] = 0;
            ownerCounts[c]++;
        }

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
        matrix.BakeFlatArrays(flatOwners, flatShows);
        EditorUtility.SetDirty(matrix);

        // 6. Báo cáo: ô/box đúng công thức runtime (3 sợi = 1 box)
        sb.AppendLine("[OK] " + levelName + " -> " + assetPath + $" ({ownerCounts.Count} màu gốc, display giữ màu ảnh)");
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

    // ---------- JSON tay: palette (màu in) + owners (màu gốc matching) ----------
    // LƯU Ý: Unity JsonUtility KHÔNG serialize Dictionary {"1":"#..."} nên
    // import/export bên dưới tự parse/build chuỗi JSON tay (Regex + StringBuilder).

    private sealed class MatrixJsonDto
    {
        public int width = LevelColorMatrix.DefaultGrid;
        public int height = LevelColorMatrix.DefaultGrid;
        public Dictionary<string, string> palette = new Dictionary<string, string>();
        public Dictionary<string, string> owners; // optional: "4" -> "Pink"
        public List<List<int>> matrix = new List<List<int>>();
    }

    /// <summary>Tạo JSON mẫu copy sẵn: 1=Green nền, 2=Orange viền, 3=Pink, 4=Pink đậm (shading của Pink).</summary>
    private static string BuildSampleJson()
    {
        const int g = LevelColorMatrix.DefaultGrid;
        var dto = new MatrixJsonDto
        {
            width = g,
            height = g,
            palette = new Dictionary<string, string>
            {
                { "1", "#8EC336" },
                { "2", "#FF8C33" },
                { "3", "#FF75C4" },
                { "4", "#E028AA" },
            },
            owners = new Dictionary<string, string>
            {
                { "1", "Green" },
                { "2", "Orange" },
                { "3", "Pink" },
                { "4", "Pink" },
            },
            matrix = new List<List<int>>()
        };
        // Nền 1, khối hoa ở giữa: viền 2, cánh 3, nhụy đậm 4 (tỉ lệ theo grid).
        int q = g / 4, h = g / 2;
        for (int r = 0; r < g; r++)
        {
            var row = new List<int>();
            for (int c = 0; c < g; c++)
            {
                int id = 1;
                bool inBox = r >= h - q && r < h + q && c >= h - q && c < h + q;
                bool inInner = r >= h - q / 2 && r < h + q / 2 && c >= h - q / 2 && c < h + q / 2;
                bool inCore = r >= h - 2 && r < h + 2 && c >= h - 2 && c < h + 2;
                if (inCore) id = 4;
                else if (inInner) id = 3;
                else if (inBox) id = 2;
                row.Add(id);
            }
            dto.matrix.Add(row);
        }
        return BuildMatrixJsonString(dto);
    }

    private static string BuildMatrixJsonString(MatrixJsonDto dto)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");
        sb.AppendLine($"  \"width\": {dto.width},");
        sb.AppendLine($"  \"height\": {dto.height},");
        sb.AppendLine("  \"palette\": {");
        var pKeys = dto.palette.Keys.OrderBy(k => int.TryParse(k, out int n) ? n : int.MaxValue).ToList();
        for (int i = 0; i < pKeys.Count; i++)
            sb.AppendLine($"    \"{pKeys[i]}\": \"{dto.palette[pKeys[i]]}\"{(i < pKeys.Count - 1 ? "," : "")}");
        sb.AppendLine("  },");
        if (dto.owners != null && dto.owners.Count > 0)
        {
            sb.AppendLine("  \"owners\": {");
            var oKeys = dto.owners.Keys.OrderBy(k => int.TryParse(k, out int n) ? n : int.MaxValue).ToList();
            for (int i = 0; i < oKeys.Count; i++)
                sb.AppendLine($"    \"{oKeys[i]}\": \"{dto.owners[oKeys[i]]}\"{(i < oKeys.Count - 1 ? "," : "")}");
            sb.AppendLine("  },");
        }
        sb.AppendLine("  \"matrix\": [");
        for (int r = 0; r < dto.matrix.Count; r++)
            sb.AppendLine($"    [{string.Join(",", dto.matrix[r])}]{(r < dto.matrix.Count - 1 ? "," : "")}");
        sb.AppendLine("  ]");
        sb.AppendLine("}");
        return sb.ToString();
    }

    /// <summary>Parse JSON dict-style {"1":"#..."} + matrix số. Trả false + err nếu sai format.</summary>
    private static bool TryParseMatrixJson(string text, out MatrixJsonDto dto, out string err)
    {
        dto = new MatrixJsonDto();
        err = null;
        try
        {
            var mW = Regex.Match(text, @"""width""\s*:\s*(\d+)");
            var mH = Regex.Match(text, @"""height""\s*:\s*(\d+)");
            if (!mW.Success || !mH.Success) { err = "Thiếu width/height."; return false; }
            dto.width = int.Parse(mW.Groups[1].Value);
            dto.height = int.Parse(mH.Groups[1].Value);

            dto.palette = ParseStringMapBlock(text, "palette");
            if (dto.palette == null || dto.palette.Count == 0) { err = "Thiếu palette (VD \"palette\": {\"1\": \"#8EC336\"})."; return false; }
            dto.owners = ParseStringMapBlock(text, "owners"); // optional

            if (!TryParseMatrixBlock(text, dto.width, dto.height, out var mat, out err)) { dto = null; return false; }
            dto.matrix = mat;
            return true;
        }
        catch (Exception ex) { err = "JSON parse fail: " + ex.Message; dto = null; return false; }
    }

    private static Dictionary<string, string> ParseStringMapBlock(string text, string key)
    {
        int ki = text.IndexOf("\"" + key + "\"", StringComparison.Ordinal);
        if (ki < 0) return null;
        int b0 = text.IndexOf('{', ki);
        if (b0 < 0) return null;
        int depth = 0, b1 = -1;
        for (int i = b0; i < text.Length; i++)
        {
            if (text[i] == '{') depth++;
            else if (text[i] == '}') { depth--; if (depth == 0) { b1 = i; break; } }
        }
        if (b1 < 0) return null;
        var inner = text.Substring(b0 + 1, b1 - b0 - 1);
        var map = new Dictionary<string, string>();
        foreach (Match m in Regex.Matches(inner, @"""([^""]+)""\s*:\s*""([^""]*)"""))
            map[m.Groups[1].Value] = m.Groups[2].Value;
        return map;
    }

    private static bool TryParseMatrixBlock(string text, int w, int h, out List<List<int>> mat, out string err)
    {
        mat = new List<List<int>>();
        err = null;
        int ki = text.IndexOf("\"matrix\"", StringComparison.Ordinal);
        if (ki < 0) { err = "Thiếu matrix."; return false; }
        int b0 = text.IndexOf('[', ki);
        if (b0 < 0) { err = "Thiếu matrix."; return false; }
        int depth = 0, b1 = -1;
        for (int i = b0; i < text.Length; i++)
        {
            if (text[i] == '[') depth++;
            else if (text[i] == ']') { depth--; if (depth == 0) { b1 = i; break; } }
        }
        if (b1 < 0) { err = "matrix ngoặc [] không đóng."; return false; }
        var inner = text.Substring(b0 + 1, b1 - b0 - 1); // các hàng [1,2,...]
        foreach (Match row in Regex.Matches(inner, @"\[([^\[\]]*)\]"))
        {
            var list = new List<int>();
            foreach (Match n in Regex.Matches(row.Groups[1].Value, @"-?\d+"))
                list.Add(int.Parse(n.Value));
            if (list.Count > 0) mat.Add(list);
        }
        if (mat.Count != h) { err = $"matrix có {mat.Count} hàng, cần {h}."; return false; }
        for (int r = 0; r < mat.Count; r++)
            if (mat[r].Count != w) { err = $"hàng {r} có {mat[r].Count} ô, cần {w}."; return false; }
        return true;
    }

    /// <summary>
    /// Đếm lại holder từ part thật + lưu prefab (chống holder stale).
    /// Trả về dataset màu gốc đúng nguồn runtime (colorCountEntries count > 0).
    /// </summary>
    private static bool RefreshHolderCounts(string levelName, out List<YarnColor> available, out Dictionary<YarnColor, int> yarnCounts, StringBuilder log)
    {
        available = new List<YarnColor>();
        yarnCounts = new Dictionary<YarnColor, int>();
        string path = $"Assets/Resources/Levels/{levelName}.prefab";
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(path);
            if (root == null) return false;
            var holder = root.GetComponent<LevelYarnInfoHolder>();
            if (holder == null) holder = root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null)
            {
                foreach (var p in root.GetComponentsInChildren<YarnPart>(true))
                    if (!available.Contains(p.initColor)) { available.Add(p.initColor); yarnCounts[p.initColor] = 0; }
                return available.Count > 0;
            }
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            if (holder.colorCountEntries != null)
                foreach (var e in holder.colorCountEntries)
                    if (e.count > 0 && !available.Contains(e.color)) { available.Add(e.color); yarnCounts[e.color] = e.count; }
            log?.AppendLine($"  (holder {levelName} đã tự đếm lại + lưu)");
            return available.Count > 0;
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>
    /// Vá màu thiếu: màu nào của level mà 0 ô thì lấy ô từ màu nhiều nhất sang.
    /// Mỗi màu cần ít nhất (sợi/3) ô để mỗi box đều có batch để tô.
    /// </summary>
    private void FixMissingColors(string levelName)
    {
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        if (db == null) { report = "[LỖI] Không thấy Database: " + DatabasePath; return; }
        var matrix = AssetDatabase.LoadAssetAtPath<LevelColorMatrix>($"{MatrixFolder}/{levelName}.asset");
        if (matrix == null || !matrix.IsValid) { report = "[LỖI] Chưa có matrix hợp lệ cho " + levelName + " — Extract/Import trước."; return; }
        var sb = new StringBuilder();
        if (!RefreshHolderCounts(levelName, out _, out var yarnCounts, sb))
        { report = "[LỖI] " + levelName + ": level không có màu nào."; return; }
        int g = matrix.gridSize;

        var owners = new YarnColor[g * g];
        var displays = new Color[g * g];
        var cellsOf = new Dictionary<YarnColor, List<Vector2Int>>();
        for (int y = 0; y < g; y++)
            for (int x = 0; x < g; x++)
            {
                owners[y * g + x] = matrix.GetColorId(x, y);
                displays[y * g + x] = matrix.GetDisplayColor(x, y);
                var o = owners[y * g + x];
                if (!cellsOf.ContainsKey(o)) cellsOf[o] = new List<Vector2Int>();
                cellsOf[o].Add(new Vector2Int(x, y));
            }
        int Cells(YarnColor c) => cellsOf.TryGetValue(c, out var l) ? l.Count : 0;

        var missing = new List<YarnColor>();
        foreach (var kv in yarnCounts)
            if (kv.Value > 0 && Cells(kv.Key) == 0) missing.Add(kv.Key);
        if (missing.Count == 0) { report = $"[OK] {levelName}: không thiếu màu nào."; return; }

        var sb2 = new StringBuilder();
        sb2.AppendLine($"[VÁ] {levelName}: thiếu {missing.Count} màu.");
        foreach (var m in missing.OrderByDescending(m => yarnCounts[m]))
        {
            int need = Math.Max(1, Mathf.CeilToInt(yarnCounts[m] / 3f));
            // Cho màu cho: nhiều ô nhất mà sau khi cho vẫn đủ batch của nó.
            YarnColor donor = YarnColor.None;
            int donorCells = -1;
            foreach (var kv in cellsOf)
            {
                int yarn = yarnCounts.TryGetValue(kv.Key, out var y) ? y : 0;
                int keep = Math.Max(1, Mathf.CeilToInt(yarn / 3f));
                if (kv.Value.Count - need >= keep && kv.Value.Count > donorCells) { donor = kv.Key; donorCells = kv.Value.Count; }
            }
            if (donor == YarnColor.None)
            {
                // Không ai đủ cho mà vẫn đủ batch -> lấy của đứa nhiều nhất (báo rõ).
                foreach (var kv in cellsOf)
                    if (kv.Value.Count > donorCells) { donor = kv.Key; donorCells = kv.Value.Count; }
                sb2.AppendLine($"  [CHÚ Ý] {m} lấy {need} ô của {donor} nhưng {donor} có thể thiếu batch — kiểm tra lại.");
            }
            var show = LevelColorMatrixUtility.RefColor(db, m);
            show.a = 1f;
            int take = Math.Min(need, cellsOf[donor].Count);
            for (int i = 0; i < take; i++)
            {
                var cell = cellsOf[donor][0];
                cellsOf[donor].RemoveAt(0);
                owners[cell.y * g + cell.x] = m;
                displays[cell.y * g + cell.x] = show;
                if (!cellsOf.ContainsKey(m)) cellsOf[m] = new List<Vector2Int>();
                cellsOf[m].Add(cell);
            }
            sb2.AppendLine($"  - {m} ({yarnCounts[m]} sợi): lấy {take} ô từ {donor}.");
        }

        matrix.BakeFlatArrays(owners, displays);
        EditorUtility.SetDirty(matrix);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        cachedLevel = null;
        sb2.AppendLine("[OK] Đã vá xong — quét Validate lại. Muốn đẹp thì Export JSON sửa tay vị trí ô.");
        report = sb.ToString() + sb2.ToString();
        Debug.Log(report);
    }

    /// <summary>
    /// Import JSON vẽ tay -> .asset. matrix[0] = hàng TRÊN cùng.
    /// palette[id] = màu IN RA (hex). owners[id] = màu GỐC để matching
    /// (thiếu owners -> tự map hex về YarnColor dataset gần nhất).
    /// </summary>
    private void ImportJson(string levelName, string text)
    {
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        if (db == null) { report = "[LỖI] Không thấy Database: " + DatabasePath; return; }
        if (!TryParseMatrixJson(text, out var dto, out var parseErr))
        { report = "[LỖI] JSON: " + parseErr; return; }
        if (dto.width != Grid || dto.height != Grid)
        { report = $"[LỖI] JSON {dto.width}x{dto.height}, tool cần {Grid}x{Grid}."; return; }
        if (dto.matrix.Count != Grid)
        { report = $"[LỖI] matrix có {dto.matrix.Count} hàng, cần {Grid}."; return; }

        // Dataset gốc của level (đúng nguồn runtime dùng để matching).
        // Tự đếm lại + lưu holder trước khi đọc (khỏi mở prefab tay — chống holder stale).
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/Resources/Levels/{levelName}.prefab");
        if (prefab == null) { report = "[LỖI] " + levelName + ": không thấy prefab."; return; }
        var holderSb = new StringBuilder();
        if (!RefreshHolderCounts(levelName, out var available, out _, holderSb))
        { report = "[LỖI] " + levelName + ": level không có màu nào."; return; }
        if (holderSb.Length > 0) Debug.Log(holderSb.ToString());

        // Giải palette hex -> display, owners -> YarnColor gốc.
        var displayById = new Dictionary<int, Color>();
        var ownerById = new Dictionary<int, YarnColor>();
        foreach (var kv in dto.palette)
        {
            if (!int.TryParse(kv.Key, out int id)) { report = $"[LỖI] palette key '{kv.Key}' không phải số."; return; }
            if (!ColorUtility.TryParseHtmlString(kv.Value, out var c))
            { report = $"[LỖI] palette[{kv.Key}]='{kv.Value}' không phải hex."; return; }
            c.a = 1f;
            displayById[id] = c;
        }
        foreach (var id in displayById.Keys)
        {
            YarnColor owner = YarnColor.None;
            bool hasOwner = dto.owners != null && dto.owners.TryGetValue(id.ToString(), out var name)
                && Enum.TryParse<YarnColor>(name, true, out owner);
            if (!hasOwner)
                owner = LevelColorMatrixUtility.FindClosest(displayById[id], available, db); // fallback
            if (!available.Contains(owner))
            {
                // Ô ghi màu không còn trong level -> map về màu gần nhất còn dùng (giống runtime).
                owner = LevelColorMatrixUtility.FindClosest(
                    LevelColorMatrixUtility.RefColor(db, owner), available, db);
            }
            ownerById[id] = owner;
        }

        var owners = new YarnColor[Grid * Grid];
        var displays = new Color[Grid * Grid];
        for (int r = 0; r < Grid; r++)
        {
            var row = dto.matrix[r];
            if (row == null || row.Count != Grid)
            { report = $"[LỖI] hàng {r} có {row?.Count ?? 0} ô, cần {Grid}."; return; }
            int y = Grid - 1 - r; // JSON hàng 0 = trên cùng -> data y = dưới lên
            for (int x = 0; x < Grid; x++)
            {
                int id = row[x];
                if (!displayById.ContainsKey(id))
                { report = $"[LỖI] ô [hàng {r}, cột {x}] id={id} không có trong palette."; return; }
                owners[y * Grid + x] = ownerById[id];
                displays[y * Grid + x] = displayById[id];
            }
        }

        if (!AssetDatabase.IsValidFolder(MatrixFolder))
            AssetDatabase.CreateFolder("Assets/Resources", "LevelColorMatrix");
        string assetPath = $"{MatrixFolder}/{levelName}.asset";
        var matrix = AssetDatabase.LoadAssetAtPath<LevelColorMatrix>(assetPath);
        if (matrix == null)
        {
            matrix = ScriptableObject.CreateInstance<LevelColorMatrix>();
            AssetDatabase.CreateAsset(matrix, assetPath);
        }
        matrix.levelName = levelName;
        matrix.gridSize = Grid;
        matrix.BakeFlatArrays(owners, displays);
        EditorUtility.SetDirty(matrix);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        cachedLevel = null;

        var sb = new StringBuilder();
        sb.AppendLine($"[OK-JSON] {levelName} -> {assetPath} ({Grid}x{Grid})");
        // Tổng theo màu gốc (đúng số runtime dùng để chia batch).
        var sum = new Dictionary<YarnColor, int>();
        var showOf = new Dictionary<YarnColor, Color>();
        for (int i = 0; i < owners.Length; i++)
        {
            if (!sum.ContainsKey(owners[i])) { sum[owners[i]] = 0; showOf[owners[i]] = displays[i]; }
            sum[owners[i]]++;
        }
        foreach (var kv in sum.OrderByDescending(kv => kv.Value))
            sb.AppendLine($"  - gốc {kv.Key} | in #{ColorUtility.ToHtmlStringRGB(showOf[kv.Key])} | {kv.Value} ô");
        foreach (var kv in sum.OrderByDescending(kv => kv.Value))
            sb.AppendLine($"  = {kv.Key}: tổng {kv.Value} ô (gồm mọi shading)");
        // Đủ màu chưa: màu nào của level mà 0 ô thì box màu đó đầy sẽ lỗi PrintImage.
        bool lack = false;
        foreach (var c in available)
            if (!sum.ContainsKey(c) || sum[c] == 0)
            {
                sb.AppendLine($"  [THIẾU MÀU] {c} có sợi trong level nhưng 0 ô trong matrix — box {c} đầy sẽ lỗi PrintImage! Sửa owners/matrix rồi Import lại.");
                lack = true;
            }
        if (!lack) sb.AppendLine("  [OK] Đủ màu: mọi màu của level đều có ô.");
        report = sb.ToString();
        Debug.Log(report);
        jsonText = text; // giữ lại để sửa tiếp
    }

    /// <summary>Xuất matrix hiện tại -> JSON dán vào ô text (để sửa tay rồi Import lại).</summary>
    private void ExportJson(string levelName)
    {
        var matrix = AssetDatabase.LoadAssetAtPath<LevelColorMatrix>($"{MatrixFolder}/{levelName}.asset");
        if (matrix == null || !matrix.IsValid) { report = "[LỖI] Chưa có matrix hợp lệ cho " + levelName; return; }
        int g = matrix.gridSize;
        // Gom (gốc + display) -> palette id.
        var keyToId = new Dictionary<string, int>();
        var palette = new Dictionary<string, string>();
        var owners = new Dictionary<string, string>();
        int nextId = 1;
        var grid = new int[g, g];
        for (int y = 0; y < g; y++)
            for (int x = 0; x < g; x++)
            {
                var owner = matrix.GetColorId(x, y);
                var show = matrix.GetDisplayColor(x, y);
                show.a = 1f;
                string key = owner + "|" + ColorUtility.ToHtmlStringRGB(show);
                if (!keyToId.TryGetValue(key, out int id))
                {
                    id = nextId++;
                    keyToId[key] = id;
                    palette[id.ToString()] = "#" + ColorUtility.ToHtmlStringRGB(show);
                    owners[id.ToString()] = owner.ToString();
                }
                grid[y, x] = id;
            }
        var dto = new MatrixJsonDto
        {
            width = g,
            height = g,
            palette = palette,
            owners = owners,
            matrix = new List<List<int>>()
        };
        for (int r = 0; r < g; r++)
        {
            int y = g - 1 - r; // xuất hàng trên cùng trước
            var row = new List<int>();
            for (int x = 0; x < g; x++) row.Add(grid[y, x]);
            dto.matrix.Add(row);
        }
        jsonText = BuildMatrixJsonString(dto);
        report = $"[OK] Đã xuất {levelName} ra ô JSON ({palette.Count} palette id, {keyToId.Count} entry). Sửa hex/owners/matrix rồi bấm Import.";
        Debug.Log(report);
    }

    /// <summary>
    /// Xuất matrix ra PNG (mỗi ô = khối vuông nét, màu đúng display từng ô).
    /// Sửa ở PTS xong thì Extract lại ảnh đó (hoặc vẽ lại JSON) để cập nhật matrix.
    /// </summary>
    private void ExportPng(string levelName)
    {
        var matrix = AssetDatabase.LoadAssetAtPath<LevelColorMatrix>($"{MatrixFolder}/{levelName}.asset");
        if (matrix == null || !matrix.IsValid) { report = "[LỖI] Chưa có matrix hợp lệ cho " + levelName + " — Extract/Import trước."; return; }
        int g = matrix.gridSize;
        int block = Mathf.Max(1, 756 / g);
        var tex = new Texture2D(g * block, g * block, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        for (int y = 0; y < g; y++)
            for (int x = 0; x < g; x++)
            {
                var c = matrix.GetDisplayColor(x, y);
                c.a = 1f;
                // SetPixel y=0 là đáy texture, encoder đưa đáy xuống đáy ảnh
                // -> map thẳng data y vào texture y là ảnh ra đúng chiều preview.
                int py = y * block;
                for (int by = 0; by < block; by++)
                    for (int bx = 0; bx < block; bx++)
                        tex.SetPixel(x * block + bx, py + by, c);
            }
        tex.Apply(false, false);
        const string folder = "Assets/MatrixExport";
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets", "MatrixExport");
        string path = $"{folder}/{levelName}.png";
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        DestroyImmediate(tex);
        AssetDatabase.Refresh();
        var saved = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
        if (saved != null) EditorGUIUtility.PingObject(saved);
        cachedLevel = null;
        report = $"[OK] Đã xuất ảnh {levelName} -> {path} ({g * block}x{g * block}, mỗi ô {block}px).\n" +
                 "Sửa ở PTS (giữ đúng lưới ô, tô tràn từng ô, không chống răng cưa viền), lưu đè rồi bấm Extract level để nạp lại.";
        Debug.Log(report);
    }
}
