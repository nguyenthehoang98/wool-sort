using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;

// Logic thuần của booster, không cần scene: chạy nhanh, ổn định.
// Dùng reflection (giống ResultPopupProgressionTests) vì game code nằm ở
// Assembly-CSharp, test assembly không reference trực tiếp được.
public class BoosterLogicTests
{
    private static Type T(string fullName) =>
        Type.GetType(fullName + ", " + "Assembly-CSharp")
        ?? throw new Exception("Không tìm thấy type " + fullName);

    private static object YarnColor(string name) =>
        Enum.Parse(T("_Game.Scripts.YarnColor"), name);

    private static object NewInfo(string color, int layer, object parent) =>
        Activator.CreateInstance(T("_Game.Scripts.Gameplay.YarnPartInfo"),
            YarnColor(color), layer, parent);

    private static int Priority(object info) =>
        (int)T("_Game.Scripts.Gameplay.YarnPartInfo").GetProperty("Priority").GetValue(info);

    private static void Call(object target, string method) =>
        target.GetType().GetMethod(method).Invoke(target, null);

    [Test]
    public void FreshInfo_HasPriorityOne()
    {
        var info = NewInfo("Blue", 1, null);
        Assert.AreEqual(1, Priority(info));
    }

    [Test]
    public void ChildOfOnBoardParent_HasPriorityTwo()
    {
        var root = NewInfo("Blue", 1, null);
        var child = NewInfo("Pink", 2, root);
        Assert.AreEqual(2, Priority(child));
    }

    [Test]
    public void PickedToQueue_HasPriorityZero()
    {
        var info = NewInfo("Blue", 1, null);
        Call(info, "MarkPickedToQueue");
        Assert.AreEqual(0, Priority(info));
    }

    [Test]
    public void FilledIntoBox_HasPriorityMinusOne()
    {
        var info = NewInfo("Blue", 1, null);
        Call(info, "MarkFilledIntoBox");
        Assert.AreEqual(-1, Priority(info));
    }

    [Test]
    public void GameData_CountDecreasesOnUse()
    {
        var gd = T("GameData").GetProperty("Instance").GetValue(null);
        int before = (int)T("GameData").GetMethod("GetCount")
            .Invoke(gd, new object[] { Enum.Parse(T("_Game.Scripts.Gameplay.BoosterType"), "AddSlot") });
        T("GameData").GetMethod("AddCount")
            .Invoke(gd, new object[] { Enum.Parse(T("_Game.Scripts.Gameplay.BoosterType"), "AddSlot"), -1, "test", "unit" });
        int after = (int)T("GameData").GetMethod("GetCount")
            .Invoke(gd, new object[] { Enum.Parse(T("_Game.Scripts.Gameplay.BoosterType"), "AddSlot") });
        Assert.AreEqual(before - 1, after);
    }

    [Test]
    public void BoosterConfig_HasThreeEntries()
    {
        var costs = (System.Collections.IList)T("Configs").GetField("LIST_BOOSTER_COST").GetValue(null);
        var unlocks = (System.Collections.IList)T("Configs").GetField("LIST_BOOSTER_UNLOCK").GetValue(null);
        Assert.AreEqual(3, costs.Count);
        Assert.AreEqual(3, unlocks.Count);
        foreach (int lv in unlocks) Assert.Greater(lv, 0);
        var names = Enum.GetNames(T("_Game.Scripts.Gameplay.BoosterType"));
        Assert.AreEqual(3, names.Length);
    }
}
