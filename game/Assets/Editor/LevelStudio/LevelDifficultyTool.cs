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
/// Check + sửa độ khó level (project wool-sort).
/// Điểm khó = Số màu x8 + Tổng sợi x0.25 + (Độ sâu-1) x6 + Tỉ lệ random x12.
/// - Dễ: <= 70 | Trung bình: <= 110 | Khó: <= 160 | Cực khó: > 160.
/// Sửa an toàn, giữ nguyên tắc mỗi màu % 3 (không bao giờ sinh box None):
/// - Preset nhịp ra box (boxSpawnThresholds, tổng phải = 100).
/// - Giảm 1 màu: gộp 2 màu hiếm nhất (hợp 2 bội số của 3 vẫn chia hết cho 3).
/// - Tăng 1 màu: chuyển đúng 3 sợi ngoài của màu nhiều nhất sang màu mới.
/// Mở: Tools / Puzzle / 7. Do kho level (check + sua)
/// </summary>
public sealed class LevelDifficultyTool : EditorWindow
{
    private const string LevelsFolder = "Assets/Resources/Levels";
    private const string DatabasePath = "Assets/_Game/Configs/YarnColorDatabase.asset";

    private Vector2 scroll;
    private Vector2 reportScroll;
    private string report = "Bấm \"Quét độ khó\" để chấm điểm tất cả level.";
    private readonly List<DiffRow> rows = new List<DiffRow>();

    private int selectedTab;
    private int selectedLevelIndex;
    private int detailLastLoaded = -1;
    private readonly int[] thresholdEdit = new int[4] { 20, 40, 15, 25 };
    private int nhịpPresetIndex;

    private sealed class NhipPreset
    {
        public string name;
        public int[] thresholds;
    }

    private static readonly List<NhipPreset> NhipPresets = new List<NhipPreset>
    {
        new NhipPreset { name = "Cân bằng TB [20,40,15,25]", thresholds = new[]{20,40,15,25} },
        new NhipPreset { name = "Dễ [30,45,10,15]", thresholds = new[]{30,45,10,15} },
        new NhipPreset { name = "Khó [10,30,30,30]", thresholds = new[]{10,30,30,30} },
        new NhipPreset { name = "Dễ thở đầu game [35,40,15,10]", thresholds = new[]{35,40,15,10} },
        new NhipPreset { name = "Khó dần [15,35,25,25]", thresholds = new[]{15,35,25,25} },
        new NhipPreset { name = "Cuối bùng nổ [20,30,20,30]", thresholds = new[]{20,30,20,30} },
        new NhipPreset { name = "Ổn định đều [25,25,25,25]", thresholds = new[]{25,25,25,25} },
        new NhipPreset { name = "Mở nhanh [30,45,10,15]", thresholds = new[]{30,45,10,15} },
        new NhipPreset { name = "Dồn cuối [15,25,30,30]", thresholds = new[]{15,25,30,30} },
        new NhipPreset { name = "Cày đầu nhẹ [40,35,15,10]", thresholds = new[]{40,35,15,10} },
    };
    private bool initialized;

    /// <summary>Gọi 1 lần khi Studio mở tab (thay Open cũ).</summary>
    public void InitPanel()
    {
        if (initialized) return;
        initialized = true;
        Scan();
    }

    public void DrawPanel() => OnGUI();

    private sealed class DiffRow
    {
        public string levelName;
        public string path;
        public int colors;
        public int total;
        public int outerCount;
        public int innerFixedCount;
        public int randomSlots;
        public int poolCount;
        public int maxDepth;
        public float randomShare;
        public float score;
        public string rating;
        public int[] thresholds = new int[4];
        public Dictionary<YarnColor, int> perColor = new Dictionary<YarnColor, int>();
    }

    // Studio tự tạo panel, không còn menu riêng.

    private void OnGUI()
    {
        GUILayout.Label("ĐỘ KHÓ LEVEL (check + sửa)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Điểm = Số màu x8 + Tổng sợi x0.25 + (Độ sâu-1) x6 + Tỉ lệ random x12.\n" +
            "Dễ ≤70 | Trung bình ≤110 | Khó ≤160 | Cực khó >160.",
            MessageType.Info);

        selectedTab = GUILayout.Toolbar(selectedTab, new[] { "Độ khó level", "Chi tiết level" }, GUILayout.Height(50));

        if (rows.Count == 0)
        {
            EditorGUILayout.TextArea(report, GUILayout.Height(60));
            return;
        }

        if (selectedTab == 0) DrawOverviewTab();
        else DrawDetailTab();

        GUILayout.Space(4);
        GUILayout.Label("Báo cáo:", EditorStyles.miniBoldLabel);
        reportScroll = EditorGUILayout.BeginScrollView(reportScroll, GUILayout.Height(220));
        EditorGUILayout.TextArea(report, GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    // ---------------- Tab 1: tổng quan độ khó ----------------

    private void DrawOverviewTab()
    {
        EditorGUILayout.HelpBox(
            "Độ khó nên tăng dần theo level — dòng nào tụt điểm sẽ báo.",
            MessageType.None);
        EditorGUILayout.HelpBox(
            "Chuẩn 21 màu = YarnColorDatabase (màu <-> material yarn/basket/slot). " +
            "Mỗi level chỉ dùng tập con (3-6 màu từ prefab đó). Nút dưới ép mesh mọi level " +
            "về đúng material DB theo initColor + báo màu thiếu material.",
            MessageType.None);
        if (GUILayout.Button("Đồng bộ màu TẤT CẢ levels theo DB 21 màu", GUILayout.Height(30)))
            ResyncAllLevels();
        scroll = EditorGUILayout.BeginScrollView(scroll);
        DiffRow prev = null;
        foreach (var r in rows)
        {
            EditorGUILayout.BeginHorizontal();
            bool drop = prev != null && r.score < prev.score - 15f;
            GUILayout.Label($"{r.levelName}", EditorStyles.boldLabel, GUILayout.Width(70));
            GUILayout.Label($"{r.rating}", GUILayout.Width(80));
            GUILayout.Label($"điểm {r.score:F0}", GUILayout.Width(70));
            GUILayout.Label($"{r.colors} màu | {r.total} sợi | sâu {r.maxDepth} | random {r.randomShare:P0}", EditorStyles.miniLabel);
            if (drop) GUILayout.Label("TỤT ĐỘ KHÓ!", EditorStyles.boldLabel);
            if (GUILayout.Button("Chi tiết", GUILayout.Width(70)))
            {
                selectedLevelIndex = rows.IndexOf(r);
                detailLastLoaded = -1; // ép nạp lại nhịp box của level này
                selectedTab = 1;
            }
            EditorGUILayout.EndHorizontal();
            prev = r;
        }
        EditorGUILayout.EndScrollView();
    }

    // ---------------- Tab 2: chi tiết + sửa 1 level ----------------

    private void DrawDetailTab()
    {
        string[] names = rows.Select(r => $"{r.levelName} ({r.rating} {r.score:F0})").ToArray();
        selectedLevelIndex = Mathf.Clamp(selectedLevelIndex, 0, rows.Count - 1);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Level:", GUILayout.Width(45));
        selectedLevelIndex = EditorGUILayout.Popup(selectedLevelIndex, names);
        EditorGUILayout.EndHorizontal();
        var sel = rows[selectedLevelIndex];

        // Nạp nhịp box hiện tại của level vào ô sửa mỗi khi đổi level.
        if (detailLastLoaded != selectedLevelIndex)
        {
            for (int i = 0; i < 4; i++) thresholdEdit[i] = sel.thresholds[i];
            detailLastLoaded = selectedLevelIndex;
        }

        // Khối thông tin chi tiết
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label($"{sel.levelName} — {sel.rating} (điểm {sel.score:F1})", EditorStyles.boldLabel);
        GUILayout.Label($"Màu: {sel.colors} | Sợi: {sel.total} (ngoài {sel.outerCount} + inner cố định {sel.innerFixedCount} + pool {sel.poolCount})", EditorStyles.miniLabel);
        GUILayout.Label($"Độ sâu tối đa: {sel.maxDepth} | Ô random: {sel.randomSlots} | Tỉ lệ random: {sel.randomShare:P0} | Nhịp box: [{string.Join(",", sel.thresholds)}]", EditorStyles.miniLabel);
        GUILayout.Label("Từng màu:", EditorStyles.miniBoldLabel);
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        foreach (var kv in sel.perColor.OrderByDescending(kv => kv.Value))
        {
            float pct = sel.total > 0 ? (float)kv.Value / sel.total * 100f : 0f;
            var show = LevelColorMatrixUtility.RefColor(db, kv.Key);
            show.a = 1f;
            EditorGUILayout.BeginHorizontal();
            var swatch = GUILayoutUtility.GetRect(16, 14, GUILayout.Width(16));
            EditorGUI.DrawRect(swatch, show);
            GUILayout.Label($"{kv.Key}", GUILayout.Width(80));
            GUILayout.Label($"{kv.Value} sợi", GUILayout.Width(70));
            GUILayout.Label($"{pct:F1}%", GUILayout.Width(55));
            var rect = GUILayoutUtility.GetRect(0, 12, GUILayout.ExpandWidth(true));
            EditorGUI.ProgressBar(rect, sel.total > 0 ? (float)kv.Value / sel.total : 0f, "");
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();

        GUILayout.Space(4);
        GUILayout.Label("SỬA ĐỘ KHÓ", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Nhịp box không đổi cấu trúc sợi. Gộp/tách màu CÓ đổi sợi ngoài (đã tự UpdateDebugInfo + lưu prefab).",
            MessageType.None);

        GUILayout.Label("Nhịp box (tổng=100):", EditorStyles.miniBoldLabel);
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 4; i++) thresholdEdit[i] = EditorGUILayout.IntField(thresholdEdit[i], GUILayout.Width(45));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Preset Dễ [30,45,10,15]")) SetThresholdEdit(30, 45, 10, 15);
        if (GUILayout.Button("Preset TB [20,40,15,25]")) SetThresholdEdit(20, 40, 15, 25);
        if (GUILayout.Button("Preset Khó [10,30,30,30]")) SetThresholdEdit(10, 30, 30, 30);
        EditorGUILayout.EndHorizontal();
        EditorGUI.BeginChangeCheck();
        nhịpPresetIndex = EditorGUILayout.Popup("Preset nhịp:", nhịpPresetIndex, NhipPresets.Select(p => p.name).ToArray());
        if (EditorGUI.EndChangeCheck())
        {
            var pr = NhipPresets[Mathf.Clamp(nhịpPresetIndex, 0, NhipPresets.Count - 1)];
            for (int i = 0; i < 4; i++) thresholdEdit[i] = pr.thresholds[i];
        }
        if (GUILayout.Button($"Áp nhịp box cho {sel.levelName} (đang [{string.Join(",", sel.thresholds)}])", GUILayout.Height(28)))
            ApplyThresholds(sel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Giảm 1 màu (gộp 2 màu hiếm nhất)", GUILayout.Height(28))) MergeRarest(sel);
        if (GUILayout.Button("Tăng 1 màu (tách 3 sợi sang màu mới)", GUILayout.Height(28))) SplitNewColor(sel);
        EditorGUILayout.EndHorizontal();

        DrawManualColorEdit(sel);

        if (GUILayout.Button($"Đồng bộ material model theo initColor ({sel.levelName})", GUILayout.Height(28)))
            ResyncModelMaterials(sel);
        if (GUILayout.Button($"Chẩn đoán lệch màu ({sel.levelName})", GUILayout.Height(28)))
            DiagnoseColors(sel);
        if (GUILayout.Button($"Quét tràn mesh inner ({sel.levelName})", GUILayout.Height(28)))
            DiagnoseMeshOverflow(sel);
        EditorGUILayout.HelpBox(
            "Nếu model nhìn sai màu so với dataset/preview: bấm Đồng bộ material (nó ép mọi MeshRenderer về đúng material của initColor trong Database).",
            MessageType.None);
    }

    // ---------------- Chỉnh tay từng màu ----------------

    private YarnColor manualSrc = YarnColor.Green;
    private YarnColor manualDst = YarnColor.Orange;
    private int manualCount = 3;
    private int keepSmallInner = 1;

    /// <summary>
    /// Chỉnh tay: +-3 sợi từng màu (thêm/bớt inner Fixed, giữ %3) + đổi màu tùy chọn.
    /// </summary>
    private void DrawManualColorEdit(DiffRow sel)
    {
        GUILayout.Space(4);
        GUILayout.Label("CHỈNH TAY TỪNG MÀU", EditorStyles.boldLabel);
        if (GUILayout.Button("❓ Muốn 1 màu ra box nhiều thì setup sao?", GUILayout.Height(26)))
            EditorUtility.DisplayDialog("Cho 1 màu ra nhiều",
                "Box mới ưu tiên màu có nhiều sợi chờ ở LỚP NGOÀI (số sum3 nhỏ nhất).\n\n" +
                "Muốn màu X ra dày:\n" +
                "1. Nhiều part ngoài màu X: mục 'Đổi part ngoài sang màu khác' bên dưới (đổi vài part sang X).\n" +
                "2. Bớt màu khác: mỗi dòng màu bấm -3 vài lần, hoặc 'Gộp 2 màu hiếm nhất'.\n" +
                "3. Nhịp box phase đầu lấy ít màu (preset Mở nhanh/Dồn cuối).\n\n" +
                "Nhớ giữ mỗi màu chia hết cho 3 (1 box = 3 sợi), xong chạy Validate.",
                "Đã hiểu");
        EditorGUILayout.HelpBox(
            "+3/-3 sợi = thêm/bớt 3 lớp inner Fixed (lớp ngoài giữ nguyên, tổng vẫn %3).\n" +
            "Đổi màu: chuyển part ngoài sang màu khác (có thể lẻ %3 -> chạy lại Validate tab 3).",
            MessageType.None);

        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        foreach (var kv in sel.perColor.OrderByDescending(kv => kv.Value))
        {
            EditorGUILayout.BeginHorizontal();
            var show = LevelColorMatrixUtility.RefColor(db, kv.Key);
            show.a = 1f;
            var swatch = GUILayoutUtility.GetRect(16, 14, GUILayout.Width(16));
            EditorGUI.DrawRect(swatch, show);
            GUILayout.Label($"{kv.Key}", GUILayout.Width(80));
            GUILayout.Label($"{kv.Value} sợi", GUILayout.Width(70));
            if (GUILayout.Button("-3", GUILayout.Width(40))) AdjustColorCount(sel, kv.Key, -3);
            if (GUILayout.Button("+3", GUILayout.Width(40))) AdjustColorCount(sel, kv.Key, +3);
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(2);
        GUILayout.Label("Đổi part ngoài sang màu khác:", EditorStyles.miniBoldLabel);
        var levelColors = sel.perColor.Keys.ToList();
        if (levelColors.Count > 0 && !levelColors.Contains(manualSrc))
            manualSrc = levelColors[0];
        EditorGUILayout.BeginHorizontal();
        manualSrc = (YarnColor)EditorGUILayout.EnumPopup(manualSrc, GUILayout.Width(110));
        GUILayout.Label("->", GUILayout.Width(25));
        manualDst = (YarnColor)EditorGUILayout.EnumPopup(manualDst, GUILayout.Width(110));
        manualCount = EditorGUILayout.IntField(manualCount, GUILayout.Width(45));
        GUILayout.Label("part", GUILayout.Width(35));
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Đổi N part ngoài", GUILayout.Height(26))) RecolorManual(sel, false);
        if (GUILayout.Button("Đổi TẤT CẢ (ngoài+inner+pool)", GUILayout.Height(26))) RecolorManual(sel, true);
        EditorGUILayout.EndHorizontal();

        GUILayout.Space(2);
        GUILayout.Label("Ưu tiên part to (dồn inner Fixed từ part nhỏ sang part to):", EditorStyles.miniBoldLabel);
        EditorGUILayout.HelpBox(
            "Gameplay sẽ xoay quanh part to (nhiều lớp), part nhỏ vẫn giữ lại vài lớp để không bị bỏ qua.\n" +
            "Chỉ chuyển chỗ inner Fixed — tổng sợi và từng màu giữ nguyên nên %3 không vỡ.",
            MessageType.None);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Part nhỏ giữ lại tối đa:", GUILayout.Width(170));
        keepSmallInner = Mathf.Max(0, EditorGUILayout.IntField(keepSmallInner, GUILayout.Width(45)));
        GUILayout.Label("inner/part", GUILayout.Width(70));
        EditorGUILayout.EndHorizontal();
        if (GUILayout.Button("Dồn inner sang part to (giữ nguyên màu/tổng)", GUILayout.Height(28)))
            RebalanceToBigParts(sel, keepSmallInner);
        if (GUILayout.Button("Random lại inner (giữ ngoài, giữ tổng từng màu)", GUILayout.Height(28)))
            RandomizeInners(sel);
    }

    /// <summary>Thêm (delta=+3) hoặc bớt (delta=-3) sợi 1 màu bằng inner Fixed.</summary>
    private void AdjustColorCount(DiffRow row, YarnColor color, int delta)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var parts = root.GetComponentsInChildren<YarnPart>(true).ToList();
            var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null) { report = "[LỖI] Thiếu LevelYarnInfoHolder."; return; }

            // Chuẩn hóa list inner theo totalInnerCount trước khi thêm/bớt.
            foreach (var p in parts) NormalizeInnerLists(p);

            if (delta > 0)
            {
                // Thêm vào part cùng màu, ít lớp nhất trước (chia đều).
                var cands = parts.Where(p => p.initColor == color).OrderBy(p => p.totalInnerCount).ToList();
                if (cands.Count == 0) { report = $"[LỖI] Không có part ngoài màu {color} để thêm inner."; return; }
                for (int i = 0; i < delta; i++)
                {
                    var p = cands.OrderBy(x => x.totalInnerCount).First();
                    p.totalInnerCount++;
                    p.innerColorTypes.Add(InnerColorType.Fixed);
                    p.innerFixedColors.Add(color);
                    EditorUtility.SetDirty(p);
                }
            }
            else
            {
                // Bớt inner Fixed màu đó, lấy từ part nhiều lớp nhất trước.
                int need = -delta, removed = 0;
                foreach (var p in parts.OrderByDescending(p => p.totalInnerCount))
                {
                    for (int j = p.innerFixedColors.Count - 1; j >= 0 && removed < need; j--)
                    {
                        if (j < p.innerColorTypes.Count && p.innerColorTypes[j] == InnerColorType.Fixed
                            && p.innerFixedColors[j] == color)
                        {
                            p.innerColorTypes.RemoveAt(j);
                            p.innerFixedColors.RemoveAt(j);
                            p.totalInnerCount = Math.Max(0, p.totalInnerCount - 1);
                            EditorUtility.SetDirty(p);
                            removed++;
                        }
                    }
                    if (removed >= need) break;
                }
                if (removed < need) { report = $"[LỖI] Màu {color} chỉ bớt được {removed}/{need} sợi inner (không đủ inner Fixed)."; return; }
            }

            holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            report = $"[OK] {row.levelName}: màu {color} {(delta > 0 ? "+" : "")}{delta} sợi (inner Fixed). Tổng vẫn %3.";
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>
    /// Dồn inner Fixed từ nửa part nhỏ sang nửa part to (to nhất nhận trước).
    /// Màu/tổng sợi giữ nguyên nên %3 không vỡ; part nhỏ giữ lại `keep` inner.
    /// </summary>
    private void RebalanceToBigParts(DiffRow row, int keep)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var parts = root.GetComponentsInChildren<YarnPart>(true).ToList();
            var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null) { report = "[LỖI] Thiếu LevelYarnInfoHolder."; return; }
            if (parts.Count < 2) { report = "[LỖI] Level ít hơn 2 part."; return; }
            foreach (var p in parts) NormalizeInnerLists(p);

            var sized = parts.Select(p =>
            {
                var r = p.GetComponent<Renderer>();
                float s = r != null ? r.bounds.size.magnitude : 0f;
                return new { part = p, size = s };
            }).OrderBy(t => t.size).ToList();
            int half = Mathf.Max(1, sized.Count / 2);
            var small = sized.Take(half).ToList();
            var big = sized.Skip(half).OrderByDescending(t => t.size).ToList();
            int cap = parts.Max(p => p.totalInnerCount) + 2;

            // Gom inner Fixed thừa trên part nhỏ (giữ lại `keep` cái đầu).
            var moving = new List<YarnColor>();
            foreach (var t in small)
            {
                var p = t.part;
                var fixedIdx = new List<int>();
                for (int j = 0; j < p.innerColorTypes.Count && j < p.innerFixedColors.Count; j++)
                    if (p.innerColorTypes[j] == InnerColorType.Fixed) fixedIdx.Add(j);
                foreach (int j in fixedIdx.Skip(keep).OrderByDescending(x => x))
                {
                    moving.Add(p.innerFixedColors[j]);
                    p.innerColorTypes.RemoveAt(j);
                    p.innerFixedColors.RemoveAt(j);
                    p.totalInnerCount = Mathf.Max(0, p.totalInnerCount - 1);
                    EditorUtility.SetDirty(p);
                }
            }
            if (moving.Count == 0) { report = $"[OK] {row.levelName}: part nhỏ không còn inner Fixed thừa để dồn."; return; }

            // Chia đều cho part to (to nhất trước, không vượt cap).
            int bi = 0, placed = 0;
            foreach (var c in moving)
            {
                int guard = 0;
                while (big[bi % big.Count].part.totalInnerCount >= cap && guard++ < big.Count) bi++;
                var target = big[bi % big.Count].part;
                bi++;
                target.totalInnerCount++;
                target.innerColorTypes.Add(InnerColorType.Fixed);
                target.innerFixedColors.Add(c);
                EditorUtility.SetDirty(target);
                placed++;
            }

            holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            int newMax = parts.Max(p => p.totalInnerCount);
            report = $"[OK] {row.levelName}: dồn {placed} inner sang {big.Count} part to (nhỏ giữ ≤{keep}). Sâu tối đa giờ {newMax}. Chạy Validate rồi test.";
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>
    /// Random lại màu inner Fixed giữa các part (ngoài giữ nguyên).
    /// Chỉ xáo vị trí màu — tổng từng màu không đổi nên %3 an toàn.
    /// </summary>
    private void RandomizeInners(DiffRow row)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var parts = root.GetComponentsInChildren<YarnPart>(true).ToList();
            var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null) { report = "[LỖI] Thiếu LevelYarnInfoHolder."; return; }
            foreach (var p in parts) NormalizeInnerLists(p);

            var slots = new List<(YarnPart part, int idx)>();
            foreach (var p in parts)
            {
                int lim = System.Math.Min(p.totalInnerCount, System.Math.Min(p.innerColorTypes.Count, p.innerFixedColors.Count));
                for (int j = 0; j < lim; j++)
                    if (p.innerColorTypes[j] == InnerColorType.Fixed)
                        slots.Add((p, j));
            }
            if (slots.Count == 0) { report = $"[OK] {row.levelName}: không có inner Fixed nào để random."; return; }

            var colors = slots.Select(s => s.part.innerFixedColors[s.idx]).ToList();
            var rng = new System.Random();
            for (int i = colors.Count - 1; i > 0; i--)
            {
                int k = rng.Next(i + 1);
                var tmp = colors[i];
                colors[i] = colors[k];
                colors[k] = tmp;
            }
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].part.innerFixedColors[slots[i].idx] = colors[i];
                EditorUtility.SetDirty(slots[i].part);
            }

            holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            report = $"[OK] {row.levelName}: đã xáo {slots.Count} inner Fixed giữa các part (ngoài + tổng màu giữ nguyên). Chạy Validate rồi test.";
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    private static void NormalizeInnerLists(YarnPart p)    {
        if (p.innerColorTypes == null) p.innerColorTypes = new List<InnerColorType>();
        if (p.innerFixedColors == null) p.innerFixedColors = new List<YarnColor>();
        while (p.innerColorTypes.Count < p.totalInnerCount) p.innerColorTypes.Add(InnerColorType.Random);
        while (p.innerFixedColors.Count < p.totalInnerCount) p.innerFixedColors.Add(p.initColor);
    }

    /// <summary>Đổi màu tay: N part ngoài (all=false) hoặc tất cả ngoài+inner+pool (all=true).</summary>
    private void RecolorManual(DiffRow row, bool all)
    {
        if (manualSrc == manualDst) { report = "[LỖI] Màu nguồn và màu đích trùng nhau."; return; }
        if (!all && manualCount <= 0) { report = "[LỖI] Số part phải > 0 (hoặc bấm Đổi TẤT CẢ)."; return; }
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
            if (db == null) { report = "[LỖI] Không load được YarnColorDatabase."; return; }
            if (db.GetMaterial(manualDst) == null) { report = $"[LỖI] Database thiếu yarn material cho {manualDst}."; return; }
            var parts = root.GetComponentsInChildren<YarnPart>(true).ToList();
            var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null) { report = "[LỖI] Thiếu LevelYarnInfoHolder."; return; }

            int moved;
            if (all)
            {
                moved = RecolorAll(parts, holder, db, manualSrc, manualDst);
                if (moved == 0) { report = $"[LỖI] Không tìm thấy sợi nào màu {manualSrc}."; return; }
            }
            else
            {
                var cands = parts.Where(p => p.initColor == manualSrc).OrderBy(p => p.totalInnerCount).Take(manualCount).ToList();
                if (cands.Count < manualCount) { report = $"[LỖI] Chỉ có {cands.Count} part ngoài màu {manualSrc}, cần {manualCount}."; return; }
                foreach (var p in cands)
                {
                    p.initColor = manualDst;
                    SwapSharedMaterial(p, db, manualSrc, manualDst);
                    EditorUtility.SetDirty(p);
                }
                moved = cands.Count;
            }

            holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            report = $"[OK] {row.levelName}: đổi {moved} sợi {manualSrc} -> {manualDst}." +
                (all ? "" : " (có thể lẻ %3 — chạy Validate để sửa)");
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    private void SetThresholdEdit(int a, int b, int c, int d)
    {
        thresholdEdit[0] = a; thresholdEdit[1] = b; thresholdEdit[2] = c; thresholdEdit[3] = d;
    }

    /// <summary>Cắt cụm số ở cuối tên level để sort theo số (Level2 trước Level10). Tên không có số xếp cuối.</summary>
    private static int LevelSortKey(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        int end = name.Length;
        int start = end;
        while (start > 0 && char.IsDigit(name[start - 1])) start--;
        if (start < end && int.TryParse(name.Substring(start, end - start), out int n)) return n;
        return int.MaxValue;
    }

    // ---------------- Check ----------------

    private void Scan()
    {
        rows.Clear();
        var sb = new StringBuilder();
        var paths = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .OrderBy(p => LevelSortKey(p)).ThenBy(p => p).ToList();
        foreach (var path in paths)
        {
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;
            var parts = go.GetComponentsInChildren<YarnPart>(true);
            if (parts.Length == 0) continue;
            var holder = go.GetComponent<LevelYarnInfoHolder>();
            if (holder == null) holder = go.GetComponentInChildren<LevelYarnInfoHolder>(true);

            var perColor = new Dictionary<YarnColor, int>();
            int randomSlots = 0, innerFixed = 0, maxDepth = 1;
            foreach (var p in parts)
            {
                AddCount(perColor, p.initColor, 1);
                int types = p.innerColorTypes != null ? p.innerColorTypes.Count : 0;
                int fixeds = p.innerFixedColors != null ? p.innerFixedColors.Count : 0;
                int lim = Math.Min(p.totalInnerCount, Math.Min(types, fixeds));
                for (int j = 0; j < lim; j++)
                {
                    if (p.innerColorTypes[j] == InnerColorType.Fixed)
                    {
                        AddCount(perColor, p.innerFixedColors[j], 1);
                        innerFixed++;
                    }
                    else randomSlots++;
                }
                maxDepth = Math.Max(maxDepth, 1 + p.totalInnerCount);
            }
            int pool = 0;
            if (holder != null && holder.randomColorEntries != null)
                foreach (var e in holder.randomColorEntries)
                {
                    AddCount(perColor, e.color, e.count);
                    pool += e.count;
                }
            int total = perColor.Values.Sum();
            float rShare = total > 0 ? (float)(randomSlots + pool) / (2 * total) : 0f;
            float score = perColor.Count * 8f + total * 0.25f + (maxDepth - 1) * 6f + rShare * 12f;
            string rating = score <= 70f ? "Dễ" : score <= 110f ? "Trung bình" : score <= 160f ? "Khó" : "Cực khó";
            rows.Add(new DiffRow
            {
                levelName = Path.GetFileNameWithoutExtension(path),
                path = path,
                colors = perColor.Count,
                total = total,
                outerCount = parts.Length,
                innerFixedCount = innerFixed,
                randomSlots = randomSlots,
                poolCount = pool,
                maxDepth = maxDepth,
                randomShare = rShare,
                score = score,
                rating = rating,
                thresholds = holder != null && holder.boxSpawnThresholds != null && holder.boxSpawnThresholds.Length == 4
                    ? (int[])holder.boxSpawnThresholds.Clone() : new int[4] { 20, 40, 15, 25 },
                perColor = perColor
            });
        }
        for (int i = 0; i < rows.Count; i++)
        {
            var r = rows[i];
            sb.AppendLine($"{r.levelName}: {r.rating} ({r.score:F0}) — {r.colors} màu, {r.total} sợi, sâu {r.maxDepth}, random {r.randomShare:P0}, nhịp [{string.Join(",", r.thresholds)}]");
        }
        report = sb.ToString();
        Debug.Log("[DoKho] Quét xong.\n" + report);
    }

    // ---------------- Sửa nhịp box ----------------

    private void ApplyThresholds(DiffRow row)
    {
        if (thresholdEdit.Sum() != 100)
        {
            report = $"[LỖI] Nhịp phải tổng = 100 (đang {thresholdEdit.Sum()}).";
            return;
        }
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null) { report = "[LỖI] Thiếu LevelYarnInfoHolder."; return; }
            holder.boxSpawnThresholds = (int[])thresholdEdit.Clone();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            report = $"[OK] {row.levelName}: nhịp box = [{string.Join(",", thresholdEdit)}].";
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    // ---------------- Giảm 1 màu (gộp 2 màu hiếm nhất) ----------------

    private void MergeRarest(DiffRow row)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
            var parts = root.GetComponentsInChildren<YarnPart>(true).ToList();
            var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null) { report = "[LỖI] Thiếu LevelYarnInfoHolder."; return; }

            var perColor = CountReal(parts, holder);
            var ordered = perColor.Where(kv => kv.Key != YarnColor.None && kv.Key != YarnColor.Rainbow)
                .OrderBy(kv => kv.Value).ToList();
            if (ordered.Count < 2) { report = "[LỖI] Level chỉ còn 1 màu, không gộp được."; return; }
            var keep = ordered[0].Key;   // giữ màu hiếm nhất
            var drop = ordered[1].Key;   // xóa màu hiếm thứ 2 (đổ vào màu giữ)
            // Cả 2 đều % 3 (validator đảm bảo) nên hợp lại vẫn % 3.
            int moved = RecolorAll(parts, holder, db, drop, keep);
            if (moved == 0) { report = "[LỖI] Không tìm thấy sợi nào để gộp."; return; }

            holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            report = $"[OK] {row.levelName}: gộp {drop} ({ordered[1].Value} sợi) vào {keep} — còn {ordered.Count - 1} màu. Chạy step 1 validator để kiểm tra lại.";
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    // ---------------- Tăng 1 màu (tách 3 sợi sang màu mới) ----------------

    private void SplitNewColor(DiffRow row)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
            if (db == null) { report = "[LỖI] Không load được YarnColorDatabase."; return; }
            var parts = root.GetComponentsInChildren<YarnPart>(true).ToList();
            var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
            if (holder == null) { report = "[LỖI] Thiếu LevelYarnInfoHolder."; return; }

            var perColor = CountReal(parts, holder);
            var src = perColor.Where(kv => kv.Key != YarnColor.None && kv.Key != YarnColor.Rainbow)
                .OrderByDescending(kv => kv.Value).FirstOrDefault();
            if (src.Value < 6) { report = $"[LỖI] Màu nhiều nhất ({src.Key}) chỉ có {src.Value} sợi, cần ≥ 6 để tách 3 mà phần còn lại vẫn đủ box."; return; }
            // Màu mới: chưa có trong level + đủ 3 material (chuyển đúng 3 sợi nên % 3 giữ nguyên).
            YarnColor dst = YarnColor.None;
            foreach (YarnColor c in Enum.GetValues(typeof(YarnColor)))
            {
                if (c == YarnColor.None || c == YarnColor.Rainbow || perColor.ContainsKey(c)) continue;
                if (db.GetMaterial(c) == null || db.GetMaterialBasket(c) == null || db.GetMaterialSlotString(c) == null) continue;
                dst = c;
                break;
            }
            if (dst == YarnColor.None) { report = "[LỖI] Không còn màu mới nào đủ material trong database."; return; }

            // Đổi đúng 3 sợi NGOÀI (không đụng inner) của màu nguồn, ưu tiên part không có inner.
            var cands = parts.Where(p => p.initColor == src.Key).OrderBy(p => p.totalInnerCount).Take(3).ToList();
            if (cands.Count < 3) { report = "[LỖI] Không đủ 3 part ngoài màu nguồn."; return; }
            foreach (var p in cands)
            {
                p.initColor = dst;
                SwapSharedMaterial(p, db, src.Key, dst);
                EditorUtility.SetDirty(p);
            }
            holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            report = $"[OK] {row.levelName}: tách 3 sợi {src.Key} thành màu mới {dst} " +
                     $"({string.Join(",", cands.Select(p => p.name))}). Chạy step 1 validator để kiểm tra lại.";
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    // ---------------- Helper đếm + đổi màu ----------------

    private static Dictionary<YarnColor, int> CountReal(List<YarnPart> parts, LevelYarnInfoHolder holder)
    {
        var real = new Dictionary<YarnColor, int>();
        foreach (var p in parts)
        {
            AddCount(real, p.initColor, 1);
            int lim = Math.Min(p.totalInnerCount, Math.Min(
                p.innerColorTypes != null ? p.innerColorTypes.Count : 0,
                p.innerFixedColors != null ? p.innerFixedColors.Count : 0));
            for (int j = 0; j < lim; j++)
                if (p.innerColorTypes[j] == InnerColorType.Fixed)
                    AddCount(real, p.innerFixedColors[j], 1);
        }
        if (holder != null && holder.randomColorEntries != null)
            foreach (var e in holder.randomColorEntries)
                AddCount(real, e.color, e.count);
        return real;
    }

    private static void AddCount(Dictionary<YarnColor, int> dict, YarnColor c, int n)
    {
        if (!dict.ContainsKey(c)) dict[c] = 0;
        dict[c] += n;
    }

    /// <summary>Đổi mọi xuất hiện của oldColor thành newColor (ngoài + inner Fixed + pool + material preview).</summary>
    private static int RecolorAll(List<YarnPart> parts, LevelYarnInfoHolder holder, YarnColorDatabase db, YarnColor oldColor, YarnColor newColor)
    {
        int moved = 0;
        foreach (var p in parts)
        {
            if (p.initColor == oldColor) { p.initColor = newColor; moved++; }
            if (p.innerFixedColors != null)
                for (int j = 0; j < p.innerFixedColors.Count; j++)
                    if (p.innerFixedColors[j] == oldColor) { p.innerFixedColors[j] = newColor; moved++; }
            SwapSharedMaterial(p, db, oldColor, newColor);
            EditorUtility.SetDirty(p);
        }
        if (holder != null && holder.randomColorEntries != null)
        {
            int poolOld = 0;
            foreach (var e in holder.randomColorEntries)
                if (e.color == oldColor) poolOld += e.count;
            holder.randomColorEntries.RemoveAll(e => e.color == oldColor);
            if (poolOld > 0)
            {
                holder.randomColorEntries.Add(new ColorCountEntry { color = newColor, count = poolOld });
                moved += poolOld;
            }
            EditorUtility.SetDirty(holder);
        }
        return moved;
    }

    private static void SwapSharedMaterial(YarnPart part, YarnColorDatabase db, YarnColor oldColor, YarnColor newColor)
    {
        // Ép toàn bộ slot về material của màu mới (đừng so sánh reference oldMat:
        // prefab cũ có thể đang cầm material lạ/instance nên so == sẽ trượt -> model lệch màu logic).
        var rend = part.GetComponent<MeshRenderer>();
        if (rend == null || db == null) return;
        var newMat = db.GetMaterial(newColor);
        if (newMat == null) return;
        var mats = rend.sharedMaterials;
        if (mats == null || mats.Length == 0) mats = new Material[1];
        for (int i = 0; i < mats.Length; i++)
            mats[i] = newMat;
        rend.sharedMaterials = mats;
    }

    /// <summary>
    /// Sửa các level đã bị lệch trước đây: ép mọi MeshRenderer về đúng material
    /// của initColor trong Database (không đổi logic sợi nào).
    /// </summary>
    private void ResyncModelMaterials(DiffRow row)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
            if (db == null) { report = "[LỖI] Không load được YarnColorDatabase."; return; }
            var parts = root.GetComponentsInChildren<YarnPart>(true);
            int fixedCount = 0;
            foreach (var p in parts)
            {
                var mat = db.GetMaterial(p.initColor);
                if (mat == null) continue;
                var rend = p.GetComponent<MeshRenderer>();
                if (rend == null) continue;
                var mats = rend.sharedMaterials;
                if (mats == null || mats.Length == 0) mats = new Material[1];
                bool need = false;
                for (int i = 0; i < mats.Length; i++)
                    if (mats[i] != mat) { mats[i] = mat; need = true; }
                if (need) { rend.sharedMaterials = mats; EditorUtility.SetDirty(p); fixedCount++; }
            }
            PrefabUtility.SaveAsPrefabAsset(root, row.path);
            AssetDatabase.SaveAssets();
            report = $"[OK] {row.levelName}: đã đồng bộ material cho {fixedCount}/{parts.Length} part theo initColor. " +
                     $"Mở lại prefab kiểm tra, rồi Extract lại matrix nếu cần.";
            Debug.Log(report);
            Scan();
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>
    /// Đồng bộ 1 chạm: mọi level ép mesh về đúng material DB 21 màu theo initColor.
    /// Logic sợi (initColor/inner/pool) GIỮ NGUYÊN — chỉ sửa phần nhìn + báo thiếu material.
    /// </summary>
    private void ResyncAllLevels()
    {
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
        if (db == null) { report = "[LỖI] Không load được YarnColorDatabase."; return; }
        var sb = new StringBuilder();
        sb.AppendLine("[ĐỒNG BỘ 21 MÀU] DB = " + DatabasePath);
        int totalFixed = 0, totalLevels = 0;
        foreach (var r in rows)
        {
            GameObject root = null;
            try
            {
                root = PrefabUtility.LoadPrefabContents(r.path);
                var parts = root.GetComponentsInChildren<YarnPart>(true);
                var holder = root.GetComponent<LevelYarnInfoHolder>() ?? root.GetComponentInChildren<LevelYarnInfoHolder>(true);
                int fixedCount = 0;
                var missing = new HashSet<YarnColor>();
                foreach (var p in parts)
                {
                    // initColor phải nằm trong 21 màu có yarn material.
                    var yarnMat = db.GetMaterial(p.initColor);
                    if (yarnMat == null) { missing.Add(p.initColor); continue; }
                    if (db.GetMaterialBasket(p.initColor) == null) missing.Add(p.initColor);
                    if (db.GetMaterialSlotString(p.initColor) == null) missing.Add(p.initColor);
                    var rend = p.GetComponent<MeshRenderer>();
                    if (rend == null) continue;
                    var mats = rend.sharedMaterials;
                    if (mats == null || mats.Length == 0) mats = new Material[1];
                    bool need = false;
                    for (int i = 0; i < mats.Length; i++)
                        if (mats[i] != yarnMat) { mats[i] = yarnMat; need = true; }
                    if (need) { rend.sharedMaterials = mats; EditorUtility.SetDirty(p); fixedCount++; }
                    // inner Fixed + pool: chỉ kiểm tra có material, không tự đổi màu logic.
                    if (p.innerFixedColors != null)
                        foreach (var c in p.innerFixedColors)
                            if (db.GetMaterial(c) == null) missing.Add(c);
                }
                if (holder != null && holder.randomColorEntries != null)
                    foreach (var e in holder.randomColorEntries)
                        if (db.GetMaterial(e.color) == null) missing.Add(e.color);
                if (holder != null)
                {
                    holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
                    holder.UpdateDebugInfo();
                    EditorUtility.SetDirty(holder);
                }
                PrefabUtility.SaveAsPrefabAsset(root, r.path);
                totalFixed += fixedCount;
                totalLevels++;
                sb.AppendLine($"  - {r.levelName}: {fixedCount}/{parts.Length} part đồng bộ" +
                    (missing.Count > 0 ? $" | THIẾU material: {string.Join(",", missing)}" : ""));
            }
            catch (Exception ex)
            {
                sb.AppendLine($"  - {r.levelName}: [LỖI] {ex.Message}");
            }
            finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
        }
        AssetDatabase.SaveAssets();
        sb.AppendLine($"[XONG] {totalLevels} levels, {totalFixed} part đã ép về material DB. Chạy validator step 1 rồi Extract lại matrix.");
        report = sb.ToString();
        Debug.Log(report);
        Scan();
    }

    /// <summary>
    /// Chẩn đoán lệch màu từng part: initColor (logic) vs material đang gắn trên mesh
    /// (nhìn thấy) vs material chuẩn trong Database. Chạy cái này TRƯỚC khi sửa.
    /// </summary>
    private void DiagnoseColors(DiffRow row)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(DatabasePath);
            if (db == null) { report = "[LỖI] Không load được YarnColorDatabase."; return; }
            var sb = new StringBuilder();
            sb.AppendLine($"[CHẨN ĐOÁN] {row.levelName} (db: {DatabasePath})");
            sb.AppendLine("Logic lúc CHẠY = initColor + database của YarnColorManager trong scene (Awake gán lại material).");
            sb.AppendLine("Prefab sharedMaterial chỉ là cái NHÌN trong Editor.");
            var parts = root.GetComponentsInChildren<YarnPart>(true);
            int mismatch = 0;
            foreach (var p in parts)
            {
                var dbMat = db.GetMaterial(p.initColor);
                var rend = p.GetComponent<MeshRenderer>();
                string meshMats = "-";
                string resolved = "-";
                bool ok = true;
                if (rend != null)
                {
                    var mats = rend.sharedMaterials;
                    var names = new List<string>();
                    var res = new List<string>();
                    if (mats != null)
                        foreach (var m in mats)
                        {
                            names.Add(m != null ? m.name : "null");
                            if (m != null && db.TryGetColor(m, out var rc)) res.Add(rc.ToString());
                            else res.Add("?");
                        }
                    meshMats = string.Join(",", names);
                    resolved = string.Join(",", res.Distinct());
                    // Lệch nếu mesh đang cầm material không map về initColor.
                    ok = mats != null && mats.Length > 0 && mats.All(m => m != null && m == dbMat);
                }
                if (!ok) mismatch++;
                sb.AppendLine($"  {(ok ? "[OK]" : "[LỆCH]")} {p.name}: init={p.initColor} | mesh=[{meshMats}] -> {resolved} | db={(dbMat != null ? dbMat.name : "THIẾU_DB")}");
            }
            sb.AppendLine(mismatch == 0
                ? "=> Model khớp initColor. Nếu lúc CHẠY vẫn sai: initColor đang sai (sửa logic), hoặc database trong scene khác file này, hoặc database gán nhầm cặp màu-material."
                : $"=> Có {mismatch} part mesh lệch initColor. Bấm Đồng bộ material để sửa phần nhìn. Nếu sau đó lúc CHẠY vẫn sai thì initColor mới là cái sai.");
            report = sb.ToString();
            Debug.Log(report);
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    /// <summary>
    /// Quét tràn mesh: mô phỏng đúng runtime hiện tại (inner giữ nguyên vị trí gốc,
    /// chỉ thu scale theo growScale/initialInnerScale), check bằng HÌNH MESH thật
    /// (bounds trước, raycast parity trên đỉnh thật cho ca nghi ngờ).
    /// </summary>
    private void DiagnoseMeshOverflow(DiffRow row)
    {
        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(row.path);
            var cfg = AssetDatabase.LoadAssetAtPath<GameConfig>("Assets/_Game/Configs/GameConfig.asset");
            float grow = cfg != null ? cfg.growScale : 0.72f;
            float init = cfg != null ? cfg.initialInnerScale : 0.9f;
            var parts = root.GetComponentsInChildren<YarnPart>(true).Where(p => p.layerIndex == 1 && p.totalInnerCount > 0).ToList();
            var sb = new StringBuilder();
            sb.AppendLine($"[TRÀN MESH] {row.levelName}: {parts.Count} part có inner (grow={grow}, initInner={init})");
            int checkedCount = 0, overflowCount = 0;
            foreach (var p in parts)
            {
                var mf = p.GetComponent<MeshFilter>();
                var mesh = mf != null ? mf.sharedMesh : null;
                if (mesh == null) { sb.AppendLine($"  [?] {p.name}: không có mesh."); continue; }
                if (!mesh.isReadable)
                {
                    sb.AppendLine($"  [?] {p.name}: mesh chưa Readable (bật Read/Write trong import settings để check đỉnh).");
                    continue;
                }
                // Khung chứa ancestor (local frame, pivot tại gốc).
                var b0 = mesh.bounds;
                for (int layer = 2; layer <= p.totalInnerCount + 1; layer++)
                {
                    float fSpawn = Mathf.Pow(grow, layer - 1) * init;
                    float fGrow = Mathf.Pow(grow, layer - 1);
                    foreach (var f in new[] { fSpawn, fGrow })
                    {
                        checkedCount++;
                        // Tầng 1: bounds chứa?
                        var bi = new Bounds(b0.center * f, b0.size * f);
                        bool inBounds = b0.Contains(bi.min) && b0.Contains(bi.max);
                        if (inBounds) continue;
                        // Tầng 2: đếm đỉnh thật lọt ra ngoài (parity raycast, mẫu 200 đỉnh).
                        var res = CountOutsideVerts(mesh, f, 200);
                        if (res.outside > res.tested * 0.02f)
                        {
                            overflowCount++;
                            sb.AppendLine($"  [TRÀN] {p.name} L{layer} f={f:F3}: {res.outside}/{res.tested} đỉnh ngoài vỏ " +
                                $"({(f == fGrow ? "lúc grow" : "lúc spawn")}) — thu growScale hoặc sửa mesh.");
                        }
                    }
                }
            }
            sb.AppendLine($"=> đã check {checkedCount} lượt inner, {overflowCount} ca tràn mesh thật.");
            if (overflowCount == 0)
                sb.AppendLine("Không tràn theo hình mesh — chồng nhìn thấy là do model xếp sát (bounds chạm), không phải ruột lòi vỏ.");
            report = sb.ToString();
            Debug.Log(report);
        }
        finally { if (root != null) PrefabUtility.UnloadPrefabContents(root); }
    }

    private struct VertCheck
    {
        public int outside;
        public int tested;
    }

    /// <summary>
    /// Đếm đỉnh của mesh (scale f quanh pivot) nằm ngoài mesh gốc bằng parity raycast.
    /// </summary>
    private static VertCheck CountOutsideVerts(Mesh mesh, float f, int maxSamples)
    {
        var res = new VertCheck();
        var verts = mesh.vertices;
        var tris = mesh.triangles;
        if (verts == null || verts.Length == 0 || tris == null || tris.Length == 0) return res;
        int step = Mathf.Max(1, verts.Length / Mathf.Max(1, maxSamples));
        for (int i = 0; i < verts.Length; i += step)
        {
            var q = verts[i] * f;
            res.tested++;
            if (!PointInMesh(q, verts, tris)) res.outside++;
        }
        return res;
    }

    /// <summary>Parity: tia +X từ p cắt mặt tam giác lẻ lần thì p nằm trong.</summary>
    private static bool PointInMesh(Vector3 p, Vector3[] verts, int[] tris)
    {
        int hits = 0;
        var dir = new Vector3(1f, 0.12345f, 0.23456f).normalized; // xiên nhẹ tránh trúng cạnh
        for (int t = 0; t < tris.Length; t += 3)
        {
            if (RayTri(p, dir, verts[tris[t]], verts[tris[t + 1]], verts[tris[t + 2]], out float dist)
                && dist > 1e-5f) hits++;
        }
        return (hits & 1) == 1;
    }

    /// <summary>Möller–Trumbore, có cull mặt sau theo hướng tia.</summary>
    private static bool RayTri(Vector3 orig, Vector3 dir, Vector3 v0, Vector3 v1, Vector3 v2, out float t)
    {
        t = 0f;
        var e1 = v1 - v0;
        var e2 = v2 - v0;
        var pvec = Vector3.Cross(dir, e2);
        float det = Vector3.Dot(e1, pvec);
        if (det > -1e-8f && det < 1e-8f) return false;
        float inv = 1f / det;
        var tvec = orig - v0;
        float u = Vector3.Dot(tvec, pvec) * inv;
        if (u < 0f || u > 1f) return false;
        var qvec = Vector3.Cross(tvec, e1);
        float v = Vector3.Dot(dir, qvec) * inv;
        if (v < 0f || u + v > 1f) return false;
        t = Vector3.Dot(e2, qvec) * inv;
        return t >= 0f;
    }
}
