namespace _Game.Scripts.Gameplay
{
    // GIÁ TRỊ PHẢI KHỚP VỚI DỰ ÁN GỐC.
    // Scene Gameplay.unity (BoosterBtn_1/2/3: type 0/1/2), mảng
    // Titles/Descs/contentIcons trong MoreBoosterPopupUI và
    // Configs.LIST_BOOSTER_COST / LIST_BOOSTER_UNLOCK đều index theo
    // (AddSlot, Clean, MagicBox). Lệch 1 là nút bấm ra sai booster.
    // None là giá trị sentinel, KHÔNG phải index — đặt -1 để lỡ dùng
    // nhầm cũng không trỏ vào ô nào trong list.
    public enum BoosterType
    {
        AddSlot = 0,
        Clean = 1,
        MagicBox = 2,
        None = -1
    }
}
