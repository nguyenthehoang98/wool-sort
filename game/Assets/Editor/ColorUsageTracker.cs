using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _Game.Scripts;
using _Game.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

public static class ColorUsageTracker
{
    private const string LevelsPath = "Assets/Resources/Levels";
    private const string ReportPath = "Assets/ColorUsageReport.txt";

    [MenuItem("Tools/Color Usage Tracker")]
    public static void ShowReport()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { LevelsPath });
        var perLevelStats = new List<LevelColorStats>();
        var globalColorCounts = new Dictionary<YarnColor, int>();
        int globalTotalSpawns = 0;
        int globalTotalOccurrences = 0;

        foreach (var guid in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var stats = CollectStatsForPrefab(path);
            if (stats == null) continue;
            perLevelStats.Add(stats);
            globalTotalSpawns += stats.totalSpawnCount;
            foreach (var kv in stats.colorCounts)
            {
                if (!globalColorCounts.ContainsKey(kv.Key)) globalColorCounts[kv.Key] = 0;
                globalColorCounts[kv.Key] += kv.Value;
            }
        }
        globalTotalOccurrences = globalColorCounts.Values.Sum();

        // Build report
        var sb = new StringBuilder();
        sb.AppendLine("Color Usage Report - CHINH XAC theo YarnPart + Material");
        sb.AppendLine("======================================================");
        sb.AppendLine($"Tong so level: {perLevelStats.Count}");
        sb.AppendLine($"Tong so YarnPart (outer): {perLevelStats.Sum(s => s.outerPartCount)}");
        sb.AppendLine($"Tong so lan xuat hien (bao gom inner layers = sum 1+totalInnerCount): {globalTotalSpawns}");
        sb.AppendLine($"Tong so luot mau duoc dem (outer + fixed-inner + randomColorEntries): {globalTotalOccurrences}");
        sb.AppendLine($"Ghi chu: % mau = so lan mau do xuat hien / tong so luot mau * 100");
        sb.AppendLine();

        // Global section
        sb.AppendLine("=== TONG HOP TOAN BO LEVELS ===");
        foreach (var kv in globalColorCounts.OrderByDescending(kv => kv.Value))
        {
            float pct = globalTotalOccurrences > 0 ? (float)kv.Value / globalTotalOccurrences * 100f : 0f;
            float pctVsSpawn = globalTotalSpawns > 0 ? (float)kv.Value / globalTotalSpawns * 100f : 0f;
            sb.AppendLine($"Mau {(int)kv.Key}: {kv.Key} - So luot: {kv.Value} | Ti le vs tong mau: {pct:F2}% | Ti le vs tong spawn: {pctVsSpawn:F2}%");
        }
        sb.AppendLine();

        // Per level
        foreach (var stats in perLevelStats.OrderBy(s => s.levelName))
        {
            sb.AppendLine($"--- {stats.levelName} ---");
            sb.AppendLine($"  YarnPart outer: {stats.outerPartCount} | Tong spawn (1+inner): {stats.totalSpawnCount} | Tong luot mau: {stats.totalOccurrences}");
            if (stats.colorCounts.Count == 0)
            {
                sb.AppendLine("  (khong co mau)");
            }
            else
            {
                foreach (var kv in stats.colorCounts.OrderByDescending(kv => kv.Value))
                {
                    float pctLevel = stats.totalOccurrences > 0 ? (float)kv.Value / stats.totalOccurrences * 100f : 0f;
                    float pctGlobal = globalTotalOccurrences > 0 ? (float)kv.Value / globalTotalOccurrences * 100f : 0f;
                    sb.AppendLine($"    Mau {(int)kv.Key}: {kv.Key} - {kv.Value} lan | {pctLevel:F2}% trong level | {pctGlobal:F2}% toan bo");
                }
            }
            // Debug cross-check with LevelYarnInfoHolder if present
            if (stats.holderColorCounts != null && stats.holderColorCounts.Count > 0)
            {
                sb.AppendLine($"  [Holder debug] colorCountEntries: {string.Join(", ", stats.holderColorCounts.Select(kv => $"{kv.Key}:{kv.Value}"))}");
            }
            sb.AppendLine();
        }

        File.WriteAllText(ReportPath, sb.ToString());
        AssetDatabase.Refresh();
        Debug.Log($"[ColorUsageTracker] Da ghi bao cao chinh xac tai: {ReportPath}\n{sb.ToString()}");
        EditorUtility.DisplayDialog("Color Usage Tracker", $"Da tao bao cao chinh xac:\n{ReportPath}\nXem Console de xem chi tiet.", "OK");
    }

    public static LevelColorStats CollectStatsForPrefab(string prefabPath)
    {
        var go = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (go == null) return null;
        var levelName = Path.GetFileNameWithoutExtension(prefabPath);
        var parts = go.GetComponentsInChildren<YarnPart>(true);
        var holder = go.GetComponent<LevelYarnInfoHolder>();
        if (holder == null) holder = go.GetComponentInChildren<LevelYarnInfoHolder>(true);

        var dict = new Dictionary<YarnColor, int>();
        int totalSpawns = 0;
        foreach (var p in parts)
        {
            // outer
            if (!dict.ContainsKey(p.initColor)) dict[p.initColor] = 0;
            dict[p.initColor]++;

            totalSpawns += 1 + p.totalInnerCount;

            // fixed-inner
            if (p.totalInnerCount > 0)
            {
                int limit = Math.Min(p.totalInnerCount, Math.Min(p.innerColorTypes.Count, p.innerFixedColors.Count));
                for (int j = 0; j < limit; j++)
                {
                    if (p.innerColorTypes[j] == InnerColorType.Fixed)
                    {
                        var c = p.innerFixedColors[j];
                        if (!dict.ContainsKey(c)) dict[c] = 0;
                        dict[c]++;
                    }
                }
                // neu list ngan hon totalInnerCount, cac inner con lai la Random -> khong dem (giong LevelYarnInfoHolder)
            }
        }
        // randomColorEntries tu holder
        Dictionary<YarnColor,int> holderDict = null;
        if (holder != null)
        {
            holderDict = new Dictionary<YarnColor,int>();
            foreach (var e in holder.randomColorEntries)
            {
                if (!dict.ContainsKey(e.color)) dict[e.color] = 0;
                dict[e.color] += e.count;
                if (!holderDict.ContainsKey(e.color)) holderDict[e.color]=0;
                holderDict[e.color]+=e.count;
            }
            // luu lai holder's colorCountEntries de cross-check (neu holder da UpdateDebugInfo)
            if (holder.colorCountEntries != null && holder.colorCountEntries.Count > 0)
            {
                holderDict = holder.colorCountEntries.ToDictionary(kv => kv.color, kv => kv.count);
            }
        }

        return new LevelColorStats
        {
            levelName = levelName,
            prefabPath = prefabPath,
            outerPartCount = parts.Length,
            totalSpawnCount = totalSpawns,
            colorCounts = dict,
            holderColorCounts = holderDict
        };
    }
}

public class LevelColorStats
{
    public string levelName;
    public string prefabPath;
    public int outerPartCount;
    public int totalSpawnCount;
    public Dictionary<YarnColor,int> colorCounts = new Dictionary<YarnColor,int>();
    public Dictionary<YarnColor,int> holderColorCounts;
    public int totalOccurrences => colorCounts.Values.Sum();
}
