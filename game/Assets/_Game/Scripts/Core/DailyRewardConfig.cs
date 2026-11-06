namespace _Game.Scripts.Core
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Thưởng một ngày trong Daily Reward (7 ngày). User kéo asset vào Inspector qua DailyRewardConfig.asset.</summary>
    [Serializable]
    public class DailyRewardEntry
    {
        public int day; // 1..7
        public string rewardId; // "coin", "addslot", "clean", "magicbox"
        public int amount;
        public Sprite icon; // gán trong Inspector
    }

    /// <summary>Config 7 ngày reward. Tạo bằng chuột phải Assets → Create → DailyRewardConfig.</summary>
    [CreateAssetMenu(fileName = "DailyRewardConfig", menuName = "WoolSort/Daily Reward Config")]
    public class DailyRewardConfig : ScriptableObject
    {
        public List<DailyRewardEntry> rewards = new List<DailyRewardEntry>(7);

        public DailyRewardEntry GetEntry(int dayIndex) // dayIndex 0-based
        {
            if (rewards == null || rewards.Count == 0) return null;
            return rewards[Mathf.Clamp(dayIndex, 0, rewards.Count - 1)];
        }
    }
}
