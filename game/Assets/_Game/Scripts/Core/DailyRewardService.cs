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

            LoadConfigIfMissing();
            LoadState();
            NormalizeStreak();
        }

        public int Streak => _streak;
        public DailyRewardConfig Config => config;

        private void LoadConfigIfMissing()
        {
            if (config == null) config = Resources.Load<DailyRewardConfig>("DailyRewardConfig");
        }

        private void LoadState()
        {
            var data = EconomyService.Instance != null ? EconomyService.Instance.Data : null;
            _streak = data != null ? Mathf.Clamp(data.dailyStreak, 0, 6) : 0;
            _lastClaimUtcTicks = data != null ? data.lastDailyClaimUtcTicks : 0;
        }

        private void NormalizeStreak()
        {
            if (_lastClaimUtcTicks <= 0) { _streak = Mathf.Clamp(_streak, 0, 6); return; }

            DateTime last;
            try { last = DateTime.FromBinary(_lastClaimUtcTicks).ToUniversalTime().Date; }
            catch { _streak = 0; _lastClaimUtcTicks = 0; Persist(); return; }

            var today = DateTime.UtcNow.Date;
            var days = (int)(today - last).TotalDays;
            if (days >= 2)
            {
                _streak = 0;
                _lastClaimUtcTicks = 0;
                Persist();
            }
            else if (days < 0)
            {
                _lastClaimUtcTicks = today.Ticks;
                Persist();
            }
        }

        /// <summary>Hôm nay đã nhận chưa.</summary>
        public bool CanClaimToday()
        {
            if (_lastClaimUtcTicks <= 0) return true;
            DateTime last;
            try { last = DateTime.FromBinary(_lastClaimUtcTicks).ToUniversalTime().Date; }
            catch { return true; }
            return last != DateTime.UtcNow.Date;
        }

        /// <summary>Index ngày có thể claim/đang chờ trong chu kỳ 7 ngày.</summary>
        public int TodayIndex => Mathf.Clamp(_streak, 0, 6);

        /// <summary>Thời gian còn lại tới lần claim tiếp theo (UTC ngày mới).</summary>
        public TimeSpan TimeUntilNextClaim()
        {
            if (CanClaimToday()) return TimeSpan.Zero;
            var now = DateTime.UtcNow;
            var next = now.Date.AddDays(1);
            var remain = next - now;
            return remain < TimeSpan.Zero ? TimeSpan.Zero : remain;
        }

        /// <summary>Nhận thưởng ngày hiện tại. Trả về index ngày (0-based) vừa nhận, hoặc -1 nếu chưa tới lượt/đã claim.</summary>
        public int Claim()
        {
            LoadConfigIfMissing();
            if (!CanClaimToday()) return -1;
            if (config == null)
            {
                Debug.LogWarning("[DailyRewardService] Chưa gán DailyRewardConfig.");
                return -1;
            }

            NormalizeStreak();
            if (!CanClaimToday()) return -1;

            int claimedIndex = Mathf.Clamp(_streak, 0, 6);
            var day = config.GetDay(claimedIndex);
            if (day == null || day.rewards == null || day.rewards.Count == 0) return -1;

            foreach (var r in day.rewards)
                Grant(r, claimedIndex);

            _streak = (claimedIndex + 1) % 7;
            _lastClaimUtcTicks = DateTime.UtcNow.Ticks;
            Persist();

            OnStreakChanged?.Invoke(_streak);
            OnRewardClaimed?.Invoke(claimedIndex);
            return claimedIndex;
        }

        private void Persist()
        {
            if (EconomyService.Instance == null || EconomyService.Instance.Data == null) return;
            var data = EconomyService.Instance.Data;
            data.dailyStreak = Mathf.Clamp(_streak, 0, 6);
            data.lastDailyClaimUtcTicks = _lastClaimUtcTicks;
            SaveService.Save(data);
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
