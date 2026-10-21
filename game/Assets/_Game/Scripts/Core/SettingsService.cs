namespace _Game.Scripts.Core
{
    using _KIT;
    using UnityEngine;

    /// <summary>
    /// Trạng thái cài đặt của người chơi: hiệu ứng (SFX), nhạc nền, rung máy.
    ///
    /// Lưu trong <see cref="PlayerData"/> (cùng file save với coin/heart/booster) nên
    /// tự động giữ qua các màn chơi. Áp dụng ngay khi đổi: chặn/cho phép âm thanh
    /// qua <see cref="KitSound"/> và rung qua <see cref="Handheld"/>.
    ///
    /// Bản gốc có SettingManager (dùng AudioSource riêng + GameData.IsEnableSound…).
    /// Target không có hệ LFramework.Vibration nên rung dùng Handheld.Vibrate().
    /// </summary>
    public static class SettingsService
    {
        public static bool Sound { get; private set; } = true;
        public static bool Music { get; private set; } = true;
        public static bool Vibration { get; private set; } = true;

        /// <summary>Nạp trạng thái từ save và áp dụng. Gọi 1 lần khi vào game.</summary>
        public static void Load()
        {
            var data = Data;
            if (data != null)
            {
                Sound = data.sound;
                Music = data.music;
                Vibration = data.vibration;
            }
            Apply();
        }

        public static void SetSound(bool value)
        {
            if (Sound == value) return;
            Sound = value;
            Save();
        }

        public static void SetMusic(bool value)
        {
            if (Music == value) return;
            Music = value;
            Save();
        }

        public static void SetVibration(bool value)
        {
            if (Vibration == value) return;
            Vibration = value;
            Save();
        }

        public static void ToggleSound() => SetSound(!Sound);
        public static void ToggleMusic() => SetMusic(!Music);
        public static void ToggleVibration() => SetVibration(!Vibration);

        /// <summary>Rung máy nếu người chơi cho phép.</summary>
        public static void Vibrate()
        {
            if (!Vibration) return;
            Handheld.Vibrate();
        }

        private static PlayerData Data
        {
            get
            {
                var eco = EnsureEconomy();
                return eco != null ? eco.Data : null;
            }
        }

        /// <summary>
        /// EconomyService giữ PlayerData (cùng file save với coin/heart). Nếu mở thẳng
        /// scene Gameplay thì service chưa tồn tại → tự tạo + nạp save để cài đặt vẫn lưu được.
        /// </summary>
        private static EconomyService EnsureEconomy()
        {
            var eco = EconomyService.Instance;
            if (eco == null)
            {
                eco = new GameObject("EconomyService").AddComponent<EconomyService>();
            }
            if (eco != null) eco.Initialize();
            return eco;
        }

        private static void Save()
        {
            var data = Data;
            if (data != null)
            {
                data.sound = Sound;
                data.music = Music;
                data.vibration = Vibration;
                SaveService.Save(data);
            }
            Apply();
        }

        private static void Apply()
        {
            KitSound.GlobalSoundsVolume = Sound ? 1f : 0f;
            KitSound.GlobalUISoundsVolume = Sound ? 1f : 0f;
            KitSound.GlobalMusicVolume = Music ? 1f : 0f;

            // tắt thì dừng luôn những gì đang phát, không chỉ chặn gọi mới
            if (!Music) KitSound.StopAllMusic();
            if (!Sound) KitSound.StopAllSounds();
        }
    }
}
