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
/// Validate trước runtime luồng "gom 3 sợi -> tô ảnh" (project wool-sort):
/// - Database có đủ material yarn/basket/slot cho mọi màu dùng trong level?
/// - Holder count có khớp part thật (tránh stale sau khi sửa prefab)?
/// - Tổng số sợi có chia hết cho 3 (1 box = 3 sợi)?
/// - Ảnh Resources/Images/{Level} có tồn tại + chia đủ ô cho mọi màu?
/// Mở window: Tools / Wool Sort / Validate Box Image Colors
/// Quét nhanh: Tools / Wool Sort / Scan All Levels (Box Image Colors)
/// </summary>
public sealed class BoxImageColorValidator : EditorWindow
{
    private const string DatabasePath = "Assets/_Game/Configs/YarnColorDatabase.asset";
    private const string LevelsFolder = "Assets/Resources/Levels";

    private Vector2 scroll;
    private string report = "Bấm \"Quét full\" để kiểm tra. Window tự quét nhanh Database lúc mở.";
    private int okLevelCount;
    private int errorLevelCount;

    [MenuItem("Tools/Wool Sort/Validate Box Image Colors", false, 10)]
    private static void Open()
    {
        var w = GetWindow<BoxImageColorValidator>("Box Image Colors");
        w.RefreshAll();
    }

    [MenuItem("Tools/Wool Sort/Scan All Levels (Box Image Colors)", false, 11)]
    private static void ScanAllMenu()
    {
        string result = ScanAllLevels(out _, out _);
        foreach (var line in result.Split('\n'))
        {
            if (line.StartsWith("[LỖI]") || line.StartsWith("  [LỖI]"))
                Debug.LogError("[BoxImageColor] " + line);
        }
        Debug.Log("[BoxImageColor] Quét xong.\n" + result);
        EditorUtility.DisplayDialog("Scan Box Image Colors",
            string.Join("\n", result.Split('\n').Take(12)) + "\n\nXem Console để click tới từng asset lỗi.", "OK");
    }

    private void OnEnable()
    {
        // Tự quét nhẹ lúc mở window để thấy ngay database có thiếu không (đúng yêu cầu check khi mở).
        try
        {
            var sb = new StringBuilder();
            int dbErrors = CheckDatabase(sb);
            sb.AppendLine(dbErrors == 0
                ? "[OK] Database đủ material yarn/basket/slot."
                : $"[LỖI] Database thiếu {dbErrors} material — bấm Quét full để xem chi tiết.");
            sb.AppendLine("Bấm \"Quét full\" để kiểm tra từng level.");
            report = sb.ToString();
        }
        catch (Exception ex)
        {
            report = "Lỗi lúc tự quét: " + ex.Message;
        }
    }

    private void OnGUI()
    {
        GUILayout.Label("Validate: gom 3 sợi -> tô ảnh đúng màu/đúng ô", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Kiểm tra trước khi Play:\n" +
            "1. Database đủ material cho mọi màu dùng trong level.\n" +
            "2. Holder count khớp part thật (tránh stale sau khi sửa prefab).\n" +
            "3. Tổng số sợi chia hết cho 3 (1 box = 3 sợi).\n" +
            "4. Ảnh tồn tại + chia đủ ô cho mọi màu (tránh bắn lên trắng/sai ô).",
            MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Quét full", GUILayout.Height(32))) RefreshAll();
        if (GUILayout.Button("Chỉ quét Database", GUILayout.Height(32)))
        {
            var sb = new StringBuilder();
            int e = CheckDatabase(sb);
            report = sb.ToString() + (e == 0 ? "\n[OK] Database." : $"\n[LỖI] {e} material thiếu.");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Tự sửa tổng %3 (thêm lớp Fixed cùng màu)", GUILayout.Height(32)))
        {
            if (EditorUtility.DisplayDialog("Tự sửa tổng %3?",
                "Với mỗi level có tổng số sợi không chia hết cho 3, tool sẽ thêm 1-2 lớp inner Fixed cùng màu vào 1 part, rồi lưu prefab.\nTiếp tục?",
                "Sửa luôn", "Hủy"))
                AutoFixDiv3();
        }

        EditorGUILayout.LabelField($"Level OK: {okLevelCount}   Level lỗi: {errorLevelCount}");
        scroll = EditorGUILayout.BeginScrollView(scroll);
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void RefreshAll()
    {
        report = ScanAllLevels(out okLevelCount, out errorLevelCount);
    }

    // ---------------- Core scan (dùng chung cho window + menu) ----------------

    private static string ScanAllLevels(out int okCount, out int errCount)
    {
        var sb = new StringBuilder();
        okCount = 0;
        errCount = 0;

        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        if (db == null)
        {
            sb.AppendLine($"[LỖI] Không load được Database: {DatabasePath}");
            return sb.ToString();
        }

        int dbErrors = CheckDatabase(sb);
        if (dbErrors > 0) sb.AppendLine($"[LỖI] Database thiếu {dbErrors} material (xem chi tiết trên).");

        var levelPaths = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p)
            .ToList();
        sb.AppendLine($"Quét {levelPaths.Count} level trong {LevelsFolder}...");

        foreach (var path in levelPaths)
        {
            var errs = ScanOneLevel(path, db);
            if (errs.Count == 0) okCount++;
            else
            {
                errCount++;
                sb.AppendLine($"[LỖI] {Path.GetFileNameWithoutExtension(path)} ({errs.Count}):");
                foreach (var e in errs) sb.AppendLine("  [LỖI] " + e);
            }
        }

        sb.AppendLine($"Xong: {okCount} OK, {errCount} lỗi / {levelPaths.Count} level.");
        return sb.ToString();
    }

    private static int CheckDatabase(StringBuilder sb)
    {
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        if (db == null)
        {
            sb.AppendLine($"[LỖI] Không thấy Database: {DatabasePath}");
            return 999;
        }
        int missing = 0;
        missing += CheckPairs(sb, "yarn", db.colorMaterialPairs, false);
        missing += CheckPairs(sb, "basket", db.basketColorMaterialPairs, true);
        missing += CheckPairs(sb, "slot", db.slotStringColorMaterialPairs, false);
        if (db.transparentMaterial == null)
        {
            sb.AppendLine("[LỖI] Database.transparentMaterial đang null.");
            missing++;
        }
        return missing;
    }

    private static int CheckPairs(StringBuilder sb, string title, ColorMaterialPair[] pairs, bool requireRainbow)
    {
        var map = new Dictionary<YarnColor, Material>();
        if (pairs != null)
            foreach (var p in pairs)
                if (p.material != null) map[p.color] = p.material;

        int missing = 0;
        foreach (YarnColor c in Enum.GetValues(typeof(YarnColor)))
        {
            if (c == YarnColor.None) continue;
            if (c == YarnColor.Rainbow && !requireRainbow) continue;
            if (!map.TryGetValue(c, out var mat) || mat == null)
            {
                sb.AppendLine($"[LỖI] Database thiếu {title} material cho màu {c} ({(int)c}).");
                missing++;
            }
        }
        return missing;
    }

    private static List<string> ScanOneLevel(string prefabPath, YarnColorDatabase db)
    {
        var errs = new List<string>();
        string levelName = Path.GetFileNameWithoutExtension(prefabPath);

        var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (go == null) return new List<string> { $"Không load được prefab: {prefabPath}" };

        var holder = go.GetComponent<LevelYarnInfoHolder>();
        if (holder == null) holder = go.GetComponentInChildren<LevelYarnInfoHolder>(true);
        if (holder == null) errs.Add("Thiếu LevelYarnInfoHolder.");

        var parts = go.GetComponentsInChildren<YarnPart>(true);
        if (parts.Length == 0) errs.Add("Không có YarnPart nào.");

        // Đếm lại màu thật: outer initColor + fixed-inner (an toàn khi list ngắn) + random entries
        var real = new Dictionary<YarnColor, int>();
        foreach (var p in parts)
        {
            AddCount(real, p.initColor, 1);
            if (p.totalInnerCount > 0)
            {
                int types = p.innerColorTypes != null ? p.innerColorTypes.Count : 0;
                int fixeds = p.innerFixedColors != null ? p.innerFixedColors.Count : 0;
                if (types < p.totalInnerCount || fixeds < p.totalInnerCount)
                    errs.Add($"Part '{p.name}': totalInnerCount={p.totalInnerCount} nhưng list mới {types}/{fixeds} — holder sẽ đếm thiếu, bắn ảnh sai.");
                int limit = Math.Min(p.totalInnerCount, Math.Min(types, fixeds));
                for (int j = 0; j < limit; j++)
                    if (p.innerColorTypes[j] == InnerColorType.Fixed)
                        AddCount(real, p.innerFixedColors[j], 1);
            }
        }
        if (holder != null && holder.randomColorEntries != null)
            foreach (var e in holder.randomColorEntries)
                AddCount(real, e.color, e.count);

        int total = real.Values.Sum();
        if (total == 0) errs.Add("Tổng số sợi = 0.");
        else if (total % 3 != 0)
            errs.Add($"Tổng {total} không chia hết cho 3 (1 box = 3 sợi) — box cuối lẻ, ảnh thiếu/thừa ô.");

        // Holder có stale không
        if (holder != null)
        {
            var held = (holder.colorCountEntries ?? new List<ColorCountEntry>())
                .ToDictionary(e => e.color, e => e.count);
            foreach (var kv in real)
                if (!held.TryGetValue(kv.Key, out var h) || h != kv.Value)
                    errs.Add($"Holder stale màu {kv.Key}: prefab thật {kv.Value} nhưng holder ghi {(held.ContainsKey(kv.Key) ? held[kv.Key].ToString() : "0")} — mở prefab cho OnValidate chạy rồi lưu.");
            foreach (var kv in held)
                if (!real.ContainsKey(kv.Key))
                    errs.Add($"Holder thừa màu {kv.Key}:{kv.Value} không còn trong prefab.");
            int expectTotal = parts.Sum(p => 1 + p.totalInnerCount);
            if (holder.totalYarnPartCount != expectTotal)
                errs.Add($"Holder.totalYarnPartCount={holder.totalYarnPartCount} nhưng đếm thật {expectTotal}.");
        }

        // Material từng màu dùng trong level
        foreach (var kv in real)
        {
            if (db.GetMaterial(kv.Key) == null)
                errs.Add($"Màu {kv.Key} ({kv.Value} sợi) thiếu yarn material.");
            if (db.GetMaterialBasket(kv.Key) == null)
                errs.Add($"Màu {kv.Key} ({kv.Value} sợi) thiếu basket material — rổ sai màu.");
            if (db.GetMaterialSlotString(kv.Key) == null)
                errs.Add($"Màu {kv.Key} ({kv.Value} sợi) thiếu slot material — vòng/dây sai màu.");
        }

        // Ảnh: GameManager load Resources/Images/{LevelN} (có fallback Level_ / LevelImages)
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.jpg");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/Level_{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/LevelImages/{levelName}.png");
        if (tex == null)
        {
            errs.Add($"Thiếu ảnh Resources/Images/{levelName}.png (JamImage.Initialize sẽ báo missing texture, in ảnh rỗng).");
            return errs;
        }

        // Mô phỏng BuildCellOwners 28x28 của JamImage để báo màu bị gom sai ô
        errs.AddRange(SimulateCellOwners(levelName, tex, holder, parts, db));
        return errs;
    }

    private static List<string> SimulateCellOwners(string levelName, Texture2D tex, LevelYarnInfoHolder holder, YarnPart[] parts, YarnColorDatabase db)
    {
        var errs = new List<string>();
        try
        {
            string assetPath = AssetDatabase.GetAssetPath(tex);
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null && !importer.isReadable)
            {
                errs.Add("Ảnh chưa bật Read/Write Enabled nên không mô phỏng chia ô được — bật trong Import Settings rồi quét lại.");
                return errs;
            }

            var available = new List<YarnColor>();
            if (holder != null && holder.colorCountEntries != null && holder.colorCountEntries.Count > 0)
            {
                foreach (var e in holder.colorCountEntries)
                    if (e.count > 0 && !available.Contains(e.color)) available.Add(e.color);
            }
            else
            {
                foreach (var p in parts)
                    if (!available.Contains(p.initColor)) available.Add(p.initColor);
            }
            if (available.Count == 0) { errs.Add("Level không có màu nào."); return errs; }

            var refColors = available.ToDictionary(c => c, c => EditorRefColor(db, c));
            const int grid = 28;
            var ownerCounts = available.ToDictionary(c => c, _ => 0);
            int w = tex.width, h = tex.height;
            for (int cy = 0; cy < grid; cy++)
                for (int cx = 0; cx < grid; cx++)
                {
                    int sx = Mathf.Clamp(Mathf.RoundToInt((cx + 0.5f) / grid * w), 0, w - 1);
                    int sy = Mathf.Clamp(Mathf.RoundToInt((cy + 0.5f) / grid * h), 0, h - 1);
                    Color sample;
                    try { sample = tex.GetPixel(sx, sy); }
                    catch { errs.Add("Không đọc được pixel ảnh (bật Read/Write)."); return errs; }
                    YarnColor best = available[0];
                    float bestD = float.MaxValue;
                    foreach (var c in available)
                    {
                        var cand = refColors[c];
                        float dr = sample.r - cand.r, dg = sample.g - cand.g, dbc = sample.b - cand.b;
                        float d = dr * dr + dg * dg + dbc * dbc;
                        if (d < bestD) { bestD = d; best = c; }
                    }
                    ownerCounts[best]++;
                }

            var zero = ownerCounts.Where(kv => kv.Value == 0).Select(kv => kv.Key.ToString()).ToList();
            if (zero.Count > 0)
                errs.Add($"Các màu có sợi nhưng được chia 0 ô ảnh (bắn lên trắng/rỗng): {string.Join(", ", zero)} — đổi màu len theo ảnh hoặc đổi ảnh.");
        }
        catch (Exception ex)
        {
            errs.Add("Lỗi mô phỏng ảnh: " + ex.Message);
        }
        return errs;
    }

    private static Color EditorRefColor(YarnColorDatabase db, YarnColor color)
    {
        var mat = db != null ? db.GetMaterial(color) : null;
        if (mat != null)
        {
            if (mat.HasProperty("_MainColor")) return mat.GetColor("_MainColor");
            if (mat.HasProperty("_Color")) return mat.GetColor("_Color");
        }
        return color switch
        {
            YarnColor.Blue => new Color(0.033f, 0.57f, 1f),
            YarnColor.Brown => new Color(0.48f, 0.30f, 0.18f),
            YarnColor.Charcoal => new Color(0.20f, 0.20f, 0.22f),
            YarnColor.Coral => new Color(0.9725f, 0.2745f, 0.2863f),
            YarnColor.Cream => new Color(1f, 0.9176f, 0.7412f),
            YarnColor.Crimson => new Color(0.75f, 0.10f, 0.20f),
            YarnColor.Deep => new Color(0.10f, 0.15f, 0.35f),
            YarnColor.Forest => new Color(0.13f, 0.42f, 0.23f),
            YarnColor.Gray => new Color(0.55f, 0.55f, 0.55f),
            YarnColor.Green => new Color(0.4235f, 0.7843f, 0.2196f),
            YarnColor.Lime => new Color(0.6196f, 0.7922f, 0.1529f),
            YarnColor.Magenta => new Color(0.90f, 0.15f, 0.70f),
            YarnColor.Orange => new Color(1f, 0.55f, 0.10f),
            YarnColor.Pink => new Color(0.9686f, 0.3294f, 0.6431f),
            YarnColor.Plum => new Color(0.55f, 0.25f, 0.45f),
            YarnColor.Purple => new Color(0.55f, 0.25f, 0.85f),
            YarnColor.Sky => new Color(0.45f, 0.75f, 1f),
            YarnColor.Snow => new Color(0.96f, 0.96f, 0.98f),
            YarnColor.Steel => new Color(0.55f, 0.62f, 0.70f),
            YarnColor.Teal => new Color(0.10f, 0.65f, 0.65f),
            YarnColor.Yellow => new Color(0.9608f, 0.8157f, 0.1451f),
            _ => Color.white
        };
    }

    private static void AddCount(Dictionary<YarnColor, int> dict, YarnColor c, int n)
    {
        if (!dict.ContainsKey(c)) dict[c] = 0;
        dict[c] += n;
    }

    /// <summary>
    /// Tự sửa lỗi tổng % 3: thêm 1-2 lớp inner Fixed cùng màu part (không đụng pool random,
    /// không đổi thiết kế) rồi cập nhật holder + lưu prefab.
    /// </summary>
    private void AutoFixDiv3()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== TỰ SỬA TỔNG %3 ===");
        int fixedCount = 0;

        var levelPaths = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p)
            .ToList();

        foreach (var path in levelPaths)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                var parts = root.GetComponentsInChildren<YarnPart>(true);
                var holder = root.GetComponent<LevelYarnInfoHolder>();
                if (holder == null) holder = root.GetComponentInChildren<LevelYarnInfoHolder>(true);
                if (parts.Length == 0 || holder == null) continue;

                var real = new Dictionary<YarnColor, int>();
                foreach (var p in parts)
                {
                    AddCount(real, p.initColor, 1);
                    int limit = Math.Min(p.totalInnerCount, Math.Min(
                        p.innerColorTypes != null ? p.innerColorTypes.Count : 0,
                        p.innerFixedColors != null ? p.innerFixedColors.Count : 0));
                    for (int j = 0; j < limit; j++)
                        if (p.innerColorTypes[j] == InnerColorType.Fixed)
                            AddCount(real, p.innerFixedColors[j], 1);
                }
                if (holder.randomColorEntries != null)
                    foreach (var e in holder.randomColorEntries)
                        AddCount(real, e.color, e.count);

                int total = real.Values.Sum();
                int deficit = (3 - total % 3) % 3;
                if (deficit == 0) continue;

                // Thêm lớp Fixed cùng màu vào part có ít lớp nhất (thay đổi gameplay ít nhất)
                var target = parts.OrderBy(p => p.totalInnerCount).First();
                if (target.innerColorTypes == null) target.innerColorTypes = new List<InnerColorType>();
                if (target.innerFixedColors == null) target.innerFixedColors = new List<YarnColor>();
                for (int i = 0; i < deficit; i++)
                {
                    target.totalInnerCount++;
                    target.innerColorTypes.Add(InnerColorType.Fixed);
                    target.innerFixedColors.Add(target.initColor);
                }
                EditorUtility.SetDirty(target);

                var list = parts.ToList();
                holder.allBaseYarnParts = list;
                holder.totalYarnPartCount = holder.GetTotalYarnPartCount(list);
                holder.UpdateDebugInfo();
                EditorUtility.SetDirty(holder);

                PrefabUtility.SaveAsPrefabAsset(root, path);
                sb.AppendLine($"[SỬA] {Path.GetFileNameWithoutExtension(path)}: tổng {total} -> {total + deficit} (thêm {deficit} lớp {target.initColor} vào '{target.name}').");
                fixedCount++;
            }
            catch (Exception ex)
            {
                sb.AppendLine($"[LỖI] {path}: " + ex.Message);
                Debug.LogException(ex);
            }
            finally
            {
                if (root != null) PrefabUtility.UnloadPrefabContents(root);
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        sb.AppendLine(fixedCount == 0 ? "Không có level nào cần sửa." : $"Đã sửa {fixedCount} level. Bấm Quét full để kiểm tra lại.");
        report = sb.ToString() + "\n" + ScanAllLevels(out okLevelCount, out errorLevelCount);
        Debug.Log(report);
    }
}
