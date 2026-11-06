#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using _Game.Scripts.Core;

public class AudioSetupEditor
{
    private const string AssetPath = "Assets/Resources/AudioConfig.asset";

    [MenuItem("WoolSort/Setup Audio (tạo AudioConfig + gắn AudioBootstrap vào EntryScene)")]
    public static void Setup()
    {
        // 1) Tạo AudioConfig.asset nếu chưa có
        var config = AssetDatabase.LoadAssetAtPath<AudioConfigAsset>(AssetPath);
        if (config == null)
        {
            config = ScriptableObject.CreateInstance<AudioConfigAsset>();
            config.config = new AudioConfig();
            AssetDatabase.CreateAsset(config, AssetPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[AudioSetup] Đã tạo " + AssetPath + " — hãy kéo clip âm thanh vào Inspector.");
        }
        else
        {
            Debug.Log("[AudioSetup] AudioConfig đã tồn tại: " + AssetPath);
        }

        // 2) Tìm scene EntryScene trong build settings
        string entryScenePath = null;
        foreach (var sb in EditorBuildSettings.scenes)
        {
            if (sb.path.Contains("EntryScene", System.StringComparison.OrdinalIgnoreCase) ||
                sb.path.Contains("Entry", System.StringComparison.OrdinalIgnoreCase))
            {
                entryScenePath = sb.path;
                break;
            }
        }

        if (string.IsNullOrEmpty(entryScenePath))
        {
            Debug.LogWarning("[AudioSetup] Không tìm thấy EntryScene trong Build Settings.");
            return;
        }

        // 3) Mở scene, thêm AudioBootstrap
        var scene = EditorSceneManager.OpenScene(entryScenePath, OpenSceneMode.Single);
        var go = new GameObject("AudioBootstrap");
        var bootstrap = go.AddComponent<AudioBootstrap>();
        // Không gán bgmHome/bgmGameplay (để AudioBootstrap tự lấy từ AudioConfig)
        Undo.RegisterCreatedObjectUndo(go, "Add AudioBootstrap");

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[AudioSetup] Đã gắn AudioBootstrap vào scene: " + entryScenePath);
        Debug.Log("[AudioSetup] XONG. Bước tiếp theo: mở AudioConfig.asset, kéo clip âm thanh (BGM, SFX) từ dự án gốc vào các field.");
    }
}
#endif
