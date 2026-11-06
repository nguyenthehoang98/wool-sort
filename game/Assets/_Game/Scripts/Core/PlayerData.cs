namespace _Game.Scripts.Core
{
    using System;
    using System.Collections.Generic;
    using _Game.Scripts.Gameplay;
    using UnityEngine;

    [Serializable]
    public class PlayerData
    {
        [Header("Progression")]
        public int currentLevel = 1;
        public int lastPlayedLevel = 1;

        [Header("Resources")]
        public int coin = 100;
        public int heart = 5;
        public int maxHeart = 5;
        public long heartRefillAtUtcTicks = 0;
        public int infiniteHeartMinutes = 0;

        [Header("Booster inventory")]
        public int addSlotCount = 0;
        public int cleanCount = 0;
        public int magicBoxCount = 0;

        [Header("Flags")]
        public bool removeAds = false;
        public bool sound = true;
        public bool music = true;
        public bool vibration = true;
        public bool tutorialDone = false;
        public bool dailyRewardSeen = false;

        [Header("Daily Reward")]
        public int dailyStreak = 0;
        public long lastDailyClaimUtcTicks = 0;

        public int GetBoosterCount(BoosterType type) => type switch
        {
            BoosterType.AddSlot => addSlotCount,
            BoosterType.Clean => cleanCount,
            BoosterType.MagicBox => magicBoxCount,
            _ => 0
        };

        public void SetBoosterCount(BoosterType type, int value)
        {
            switch (type)
            {
                case BoosterType.AddSlot: addSlotCount = Mathf.Max(0, value); break;
                case BoosterType.Clean: cleanCount = Mathf.Max(0, value); break;
                case BoosterType.MagicBox: magicBoxCount = Mathf.Max(0, value); break;
            }
        }

        /// <summary>Đảm bảo invariant dữ liệu: không âm, level hợp lệ.</summary>
        public void Validate()
        {
            if (coin < 0) coin = 0;
            if (addSlotCount < 0) addSlotCount = 0;
            if (cleanCount < 0) cleanCount = 0;
            if (magicBoxCount < 0) magicBoxCount = 0;
            if (currentLevel < 1) currentLevel = 1;
            if (lastPlayedLevel < currentLevel) lastPlayedLevel = currentLevel;
            if (dailyStreak < 0) dailyStreak = 0;
            if (dailyStreak > 7) dailyStreak = 7;
            if (lastDailyClaimUtcTicks < 0) lastDailyClaimUtcTicks = 0;
        }
    }
}
