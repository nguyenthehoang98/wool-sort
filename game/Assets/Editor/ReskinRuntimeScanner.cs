using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Phân loại asset theo "co thuc su chay trong game khong".
///
/// Dependency thu (AssetDatabase.GetDependencies) noi tat ca thu duoc gan -
/// ke ca thu bi an, thu la fallback, thu thua. Reskin theo bang do se bi
/// thua rat nhieu. Bo nay tach duoc 4 nhom:
///
///   Running      - dang active khi load scene, hoac la Resources asset ma code
///                  load bang path CU THE (Levels/Level1, LevelColorMatrix/Level1...)
///   HiddenAtLoad - chi nam tren object DANG TAT luc load (popup, VFX) -> se
///                  chay khi co event, KHONG nen xoa
///   Fallback     - Resources asset ma code load bang PREFIX + bien, va da co
///                  mot ban sao ngoai Resources (ban fallback) -> thuong khong
///                  chay vi field Inspector da gan sprite
///   Unused       - khong con duong nao toi object active/bi an -> co the xoa
/// </summary>
public static class ReskinRuntimeScanner
{
    public enum RunState { Running, HiddenAtLoad, Fallback, Unused }

    public class Result
    {
        public readonly Dictionary<string, RunState> States = new Dictionary<string, RunState>();
        public readonly List<string> Images = new List<string>();
        public int Running, Hidden, Fallback, Unused;
    }

    private static readonly string[] ImageExts =
        { ".png", ".jpg", ".jpeg", ".tga", ".bmp", ".tif", ".tiff", ".webp", ".psd", ".exr", ".hdr" };

    public static bool IsImage(string path) =>
        !string.IsNullOrEmpty(path) && ImageExts.Contains(Path.GetExtension(path).ToLower());

    /// <summary>Quét các scene gốc, trả về trạng thái của từng asset ảnh.</summary>
    public static Result Scan(IEnumerable<string> scenePaths)
    {
        var res = new Result();
        var activeRoots = new HashSet<string>();
        var hiddenRoots = new HashSet<string>();

        foreach (var scene in scenePaths)
        {
            GameObject[] roots;
            try
            {
                roots = AssetDatabase.LoadAllAssetsAtPath(scene).OfType<GameObject>().ToArray();
            }
            catch { continue; }

            foreach (var root in roots)
            {
                Transform[] trs;
                try { trs = root.GetComponentsInChildren<Transform>(true); }
                catch { continue; }

                foreach (var tr in trs)
                {
                    bool on;
                    try { on = tr.gameObject.activeInHierarchy; }
                    catch { continue; }

                    Component[] comps;
                    try { comps = tr.GetComponents<Component>(); }
                    catch { continue; }

                    foreach (var c in comps)
                    {
                        if (c == null) continue;
                        var beh = c as Behaviour;
                        if (beh != null && !beh.enabled) continue;

                        foreach (var p in ObjectRefs(c))
                        {
                            (on ? activeRoots : hiddenRoots).Add(p);
                        }
                    }
                }
            }
        }

        // Resources asset ma code load bang path cu the -> chay that
        var codeRes = CodeResourcesPaths();
        foreach (var p in AllProjectAssets())
        {
            if (!p.StartsWith("Assets/Resources/", StringComparison.Ordinal) &&
                !p.StartsWith("Assets/Resources\\", StringComparison.Ordinal)) continue;
            string inner = p.Substring("Assets/Resources/".Length).Replace('\\', '/');
            foreach (var exact in codeRes.Exact)
            {
                if (inner == exact || inner.StartsWith(exact + "/", StringComparison.Ordinal))
                {
                    activeRoots.Add(p);
                    break;
                }
            }
        }

        // phan loai
        var seen = new HashSet<string>();
        Mark(activeRoots, RunState.Running, res, seen);
        Mark(hiddenRoots, RunState.HiddenAtLoad, res, seen);

        // Resources asset load bang PREFIX + bien -> co ban sao ngoai Resources
        // thi do la FALLBACK (field Inspector da gan sprite roi)
        foreach (var p in AllProjectAssets())
        {
            if (res.States.ContainsKey(p)) continue;
            if (!IsImage(p)) continue;
            if (!p.StartsWith("Assets/Resources/", StringComparison.Ordinal)) continue;
            string inner = p.Substring("Assets/Resources/".Length).Replace('\\', '/');
            string fileName = Path.GetFileName(inner);

            bool isFallback = false;
            foreach (var pre in codeRes.Prefixes)
            {
                if (!inner.StartsWith(pre, StringComparison.Ordinal)) continue;
                // co ban sao cung ten ngoai Resources -> day la ban fallback
                if (HasCopyOutsideResources(fileName)) { isFallback = true; break; }
            }
            if (isFallback)
            {
                res.States[p] = RunState.Fallback;
                res.Fallback++;
            }
        }

        // anh trong dependency thu nhung khong phai trong 3 nhom tren -> thua
        foreach (var p in DependencyImages(scenePaths))
        {
            if (res.States.ContainsKey(p)) continue;
            res.States[p] = RunState.Unused;
            res.Unused++;
        }

        foreach (var kv in res.States)
        {
            if (IsImage(kv.Key)) res.Images.Add(kv.Key);
            switch (kv.Value)
            {
                case RunState.Running: res.Running++; break;
                case RunState.HiddenAtLoad: res.Hidden++; break;
                case RunState.Fallback: res.Fallback++; break;
                default: res.Unused++; break;
            }
        }
        res.Images.Sort(StringComparer.OrdinalIgnoreCase);
        return res;
    }

    // ------------------------------------------------------------------ helper
    private static void Mark(HashSet<string> roots, RunState state, Result res, HashSet<string> seen)
    {
        foreach (var root in roots)
        {
            if (!seen.Add(root)) continue;
            if (!res.States.ContainsKey(root)) res.States[root] = state;

            string[] deps;
            try { deps = AssetDatabase.GetDependencies(root, true); }
            catch { continue; }
            foreach (var d in deps)
            {
                if (d == root) continue;
                if (seen.Add(d) && !res.States.ContainsKey(d)) res.States[d] = state;
            }
        }
    }

    private static IEnumerable<string> ObjectRefs(Component c)
    {
        var result = new List<string>();
        try
        {
            var so = new SerializedObject(c);
            var it = so.GetIterator();
            if (!it.NextVisible(true)) return result;
            do
            {
                if (it.propertyType != SerializedPropertyType.ObjectReference) continue;
                var obj = it.objectReferenceValue;
                if (obj == null) continue;
                string p = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(p)) result.Add(p);
            } while (it.NextVisible(true));
        }
        catch { }
        return result;
    }

    private static HashSet<string> DependencyImages(IEnumerable<string> scenePaths)
    {
        var set = new HashSet<string>();
        foreach (var s in scenePaths)
        {
            string[] deps;
            try { deps = AssetDatabase.GetDependencies(s, true); }
            catch { continue; }
            foreach (var d in deps)
                if (IsImage(d)) set.Add(d);
        }
        return set;
    }

    private static bool HasCopyOutsideResources(string fileName)
    {
        foreach (var p in AllProjectAssets())
        {
            if (p.StartsWith("Assets/Resources/", StringComparison.Ordinal)) continue;
            if (!IsImage(p)) continue;
            if (string.Equals(Path.GetFileName(p), fileName, StringComparison.OrdinalIgnoreCase)) return true;
        }
        return false;
    }

    private static IEnumerable<string> AllProjectAssets()
    {
        return AssetDatabase.GetAllAssetPaths();
    }

    private class CodeRes
    {
        public readonly HashSet<string> Exact = new HashSet<string>();
        public readonly HashSet<string> Prefixes = new HashSet<string>();
    }

    /// <summary>Quét code trong project tìm các path Resources.Load.</summary>
    private static CodeRes CodeResourcesPaths()
    {
        var res = new CodeRes();
        foreach (var cs in FindScripts())
        {
            string text;
            try { text = File.ReadAllText(cs); } catch { continue; }
            if (!text.Contains("Resources.Load")) continue;

            // Resources.Load("Levels/Level1")  -> path cu the
            foreach (System.Text.RegularExpressions.Match m in
                System.Text.RegularExpressions.Regex.Matches(text, @"Resources\.Load(?:<[^>]+>)?\s*\(\s*""([^""]+)"""))
            {
                res.Exact.Add(m.Groups[1].Value);
            }
            // Resources.Load($"Levels/Level{n}") -> prefix
            foreach (System.Text.RegularExpressions.Match m in
                System.Text.RegularExpressions.Regex.Matches(text, @"Resources\.Load(?:<[^>]+>)?\s*\(\s*\$""([^""{]*)\{"))
            {
                res.Prefixes.Add(m.Groups[1].Value.TrimEnd('/'));
            }
            // Resources.Load("BoosterUI/" + name) -> prefix
            foreach (System.Text.RegularExpressions.Match m in
                System.Text.RegularExpressions.Regex.Matches(text, @"Resources\.Load(?:<[^>]+>)?\s*\(\s*""([^""]+)""\s*\+"))
            {
                res.Prefixes.Add(m.Groups[1].Value.TrimEnd('/'));
            }
        }
        return res;
    }

    private static IEnumerable<string> FindScripts()
    {
        var dirs = new[] { "Assets/_Game/Scripts", "Assets/_KIT", "Assets/_Game/_UISystem" };
        foreach (var d in dirs)
        {
            if (!Directory.Exists(d)) continue;
            foreach (var f in Directory.GetFiles(d, "*.cs", SearchOption.AllDirectories))
            {
                if (f.Contains("/Editor/") || f.Contains("\\Editor\\")) continue;
                yield return f;
            }
        }
    }
}
