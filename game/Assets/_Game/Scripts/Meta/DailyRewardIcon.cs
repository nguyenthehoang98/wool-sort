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

        public void Set(Sprite sprite, string amountText)
        {
            if (icon != null)
            {
                icon.sprite = sprite;
                icon.color = sprite != null ? Color.white : new Color(1f, 1f, 1f, 0f);
            }
            if (amount != null) amount.text = amountText;
        }
    }
}
