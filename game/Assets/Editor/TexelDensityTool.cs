using UnityEngine;
using UnityEditor;

public class TexelDensityTool : EditorWindow
{
    private float mapSize = 1024f;
    private float desiredTexelDensity = 512f;
    private float currentTexelDensity = 0f;

    [MenuItem("Tools/Texel Density Tool")]
    public static void ShowWindow()
    {
        GetWindow<TexelDensityTool>("Texel Density Tool");
    }

    void OnGUI()
    {
        // Removed header label

        if (GUILayout.Button("GET Texel Density"))
        {
            currentTexelDensity = CalculateAverageTexelDensity();
            Debug.Log($"Average Texel Density: {currentTexelDensity:F2} px/unit");
        }

        EditorGUILayout.LabelField("Current Texel Density", currentTexelDensity.ToString("F2") + " px/unit");

        mapSize = EditorGUILayout.FloatField("Texture Map Size (px)", mapSize);

        desiredTexelDensity = EditorGUILayout.FloatField("Desired Texel Density (px/unit)", desiredTexelDensity);

        if (GUILayout.Button("SET Texel Density"))
        {
            SetTexelDensity(desiredTexelDensity);
        }
    }

    float CalculateAverageTexelDensity()
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return 0;
        }

        float totalWorldArea = 0f;
        float totalUVArea = 0f;

        foreach (GameObject go in selection)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                continue;

            Mesh mesh = mf.sharedMesh;
            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;
            int[] triangles = mesh.triangles;

            Matrix4x4 localToWorld = go.transform.localToWorldMatrix;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = localToWorld.MultiplyPoint3x4(vertices[triangles[i]]);
                Vector3 v1 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
                Vector3 v2 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);

                float worldArea = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
                totalWorldArea += worldArea;

                Vector2 uv0 = uvs[triangles[i]];
                Vector2 uv1 = uvs[triangles[i + 1]];
                Vector2 uv2 = uvs[triangles[i + 2]];

                float uvArea = Mathf.Abs((uv0.x * (uv1.y - uv2.y) +
                                          uv1.x * (uv2.y - uv0.y) +
                                          uv2.x * (uv0.y - uv1.y)) * 0.5f);
                totalUVArea += uvArea;
            }
        }

        if (totalWorldArea <= 0 || totalUVArea <= 0)
        {
            Debug.LogError("Invalid geometry or UVs.");
            return 0;
        }

        return Mathf.Sqrt(totalUVArea * (mapSize * mapSize) / totalWorldArea);
    }

    void SetTexelDensity(float targetDensity)
    {
        GameObject[] selection = Selection.gameObjects;
        if (selection.Length == 0)
        {
            Debug.LogWarning("No objects selected.");
            return;
        }

        foreach (GameObject go in selection)
        {
            MeshFilter mf = go.GetComponent<MeshFilter>();
            if (mf == null || mf.sharedMesh == null)
                continue;

            Mesh originalMesh = mf.sharedMesh;
            Mesh mesh = Instantiate(originalMesh);
            mesh.name = originalMesh.name + "_TexelAdjusted";

            Vector3[] vertices = mesh.vertices;
            Vector2[] uvs = mesh.uv;
            int[] triangles = mesh.triangles;

            Matrix4x4 localToWorld = go.transform.localToWorldMatrix;

            float totalWorldArea = 0f;
            float totalUVArea = 0f;

            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 v0 = localToWorld.MultiplyPoint3x4(vertices[triangles[i]]);
                Vector3 v1 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 1]]);
                Vector3 v2 = localToWorld.MultiplyPoint3x4(vertices[triangles[i + 2]]);

                float worldArea = Vector3.Cross(v1 - v0, v2 - v0).magnitude * 0.5f;
                totalWorldArea += worldArea;

                Vector2 uv0 = uvs[triangles[i]];
                Vector2 uv1 = uvs[triangles[i + 1]];
                Vector2 uv2 = uvs[triangles[i + 2]];

                float uvArea = Mathf.Abs((uv0.x * (uv1.y - uv2.y) +
                                          uv1.x * (uv2.y - uv0.y) +
                                          uv2.x * (uv0.y - uv1.y)) * 0.5f);
                totalUVArea += uvArea;
            }

            if (totalWorldArea <= 0 || totalUVArea <= 0)
            {
                Debug.LogWarning($"Skipping {go.name} due to invalid geometry or UVs.");
                continue;
            }

            float currentDensity = Mathf.Sqrt(totalUVArea * (mapSize * mapSize) / totalWorldArea);
            float scale = targetDensity / currentDensity;

            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] *= scale;
            }

            mesh.uv = uvs;
            mf.sharedMesh = mesh;

            Debug.Log($"{go.name}: UVs scaled by {scale:F2} for target density {targetDensity:F2} px/unit.");
        }

        Debug.Log("Texel density applied to selected objects.");
    }
}
