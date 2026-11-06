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
        public Sprite icon; // do ShopUI gán từ ref Inspector (khớp `id`)
        public int coinCost;
        public BoosterType boosterType; // None nếu là gói coin
        public int boosterAmount; // số lượng booster nếu mua booster
        public int coinAmount; // số lượng coin nếu mua gói coin

        /// <summary>
        /// Gói này có cho booster không. Dùng cờ này thay vì so sánh trực tiếp với
        /// BoosterType.None: mặc định của enum là AddSlot (0) nên gói coin phải khai
        /// báo tường minh BoosterType.None.
        /// </summary>
        public bool IsBooster => boosterType != BoosterType.None && boosterAmount > 0;
    }

    public sealed class ShopService : MonoBehaviour
    {
        public static ShopService Instance { get; private set; }

        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed; // itemId, reason

        [Header("Gói có sẵn trong shop (mua bằng coin)")]
        [SerializeField] private List<ShopItem> items = new List<ShopItem>
        {
            new ShopItem { id = "coin_pack_small", displayName = "Small Coin Pack", coinCost = 0, coinAmount = 500, boosterType = BoosterType.None },
            new ShopItem { id = "coin_pack_big", displayName = "Large Coin Pack", coinCost = 0, coinAmount = 1400, boosterType = BoosterType.None },
            new ShopItem { id = "booster_addslot", displayName = "Add Slot", coinCost = 80, boosterType = BoosterType.AddSlot, boosterAmount = 1 },
            new ShopItem { id = "booster_clean", displayName = "Clean", coinCost = 120, boosterType = BoosterType.Clean, boosterAmount = 1 },
            new ShopItem { id = "booster_magicbox", displayName = "Magic Box", coinCost = 60, boosterType = BoosterType.MagicBox, boosterAmount = 1 },
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
        }

        private void Start()
        {
            _economy = EconomyService.Instance;
            if (_economy == null)
                Debug.LogError("[ShopService] EconomyService chưa khởi tạo trong scene.");
        }

        public IReadOnlyList<ShopItem> GetItems() => items;

        /// <summary>Nhãn giá hiển thị trên item (chỉ giá trị số, icon coin nằm riêng trên UI).</summary>
        public string GetPriceLabel(ShopItem item)
        {
            if (item == null) return string.Empty;
            return NumberFormatter.FormatCoin(item.coinCost);
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

            if (item.IsBooster)
                _economy.AddBooster(item.boosterType, item.boosterAmount, "shop_purchase", itemId);
            else if (item.coinAmount > 0)
                _economy.AddCoins(item.coinAmount, "shop_purchase", itemId);

            OnPurchaseSucceeded?.Invoke(itemId);
            return true;
        }
    }
}
