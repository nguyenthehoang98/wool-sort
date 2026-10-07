using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _Game.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Chuyển mesh len thành decor thuần túy (không bấm được, không đếm sợi):
/// - Gỡ YarnPart (+ YarnDecoration của các con, vì cha không bao giờ dissolve nữa).
/// - Chuyển layer sang Decoration (raycast tap len không bao giờ trúng).
/// - Gỡ khỏi holder.allBaseYarnParts, cộng totalYarnDecorations, tính lại count.
/// Mở: Tools / Puzzle / Chuyển len thành decor
/// </summary>
public sealed class YarnDecorTool : EditorWindow
{
    private const string LevelsFolder = "Assets/Resources/Levels";

    private GameObject levelPrefabAsset;
    private GameObject contentsRoot;
    private string contentsPath;
    private readonly List<YarnPart> shownParts = new List<YarnPart>();
    private readonly HashSet<YarnPart> picked = new HashSet<YarnPart>();
    private Vector2 scroll;
    private string report = "Kéo prefab level vào rồi tick các mesh muốn thành decor.";
    private readonly Dictionary<Mesh, Texture> previewCache = new Dictionary<Mesh, Texture>();

    // Panel preview prefab bên phải
    private PreviewRenderUtility previewUtil;
    private GameObject highlightCube;
    private Material highlightMat;
    private YarnPart focusPart;
    private YarnPart appliedFocus;
    private bool focusSelection = true;
    private bool contentsInPreview;
    // Orbit camera preview
    private float orbitYaw = 160f;
    private float orbitPitch = 20f;
    private float orbitDist = -1f;
    private Vector3 orbitTarget;
    private static readonly int ColorID = Shader.PropertyToID("_Color");
    private static readonly int MainColorID = Shader.PropertyToID("_MainColor");
    private static readonly int DarkThreadColorID = Shader.PropertyToID("_DarkThreadColor");
    private static readonly MaterialPropertyBlock dimBlock = new MaterialPropertyBlock();

    [MenuItem("Tools/Puzzle/Chuyển len thành decor", false, 16)]
    private static void Open()
    {
        var w = GetWindow<YarnDecorTool>("Len thành decor");
        w.minSize = new Vector2(760, 520);
    }

    private void OnEnable()
    {
        // Tiện nhất: đang mở prefab level nào thì tự nạp prefab đó
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
        if (stage != null)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);
            if (asset != null) levelPrefabAsset = asset;
        }
        RefreshList();
    }

    private void OnDisable()
    {
        UnloadContents();
        if (highlightCube != null) Object.DestroyImmediate(highlightCube);
        if (highlightMat != null) Object.DestroyImmediate(highlightMat);
        if (previewUtil != null)
        {
            previewUtil.Cleanup();
            previewUtil = null;
        }
    }

    private void EnsurePreview()
    {
        if (previewUtil != null) return;
        previewUtil = new PreviewRenderUtility();
        var key = new GameObject("preview key");
        key.hideFlags = HideFlags.HideAndDontSave;
        var light = key.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.2f;
        light.transform.rotation = Quaternion.Euler(30f, -30f, 0f);
        previewUtil.AddSingleGO(key);

        highlightMat = new Material(Shader.Find("Unlit/Transparent"));
        highlightMat.hideFlags = HideFlags.HideAndDontSave;
        highlightMat.color = new Color(0.2f, 1f, 0.2f, 0.28f);
        highlightCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        highlightCube.hideFlags = HideFlags.HideAndDontSave;
        Object.DestroyImmediate(highlightCube.GetComponent<Collider>());
        var hrend = highlightCube.GetComponent<MeshRenderer>();
        hrend.sharedMaterial = highlightMat;
        hrend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        hrend.receiveShadows = false;
        highlightCube.SetActive(false);
        previewUtil.AddSingleGO(highlightCube);
    }

    private void UnloadContents()
    {
        contentsInPreview = false;
        if (contentsRoot != null)
        {
            PrefabUtility.UnloadPrefabContents(contentsRoot);
            contentsRoot = null;
            contentsPath = null;
        }
        shownParts.Clear();
        picked.Clear();
        previewCache.Clear();
    }

    private void RefreshList()
    {
        UnloadContents();
        focusPart = null;
        appliedFocus = null;
        focusSelection = false;
        orbitDist = -1f;
        if (levelPrefabAsset == null) return;
        contentsPath = AssetDatabase.GetAssetPath(levelPrefabAsset);
        if (string.IsNullOrEmpty(contentsPath)) return;
        contentsRoot = PrefabUtility.LoadPrefabContents(contentsPath);
        shownParts.AddRange(contentsRoot.GetComponentsInChildren<YarnPart>(true)
            .OrderBy(p => p.name));
    }

    private void OnGUI()
    {
        GUILayout.Label("CHUYỂN LEN THÀNH DECOR", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Mesh decor sẽ: gỡ YarnPart (không bấm/dissolve), sang layer Decoration\n" +
            "(tap không trúng), gỡ khỏi danh sách len của Holder, đếm lại count.",
            MessageType.Info);

        EditorGUI.BeginChangeCheck();
        levelPrefabAsset = (GameObject)EditorGUILayout.ObjectField("Prefab level", levelPrefabAsset, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
            RefreshList();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Tải lại danh sách")) RefreshList();
        if (GUILayout.Button("Chọn hết")) { foreach (var p in shownParts) picked.Add(p); }
        if (GUILayout.Button("Bỏ chọn hết")) picked.Clear();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Len trong prefab: {shownParts.Count} | Đã tick: {picked.Count} (bấm vào hình để tick/bỏ, prefab dim theo)");
        EditorGUILayout.BeginHorizontal();
        // Chia đôi: trái grid, phải preview (rộng bằng nhau)
        float halfW = Mathf.Max(300f, position.width * 0.5f);
        // Trái: grid (co theo width còn lại)
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        float gridWidth = Mathf.Max(320f, position.width - halfW - 30f);
        int cols = Mathf.Max(2, (int)(gridWidth / 160));
        scroll = EditorGUILayout.BeginScrollView(scroll);
        int col = 0;
        EditorGUILayout.BeginHorizontal();
        foreach (var part in shownParts)
        {
            if (part == null) continue;
            if (col >= cols)
            {
                col = 0;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
            }
            col++;
            DrawPartCell(part);
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        // Phải: tab prefab full level (rộng bằng nửa window)
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(halfW));
        DrawPrefabPane();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();

        // Vẽ lại khi thumbnail đang load dần
        if (AssetPreview.IsLoadingAssetPreviews())
            Repaint();

        if (GUILayout.Button("Chuyển đã tick thành decor", GUILayout.Height(32)))
        {
            if (contentsRoot == null || picked.Count == 0)
            {
                report = "[LỖI] Chưa nạp prefab hoặc chưa tick mesh nào.";
            }
            else if (EditorUtility.DisplayDialog("Chuyển thành decor?",
                $"Gỡ YarnPart của {picked.Count} mesh, sang layer Decoration, đếm lại Holder?\nKhông undo được sau khi lưu prefab.",
                "Chuyển luôn", "Hủy"))
            {
                ConvertPicked();
            }
        }

        EditorGUILayout.TextArea(report, GUILayout.MinHeight(120));
    }

    private void DrawPartCell(YarnPart part)
    {
        bool tick = picked.Contains(part);
        var mf = part.GetComponent<MeshFilter>();
        var mesh = mf != null ? mf.sharedMesh : null;
        int vtx = mesh != null ? mesh.vertexCount : 0;

        var bg = tick ? new Color(0.3f, 0.7f, 0.3f, 0.35f) : new Color(0.5f, 0.5f, 0.5f, 0.15f);
        var rect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150), GUILayout.Height(172));
        EditorGUI.DrawRect(rect, bg);

        // Preview model (bấm vào hình để tick/bỏ tick)
        Texture prev = null;
        if (mesh != null && !previewCache.TryGetValue(mesh, out prev))
        {
            prev = AssetPreview.GetAssetPreview(mesh);
            previewCache[mesh] = prev;
        }
        GUIContent content = prev != null
            ? new GUIContent(prev)
            : new GUIContent("đang tải...");
        if (GUILayout.Button(content, GUILayout.Height(96)))
        {
            if (tick) picked.Remove(part);
            else picked.Add(part);
            tick = !tick;
            // Bấm vào model nào thì tab prefab sáng đúng chỗ đó + camera bay tới
            focusPart = part;
            focusSelection = true;
            var pb = GetPartBounds(part);
            orbitTarget = pb.center;
            orbitDist = FitDistance(pb) * 2.5f;
        }

        bool next = EditorGUILayout.ToggleLeft(part.name, tick);
        if (next != tick)
        {
            if (next) picked.Add(part);
            else picked.Remove(part);
        }
        EditorGUILayout.LabelField($"{part.initColor} | inner {part.totalInnerCount}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"vtx {vtx} | {part.gameObject.layer}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
    }

    private void DrawPrefabPane()
    {
        GUILayout.Label("PREFAB FULL LEVEL (kéo chuột để xoay)", EditorStyles.boldLabel);
        EnsurePreview();

        if (contentsRoot == null)
        {
            GUILayout.Label("Chưa nạp prefab.", EditorStyles.miniLabel);
            return;
        }

        // Prefab contents nằm ở scene ẩn riêng: phải chuyển vào scene preview
        // thì camera preview mới thấy (đèn + khung highlight đã ở đó).
        if (!contentsInPreview)
        {
            previewUtil.AddSingleGO(contentsRoot);
            contentsInPreview = true;
            orbitDist = -1f;
        }

        if (focusPart == null || focusPart.gameObject == null)
        {
            focusPart = null;
            focusSelection = false;
        }

        // Item được bấm: sáng 1.0, các item khác dim 0.5 (per-renderer, không đụng asset)
        if (appliedFocus != focusPart)
        {
            ApplyDimAll();
            appliedFocus = focusPart;
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Toàn cảnh"))
        {
            focusSelection = false;
            var all = GetContentsBounds();
            orbitTarget = all.center;
            orbitDist = FitDistance(all);
        }
        GUI.enabled = focusPart != null;
        if (GUILayout.Button("Tới part đã bấm"))
        {
            focusSelection = true;
            var pb = GetPartBounds(focusPart);
            orbitTarget = pb.center;
            orbitDist = FitDistance(pb) * 2.5f;
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        // Nút xoay/lăn khi không kéo chuột được
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("◀")) orbitYaw -= 15f;
        if (GUILayout.Button("▶")) orbitYaw += 15f;
        if (GUILayout.Button("▲")) orbitPitch = Mathf.Clamp(orbitPitch + 10f, -80f, 80f);
        if (GUILayout.Button("▼")) orbitPitch = Mathf.Clamp(orbitPitch - 10f, -80f, 80f);
        if (GUILayout.Button("＋")) orbitDist = Mathf.Max(0.5f, orbitDist * 0.85f);
        if (GUILayout.Button("－")) orbitDist *= 1.18f;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField(focusPart != null ? focusPart.name + " (sáng) — còn lại dim" : "(chưa chọn model)", EditorStyles.miniLabel);

        Rect r = GUILayoutUtility.GetRect(10, 320, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (r.width <= 0 || r.height <= 0) return;

        // Kéo chuột trong khung để xoay, lăn chuột để zoom
        var e = Event.current;
        if (r.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDrag && e.button == 0)
            {
                orbitYaw -= e.delta.x * 0.4f;
                orbitPitch = Mathf.Clamp(orbitPitch - e.delta.y * 0.4f, -80f, 80f);
                e.Use();
            }
            else if (e.type == EventType.ScrollWheel)
            {
                orbitDist = Mathf.Clamp(orbitDist * Mathf.Pow(1.12f, e.delta.y / 3f), 0.5f, 200f);
                e.Use();
            }
        }

        var cam = previewUtil.camera;
        cam.aspect = r.width / r.height;
        cam.nearClipPlane = 0.01f;
        if (orbitDist <= 0f)
        {
            var all = GetContentsBounds();
            orbitTarget = all.center;
            orbitDist = FitDistance(all);
        }
        float yawR = orbitYaw * Mathf.Deg2Rad;
        float pitchR = orbitPitch * Mathf.Deg2Rad;
        Vector3 dir = new Vector3(
            Mathf.Sin(yawR) * Mathf.Cos(pitchR),
            Mathf.Sin(pitchR),
            Mathf.Cos(yawR) * Mathf.Cos(pitchR));
        cam.transform.position = orbitTarget + dir * orbitDist;
        cam.transform.rotation = Quaternion.LookRotation(-dir);
        cam.farClipPlane = Mathf.Max(10f, orbitDist * 10f);

        // Khung highlight ôm đúng part được bấm
        bool showBox = focusSelection && focusPart != null;
        highlightCube.SetActive(showBox);
        if (showBox)
        {
            Bounds pb = GetPartBounds(focusPart);
            highlightCube.transform.position = pb.center;
            highlightCube.transform.rotation = focusPart.transform.rotation;
            highlightCube.transform.localScale = pb.size + Vector3.one * Mathf.Max(0.05f, pb.size.magnitude * 0.08f);
        }

        previewUtil.BeginPreview(r, GUIStyle.none);
        previewUtil.Render();
        GUI.DrawTexture(r, previewUtil.EndPreview(), ScaleMode.StretchToFill, false);
    }

    private float FitDistance(Bounds b)
    {
        float maxExtent = Mathf.Max(b.size.x, b.size.y, b.size.z, 0.5f);
        var cam = previewUtil.camera;
        return maxExtent / Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * 1.1f;
    }

    private void ApplyDimAll()
    {
        foreach (var part in shownParts)
        {
            if (part == null) continue;
            var rend = part.GetComponent<MeshRenderer>();
            if (rend == null) continue;
            SetDim(rend, focusPart != null && part != focusPart);
        }
    }

    private static void SetDim(MeshRenderer rend, bool dimmed)
    {
        rend.GetPropertyBlock(dimBlock);
        var mat = rend.sharedMaterial;
        bool isNew = mat != null && mat.shader != null && mat.shader.name.Contains("YarnDissolve");
        Color mul = dimmed ? new Color(0.45f, 0.45f, 0.45f, 1f) : Color.white;
        dimBlock.SetColor(ColorID, mul);
        if (!isNew && mat != null)
        {
            // Shader Amplify cũ không nhân _Color: dim trực tiếp 2 màu sợi
            if (mat.HasProperty(MainColorID))
            {
                var c = mat.GetColor(MainColorID);
                dimBlock.SetColor(MainColorID, dimmed ? c * 0.45f : c);
            }
            if (mat.HasProperty(DarkThreadColorID))
            {
                var c = mat.GetColor(DarkThreadColorID);
                dimBlock.SetColor(DarkThreadColorID, dimmed ? c * 0.45f : c);
            }
        }
        rend.SetPropertyBlock(dimBlock);
    }

    private static Bounds GetPartBounds(YarnPart part)
    {
        var rend = part.GetComponent<MeshRenderer>();
        if (rend != null) return rend.bounds;
        return new Bounds(part.transform.position, Vector3.one * 0.3f);
    }

    private Bounds GetContentsBounds()
    {
        var rends = contentsRoot.GetComponentsInChildren<MeshRenderer>(true);
        if (rends.Length == 0) return new Bounds(Vector3.zero, Vector3.one);
        var b = rends[0].bounds;
        for (int i = 1; i < rends.Length; i++)
            b.Encapsulate(rends[i].bounds);
        if (b.size.magnitude < 0.01f) b.size = Vector3.one;
        return b;
    }

    private void ConvertPicked()
    {
        var sb = new StringBuilder();
        int decorLayer = LayerMask.NameToLayer("Decoration");
        if (decorLayer < 0) decorLayer = 0;

        var holder = contentsRoot.GetComponent<LevelYarnInfoHolder>();
        if (holder == null) holder = contentsRoot.GetComponentInChildren<LevelYarnInfoHolder>(true);
        if (holder == null)
        {
            report = "[LỖI] Prefab không có LevelYarnInfoHolder.";
            return;
        }

        int done = 0, childDeco = 0;
        foreach (var part in picked.ToList())
        {
            if (part == null) continue;
            var go = part.gameObject;
            // Con cháu là YarnDecoration thì cũng gỡ (cha không dissolve nữa nên Drop không bao giờ chạy)
            foreach (var d in go.GetComponentsInChildren<YarnDecoration>(true))
            {
                Object.DestroyImmediate(d);
                childDeco++;
            }
            Object.DestroyImmediate(part);
            go.layer = decorLayer;
            EditorUtility.SetDirty(go);
            holder.allBaseYarnParts.RemoveAll(p => p == null || p.gameObject == go);
            holder.totalYarnDecorations++;
            done++;
        }

        holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
        holder.UpdateDebugInfo();
        EditorUtility.SetDirty(holder);
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, contentsPath);
        AssetDatabase.SaveAssets();

        sb.AppendLine($"[OK] Đã chuyển {done} mesh thành decor (+ gỡ {childDeco} YarnDecoration con).");
        sb.AppendLine($"Holder: còn {holder.allBaseYarnParts.Count} len, tổng sợi {holder.totalYarnPartCount}, decor {holder.totalYarnDecorations}.");
        sb.AppendLine("Mở prefab kiểm tra lại rồi chạy Validate quét full.");
        report = sb.ToString();
        Debug.Log(report);
        RefreshList();
    }
}
