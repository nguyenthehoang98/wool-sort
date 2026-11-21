namespace _Game.Scripts.Meta
{
    using _Game.Scripts.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Một ô ngày trong Daily Reward popup (7 ô). Hiển thị các phần thưởng của ngày đó + trạng thái.
    /// 3 trạng thái: khoá / có thể nhận / đã nhận. Kéo ref trong prefab DailyRewardItem.</summary>
    public class DailyRewardItem : MonoBehaviour
    {
        [Header("State")]
        public Image background;       // Image nền của ô (Slot_Daily_1A)
        public GameObject todayState;  // viền "có thể nhận" (Slot_Daily_1B)
        public GameObject lockMask;    // lớp phủ tối cho trạng thái khoá
        public GameObject lockMark;    // icon ổ khoá
        public GameObject claimedMark; // dấu tick đã nhận

        [Header("Reward display")]
        public Transform rewardContainer;
        public DailyRewardIcon rewardIconPrefab;
        [Tooltip("Icon dự phòng khi entry.icon trống")]
        public Sprite coinIcon;
        public Sprite addSlotIcon;
        public Sprite cleanIcon;
        public Sprite magicBoxIcon;

        [Header("Text")]
        public TextMeshProUGUI txtDay;

        public int DayIndex { get; private set; }

        public void Setup(int dayIndex, DailyRewardDay day, bool isToday, bool isClaimed, bool isLocked)
        {
            DayIndex = dayIndex;
            ResolveRefs();

            if (txtDay != null)
            {
                txtDay.text = $"DAY {dayIndex + 1}";
                txtDay.color = isLocked ? new Color(0.72f, 0.72f, 0.72f, 1f) : Color.white;
            }

            // Nền: khoá thì làm xám, còn lại giữ màu gốc; hôm nay phủ viền xanh phía dưới nội dung.
            if (background != null)
                background.color = isLocked ? new Color(0.62f, 0.62f, 0.62f, 1f) : Color.white;

            if (todayState != null) todayState.SetActive(isToday);
            if (lockMask != null) lockMask.SetActive(isLocked);
            if (lockMark != null) lockMark.SetActive(isLocked);
            if (claimedMark != null) claimedMark.SetActive(isClaimed);

            // Dựng lại icon phần thưởng — LUÔN hiện (kể cả đã nhận) để thấy nội dung quà.
            if (rewardContainer != null)
            {
                for (int i = rewardContainer.childCount - 1; i >= 0; i--)
                    Destroy(rewardContainer.GetChild(i).gameObject);

                if (day != null && day.rewards != null && rewardIconPrefab != null)
                {
                    foreach (var r in day.rewards)
                    {
                        var go = Instantiate(rewardIconPrefab, rewardContainer);
                        go.Set(r.icon != null ? r.icon : IconFor(r.rewardId), AmountText(r), isLocked);
                    }
                }
            }
        }

        /// <summary>Tự nối ref theo tên con nếu prefab bị mất tham chiếu (Unity re-save / đổi cấu trúc).</summary>
        private void ResolveRefs()
        {
            if (background == null) background = GetComponent<Image>();
            if (todayState == null) todayState = Child("TodayState");
            if (lockMask == null) lockMask = Child("LockMask");
            if (lockMark == null) lockMark = Child("LockMark");
            if (claimedMark == null) claimedMark = Child("ClaimedMark");
            if (rewardContainer == null)
            {
                var t = transform.Find("RewardContainer");
                if (t != null) rewardContainer = t;
            }
            if (txtDay == null)
            {
                var t = transform.Find("TxtDay");
                if (t != null) txtDay = t.GetComponent<TextMeshProUGUI>();
            }
        }

        private GameObject Child(string childName)
        {
            var t = transform.Find(childName);
            return t != null ? t.gameObject : null;
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
