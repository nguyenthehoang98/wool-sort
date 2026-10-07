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
    /// Mở window: Tools / Puzzle / Kiểm tra màu box-ảnh
    /// Quét nhanh: Tools / Puzzle / Quét tất cả level
/// </summary>
public sealed class BoxImageColorValidator : EditorWindow
{
    private const string DatabasePath = "Assets/_Game/Configs/YarnColorDatabase.asset";
    private const string LevelsFolder = "Assets/Resources/Levels";

    private Vector2 scroll;
    private Vector2 reportScroll;
    private string report = "Bấm \"Quét full\" để kiểm tra. Tab tự quét nhanh Database lúc mở.";
    private int okLevelCount;
    private int errorLevelCount;
    private bool initialized;

    /// <summary>Gọi 1 lần khi Studio mở tab (thay OnEnable cũ).</summary>
    public void InitPanel()
    {
        if (initialized) return;
        initialized = true;
        OnEnable();
    }

    public void DrawPanel() => OnGUI();

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

        GUILayout.Label("BƯỚC 1 — LEVEL (sợi, màu, số lượng)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("1. Quét level", GUILayout.Height(32))) RefreshLevelOnly();
        if (GUILayout.Button("Chỉ quét Database", GUILayout.Height(28)))
        {
            var sb = new StringBuilder();
            int e = CheckDatabase(sb);
            report = sb.ToString() + (e == 0 ? "\n[OK] Database." : $"\n[LỖI] {e} material thiếu.");
        }
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Sửa từng màu %3 (giữ lớp ngoài, chỉ thêm inner)", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Sửa từng màu %3?",
                "Với mỗi màu lẻ trong từng level, tool sẽ thêm 1-2 lớp inner Fixed màu đó vào part ít lớp nhất.\nLớp ngoài (hình ảnh) giữ nguyên, chỉ inner thay đổi.\nTiếp tục?",
                "Sửa luôn", "Hủy"))
                AutoFixPerColorDiv3();
        }

        if (GUILayout.Button("Tự sửa tổng %3 (thêm lớp Fixed cùng màu)", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Tự sửa tổng %3?",
                "Với mỗi level có tổng số sợi không chia hết cho 3, tool sẽ thêm 1-2 lớp inner Fixed cùng màu vào 1 part, rồi lưu prefab.\nTiếp tục?",
                "Sửa luôn", "Hủy"))
                AutoFixDiv3();
        }

        GUILayout.Space(4);
        GUILayout.Label("BƯỚC 2 — BOX ẢNH (ma trận màu, texture, chia ô)", EditorStyles.boldLabel);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("2. Quét box-ảnh", GUILayout.Height(32))) RefreshImageOnly();
        if (GUILayout.Button("Quét full (cả 2 bước)", GUILayout.Height(32))) RefreshAll();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Level OK: {okLevelCount}   Level lỗi: {errorLevelCount}");
        // Trong Studio: bỏ scroll lồng nhau, để scroll ngoài của Studio lo.
        GUILayout.Label("Báo cáo:", EditorStyles.miniBoldLabel);
        reportScroll = EditorGUILayout.BeginScrollView(reportScroll, GUILayout.Height(180));
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void RefreshAll()
    {
        report = ScanAllLevels(out okLevelCount, out errorLevelCount, doLevel: true, doImage: true);
    }

    private void RefreshLevelOnly()
    {
        report = ScanAllLevels(out okLevelCount, out errorLevelCount, doLevel: true, doImage: false);
    }

    private void RefreshImageOnly()
    {
        report = ScanAllLevels(out okLevelCount, out errorLevelCount, doLevel: false, doImage: true);
    }

    // ---------------- Core scan (dùng chung cho window + menu) ----------------

    private static string ScanAllLevels(out int okCount, out int errCount, bool doLevel = true, bool doImage = true)
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

        string step = doLevel && doImage ? "full (level + box-ảnh)"
            : doLevel ? "bước 1 (level)" : "bước 2 (box-ảnh)";
        sb.AppendLine($"Quét {step}: {levelPaths.Count} level trong {LevelsFolder}...");

        foreach (var path in levelPaths)
        {
            var errs = new List<string>();
            if (doLevel) errs.AddRange(ScanLevelConfig(path, db));
            if (doImage) errs.AddRange(ScanBoxImage(path, db));
            if (errs.Count == 0) okCount++;
            else
            {
                errCount++;
                sb.AppendLine($"[LỖI] {Path.GetFileNameWithoutExtension(path)} ({errs.Count}):");
                foreach (var e in errs) sb.AppendLine("  [LỖI] " + e);
            }
        }

        sb.AppendLine($"Xong {step}: {okCount} OK, {errCount} lỗi / {levelPaths.Count} level.");
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

    /// <summary>BƯỚC 1 — level: part, màu, số lượng, holder, material.</summary>
    private static List<string> ScanLevelConfig(string prefabPath, YarnColorDatabase db)
    {
        var errs = new List<string>();

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

        // Pool random phải khớp demand: thiếu là runtime rớt info (vỡ %3, pool rỗng cuối game),
        // thừa là sợi ma không bao giờ spawn.
        {
            int demand = 0;
            foreach (var p in parts)
            {
                int types = p.innerColorTypes != null ? p.innerColorTypes.Count : 0;
                int fixeds = p.innerFixedColors != null ? p.innerFixedColors.Count : 0;
                int limit = Math.Min(p.totalInnerCount, Math.Min(types, fixeds));
                for (int j = 0; j < limit; j++)
                    if (p.innerColorTypes[j] == InnerColorType.Random) demand++;
            }
            int supply = 0;
            if (holder != null && holder.randomColorEntries != null)
                foreach (var e in holder.randomColorEntries) supply += e.count;
            if (demand != supply)
                errs.Add(demand > supply
                    ? $"Pool random lệch: {demand} slot Random nhưng randomColorEntries chỉ {supply} (thiếu {demand - supply}) — runtime rớt info, vỡ %3, cuối game pool rỗng! Thêm {demand - supply} vào pool (dồn 1 màu để khỏi vỡ %3)."
                    : $"Pool random lệch: {demand} slot Random nhưng randomColorEntries tới {supply} (thừa {supply - demand}) — holder đếm sợi ma không bao giờ spawn, tổng ảo, cuối game lệch box! Bớt {supply - demand} khỏi pool.");
        }

        int total = real.Values.Sum();
        if (total == 0) errs.Add("Tổng số sợi = 0.");
        else if (total % 3 != 0)
            errs.Add($"Tổng {total} không chia hết cho 3 (1 box = 3 sợi) — box cuối lẻ, ảnh thiếu/thừa ô.");
        // Từng màu cũng phải chia hết cho 3, nếu không box cuối của màu đó không bao giờ đủ 3 sợi
        // cùng màu -> runtime sinh box None (nhận mọi màu -> box lẫn Green,Orange,Gray) hoặc kẹt game.
        foreach (var kv in real.OrderBy(kv => kv.Key.ToString()))
        {
            if (kv.Key == YarnColor.None || kv.Key == YarnColor.Rainbow) continue;
            if (kv.Value % 3 != 0)
                errs.Add($"Màu {kv.Key} có {kv.Value} sợi, không chia hết cho 3 — cuối game sẽ dư {kv.Value % 3} sợi lẻ, box lẫn màu/BoxColor=None, ảnh tô sai!");
        }

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
        return errs;
    }

    /// <summary>BƯỚC 2 — box ảnh: ma trận color ID, texture, mô phỏng chia ô.</summary>
    private static List<string> ScanBoxImage(string prefabPath, YarnColorDatabase db)
    {
        var errs = new List<string>();
        string levelName = Path.GetFileNameWithoutExtension(prefabPath);

        var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (go == null) return new List<string> { $"Không load được prefab: {prefabPath}" };

        var holder = go.GetComponent<LevelYarnInfoHolder>();
        if (holder == null) holder = go.GetComponentInChildren<LevelYarnInfoHolder>(true);
        var parts = go.GetComponentsInChildren<YarnPart>(true);

        // Ma trận color ID: runtime JamImage ưu tiên đọc file này thay vì ảnh trực tiếp.
        // So với ĐỦ màu (ngoài + inner cố định + pool) vì lớp trong hiện ra cũng cần ô để tô.
        var realForMatrix = new Dictionary<YarnColor, int>();
        foreach (var p in parts)
        {
            AddCount(realForMatrix, p.initColor, 1);
            int lim = System.Math.Min(p.totalInnerCount, System.Math.Min(
                p.innerColorTypes != null ? p.innerColorTypes.Count : 0,
                p.innerFixedColors != null ? p.innerFixedColors.Count : 0));
            for (int j = 0; j < lim; j++)
                if (p.innerColorTypes[j] == InnerColorType.Fixed)
                    AddCount(realForMatrix, p.innerFixedColors[j], 1);
        }
        if (holder != null && holder.randomColorEntries != null)
            foreach (var e in holder.randomColorEntries)
                AddCount(realForMatrix, e.color, e.count);
        var matrix = AssetDatabase.LoadAssetAtPath<LevelColorMatrix>($"Assets/Resources/LevelColorMatrix/{levelName}.asset");
        bool matrixOk = matrix != null && matrix.IsValid;
        bool matrixDisplay = matrixOk && matrix.HasDisplayColors;

        // Ảnh: chỉ bắt buộc khi chưa có ma trận đủ màu hiển thị (cho phép dời ảnh khỏi Resources).
        Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/{levelName}.jpg");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/Images/Level_{levelName}.png");
        if (tex == null) tex = AssetDatabase.LoadAssetAtPath<Texture2D>($"Assets/Resources/LevelImages/{levelName}.png");
        if (tex == null && !matrixDisplay)
        {
            errs.Add($"Thiếu ảnh Resources/Images/{levelName}.png (JamImage.Initialize sẽ báo missing texture, in ảnh rỗng).");
            return errs;
        }

        if (matrix == null)
        {
            errs.Add("Chưa có ma trận color ID Assets/Resources/LevelColorMatrix/" + levelName + ".asset — chạy Tools/Puzzle/Extract ảnh thành ma trận màu (runtime đang đọc ảnh trực tiếp, dễ sai màu).");
        }
        else if (!matrixOk)
        {
            errs.Add($"Ma trận color ID {levelName} hỏng (grid {matrix.gridSize}, {matrix.TotalCellCount} ô) — extract lại.");
        }
        else
        {
            var matrixColors = matrix.GetColors();
            foreach (var c in matrixColors)
                if (!realForMatrix.ContainsKey(c))
                    errs.Add($"Ma trận có màu {c} ({matrix.GetCellCount(c)} ô) nhưng level không có sợi nào màu đó — extract lại sau khi sửa level.");
            foreach (var kv in realForMatrix)
                if (!matrixColors.Contains(kv.Key))
                    errs.Add($"Màu {kv.Key} ({kv.Value} sợi) không có trong ma trận — bắn lên sẽ rỗng, extract lại ảnh.");
            if (!matrixDisplay)
                errs.Add($"Ma trận {levelName} thiếu màu hiển thị từng ô — extract lại để dời ảnh khỏi Resources.");
        }

        // Mô phỏng BuildCellOwners của JamImage để báo màu bị gom sai ô (cần ảnh gốc)
        if (tex != null)
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
            const int grid = LevelColorMatrix.DefaultGrid;
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

    /// <summary>
    /// Sửa từng màu % 3 mà KHÔNG đụng lớp ngoài (hình ảnh giữ nguyên):
    /// - Chuẩn hóa list inner về đúng totalInnerCount (thiếu thì bù Fixed cùng màu ngoài để đếm được).
    /// - Màu nào lẻ thì thêm 1-2 lớp inner Fixed màu đó vào part ít lớp nhất.
    /// Random pool và initColor ngoài giữ nguyên.
    /// </summary>
    private void AutoFixPerColorDiv3()
    {
        string fix = AutoFixPerColorDiv3Core();
        report = fix + "\n" + ScanAllLevels(out okLevelCount, out errorLevelCount);
        Debug.Log(report);
    }

    private static string AutoFixPerColorDiv3Core()
    {
        var sb = new StringBuilder();
        sb.AppendLine("=== TỰ SỬA TỪNG MÀU %3 (giữ lớp ngoài) ===");
        int fixedLevelCount = 0;

        var levelPaths = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => p)
            .ToList();

        foreach (var path in levelPaths)
        {
            string levelName = Path.GetFileNameWithoutExtension(path);
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(path);
                var parts = root.GetComponentsInChildren<YarnPart>(true).OrderBy(p => p.name).ToList();
                var holder = root.GetComponent<LevelYarnInfoHolder>();
                if (holder == null) holder = root.GetComponentInChildren<LevelYarnInfoHolder>(true);
                if (parts.Count == 0 || holder == null) continue;

                // 1) Chuẩn hóa list inner (chỉ thêm/bớt ở inner, ngoài giữ nguyên)
                foreach (var p in parts)
                {
                    bool dirty = false;
                    if (p.innerColorTypes == null) { p.innerColorTypes = new List<InnerColorType>(); dirty = true; }
                    if (p.innerFixedColors == null) { p.innerFixedColors = new List<YarnColor>(); dirty = true; }
                    while (p.innerColorTypes.Count < p.totalInnerCount) { p.innerColorTypes.Add(InnerColorType.Fixed); dirty = true; }
                    while (p.innerFixedColors.Count < p.totalInnerCount) { p.innerFixedColors.Add(p.initColor); dirty = true; }
                    while (p.innerColorTypes.Count > p.totalInnerCount) { p.innerColorTypes.RemoveAt(p.innerColorTypes.Count - 1); dirty = true; }
                    while (p.innerFixedColors.Count > p.totalInnerCount) { p.innerFixedColors.RemoveAt(p.innerFixedColors.Count - 1); dirty = true; }
                    if (dirty) EditorUtility.SetDirty(p);
                }

                // 2) Đếm thật: ngoài + inner Fixed + random pool
                var real = new Dictionary<YarnColor, int>();
                foreach (var p in parts)
                {
                    AddCount(real, p.initColor, 1);
                    for (int j = 0; j < p.totalInnerCount; j++)
                        if (p.innerColorTypes[j] == InnerColorType.Fixed)
                            AddCount(real, p.innerFixedColors[j], 1);
                }
                if (holder.randomColorEntries != null)
                    foreach (var e in holder.randomColorEntries)
                        AddCount(real, e.color, e.count);

                // 3) Màu lẻ -> thêm lớp inner Fixed màu đó (ngoài của part được chọn giữ nguyên màu cũ)
                bool changed = false;
                foreach (var kv in real.Where(kv => kv.Key != YarnColor.None && kv.Key != YarnColor.Rainbow).OrderBy(kv => kv.Key.ToString()).ToList())
                {
                    int need = (3 - kv.Value % 3) % 3;
                    for (int i = 0; i < need; i++)
                    {
                        var target = parts.OrderBy(p => p.totalInnerCount).First();
                        target.totalInnerCount++;
                        target.innerColorTypes.Add(InnerColorType.Fixed);
                        target.innerFixedColors.Add(kv.Key);
                        EditorUtility.SetDirty(target);
                        AddCount(real, kv.Key, 1);
                        changed = true;
                        sb.AppendLine($"[SỬA] {levelName}: +1 inner {kv.Key} vào '{target.name}' (ngoài {target.initColor} giữ nguyên).");
                    }
                }

                if (!changed) continue;

                holder.allBaseYarnParts = parts;
                holder.totalYarnPartCount = holder.GetTotalYarnPartCount(parts);
                holder.UpdateDebugInfo();
                EditorUtility.SetDirty(holder);

                PrefabUtility.SaveAsPrefabAsset(root, path);
                fixedLevelCount++;
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
        sb.AppendLine(fixedLevelCount == 0 ? "Không có level nào cần sửa." : $"Đã sửa {fixedLevelCount} level (lớp ngoài giữ nguyên). Quét lại bước 1 để kiểm tra.");
        return sb.ToString();
    }
}
