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
    }

    /// <summary>
    /// Regression: enum BoosterType phai map 1-1 voi index cua list cau hinh
    /// (AddSlot=0, Clean=1, MagicBox=2) — scene Gameplay.unity, mang
    /// Titles/Descs/contentIcons trong MoreBoosterPopupUI va
    /// LIST_BOOSTER_COST/LIST_BOOSTER_UNLOCK deu index theo thu tu nay.
    /// Lệch 1 la nut bam ra sai booster.
    /// </summary>
    [Test]
    public void BoosterType_ValuesMatchConfigListOrder()
    {
        var type = T("_Game.Scripts.Gameplay.BoosterType");
        Assert.AreEqual(0, (int)Enum.Parse(type, "AddSlot"));
        Assert.AreEqual(1, (int)Enum.Parse(type, "Clean"));
        Assert.AreEqual(2, (int)Enum.Parse(type, "MagicBox"));
        // None la sentinel, khong phai index
        Assert.Less((int)Enum.Parse(type, "None"), 0);
    }

    [Test]
    public void Configs_GetBoosterCost_MapsCorrectType()
    {
        var cfg = T("Configs");
        var getCost = cfg.GetMethod("GetBoosterCost");
        var type = T("_Game.Scripts.Gameplay.BoosterType");

        Assert.AreEqual(80, (int)getCost.Invoke(null, new object[] { Enum.Parse(type, "AddSlot") }));
        Assert.AreEqual(120, (int)getCost.Invoke(null, new object[] { Enum.Parse(type, "Clean") }));
        Assert.AreEqual(60, (int)getCost.Invoke(null, new object[] { Enum.Parse(type, "MagicBox") }));
    }

    [Test]
    public void Configs_GetBoosterUnlockLevel_MapsCorrectType()
    {
        var cfg = T("Configs");
        var get = cfg.GetMethod("GetBoosterUnlockLevel");
        var type = T("_Game.Scripts.Gameplay.BoosterType");

        Assert.AreEqual(1, (int)get.Invoke(null, new object[] { Enum.Parse(type, "AddSlot") }));
        Assert.AreEqual(1, (int)get.Invoke(null, new object[] { Enum.Parse(type, "Clean") }));
        Assert.AreEqual(1, (int)get.Invoke(null, new object[] { Enum.Parse(type, "MagicBox") }));
    }
}
