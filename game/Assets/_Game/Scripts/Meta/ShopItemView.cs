namespace _Game.Scripts.Meta
{
    using System;
    using _Game.Scripts.Core;
    using _Game.Scripts.Gameplay;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    // View của một dòng item trong shop. Gắn lên prefab `ShopItem.prefab`;
    // ShopUI chỉ instantiate prefab rồi gọi Bind().
    public sealed class ShopItemView : MonoBehaviour
    {
        [Header("Parts (gán trong prefab)")]
        [SerializeField] private Image icon;
        [SerializeField] private TMP_Text title;
        [SerializeField] private TMP_Text amount;
        [SerializeField] private TMP_Text cost;
        [SerializeField] private Button btnBuy;
        [SerializeField] private TMP_Text btnLabel;

        private ShopService _shop;
        private ShopItem _item;

        public void Bind(ShopItem item, ShopService shop, Action<string> onNotify)
        {
            _item = item;
            _shop = shop;

            if (icon != null) icon.sprite = item != null ? item.icon : null;
            if (title != null) title.text = item != null ? item.displayName : string.Empty;
            if (amount != null) amount.text = BuildAmount(item);
            if (cost != null) cost.text = shop != null ? shop.GetPriceLabel(item) : string.Empty;
            if (btnLabel != null) btnLabel.text = "BUY";

            if (btnBuy != null)
            {
                // Nút luôn bấm được: nếu không đủ coin thì báo "Not enough coins"
                // thay vì khoá nút (người chơi không hiểu vì sao bấm không được).
                btnBuy.interactable = true;
                btnBuy.onClick.RemoveAllListeners();
                btnBuy.onClick.AddListener(() => OnBuy(onNotify));
            }
        }

        private static string BuildAmount(ShopItem item)
        {
            if (item == null) return string.Empty;
            if (item.IsBooster) return $"x{item.boosterAmount} {item.boosterType}";
            return item.coinAmount > 0 ? $"+{item.coinAmount} coins" : string.Empty;
        }

        private void OnBuy(Action<string> onNotify)
        {
            if (_shop == null || _item == null) return;

            if (_shop.Buy(_item.id))
            {
                Notify($"Purchased {_item.displayName}", onNotify);
                // Màn hình nhận thưởng sau khi mua.
                // Dùng Acquire() vì GameObject gốc của popup để TẮT trong scene
                // -> Instance còn null nếu chỉ gọi PurchaseSuccessPopup.Instance.
                var popup = PurchaseSuccessPopup.Acquire();
                if (popup != null) popup.Show(_item);
                else Debug.LogWarning("[ShopItemView] Không tìm thấy PurchaseSuccessPopup trong scene.");
            }
            else
            {
                Notify(ReasonToMessage(_shop.LastError), onNotify);
            }
        }

        private static string ReasonToMessage(string reason) => reason switch
        {
            "not_enough_coin" => "Not enough coins",
            "item_not_found" => "Item not found",
            "economy_missing" => "Not ready, please try again",
            _ => "Purchase failed",
        };

        /// <summary>Báo kết quả bằng tooltip (giống phần báo của dự án gốc).</summary>
        private static void Notify(string message, Action<string> onNotify)
        {
            var tooltip = _KIT.KitTooltipManager.Instance;
            if (tooltip != null) tooltip.Show(message);
            onNotify?.Invoke(message);
        }
    }
}
