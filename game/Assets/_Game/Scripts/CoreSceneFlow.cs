using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class CoreSceneFlow : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private bool autoAdvance;

    private GUIStyle titleStyle;
    private GUIStyle buttonStyle;

    private void Start()
    {
        if (autoAdvance)
            Invoke(nameof(LoadNextScene), 0.75f);
    }

        // OnGUI removed – UI now provided by real UI assets.
        // Kept for optional editor debugging.
#if UNITY_EDITOR
        private void OnGUI()
        {
            // placeholder for debug UI if needed.
        }
#endif

    private void LoadNextScene()
    {
        if (!string.IsNullOrEmpty(nextScene))
        {
            SceneManager.LoadScene(nextScene);
        }
    }

    private GUIStyle TitleStyle()
    {
        return titleStyle ??= new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontSize = Mathf.Max(24, Screen.height / 18),
            normal = { textColor = Color.white }
        };
    }

    private GUIStyle ButtonStyle()
    {
        return buttonStyle ??= new GUIStyle(GUI.skin.button)
        {
            fontSize = Mathf.Max(18, Screen.height / 32)
        };
    }
}
