namespace _Game.Scripts.Meta
{
    using _Game.Scripts.Core;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>Một ô ngày trong Daily Reward popup. Kéo ref vào prefab: TxtDay, TxtAmount, Icon, Claimed, Button.</summary>
    public class DailyRewardItem : MonoBehaviour
    {
        [Header("Controls (kéo thả trong Inspector / prefab)")]
        public TextMeshProUGUI txtDay;
        public TextMeshProUGUI txtAmount;
        public Image icon;
        public GameObject claimedMark;
        public Button btn;

        [Header("Sprite trạng thái nút (tùy chọn)")]
        public Sprite btnLocked;
        public Sprite btnAvailable;

        public int Day { get; private set; }
        public DailyRewardEntry Entry { get; private set; }

        public void Setup(int day, DailyRewardEntry entry, bool unlocked, bool available, bool claimed, System.Action<int> onClaim)
        {
            Day = day;
            Entry = entry;

            if (txtDay != null) txtDay.text = $"DAY {day}";
            if (txtAmount != null && entry != null) txtAmount.text = entry.amount.ToString();
            if (icon != null && entry != null && entry.icon != null) icon.sprite = entry.icon;

            bool interactable = unlocked && available && !claimed;
            if (btn != null)
            {
                btn.interactable = interactable;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => onClaim?.Invoke(Day));
            }

            if (claimedMark != null) claimedMark.SetActive(claimed);

            if (btnLocked != null && btnAvailable != null)
            {
                var img = btn.GetComponent<Image>();
                if (img != null) img.sprite = interactable ? btnAvailable : btnLocked;
            }
        }
    }
}
