using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Reskin hàng loạt: ghi đè ảnh mới vào đúng asset cũ, GIU NGUYEN .meta
/// nên GUID không đổi (prefab/scene/material vẫn trỏ đúng) và spriteBorder
/// (9-slice) + PPU không bị mất.
///
/// Khớp file theo thứ tự:
///   1. đường dẫn tương đối (folder ảnh mới cấu trúc y hệt project)
///   2. tên file — nếu trùng nhiều nơi thì chọn bản không nằm trong Resources/
///      (bản editable) và có thể chọn tay bằng dropdown
///
/// Mở: Tools / Reskin / Reskin hàng loạt
/// </summary>
public sealed class BulkReskinWindow : EditorWindow
{
    private const string MenuPath = "Tools/Reskin/Reskin hàng loạt";
    private static readonly string[] ImageExts = { ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tif", ".tiff", ".webp" };

    private string sourceFolder;
    private Vector2 scroll;
    private bool replaceAllSameName;
    private bool mirrorBooster;
    private string report = "";
    private bool showOnlyProblems;

    private readonly List<Row> rows = new List<Row>();
    private readonly Dictionary<string, Texture2D> previewCache = new Dictionary<string, Texture2D>();
    private readonly Dictionary<string, Texture2D> oldPreviewCache = new Dictionary<string, Texture2D>();

    private sealed class Row
    {
        public string newFull;      // đường dẫn tuyệt đối của ảnh mới
        public string newRel;       // đường dẫn tương đối so với folder nguồn
        public string[] candidates; // các asset trong project trùng tên
        public int targetIndex;
        public bool selected = true;
        public string status = "";
        public string warn = "";
        public Vector2Int newSize;
        public Vector2Int oldSize;
        public bool hasBorder;
        public Vector4 border;

        public string Target => candidates != null && targetIndex >= 0 && targetIndex < candidates.Length
            ? candidates[targetIndex] : null;
    }

    [MenuItem(MenuPath, false, 40)]
    private static void Open()
    {
        var w = GetWindow<BulkReskinWindow>("Reskin hàng loạt");
        w.minSize = new Vector2(900, 480);
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(sourceFolder))
            sourceFolder = string.Empty;
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
        EditorGUILayout.LabelField("Reskin hàng loạt — ghi đè ảnh, giữ nguyên .meta (GUID / 9-slice / PPU)",
            EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            EditorGUILayout.PrefixLabel("Folder ảnh mới");
            EditorGUILayout.SelectableLabel(sourceFolder, EditorStyles.textField, GUILayout.Height(18));
            if (GUILayout.Button("Chọn…", GUILayout.Width(70)))
            {
                var picked = EditorUtility.OpenFolderPanel("Chọn folder chứa ảnh mới", string.IsNullOrEmpty(sourceFolder) ? "" : sourceFolder, "");
                if (!string.IsNullOrEmpty(picked))
                {
                    sourceFolder = picked;
                    Scan();
                }
            }
            if (GUILayout.Button("Quét lại", GUILayout.Width(80))) Scan();
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            replaceAllSameName = EditorGUILayout.ToggleLeft(
                "Thay cả các bản trùng tên (mặc định: chỉ bản editable)", replaceAllSameName);
            mirrorBooster = EditorGUILayout.ToggleLeft(
                "Copy thêm sang Resources/BoosterUI (bản runtime fallback)", mirrorBooster);
            showOnlyProblems = EditorGUILayout.ToggleLeft("Chỉ hiện dòng có vấn đề", showOnlyProblems, GUILayout.Width(170));
        }

        EditorGUILayout.Space(2);
        DrawToolbar();
        EditorGUILayout.Space(2);
        DrawRows();

        EditorGUILayout.Space(6);
        if (!string.IsNullOrEmpty(report))
            EditorGUILayout.HelpBox(report, MessageType.Info);

        EditorGUILayout.Space(4);
        using (new EditorGUILayout.HorizontalScope())
        {
            int selected = rows.Count(r => r.selected && r.Target != null);
            GUI.enabled = selected > 0;
            if (GUILayout.Button("Áp dụng (" + selected + ")", GUILayout.Height(28)))
                Apply();
            GUI.enabled = true;

            if (GUILayout.Button("Hoàn tác từ backup", GUILayout.Width(150), GUILayout.Height(28)))
                Restore();
        }
    }

    private void DrawToolbar()
    {
        using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar))
        {
            if (GUILayout.Button("Chọn tất cả", EditorStyles.toolbarButton, GUILayout.Width(80)))
                foreach (var r in rows) r.selected = true;
            if (GUILayout.Button("Bỏ chọn tất cả", EditorStyles.toolbarButton, GUILayout.Width(95)))
                foreach (var r in rows) r.selected = false;

            GUILayout.FlexibleSpace();
            GUILayout.Label(rows.Count + " ảnh", EditorStyles.miniLabel);
        }
    }

    private void DrawRows()
    {
        if (rows.Count == 0)
        {
            EditorGUILayout.HelpBox("Chọn folder chứa ảnh mới rồi bấm \"Quét lại\".", MessageType.Info);
            return;
        }

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label(" ", GUILayout.Width(22));
        GUILayout.Label("Ảnh mới", GUILayout.Width(190));
        GUILayout.Label("Cũ", GUILayout.Width(52));
        GUILayout.Label("Ảnh trong project", GUILayout.Width(320));
        GUILayout.Label("Cảnh báo", GUILayout.ExpandWidth(true));
        EditorGUILayout.EndHorizontal();

        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            if (showOnlyProblems && string.IsNullOrEmpty(r.warn) && r.Target != null) continue;

            EditorGUILayout.BeginHorizontal();
            r.selected = EditorGUILayout.Toggle(r.selected, GUILayout.Width(20));

            // preview ảnh mới
            var tex = GetPreview(r.newFull);
            if (tex != null)
            {
                var rect = GUILayoutUtility.GetRect(46, 46, GUILayout.Width(46));
                GUI.DrawTexture(rect, tex, ScaleMode.ScaleToFit);
            }
            else GUILayout.Space(46);

            EditorGUILayout.LabelField(r.newRel, GUILayout.Width(190));

            // preview ảnh cũ
            var oldTex = r.Target != null ? GetOldPreview(r.Target) : null;
            if (oldTex != null)
            {
                var rect = GUILayoutUtility.GetRect(46, 46, GUILayout.Width(46));
                GUI.DrawTexture(rect, oldTex, ScaleMode.ScaleToFit);
            }
            else GUILayout.Space(46);

            // chọn đích
            if (r.candidates != null && r.candidates.Length > 0)
            {
                var names = r.candidates.Select(c => c.Replace("Assets/", "")).ToArray();
                int newIdx = EditorGUILayout.Popup(r.targetIndex, names, GUILayout.Width(320));
                if (newIdx != r.targetIndex)
                {
                    r.targetIndex = newIdx;
                    r.warn = RecalcWarn(r);
                }
            }
            else
            {
                EditorGUILayout.LabelField(r.status, GUILayout.Width(320));
            }

            EditorGUILayout.LabelField(r.warn, EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    // ------------------------------------------------------------------ scan
    private void Scan()
    {
        rows.Clear();
        report = "";
        if (string.IsNullOrEmpty(sourceFolder) || !Directory.Exists(sourceFolder))
        {
            report = "Chưa chọn folder ảnh mới.";
            return;
        }

        // index mọi ảnh trong project
        var allImages = new List<string>();
        foreach (var p in AssetDatabase.GetAllAssetPaths())
        {
            if (p.StartsWith("Assets/", StringComparison.Ordinal) && ImageExts.Contains(Path.GetExtension(p).ToLower()))
                allImages.Add(p);
        }
        var byName = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var p in allImages)
        {
            string n = Path.GetFileName(p);
            if (!byName.TryGetValue(n, out var list)) byName[n] = list = new List<string>();
            list.Add(p);
        }

        foreach (var file in Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                                    .OrderBy(f => f))
        {
            if (!ImageExts.Contains(Path.GetExtension(file).ToLower())) continue;
            if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;

            var rel = file.Substring(sourceFolder.Length).TrimStart('\\', '/').Replace('\\', '/');
            var row = new Row { newFull = file, newRel = rel };
            row.newSize = ReadPngSize(file);

            // 1) khớp đường dẫn tương đối
            string exact = "Assets/" + rel;
            if (allImages.Contains(exact))
            {
                row.candidates = new[] { exact };
                row.targetIndex = 0;
            }
            // 2) khớp tên file
            else
            {
                var cands = byName.TryGetValue(Path.GetFileName(file), out var l) ? l : null;
                if (cands == null || cands.Count == 0)
                {
                    row.status = "Không có ảnh nào trùng tên trong project";
                    row.selected = false;
                }
                else
                {
                    var editable = cands.Where(c => !c.Contains("/Resources/")).ToList();
                    var ordered = (editable.Count > 0 ? editable : cands)
                        .OrderBy(c => c, StringComparer.Ordinal).ToList();
                    if (replaceAllSameName) ordered = cands.OrderBy(c => c, StringComparer.Ordinal).ToList();
                    row.candidates = ordered.ToArray();
                    row.targetIndex = 0;
                    if (ordered.Count > 1) row.status = ordered.Count + " bản trùng tên — chọn ở dropdown";
                }
            }

            row.warn = RecalcWarn(row);
            rows.Add(row);
        }

        report = "Quét xong: " + rows.Count + " ảnh.";
    }

    private string RecalcWarn(Row r)
    {
        if (r.Target == null) return r.status;

        var oldTex = AssetDatabase.LoadAssetAtPath<Texture2D>(r.Target);
        r.oldSize = oldTex != null ? new Vector2Int(oldTex.width, oldTex.height) : Vector2Int.zero;

        r.border = ReadBorder(r.Target);
        r.hasBorder = r.border != Vector4.zero;

        string targetExt = Path.GetExtension(r.Target).ToLower();
        string newExt = Path.GetExtension(r.newFull).ToLower();
        if (targetExt != newExt)
            return "KHAC DINH DANG: " + targetExt + " -> " + newExt + " — se bi bo qua, hay doi file cho giong";

        if (r.newSize.x > 0 && r.oldSize.x > 0 && r.newSize != r.oldSize)
        {
            if (r.hasBorder)
                return "9-slice " + r.border + " nhung kich thuoc " + r.oldSize + " -> " + r.newSize +
                       " (spriteBorder theo pixel cu)";
            return "Kich thuoc " + r.oldSize + " -> " + r.newSize;
        }
        return "";
    }

    // ------------------------------------------------------------------ apply
    private void Apply()
    {
        int done = 0, skip = 0;
        var problems = new List<string>();

        foreach (var r in rows)
        {
            if (!r.selected || r.Target == null) continue;

            string targetExt = Path.GetExtension(r.Target).ToLower();
            string newExt = Path.GetExtension(r.newFull).ToLower();
            if (targetExt != newExt)
            {
                skip++;
                problems.Add(r.newRel + ": khac dinh dang (" + targetExt + " vs " + newExt + ")");
                continue;
            }

            string full = Path.Combine(Directory.GetParent(Application.dataPath).FullName,
                                       r.Target.Replace('/', Path.DirectorySeparatorChar));
            try
            {
                // backup ban goc
                var root = Directory.GetParent(Application.dataPath).FullName;
                var stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var bakDir = Path.Combine(root, ".reskin_backup", stamp);
                var bakFile = Path.Combine(bakDir, r.Target.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(bakFile));
                File.Copy(full, bakFile, true);

                // ghi de noi dung — GIU NGUYEN .meta
                File.WriteAllBytes(full, File.ReadAllBytes(r.newFull));
                AssetDatabase.ImportAsset(r.Target, ImportAssetOptions.ForceUpdate);

                // mirror sang Resources/BoosterUI
                if (mirrorBooster && r.Target.StartsWith("Assets/_Game/BoosterUI_Picked/", StringComparison.Ordinal))
                {
                    string name = Path.GetFileName(r.Target);
                    string mirror = "Assets/_Game/Resources/BoosterUI/" + name;
                    if (File.Exists(mirror))
                    {
                        string mirrorFull = Path.Combine(root, mirror.Replace('/', Path.DirectorySeparatorChar));
                        File.WriteAllBytes(mirrorFull, File.ReadAllBytes(r.newFull));
                        AssetDatabase.ImportAsset(mirror, ImportAssetOptions.ForceUpdate);
                    }
                }

                done++;
            }
            catch (Exception e)
            {
                skip++;
                problems.Add(r.newRel + ": " + e.Message);
            }
        }

        // doc lai border + size sau khi ghi de
        foreach (var r in rows)
        {
            if (r.Target == null) continue;
            r.hasBorder = false;
            r.border = Vector4.zero;
            r.warn = RecalcWarn(r);
        }

        AssetDatabase.Refresh();
        report = "Da thay " + done + " anh, bo qua " + skip + ".";
        if (problems.Count > 0) report += "\n" + string.Join("\n", problems);
    }

    private void Restore()
    {
        var root = Directory.GetParent(Application.dataPath).FullName;
        var backupRoot = Path.Combine(root, ".reskin_backup");
        if (!Directory.Exists(backupRoot))
        {
            report = "Chua co thu muc backup (.reskin_backup).";
            return;
        }
        var latest = new DirectoryInfo(backupRoot).GetDirectories()
                            .OrderByDescending(d => d.Name).FirstOrDefault();
        if (latest == null)
        {
            report = "Chua co ban backup nao.";
            return;
        }

        int n = 0;
        foreach (var f in latest.GetFiles("*.*", SearchOption.AllDirectories))
        {
            if (!ImageExts.Contains(f.Extension.ToLower())) continue;
            string rel = f.FullName.Substring(latest.FullName.Length).TrimStart('\\', '/').Replace('\\', '/');
            string dest = Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar));
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
            var bytes = File.ReadAllBytes(path);
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
        const int max = 64;
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
            using (var fs = File.OpenRead(path))
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
        string meta = assetPath + ".meta";
        if (!File.Exists(meta)) return Vector4.zero;
        try
        {
            var lines = File.ReadAllLines(meta);
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Contains("spriteBorder:")) continue;
                var s = lines[i].Substring(lines[i].IndexOf("spriteBorder:") + 13);
                s = s.Replace("{", "").Replace("}", "").Replace("x:", "").Replace("y:", "")
                     .Replace("z:", "").Replace("w:", "").Replace(" ", "");
                var parts = s.Split(',');
                if (parts.Length != 4) return Vector4.zero;
                return new Vector4(
                    float.Parse(parts[0]), float.Parse(parts[1]),
                    float.Parse(parts[2]), float.Parse(parts[3]));
            }
        }
        catch { }
        return Vector4.zero;
    }
}
