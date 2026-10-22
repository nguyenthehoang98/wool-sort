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
    /// Nền ô chọn theo độ khó (Normal / Hard / VeryHard) — giống 4 state của bản gốc
    /// (stateNormal / stateCurrent / stateHard / stateVeryHard); sprite "current" chỉ
    /// dùng cho màn Normal đang chơi. Hard / VeryHard hiện thêm badge `icon_hard` /
    /// `icon_veryhard` (bản gốc: ImgDecor).
    /// </summary>
    public class LevelButton : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text label;
        [SerializeField] private Image badge;
        [SerializeField] private Button button;

        public void Bind(int level, bool unlocked, bool isCurrent, LevelModeType modeType,
                         Sprite normalSprite, Sprite currentSprite,
                         Sprite hardSprite, Sprite veryHardSprite,
                         Sprite hardIconSprite, Sprite veryHardIconSprite,
                         Action<int> onClick)
        {
            if (icon != null)
            {
                var sprite = ResolveGroundSprite(modeType, isCurrent,
                                                 normalSprite, currentSprite,
                                                 hardSprite, veryHardSprite);
                if (sprite != null) icon.sprite = sprite;
                icon.color = unlocked ? Color.white : new Color(0.42f, 0.42f, 0.42f, 1f);
            }

            if (label != null) label.text = level.ToString();
            ApplyBadge(modeType, hardIconSprite, veryHardIconSprite);

            if (button != null)
            {
                button.interactable = unlocked;
                button.onClick.RemoveAllListeners();
                if (unlocked) button.onClick.AddListener(() => onClick?.Invoke(level));
            }
        }

        /// <summary>
        /// Nền ô theo độ khó; màn Normal đang chơi dùng sprite "current".
        /// Thiếu sprite độ khó thì lùi về sprite Normal.
        /// </summary>
        private static Sprite ResolveGroundSprite(LevelModeType modeType, bool isCurrent,
                                                  Sprite normal, Sprite current,
                                                  Sprite hard, Sprite veryHard)
        {
            switch (modeType)
            {
                case LevelModeType.Hard: return hard != null ? hard : normal;
                case LevelModeType.VeryHard: return veryHard != null ? veryHard : normal;
                default: return isCurrent && current != null ? current : normal;
            }
        }

        /// <summary>Badge độ khó: Hard → icon_hard, VeryHard → icon_veryhard, Normal → ẩn.</summary>
        private void ApplyBadge(LevelModeType modeType, Sprite hardIcon, Sprite veryHardIcon)
        {
            if (badge == null) return;

            Sprite sprite = null;
            if (modeType == LevelModeType.Hard) sprite = hardIcon;
            else if (modeType == LevelModeType.VeryHard) sprite = veryHardIcon;

            badge.sprite = sprite;
            badge.enabled = sprite != null;
        }
    }
}
