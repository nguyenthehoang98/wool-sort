namespace _Game.Scripts.Meta
{
    using _Game.Scripts.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Một ô ngày trong Daily Reward popup (7 ô). Hiển thị các phần thưởng của ngày đó + trạng thái.
    /// Kéo ref trong prefab DailyRewardItem.</summary>
    public class DailyRewardItem : MonoBehaviour
    {
        [Header("State overlays (tùy chọn)")]
        public GameObject normalState;
        public GameObject todayState;
        public GameObject missedState;

        [Header("Reward display")]
        public Transform rewardContainer;
        public DailyRewardIcon rewardIconPrefab;
        [Tooltip("Icon dự phòng khi entry.icon trống")]
        public Sprite coinIcon;
        public Sprite addSlotIcon;
        public Sprite cleanIcon;
        public Sprite magicBoxIcon;

        [Header("Claimed")]
        public GameObject claimedMark;

        [Header("Text")]
        public TextMeshProUGUI txtDay;

        public int DayIndex { get; private set; }

        public void Setup(int dayIndex, DailyRewardDay day, bool isToday, bool isClaimed, bool isMissed)
        {
            DayIndex = dayIndex;

            if (txtDay != null) txtDay.text = $"DAY {dayIndex + 1}";

            // Dựng lại icon phần thưởng
            if (rewardContainer != null)
            {
                for (int i = rewardContainer.childCount - 1; i >= 0; i--)
                    Destroy(rewardContainer.GetChild(i).gameObject);

                if (day != null && day.rewards != null && rewardIconPrefab != null)
                {
                    foreach (var r in day.rewards)
                    {
                        var go = Instantiate(rewardIconPrefab, rewardContainer);
                        go.Set(r.icon != null ? r.icon : IconFor(r.rewardId), AmountText(r));
                    }
                }
            }

            if (normalState != null) normalState.SetActive(!isToday && !isMissed);
            if (todayState != null) todayState.SetActive(isToday);
            if (missedState != null) missedState.SetActive(isMissed);

            if (claimedMark != null) claimedMark.SetActive(isClaimed);
            if (rewardContainer != null) rewardContainer.gameObject.SetActive(!isClaimed);
        }

        private Sprite IconFor(string rewardId) => rewardId switch
        {
            "coin" => coinIcon,
            "addslot" => addSlotIcon,
            "clean" => cleanIcon,
            "magicbox" => magicBoxIcon,
            _ => null
        };

        private static string AmountText(DailyRewardEntry e)
        {
            if (e == null) return string.Empty;
            return e.rewardId == "coin" ? e.amount.ToString() : $"x{e.amount}";
        }
    }
}
