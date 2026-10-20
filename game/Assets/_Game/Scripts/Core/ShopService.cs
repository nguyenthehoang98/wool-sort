namespace _Game.Scripts.Core
{
    using System;
    using System.Collections.Generic;
    using _Game.Scripts.Gameplay;
    using UnityEngine;

    [Serializable]
    public class ShopItem
    {
        public string id;
        public string displayName;
        public Sprite icon; // gán từ project gốc qua Inspector
        public string iconResPath; // đường dẫn Resources để load icon khi tạo runtime
        public int coinCost;
        public BoosterType boosterType; // None nếu là gói coin
        public int boosterAmount; // số lượng booster nếu mua booster
        public int coinAmount; // số lượng coin nếu mua gói coin
    }

    public sealed class ShopService : MonoBehaviour
    {
        public static ShopService Instance { get; private set; }

        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed; // itemId, reason

        [Header("Gói có sẵn trong shop (mua bằng coin)")]
        [SerializeField] private List<ShopItem> items = new List<ShopItem>
        {
            new ShopItem { id = "coin_pack_small", displayName = "Gói coin nhỏ", coinCost = 0, coinAmount = 500, iconResPath = "Shop/coinShopPack1" },
            new ShopItem { id = "coin_pack_big", displayName = "Gói coin lớn", coinCost = 0, coinAmount = 1400, iconResPath = "Shop/coinShopPack2" },
            new ShopItem { id = "booster_addslot", displayName = "Add Slot", coinCost = 80, boosterType = BoosterType.AddSlot, boosterAmount = 1, iconResPath = "Shop/power1_120x120" },
            new ShopItem { id = "booster_clean", displayName = "Clean", coinCost = 120, boosterType = BoosterType.Clean, boosterAmount = 1, iconResPath = "Shop/power2_120x120" },
            new ShopItem { id = "booster_magicbox", displayName = "Magic Box", coinCost = 60, boosterType = BoosterType.MagicBox, boosterAmount = 1, iconResPath = "Shop/power3_120x120" },
        };

        /// <summary>Lý do mua thất bại gần nhất (để UI hiện tooltip đúng nội dung).</summary>
        public string LastError { get; private set; } = "";

        private EconomyService _economy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            LoadIcons();
        }

        /// <summary>Icon lấy từ Resources/Shop (asset pick từ dự án gốc).</summary>
        private void LoadIcons()
        {
            foreach (var item in items)
            {
                if (item == null) continue;
                if (item.icon != null) continue;
                if (string.IsNullOrEmpty(item.iconResPath)) continue;

                item.icon = Resources.Load<Sprite>(item.iconResPath);
                if (item.icon == null)
                    Debug.LogWarning($"[ShopService] Không load được icon '{item.iconResPath}' cho '{item.id}'.");
            }
        }

        private void Start()
        {
            _economy = EconomyService.Instance;
            if (_economy == null)
                Debug.LogError("[ShopService] EconomyService chưa khởi tạo trong scene.");
        }

        public IReadOnlyList<ShopItem> GetItems() => items;

        /// <summary>Nhãn giá hiển thị trên item.</summary>
        public string GetPriceLabel(ShopItem item)
        {
            if (item == null) return string.Empty;
            return item.coinCost + " coin";
        }

        public bool CanBuy(string itemId)
        {
            var item = items.Find(x => x.id == itemId);
            if (item == null) return false;
            if (_economy == null) return false;
            return _economy.GetCoin() >= item.coinCost;
        }

        public bool Buy(string itemId)
        {
            LastError = "";

            var item = items.Find(x => x.id == itemId);
            if (item == null)
            {
                LastError = "item_not_found";
                OnPurchaseFailed?.Invoke(itemId, LastError);
                return false;
            }
            if (_economy == null)
            {
                LastError = "economy_missing";
                OnPurchaseFailed?.Invoke(itemId, LastError);
                return false;
            }

            if (!_economy.SpendCoins(item.coinCost, "shop_purchase", itemId))
            {
                LastError = "not_enough_coin";
                OnPurchaseFailed?.Invoke(itemId, LastError);
                return false;
            }

            if (item.boosterType != BoosterType.None && item.boosterAmount > 0)
                _economy.AddBooster(item.boosterType, item.boosterAmount, "shop_purchase", itemId);
            else if (item.coinAmount > 0)
                _economy.AddCoins(item.coinAmount, "shop_purchase", itemId);

            OnPurchaseSucceeded?.Invoke(itemId);
            return true;
        }
    }
}
