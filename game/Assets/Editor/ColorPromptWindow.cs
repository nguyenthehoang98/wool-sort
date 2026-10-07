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
    public string promptTemplate = @"Create a cute chunky pixel-art illustration in the exact visual style of a 36x36 logical pixel grid.

PIXEL GRID:
- Logical resolution: 36x36 pixels (1296 logical pixels total)
- Final canvas: 1296x1296 pixels
- Each logical pixel must be exactly 36x36 physical pixels
- Use hard-edged square pixels only
- Every shape must align perfectly to the 36x36 pixel grid
- No sub-pixels, No anti-aliasing, No smooth edges, No blur, No gradients, No semi-transparent pixels
- Nearest-neighbor pixel rendering

ART STYLE:
- Very chunky, low-resolution pixel art, simple geometric silhouettes, large blocks of flat colors, stepped pixel contours
- Cute casual mobile-game aesthetic, bright cheerful highly saturated colors, minimal but expressive details
- Use color blocks for shading instead of gradients, thick dark brown-gray pixel outlines #685155 following same grid

COLOR STYLE - Use ONLY these {colorCount} colors from this level (filtered palette - MUST follow exact ratio, do NOT add other colors):
{filteredPalette}
- Outline: Dark brown-gray #685155 for thick outlines only (not counted in ratio)

COMPOSITION:
- Large simple central subject, strong readable silhouette
- Background from large flat pixel shapes, decorative elements from chunky square pixels
- Visually clean and uncluttered, avoid tiny details that would not survive at 36x36

LEVEL COLOR DISTRIBUTION - MUST FOLLOW EXACTLY (pixel ratio approx color %):
Level: {levelName} | Yarn balls: {totalCells} (outer {outerCount}, total spawns {totalSpawns}) | {colorCount} colors
Total logical pixels: 1296 (36x36) -> physical canvas 1296x1296
{pixelDistribution}

IMPORTANT:
- Image must look as if originally designed on 36x36 and enlarged to 1296x1296 via nearest-neighbor
- Do NOT create high-res and pixelate after, construct directly from 36x36 grid
- Colors: {colorList} with exact %: {colorDistribution}

NO: photorealism, smooth vector art, anti-aliasing, gradients, soft shadows, realistic lighting, fine line art, detailed textures, painterly effects, 3D rendering, glossy effects.";
    public PromptStyle style = PromptStyle.Detailed;
    public int imageWidth = 1296;
    public int imageHeight = 1296;
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
        GUILayout.Label("Placeholder: {levelName} {colorList} {filteredPalette} {colorDistribution} {pixelDistribution} {totalCells} {outerCount} {totalSpawns} {colorCount} {style}", EditorStyles.miniLabel);
        if (!_savedData.promptTemplate.Contains("PIXEL GRID") || !_savedData.promptTemplate.Contains("{filteredPalette}"))
        {
            EditorGUILayout.HelpBox("Template cu chua co palette loc theo mau + ti le. Bam 'Reset Pixel Template' de dung mau moi chi lay mau co xuat hien kem % va pixel.", MessageType.Warning);
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
            _savedData.imageWidth = 1296; _savedData.imageHeight = 1296;
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
        return c switch
        {
            YarnColor.Yellow => "#F5D025",
            YarnColor.Cream => "#FFEABD",
            YarnColor.Snow => "#FFEABD",
            YarnColor.Pink => "#F57DC7",
            YarnColor.Magenta => "#F57DC7",
            YarnColor.Sky => "#41D3F0",
            YarnColor.Teal => "#41D3F0",
            YarnColor.Blue => "#41D3F0",
            YarnColor.Green => "#9ECA27",
            YarnColor.Forest => "#9ECA27",
            YarnColor.Lime => "#5CD94A",
            YarnColor.Orange => "#FF943E",
            YarnColor.Coral => "#FF943E",
            YarnColor.Purple => "#D162EC",
            YarnColor.Plum => "#D162EC",
            YarnColor.Crimson => "#F14E17",
            YarnColor.Brown => "#8B4513",
            YarnColor.Charcoal => "#685155",
            YarnColor.Gray => "#9E9E9E",
            YarnColor.Steel => "#607D8B",
            YarnColor.Deep => "#3F2A2A",
            _ => "#9ECA27"
        };
    }

    private string GeneratePrompt(LevelColorStats stats)
    {
        var sorted = stats.colorCounts.OrderByDescending(kv => kv.Value).ToList();
        // colorDistribution: "35.2% Green (9 balls)"
        string colorDist = sorted.Count == 0 ? "no yarn" : string.Join(", ", sorted.Select(kv => {
            float pct = stats.totalOccurrences > 0 ? (float)kv.Value / stats.totalOccurrences * 100f : 0f;
            return $"{pct:F1}% {kv.Key} {GetHexForYarnColor(kv.Key)} ({kv.Value} balls)";
        }));
        // colorList: "Green, Blue, Pink"
        string colorList = sorted.Count == 0 ? "none" : string.Join(", ", sorted.Select(kv => $"{kv.Key} {GetHexForYarnColor(kv.Key)}"));
        // filteredPalette: chi lay mau co xuat hien, kem ti le % va so luong
        string filteredPalette = sorted.Count == 0 ? "- none" : string.Join("\n", sorted.Select(kv => {
            float pct = stats.totalOccurrences > 0 ? (float)kv.Value / stats.totalOccurrences * 100f : 0f;
            return $"- {kv.Key}: {GetHexForYarnColor(kv.Key)} - {pct:F1}% ({kv.Value} balls)";
        }));
        // pixelDistribution: tinh pixel dua tren 1296 logical pixels
        const int totalLogical = 36 * 36; // 1296
        string pixelDist;
        if (sorted.Count == 0) pixelDist = "- No colors";
        else
        {
            // tinh so pixel lam tron, dieu chinh de tong =1296
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
                int physical = logical * 36 * 36; // moi logical = 1296 physical pixels
                string hex = GetHexForYarnColor(kv.Key);
                lines.Add($"- {kv.Key} {hex}: {pct:F1}% ({kv.Value} balls) = {logical} logical pixels ({logical} x 36x36 = {physical} physical pixels)");
            }
            lines.Add($"- Outline #685155: used for thick outlines, not counted in %");
            lines.Add($"- Total: {totalLogical} logical pixels = 1296x1296 physical canvas");
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
