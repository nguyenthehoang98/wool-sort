using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using _Game.Scripts;
using _Game.Scripts.Gameplay;

public enum PromptStyle { Detailed, Minimal, Technical }

[Serializable]
public class SavedPromptData
{
    public string promptTemplate = @"# RANDOM 28×28 CELL PIXEL ART

Create **one random cute cartoon/chibi pixel-art illustration**.

The subject, objects, composition, and visual arrangement are completely random. You may create animals, characters, monsters, plants, food, toys, objects, scenery, fantasy creatures, or any other suitable subject.

The artwork must follow the **exact 28×28 logical cell grid** and the color requirements provided below.

---

## 1. EXACT 28×28 LOGICAL GRID

The artwork must be constructed on exactly:

**28 columns × 28 rows**

Each position in this grid represents exactly **one logical cell**.

Rules:

- Exactly 28 cells horizontally.
- Exactly 28 cells vertically.
- Every visual element must align to this grid.
- Each logical cell contains exactly one solid color.
- Do not create shapes smaller than one logical cell.
- Do not place details between cells.
- Do not use sub-cell positioning.
- Do not change the grid size.

The artwork must look as if it was created directly on a **28×28 logical pixel grid**.

---

## 2. COLOR REQUIREMENTS

The number of colors, the colors themselves, and the required number of cells for each color are specified below:

**{0}**

These color requirements are **exact**.

For every color specified above:

- Use exactly the specified color.
- Use exactly the specified number of logical cells.
- Do not add additional colors.
- Do not remove any required color cells.
- Do not substitute one color for another.
- Every one of the 28×28 cells must belong to one of the specified colors.

The specified cell counts must add up to exactly **784 cells**.

Do not interpret the specified cell counts as percentages or approximate values.

---

## 3. RANDOM CONTENT

The content is intentionally random.

Possible subjects include, but are not limited to:

- animals
- cute characters
- monsters
- plants
- flowers
- food
- toys
- buildings
- objects
- vehicles
- fantasy creatures
- scenery
- combinations of multiple objects

Each generation may use a completely different subject and composition.

Do not repeatedly use the same subject unless it happens randomly.

The subject and composition are flexible, but must remain readable within the **28×28 grid**.

---

## 4. PIXEL ART STYLE

Use a:

**cute + cartoon/chibi + pixel-art style**

The visual design should be:

- cute
- playful
- readable
- simplified
- suitable for a casual game
- clearly constructed from logical cells

The exact shapes, proportions, composition, and detail level may vary between generations.

Do not prioritize visual complexity over the required grid and color constraints.

---

## 5. GRID-BASED CONSTRUCTION

Think of the artwork as a matrix:

```text
28 × 28 cells
```

Each cell is an independent color unit.

All shapes, outlines, patterns, and details must be constructed by placing colors into these cells.

Do not create a smooth illustration first and then apply a pixel-art effect.

---

## 6. PRIORITY ORDER

If requirements conflict, follow this priority:

1. **Exactly 28×28 logical cells**
2. **Exactly the specified color-cell counts in {0}**
3. **Use only the colors specified in {0}**
4. Cute cartoon/chibi pixel-art appearance
5. Random subject and composition
6. Visual detail and attractiveness

Never sacrifice the grid or exact color requirements for visual quality.

---

## FINAL REQUIREMENT

Create **one random cute cartoon/chibi pixel-art illustration** directly on an **exact 28×28 logical cell grid**.

Follow the color palette and exact cell counts specified in:

**{0}**

Every one of the 784 logical cells must contain exactly one of the specified colors.

No additional colors, no missing cells, and no changed grid dimensions.";
    public string promptExample = @"# RANDOM 28×28 CELL PIXEL ART

Create **one random cute cartoon/chibi pixel-art illustration**.

The subject, objects, composition, and visual arrangement are completely random. You may create animals, characters, monsters, plants, food, toys, objects, scenery, fantasy creatures, or any other suitable subject.

The artwork must follow the **exact 28×28 logical cell grid** and the color requirements provided below.

---

## 1. EXACT 28×28 LOGICAL GRID

The artwork must be constructed on exactly:

**28 columns × 28 rows**

Each position in this grid represents exactly **one logical cell**.

Rules:

- Exactly 28 cells horizontally.
- Exactly 28 cells vertically.
- Every visual element must align to this grid.
- Each logical cell contains exactly one solid color.
- Do not create shapes smaller than one logical cell.
- Do not place details between cells.
- Do not use sub-cell positioning.
- Do not change the grid size.

The artwork must look as if it was created directly on a **28×28 logical pixel grid**.

---

## 2. COLOR REQUIREMENTS

The number of colors, the colors themselves, and the required number of cells for each color are specified below:

**Number of colors: 5**

- `#1E1E2E` — exactly **120 cells**
- `#F2F0E6` — exactly **280 cells**
- `#E85D75` — exactly **160 cells**
- `#4DA6FF` — exactly **140 cells**
- `#72C472` — exactly **84 cells**

**Total: 784 cells**

These color requirements are **exact**.

For every color specified above:

- Use exactly the specified color.
- Use exactly the specified number of logical cells.
- Do not add additional colors.
- Do not remove any required color cells.
- Do not substitute one color for another.
- Every one of the 28×28 cells must belong to one of the specified colors.

Do not interpret the cell counts as percentages or approximate values.

---

## 3. RANDOM CONTENT

The content is intentionally random.

Possible subjects include, but are not limited to:

- animals
- cute characters
- monsters
- plants
- flowers
- food
- toys
- buildings
- objects
- vehicles
- fantasy creatures
- scenery
- combinations of multiple objects

Each generation may use a completely different subject and composition.

Do not repeatedly use the same subject unless it happens randomly.

The subject and composition are flexible, but must remain readable within the **28×28 grid**.

---

## 4. PIXEL ART STYLE

Use a:

**cute + cartoon/chibi + pixel-art style**

The visual design should be:

- cute
- playful
- readable
- simplified
- suitable for a casual game
- clearly constructed from logical cells

The exact shapes, proportions, composition, and detail level may vary between generations.

Do not prioritize visual complexity over the required grid and color constraints.

---

## 5. GRID-BASED CONSTRUCTION

Think of the artwork as a matrix:

```text
28 × 28 cells
```

Each cell is an independent color unit.

All shapes, outlines, patterns, and details must be constructed by placing colors into these cells.

Do not create a smooth illustration first and then apply a pixel-art effect.

---

## 6. PRIORITY ORDER

If requirements conflict, follow this priority:

1. **Exactly 28×28 logical cells**
2. **Exactly the specified color-cell counts**
3. **Use only the specified colors**
4. Cute cartoon/chibi pixel-art appearance
5. Random subject and composition
6. Visual detail and attractiveness

Never sacrifice the grid or exact color requirements for visual quality.

---

## FINAL REQUIREMENT

Create **one random cute cartoon/chibi pixel-art illustration** directly on an **exact 28×28 logical cell grid**.

Use exactly these 5 colors:

`#1E1E2E`
`#F2F0E6`
`#E85D75`
`#4DA6FF`
`#72C472`

Use exactly the specified number of cells for each color.

Every one of the **784 logical cells** must contain exactly one of these colors.

No additional colors, no missing cells, and no changed grid dimensions.";
    public PromptStyle style = PromptStyle.Detailed;
    public int imageWidth = 784;
    public int imageHeight = 784;
}

public class ColorPromptWindow : EditorWindow
{
    private const string LevelsPath = "Assets/Resources/Levels";
    private const string SavedDataPath = "Assets/ColorPromptSettings.json";

    private List<LevelColorStats> _levels = new List<LevelColorStats>();
    private SavedPromptData _savedData = new SavedPromptData();
    private Vector2 _scrollPosition;
    private string _search = "";

    // Studio tự tạo panel, không còn menu riêng.
    private bool initialized;

    /// <summary>Gọi 1 lần khi Studio mở tab (thay ShowWindow/OnEnable cũ).</summary>
    public void InitPanel()
    {
        if (initialized) return;
        initialized = true;
        LoadData();
        RefreshLevels();
    }

    public void DrawPanel() => OnGUI();

    private void LoadData()
    {
        try
        {
            if (File.Exists(SavedDataPath))
            {
                var json = File.ReadAllText(SavedDataPath);
                var data = JsonUtility.FromJson<SavedPromptData>(json);
                if (data != null && !string.IsNullOrEmpty(data.promptTemplate)) _savedData = data;
            }
        }
        catch {}
    }

    private void SaveData()
    {
        try
        {
            var json = JsonUtility.ToJson(_savedData, true);
            File.WriteAllText(SavedDataPath, json);
            AssetDatabase.Refresh();
        }
        catch (Exception e) { Debug.LogWarning(e.Message); }
    }

    private void RefreshLevels()
    {
        _levels.Clear();
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsPath });
        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var stats = ColorUsageTracker.CollectStatsForPrefab(path);
            if (stats != null) _levels.Add(stats);
        }
        _levels = _levels.OrderBy(s => LevelSortKey(s.levelName)).ThenBy(s => s.levelName).ToList();
    }

    /// <summary>Sort theo số cuối tên (Level2 trước Level10). Tên không có số xếp cuối.</summary>
    private static int LevelSortKey(string name)
    {
        if (string.IsNullOrEmpty(name)) return int.MaxValue;
        int end = name.Length, start = end;
        while (start > 0 && char.IsDigit(name[start - 1])) start--;
        if (start < end && int.TryParse(name.Substring(start, end - start), out int n)) return n;
        return int.MaxValue;
    }

    private void OnGUI()
    {
        // PINNED TOP AREA
        EditorGUILayout.Space(4);
        GUILayout.Label("Color Prompt Generator - Chinh xac theo YarnPart", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Prompt chung o tren se duoc pin va luu lai. Moi level ben duoi co nut Copy rieng. Ti le mau tinh chinh xac theo YarnPart (outer + fixed-inner + random) giong LevelYarnInfoHolder.UpdateDebugInfo, khop voi model/material.", MessageType.Info);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Prompt Template - {0} = khoi yeu cau mau (tu dien theo tung level):", EditorStyles.miniBoldLabel);
        if (!_savedData.promptTemplate.Contains("{0}"))
        {
            EditorGUILayout.HelpBox("Template thieu placeholder {0}. Bam 'Reset Pixel Template' de dung mau moi.", MessageType.Warning);
        }
        EditorGUI.BeginChangeCheck();
        _savedData.promptTemplate = EditorGUILayout.TextArea(_savedData.promptTemplate, GUILayout.Height(110));
        if (EditorGUI.EndChangeCheck()) SaveData();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Style:", GUILayout.Width(40));
        EditorGUI.BeginChangeCheck();
        _savedData.style = (PromptStyle)EditorGUILayout.EnumPopup(_savedData.style, GUILayout.Width(120));
        if (EditorGUI.EndChangeCheck()) SaveData();
        GUILayout.Label("W:", GUILayout.Width(20)); EditorGUI.BeginChangeCheck(); _savedData.imageWidth = EditorGUILayout.IntField(_savedData.imageWidth, GUILayout.Width(60)); if (EditorGUI.EndChangeCheck()) SaveData();
        GUILayout.Label("H:", GUILayout.Width(15)); EditorGUI.BeginChangeCheck(); _savedData.imageHeight = EditorGUILayout.IntField(_savedData.imageHeight, GUILayout.Width(60)); if (EditorGUI.EndChangeCheck()) SaveData();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Reset Pixel Template", GUILayout.Width(140)))
        {
            _savedData.promptTemplate = new SavedPromptData().promptTemplate;
            _savedData.imageWidth = 784; _savedData.imageHeight = 784;
            SaveData();
        }
        if (GUILayout.Button("Luu", GUILayout.Width(50))) { SaveData(); EditorUtility.DisplayDialog("Da luu", $"Da luu vao {SavedDataPath}", "OK"); }
        if (GUILayout.Button("Refresh", GUILayout.Width(60))) RefreshLevels();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Ban vi du da dien mau san (copy gui kem AI / tham khao):", EditorStyles.miniBoldLabel);
        EditorGUI.BeginChangeCheck();
        _savedData.promptExample = EditorGUILayout.TextArea(_savedData.promptExample ?? "", GUILayout.Height(110));
        if (EditorGUI.EndChangeCheck()) SaveData();
        EditorGUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Copy vi du", GUILayout.Width(100))) { GUIUtility.systemCopyBuffer = _savedData.promptExample ?? ""; }
        if (GUILayout.Button("Reset vi du", GUILayout.Width(100)))
        {
            _savedData.promptExample = new SavedPromptData().promptExample;
            SaveData();
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label($"Levels: {_levels.Count} | Tong mau: {_levels.SelectMany(s=>s.colorCounts).GroupBy(kv=>kv.Key).Count()} mau", EditorStyles.miniLabel);
        GUILayout.FlexibleSpace();
        GUILayout.Label("Search:", GUILayout.Width(50));
        _search = EditorGUILayout.TextField(_search, GUILayout.Width(120));
        EditorGUILayout.EndHorizontal();

        // SCROLL LIST
        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        var filtered = string.IsNullOrEmpty(_search) ? _levels : _levels.Where(s => s.levelName.IndexOf(_search, StringComparison.OrdinalIgnoreCase) >= 0).ToList();

        for (int i = 0; i < filtered.Count; i++)
        {
            var stats = filtered[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            // Header (đánh số thứ tự để chọn nhanh)
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"{i + 1}. {stats.levelName}", EditorStyles.boldLabel, GUILayout.Width(130));
            GUILayout.Label($"outer:{stats.outerPartCount} spawn:{stats.totalSpawnCount} mau:{stats.totalOccurrences}", EditorStyles.miniLabel, GUILayout.Width(180));
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Copy Prompt", GUILayout.Width(100), GUILayout.Height(22)))
            {
                var prompt = GeneratePrompt(stats);
                GUIUtility.systemCopyBuffer = prompt;
                EditorUtility.DisplayDialog("Da copy", $"Prompt cho {stats.levelName} da copy:\n\n{prompt}", "OK");
                Debug.Log($"[Prompt] {stats.levelName}:\n{prompt}");
            }
            EditorGUILayout.EndHorizontal();

            // Color breakdown with % - chinh xac
            if (stats.colorCounts.Count == 0)
            {
                GUILayout.Label("Khong co mau", EditorStyles.miniLabel);
            }
            else
            {
                // Sort by count desc
                var sorted = stats.colorCounts.OrderByDescending(kv => kv.Value).ToList();
                EditorGUILayout.BeginVertical();
                foreach (var kv in sorted)
                {
                    float pct = stats.totalOccurrences > 0 ? (float)kv.Value / stats.totalOccurrences * 100f : 0f;
                    // bar
                    EditorGUILayout.BeginHorizontal();
                    // Mau
                    GUILayout.Label($"{kv.Key} ({(int)kv.Key})", GUILayout.Width(110));
                    GUILayout.Label($"{kv.Value} lan", GUILayout.Width(55));
                    GUILayout.Label($"{pct:F1}%", GUILayout.Width(50));
                    // visual bar
                    var rect = GUILayoutUtility.GetRect(0, 12, GUILayout.ExpandWidth(true));
                    EditorGUI.ProgressBar(rect, pct/100f, "");
                    EditorGUILayout.EndHorizontal();
                }
                EditorGUILayout.EndVertical();

                // preview distribution string
                string dist = string.Join(", ", sorted.Select(kv => $"{kv.Key} {(float)kv.Value/stats.totalOccurrences*100f:F1}% ({kv.Value})"));
                GUILayout.Label($"Phan bo: {dist}", EditorStyles.wordWrappedMiniLabel);
                GUILayout.Label($"Cau truc anh: {stats.totalOccurrences} o, {stats.colorCounts.Count} mau, ti le diem anh sap si % mau", EditorStyles.miniLabel);
            }

            // Preview prompt
            var preview = GeneratePrompt(stats);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Preview:", GUILayout.Width(50));
            EditorGUILayout.SelectableLabel(preview, EditorStyles.wordWrappedMiniLabel, GUILayout.Height(36));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4);
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>Hex chuẩn từng YarnColor (đọc _MainColor trong Database, không gộp).</summary>
    private static string GetHexExact(YarnColor c)
    {
        var db = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>("Assets/_Game/Configs/YarnColorDatabase.asset");
        var col = LevelColorMatrixUtility.RefColor(db, c);
        col.a = 1f;
        return "#" + ColorUtility.ToHtmlStringRGB(col);
    }

    private static string BuildColorParams(List<(string name, string hex, int balls)> merged, int totalBalls)
    {
        // Khối {0} cho template prompt: tỉ lệ sợi của level quy ra số cell, tổng đúng 784.
        const int totalCells = 28 * 28;
        var usable = merged.Where(m => m.balls > 0).OrderByDescending(m => m.balls).ToList();
        if (usable.Count == 0 || totalBalls <= 0)
            return "**Number of colors: 0**\n\n(no colors)\n\n**Total: 784 cells**";
        // Largest remainder để tổng đúng 784.
        var floors = new int[usable.Count];
        var rem = new double[usable.Count];
        int assigned = 0;
        for (int i = 0; i < usable.Count; i++)
        {
            double q = (double)usable[i].balls / totalBalls * totalCells;
            floors[i] = Mathf.FloorToInt((float)q);
            rem[i] = q - floors[i];
            assigned += floors[i];
        }
        int left = totalCells - assigned;
        var order = Enumerable.Range(0, usable.Count).OrderByDescending(i => rem[i]).ToList();
        for (int k = 0; k < left && k < order.Count; k++) floors[order[k]]++;
        var lines = new List<string> { $"**Number of colors: {usable.Count}**", "" };
        for (int i = 0; i < usable.Count; i++)
            lines.Add($"- `{usable[i].hex}` — exactly **{floors[i]} cells**");
        lines.Add("");
        lines.Add($"**Total: {totalCells} cells**");
        return string.Join("\n", lines);
    }

    private string GeneratePrompt(LevelColorStats stats)
    {
        var sorted = stats.colorCounts.OrderByDescending(kv => kv.Value).ToList();
        // Liệt kê đúng màu level đang có: mỗi YarnColor 1 dòng, hex chuẩn từ Database.
        // Không gộp cứng theo số lượng nào — level mấy màu thì prompt mấy màu.
        var merged = new List<(string name, string hex, int balls)>();
        foreach (var kv in sorted)
            merged.Add((kv.Key.ToString(), GetHexExact(kv.Key), kv.Value));
        // colorTable: markdown table matching spec section 4 (Color | HEX | Balls + TOTAL)
        string colorTable;
        if (merged.Count == 0)
        {
            colorTable = "| Color | HEX | Balls |\n|---|---|---:|\n| **TOTAL** | | **0** |";
        }
        else
        {
            var rows = new List<string>
            {
                "| Color | HEX | Balls |",
                "|---|---|---:|"
            };
            foreach (var m in merged)
                rows.Add($"| {m.name} | `{m.hex}` | **{m.balls}** |");
            rows.Add($"| **TOTAL** | | **{stats.totalOccurrences}** |");
            colorTable = string.Join("\n", rows);
        }
        // colorDistribution: short form for spec lines, e.g. "24 Green + 9 Orange + 3 Gray = 48"
        string colorDist = merged.Count == 0 ? "no yarn" : string.Join(" + ", merged.Select(m => $"{m.balls} {m.name}")) + $" = {stats.totalOccurrences}";
        // colorParams: khối {0} cho template prompt (legacy placeholders khác giữ nguyên để tương thích)
        string colorParams = BuildColorParams(merged, stats.totalOccurrences);
        // colorList: "Green, Blue, Pink"
        string colorList = sorted.Count == 0 ? "none" : string.Join(", ", sorted.Select(kv => $"{kv.Key} {GetHexExact(kv.Key)}"));
        // filteredPalette: chi lay mau co xuat hien, kem ti le % va so luong
        string filteredPalette = sorted.Count == 0 ? "- none" : string.Join("\n", sorted.Select(kv => {
            float pct = stats.totalOccurrences > 0 ? (float)kv.Value / stats.totalOccurrences * 100f : 0f;
            return $"- {kv.Key}: {GetHexExact(kv.Key)} - {pct:F1}% ({kv.Value} balls)";
        }));
        // pixelDistribution: tinh pixel dua tren 784 logical pixels (legacy, template static hien khong dung)
        const int totalLogical = 28 * 28; // 784
        string pixelDist;
        if (sorted.Count == 0) pixelDist = "- No colors";
        else
        {
            // tinh so pixel lam tron, dieu chinh de tong =784
            var pixels = new List<int>();
            int sum = 0;
            foreach (var kv in sorted)
            {
                float pct = (float)kv.Value / stats.totalOccurrences;
                int px = Mathf.RoundToInt(pct * totalLogical);
                pixels.Add(px); sum += px;
            }
            int diff = totalLogical - sum;
            if (pixels.Count > 0 && diff != 0) pixels[0] += diff; // dieu chinh mau lon nhat

            var lines = new List<string>();
            for (int i=0;i<sorted.Count;i++)
            {
                var kv = sorted[i];
                float pct = (float)kv.Value / stats.totalOccurrences * 100f;
                int logical = pixels[i];
                int physical = logical * 28 * 28; // moi logical = 784 physical pixels
                string hex = GetHexExact(kv.Key);
                lines.Add($"- {kv.Key} {hex}: {pct:F1}% ({kv.Value} balls) = {logical} logical pixels ({logical} x 28x28 = {physical} physical pixels)");
            }
            lines.Add($"- Total: {totalLogical} logical pixels = 784x784 physical canvas");
            pixelDist = string.Join("\n", lines);
        }

        string styleDesc = _savedData.style switch
        {
            PromptStyle.Detailed => "highly detailed chunky pixel art, bright cheerful saturated",
            PromptStyle.Minimal => "minimalist clean flat pixel blocks",
            PromptStyle.Technical => "technical grid diagram, labeled color blocks",
            _ => "detailed"
        };

        string prompt = _savedData.promptTemplate
            .Replace("{0}", colorParams)
            .Replace("{colorTable}", colorTable)
            .Replace("{filteredPalette}", filteredPalette)
            .Replace("{colorDistribution}", colorDist)
            .Replace("{pixelDistribution}", pixelDist)
            .Replace("{colorList}", colorList)
            .Replace("{colorCount}", stats.colorCounts.Count.ToString())
            .Replace("{totalCells}", stats.totalOccurrences.ToString())
            .Replace("{outerCount}", stats.outerPartCount.ToString())
            .Replace("{totalSpawns}", stats.totalSpawnCount.ToString())
            .Replace("{totalLogicalPixels}", totalLogical.ToString())
            .Replace("{style}", styleDesc)
            .Replace("{levelName}", stats.levelName)
            .Replace("{colorName}", sorted.Count>0 ? sorted[0].Key.ToString() : "Mixed")
            .Replace("{count}", stats.totalOccurrences.ToString());

        return prompt;
    }
}
