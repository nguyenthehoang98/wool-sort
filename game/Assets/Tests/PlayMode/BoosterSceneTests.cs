using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// Kiểm tra booster trên scene Gameplay thật (cần prefab/scene đầy đủ).
// Mỗi test load lại scene nên trạng thái độc lập, thứ tự chạy bất kỳ.
public class BoosterSceneTests
{
    private const string Prefix = "_Game.Scripts.Gameplay.";
    private object boosterManager;
    private object waitingAreaManager;
    private object boxManager;
    private Type boosterTypeType;
    private Type boosterManagerType;

    private static Type T(string fullName) =>
        Type.GetType(fullName + ", Assembly-CSharp")
        ?? throw new Exception("Không tìm thấy type " + fullName);

    private static object Find(Type t) => UnityEngine.Object.FindFirstObjectByType(t);

    private static object Get(object target, string property) => target.GetType()
        .GetProperty(property, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .GetValue(target);

    private static object Invoke(object target, string method, params object[] args) => target.GetType()
        .GetMethod(method, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        .Invoke(target, args);

    [UnitySetUp]
    public IEnumerator LoadGameplay()
    {
        yield return SceneManager.LoadSceneAsync("Gameplay");
        var gmType = T(Prefix + "GameManager");
        var gm = Find(gmType);
        // Chờ sequence mở màn chạy xong (quay model + box bay vào).
        float deadline = Time.realtimeSinceStartup + 25f;
        while (!(bool)gmType.GetMethod("CanInteractWithGame").Invoke(gm, null))
        {
            if (Time.realtimeSinceStartup > deadline)
                Assert.Fail("Quá 25s mà level chưa vào trạng thái tương tác được.");
            yield return null;
        }

        boosterManagerType = T(Prefix + "BoosterManager");
        boosterTypeType = T(Prefix + "BoosterType");
        boosterManager = Find(boosterManagerType);
        waitingAreaManager = Find(T(Prefix + "WaitingAreaManager"));
        boxManager = Find(T(Prefix + "BoxManager"));
        Assert.IsNotNull(boosterManager, "Thiếu BoosterManager trong scene Gameplay.");
        Assert.IsNotNull(waitingAreaManager, "Thiếu WaitingAreaManager trong scene Gameplay.");
    }

    private object Booster(string name) => Enum.Parse(boosterTypeType, name);

    private int WaitingCount() =>
        ((IList)Invoke(waitingAreaManager, "GetWaitingSlots")).Count;

    private bool CanUse(string name) =>
        (bool)boosterManagerType.GetMethod("CanUseBooster")
            .Invoke(boosterManager, new object[] { Booster(name), false });

    private void Use(string name) =>
        boosterManagerType.GetMethod("UseBooster")
            .Invoke(boosterManager, new object[] { Booster(name), false });

    [UnityTest]
    public IEnumerator AddSlot_GrowsByOneUntilCap()
    {
        int before = WaitingCount();
        bool cappedBefore = (bool)Invoke(waitingAreaManager, "HasReachMaxSlot");
        Assert.AreEqual(!cappedBefore, CanUse("AddSlot"), "CanUseBooster(AddSlot) phải khớp HasReachMaxSlot.");

        Use("AddSlot");
        yield return null;

        if (cappedBefore)
            Assert.AreEqual(before, WaitingCount(), "Đã chạm trần thì AddSlot không được thêm slot.");
        else
            Assert.AreEqual(before + 1, WaitingCount(), "AddSlot phải thêm đúng 1 slot.");
    }

    [UnityTest]
    public IEnumerator AddSlot_NeverExceedsMax()
    {
        for (int i = 0; i < 10; i++)
        {
            if ((bool)Invoke(waitingAreaManager, "HasReachMaxSlot")) break;
            Use("AddSlot");
            yield return null;
        }
        Assert.IsTrue((bool)Invoke(waitingAreaManager, "HasReachMaxSlot"), "Spam AddSlot phải chạm trần.");
        int capped = WaitingCount();
        Use("AddSlot");
        yield return null;
        Assert.AreEqual(capped, WaitingCount(), "Vượt trần thì số slot phải đứng yên.");
        Assert.IsFalse(CanUse("AddSlot"), "Chạm trần thì CanUseBooster(AddSlot) phải false.");
    }

    [UnityTest]
    public IEnumerator Clean_AvailabilityMatchesWaitingState()
    {
        bool empty = (bool)Invoke(waitingAreaManager, "IsAllEmpty");
        Assert.AreEqual(!empty, CanUse("Clean"), "CanUseBooster(Clean) phải khớp IsAllEmpty.");
        yield break;
    }
}
