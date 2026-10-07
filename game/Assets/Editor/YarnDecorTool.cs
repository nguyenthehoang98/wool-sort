using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using _Game.Scripts;
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

    private int toolMode; // 0 = chuyển len thành decor, 1 = gắn decor vào len
    private YarnPart attachParent;
    private int attachTypeIndex; // 0 = YarnDecoration, 1 = SpecialDecor
    private readonly List<GameObject> orphanDecors = new List<GameObject>();
    private readonly HashSet<GameObject> attachPicked = new HashSet<GameObject>();
    private bool showOnlyMissingDecor = true; // chỉ hiện len chưa có decor con
    private YarnColor revertColor = YarnColor.Blue; // màu khi hoàn tác decor tĩnh thành len
    private readonly HashSet<GameObject> revertPicked = new HashSet<GameObject>();

    // Panel preview prefab bên phải
    private PreviewRenderUtility previewUtil;
    private GameObject highlightGO; // mesh overlay đúng dáng model (thay cho khối cube sai dáng cũ)
    private Material highlightMat;
    private YarnPart focusPart;
    private GameObject focusDecorGO;
    private int appliedFocusKey;
    private bool focusSelection = true;
    private bool contentsInPreview;
    // Orbit camera preview
    private float orbitYaw = 160f;
    private float orbitPitch = 20f;
    private float orbitDist = -1f;
    private Vector3 orbitTarget;
    private bool isolateFocus; // ẩn các part khác để thấy model bên trong
    private readonly HashSet<MeshRenderer> hiddenByIsolate = new HashSet<MeshRenderer>();
    private Vector2 mouseDownPos;
    private bool mouseDownInPreview;
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
        if (highlightGO != null) Object.DestroyImmediate(highlightGO);
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
        // Overlay đúng dáng model: clone mesh của part/decor đang focus, không dùng khối Cube nữa
        highlightGO = new GameObject("decor highlight");
        highlightGO.hideFlags = HideFlags.HideAndDontSave;
        highlightGO.AddComponent<MeshFilter>();
        var hrend = highlightGO.AddComponent<MeshRenderer>();
        hrend.sharedMaterial = highlightMat;
        hrend.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        hrend.receiveShadows = false;
        highlightGO.SetActive(false);
        previewUtil.AddSingleGO(highlightGO);
    }

    private void UnloadContents()
    {
        ClearIsolate();
        contentsInPreview = false;
        if (contentsRoot != null)
        {
            PrefabUtility.UnloadPrefabContents(contentsRoot);
            contentsRoot = null;
            contentsPath = null;
        }
        shownParts.Clear();
        picked.Clear();
        orphanDecors.Clear();
        attachPicked.Clear();
        revertPicked.Clear();
        previewCache.Clear();
    }

    private void RefreshList()
    {
        ClearIsolate();
        UnloadContents();
        focusPart = null;
        focusDecorGO = null;
        appliedFocusKey = 0;
        focusSelection = false;
        orbitDist = -1f;
        if (levelPrefabAsset == null) return;
        contentsPath = AssetDatabase.GetAssetPath(levelPrefabAsset);
        if (string.IsNullOrEmpty(contentsPath)) return;
        contentsRoot = PrefabUtility.LoadPrefabContents(contentsPath);
        shownParts.AddRange(contentsRoot.GetComponentsInChildren<YarnPart>(true)
            .OrderBy(p => p.name));
        // Decor mồ côi: có Mesh nhưng không có YarnPart cha (kể cả đã có script decor mà chưa gắn)
        var partSet = new HashSet<YarnPart>(shownParts);
        foreach (var mf in contentsRoot.GetComponentsInChildren<MeshFilter>(true))
        {
            var go = mf.gameObject;
            if (go.GetComponent<YarnPart>() != null) continue;
            if (go.GetComponentInParent<YarnPart>() != null) continue;
            orphanDecors.Add(go);
        }
        orphanDecors.Sort((a, b) => string.Compare(a.name, b.name));
    }

    private void OnGUI()
    {
        toolMode = GUILayout.Toolbar(toolMode, new[] { "Chuyển len thành decor", "Gắn decor vào len" });

        EditorGUI.BeginChangeCheck();
        levelPrefabAsset = (GameObject)EditorGUILayout.ObjectField("Prefab level", levelPrefabAsset, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
            RefreshList();

        if (toolMode == 0)
            DrawConvertMode();
        else
            DrawAttachMode();

        EditorGUILayout.TextArea(report, GUILayout.MinHeight(90));
    }

    private void DrawConvertMode()
    {
        GUILayout.Label("CHUYỂN LEN THÀNH DECOR", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Mesh decor sẽ: gỡ YarnPart (không bấm/dissolve), sang layer Decoration\n" +
            "(tap không trúng), gỡ khỏi danh sách len của Holder, đếm lại count.",
            MessageType.Info);

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Tải lại danh sách")) RefreshList();
        if (GUILayout.Button("Chọn hết (theo lọc)")) { foreach (var p in shownParts) { if (p == null) continue; if (showOnlyMissingDecor && CountAttachedDecors(p) > 0) continue; picked.Add(p); } }
        if (GUILayout.Button("Bỏ chọn hết")) picked.Clear();
        EditorGUILayout.EndHorizontal();

        showOnlyMissingDecor = EditorGUILayout.ToggleLeft("Chỉ hiện len thiếu decor (click hình chỉ xem, checkbox mới tick)", showOnlyMissingDecor);
        int missingCount = shownParts.Count(p => p != null && CountAttachedDecors(p) == 0);
        EditorGUILayout.LabelField($"Len: {shownParts.Count} (thiếu decor: {missingCount}) | Đã tick: {picked.Count}");
        DrawSplitPane(drawAttachGrid: false);
        DrawRevertSection();

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
    }

    private void DrawAttachMode()
    {
        GUILayout.Label("GẮN DECOR VÀO LEN (setup chuẩn)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Chọn len cha + tick các mesh mồ côi -> tự set parent, layer YarnPart,\n" +
            "MeshCollider (convex) + Rigidbody (kinematic) + script YarnDecoration/SpecialDecor.",
            MessageType.Info);

        attachParent = (YarnPart)EditorGUILayout.ObjectField("Len cha (Parent YarnPart)", attachParent, typeof(YarnPart), true);
        attachTypeIndex = EditorGUILayout.Popup("Loại decor", attachTypeIndex, new[] { "YarnDecoration (rơi theo dissolve)", "SpecialDecor (rơi ở layer cuối)" });
        DrawParentQuickPick();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Tải lại danh sách")) RefreshList();
        if (GUILayout.Button("Chọn hết decor")) { foreach (var d in orphanDecors) attachPicked.Add(d); }
        if (GUILayout.Button("Bỏ chọn")) attachPicked.Clear();
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField($"Decor mồ côi: {orphanDecors.Count} | Đã tick: {attachPicked.Count} | Len: {shownParts.Count}");
        DrawSplitPane(drawAttachGrid: true);

        if (AssetPreview.IsLoadingAssetPreviews())
            Repaint();

        GUI.enabled = contentsRoot != null && attachPicked.Count > 0 && attachParent != null;
        if (GUILayout.Button($"GẮN {attachPicked.Count} DECOR VÀO {((attachParent != null) ? attachParent.name : "?")}", GUILayout.Height(32)))
            AttachPicked();
        GUI.enabled = true;
    }

    private void DrawSplitPane(bool drawAttachGrid)
    {
        EditorGUILayout.BeginHorizontal();
        float halfW = Mathf.Max(300f, position.width * 0.5f);
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        float gridWidth = Mathf.Max(320f, position.width - halfW - 30f);
        int cols = Mathf.Max(2, (int)(gridWidth / 160));
        scroll = EditorGUILayout.BeginScrollView(scroll);
        int col = 0;
        EditorGUILayout.BeginHorizontal();
        if (drawAttachGrid)
        {
            foreach (var go in orphanDecors)
            {
                if (go == null) continue;
                if (col >= cols)
                {
                    col = 0;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                col++;
                DrawDecorCell(go);
            }
        }
        else
        {
            foreach (var part in shownParts)
            {
                if (part == null) continue;
                if (showOnlyMissingDecor && CountAttachedDecors(part) > 0) continue;
                if (col >= cols)
                {
                    col = 0;
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.BeginHorizontal();
                }
                col++;
                DrawPartCell(part);
            }
        }
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(halfW));
        DrawPrefabPane();
        EditorGUILayout.EndVertical();
        EditorGUILayout.EndHorizontal();
    }

    private void DrawDecorCell(GameObject go)
    {
        bool tick = attachPicked.Contains(go);
        var mf = go.GetComponent<MeshFilter>();
        var mesh = mf != null ? mf.sharedMesh : null;
        var bg = tick ? new Color(0.3f, 0.7f, 0.3f, 0.35f) : new Color(0.5f, 0.5f, 0.5f, 0.15f);
        var rect = EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150), GUILayout.Height(172));
        EditorGUI.DrawRect(rect, bg);
        Texture prev = null;
        if (mesh != null && !previewCache.TryGetValue(mesh, out prev))
        {
            prev = AssetPreview.GetAssetPreview(mesh);
            previewCache[mesh] = prev;
        }
        // Click hình chỉ xem preview (select), tick checkbox mới chọn
        if (GUILayout.Button(prev != null ? new GUIContent(prev) : new GUIContent("đang tải..."), GUILayout.Height(96)))
        {
            focusDecorGO = go;
            focusPart = null;
            focusSelection = true;
            var b = GetGoBounds(go);
            orbitTarget = b.center;
            orbitDist = FitDistance(b) * 2.5f;
        }
        bool next = EditorGUILayout.ToggleLeft(go.name, tick);
        if (next != tick)
        {
            if (next) attachPicked.Add(go);
            else attachPicked.Remove(go);
        }
        EditorGUILayout.LabelField($"layer {go.layer}", EditorStyles.miniLabel);
        EditorGUILayout.EndVertical();
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

        // Click hình chỉ xem preview (select), tick checkbox mới chọn
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
            // Bấm vào model nào thì tab prefab sáng đúng chỗ đó + camera bay tới
            focusPart = part;
            focusDecorGO = null;
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
        int decoCount = CountAttachedDecors(part);
        EditorGUILayout.LabelField(decoCount == 0 ? "thiếu decor" : $"đã có {decoCount} decor",
            decoCount == 0 ? EditorStyles.boldLabel : EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"{part.initColor} | inner {part.totalInnerCount}", EditorStyles.miniLabel);
        EditorGUILayout.LabelField($"vtx {vtx} | layer {part.gameObject.layer}", EditorStyles.miniLabel);
        // Revert: gỡ decor đã gắn ra thành mồ côi
        if (decoCount > 0 && GUILayout.Button("Gỡ decor", GUILayout.Height(20)))
            DetachDecors(part);
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

        if (focusDecorGO == null)
            focusDecorGO = null; // dọn ref đã bị xóa (Unity null)
        if ((focusPart == null || focusPart.gameObject == null) && focusDecorGO == null)
        {
            focusPart = null;
            focusSelection = false;
        }

        // Click item nào: item đó sáng 100%, các item khác mờ dần theo lớp khoảng cách
        // (lớp gần nhất 80%, lớp 2 60%, lớp 3 40%, còn lại 20%) — per-renderer, không đụng asset
        int focusKey = CurrentFocusKey();
        if (appliedFocusKey != focusKey)
        {
            ApplyFocusFade();
            appliedFocusKey = focusKey;
        }
        ApplyIsolate();
        isolateFocus = EditorGUILayout.ToggleLeft("Cô lập part đang chọn (ẩn còn lại để thấy model bên trong)", isolateFocus);
        if (isolateFocus && !focusSelection)
            EditorGUILayout.HelpBox("Tick 1 model ở lưới bên trái (hoặc click vào model trong khung preview) để cô lập nó.", MessageType.Info);

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

        string focusName = focusPart != null ? focusPart.name : (focusDecorGO != null ? focusDecorGO.name : null);
        EditorGUILayout.LabelField(focusName != null ? focusName + " (100%) — lớp gần 80% / 60% / 40% / xa 20%" : "(chưa chọn model — click 1 item để làm mờ các cái khác)", EditorStyles.miniLabel);

        Rect r = GUILayoutUtility.GetRect(10, 320, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        if (r.width <= 0 || r.height <= 0) return;

        // Chuột trái: xoay. Giữ chuột giữa/phải kéo: di chuyển model (pan). Lăn: zoom. Click: chọn model bên trong.
        var e = Event.current;
        if (r.Contains(e.mousePosition))
        {
            if (e.type == EventType.MouseDown && (e.button == 0 || e.button == 1 || e.button == 2))
            {
                mouseDownPos = e.mousePosition;
                mouseDownInPreview = true;
            }
            else if (e.type == EventType.MouseDrag && mouseDownInPreview)
            {
                if (e.button == 0)
                {
                    orbitYaw -= e.delta.x * 0.4f;
                    orbitPitch = Mathf.Clamp(orbitPitch - e.delta.y * 0.4f, -80f, 80f);
                    e.Use();
                    Repaint();
                }
                else if (e.button == 1 || e.button == 2)
                {
                    // Pan theo mặt phẳng camera, tốc độ tỉ lệ khoảng cách
                    var camT = previewUtil.camera.transform;
                    float panScale = Mathf.Max(0.001f, orbitDist / Mathf.Max(1f, r.height)) * 1.1f;
                    orbitTarget -= camT.right * e.delta.x * panScale;
                    orbitTarget += camT.up * e.delta.y * panScale;
                    e.Use();
                    Repaint();
                }
            }
            else if (e.type == EventType.MouseUp && mouseDownInPreview)
            {
                bool isClick = Vector2.Distance(mouseDownPos, e.mousePosition) < 5f;
                mouseDownInPreview = false;
                if (isClick && e.button == 0)
                {
                    PickFocusAt(e.mousePosition, r);
                    e.Use();
                }
            }
            else if (e.type == EventType.ScrollWheel)
            {
                orbitDist = Mathf.Clamp(orbitDist * Mathf.Pow(1.12f, e.delta.y / 3f), 0.5f, 200f);
                e.Use();
                Repaint();
            }
        }
        else if (e.type == EventType.MouseUp)
        {
            mouseDownInPreview = false;
        }

        // Chỉ render preview ở event Repaint. Render ở Layout/MouseMove sẽ làm
        // PreviewRenderUtility mất trạng thái -> khung preview lúc hiện lúc ẩn (chớp).
        if (e.type != EventType.Repaint)
            return;

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

        // Highlight đúng dáng model: overlay clone mesh của part/decor đang focus
        Transform focusT = focusPart != null ? focusPart.transform : (focusDecorGO != null ? focusDecorGO.transform : null);
        bool showBox = focusSelection && focusT != null;
        highlightGO.SetActive(showBox);
        if (showBox)
        {
            var srcMf = focusT.GetComponent<MeshFilter>();
            var hlMf = highlightGO.GetComponent<MeshFilter>();
            hlMf.sharedMesh = srcMf != null ? srcMf.sharedMesh : null;
            highlightGO.transform.position = focusT.position;
            highlightGO.transform.rotation = focusT.rotation;
            highlightGO.transform.localScale = focusT.lossyScale * 1.02f;
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

    // Ẩn các renderer khác để nhìn thấy model bên trong (chỉ ẩn tạm trong preview, restore trước khi lưu)
    private void ApplyIsolate()
    {
        ClearIsolate(restoreOnly: true);
        Transform focusT = focusPart != null ? focusPart.transform : (focusDecorGO != null ? focusDecorGO.transform : null);
        if (!isolateFocus || !focusSelection || focusT == null || contentsRoot == null) return;
        var keep = new HashSet<Transform>();
        for (var t = focusT; t != null && t.gameObject != contentsRoot; t = t.parent) keep.Add(t);
        foreach (var rend in contentsRoot.GetComponentsInChildren<MeshRenderer>(true))
        {
            bool insideFocus = false;
            for (var t = rend.transform; t != null; t = t.parent)
            {
                if (keep.Contains(t)) { insideFocus = true; break; }
            }
            if (!insideFocus && rend.enabled)
            {
                rend.enabled = false;
                hiddenByIsolate.Add(rend);
            }
        }
        // Overlay highlight cũng ẩn theo để khỏi che
        if (highlightGO != null) highlightGO.SetActive(false);
    }

    private void ClearIsolate(bool restoreOnly = false)
    {
        foreach (var rend in hiddenByIsolate)
        {
            if (rend != null) rend.enabled = true;
        }
        hiddenByIsolate.Clear();
    }

    // Click vào model trong khung preview để chọn (kể cả model bên trong)
    private void PickFocusAt(Vector2 mousePos, Rect r)
    {
        if (previewUtil == null || previewUtil.camera == null || contentsRoot == null) return;
        var cam = previewUtil.camera;
        Vector2 local = mousePos - new Vector2(r.x, r.y);
        Vector2 screenPt = new Vector2(local.x, r.height - local.y);
        float bestScore = 40f; // ngưỡng pixel
        YarnPart bestPart = null;
        GameObject bestDecor = null;
        float bestDepth = float.MaxValue;
        void Consider(Vector3 worldPos, float depth, YarnPart p, GameObject d)
        {
            Vector3 sp = cam.WorldToScreenPoint(worldPos);
            if (sp.z < 0) return;
            float dist = Vector2.Distance(new Vector2(sp.x, sp.y), screenPt);
            if (dist < bestScore || (Mathf.Approximately(dist, bestScore) && depth < bestDepth))
            {
                bestScore = dist;
                bestDepth = depth;
                bestPart = p;
                bestDecor = d;
            }
        }
        foreach (var p in shownParts)
        {
            if (p == null) continue;
            var b = GetPartBounds(p);
            float depth = Vector3.Distance(cam.transform.position, b.center);
            Consider(b.center, depth, p, null);
        }
        foreach (var go in orphanDecors)
        {
            if (go == null) continue;
            var b = GetGoBounds(go);
            float depth = Vector3.Distance(cam.transform.position, b.center);
            Consider(b.center, depth, null, go);
        }
        // Cả decor đã gắn (con của part) cũng cho click được
        foreach (var d in contentsRoot.GetComponentsInChildren<YarnDecoration>(true))
        {
            var b = GetGoBounds(d.gameObject);
            float depth = Vector3.Distance(cam.transform.position, b.center);
            Consider(b.center, depth, null, d.gameObject);
        }
        foreach (var s in contentsRoot.GetComponentsInChildren<SpecialDecor>(true))
        {
            var b = GetGoBounds(s.gameObject);
            float depth = Vector3.Distance(cam.transform.position, b.center);
            Consider(b.center, depth, null, s.gameObject);
        }
        if (bestPart != null || bestDecor != null)
        {
            focusPart = bestPart;
            focusDecorGO = bestPart != null ? null : bestDecor;
            focusSelection = true;
            Transform t = bestPart != null ? bestPart.transform : bestDecor.transform;
            var b = bestPart != null ? GetPartBounds(bestPart) : GetGoBounds(bestDecor);
            orbitTarget = b.center;
            ApplyFocusFade();
            appliedFocusKey = CurrentFocusKey();
            Repaint();
        }
    }

    private int CurrentFocusKey()
    {
        int key = focusSelection ? 1 : 0;
        key = key * 397 ^ (focusPart != null ? focusPart.GetInstanceID() : 0);
        key = key * 397 ^ (focusDecorGO != null ? focusDecorGO.GetInstanceID() : 0);
        return key;
    }

    // Item focus sáng 100%, các renderer khác mờ dần theo lớp khoảng cách tới focus:
    // lớp gần nhất 80%, lớp 2 60%, lớp 3 40%, xa nhất 20%.
    private void ApplyFocusFade()
    {
        if (contentsRoot == null) return;
        Transform focusT = focusPart != null ? focusPart.transform : (focusDecorGO != null ? focusDecorGO.transform : null);
        var rends = contentsRoot.GetComponentsInChildren<MeshRenderer>(true);
        if (focusT == null || !focusSelection)
        {
            foreach (var r in rends) SetDim(r, 1f);
            return;
        }
        Vector3 center = GetGoBounds(focusT.gameObject).center;
        var others = new List<(MeshRenderer r, float d)>();
        foreach (var r in rends)
        {
            if (r == null) continue;
            // Bản thân focus + con cháu của nó giữ nguyên 100%
            if (r.transform == focusT || r.transform.IsChildOf(focusT))
            {
                SetDim(r, 1f);
                continue;
            }
            others.Add((r, Vector3.Distance(r.bounds.center, center)));
        }
        others.Sort((a, b) => a.d.CompareTo(b.d));
        int n = others.Count;
        for (int i = 0; i < n; i++)
        {
            // Chia thành 4 lớp theo thứ hạng khoảng cách: 80% / 60% / 40% / 20%
            int tier = n <= 1 ? 0 : Mathf.Min(3, (int)((float)i / n * 4f));
            SetDim(others[i].r, 0.8f - tier * 0.2f);
        }
    }

    private static void SetDim(MeshRenderer rend, float brightness)
    {
        if (rend == null) return;
        rend.GetPropertyBlock(dimBlock);
        var mat = rend.sharedMaterial;
        bool isNew = mat != null && mat.shader != null && mat.shader.name.Contains("YarnDissolve");
        brightness = Mathf.Clamp01(brightness);
        Color mul = new Color(brightness, brightness, brightness, 1f);
        dimBlock.SetColor(ColorID, mul);
        if (!isNew && mat != null)
        {
            // Shader Amplify cũ không nhân _Color: dim trực tiếp 2 màu sợi
            if (mat.HasProperty(MainColorID))
            {
                var c = mat.GetColor(MainColorID);
                dimBlock.SetColor(MainColorID, dimmed2(c, brightness));
            }
            if (mat.HasProperty(DarkThreadColorID))
            {
                var c = mat.GetColor(DarkThreadColorID);
                dimBlock.SetColor(DarkThreadColorID, dimmed2(c, brightness));
            }
        }
        rend.SetPropertyBlock(dimBlock);
    }

    private static Color dimmed2(Color c, float b) => new Color(c.r * b, c.g * b, c.b * b, c.a);

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
        ClearIsolate();
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, contentsPath);
        AssetDatabase.SaveAssets();

        sb.AppendLine($"[OK] Đã chuyển {done} mesh thành decor (+ gỡ {childDeco} YarnDecoration con).");
        sb.AppendLine($"Holder: còn {holder.allBaseYarnParts.Count} len, tổng sợi {holder.totalYarnPartCount}, decor {holder.totalYarnDecorations}.");
        sb.AppendLine("Mở prefab kiểm tra lại rồi chạy Validate quét full.");
        report = sb.ToString();
        Debug.Log(report);
        RefreshList();
    }

    private static Bounds GetGoBounds(GameObject go)
    {
        var rend = go.GetComponent<MeshRenderer>();
        if (rend != null) return rend.bounds;
        return new Bounds(go.transform.position, Vector3.one * 0.3f);
    }

    private void AttachPicked()
    {
        if (contentsRoot == null || attachParent == null || attachPicked.Count == 0)
        {
            report = "[LỖI] Chưa nạp prefab / chưa chọn len cha / chưa tick decor.";
            return;
        }
        // attachParent phải thuộc cùng prefab contents
        var host = attachParent;
        if (!shownParts.Contains(host))
        {
            report = "[LỖI] Len cha không thuộc prefab đang mở. Chọn lại len trong list bên trái.";
            return;
        }
        int yarnLayer = LayerMask.NameToLayer("YarnPart");
        bool wantSpecial = attachTypeIndex == 1;
        int done = 0;
        foreach (var go in attachPicked.ToList())
        {
            if (go == null) continue;
            go.transform.SetParent(host.transform, worldPositionStays: true);
            go.layer = yarnLayer;
            foreach (Transform c in go.transform) c.gameObject.layer = yarnLayer;

            var yd = go.GetComponent<YarnDecoration>();
            var sd = go.GetComponent<SpecialDecor>();
            if (wantSpecial && yd != null) Object.DestroyImmediate(yd);
            if (!wantSpecial && sd != null) Object.DestroyImmediate(sd);

            var col = go.GetComponent<MeshCollider>();
            if (col == null) col = go.AddComponent<MeshCollider>();
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null) col.sharedMesh = mf.sharedMesh;
            col.convex = true;

            var rb = go.GetComponent<Rigidbody>();
            if (rb == null) rb = go.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearDamping = 1.36f;
            rb.angularDamping = 1.36f;

            if (wantSpecial)
            {
                if (go.GetComponent<SpecialDecor>() == null) go.AddComponent<SpecialDecor>();
            }
            else
            {
                var dec = go.GetComponent<YarnDecoration>() ?? go.AddComponent<YarnDecoration>();
                if (!host.decorations.Contains(dec)) host.decorations.Add(dec);
            }
            EditorUtility.SetDirty(go);
            done++;
        }
        EditorUtility.SetDirty(host);
        ClearIsolate();
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, contentsPath);
        AssetDatabase.SaveAssets();
        report = $"[OK] Đã gắn chuẩn {done} decor loại {(wantSpecial ? "SpecialDecor" : "YarnDecoration")} vào {host.name}.\nLưu prefab xong, chạy Validate decor gắn len để kiểm tra.";
        Debug.Log(report);
        RefreshList();
    }

    private static int CountAttachedDecors(YarnPart part)
    {
        if (part == null) return 0;
        return part.GetComponentsInChildren<YarnDecoration>(true).Length
             + part.GetComponentsInChildren<SpecialDecor>(true).Length;
    }

    // Revert 1: gỡ decor đã gắn ra thành mồ côi (tách khỏi len)
    private void DetachDecors(YarnPart host)
    {
        if (contentsRoot == null || host == null) return;
        var decos = host.GetComponentsInChildren<YarnDecoration>(true).Cast<Component>()
            .Concat(host.GetComponentsInChildren<SpecialDecor>(true)).ToList();
        if (decos.Count == 0) return;
        int decorLayer = LayerMask.NameToLayer("Decoration");
        var hostParent = host.transform.parent;
        foreach (var d in decos)
        {
            if (d == null) continue;
            var go = d.gameObject;
            host.decorations.RemoveAll(x => x == null || x.gameObject == go);
            Object.DestroyImmediate(d, true);
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null) Object.DestroyImmediate(rb, true);
            go.transform.SetParent(hostParent, worldPositionStays: true);
            go.layer = decorLayer;
            EditorUtility.SetDirty(go);
        }
        EditorUtility.SetDirty(host);
        ClearIsolate();
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, contentsPath);
        AssetDatabase.SaveAssets();
        report = $"[OK] Đã gỡ {decos.Count} decor khỏi {host.name} (giờ là mồ côi layer Decoration).";
        Debug.Log(report);
        RefreshList();
    }

    // Chọn nhanh len cha trong tab gắn: chỉ hiện len thiếu decor
    private void DrawParentQuickPick()
    {
        var missing = shownParts.Where(p => p != null && CountAttachedDecors(p) == 0).OrderBy(p => p.name).ToList();
        if (missing.Count == 0)
        {
            EditorGUILayout.HelpBox("Tất cả len đã có decor.", MessageType.Info);
            return;
        }
        string[] names = missing.Select(p => p.name).ToArray();
        int cur = missing.IndexOf(attachParent);
        int sel = EditorGUILayout.Popup("Chọn nhanh len thiếu decor", Mathf.Max(0, cur), names);
        if (missing.Count > 0 && (cur < 0 || sel != cur))
        {
            attachParent = missing[Mathf.Clamp(sel, 0, missing.Count - 1)];
            focusPart = attachParent;
            focusDecorGO = null;
            focusSelection = true;
            var pb = GetPartBounds(attachParent);
            orbitTarget = pb.center;
            orbitDist = FitDistance(pb) * 2.5f;
        }
    }

    // Revert 2: hoàn tác decor tĩnh thành len (cần chọn màu)
    private void DrawRevertSection()
    {
        if (orphanDecors.Count == 0) return;
        EditorGUILayout.Space(4);
        GUILayout.Label("HOÀN TÁC DECOR TĨNH THÀNH LEN (revert)", EditorStyles.boldLabel);
        revertColor = (YarnColor)EditorGUILayout.EnumPopup("Màu cho len hoàn tác", revertColor);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Tick hết decor tĩnh")) { foreach (var d in orphanDecors) revertPicked.Add(d); }
        if (GUILayout.Button("Bỏ tick")) revertPicked.Clear();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"Decor tĩnh: {orphanDecors.Count} | Đã tick revert: {revertPicked.Count} (click hình chỉ xem, checkbox mới tick)");
        EditorGUILayout.BeginHorizontal();
        int col = 0;
        foreach (var go in orphanDecors)
        {
            if (go == null) continue;
            if (col >= 4) { col = 0; EditorGUILayout.EndHorizontal(); EditorGUILayout.BeginHorizontal(); }
            col++;
            EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.Width(150));
            bool tick = revertPicked.Contains(go);
            var mf = go.GetComponent<MeshFilter>();
            var mesh = mf != null ? mf.sharedMesh : null;
            Texture prev = null;
            if (mesh != null && !previewCache.TryGetValue(mesh, out prev))
            {
                prev = AssetPreview.GetAssetPreview(mesh);
                previewCache[mesh] = prev;
            }
            if (GUILayout.Button(prev != null ? new GUIContent(prev) : new GUIContent("đang tải..."), GUILayout.Height(72)))
            {
                focusDecorGO = go;
                focusPart = null;
                focusSelection = true;
                var b = GetGoBounds(go);
                orbitTarget = b.center;
                orbitDist = FitDistance(b) * 2.5f;
            }
            bool next = EditorGUILayout.ToggleLeft(go.name, tick);
            if (next != tick)
            {
                if (next) revertPicked.Add(go);
                else revertPicked.Remove(go);
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();
        if (AssetPreview.IsLoadingAssetPreviews())
            Repaint();
        GUI.enabled = revertPicked.Count > 0;
        if (GUILayout.Button($"HOÀN TÁC {revertPicked.Count} DECOR THÀNH LEN ({revertColor})", GUILayout.Height(28)))
            RevertToYarn();
        GUI.enabled = true;
    }

    private void RevertToYarn()
    {
        if (contentsRoot == null || revertPicked.Count == 0) return;
        var holder = contentsRoot.GetComponent<LevelYarnInfoHolder>();
        if (holder == null) holder = contentsRoot.GetComponentInChildren<LevelYarnInfoHolder>(true);
        if (holder == null) { report = "[LỖI] Prefab không có LevelYarnInfoHolder."; return; }
        if (!EditorUtility.DisplayDialog("Hoàn tác thành len?",
            $"Biến {revertPicked.Count} decor tĩnh thành len màu {revertColor} (layer YarnPart, vào danh sách Holder)?",
            "Hoàn tác", "Hủy")) return;
        int yarnLayer = LayerMask.NameToLayer("YarnPart");
        int done = 0;
        foreach (var go in revertPicked.ToList())
        {
            if (go == null) continue;
            if (go.GetComponent<YarnPart>() != null) continue;
            go.layer = yarnLayer;
            var col = go.GetComponent<MeshCollider>();
            if (col == null) col = go.AddComponent<MeshCollider>();
            var mf = go.GetComponent<MeshFilter>();
            if (mf != null) col.sharedMesh = mf.sharedMesh;
            col.convex = false;
            var rb = go.GetComponent<Rigidbody>();
            if (rb != null) Object.DestroyImmediate(rb, true);
            var yd = go.GetComponent<YarnDecoration>();
            if (yd != null) Object.DestroyImmediate(yd, true);
            var sd = go.GetComponent<SpecialDecor>();
            if (sd != null) Object.DestroyImmediate(sd, true);
            var part = go.AddComponent<YarnPart>();
            part.initColor = revertColor;
            part.totalInnerCount = 0;
            part.layerIndex = 1;
            part.ancestor = part;
            part.rootPart = part;
            part.childPart = null;
            if (!holder.allBaseYarnParts.Contains(part)) holder.allBaseYarnParts.Add(part);
            holder.totalYarnDecorations = Mathf.Max(0, holder.totalYarnDecorations - 1);
            EditorUtility.SetDirty(go);
            done++;
        }
        holder.totalYarnPartCount = holder.GetTotalYarnPartCount(holder.allBaseYarnParts);
        holder.UpdateDebugInfo();
        EditorUtility.SetDirty(holder);
        ClearIsolate();
        PrefabUtility.SaveAsPrefabAsset(contentsRoot, contentsPath);
        AssetDatabase.SaveAssets();
        report = $"[OK] Đã hoàn tác {done} decor thành len màu {revertColor}.";
        Debug.Log(report);
        RefreshList();
    }
}
