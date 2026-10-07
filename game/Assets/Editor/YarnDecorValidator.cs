using System.Collections.Generic;
using System.Text;
using _Game.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Check ở EDITOR (không cần Play):
/// - YarnDecoration nào không nằm dưới YarnPart -> sẽ không bao giờ Drop (YarnPart.decorations rỗng).
/// - SpecialDecor nào hostPart null / ancestor chưa gán.
/// - YarnPart nào ancestor null / decorations list lệch với con thực tế.
/// Mở: Tools / Puzzle / Validate decor gắn len
/// </summary>
public static class YarnDecorValidator
{
    [MenuItem("Tools/Puzzle/3. Quét decor gắn len (scene đang mở)", false, 3)]
    public static void ValidateCurrentScene()
    {
        var sb = new StringBuilder();
        int err = 0, warn = 0;

        var allParts = Object.FindObjectsByType<YarnPart>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var allDecos = Object.FindObjectsByType<YarnDecoration>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var allSpecials = Object.FindObjectsByType<SpecialDecor>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        sb.AppendLine($"[Scene] YarnPart: {allParts.Length}, YarnDecoration: {allDecos.Length}, SpecialDecor: {allSpecials.Length}");

        // 1) YarnDecoration phải có YarnPart cha
        var decoUnderPart = new HashSet<YarnDecoration>();
        foreach (var p in allParts)
            foreach (var d in p.GetComponentsInChildren<YarnDecoration>(true))
                decoUnderPart.Add(d);

        foreach (var d in allDecos)
        {
            var host = d.GetComponentInParent<YarnPart>();
            if (host == null || !decoUnderPart.Contains(d))
            {
                sb.AppendLine($"[ERROR] {FullPath(d.transform)}: YarnDecoration mồ côi, không nằm dưới YarnPart nào -> click len sẽ không Drop. Kéo nó vào làm con của YarnPart đúng.");
                err++;
            }
            else if (d.GetComponent<Rigidbody>() == null)
            {
                sb.AppendLine($"[WARN] {FullPath(d.transform)}: thiếu Rigidbody (runtime sẽ tự Add + isKinematic=true, nhưng nên gắn sẵn).");
                warn++;
            }
        }

        // 2) YarnPart: ancestor + decorations list
        foreach (var p in allParts)
        {
            if (p.layerIndex == 1 && p.ancestor == null)
            {
                sb.AppendLine($"[ERROR] {FullPath(p.transform)}: layer 1 nhưng ancestor == null (level không tạo bằng QuickCreator). Runtime SpecialDecor so sánh ancestor==host sẽ luôn false.");
                err++;
            }
            var real = p.GetComponentsInChildren<YarnDecoration>(true);
            if ((p.decorations == null || p.decorations.Count == 0) && real.Length > 0)
            {
                sb.AppendLine($"[WARN] {FullPath(p.transform)}: có {real.Length} YarnDecoration con nhưng list decorations rỗng (Awake sẽ tự tìm lại, nhưng nên gán sẵn trong prefab).");
                warn++;
            }
            if (p.decorations != null)
            {
                foreach (var d in p.decorations)
                {
                    if (d == null) continue;
                    if (d.transform.parent == null || d.GetComponentInParent<YarnPart>() != p)
                    {
                        sb.AppendLine($"[ERROR] {FullPath(p.transform)}: decorations list chứa {d.name} nhưng nó không phải con của part này.");
                        err++;
                    }
                }
            }
        }

        // 3) SpecialDecor: phải nằm dưới YarnPart lúc EDIT (runtime nó mới detach)
        foreach (var s in allSpecials)
        {
            var host = s.GetComponentInParent<YarnPart>();
            if (host == null)
            {
                sb.AppendLine($"[ERROR] {FullPath(s.transform)}: SpecialDecor mồ côi, không nằm dưới YarnPart nào -> hostPart null, Drop() lỗi.");
                err++;
            }
        }

        sb.AppendLine($"--- Xong: {err} lỗi, {warn} cảnh báo ---");
        if (err == 0 && warn == 0) sb.AppendLine("[OK] Decor gắn đúng hết.");
        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Validate decor gắn len", sb.ToString(), "OK");
    }

    [MenuItem("Tools/Puzzle/4. Quét decor tất cả Level prefab", false, 4)]
    public static void ValidateAllLevelPrefabs()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Resources/Levels" });
        var sb = new StringBuilder();
        int err = 0;
        foreach (var g in guids)
        {
            var path = AssetDatabase.GUIDToAssetPath(g);
            var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (root == null) continue;
            var parts = root.GetComponentsInChildren<YarnPart>(true);
            var decos = root.GetComponentsInChildren<YarnDecoration>(true);
            var specials = root.GetComponentsInChildren<SpecialDecor>(true);
            var underPart = new HashSet<YarnDecoration>();
            foreach (var p in parts)
                foreach (var d in p.GetComponentsInChildren<YarnDecoration>(true))
                    underPart.Add(d);
            foreach (var d in decos)
            {
                if (!underPart.Contains(d))
                {
                    sb.AppendLine($"[ERROR] {path}: {d.name} mồ côi (không dưới YarnPart).");
                    err++;
                }
            }
            foreach (var p in parts)
            {
                if (p.layerIndex == 1 && p.ancestor == null)
                {
                    sb.AppendLine($"[ERROR] {path}: {p.name} ancestor null.");
                    err++;
                }
            }
            foreach (var s in specials)
            {
                if (s.GetComponentInParent<YarnPart>() == null)
                {
                    sb.AppendLine($"[ERROR] {path}: SpecialDecor {s.name} mồ côi.");
                    err++;
                }
            }
        }
        if (err == 0) sb.AppendLine("[OK] Tất cả Level prefab decor gắn đúng.");
        Debug.Log(sb.ToString());
        EditorUtility.DisplayDialog("Validate tất cả Level", sb.ToString(), "OK");
    }

    private static string FullPath(Transform t)
    {
        var names = new List<string>();
        while (t != null) { names.Add(t.name); t = t.parent; }
        names.Reverse();
        return string.Join("/", names);
    }
}
