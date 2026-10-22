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
    ///
    /// Màu ô được chọn theo độ khó trong LevelCatalog; màn đang chơi luôn dùng sprite
    /// "current" bất kể độ khó.
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Button button;

        public void Bind(int level, bool unlocked, bool isCurrent, LevelModeType modeType,
                         Sprite normalSprite, Sprite currentSprite,
                         Sprite hardSprite, Sprite veryHardSprite,
                         Action<int> onClick)
        {
            if (icon != null)
            {
                var sprite = ResolveSprite(modeType, normalSprite, hardSprite, veryHardSprite);
                if (isCurrent && currentSprite != null) icon.sprite = currentSprite;
                else if (sprite != null) icon.sprite = sprite;
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

        /// <summary>Sprite theo độ khó; thiếu sprite độ khó thì lùi về sprite Normal.</summary>
        private static Sprite ResolveSprite(LevelModeType modeType, Sprite normal,
                                            Sprite hard, Sprite veryHard)
        {
            switch (modeType)
            {
                case LevelModeType.Hard: return hard != null ? hard : normal;
                case LevelModeType.VeryHard: return veryHard != null ? veryHard : normal;
                default: return normal;
            }
        }
    }
}
