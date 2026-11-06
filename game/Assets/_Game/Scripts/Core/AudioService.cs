namespace _Game.Scripts.Core
{
    using System;
    using _KIT;
    using UnityEngine;

    /// <summary>Quản lý âm thanh: BGM, SFX, UI sound. Đứng theo SettingsService (sound/music).</summary>
    public sealed class AudioService : MonoBehaviour
    {
        private static AudioService _instance;
        public static AudioService Instance
        {
            get
            {
                if (_instance == null) _instance = EnsureInstance();
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Config (kéo AudioConfig.asset vào đây)")]
        [SerializeField] private AudioConfig config;
        public AudioConfig Config { get; private set; }

        /// <summary>Gán config từ bên ngoài (dùng khi chưa gán qua Inspector).</summary>
        public void SetConfig(AudioConfig cfg) => Config = cfg;

        /// <summary>Config hiệu quả: ưu tiên Config đã set runtime, ngược lại lấy từ config Inspector hoặc Resources.</summary>
        public AudioConfig EffectiveConfig
        {
            get
            {
                if (Config != null) return Config;
                if (config != null) return config;
                var loaded = Resources.Load<AudioConfig>("AudioConfig");
                if (loaded != null)
                {
                    SetConfig(loaded);
                    return loaded;
                }
                Debug.LogWarning("[AudioService] AudioConfig not found in Resources! Place AudioConfig.asset under Assets/Resources/");
                return null;
            }
        }

        private int _musicId = -1;
        private bool _initialized;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
            DontDestroyOnLoad(gameObject);

            _initialized = true;
            ApplySettings();
        }

        /// <summary>Đảm bảo đã có instance (tự tạo GameObject nếu chưa có).</summary>
        public static AudioService EnsureInstance()
        {
            if (_instance != null) return _instance;
            var go = new GameObject("AudioService");
            return go.AddComponent<AudioService>();
        }

        private void ApplySettings()
        {
            KitSound.GlobalMusicVolume = SettingsService.Music ? 1f : 0f;
            KitSound.GlobalSoundsVolume = SettingsService.Sound ? 1f : 0f;
            KitSound.GlobalUISoundsVolume = SettingsService.Sound ? 1f : 0f;
        }

        /// <summary>Phát BGM (dùng cho Home / Gameplay). Khóa nếu music tắt.</summary>
        public void PlayMusic(AudioClip clip, bool loop = true, bool persist = true)
        {
            if (clip == null) return;
            if (!SettingsService.Music) return;
            if (_musicId >= 0)
            {
                var prev = KitSound.GetMusicAudio(_musicId);
                if (prev != null) prev.Stop();
            }
            _musicId = KitSound.PlayMusic(clip, 1f, loop, persist);
        }

        public void StopMusic()
        {
            if (_musicId >= 0)
            {
                var prev = KitSound.GetMusicAudio(_musicId);
                if (prev != null) prev.Stop();
                _musicId = -1;
            }
        }

        /// <summary>Phát SFX một lần (dùng cho win/lose/booster/coin...). Tuân theo cài Sound.</summary>
        public void PlaySfx(AudioClip clip)
        {
            if (clip == null || !SettingsService.Sound) return;
            KitSound.PlaySound(clip, 1f);
        }

        /// <summary>Phát UI sound một lần (click, mở/đóng popup...). Tuân theo cài Sound.</summary>
        public void PlayUISound(AudioClip clip)
        {
            if (clip == null || !SettingsService.Sound) return;
            KitSound.PlayUISound(clip, 1f);
        }

        /// <summary>Phát âm thanh click nút.</summary>
        public void PlayClick() => PlayUISound(EffectiveConfig?.sfxClick);

        /// <summary>Phát âm thanh mở popup.</summary>
        public void PlayPopupOpen() => PlayUISound(EffectiveConfig?.sfxPopupOpen);

        /// <summary>Phát âm thanh đóng popup.</summary>
        public void PlayPopupClose() => PlayUISound(EffectiveConfig?.sfxPopupClose);

        /// <summary>Phát âm thanh thắng màn.</summary>
        public void PlayWin() => PlaySfx(EffectiveConfig?.sfxWin);

        /// <summary>Phát âm thanh thua màn.</summary>
        public void PlayLose() => PlaySfx(EffectiveConfig?.sfxLose);

        /// <summary>Phát âm thanh dùng booster.</summary>
        public void PlayBoosterUse() => PlaySfx(EffectiveConfig?.sfxBoosterUse);

        /// <summary>Phát âm thanh mua booster.</summary>
        public void PlayBoosterBuy() => PlaySfx(EffectiveConfig?.sfxBoosterBuy);

        /// <summary>Phát âm thanh thu coin.</summary>
        public void PlayCoinCollect() => PlaySfx(EffectiveConfig?.sfxCoinCollect);
    }
}
