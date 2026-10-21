using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Reskin hàng loạt theo BẢNG: mỗi dòng là 1 cặp "ảnh cũ -&gt; ảnh mới".
/// Ghi đè nội dung ảnh mới vào asset cũ, GIU NGUYEN .meta nên GUID không đổi
/// (prefab/scene/material vẫn trỏ đúng) và spriteBorder (9-slice) + PPU không mất.
///
/// Cách dùng:
///   1. "Quét ảnh trong project" hoặc "Nạp từ folder..." để lấy danh sách dòng
///      (quét chạy theo chunk nên không đơ, có thanh tiến trình)
///   2. Mỗi dòng: chọn ảnh MỚI (kéo file từ Explorer, bấm "…", hoặc kéo 1
///      Texture2D trong project) — có preview ảnh đầy đủ
///   3. Cột "Lưu vào" là đường path sẽ ghi ảnh (mặc định = path của ảnh cũ)
///   4. "Áp dụng" — backup bản gốc rồi ghi đè
///
/// Bảng lưu ở Assets/_Game/Configs/ReskinMap.json nên không mất khi đóng Unity.
///
/// Mở: Tools / Reskin / Reskin hàng loạt
/// </summary>
public sealed class BulkReskinWindow : EditorWindow
{
    private const string MenuPath = "Tools/Reskin/Reskin hàng loạt";
    private const string MapPath = "Assets/_Game/Configs/ReskinMap.json";

    // layout: mọi cột dùng chung bề rộng để header với dòng không lệch
    private const int ToggleW = 18;
    private const int CellOldW = 250;
    private const int CellNewW = 250;
    private const int CellDestW = 300;
    private const int ThumbSize = 72;
    private const int LabelW = 60;

    private static readonly string[] ImageExts = { ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tif", ".tiff", ".webp" };

    [Serializable]
    private sealed class Entry
    {
        public string oldPath;   // asset cũ trong project ("Assets/...")
        public string newPath;   // ảnh mới: path tuyệt đối ngoài project, hoặc "Assets/..." nếu đã import
        public string destPath;  // nơi sẽ ghi ("Assets/...")
        public bool enabled = true;
    }

    [Serializable]
    private sealed class MapFile
    {
        public List<Entry> entries = new List<Entry>();
    }

    private readonly List<Entry> entries = new List<Entry>();
    private Vector2 scroll;
    private string filter = "";
    private string report = "";
    private bool mirrorBooster = true;
    private bool showDestColumn = true;
    private bool showUsage = true;

    // ---- quét (chunked, không đơ)
    private bool scanning;
    private float scanProgress;
    private string scanLabel = "";
    private readonly Queue<string> scanQueue = new Queue<string>();
    private readonly Queue<string> scanFolderFiles = new Queue<string>();
    private bool scanIsFolderScan;
    private readonly Dictionary<string, List<string>> usageMap = new Dictionary<string, List<string>>();
    private readonly HashSet<string> knownOldPaths = new HashSet<string>();
    private Dictionary<string, List<string>> folderIndex = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

    // cache để không đọc file / resolve dependency mỗi frame
    private readonly Dictionary<string, string> warnCache = new Dictionary<string, string>();
    private readonly Dictionary<string, Vector2Int> sizeCache = new Dictionary<string, Vector2Int>();
    private readonly Dictionary<string, Vector4> borderCache = new Dictionary<string, Vector4>();

    private readonly Dictionary<string, Texture2D> previewCache = new Dictionary<string, Texture2D>();
    private readonly Dictionary<string, Texture2D> oldPreviewCache = new Dictionary<string, Texture2D>();

    [MenuItem(MenuPath, false, 40)]
    private static void Open()
    {
        var w = GetWindow<BulkReskinWindow>("Reskin hàng loạt");
        w.minSize = new Vector2(1100, 520);
    }

    private void OnEnable()
    {
        LoadMap();
    }

    private void OnDisable()
    {
        EditorApplication.update -= OnScanTick;
        scanning = false;
        foreach (var t in previewCache.Values) DestroyImmediate(t);
        foreach (var t in oldPreviewCache.Values) DestroyImmediate(t);
        previewCache.Clear();
        oldPreviewCache.Clear();
    }

    // ================================================================== UI
    private void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Reskin hàng loạt — bảng ảnh cũ → ảnh mới", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(
            "Ghi đè nội dung ảnh, giữ nguyên .meta (GUID / 9-slice / PPU). Cột \"Lưu vào\" là path sẽ ghi ảnh.",
            EditorStyles.miniLabel);

        DrawToolbar();
        EditorGUILayout.Space(2);
        DrawHeader();
        DrawProgress();
        DrawRows();

        EditorGUILayout.Space(6);
        if (!string.IsNullOrEmpty(report))
            EditorGUILayout.HelpBox(report, MessageType.Info);

        EditorGUILayout.Space(4);
        DrawFooter();
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUI.enabled = !scanning;
            if (GUILayout.Button("Quét ảnh trong project", EditorStyles.toolbarButton, GUILayout.Width(150)))
                StartScanProject();
            if (GUILayout.Button("Nạp từ folder…", EditorStyles.toolbarButton, GUILayout.Width(110)))
                StartScanFolder();
            GUI.enabled = true;

            if (GUILayout.Button("Thêm dòng", EditorStyles.toolbarButton, GUILayout.Width(80)))
                entries.Add(new Entry());
            if (GUILayout.Button("Lưu bảng", EditorStyles.toolbarButton, GUILayout.Width(70))) SaveMap();
            if (GUILayout.Button("Nạp bảng", EditorStyles.toolbarButton, GUILayout.Width(70))) LoadMap();

            GUILayout.FlexibleSpace();
            mirrorBooster = GUILayout.Toggle(mirrorBooster, "Mirror BoosterUI", EditorStyles.toolbarButton, GUILayout.Width(110));
            showDestColumn = GUILayout.Toggle(showDestColumn, "Cột đường path", EditorStyles.toolbarButton, GUILayout.Width(100));
            showUsage = GUILayout.Toggle(showUsage, "Cột nơi dùng", EditorStyles.toolbarButton, GUILayout.Width(95));
            GUILayout.Label(entries.Count + " dòng", EditorStyles.miniLabel);
        }
    }

    private void DrawHeader()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Space(ToggleW);
            GUILayout.Label("ẢNH CŨ (asset trong project)", EditorStyles.miniBoldLabel, GUILayout.Width(CellOldW));
            GUILayout.Label("ẢNH MỚI (kéo file / bấm …)", EditorStyles.miniBoldLabel, GUILayout.Width(CellNewW));
            if (showDestColumn)
                GUILayout.Label("LƯU VÀO (đường path đích)", EditorStyles.miniBoldLabel, GUILayout.Width(CellDestW));
            GUILayout.Label("CẢNH BÁO", EditorStyles.miniBoldLabel, GUILayout.ExpandWidth(true));
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Space(ToggleW);
            filter = GUILayout.TextField(filter, GUILayout.Width(CellOldW));
            GUILayout.Space(CellNewW);
            if (showDestColumn) GUILayout.Space(CellDestW);
        }
    }

    private void DrawProgress()
    {
        if (!scanning) return;
        using (new EditorGUILayout.VerticalScope())
        {
            var rect = GUILayoutUtility.GetRect(10, 18, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, scanProgress, scanLabel);
        }
    }

    private void DrawRows()
    {
        if (entries.Count == 0 && !scanning)
        {
            EditorGUILayout.HelpBox(
                "Chưa có dòng nào. Bấm \"Quét ảnh trong project\" để lấy ảnh đang dùng, " +
                "\"Nạp từ folder…\" để nạp ảnh mới theo tên, hoặc \"Thêm dòng\" để làm tay.",
                MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        foreach (var e in entries)
        {
            if (!string.IsNullOrEmpty(filter) && !MatchFilter(e, filter)) continue;

            using (new EditorGUILayout.HorizontalScope())
            {
                e.enabled = EditorGUILayout.Toggle(e.enabled, GUILayout.Width(ToggleW), GUILayout.Height(ThumbSize));

                DrawOldCell(e);
                DrawNewCell(e);

                if (showDestColumn)
                {
                    using (new EditorGUILayout.VerticalScope(GUILayout.Width(CellDestW)))
                    {
                        string dest = e.destPath ?? "";
                        string newDest = EditorGUILayout.TextField(dest, GUILayout.Width(CellDestW - 4));
                        if (newDest != dest) e.destPath = newDest;
                        EditorGUILayout.LabelField(FileName(e.destPath), EditorStyles.miniLabel);
                    }
                }

                using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
                {
                    EditorGUILayout.LabelField(BuildWarn(e), EditorStyles.miniLabel, GUILayout.Height(16));
                    if (showUsage && !string.IsNullOrEmpty(e.oldPath))
                        EditorGUILayout.LabelField(BuildUsage(e), EditorStyles.miniLabel, GUILayout.Height(16));
                }
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawOldCell(Entry e)
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(CellOldW)))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                var tex = e.oldPath != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(e.oldPath) : null;
                var picked = EditorGUILayout.ObjectField(tex, typeof(Texture2D), false,
                    GUILayout.Width(CellOldW - ThumbSize - 6), GUILayout.Height(ThumbSize));

                var pickedPath = picked != null ? AssetDatabase.GetAssetPath(picked) : null;
                if (pickedPath != e.oldPath)
                {
                    e.oldPath = pickedPath;
                    if (string.IsNullOrEmpty(e.destPath)) e.destPath = pickedPath;
                }

                // preview ảnh cũ (preview chính thức của Unity, đúng sprite rect)
                var rect = GUILayoutUtility.GetRect(ThumbSize, ThumbSize, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize));
                DrawThumb(rect, e.oldPath != null ? GetOldPreview(e.oldPath) : null);
            }
            EditorGUILayout.LabelField(e.oldPath != null ? e.oldPath : "(chưa chọn ảnh cũ)",
                EditorStyles.miniLabel, GUILayout.Width(CellOldW - 4));
        }
    }

    private void DrawNewCell(Entry e)
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(CellNewW)))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                // kéo file từ Explorer / Texture2D trong project vào ô này
                var dropRect = GUILayoutUtility.GetRect(LabelW, ThumbSize, GUILayout.Width(LabelW), GUILayout.Height(ThumbSize));
                HandleDragDrop(e, dropRect);
                GUI.Label(dropRect, "kéo\nfile", EditorStyles.miniLabel);

                var tex = e.newPath != null && e.newPath.StartsWith("Assets/")
                    ? AssetDatabase.LoadAssetAtPath<Texture2D>(e.newPath)
                    : null;
                var picked = EditorGUILayout.ObjectField(tex, typeof(Texture2D), false,
                    GUILayout.Width(CellNewW - ThumbSize - LabelW - 12), GUILayout.Height(ThumbSize));
                if (picked != null)
                {
                    string p = AssetDatabase.GetAssetPath(picked);
                    if (p != e.newPath) e.newPath = p;
                }
                else if (e.newPath != null && e.newPath.StartsWith("Assets/"))
                {
                    e.newPath = null;
                }

                if (GUILayout.Button("…", GUILayout.Width(22), GUILayout.Height(22)))
                {
                    var file = EditorUtility.OpenFilePanel("Chọn ảnh mới", string.Empty, "");
                    if (!string.IsNullOrEmpty(file)) e.newPath = file;
                }
            }

            // preview ảnh mới
            var rect = GUILayoutUtility.GetRect(ThumbSize, ThumbSize, GUILayout.Width(ThumbSize), GUILayout.Height(ThumbSize));
            DrawThumb(rect, e.newPath != null ? GetPreview(e.newPath) : null);

            EditorGUILayout.LabelField(e.newPath != null ? e.newPath : "(chưa gán ảnh mới)",
                EditorStyles.miniLabel, GUILayout.Width(CellNewW - 4));
        }
    }

    private static void DrawThumb(Rect rect, Texture2D tex)
    {
        EditorGUI.DrawRect(rect, new Color(0.16f, 0.16f, 0.16f));
        if (tex != null)
            GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit, true);
        var border = new Color(0f, 0f, 0f, 0.6f);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, 1f), border);
        EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), border);
        EditorGUI.DrawRect(new Rect(rect.x, rect.y, 1f, rect.height), border);
        EditorGUI.DrawRect(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), border);
    }

    private void HandleDragDrop(Entry e, Rect rect)
    {
        var evt = Event.current;
        if (!rect.Contains(evt.mousePosition)) return;
        if (evt.type != EventType.DragUpdated && evt.type != EventType.DragPerform) return;

        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
        if (evt.type == EventType.DragPerform)
        {
            DragAndDrop.AcceptDrag();
            if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                e.newPath = DragAndDrop.paths[0];
            else if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                e.newPath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
            evt.Use();
        }
        evt.Use();
    }

    private void DrawFooter()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            int ready = entries.Count(e => e.enabled && !string.IsNullOrEmpty(e.newPath) && !string.IsNullOrEmpty(e.destPath));
            GUI.enabled = ready > 0 && !scanning;
            if (GUILayout.Button("Áp dụng (" + ready + ")", GUILayout.Height(28))) Apply();
            GUI.enabled = !scanning;
            if (GUILayout.Button("Hoàn tác từ backup", GUILayout.Width(150), GUILayout.Height(28))) Restore();
            if (GUILayout.Button("Lưu bảng", GUILayout.Width(90), GUILayout.Height(28))) SaveMap();
            GUI.enabled = true;
        }
    }

    // ================================================================== helpers
    private bool MatchFilter(Entry e, string f)
    {
        f = f.ToLowerInvariant();
        return (e.oldPath ?? "").ToLowerInvariant().Contains(f)
            || (e.newPath ?? "").ToLowerInvariant().Contains(f)
            || (e.destPath ?? "").ToLowerInvariant().Contains(f);
    }

    private static string FileName(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        return Path.GetFileName(path);
    }

    private string BuildWarn(Entry e)
    {
        if (string.IsNullOrEmpty(e.newPath)) return "";
        if (string.IsNullOrEmpty(e.destPath)) return "chưa có đường path đích";

        string key = (e.oldPath ?? "") + "|" + e.newPath + "|" + e.destPath;
        if (warnCache.TryGetValue(key, out var cached)) return cached;

        string result = "";
        string destExt = Path.GetExtension(e.destPath).ToLower();
        string newExt = Path.GetExtension(e.newPath).ToLower();
        if (destExt != newExt)
        {
            result = "KHÁC ĐỊNH DẠNG " + destExt + " vs " + newExt + " → sẽ bỏ qua";
        }
        else
        {
            var newSize = GetSize(e.newPath);
            var oldTex = !string.IsNullOrEmpty(e.oldPath) ? AssetDatabase.LoadAssetAtPath<Texture2D>(e.oldPath) : null;
            if (newSize.x > 0 && oldTex != null)
            {
                var oldSize = new Vector2Int(oldTex.width, oldTex.height);
                if (newSize != oldSize)
                {
                    var b = GetBorder(e.destPath);
                    if (b != Vector4.zero)
                        result = "9-slice " + b + " nhưng kích thước " + oldSize + " → " + newSize;
                    else
                        result = "Kích thước " + oldSize + " → " + newSize;
                }
            }
        }

        warnCache[key] = result;
        return result;
    }

    private string BuildUsage(Entry e)
    {
        if (string.IsNullOrEmpty(e.oldPath)) return "";
        if (!usageMap.TryGetValue(e.oldPath, out var list)) return "Dùng ở: …";
        return list.Count == 0 ? "Dùng ở: (không asset nào)" : "Dùng ở: " + string.Join(", ", list);
    }

    private Vector2Int GetSize(string path)
    {
        if (sizeCache.TryGetValue(path, out var v)) return v;
        v = ReadPngSize(path);
        sizeCache[path] = v;
        return v;
    }

    private Vector4 GetBorder(string assetPath)
    {
        if (borderCache.TryGetValue(assetPath, out var v)) return v;
        v = ReadBorder(assetPath);
        borderCache[assetPath] = v;
        return v;
    }

    // ================================================================== quét
    private void StartScanProject()
    {
        var list = new List<string>();
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (!p.StartsWith("Assets/", StringComparison.Ordinal)) continue;
            if (!ImageExts.Contains(Path.GetExtension(p).ToLower())) continue;
            list.Add(p);
        }
        list.Sort(StringComparer.OrdinalIgnoreCase);

        scanQueue.Clear();
        foreach (var p in list) scanQueue.Enqueue(p);
        scanFolderFiles.Clear();
        scanIsFolderScan = false;
        SeedKnownPaths();
        scanProgress = 0f;
        scanLabel = "Đang quét ảnh trong project…";
        scanning = true;
        EditorApplication.update -= OnScanTick;
        EditorApplication.update += OnScanTick;
    }

    private void StartScanFolder()
    {
        var folder = EditorUtility.OpenFolderPanel("Folder chứa ảnh mới", string.Empty, "");
        if (string.IsNullOrEmpty(folder)) return;

        var byName = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (!p.StartsWith("Assets/", StringComparison.Ordinal)) continue;
            if (!ImageExts.Contains(Path.GetExtension(p).ToLower())) continue;
            string n = Path.GetFileName(p);
            if (!byName.TryGetValue(n, out var l)) byName[n] = l = new List<string>();
            l.Add(p);
        }

        var files = Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories)
            .Where(f => ImageExts.Contains(Path.GetExtension(f).ToLower()))
            .OrderBy(f => f, StringComparer.OrdinalIgnoreCase)
            .ToList();

        scanFolderFiles.Clear();
        foreach (var f in files) scanFolderFiles.Enqueue(f);
        scanQueue.Clear();
        scanIsFolderScan = true;
        scanProgress = 0f;
        scanLabel = "Đang nạp ảnh mới từ folder…";
        scanning = true;
        EditorApplication.update -= OnScanTick;
        EditorApplication.update += OnScanTick;

        // giữ byName cho vòng lặp xử lý
        folderIndex = byName;
    }

    private void SeedKnownPaths()
    {
        knownOldPaths.Clear();
        foreach (var e in entries)
            if (!string.IsNullOrEmpty(e.oldPath)) knownOldPaths.Add(e.oldPath);
    }

    private void OnScanTick()
    {
        // xử lý theo lô để Editor không đơ
        const int budget = 60;
        int done = 0;

        if (scanIsFolderScan)
        {
            int total = scanFolderFiles.Count;
            while (done < budget && scanFolderFiles.Count > 0)
            {
                string file = scanFolderFiles.Dequeue();
                if (folderIndex.TryGetValue(Path.GetFileName(file), out var cands) && cands.Count > 0)
                {
                    var editable = cands.Where(c => !c.Contains("/Resources/")).ToList();
                    var target = (editable.Count > 0 ? editable : cands)
                        .OrderBy(c => c, StringComparer.Ordinal).First();
                    if (!entries.Any(e => e.newPath == file))
                        entries.Add(new Entry { oldPath = target, newPath = file, destPath = target });
                    knownOldPaths.Add(target);
                }
                done++;
            }
            scanProgress = total == 0 ? 1f : 1f - (scanQueue.Count + scanFolderFiles.Count) / (float)Math.Max(1, total);
            if (scanFolderFiles.Count == 0)
            {
                scanning = false;
                EditorApplication.update -= OnScanTick;
                report = "Đã nạp " + entries.Count + " dòng từ folder.";
            }
        }
        else
        {
            int total = scanQueue.Count;
            while (done < budget && scanQueue.Count > 0)
            {
                string p = scanQueue.Dequeue();
                if (knownOldPaths.Add(p))
                    entries.Add(new Entry { oldPath = p, destPath = p });

                // resolve "dung o" ngay trong luc quet (chunked) de khi ve khong
                // phai goi AssetDatabase.GetDependencies moi dong
                if (!usageMap.ContainsKey(p))
                {
                    var deps = new List<string>();
                    try
                    {
                        deps.AddRange(AssetDatabase.GetDependencies(p, true)
                            .Where(d => d != p && !d.EndsWith(".cs"))
                            .Take(3));
                    }
                    catch { }
                    usageMap[p] = deps;
                }
                done++;
            }
            scanProgress = total == 0 ? 1f : 1f - scanQueue.Count / (float)Math.Max(1, total);
            if (scanQueue.Count == 0)
            {
                scanning = false;
                EditorApplication.update -= OnScanTick;
                report = "Đã quét " + entries.Count + " ảnh trong project.";
            }
        }

        Repaint();
    }

    // ================================================================== lưu / nạp
    private void SaveMap()
    {
        var map = new MapFile { entries = entries };
        string json = JsonUtility.ToJson(map, true);
        var dir = Path.GetDirectoryName(MapPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(ProjectRoot, MapPath.Replace('/', Path.DirectorySeparatorChar)), json);
        AssetDatabase.ImportAsset(MapPath, ImportAssetOptions.ForceUpdate);
        report = "Đã lưu bảng: " + MapPath;
    }

    private void LoadMap()
    {
        string full = Path.Combine(ProjectRoot, MapPath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(full)) { entries.Clear(); return; }
        try
        {
            var map = new MapFile();
            JsonUtility.FromJsonOverwrite(File.ReadAllText(full), map);
            entries.Clear();
            if (map.entries != null) entries.AddRange(map.entries);
            SeedKnownPaths();
            report = "Đã nạp bảng: " + entries.Count + " dòng.";
        }
        catch (Exception e)
        {
            report = "Lỗi đọc bảng: " + e.Message;
        }
    }

    private static string ProjectRoot => Directory.GetParent(Application.dataPath).FullName;

    // ================================================================== apply
    private void Apply()
    {
        int done = 0, skip = 0;
        var problems = new List<string>();
        string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
        string backupRoot = Path.Combine(ProjectRoot, ".reskin_backup", stamp);

        foreach (var e in entries)
        {
            if (!e.enabled) continue;
            if (string.IsNullOrEmpty(e.newPath) || string.IsNullOrEmpty(e.destPath)) { skip++; continue; }

            string destExt = Path.GetExtension(e.destPath).ToLower();
            string newExt = Path.GetExtension(e.newPath).ToLower();
            if (destExt != newExt)
            {
                skip++;
                problems.Add(FileName(e.newPath) + ": khác định dạng (" + destExt + " vs " + newExt + ")");
                continue;
            }

            string destFull = Path.Combine(ProjectRoot, e.destPath.Replace('/', Path.DirectorySeparatorChar));
            string newFull = e.newPath.StartsWith("Assets/")
                ? Path.Combine(ProjectRoot, e.newPath.Replace('/', Path.DirectorySeparatorChar))
                : e.newPath;

            try
            {
                if (File.Exists(destFull))
                {
                    string bak = Path.Combine(backupRoot, e.destPath.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(bak));
                    File.Copy(destFull, bak, true);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destFull));
                File.WriteAllBytes(destFull, File.ReadAllBytes(newFull));

                // ghi ra path khác → copy .meta của ảnh cũ sang để giữ GUID/9-slice/PPU
                if (!string.IsNullOrEmpty(e.oldPath) && e.oldPath != e.destPath)
                {
                    string oldMeta = Path.Combine(ProjectRoot, e.oldPath.Replace('/', Path.DirectorySeparatorChar)) + ".meta";
                    string newMeta = destFull + ".meta";
                    if (File.Exists(oldMeta) && !File.Exists(newMeta))
                        File.Copy(oldMeta, newMeta, true);
                }

                if (e.destPath.StartsWith("Assets/"))
                    AssetDatabase.ImportAsset(e.destPath, ImportAssetOptions.ForceUpdate);

                if (mirrorBooster && e.destPath.StartsWith("Assets/_Game/BoosterUI_Picked/", StringComparison.Ordinal))
                {
                    string name = Path.GetFileName(e.destPath);
                    string mirror = "Assets/_Game/Resources/BoosterUI/" + name;
                    string mirrorFull = Path.Combine(ProjectRoot, mirror.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(mirrorFull))
                    {
                        File.WriteAllBytes(mirrorFull, File.ReadAllBytes(newFull));
                        AssetDatabase.ImportAsset(mirror, ImportAssetOptions.ForceUpdate);
                    }
                }

                done++;
            }
            catch (Exception ex)
            {
                skip++;
                problems.Add(FileName(e.newPath) + ": " + ex.Message);
            }
        }

        AssetDatabase.Refresh();
        report = "Đã ghi " + done + " ảnh, bỏ qua " + skip + ".";
        if (problems.Count > 0) report += "\n" + string.Join("\n", problems);
    }

    private void Restore()
    {
        string backupRoot = Path.Combine(ProjectRoot, ".reskin_backup");
        if (!Directory.Exists(backupRoot)) { report = "Chưa có thư mục backup."; return; }
        var latest = new DirectoryInfo(backupRoot).GetDirectories().OrderByDescending(d => d.Name).FirstOrDefault();
        if (latest == null) { report = "Chưa có bản backup nào."; return; }

        int n = 0;
        foreach (var f in latest.GetFiles("*.*", SearchOption.AllDirectories))
        {
            if (!ImageExts.Contains(f.Extension.ToLower())) continue;
            string rel = f.FullName.Substring(latest.FullName.Length).TrimStart('\\', '/').Replace('\\', '/');
            string dest = Path.Combine(ProjectRoot, rel.Replace('/', Path.DirectorySeparatorChar));
            if (!File.Exists(dest)) continue;
            File.Copy(f.FullName, dest, true);
            n++;
        }
        AssetDatabase.Refresh();
        report = "Đã khôi phục " + n + " ảnh từ backup " + latest.Name + ".";
    }

    // ================================================================== preview
    private Texture2D GetPreview(string path)
    {
        if (previewCache.TryGetValue(path, out var cached)) return cached;
        Texture2D tex = null;
        try
        {
            string full = path.StartsWith("Assets/")
                ? Path.Combine(ProjectRoot, path.Replace('/', Path.DirectorySeparatorChar)) : path;
            if (!File.Exists(full)) { previewCache[path] = null; return null; }
            var bytes = File.ReadAllBytes(full);
            tex = new Texture2D(2, 2, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
            if (ImageConversion.LoadImage(tex, bytes)) tex = Scale(tex);
            else { DestroyImmediate(tex); tex = null; }
        }
        catch { tex = null; }
        previewCache[path] = tex;
        return tex;
    }

    private Texture2D GetOldPreview(string assetPath)
    {
        if (oldPreviewCache.TryGetValue(assetPath, out var cached)) return cached;
        var tex = AssetPreview.GetAssetPreview(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath));
        oldPreviewCache[assetPath] = tex;
        return tex;
    }

    private static Texture2D Scale(Texture2D src)
    {
        const int max = ThumbSize * 2;
        if (src.width <= max && src.height <= max) return src;
        float k = max / (float)Math.Max(src.width, src.height);
        var dst = new Texture2D(Math.Max(1, (int)(src.width * k)), Math.Max(1, (int)(src.height * k)),
                                TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave };
        var pixels = src.GetPixels();
        var outPixels = new Color[dst.width * dst.height];
        for (int y = 0; y < dst.height; y++)
        {
            int sy = Mathf.Min(src.height - 1, (int)(y / k));
            for (int x = 0; x < dst.width; x++)
            {
                int sx = Mathf.Min(src.width - 1, (int)(x / k));
                outPixels[y * dst.width + x] = pixels[sy * src.width + sx];
            }
        }
        dst.SetPixels(outPixels);
        dst.Apply();
        DestroyImmediate(src);
        return dst;
    }

    private static Vector2Int ReadPngSize(string path)
    {
        try
        {
            string full = path.StartsWith("Assets/")
                ? Path.Combine(ProjectRoot, path.Replace('/', Path.DirectorySeparatorChar)) : path;
            using (var fs = File.OpenRead(full))
            {
                var head = new byte[26];
                if (fs.Read(head, 0, 26) < 26) return Vector2Int.zero;
                if (head[0] != 0x89 || head[1] != 0x50) return Vector2Int.zero;
                int w = (head[16] << 24) | (head[17] << 16) | (head[18] << 8) | head[19];
                int h = (head[20] << 24) | (head[21] << 16) | (head[22] << 8) | head[23];
                return new Vector2Int(w, h);
            }
        }
        catch { return Vector2Int.zero; }
    }

    private static Vector4 ReadBorder(string assetPath)
    {
        string meta = Path.Combine(ProjectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar)) + ".meta";
        if (!File.Exists(meta)) return Vector4.zero;
        try
        {
            foreach (var line in File.ReadAllLines(meta))
            {
                if (!line.Contains("spriteBorder:")) continue;
                var s = line.Substring(line.IndexOf("spriteBorder:") + 13)
                    .Replace("{", "").Replace("}", "")
                    .Replace("x:", "").Replace("y:", "").Replace("z:", "").Replace("w:", "")
                    .Replace(" ", "");
                var parts = s.Split(',');
                if (parts.Length != 4) return Vector4.zero;
                return new Vector4(float.Parse(parts[0]), float.Parse(parts[1]),
                                  float.Parse(parts[2]), float.Parse(parts[3]));
            }
        }
        catch { }
        return Vector4.zero;
    }
}
