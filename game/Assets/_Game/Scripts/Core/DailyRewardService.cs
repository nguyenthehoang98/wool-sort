namespace _Game.Scripts.Core
{
    using System;
    using UnityEngine;
    using _Game.Scripts.Gameplay;

    /// <summary>Quản lý Daily Reward (7 ngày, claim 1 lần/ngày, mỗi ngày có thể nhiều phần thưởng).
    /// Lưu vào PlayerData qua EconomyService.</summary>
    public sealed class DailyRewardService : MonoBehaviour
    {
        public static DailyRewardService Instance { get; private set; }

        public event Action<int> OnStreakChanged;      // streak mới
        public event Action<int> OnRewardClaimed;      // day index (0-based) vừa nhận

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

        public int Streak => _streak;
        public DailyRewardConfig Config => config;

        /// <summary>Hôm nay đã nhận chưa.</summary>
        public bool CanClaimToday()
        {
            if (_lastClaimUtcTicks <= 0) return true;
            var last = DateTime.FromBinary(_lastClaimUtcTicks);
            return last.Date != DateTime.UtcNow.Date;
        }

        /// <summary>Index ngày đang hiện (0-based).</summary>
        public int TodayIndex => _streak;

        /// <summary>Nhận thưởng ngày hiện tại. Trả về index ngày (0-based) vừa nhận, hoặc -1 nếu chưa tới lượt/đã claim.</summary>
        public int Claim()
        {
            if (!CanClaimToday()) return -1;
            if (config == null)
            {
                Debug.LogWarning("[DailyRewardService] Chưa gán DailyRewardConfig.");
                return -1;
            }

            var day = config.GetDay(_streak);
            if (day == null || day.rewards == null || day.rewards.Count == 0) return -1;

            int claimedIndex = _streak;
            foreach (var r in day.rewards)
                Grant(r, claimedIndex);

            // Tăng streak (0..6), hết 7 ngày thì vòng lại từ 0
            _streak++;
            if (_streak >= 7) _streak = 0;

            _lastClaimUtcTicks = DateTime.UtcNow.Ticks;

            var data = EconomyService.Instance.Data;
            data.dailyStreak = _streak;
            data.lastDailyClaimUtcTicks = _lastClaimUtcTicks;
            SaveService.Save(data);

            OnStreakChanged?.Invoke(_streak);
            OnRewardClaimed?.Invoke(claimedIndex);
            return claimedIndex;
        }

        private static void Grant(DailyRewardEntry entry, int dayIndex)
        {
            if (entry == null || entry.amount <= 0) return;
            var reason = $"day_{dayIndex + 1}";

            if (entry.rewardId == "coin")
            {
                EconomyService.Instance.AddCoins(entry.amount, "daily_reward", reason);
                return;
            }

            var type = ParseBooster(entry.rewardId);
            if (type != BoosterType.None)
                EconomyService.Instance.AddBooster(type, entry.amount, "daily_reward", reason);
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
