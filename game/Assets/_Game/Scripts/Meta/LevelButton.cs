namespace _Game.Scripts.Meta
{
    using System;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    /// <summary>
    /// Một ô màn trong danh sách chọn màn (pick từ ItemLevelHome.prefab của dự án gốc).
    ///
    /// Sprite gốc: Button_Lv_1 = normal (xanh), Button_Lv_2 = màn hiện tại (be),
    /// Button_Lv_3 = hard (tím), Button_Lv_4 = very hard (đỏ).
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

        public void Bind(int level, bool unlocked, bool isCurrent,
                         Sprite normalSprite, Sprite currentSprite, Action<int> onClick)
        {
            if (icon != null)
            {
                if (isCurrent && currentSprite != null) icon.sprite = currentSprite;
                else if (normalSprite != null) icon.sprite = normalSprite;
                icon.color = unlocked ? Color.white : new Color(0.42f, 0.42f, 0.42f, 1f);
            }

            if (label != null) label.text = level.ToString();

            if (button != null)
            {
                button.interactable = unlocked;
                button.onClick.RemoveAllListeners();
                if (unlocked) button.onClick.AddListener(() => onClick?.Invoke(level));
            }
        }
    }
}
