using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Reskin hàng loạt theo BẢNG: mỗi dòng là 1 cặp "ảnh cũ -> ảnh mới".
/// Ghi đè nội dung ảnh mới vào asset cũ, GIU NGUYEN .meta nên GUID không đổi
/// (prefab/scene/material vẫn trỏ đúng) và spriteBorder (9-slice) + PPU không mất.
///
/// Cách dùng:
///   1. "Quét ảnh trong project" hoặc "Nạp từ folder..." để lấy danh sách dòng
///   2. Mỗi dòng: chọn ảnh MỚI (kéo file từ Explorer, hoặc bấm "..." để browse,
///      hoặc kéo 1 Texture2D trong project vào ô bên phải)
///   3. Cột "Lưu vào" là đường path sẽ ghi ảnh (mặc định = path của ảnh cũ)
///   4. "Áp dụng" — backup bản gốc rồi ghi đè
///
/// Bảng được lưu ở Assets/_Game/Configs/ReskinMap.json nên lưu lại được.
///
/// Mở: Tools / Reskin / Reskin hàng loạt
/// </summary>
public sealed class BulkReskinWindow : EditorWindow
{
    private const string MenuPath = "Tools/Reskin/Reskin hàng loạt";
    private const string MapPath = "Assets/_Game/Configs/ReskinMap.json";

    private static readonly string[] ImageExts = { ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tif", ".tiff", ".webp" };

    [Serializable]
    private sealed class Entry
    {
        public string oldPath;   // asset cũ trong project ("Assets/...") — rỗng nếu chỉ định đích
        public string newPath;   // ảnh mới: đường dẫn tuyệt đối ngoài project, hoặc "Assets/..." nếu đã import
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

    private readonly Dictionary<string, Texture2D> previewCache = new Dictionary<string, Texture2D>();
    private readonly Dictionary<string, Texture2D> oldPreviewCache = new Dictionary<string, Texture2D>();
    private readonly Dictionary<string, string> usageCache = new Dictionary<string, string>();

    [MenuItem(MenuPath, false, 40)]
    private static void Open()
    {
        var w = GetWindow<BulkReskinWindow>("Reskin hàng loạt");
        w.minSize = new Vector2(1040, 520);
    }

    private void OnEnable()
    {
        LoadMap();
    }

    private void OnDisable()
    {
        foreach (var t in previewCache.Values) DestroyImmediate(t);
        foreach (var t in oldPreviewCache.Values) DestroyImmediate(t);
        previewCache.Clear();
        oldPreviewCache.Clear();
    }

    // ------------------------------------------------------------------ UI
    private void OnGUI()
    {
        EditorGUILayout.Space(4);
        EditorGUILayout.LabelField("Reskin hàng loạt — bảng ảnh cũ → ảnh mới",
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Ghi đè nội dung ảnh, giữ nguyên .meta (GUID / 9-slice / PPU). " +
                                    "Đường path đích hiện ở cột \"Lưu vào\".",
            EditorStyles.miniLabel);

        DrawToolbar();
        EditorGUILayout.Space(2);
        DrawHeader();
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
            if (GUILayout.Button("Quét ảnh trong project", EditorStyles.toolbarButton, GUILayout.Width(150)))
                ScanProjectImages();
            if (GUILayout.Button("Nạp từ folder…", EditorStyles.toolbarButton, GUILayout.Width(110)))
                LoadFromFolder();
            if (GUILayout.Button("Thêm dòng", EditorStyles.toolbarButton, GUILayout.Width(80)))
            {
                entries.Add(new Entry());
                return;
            }
            if (GUILayout.Button("Lưu bảng", EditorStyles.toolbarButton, GUILayout.Width(70))) SaveMap();
            if (GUILayout.Button("Nạp bảng", EditorStyles.toolbarButton, GUILayout.Width(70))) LoadMap();

            GUILayout.FlexibleSpace();
            mirrorBooster = GUILayout.Toggle(mirrorBooster, "Mirror BoosterUI", EditorStyles.toolbarButton, GUILayout.Width(110));
            showDestColumn = GUILayout.Toggle(showDestColumn, "Cột đường path", EditorStyles.toolbarButton, GUILayout.Width(100));
            GUILayout.Label(entries.Count + " dòng", EditorStyles.miniLabel);
        }
    }

    private void DrawHeader()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            GUILayout.Label(" ", GUILayout.Width(20));
            GUILayout.Label("ẢNH CŨ (asset trong project)", GUILayout.Width(230));
            GUILayout.Label("ẢNH MỚI (kéo file / bấm …)", GUILayout.Width(230));
            if (showDestColumn) GUILayout.Label("LƯU VÀO (đường path đích)", GUILayout.Width(300));
            GUILayout.Label("CẢNH BÁO / DÙNG Ở", GUILayout.ExpandWidth(true));
        }
        using (new EditorGUILayout.HorizontalScope())
        {
            GUILayout.Label(" ", GUILayout.Width(20));
            filter = GUILayout.TextField(filter, GUILayout.Width(230));
            GUILayout.Space(232);
            if (showDestColumn) GUILayout.Space(302);
        }
    }

    private void DrawRows()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        for (int i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            if (!string.IsNullOrEmpty(filter) &&
                !MatchFilter(e, filter)) continue;

            using (new EditorGUILayout.HorizontalScope())
            {
                e.enabled = EditorGUILayout.Toggle(e.enabled, GUILayout.Width(18));

                // ---- ảnh cũ: ObjectField + preview
                DrawOldCell(e);

                // ---- ảnh mới: ObjectField + browse + preview
                DrawNewCell(e);

                // ---- đường path đích
                if (showDestColumn)
                {
                    string dest = e.destPath ?? "";
                    string newDest = EditorGUILayout.TextField(dest, GUILayout.Width(300));
                    if (newDest != dest) e.destPath = newDest;
                }

                // ---- cảnh báo + nơi dùng
                EditorGUILayout.LabelField(BuildInfo(e), EditorStyles.miniLabel);
            }
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawOldCell(Entry e)
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(230)))
        {
            var tex = e.oldPath != null ? AssetDatabase.LoadAssetAtPath<Texture2D>(e.oldPath) : null;
            var picked = EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.Width(210));
            var pickedPath = picked != null ? AssetDatabase.GetAssetPath(picked) : null;
            if (pickedPath != e.oldPath)
            {
                e.oldPath = pickedPath;
                if (string.IsNullOrEmpty(e.destPath)) e.destPath = pickedPath;
                usageCache.Remove(e.oldPath);
            }
            if (tex != null)
            {
                var rect = GUILayoutUtility.GetRect(40, 40, GUILayout.Width(40));
                GUI.DrawTexture(rect, GetOldPreview(e.oldPath), ScaleMode.ScaleToFit);
            }
            if (!string.IsNullOrEmpty(e.oldPath))
                EditorGUILayout.LabelField(e.oldPath, EditorStyles.miniLabel);
        }
    }

    private void DrawNewCell(Entry e)
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.Width(230)))
        {
            // 1) kéo Texture2D trong project vào ô này
            var rectObj = GUILayoutUtility.GetRect(210, 16, GUILayout.Width(210));
            HandleDragDrop(e, rectObj);

            var tex = e.newPath != null && e.newPath.StartsWith("Assets/")
                ? AssetDatabase.LoadAssetAtPath<Texture2D>(e.newPath)
                : null;
            var picked = EditorGUILayout.ObjectField(tex, typeof(Texture2D), false, GUILayout.Width(165));
            if (picked != null)
            {
                string p = AssetDatabase.GetAssetPath(picked);
                if (p != e.newPath) e.newPath = p;
            }
            else if (e.newPath != null && e.newPath.StartsWith("Assets/"))
            {
                e.newPath = null;
            }

            if (GUILayout.Button("…", GUILayout.Width(24)))
            {
                var file = EditorUtility.OpenFilePanel("Chọn ảnh mới", string.Empty, "");
                if (!string.IsNullOrEmpty(file)) e.newPath = file;
            }

            if (!string.IsNullOrEmpty(e.newPath))
            {
                var rect = GUILayoutUtility.GetRect(40, 40, GUILayout.Width(40));
                var preview = GetPreview(e.newPath);
                if (preview != null) GUI.DrawTexture(rect, preview, ScaleMode.ScaleToFit);
                EditorGUILayout.LabelField(ShortName(e.newPath), EditorStyles.miniLabel);
            }
        }
    }

    private void HandleDragDrop(Entry e, Rect rect)
    {
        var evt = Event.current;
        if (!rect.Contains(evt.mousePosition)) return;

        if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
        {
            DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            if (evt.type == EventType.DragPerform)
            {
                DragAndDrop.AcceptDrag();
                // ưu tiên file ngoài project; nếu kéo Texture2D trong project thì lấy asset path
                if (DragAndDrop.paths != null && DragAndDrop.paths.Length > 0)
                    e.newPath = DragAndDrop.paths[0];
                else if (DragAndDrop.objectReferences != null && DragAndDrop.objectReferences.Length > 0)
                    e.newPath = AssetDatabase.GetAssetPath(DragAndDrop.objectReferences[0]);
                evt.Use();
            }
            evt.Use();
        }
    }

    private void DrawFooter()
    {
        using (new EditorGUILayout.HorizontalScope())
        {
            int ready = entries.Count(e => e.enabled && !string.IsNullOrEmpty(e.newPath) && !string.IsNullOrEmpty(e.destPath));
            GUI.enabled = ready > 0;
            if (GUILayout.Button("Áp dụng (" + ready + ")", GUILayout.Height(28))) Apply();
            GUI.enabled = true;

            if (GUILayout.Button("Hoàn tác từ backup", GUILayout.Width(150), GUILayout.Height(28))) Restore();
            if (GUILayout.Button("Lưu bảng", GUILayout.Width(90), GUILayout.Height(28))) SaveMap();
        }
    }

    // ------------------------------------------------------------------ helpers
    private bool MatchFilter(Entry e, string f)
    {
        f = f.ToLowerInvariant();
        return (e.oldPath ?? "").ToLowerInvariant().Contains(f)
            || (e.newPath ?? "").ToLowerInvariant().Contains(f)
            || (e.destPath ?? "").ToLowerInvariant().Contains(f);
    }

    private static string ShortName(string path)
    {
        if (string.IsNullOrEmpty(path)) return "";
        return Path.GetFileName(path);
    }

    private string BuildInfo(Entry e)
    {
        var parts = new List<string>();

        // nơi ảnh cũ đang dùng
        if (!string.IsNullOrEmpty(e.oldPath))
        {
            string usage;
            if (!usageCache.TryGetValue(e.oldPath, out usage))
            {
                var deps = AssetDatabase.GetDependencies(e.oldPath, true)
                    .Where(d => d != e.oldPath && !d.EndsWith(".cs"))
                    .Take(3).ToArray();
                usage = deps.Length == 0 ? "(khong asset nao dung)" : string.Join(", ", deps);
                usageCache[e.oldPath] = usage;
            }
            parts.Add("Dung o: " + usage);
        }

        // cảnh báo
        if (!string.IsNullOrEmpty(e.newPath) && !string.IsNullOrEmpty(e.destPath))
        {
            string destExt = Path.GetExtension(e.destPath).ToLower();
            string newExt = Path.GetExtension(e.newPath).ToLower();
            if (destExt != newExt)
                parts.Add("KHAC DINH DANG " + destExt + " vs " + newExt + " -> se bo qua");
            else
            {
                var newSize = ReadPngSize(e.newPath);
                var oldTex = !string.IsNullOrEmpty(e.oldPath) ? AssetDatabase.LoadAssetAtPath<Texture2D>(e.oldPath) : null;
                if (newSize.x > 0 && oldTex != null && newSize != new Vector2Int(oldTex.width, oldTex.height))
                {
                    var b = ReadBorder(e.destPath);
                    if (b != Vector4.zero)
                        parts.Add("9-slice " + b + " nhung kich thuoc " +
                                 new Vector2Int(oldTex.width, oldTex.height) + " -> " + newSize);
                    else
                        parts.Add("Kich thuoc " + new Vector2Int(oldTex.width, oldTex.height) + " -> " + newSize);
                }
            }
        }

        if (string.IsNullOrEmpty(e.newPath)) parts.Add("(chua gan anh moi)");
        return string.Join("  |  ", parts);
    }

    // ------------------------------------------------------------------ load
    private void ScanProjectImages()
    {
        var list = new List<string>();
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (!p.StartsWith("Assets/", StringComparison.Ordinal)) continue;
            if (!ImageExts.Contains(Path.GetExtension(p).ToLower())) continue;
            list.Add(p);
        }
        list.Sort(StringComparer.OrdinalIgnoreCase);

        int added = 0;
        foreach (var p in list)
        {
            if (entries.Any(e => e.oldPath == p)) continue;
            entries.Add(new Entry { oldPath = p, destPath = p });
            added++;
        }
        report = "Da them " + added + " anh vao bang (" + list.Count + " anh trong project).";
    }

    private void LoadFromFolder()
    {
        var folder = EditorUtility.OpenFolderPanel("Folder chua anh moi", string.Empty, "");
        if (string.IsNullOrEmpty(folder)) return;

        // index ảnh trong project theo tên
        var byName = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (!p.StartsWith("Assets/", StringComparison.Ordinal)) continue;
            if (!ImageExts.Contains(Path.GetExtension(p).ToLower())) continue;
            string n = Path.GetFileName(p);
            if (!byName.TryGetValue(n, out var l)) byName[n] = l = new List<string>();
            l.Add(p);
        }

        int added = 0, miss = 0;
        foreach (var file in Directory.GetFiles(folder, "*.*", SearchOption.AllDirectories).OrderBy(f => f))
        {
            if (!ImageExts.Contains(Path.GetExtension(file).ToLower())) continue;
            var cands = byName.TryGetValue(Path.GetFileName(file), out var l) ? l : null;
            if (cands == null || cands.Count == 0) { miss++; continue; }
            var editable = cands.Where(c => !c.Contains("/Resources/")).ToList();
            var target = (editable.Count > 0 ? editable : cands).OrderBy(c => c, StringComparer.Ordinal).First();
            if (entries.Any(e => e.newPath == file)) continue;
            entries.Add(new Entry { oldPath = target, newPath = file, destPath = target });
            added++;
        }
        report = "Da them " + added + " dong tu folder" + (miss > 0 ? ", " + miss + " anh khong khop ten" : "") + ".";
    }

    private void SaveMap()
    {
        var map = new MapFile { entries = entries };
        string json = JsonUtility.ToJson(map, true);
        var dir = Path.GetDirectoryName(MapPath);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(ProjectRoot, MapPath.Replace('/', Path.DirectorySeparatorChar)), json);
        AssetDatabase.ImportAsset(MapPath, ImportAssetOptions.ForceUpdate);
        report = "Da luu bang: " + MapPath;
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
            report = "Da nap bang: " + entries.Count + " dong.";
        }
        catch (Exception e)
        {
            report = "Loi doc bang: " + e.Message;
        }
    }

    private static string ProjectRoot =>
        Directory.GetParent(Application.dataPath).FullName;

    // ------------------------------------------------------------------ apply
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
                problems.Add((e.newPath ?? "") + ": khac dinh dang (" + destExt + " vs " + newExt + ")");
                continue;
            }

            string destFull = Path.Combine(ProjectRoot, e.destPath.Replace('/', Path.DirectorySeparatorChar));
            string newFull = e.newPath.StartsWith("Assets/")
                ? Path.Combine(ProjectRoot, e.newPath.Replace('/', Path.DirectorySeparatorChar))
                : e.newPath;

            try
            {
                // backup ban goc
                if (File.Exists(destFull))
                {
                    string bak = Path.Combine(backupRoot, e.destPath.Replace('/', Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(bak));
                    File.Copy(destFull, bak, true);
                }

                Directory.CreateDirectory(Path.GetDirectoryName(destFull));
                File.WriteAllBytes(destFull, File.ReadAllBytes(newFull));

                // neu viet ra file moi (dest khac old) va co .meta cu -> giu lai .meta
                if (!string.IsNullOrEmpty(e.oldPath) && e.oldPath != e.destPath)
                {
                    string oldMeta = Path.Combine(ProjectRoot, e.oldPath.Replace('/', Path.DirectorySeparatorChar)) + ".meta";
                    string newMeta = destFull + ".meta";
                    if (File.Exists(oldMeta) && !File.Exists(newMeta))
                        File.Copy(oldMeta, newMeta, true);
                }

                if (e.destPath.StartsWith("Assets/"))
                    AssetDatabase.ImportAsset(e.destPath, ImportAssetOptions.ForceUpdate);

                // mirror sang Resources/BoosterUI
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
                problems.Add((e.newPath ?? "") + ": " + ex.Message);
            }
        }

        AssetDatabase.Refresh();
        report = "Da ghi " + done + " anh, bo qua " + skip + ".";
        if (problems.Count > 0) report += "\n" + string.Join("\n", problems);
    }

    private void Restore()
    {
        string backupRoot = Path.Combine(ProjectRoot, ".reskin_backup");
        if (!Directory.Exists(backupRoot)) { report = "Chua co thu muc backup."; return; }
        var latest = new DirectoryInfo(backupRoot).GetDirectories().OrderByDescending(d => d.Name).FirstOrDefault();
        if (latest == null) { report = "Chua co ban backup nao."; return; }

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
        report = "Da khoi phuc " + n + " anh tu backup " + latest.Name + ".";
    }

    // ------------------------------------------------------------------ preview
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
        const int max = 40;
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
