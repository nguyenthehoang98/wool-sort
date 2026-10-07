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
    public string promptTemplate = @"# STRICT 28×28 CELL PIXEL ART

Create **one random cute cartoon/chibi pixel-art illustration**.

The subject and composition are completely random. You may create animals, characters, objects, plants, scenery, food, toys, monsters, or any other cute subject.

The content is random, but the following technical constraints are **ABSOLUTE**.

---

## 1. EXACT 28×28 LOGICAL CELL GRID

The entire image is constructed on exactly:

**28 columns × 28 rows = 784 logical cells**

Think of the image as a physical grid:

- 28 cells wide
- 28 cells high
- 784 cells total
- Every cell is exactly one square unit
- Every cell contains exactly one solid color
- Every shape must align to whole cells
- No partial cells
- No sub-cell details
- No anti-aliasing
- No smoothing
- No blur
- No gradients
- No transparency
- No blended pixels
- No intermediate colors

The artwork must look as if it was manually drawn directly on a **28×28 pixel grid**.

Do NOT create a normal smooth illustration and then apply a pixel-art filter.

---

## 2. EXACT COLOR PALETTE

Use **ONLY** the following colors:

| Color | HEX | Required Cell Count |
|---|---|---:|
| Green | `#9ECA27` | 302 |
| Orange | `#FF943E` | 151 |
| Gray | `#9E9E9E` | 91 |
| Magenta | `#F57DC7` | 73 |
| Purple | `#D162EC` | 61 |
| Yellow | `#F5D025` | 58 |
| Blue | `#41D3F0` | 48 |
| **TOTAL** | | **784** |

These seven colors are the **complete color palette**.

No other colors are permitted.

---

## 3. EXACT COLOR CELL COUNTS

The final 28×28 image must contain exactly:

- `#9ECA27` → **302 cells**
- `#FF943E` → **151 cells**
- `#9E9E9E` → **91 cells**
- `#F57DC7` → **73 cells**
- `#D162EC` → **61 cells**
- `#F5D025` → **58 cells**
- `#41D3F0` → **48 cells**

Total:

**302 + 151 + 91 + 73 + 61 + 58 + 48 = 784 cells**

Therefore every one of the 784 cells must belong to exactly one of these seven colors.

Do not interpret these numbers as approximate percentages.

They are **exact cell counts**.

---

## 4. RANDOM CONTENT

The subject is intentionally random.

Each generation may contain a completely different composition.

Examples:

- cute animal
- chibi character
- small monster
- fruit
- toy
- house
- tree
- flower
- food
- fantasy creature
- game character
- small landscape
- combination of several cute objects

Do not repeatedly use the same subject.

The content is random, but the **28×28 grid and color counts must never change**.

---

## 5. STYLE

Use a:

**cute cartoon + chibi + chunky pixel-art style**

Visual characteristics:

- cute
- playful
- friendly
- colorful
- chunky
- simplified
- readable silhouette
- large simple shapes
- stepped edges
- square pixel construction
- minimal but expressive details
- casual mobile-game aesthetic

Avoid:

- photorealism
- realistic rendering
- painterly style
- smooth vector art
- realistic lighting
- gradients
- soft shadows
- glow
- bloom
- texture noise
- anti-aliased edges

---

## 6. COLOR USAGE

The required cell counts are more important than visual realism.

Do NOT introduce additional shades to make the image prettier.

For example:

Allowed:

`#9ECA27`

Not allowed:

`#8FB923`

`#A8D52B`

`#B5E03A`

The same rule applies to every palette color.

If shading is needed, use another color from the approved palette.

Never create a new shade.

---

## 7. IMPORTANT INTERPRETATION

The word **pixel** means one logical cell.

One logical cell = one solid color.

The image should be understood as:

```text
28 columns
×
28 rows
=
784 cells
```

Each cell has exactly one of the seven approved colors.

The final result is therefore a **28×28 color-cell artwork**.

---

## 8. PRIORITY ORDER

If requirements conflict, follow this priority:

1. **Exactly 28×28 cells**
2. **Exactly 784 cells**
3. **Exactly 7 colors**
4. **Exact cell count for every color**
5. Cute cartoon/chibi pixel-art style
6. Random subject and composition
7. Visual detail

Never sacrifice the technical constraints for visual quality.

---

## FINAL REQUIREMENT

Create a **random cute cartoon/chibi pixel-art illustration** directly on a **28×28 logical cell grid**.

Use **exactly 7 colors** and exactly the specified number of cells for each color.

Every cell must contain exactly one solid approved color.

**No other colors. No extra cells. No partial cells. No gradients. No anti-aliasing.**";
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

    [MenuItem("Tools/Puzzle/6. Tao prompt anh level (copy)", false, 6)]
    public static void ShowWindow()
    {
        var window = GetWindow<ColorPromptWindow>();
        window.minSize = new Vector2(650, 500);
        window.titleContent = new GUIContent("Color Prompt Generator");
        window.LoadData();
        window.RefreshLevels();
        window.Show();
    }

    private void OnEnable() => LoadData();

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
        _levels = _levels.OrderBy(s => s.levelName).ToList();
    }

    private void OnGUI()
    {
        // PINNED TOP AREA
        EditorGUILayout.Space(4);
        GUILayout.Label("Color Prompt Generator - Chinh xac theo YarnPart", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Prompt chung o tren se duoc pin va luu lai. Moi level ben duoi co nut Copy rieng. Ti le mau tinh chinh xac theo YarnPart (outer + fixed-inner + random) giong LevelYarnInfoHolder.UpdateDebugInfo, khop voi model/material.", MessageType.Info);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        GUILayout.Label("Prompt Template (pin on top - co the chinh & luu):", EditorStyles.miniBoldLabel);
        GUILayout.Label("Static template: random subject + fixed 7-color cell counts (302/151/91/73/61/58/48 = 784). Same prompt for every level.", EditorStyles.miniLabel);
        if (!_savedData.promptTemplate.Contains("28×28") || !_savedData.promptTemplate.Contains("784"))
        {
            EditorGUILayout.HelpBox("Template cu chua theo cau truc STATIC 28x28 moi (random subject + so cell co dinh). Bam 'Reset Pixel Template' de dung mau moi.", MessageType.Warning);
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

    private static string GetHexForYarnColor(YarnColor c)
    {
        // STRICT 7-color palette only. No outline color, no white/cream/black/variants.
        return c switch
        {
            YarnColor.Yellow => "#F5D025",
            YarnColor.Cream => "#F5D025",   // cream banned -> Yellow
            YarnColor.Snow => "#F5D025",    // snow/white banned -> Yellow
            YarnColor.Pink => "#F57DC7",
            YarnColor.Magenta => "#F57DC7",
            YarnColor.Sky => "#41D3F0",
            YarnColor.Teal => "#41D3F0",
            YarnColor.Blue => "#41D3F0",
            YarnColor.Green => "#9ECA27",
            YarnColor.Forest => "#9ECA27",
            YarnColor.Lime => "#9ECA27",    // lime variant banned -> Green
            YarnColor.Orange => "#FF943E",
            YarnColor.Coral => "#FF943E",
            YarnColor.Purple => "#D162EC",
            YarnColor.Plum => "#D162EC",
            YarnColor.Crimson => "#FF943E", // crimson banned -> Orange
            YarnColor.Brown => "#FF943E",   // brown banned -> Orange
            YarnColor.Charcoal => "#9E9E9E",// outline banned -> Gray
            YarnColor.Gray => "#9E9E9E",
            YarnColor.Steel => "#9E9E9E",   // steel banned -> Gray
            YarnColor.Deep => "#9E9E9E",    // deep banned -> Gray (no outline)
            _ => "#9ECA27"
        };
    }

    private static string GetDisplayName(YarnColor c)
    {
        // Orange and Coral share the same HEX, keep distinct label but same color.
        return c switch
        {
            YarnColor.Coral => "Coral / Orange",
            YarnColor.Orange => "Coral / Orange",
            _ => c.ToString()
        };
    }

    private static string GetCanonicalName(string hex)
    {
        // Canonical display name for each of the 7 strict HEX colors.
        return hex switch
        {
            "#9ECA27" => "Green",
            "#FF943E" => "Orange",
            "#9E9E9E" => "Gray",
            "#F57DC7" => "Magenta",
            "#D162EC" => "Purple",
            "#F5D025" => "Yellow",
            "#41D3F0" => "Blue",
            _ => hex
        };
    }

    private string GeneratePrompt(LevelColorStats stats)
    {
        var sorted = stats.colorCounts.OrderByDescending(kv => kv.Value).ToList();
        // Merge counts by HEX: same HEX = same color (Orange+Coral, Pink+Magenta,
        // Sky+Teal+Blue, Green+Forest+Lime, Purple+Plum...). Canonical spec order.
        var byHex = new Dictionary<string, int>();
        foreach (var kv in sorted)
        {
            string hex = GetHexForYarnColor(kv.Key);
            if (!byHex.ContainsKey(hex)) byHex[hex] = 0;
            byHex[hex] += kv.Value;
        }
        string[] canonicalOrder = { "#9ECA27", "#FF943E", "#9E9E9E", "#F57DC7", "#D162EC", "#F5D025", "#41D3F0" };
        var merged = new List<(string name, string hex, int balls)>();
        foreach (var hex in canonicalOrder)
            if (byHex.TryGetValue(hex, out var b) && b > 0)
                merged.Add((GetCanonicalName(hex), hex, b));
        foreach (var kv in byHex)
            if (Array.IndexOf(canonicalOrder, kv.Key) < 0 && kv.Value > 0)
                merged.Add((GetCanonicalName(kv.Key), kv.Key, kv.Value));
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
        // colorList: "Green, Blue, Pink"
        string colorList = sorted.Count == 0 ? "none" : string.Join(", ", sorted.Select(kv => $"{kv.Key} {GetHexForYarnColor(kv.Key)}"));
        // filteredPalette: chi lay mau co xuat hien, kem ti le % va so luong
        string filteredPalette = sorted.Count == 0 ? "- none" : string.Join("\n", sorted.Select(kv => {
            float pct = stats.totalOccurrences > 0 ? (float)kv.Value / stats.totalOccurrences * 100f : 0f;
            return $"- {kv.Key}: {GetHexForYarnColor(kv.Key)} - {pct:F1}% ({kv.Value} balls)";
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
                string hex = GetHexForYarnColor(kv.Key);
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
