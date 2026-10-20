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
            if (cost != null) cost.text = item != null ? item.coinCost + " coin" : string.Empty;
            if (btnLabel != null) btnLabel.text = "BUY";

            if (btnBuy != null)
            {
                btnBuy.onClick.RemoveAllListeners();
                btnBuy.onClick.AddListener(() => OnBuy(onNotify));
            }
            RefreshInteractable();
        }

        public void RefreshInteractable()
        {
            if (btnBuy == null || _shop == null || _item == null) return;
            btnBuy.interactable = _shop.CanBuy(_item.id);
        }

        private static string BuildAmount(ShopItem item)
        {
            if (item == null) return string.Empty;
            if (item.boosterType != BoosterType.None) return $"x{item.boosterAmount} {item.boosterType}";
            return item.coinAmount > 0 ? $"+{item.coinAmount} coin" : string.Empty;
        }

        private void OnBuy(Action<string> onNotify)
        {
            if (_shop == null || _item == null) return;

            if (_shop.Buy(_item.id))
            {
                onNotify?.Invoke($"Mua thành công {_item.displayName}");
            }
            else
            {
                onNotify?.Invoke("Không đủ coin để mua");
            }
            RefreshInteractable();
        }
    }
}
