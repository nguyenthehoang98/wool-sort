using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using _Game.Scripts;
using _Game.Scripts.Gameplay;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Tạo prefab level từ model FBX. Bỏ chọn các mesh trang trí.
/// Panel trong Level Studio (tab 1), viết bằng IMGUI để nhúng chung cửa sổ.
/// </summary>
public sealed class LevelQuickCreatorWindow : EditorWindow
{
    private const string TemplatePath = "Assets/Resources/Levels/Level1.prefab";
    private const string LevelsFolder = "Assets/Resources/Levels";
    private const string ColorDatabasePath = "Assets/_Game/Configs/YarnColorDatabase.asset";
    private const float TargetModelMaxSize = 6.5698f; // Average max bounds of Backup/Level1 and Backup/Level3.

    private readonly List<MeshPartChoice> parts = new List<MeshPartChoice>();
    private GameObject modelAsset;
    private string levelName;
    private Vector2 scroll;
    private string status = "Chọn model FBX để quét các mesh.";
    private MessageType statusType = MessageType.Info;
    private bool initialized;

    /// <summary>Gọi 1 lần khi Studio mở tab.</summary>
    public void InitPanel()
    {
        if (initialized) return;
        initialized = true;
        if (string.IsNullOrEmpty(levelName))
            levelName = GetNextLevelName();
    }

    public void DrawPanel() => OnGUI();

    private void OnGUI()
    {
        GUILayout.Label("Tạo prefab level từ model FBX. Bỏ chọn các mesh trang trí.", EditorStyles.boldLabel);

        EditorGUI.BeginChangeCheck();
        var picked = (GameObject)EditorGUILayout.ObjectField("Model", modelAsset, typeof(GameObject), false);
        if (EditorGUI.EndChangeCheck())
            ScanModel(picked);

        levelName = EditorGUILayout.TextField("Tên prefab", levelName ?? "");

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Chọn tất cả mesh")) SetAllParts(true);
        if (GUILayout.Button("Bỏ chọn tất cả")) SetAllParts(false);
        EditorGUILayout.EndHorizontal();

        scroll = EditorGUILayout.BeginScrollView(scroll, GUILayout.MinHeight(150));
        foreach (var part in parts)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginHorizontal();
            part.include = EditorGUILayout.Toggle(part.include, GUILayout.Width(20));
            GUILayout.Label(part.path, EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            part.color = (YarnColor)EditorGUILayout.EnumPopup("Màu", part.color);
            part.innerCount = Mathf.Max(0, EditorGUILayout.IntField("Số lớp trong", part.innerCount));
            EditorGUILayout.EndVertical();
        }
        if (parts.Count == 0)
            GUILayout.Label("Chưa có mesh nào (chọn model FBX).", EditorStyles.miniLabel);
        EditorGUILayout.EndScrollView();

        EditorGUILayout.HelpBox(status, statusType);
        if (GUILayout.Button("Tạo Level Prefab", GUILayout.Height(32)))
            CreateLevel();
    }

    private void ScanModel(GameObject selectedModel)
    {
        modelAsset = selectedModel;
        parts.Clear();

        if (modelAsset == null)
        {
            SetStatus("Chọn model FBX để quét các mesh.", MessageType.Info);
            return;
        }

        var database = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(ColorDatabasePath);
        foreach (var filter in modelAsset.GetComponentsInChildren<MeshFilter>(true))
        {
            var renderer = filter.GetComponent<MeshRenderer>();
            if (filter.sharedMesh == null || renderer == null)
                continue;

            var color = YarnColor.Blue;
            if (database != null)
            {
                foreach (var material in renderer.sharedMaterials)
                {
                    if (database.TryGetColor(material, out color))
                        break;
                    color = YarnColor.Blue;
                }
            }

            parts.Add(new MeshPartChoice
            {
                indices = GetSiblingIndices(filter.transform, modelAsset.transform),
                path = GetHierarchyPath(filter.transform, modelAsset.transform),
                color = color,
                include = true
            });
        }

        SetStatus(parts.Count == 0
            ? "Model không có MeshFilter + MeshRenderer phù hợp."
            : $"Tìm thấy {parts.Count} mesh. Kiểm tra mesh trang trí và màu trước khi tạo.",
            parts.Count == 0 ? MessageType.Error : MessageType.Info);
    }

    private void SetAllParts(bool include)
    {
        foreach (var part in parts)
            part.include = include;
    }

    private void CreateLevel()
    {
        var name = (levelName ?? "").Trim();
        if (modelAsset == null || parts.All(part => !part.include))
        {
            SetStatus("Chọn model và ít nhất một mesh gameplay.", MessageType.Error);
            return;
        }

        if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
        {
            SetStatus("Tên level không hợp lệ.", MessageType.Error);
            return;
        }

        var modelPath = AssetDatabase.GetAssetPath(modelAsset);
        var outputPath = $"{LevelsFolder}/{name}.prefab";
        if (string.IsNullOrEmpty(modelPath) || !AssetDatabase.IsValidFolder(LevelsFolder))
        {
            SetStatus("Model phải là asset trong Project và thư mục Resources/Levels phải tồn tại.", MessageType.Error);
            return;
        }

        if (string.Equals(outputPath, TemplatePath, StringComparison.OrdinalIgnoreCase))
        {
            SetStatus("Không thể ghi đè prefab mẫu Level1.", MessageType.Error);
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<GameObject>(outputPath) != null &&
            !EditorUtility.DisplayDialog("Ghi đè level?", $"{outputPath} đã tồn tại.", "Ghi đè", "Hủy"))
            return;

        var template = AssetDatabase.LoadAssetAtPath<GameObject>(TemplatePath);
        var database = AssetDatabase.LoadAssetAtPath<YarnColorDatabase>(ColorDatabasePath);
        var yarnLayer = LayerMask.NameToLayer("YarnPart");
        if (template == null || database == null || yarnLayer < 0)
        {
            SetStatus("Thiếu prefab Level1, YarnColorDatabase hoặc layer YarnPart.", MessageType.Error);
            return;
        }

        GameObject root = null;
        try
        {
            root = PrefabUtility.LoadPrefabContents(TemplatePath);
            var holder = root.GetComponent<LevelYarnInfoHolder>();
            if (holder == null || root.GetComponent<YarnPartLayerSpawner>() == null)
                throw new InvalidOperationException("Prefab Level1 thiếu LevelYarnInfoHolder hoặc YarnPartLayerSpawner.");

            var oldModel = root.transform.childCount > 0 ? root.transform.GetChild(0) : null;
            var localPosition = oldModel != null ? oldModel.localPosition : Vector3.zero;
            var localRotation = oldModel != null ? oldModel.localRotation : Quaternion.identity;
            var localScale = oldModel != null ? oldModel.localScale : Vector3.one;
            while (root.transform.childCount > 0)
                UnityEngine.Object.DestroyImmediate(root.transform.GetChild(0).gameObject);

            var modelRoot = new GameObject("ModelRoot").transform;
            modelRoot.SetParent(root.transform, false);
            modelRoot.localPosition = localPosition;
            modelRoot.localRotation = localRotation;

            var modelInstance = PrefabUtility.InstantiatePrefab(modelAsset, modelRoot) as GameObject;
            if (modelInstance == null)
                throw new InvalidOperationException("Không thể instantiate model đã chọn.");
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = localScale;
            modelRoot.localScale = Vector3.one * (TargetModelMaxSize / GetMaxRendererBoundsSize(modelInstance));
            root.name = name;

            var createdParts = new List<YarnPart>();
            foreach (var choice in parts.Where(part => part.include))
            {
                var target = Resolve(modelInstance.transform, choice.indices);
                var filter = target.GetComponent<MeshFilter>();
                var renderer = target.GetComponent<MeshRenderer>();
                var material = database.GetMaterial(choice.color);
                if (filter == null || filter.sharedMesh == null || renderer == null || material == null)
                    throw new InvalidOperationException($"Mesh hoặc material màu không hợp lệ: {choice.path} ({choice.color}).");

                target.gameObject.layer = yarnLayer;
                var collider = target.GetComponent<MeshCollider>();
                if (collider == null)
                    collider = target.gameObject.AddComponent<MeshCollider>();
                if (collider == null)
                    throw new InvalidOperationException($"Không thể tạo MeshCollider cho mesh: {choice.path}.");
                collider.sharedMesh = filter.sharedMesh;
                collider.convex = false;

                var part = target.GetComponent<YarnPart>() ?? target.gameObject.AddComponent<YarnPart>();
                part.initColor = choice.color;
                part.totalInnerCount = Mathf.Max(0, choice.innerCount);
                part.innerColorTypes = Enumerable.Repeat(InnerColorType.Fixed, part.totalInnerCount).ToList();
                part.innerFixedColors = Enumerable.Repeat(choice.color, part.totalInnerCount).ToList();
                part.ancestor = part;
                part.rootPart = part;
                part.childPart = null;
                part.layerIndex = 1;

                var materials = renderer.sharedMaterials;
                if (materials.Length == 0)
                    materials = new Material[1];
                for (var i = 0; i < materials.Length; i++)
                    materials[i] = material;
                renderer.sharedMaterials = materials;

                createdParts.Add(part);
                EditorUtility.SetDirty(part);
            }

            holder.allBaseYarnParts = createdParts;
            holder.totalYarnPartCount = holder.GetTotalYarnPartCount(createdParts);
            holder.totalYarnDecorations = 0;
            holder.totalYarnSpecialDecor = 0;
            holder.randomColorEntries = new List<ColorCountEntry>();
            holder.UpdateDebugInfo();
            EditorUtility.SetDirty(holder);

            var saved = PrefabUtility.SaveAsPrefabAsset(root, outputPath);
            if (saved == null)
                throw new IOException($"Không lưu được prefab: {outputPath}");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = AssetDatabase.LoadAssetAtPath<GameObject>(outputPath);
            EditorGUIUtility.PingObject(Selection.activeObject);
            levelName = GetNextLevelName();
            SetStatus($"Đã tạo {outputPath} với {createdParts.Count} YarnPart.", MessageType.Info);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            SetStatus(exception.Message, MessageType.Error);
        }
        finally
        {
            if (root != null)
                PrefabUtility.UnloadPrefabContents(root);
        }
    }

    private static float GetMaxRendererBoundsSize(GameObject model)
    {
        var renderers = model.GetComponentsInChildren<Renderer>(true)
            .Where(renderer => renderer.enabled && renderer.gameObject.activeInHierarchy)
            .ToArray();
        if (renderers.Length == 0)
            throw new InvalidOperationException("Model không có Renderer đang hoạt động.");

        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        var size = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z);
        if (size <= Mathf.Epsilon)
            throw new InvalidOperationException("Không tính được kích thước Renderer của model.");
        return size;
    }

    private string GetNextLevelName()
    {
        var max = 0;
        foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { LevelsFolder }))
        {
            var fileName = Path.GetFileNameWithoutExtension(AssetDatabase.GUIDToAssetPath(guid));
            if (fileName.StartsWith("Level", StringComparison.OrdinalIgnoreCase) &&
                int.TryParse(fileName.Substring(5), out var number))
                max = Math.Max(max, number);
        }
        return $"Level{max + 1}";
    }

    private static int[] GetSiblingIndices(Transform target, Transform root)
    {
        var indices = new List<int>();
        while (target != root)
        {
            if (target.parent == null)
                throw new InvalidOperationException("Mesh nằm ngoài model đã chọn.");
            indices.Add(target.GetSiblingIndex());
            target = target.parent;
        }
        indices.Reverse();
        return indices.ToArray();
    }

    private static string GetHierarchyPath(Transform target, Transform root)
    {
        var names = new List<string>();
        while (target != root)
        {
            names.Add(target.name);
            target = target.parent;
        }
        names.Reverse();
        return string.Join("/", names);
    }

    private static Transform Resolve(Transform root, IReadOnlyList<int> indices)
    {
        foreach (var index in indices)
            root = root.GetChild(index);
        return root;
    }

    private void SetStatus(string message, MessageType type)
    {
        status = message;
        statusType = type;
    }

    private sealed class MeshPartChoice
    {
        public int[] indices;
        public string path;
        public YarnColor color;
        public bool include;
        public int innerCount;
    }
}
