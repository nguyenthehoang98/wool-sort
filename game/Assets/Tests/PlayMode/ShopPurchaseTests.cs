using System;
using System.Collections;
using System.Reflection;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Kiểm tra luồng shop local: bật tab Shop -> item được dựng -> bấm BUY -> coin/booster đổi thật.
/// </summary>
public class ShopPurchaseTests
{
    private Type economyType;
    private Type shopServiceType;
    private Type menuBarType;
    private Type itemViewType;
    private Component economy;

    [UnitySetUp]
    public IEnumerator LoadHome()
    {
        yield return SceneManager.LoadSceneAsync("Home");
        yield return null;

        economyType = Type.GetType("_Game.Scripts.Core.EconomyService, Assembly-CSharp");
        shopServiceType = Type.GetType("_Game.Scripts.Core.ShopService, Assembly-CSharp");
        menuBarType = Type.GetType("_Game.Scripts.MenuBarManager, Assembly-CSharp");
        itemViewType = Type.GetType("_Game.Scripts.Meta.ShopItemView, Assembly-CSharp");

        economy = (Component)UnityEngine.Object.FindFirstObjectByType(economyType);
    }

    [UnityTest]
    public IEnumerator ShopTab_BuildsItems_AndBuyChangesCoin()
    {
        Assert.IsNotNull(economy, "Không có EconomyService trong scene Home.");
        Assert.IsNotNull(shopServiceType, "Thiếu type ShopService.");
        Assert.IsNotNull(itemViewType, "Thiếu type ShopItemView.");

        // Nạp thêm coin để chắc chắn đủ tiền mua (không phụ thuộc save).
        Invoke(economy, "AddCoins", 1000, "test", "test");
        int coinBefore = (int)Invoke(economy, "GetCoin");
        int boosterBefore = GetBoosterCount(economy, "MagicBox");

        // Bật tab Shop.
        var menuBar = UnityEngine.Object.FindFirstObjectByType(menuBarType);
        Assert.IsNotNull(menuBar, "Thiếu MenuBar trong scene Home.");
        menuBarType.GetMethod("OpenShop").Invoke(menuBar, null);
        yield return null;

        // Item phải được dựng ra khi mở tab Shop.
        var found = UnityEngine.Object.FindObjectsByType(itemViewType, FindObjectsSortMode.None);
        Assert.IsNotNull(found, "Không tìm thấy ShopItemView nào.");
        Assert.Greater(found.Length, 0, "Shop không dựng được item nào.");

        var views = new System.Collections.Generic.List<Component>();
        foreach (var o in found) views.Add((Component)o);

        // Tìm dòng "Magic Box" rồi bấm BUY.
        Component target = FindByTitle(views, "Magic Box");
        Assert.IsNotNull(target, "Không thấy item 'Magic Box' trong shop.");

        var btn = GetPrivate(target, "btnBuy") as Button;
        Assert.IsNotNull(btn, "Item không có nút BUY.");

        btn.onClick.Invoke();
        yield return null;

        // Coin phải trừ, booster phải cộng.
        int coinAfter = (int)Invoke(economy, "GetCoin");
        int boosterAfter = GetBoosterCount(economy, "MagicBox");

        Assert.Less(coinAfter, coinBefore, "Coin không bị trừ sau khi mua.");
        Assert.AreEqual(boosterBefore + 1, boosterAfter, "Booster Magic Box không được cộng.");
    }

    /// <summary>Gói coin miễn phí: bấm BUY phải +coin, và chỉ nhận được 1 lần.</summary>
    [UnityTest]
    public IEnumerator FreeCoinPack_AddsCoin_OnlyOnce()
    {
        Assert.IsNotNull(economy, "Không có EconomyService trong scene Home.");

        var menuBar = UnityEngine.Object.FindFirstObjectByType(menuBarType);
        Assert.IsNotNull(menuBar, "Thiếu MenuBar trong scene Home.");
        menuBarType.GetMethod("OpenShop").Invoke(menuBar, null);
        yield return null;

        var found = UnityEngine.Object.FindObjectsByType(itemViewType, FindObjectsSortMode.None);
        var views = new System.Collections.Generic.List<Component>();
        foreach (var o in found) views.Add((Component)o);

        Component target = FindByTitle(views, "Gói coin miễn phí");
        Assert.IsNotNull(target, "Không thấy gói coin miễn phí trong shop.");

        var btn = GetPrivate(target, "btnBuy") as Button;
        Assert.IsNotNull(btn, "Gói coin miễn phí không có nút BUY.");

        int coinBefore = (int)Invoke(economy, "GetCoin");

        btn.onClick.Invoke();
        yield return null;

        int coinAfter = (int)Invoke(economy, "GetCoin");
        Assert.Greater(coinAfter, coinBefore, "Gói coin miễn phí không cộng coin.");

        // Lần thứ hai phải bị chặn (tránh vòng lặp coin vô hạn).
        btn.onClick.Invoke();
        yield return null;

        int coinAfter2 = (int)Invoke(economy, "GetCoin");
        Assert.AreEqual(coinAfter, coinAfter2, "Gói coin miễn phí bị mua lần thứ 2.");
    }

    private Component FindByTitle(System.Collections.Generic.List<Component> views, string title)
    {
        foreach (var v in views)
        {
            var titleObj = GetPrivate(v, "title");
            if (titleObj == null) continue;
            var textProp = titleObj.GetType().GetProperty("text");
            if (textProp == null) continue;
            if (textProp.GetValue(titleObj) as string == title) return v;
        }
        return null;
    }

    private object Invoke(Component target, string method, params object[] args)
    {
        var mi = target.GetType().GetMethod(method,
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.IsNotNull(mi, "Không tìm thấy method " + method);
        return mi.Invoke(target, args);
    }

    private object GetPrivate(object target, string field)
    {
        var fi = target.GetType().GetField(field,
            BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        Assert.IsNotNull(fi, "Không tìm thấy field " + field);
        return fi.GetValue(target);
    }

    private int GetBoosterCount(Component target, string boosterName)
    {
        var data = Invoke(target, "Data");
        var value = data.GetType().GetField("magicBoxCount").GetValue(data);
        return Convert.ToInt32(value);
    }
}
