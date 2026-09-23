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

    private void OnGUI()
    {
        if (autoAdvance)
        {
            GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Loading...", TitleStyle());
            return;
        }

        GUI.Box(new Rect(Screen.width * 0.2f, Screen.height * 0.2f, Screen.width * 0.6f, Screen.height * 0.6f), "");
        GUI.Label(new Rect(Screen.width * 0.25f, Screen.height * 0.3f, Screen.width * 0.5f, 80f), "PUZZLE GAME", TitleStyle());
        if (GUI.Button(new Rect(Screen.width * 0.3f, Screen.height * 0.55f, Screen.width * 0.4f, 80f), "PLAY LEVEL 1", ButtonStyle()))
            LoadNextScene();
    }

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
