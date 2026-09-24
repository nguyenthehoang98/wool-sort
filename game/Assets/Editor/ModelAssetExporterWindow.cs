using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class ModelAssetExporterWindow : EditorWindow
{
    private ObjectField modelField;
    private PopupField<string> formatField;
    private HelpBox status;

    [MenuItem("Tools/Model Asset Exporter")]
    private static void Open()
    {
        GetWindow<ModelAssetExporterWindow>("Model Exporter");
    }

    private void CreateGUI()
    {
        rootVisualElement.Add(new Label("Export a Unity model asset to OBJ or FBX."));

        modelField = new ObjectField("Model asset")
        {
            objectType = typeof(GameObject),
            allowSceneObjects = true
        };
        modelField.RegisterValueChangedCallback(_ => UpdateState());
        rootVisualElement.Add(modelField);

        formatField = new PopupField<string>(
            "Format",
            new List<string> { "OBJ", "FBX" },
            0);
        rootVisualElement.Add(formatField);

        var exportButton = new Button(() => Export(formatField.value))
        {
            text = "Export model"
        };
        rootVisualElement.Add(exportButton);

        status = new HelpBox(string.Empty, HelpBoxMessageType.Info);
        rootVisualElement.Add(status);

        if (Selection.activeObject is GameObject selected)
            modelField.SetValueWithoutNotify(selected);

        UpdateState();
    }

    private void UpdateState()
    {
        if (status == null)
            return;

        status.text = modelField.value == null
            ? "Select a model asset or a GameObject with MeshFilter/SkinnedMeshRenderer."
            : "Ready to export.";
        status.messageType = modelField.value == null
            ? HelpBoxMessageType.Warning
            : HelpBoxMessageType.Info;
    }

    private void Export(string format)
    {
        if (modelField.value is not GameObject model)
        {
            status.text = "Please select a GameObject model.";
            status.messageType = HelpBoxMessageType.Error;
            return;
        }

        var extension = format.ToLowerInvariant();
        var path = EditorUtility.SaveFilePanel(
            "Export model",
            string.Empty,
            model.name,
            extension);
        if (string.IsNullOrEmpty(path))
            return;

        try
        {
            string exportedPath;
            if (extension == "obj")
            {
                ObjModelExporter.Export(model, path);
                exportedPath = path;
            }
            else
            {
                exportedPath = ModelExporter.ExportObject(path, model);
            }

            if (string.IsNullOrEmpty(exportedPath) || !File.Exists(exportedPath))
                throw new IOException($"The {format} exporter did not create a file.");

            status.text = $"Exported: {exportedPath}";
            status.messageType = HelpBoxMessageType.Info;
            Debug.Log($"Model exported to {exportedPath}", model);
        }
        catch (System.Exception exception)
        {
            status.text = $"Export failed: {exception.Message}";
            status.messageType = HelpBoxMessageType.Error;
            Debug.LogException(exception, model);
        }
    }
}

internal static class ObjModelExporter
{
    private sealed class MeshPart
    {
        public Mesh mesh;
        public Matrix4x4 localToRoot;
        public string name;
        public Material[] materials;
    }

    public static void Export(GameObject root, string path)
    {
        var parts = new List<MeshPart>();
        var rootWorldToLocal = root.transform.worldToLocalMatrix;

        foreach (var filter in root.GetComponentsInChildren<MeshFilter>(true))
        {
            if (filter.sharedMesh == null)
                continue;

            parts.Add(new MeshPart
            {
                mesh = filter.sharedMesh,
                localToRoot = rootWorldToLocal * filter.transform.localToWorldMatrix,
                name = filter.name,
                materials = filter.GetComponent<Renderer>()?.sharedMaterials
            });
        }

        foreach (var renderer in root.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (renderer.sharedMesh == null)
                continue;

            parts.Add(new MeshPart
            {
                mesh = renderer.sharedMesh,
                localToRoot = rootWorldToLocal * renderer.transform.localToWorldMatrix,
                name = renderer.name,
                materials = renderer.sharedMaterials
            });
        }

        if (parts.Count == 0)
            throw new System.InvalidOperationException("The selected object has no MeshFilter or SkinnedMeshRenderer.");

        var materials = new Dictionary<string, Color>();
        using (var writer = new StreamWriter(path, false, new UTF8Encoding(false)))
        {
            writer.WriteLine("# Exported by Model Asset Exporter");
            writer.WriteLine("mtllib " + Path.GetFileNameWithoutExtension(path) + ".mtl");

            var vertexOffset = 1;
            foreach (var part in parts)
            {
                var mesh = part.mesh;
                var vertices = mesh.vertices;
                var normals = mesh.normals;
                var uvs = mesh.uv;

                writer.WriteLine("o " + CleanName(part.name));
                for (var i = 0; i < vertices.Length; i++)
                {
                    var vertex = part.localToRoot.MultiplyPoint3x4(vertices[i]);
                    vertex.z = -vertex.z;
                    writer.WriteLine($"v {Number(vertex.x)} {Number(vertex.y)} {Number(vertex.z)}");
                }

                for (var i = 0; i < vertices.Length; i++)
                {
                    var uv = uvs.Length == vertices.Length ? uvs[i] : Vector2.zero;
                    writer.WriteLine($"vt {Number(uv.x)} {Number(1f - uv.y)}");
                }

                for (var i = 0; i < vertices.Length; i++)
                {
                    var normal = normals.Length == vertices.Length
                        ? part.localToRoot.MultiplyVector(normals[i]).normalized
                        : Vector3.up;
                    normal.z = -normal.z;
                    writer.WriteLine($"vn {Number(normal.x)} {Number(normal.y)} {Number(normal.z)}");
                }

                for (var subMesh = 0; subMesh < mesh.subMeshCount; subMesh++)
                {
                    var material = part.materials != null && subMesh < part.materials.Length
                        ? part.materials[subMesh]
                        : null;
                    var materialName = AddMaterial(material, materials);
                    if (materialName != null)
                        writer.WriteLine("usemtl " + materialName);

                    var triangles = mesh.GetTriangles(subMesh);
                    for (var i = 0; i + 2 < triangles.Length; i += 3)
                    {
                        var a = vertexOffset + triangles[i];
                        var b = vertexOffset + triangles[i + 1];
                        var c = vertexOffset + triangles[i + 2];
                        writer.WriteLine($"f {a}/{a}/{a} {c}/{c}/{c} {b}/{b}/{b}");
                    }
                }

                vertexOffset += vertices.Length;
            }
        }

        var materialPath = Path.Combine(
            Path.GetDirectoryName(path) ?? string.Empty,
            Path.GetFileNameWithoutExtension(path) + ".mtl");
        using (var writer = new StreamWriter(materialPath, false, new UTF8Encoding(false)))
        {
            foreach (var material in materials)
            {
                writer.WriteLine("newmtl " + material.Key);
                writer.WriteLine("Kd " + Number(material.Value.r) + " " + Number(material.Value.g) + " " + Number(material.Value.b));
                writer.WriteLine("d " + Number(material.Value.a));
                writer.WriteLine();
            }
        }
    }

    private static string AddMaterial(Material material, Dictionary<string, Color> materials)
    {
        if (material == null)
            return null;

        var name = CleanName(material.name);
        if (!materials.ContainsKey(name))
        {
            var color = material.HasProperty("_Color") ? material.color : Color.white;
            materials.Add(name, color);
        }

        return name;
    }

    private static string CleanName(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "Mesh";

        return value.Replace("\r", "_").Replace("\n", "_");
    }

    private static string Number(float value)
    {
        return value.ToString("0.######", CultureInfo.InvariantCulture);
    }
}
