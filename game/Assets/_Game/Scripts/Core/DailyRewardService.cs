namespace _Game.Scripts.Core
{
    using System;
    using UnityEngine;
    using _Game.Scripts.Gameplay;

    /// <summary>Quản lý Daily Reward (7 ngày, claim 1 lần/ngày). Lưu vào PlayerData qua EconomyService.</summary>
    public sealed class DailyRewardService : MonoBehaviour
    {
        public static DailyRewardService Instance { get; private set; }

        public event Action<int> OnStreakChanged; // streak mới
        public event Action<DailyRewardEntry> OnRewardClaimed;

        [Header("Config (kéo DailyRewardConfig.asset vào đây)")]
        [SerializeField] private DailyRewardConfig config;

        private int _streak;
        private long _lastClaimUtcTicks;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            _streak = EconomyService.Instance?.Data?.dailyStreak ?? 0;
            _lastClaimUtcTicks = EconomyService.Instance?.Data?.lastDailyClaimUtcTicks ?? 0;
        }

        public bool CanClaimToday()
        {
            if (_lastClaimUtcTicks <= 0) return true;
            var last = DateTime.FromBinary(_lastClaimUtcTicks);
            var now = DateTime.UtcNow;
            return last.Date != now.Date;
        }

        public int Streak => _streak;

        public DailyRewardConfig Config => config;

        /// <summary>Claim thưởng ngày hiện tại. Trả entry đã grant hoặc null nếu chưa đến lượt/đã claim.</summary>
        public DailyRewardEntry Claim()
        {
            if (!CanClaimToday()) return null;
            if (config == null)
            {
                Debug.LogWarning("[DailyRewardService] Chưa gán DailyRewardConfig.");
                return null;
            }

            var entry = config.GetEntry(_streak); // streak 0-based -> day streak+1
            if (entry == null) return null;

            // Grant reward
            if (entry.rewardId == "coin")
            {
                EconomyService.Instance.AddCoins(entry.amount, "daily_reward", $"day_{entry.day}");
            }
            else
            {
                var type = ParseBooster(entry.rewardId);
                if (type != BoosterType.None)
                    EconomyService.Instance.AddBooster(type, entry.amount, "daily_reward", $"day_{entry.day}");
            }

            // Tăng streak (tối đa 7, rồi vòng lại)
            _streak++;
            if (_streak > 7) _streak = 1;

            _lastClaimUtcTicks = DateTime.UtcNow.Ticks;

            var data = EconomyService.Instance.Data;
            data.dailyStreak = _streak;
            data.lastDailyClaimUtcTicks = _lastClaimUtcTicks;
            SaveService.Save(data);

            OnStreakChanged?.Invoke(_streak);
            OnRewardClaimed?.Invoke(entry);
            return entry;
        }

        private static BoosterType ParseBooster(string id) => id switch
        {
            "addslot" => BoosterType.AddSlot,
            "clean" => BoosterType.Clean,
            "magicbox" => BoosterType.MagicBox,
            _ => BoosterType.None
        };
    }
}
