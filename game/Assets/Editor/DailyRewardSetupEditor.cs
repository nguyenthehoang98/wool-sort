#if UNITY_EDITOR
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using _Game.Scripts.Core;
using _Game.Scripts.Meta;

/// <summary>
/// Dựng UI Daily Reward (native) từ art đã pick ở `_Game/Textures/UIV2/`:
/// tạo DailyRewardConfig.asset + prefab DailyRewardIcon / DailyRewardItem / DailyRewardPopup,
/// và (tùy chọn) instance popup + nút mở vào scene Home.
/// Menu: WoolSort / Daily Reward / ...
/// </summary>
public static class DailyRewardSetupEditor
{
    private const string ArtDir = "Assets/_Game/Textures/UIV2";
    private const string UiDir = "Assets/_Game/Prefab/UI";
    private const string ConfigDir = "Assets/_Game/Resources";
    private const string ConfigPath = ConfigDir + "/DailyRewardConfig.asset";

    private static Sprite Art(string name) => AssetDatabase.LoadAssetAtPath<Sprite>($"{ArtDir}/{name}.png");
    private static Sprite Res(string path) => AssetDatabase.LoadAssetAtPath<Sprite>(path);
    private static TMP_FontAsset FontAsset()
        => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Game/Fonts/LilitaOne-Regular SDF.asset");

    [MenuItem("WoolSort/Daily Reward/1. Build Config + Prefabs")]
    public static void BuildAll()
    {
        var cfg = EnsureConfig();
        var icon = BuildIconPrefab();
        var item = BuildItemPrefab(icon);
        BuildPopupPrefab(item, cfg);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[DailyRewardSetup] Xong. Config: {ConfigPath}; Prefab: {UiDir}/DailyRewardIcon|DailyRewardItem|DailyRewardPopup.prefab");
    }

    [MenuItem("WoolSort/Daily Reward/2. Add Popup + Button to Home")]
    public static void AddToHome()
    {
        var popupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{UiDir}/DailyRewardPopup.prefab");
        if (popupPrefab == null) { Debug.LogError("[DailyRewardSetup] Chạy bước 1 trước."); return; }

        var scene = EditorSceneManager.OpenScene("Assets/Scenes/Home.unity", OpenSceneMode.Single);

        Canvas canvas = null;
        foreach (var c in Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            if (c.renderMode == RenderMode.ScreenSpaceOverlay) { canvas = c; break; }
        if (canvas == null) { Debug.LogError("[DailyRewardSetup] Home không có Canvas overlay."); return; }

        if (Object.FindFirstObjectByType<DailyRewardPopup>(FindObjectsInactive.Include) == null)
        {
            var go = (GameObject)PrefabUtility.InstantiatePrefab(popupPrefab, canvas.transform);
            go.name = "DailyRewardPopup";
            Undo.RegisterCreatedObjectUndo(go, "Add DailyRewardPopup");
        }

        var popup = Object.FindFirstObjectByType<DailyRewardPopup>(FindObjectsInactive.Include);

        if (Object.FindFirstObjectByType<DailyRewardButton>(FindObjectsInactive.Include) == null)
        {
            var btnGo = new GameObject("DailyRewardButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(DailyRewardButton));
            var rt = (RectTransform)btnGo.transform;
            rt.SetParent(canvas.transform, false);
            rt.anchorMin = rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-40f, -300f);
            rt.sizeDelta = new Vector2(120f, 120f);
            var img = btnGo.GetComponent<Image>();
            img.sprite = Res("Assets/_Game/Textures/UIV2/icon_Daily.png");
            img.preserveAspect = true;
            var btn = btnGo.GetComponent<DailyRewardButton>();
            btn.popup = popup;
            Undo.RegisterCreatedObjectUndo(btnGo, "Add DailyRewardButton");
        }

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Debug.Log("[DailyRewardSetup] Đã thêm DailyRewardPopup + DailyRewardButton vào Home.");
    }

    // ---------------------------------------------------------------- config

    private static DailyRewardConfig EnsureConfig()
    {
        var cfg = AssetDatabase.LoadAssetAtPath<DailyRewardConfig>(ConfigPath);
        if (cfg == null)
        {
            if (!AssetDatabase.IsValidFolder(ConfigDir))
                AssetDatabase.CreateFolder("Assets/_Game", "Resources");
            cfg = ScriptableObject.CreateInstance<DailyRewardConfig>();
            AssetDatabase.CreateAsset(cfg, ConfigPath);
        }

        // giá trị lấy từ game gốc (chỉ coin + booster; bỏ heart-time)
        var coin = Res("Assets/_Game/Resources/Textures/icon_coin.png");
        var slot = Res("Assets/_Game/Resources/Textures/booster_1.png");
        var broom = Res("Assets/_Game/Resources/Textures/booster_3.png");
        var spool = Res("Assets/_Game/Resources/Textures/booster_2.png");

        DailyRewardDay D(params DailyRewardEntry[] es) => new DailyRewardDay { rewards = new List<DailyRewardEntry>(es) };
        DailyRewardEntry E(string id, int amt, Sprite ic) => new DailyRewardEntry { rewardId = id, amount = amt, icon = ic };

        cfg.days = new List<DailyRewardDay>
        {
            D(E("coin", 60, coin)),
            D(E("magicbox", 1, spool)),
            D(E("coin", 80, coin)),
            D(E("addslot", 1, slot)),
            D(E("coin", 120, coin)),
            D(E("clean", 1, broom)),
            D(E("coin", 240, coin), E("magicbox", 2, spool)),
        };

        EditorUtility.SetDirty(cfg);
        return cfg;
    }

    // ---------------------------------------------------------------- helpers

    private static RectTransform NewUI(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        return rt;
    }

    private static Image AddImage(RectTransform rt, Sprite sprite, bool sliced = false)
    {
        var img = rt.gameObject.AddComponent<Image>();
        img.sprite = sprite;
        img.type = sliced ? Image.Type.Sliced : Image.Type.Simple;
        if (!sliced) img.preserveAspect = true;
        return img;
    }

    private static TextMeshProUGUI AddText(RectTransform rt, string text, float size, Vector2 dims, Vector2 pos)
    {
        var t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        t.text = text;
        t.font = FontAsset();
        t.fontSize = size;
        t.alignment = TextAlignmentOptions.Center;
        t.color = Color.white;
        rt.sizeDelta = dims;
        rt.anchoredPosition = pos;
        return t;
    }

    private static GameObject Save(RectTransform root, string dir, string name)
    {
        if (!AssetDatabase.IsValidFolder(dir))
        {
            var parent = System.IO.Path.GetDirectoryName(dir).Replace('\\', '/');
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(dir));
        }
        var prefab = PrefabUtility.SaveAsPrefabAsset(root.gameObject, $"{dir}/{name}.prefab");
        Object.DestroyImmediate(root.gameObject);
        return prefab;
    }

    // Popup đặt trong Resources để runtime load được (DailyRewardLauncher tự mở).
    private const string PopupPath = ConfigDir + "/DailyRewardPopup.prefab";

    // ---------------------------------------------------------------- prefabs

    private static GameObject BuildIconPrefab()
    {
        var root = NewUI("DailyRewardIcon", null);
        root.sizeDelta = new Vector2(96f, 96f);
        var img = AddImage(root, null);
        var comp = root.gameObject.AddComponent<DailyRewardIcon>();
        comp.icon = img;

        var amtRt = NewUI("Amount", root);
        comp.amount = AddText(amtRt, "0", 34f, new Vector2(120f, 40f), new Vector2(0f, -58f));
        comp.amount.color = new Color(0.42f, 0.26f, 0.12f, 1f);

        return Save(root, UiDir, "DailyRewardIcon");
    }

    private static GameObject BuildItemPrefab(GameObject iconPrefab)
    {
        var root = NewUI("DailyRewardItem", null);
        root.sizeDelta = new Vector2(230f, 250f);
        var bg = AddImage(root, Art("Slot_Daily_1A"), sliced: true);
        var comp = root.gameObject.AddComponent<DailyRewardItem>();
        comp.background = bg;

        // Overlay nền phải nằm dưới nội dung (thêm trước TxtDay/RewardContainer).
        var today = NewUI("TodayState", root);
        today.sizeDelta = new Vector2(230f, 250f);
        AddImage(today, Art("Slot_Daily_1B"), sliced: true);
        today.gameObject.SetActive(false);
        comp.todayState = today.gameObject;

        var txtRt = NewUI("TxtDay", root);
        comp.txtDay = AddText(txtRt, "DAY 1", 30f, new Vector2(200f, 40f), new Vector2(0f, 92f));

        // Lớp masking tối chỉ bật khi ngày đó đang bị khoá.
        var lockMask = NewUI("LockMask", root);
        lockMask.sizeDelta = new Vector2(230f, 250f);
        var lockMaskImg = AddImage(lockMask, Res("Assets/_Game/Resources/Textures/Slot_Daily_masking.png"));
        lockMaskImg.color = new Color(0f, 0f, 0f, 0.7647059f);
        lockMaskImg.preserveAspect = false;
        lockMask.gameObject.SetActive(false);
        comp.lockMask = lockMask.gameObject;

        var rc = NewUI("RewardContainer", root);
        rc.sizeDelta = new Vector2(200f, 110f);
        rc.anchoredPosition = new Vector2(0f, -6f);
        var hlg = rc.gameObject.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleCenter;
        hlg.spacing = 8f;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        comp.rewardContainer = rc;
        comp.rewardIconPrefab = iconPrefab.GetComponent<DailyRewardIcon>();

        var tick = NewUI("ClaimedMark", root);
        tick.sizeDelta = new Vector2(52f, 52f);
        tick.anchoredPosition = new Vector2(82f, 88f);
        AddImage(tick, Art("Icon_Check"));
        tick.gameObject.SetActive(false);
        comp.claimedMark = tick.gameObject;

        var lockMark = NewUI("LockMark", root);
        lockMark.sizeDelta = new Vector2(96f, 96f);
        lockMark.anchoredPosition = new Vector2(0f, 10f);
        AddImage(lockMark, Res("Assets/_Game/Resources/Textures/icon_lock.png"));
        lockMark.gameObject.SetActive(false);
        comp.lockMark = lockMark.gameObject;

        comp.coinIcon = Res("Assets/_Game/Resources/Textures/icon_coin.png");
        comp.addSlotIcon = Res("Assets/_Game/Resources/Textures/booster_1.png");
        comp.cleanIcon = Res("Assets/_Game/Resources/Textures/booster_3.png");
        comp.magicBoxIcon = Res("Assets/_Game/Resources/Textures/booster_2.png");

        return Save(root, UiDir, "DailyRewardItem");
    }

    private static GameObject BuildPopupPrefab(GameObject itemPrefab, DailyRewardConfig cfg)
    {
        var root = NewUI("DailyRewardPopup", null);
        root.anchorMin = Vector2.zero;
        root.anchorMax = Vector2.one;
        root.offsetMin = Vector2.zero;
        root.offsetMax = Vector2.zero;
        var comp = root.gameObject.AddComponent<DailyRewardPopup>();

        // Shade
        var shade = NewUI("Shade", root);
        shade.anchorMin = Vector2.zero; shade.anchorMax = Vector2.one;
        shade.offsetMin = Vector2.zero; shade.offsetMax = Vector2.zero;
        var shadeImg = AddImage(shade, null);
        shadeImg.color = new Color(0f, 0f, 0f, 0.6f);
        shadeImg.preserveAspect = false;

        // Card
        var card = NewUI("Card", root);
        card.sizeDelta = new Vector2(880f, 1500f);
        AddImage(card, Art("Panel_Special"));
        comp.cardRect = card;

        var title = NewUI("Title", card);
        title.sizeDelta = new Vector2(620f, 150f);
        title.anchoredPosition = new Vector2(0f, 620f);
        AddImage(title, Art("Text_DailyReward"));

        var decor = NewUI("Decor", card);
        decor.sizeDelta = new Vector2(820f, 400f);
        decor.anchoredPosition = new Vector2(0f, -560f);
        AddImage(decor, Art("Decor"));

        var dc = NewUI("DayContainer", card);
        dc.sizeDelta = new Vector2(780f, 1000f);
        dc.anchoredPosition = new Vector2(0f, 20f);
        var grid = dc.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(230f, 250f);
        grid.spacing = new Vector2(20f, 20f);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;
        grid.childAlignment = TextAnchor.MiddleCenter;
        comp.dayContainer = dc;
        comp.dayItemPrefab = itemPrefab.GetComponent<DailyRewardItem>();

        // Countdown tới lượt nhận kế tiếp
        var countdown = NewUI("Countdown", card);
        comp.txtCountdown = AddText(countdown, "Claim now!", 34f,
            new Vector2(800f, 60f), new Vector2(0f, 470f));
        comp.txtCountdown.color = new Color(0.55f, 0.22f, 0.02f, 1f);

        // BtnClaim
        var claim = NewUI("BtnClaim", card);
        claim.sizeDelta = new Vector2(420f, 130f);
        claim.anchoredPosition = new Vector2(0f, -660f);
        AddImage(claim, Art("Button_Green"), sliced: true);
        var claimBtn = claim.gameObject.AddComponent<Button>();
        var claimTxt = NewUI("Label", claim);
        AddText(claimTxt, "CLAIM", 48f, new Vector2(400f, 100f), Vector2.zero);
        comp.btnClaim = claimBtn;

        // BtnClaimInactive
        var claimInactive = NewUI("BtnClaimInactive", card);
        claimInactive.sizeDelta = new Vector2(420f, 130f);
        claimInactive.anchoredPosition = new Vector2(0f, -660f);
        var claimInactiveImg = AddImage(claimInactive, Art("Button_Green"), sliced: true);
        claimInactiveImg.color = new Color(0.55f, 0.55f, 0.55f, 1f);
        var claimInactiveBtn = claimInactive.gameObject.AddComponent<Button>();
        claimInactiveBtn.interactable = false;
        var claimInactiveTxt = NewUI("Label", claimInactive);
        AddText(claimInactiveTxt, "CLAIM", 48f, new Vector2(400f, 100f), Vector2.zero);
        claimInactive.gameObject.SetActive(false);
        comp.btnClaimInactive = claimInactiveBtn;

        // BtnClose
        var close = NewUI("BtnClose", card);
        close.sizeDelta = new Vector2(90f, 90f);
        close.anchorMin = close.anchorMax = new Vector2(1f, 1f);
        close.pivot = new Vector2(1f, 1f);
        close.anchoredPosition = new Vector2(-24f, -24f);
        AddImage(close, Art("Button_Close"));
        comp.btnClose = close.gameObject.AddComponent<Button>();

        comp.config = cfg;

        return Save(root, ConfigDir, "DailyRewardPopup");
    }
}

/// <summary>Tự build config + prefab Daily Reward khi Unity reload nếu còn thiếu (khỏi phải bấm menu).</summary>
[InitializeOnLoad]
internal static class DailyRewardAutoSetup
{
    static DailyRewardAutoSetup()
    {
        EditorApplication.delayCall += () =>
        {
            const string popup = "Assets/_Game/Resources/DailyRewardPopup.prefab";
            if (System.IO.File.Exists(popup)) return;
            DailyRewardSetupEditor.BuildAll();
        };
    }
}
#endif
