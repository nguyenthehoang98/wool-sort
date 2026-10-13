// MissingStubs.cs
// Shim tạm cho các hệ meta/framework vắng mặt trong bản core này.
// Khi có đủ source gốc (LFramework, I2, SettingManager, shop/ads...), xóa file này
// và trả lại code gọi gốc.

using System;
using System.Collections.Generic;
using _Game.Scripts;
using _Game.Scripts.Gameplay;
using UnityEngine;

// --- Tiến trình/meta-game tối giản (thay GameData gốc) ---
// Hỗ trợ test nhanh: SetCount/SetLevel/Reset + RaiseChanged để UI refresh ngay.
public class GameData
{
    private static GameData _instance;
    public static GameData Instance => _instance ??= new GameData();

    private readonly Dictionary<BoosterType, int> _counts =
        new Dictionary<BoosterType, int>();

    // Cho phép test lock: mặc định 1 (unlock hết vì LIST_BOOSTER_UNLOCK={1,1,1}).
    // BoosterQuickTest / menu Tools có thể đổi lúc Play.
    private int _levelOverride = 1;

    public int Level() => _levelOverride;
    public bool IsBoosterTutShown(BoosterType _) => true;
    public void MarkBoosterTutShown(BoosterType _) { }

    // Mặc định 99 để test core gameplay không bị chặn hết booster.
    public int GetCount(BoosterType type) =>
        _counts.TryGetValue(type, out var c) ? c : 99;

    public void AddCount(BoosterType type, int delta, string _, string __)
    {
        _counts[type] = GetCount(type) + delta;
        GameConst.RaiseChanged();
    }

    // ---- Test hooks (gọi từ BoosterQuickTest / Tools menu / Console) ----
    public void SetCount(BoosterType type, int value, bool notify = true)
    {
        _counts[type] = Mathf.Max(0, value);
        if (notify) GameConst.RaiseChanged();
    }

    public void SetAllCounts(int addSlot, int clean, int magic, bool notify = true)
    {
        _counts[BoosterType.AddSlot] = Mathf.Max(0, addSlot);
        _counts[BoosterType.Clean] = Mathf.Max(0, clean);
        _counts[BoosterType.MagicBox] = Mathf.Max(0, magic);
        if (notify) GameConst.RaiseChanged();
    }

    public void ResetCounts(bool notify = true)
    {
        _counts.Clear();
        if (notify) GameConst.RaiseChanged();
    }

    public void SetLevel(int level, bool notify = true)
    {
        _levelOverride = Mathf.Max(1, level);
        if (notify) GameConst.RaiseChanged();
    }
}

// --- Sự kiện đổi booster (thay GameConst gốc) ---
public static class GameConst
{
    public static event Action<ChangeBoosterArgs> OnChangeBooster;

    public static void RaiseChanged()
    {
        try { OnChangeBooster?.Invoke(new ChangeBoosterArgs()); }
        catch (Exception e) { Debug.LogWarning("[GameConst] OnChangeBooster throw: " + e.Message); }
    }
}

public class ChangeBoosterArgs { }

// --- Nút mở khóa box (UI prefab, chưa có trong bản core) ---
public class BoxUnlockButton : MonoBehaviour { }

// --- Chổi quét của booster Clean (visual, stub không hiệu ứng) ---
public class BroomSweep : MonoBehaviour
{
    public void Show(bool value)
    {
        if (this != null && gameObject != null) gameObject.SetActive(value);
    }

    public void PlaySweep(Action onDone) => onDone?.Invoke();
}
