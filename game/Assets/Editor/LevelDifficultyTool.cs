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
    private string report = "Bấm \"Quét độ khó\" để chấm điểm tất cả level.";
    private readonly List<DiffRow> rows = new List<DiffRow>();

    private int selectedTab;
    private int selectedLevelIndex;
    private int detailLastLoaded = -1;
    private readonly int[] thresholdEdit = new int[4] { 20, 40, 15, 25 };

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

    [MenuItem("Tools/Puzzle/7. Do kho level (check + sua)", false, 7)]
    private static void Open()
    {
        var w = GetWindow<LevelDifficultyTool>("Do kho level");
        w.minSize = new Vector2(720, 560);
        w.Scan();
    }

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
        EditorGUILayout.TextArea(report, GUILayout.MinHeight(80));
    }

    // ---------------- Tab 1: tổng quan độ khó ----------------

    private void DrawOverviewTab()
    {
        EditorGUILayout.HelpBox(
            "Độ khó nên tăng dần theo level — dòng nào tụt điểm sẽ báo.",
            MessageType.None);
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
        if (GUILayout.Button($"Áp nhịp box cho {sel.levelName} (đang [{string.Join(",", sel.thresholds)}])", GUILayout.Height(28)))
            ApplyThresholds(sel);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Giảm 1 màu (gộp 2 màu hiếm nhất)", GUILayout.Height(28))) MergeRarest(sel);
        if (GUILayout.Button("Tăng 1 màu (tách 3 sợi sang màu mới)", GUILayout.Height(28))) SplitNewColor(sel);
        EditorGUILayout.EndHorizontal();
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
        var rend = part.GetComponent<MeshRenderer>();
        if (rend == null || db == null) return;
        var oldMat = db.GetMaterial(oldColor);
        var newMat = db.GetMaterial(newColor);
        if (newMat == null) return;
        var mats = rend.sharedMaterials;
        bool changed = false;
        for (int i = 0; i < mats.Length; i++)
            if (mats[i] == oldMat) { mats[i] = newMat; changed = true; }
        if (changed) rend.sharedMaterials = mats;
    }
}
