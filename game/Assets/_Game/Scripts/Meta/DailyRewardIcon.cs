namespace _Game.Scripts.Meta
{
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>1 widget phần thưởng nhỏ (icon + số lượng) trong ô ngày của Daily Reward.</summary>
    public class DailyRewardIcon : MonoBehaviour
    {
        public Image icon;
        public TextMeshProUGUI amount;

        public void Set(Sprite sprite, string amountText, bool dimmed = false)
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                if (sprite == null) icon.color = new Color(1f, 1f, 1f, 0f);
                else icon.color = dimmed ? new Color(1f, 1f, 1f, 0.45f) : Color.white;
            }

            if (amount != null)
            {
                amount.text = amountText;
                amount.alpha = dimmed ? 0.45f : 1f;
            }
        }
    }
}
