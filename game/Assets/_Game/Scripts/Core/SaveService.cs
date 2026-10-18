namespace _Game.Scripts.Core
{
    using System;
    using UnityEngine;

    public static class SaveService
    {
        private const string Key = "WoolSort_PlayerData_v1";

        public static void Save(PlayerData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
        }

        public static PlayerData Load()
        {
            if (!PlayerPrefs.HasKey(Key)) return null;
            var json = PlayerPrefs.GetString(Key);
            if (string.IsNullOrEmpty(json)) return null;
            try
            {
                return JsonUtility.FromJson<PlayerData>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SaveService] Lỗi đọc save data, reset mới: {e.Message}");
                return null;
            }
        }

        public static void Delete() => PlayerPrefs.DeleteKey(Key);

        public static bool HasSave() => PlayerPrefs.HasKey(Key);
    }
}
