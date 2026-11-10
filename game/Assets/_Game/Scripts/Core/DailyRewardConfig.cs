namespace _Game.Scripts.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Một phần thưởng trong Daily Reward (1 ngày có thể nhiều phần thưởng).</summary>
    [Serializable]
    public class DailyRewardEntry
    {
        public string rewardId; // "coin", "addslot", "clean", "magicbox"
        public int amount;
        public Sprite icon; // gán trong Inspector (nếu trống thì DailyRewardItem tự lấy theo rewardId)
    }

    /// <summary>Phần thưởng của một ngày.</summary>
    [Serializable]
    public class DailyRewardDay
    {
        public List<DailyRewardEntry> rewards = new List<DailyRewardEntry>(1);
    }

    /// <summary>Config 7 ngày reward. Tạo bằng chuột phải Assets → Create → WoolSort/Daily Reward Config.</summary>
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "WoolSort/Daily Reward Config")]
    public class DailyRewardConfig : ScriptableObject
    {
        public List<DailyRewardDay> days = new List<DailyRewardDay>(7);

        /// <summary>Luôn đủ 7 ngày (thiếu thì bù rỗng) để popup không lệch index.</summary>
        public DailyRewardDay GetDay(int dayIndex)
        {
            if (days == null || days.Count == 0) return null;
            return days[Mathf.Clamp(dayIndex, 0, days.Count - 1)];
        }
    }
}
