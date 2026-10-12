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
public class GameData
{
    private static GameData _instance;
    public static GameData Instance => _instance ??= new GameData();

    private readonly Dictionary<BoosterType, int> _counts =
        new Dictionary<BoosterType, int>();

    public int Level() => 1;
    public bool IsBoosterTutShown(BoosterType _) => true;
    public void MarkBoosterTutShown(BoosterType _) { }

    // Mặc định 99 để test core gameplay không bị chặn hết booster.
    public int GetCount(BoosterType type) =>
        _counts.TryGetValue(type, out var c) ? c : 99;

    public void AddCount(BoosterType type, int delta, string _, string __) =>
        _counts[type] = GetCount(type) + delta;
}

// --- Sự kiện đổi booster (thay GameConst gốc) ---
public static class GameConst
{
    public static event Action<ChangeBoosterArgs> OnChangeBooster;
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
