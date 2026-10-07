using UnityEditor;
using UnityEngine;

/// <summary>
/// Cửa sổ gom toàn bộ pipeline thiết kế level, làm từng bước 1 là xong:
/// 1. Tạo level (FBX -> prefab) | 2. Validate (quét + sửa %3) | 3. Prompt ảnh AI |
/// 4. Matrix ảnh (extract + JSON tay) | 5. Độ khó & màu (đo khó, đồng bộ 21 màu).
/// Mở: Tools / Puzzle / Level Studio
/// </summary>
public sealed class LevelStudioWindow : EditorWindow
{
    [MenuItem("Tools/Puzzle/Level Studio", false, 0)]
    private static void Open()
    {
        var w = GetWindow<LevelStudioWindow>("Level Studio");
        w.minSize = new Vector2(920, 640);
    }

    private int tab;
    private readonly string[] tabNames = new[]
    {
        "1. Tạo level",
        "2. Độ khó & Màu",
        "3. Validate",
        "4. Prompt ảnh",
        "5. Matrix ảnh",
    };

    private LevelQuickCreatorWindow createPanel;
    private BoxImageColorValidator validatePanel;
    private ColorPromptWindow promptPanel;
    private LevelImageMatrixTool matrixPanel;
    private LevelDifficultyTool difficultyPanel;

    private void OnGUI()
    {
        GUILayout.Label("LEVEL STUDIO — làm từng bước 1 là xong", EditorStyles.boldLabel);
        tab = GUILayout.Toolbar(tab, tabNames, GUILayout.Height(32));
        EditorGUILayout.Space(4);

        switch (tab)
        {
            case 0:
                if (createPanel == null) createPanel = CreateInstance<LevelQuickCreatorWindow>();
                createPanel.InitPanel();
                createPanel.DrawPanel();
                break;
            case 1:
                if (difficultyPanel == null) difficultyPanel = CreateInstance<LevelDifficultyTool>();
                difficultyPanel.InitPanel();
                difficultyPanel.DrawPanel();
                break;
            case 2:
                if (validatePanel == null) validatePanel = CreateInstance<BoxImageColorValidator>();
                validatePanel.InitPanel();
                validatePanel.DrawPanel();
                break;
            case 3:
                if (promptPanel == null) promptPanel = CreateInstance<ColorPromptWindow>();
                promptPanel.InitPanel();
                promptPanel.DrawPanel();
                break;
            default:
                if (matrixPanel == null) matrixPanel = CreateInstance<LevelImageMatrixTool>();
                matrixPanel.InitPanel();
                matrixPanel.DrawPanel();
                break;
        }
    }
}
