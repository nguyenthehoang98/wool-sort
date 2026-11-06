namespace _Game.Scripts.Core
{
    using System;
    using _Game.Scripts.Gameplay;
    using UnityEngine;

    public sealed class EconomyService : MonoBehaviour
    {
        public static EconomyService Instance { get; private set; }

        public event Action<int> OnCoinChanged;
        public event Action<int, int> OnHeartChanged; // current, max
        public event Action<BoosterType, int> OnBoosterChanged;

        [Header("Default values (dùng khi chưa có save)")]
        [SerializeField] private int defaultCoin = 100;
        [SerializeField] private int defaultHeart = 5;
        [SerializeField] private int defaultMaxHeart = 5;
        [SerializeField] private int heartRefillSeconds = 1800;

        private PlayerData _data;
        private bool _initialized;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Init NGAY trong Awake (không đợi Start): `Instance` đã có mà `_data`
            // thì chưa → mọi accessor đọc `_data` sẽ NRE nếu scene gọi service
            // trong Awake của nó (vd LevelList gọi GetLevel()).
            Initialize();
        }

        public void Initialize()
        {
            if (_initialized) return;
            _data = SaveService.Load() ?? NewPlayerData();
            ApplyDefaultsIfMissing();
            _initialized = true;

            // Tái đính lịch hồi heart nếu đã tới giờ (dùng UTC ticks nếu có)
            RefreshHeartFromTime();

            OnCoinChanged?.Invoke(_data.coin);
            OnHeartChanged?.Invoke(_data.heart, _data.maxHeart);
            foreach (BoosterType t in Enum.GetValues(typeof(BoosterType)))
                OnBoosterChanged?.Invoke(t, GetBoosterCount(t));
        }

        /// <summary>True nếu service đã sẵn sàng đọc (dữ liệu đã nạp).</summary>
        public bool IsInitialized => _initialized;

        public PlayerData Data => _data;

        private PlayerData NewPlayerData() => new PlayerData
        {
            coin = defaultCoin,
            heart = defaultHeart,
            maxHeart = defaultMaxHeart,
            currentLevel = 1,
            lastPlayedLevel = 1
        };

        /// <summary>
        /// Chỉ vá những field KHÔNG có giá trị hợp lệ.
        ///
        /// KHÔNG set lại `coin`/`heart` khi bằng 0: 0 là trạng thái hợp lệ
        /// (hết tiền / hết tim) — set lại mặc định là bug tự cộng 100 coin + 5 tim
        /// mỗi lần mở game.
        /// </summary>
        private void ApplyDefaultsIfMissing()
        {
            if (_data.maxHeart <= 0) _data.maxHeart = defaultMaxHeart;
            if (_data.heart < 0) _data.heart = 0;
            if (_data.heart > _data.maxHeart) _data.heart = _data.maxHeart;
            if (_data.coin < 0) _data.coin = 0;

            _data.Validate();
        }

        private void RefreshHeartFromTime()
        {
            if (_data.maxHeart <= 0) return;
            if (_data.heart >= _data.maxHeart) return;
            if (_data.heartRefillAtUtcTicks <= 0) return;
            var now = DateTime.UtcNow.Ticks;
            if (now >= _data.heartRefillAtUtcTicks)
            {
                var refilled = (int)((now - _data.heartRefillAtUtcTicks) / TimeSpan.TicksPerSecond / heartRefillSeconds);
                if (refilled > 0)
                {
                    _data.heart = Mathf.Min(_data.maxHeart, _data.heart + refilled);
                    _data.heartRefillAtUtcTicks = 0;
                    Save();
                    OnHeartChanged?.Invoke(_data.heart, _data.maxHeart);
                }
            }
        }

        private void Save() => SaveService.Save(_data);

        #region Coin

        public int GetCoin() => _data.coin;

        public bool AddCoins(int amount, string reason = null, string placement = null)
        {
            if (amount <= 0) return false;
            _data.coin += amount;
            Save();
            OnCoinChanged?.Invoke(_data.coin);
            return true;
        }

        public bool SpendCoins(int amount, string reason = null, string placement = null)
        {
            if (amount <= 0) return true;
            if (_data.coin < amount) return false;
            _data.coin -= amount;
            Save();
            OnCoinChanged?.Invoke(_data.coin);
            return true;
        }

        #endregion

        #region Heart

        public int GetHeart() => _data.heart;
        public int GetMaxHeart() => _data.maxHeart;

        public bool SpendHeart()
        {
            if (_data.infiniteHeartMinutes > 0) return true;
            if (_data.heart <= 0) return false;
            _data.heart--;
            ScheduleHeartRefill();
            Save();
            OnHeartChanged?.Invoke(_data.heart, _data.maxHeart);
            return true;
        }

        public bool AddHeart(int amount = 1)
        {
            if (amount <= 0) return false;
            _data.heart = Mathf.Min(_data.maxHeart, _data.heart + amount);
            if (_data.heart >= _data.maxHeart) _data.heartRefillAtUtcTicks = 0;
            Save();
            OnHeartChanged?.Invoke(_data.heart, _data.maxHeart);
            return true;
        }

        public void AddInfiniteHeartMinutes(int minutes)
        {
            if (minutes <= 0) return;
            _data.infiniteHeartMinutes += minutes;
            Save();
        }

        public int GetInfiniteHeartMinutes() => _data.infiniteHeartMinutes;

        private void ScheduleHeartRefill()
        {
            if (_data.heart >= _data.maxHeart)
            {
                _data.heartRefillAtUtcTicks = 0;
                return;
            }
            _data.heartRefillAtUtcTicks = DateTime.UtcNow.AddSeconds(heartRefillSeconds).Ticks;
        }

        #endregion

        #region Booster

        public int GetBoosterCount(BoosterType type) => _data.GetBoosterCount(type);

        public bool AddBooster(BoosterType type, int amount, string reason = null, string placement = null)
        {
            if (amount <= 0) return false;
            var current = _data.GetBoosterCount(type);
            _data.SetBoosterCount(type, current + amount);
            Save();
            OnBoosterChanged?.Invoke(type, _data.GetBoosterCount(type));
            return true;
        }

        public bool SpendBooster(BoosterType type, int amount = 1, string reason = null, string placement = null)
        {
            if (amount <= 0) return true;
            if (_data.GetBoosterCount(type) < amount) return false;
            _data.SetBoosterCount(type, _data.GetBoosterCount(type) - amount);
            Save();
            OnBoosterChanged?.Invoke(type, _data.GetBoosterCount(type));
            return true;
        }

        #endregion

        #region Level

        public int GetLevel() => _data.currentLevel;

        // Level được CHỌN để chơi (khác với currentLevel = level sẽ chơi tiếp).
        // Dùng khi bấm 1 ô trong danh sách chọn màn; GameManager đọc rồi xoá để
        // lần load sau quay về level sẽ chơi tiếp.
        private int _playLevel;

        public void PlayLevel(int level) => _playLevel = Mathf.Max(1, level);
        public int GetPlayLevel() => _playLevel > 0 ? _playLevel : GetLevel();
        public void ClearPlayLevel() => _playLevel = 0;

        public void SetLevel(int level)        {
            if (level < 1) level = 1;
            _data.currentLevel = level;
            if (_data.lastPlayedLevel < level) _data.lastPlayedLevel = level;
            Save();
        }

        public int GetLastPlayedLevel() => _data.lastPlayedLevel;

        #endregion
    }
}
