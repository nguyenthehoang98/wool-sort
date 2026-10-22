using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using _Game.Scripts.LevelData;

/// <summary>
/// Kiểm tra lớp dữ liệu level (LevelCatalog) — thuần logic, không cần scene.
/// Đây là nguồn chân lý cho thứ tự chơi / id màn / độ khó sau khi bỏ hardcode.
///
/// Nhóm test:
/// - LC-xx: catalog thật trong Resources phải toàn vẹn.
/// - LV-xx: hàm truy vấn (GetByOrder / GetNextPlayableOrder / ...).
/// - LVAL-xx: Validate() phải bắt được từng loại lỗi.
/// </summary>
public class LevelCatalogTests
{
    private const string CatalogResourcePath = "LevelCatalog";

    private readonly List<GameObject> _spawned = new List<GameObject>();

    [TearDown]
    public void TearDown()
    {
        foreach (var go in _spawned)
            if (go != null) Object.DestroyImmediate(go);
        _spawned.Clear();
    }

    private GameObject NewLevelObject(int levelId, string nameOverride = null)
    {
        var go = new GameObject(nameOverride ?? $"Level{levelId}");
        _spawned.Add(go);
        return go;
    }

    private static LevelCatalog MakeCatalog(params LevelDefinition[] defs)
    {
        var catalog = ScriptableObject.CreateInstance<LevelCatalog>();
        catalog.SetLevels(defs.ToList(), "test", "test");
        return catalog;
    }

    private static LevelDefinition Def(int order, int levelId, LevelModeType mode = LevelModeType.Normal,
                                       GameObject prefab = null)
    {
        return new LevelDefinition
        {
            orderIndex = order,
            levelId = levelId,
            modeType = mode,
            prefab = prefab,
            seed = LevelDefinition.DefaultSeed(levelId),
        };
    }

    // ------------------------------------------------------------------
    // LC — catalog thật trong Resources
    // ------------------------------------------------------------------

    [Test]
    public void LC_01_Catalog_Exists_In_Resources()
    {
        var catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
        Assert.IsNotNull(catalog, "Không có Resources/LevelCatalog.asset — chạy `Tools/Level/Build Level Catalog`.");
    }

    [Test]
    public void LC_02_ShippedCatalog_Has_No_Validation_Errors()
    {
        var catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
        Assert.IsNotNull(catalog, "Thiếu LevelCatalog.asset.");

        var errors = catalog.Validate()
            .Where(i => i.severity == LevelCatalogSeverity.Error)
            .ToList();

        Assert.IsEmpty(errors,
            "LevelCatalog có lỗi:\n" + string.Join("\n", errors.Select(e => e.ToString())));
    }

    [Test]
    public void LC_03_ShippedCatalog_OrderIndexes_Are_Contiguous_From_One()
    {
        var catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
        Assert.IsNotNull(catalog);

        var orders = catalog.Levels.Where(l => l != null).Select(l => l.orderIndex).OrderBy(x => x).ToList();
        Assert.IsNotEmpty(orders, "Catalog rỗng.");

        for (int i = 0; i < orders.Count; i++)
            Assert.AreEqual(i + 1, orders[i], $"orderIndex phải liên tục từ 1, lệch ở vị trí {i}.");
    }

    [Test]
    public void LC_04_ShippedCatalog_LevelIds_Are_Unique()
    {
        var catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
        Assert.IsNotNull(catalog);

        var ids = catalog.Levels.Where(l => l != null).Select(l => l.levelId).ToList();
        var dup = ids.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        Assert.IsEmpty(dup, "levelId bị trùng: " + string.Join(", ", dup));
    }

    [Test]
    public void LC_05_ShippedCatalog_Every_Level_Has_Prefab_Named_After_LevelId()
    {
        var catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
        Assert.IsNotNull(catalog);

        foreach (var def in catalog.Levels.Where(l => l != null))
        {
            Assert.IsNotNull(def.prefab, $"Màn order {def.orderIndex} (id {def.levelId}) chưa có prefab.");
            Assert.AreEqual($"Level{def.levelId}", def.prefab.name,
                $"Tên prefab của màn id {def.levelId} phải là 'Level{def.levelId}'.");
        }
    }

    [Test]
    public void LC_06_ShippedCatalog_ModeTypes_Are_Valid()
    {
        var catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
        Assert.IsNotNull(catalog);

        foreach (var def in catalog.Levels.Where(l => l != null))
            Assert.IsTrue(System.Enum.IsDefined(typeof(LevelModeType), def.modeType),
                $"Màn id {def.levelId} có modeType không hợp lệ: {def.modeType}.");
    }

    [Test]
    public void LC_07_ShippedCatalog_Seed_Is_Deterministic_Per_LevelId()
    {
        var catalog = Resources.Load<LevelCatalog>(CatalogResourcePath);
        Assert.IsNotNull(catalog);

        foreach (var def in catalog.Levels.Where(l => l != null))
            Assert.AreEqual(LevelDefinition.DefaultSeed(def.levelId), def.seed,
                $"Seed của màn id {def.levelId} phải theo công thức cố định.");
    }

    // ------------------------------------------------------------------
    // LV — hàm truy vấn
    // ------------------------------------------------------------------

    [Test]
    public void LV_01_GetByOrder_Returns_Definition_With_Matching_OrderIndex()
    {
        var catalog = MakeCatalog(Def(1, 11), Def(2, 22));

        var def = catalog.GetByOrder(2);
        Assert.IsNotNull(def);
        Assert.AreEqual(2, def.orderIndex);
        Assert.AreEqual(22, def.levelId);
    }

    [Test]
    public void LV_02_GetByOrder_Returns_Null_For_Unknown_Order()
    {
        var catalog = MakeCatalog(Def(1, 11));
        Assert.IsNull(catalog.GetByOrder(99));
    }

    [Test]
    public void LV_03_GetLevelIdByOrder_Falls_Back_To_Order_When_Level_Not_In_Catalog()
    {
        var catalog = MakeCatalog(Def(1, 11));
        Assert.AreEqual(11, catalog.GetLevelIdByOrder(1));
        Assert.AreEqual(7, catalog.GetLevelIdByOrder(7), "Không có màn thì id = thứ tự (fallback hành vi cũ).");
    }

    [Test]
    public void LV_04_GetNextPlayableOrder_Skips_Levels_Without_Asset()
    {
        var catalog = MakeCatalog(
            Def(1, 11, LevelModeType.Normal, NewLevelObject(11)),
            Def(2, 22), // chưa có prefab -> bỏ qua
            Def(3, 33, LevelModeType.Hard, NewLevelObject(33)));

        Assert.AreEqual(3, catalog.GetNextPlayableOrder(1));
    }

    [Test]
    public void LV_05_GetNextPlayableOrder_Returns_Zero_When_No_Further_Playable_Level()
    {
        var catalog = MakeCatalog(Def(1, 11, LevelModeType.Normal, NewLevelObject(11)));
        Assert.AreEqual(0, catalog.GetNextPlayableOrder(1));
    }

    [Test]
    public void LV_06_GetFirstPlayableOrder_Skips_Levels_Without_Asset()
    {
        var catalog = MakeCatalog(
            Def(1, 11), // chưa có prefab
            Def(2, 22, LevelModeType.Normal, NewLevelObject(22)));

        Assert.AreEqual(2, catalog.GetFirstPlayableOrder());
    }

    [Test]
    public void LV_07_PlayableCount_Counts_Only_Levels_With_Asset()
    {
        var catalog = MakeCatalog(
            Def(1, 11, LevelModeType.Normal, NewLevelObject(11)),
            Def(2, 22));

        Assert.AreEqual(2, catalog.TotalLevels);
        Assert.AreEqual(1, catalog.PlayableCount);
        Assert.IsTrue(catalog.IsPlayable(1));
        Assert.IsFalse(catalog.IsPlayable(2));
    }

    [Test]
    public void LV_08_GetModeTypeByOrder_Returns_Mode_Of_Level()
    {
        var catalog = MakeCatalog(Def(1, 11, LevelModeType.VeryHard, NewLevelObject(11)));
        Assert.AreEqual(LevelModeType.VeryHard, catalog.GetModeTypeByOrder(1));
        Assert.AreEqual(LevelModeType.Normal, catalog.GetModeTypeByOrder(99));
    }

    // ------------------------------------------------------------------
    // LVAL — Validate() bắt lỗi
    // ------------------------------------------------------------------

    [Test]
    public void LVAL_01_Valid_Catalog_Has_No_Errors()
    {
        var catalog = MakeCatalog(
            Def(1, 11, LevelModeType.Normal, NewLevelObject(11)),
            Def(2, 22, LevelModeType.Hard, NewLevelObject(22)));

        var errors = catalog.Validate().Where(i => i.severity == LevelCatalogSeverity.Error).ToList();
        Assert.IsEmpty(errors, string.Join("\n", errors.Select(e => e.ToString())));
    }

    [Test]
    public void LVAL_02_Detects_Duplicate_OrderIndex()
    {
        var catalog = MakeCatalog(
            Def(1, 11, LevelModeType.Normal, NewLevelObject(11)),
            Def(1, 22, LevelModeType.Normal, NewLevelObject(22)));

        Assert.IsTrue(catalog.Validate().Any(i => i.severity == LevelCatalogSeverity.Error
                                                  && i.message.Contains("orderIndex bị trùng")),
            "Phải bắt được orderIndex trùng.");
    }

    [Test]
    public void LVAL_03_Detects_Duplicate_LevelId()
    {
        var catalog = MakeCatalog(
            Def(1, 11, LevelModeType.Normal, NewLevelObject(11)),
            Def(2, 11, LevelModeType.Normal, NewLevelObject(11)));

        Assert.IsTrue(catalog.Validate().Any(i => i.severity == LevelCatalogSeverity.Error
                                                  && i.message.Contains("levelId bị trùng")),
            "Phải bắt được levelId trùng.");
    }

    [Test]
    public void LVAL_04_Detects_Missing_Prefab()
    {
        var catalog = MakeCatalog(Def(1, 11));
        Assert.IsTrue(catalog.Validate().Any(i => i.severity == LevelCatalogSeverity.Error
                                                  && i.message.Contains("Thiếu prefab")),
            "Phải bắt được màn thiếu prefab.");
    }

    [Test]
    public void LVAL_05_Detects_Prefab_Name_Mismatch()
    {
        var catalog = MakeCatalog(Def(1, 11, LevelModeType.Normal, NewLevelObject(11, "KhacTen")));
        Assert.IsTrue(catalog.Validate().Any(i => i.severity == LevelCatalogSeverity.Error
                                                  && i.message.Contains("không khớp levelId")),
            "Phải bắt được tên prefab không khớp levelId.");
    }

    [Test]
    public void LVAL_06_Detects_Non_Contiguous_OrderIndex()
    {
        var catalog = MakeCatalog(
            Def(1, 11, LevelModeType.Normal, NewLevelObject(11)),
            Def(3, 33, LevelModeType.Normal, NewLevelObject(33)));

        Assert.IsTrue(catalog.Validate().Any(i => i.severity == LevelCatalogSeverity.Error
                                                  && i.message.Contains("không liên tục")),
            "Phải bắt được orderIndex không liên tục.");
    }

    [Test]
    public void LVAL_07_Empty_Catalog_Is_Error()
    {
        var catalog = MakeCatalog();
        Assert.IsTrue(catalog.Validate().Any(i => i.severity == LevelCatalogSeverity.Error
                                                  && i.message.Contains("rỗng")),
            "Catalog rỗng phải là lỗi.");
    }
}
