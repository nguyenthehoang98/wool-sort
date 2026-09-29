using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

public class ResultPopupProgressionTests
{
    private Type gameManagerType;
    private Type gameUiControllerType;
    private object gameManager;
    private Component gameUiController;

    [UnitySetUp]
    public IEnumerator LoadGameplay()
    {
        yield return SceneManager.LoadSceneAsync("Gameplay");
        yield return null;
        gameManagerType = Type.GetType("_Game.Scripts.Gameplay.GameManager, Assembly-CSharp");
        gameUiControllerType = Type.GetType("_Game.Scripts.Gameplay.GameUIController, Assembly-CSharp");
        gameManager = UnityEngine.Object.FindFirstObjectByType(gameManagerType);
        gameUiController = (Component)UnityEngine.Object.FindFirstObjectByType(gameUiControllerType);
        if ((int)Get(gameManager, "CurrentLevel") < 1)
            gameManagerType.GetMethod("LoadLevel").Invoke(gameManager, null);
    }

    [Test]
    public void NextAdvancesAndRetryKeepsCurrentLevel()
    {
        Assert.AreEqual(1, Get(gameManager, "CurrentLevel"));
        Assert.AreEqual(1, Get(gameManager, "RealCurrentLevel"));

        ShowResult(true);
        Assert.IsTrue(GameObject.Find("ResultPopup").activeSelf);
        ClickResultButton();
        Assert.AreEqual(2, Get(gameManager, "CurrentLevel"));
        Assert.AreEqual(2, Get(gameManager, "RealCurrentLevel"));
        ShowResult(false);
        ClickResultButton();
        Assert.AreEqual(2, Get(gameManager, "CurrentLevel"));
        Assert.AreEqual(2, Get(gameManager, "RealCurrentLevel"));
    }

    private void ShowResult(bool won) => gameUiControllerType
        .GetMethod("ShowResultPopup", BindingFlags.Instance | BindingFlags.NonPublic)
        .Invoke(gameUiController, new object[] { won });

    private static object Get(object target, string property) => target.GetType()
        .GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .GetValue(target);

    private static void ClickResultButton() => GameObject.Find("ResultPopup")
        .GetComponentInChildren<Button>().onClick.Invoke();
}
