namespace _Game.Scripts.Meta
{
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    // Shop local: chỉ gói mua bằng coin (gói coin + booster).
    // Không IAP, không IAA (không xem ads để nhận thưởng).
    // Item được instantiate từ prefab `ShopItem.prefab`.
    public sealed class ShopUI : MonoBehaviour
    {
        [Header("Container item (content của scroll view)")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemPrefab; // ShopItem.prefab

        [Header("Thông báo mua")]
        [SerializeField] private TMP_Text txtNotify;
        [SerializeField] private float notifyDuration = 2f;

        private readonly List<ShopItemView> _items = new List<ShopItemView>();
        private float _notifyTimer;
        private EconomyService _economy;

        private void Awake()
        {
            _economy = EconomyService.Instance;
        }

        private void OnEnable()
        {
            if (_economy == null) _economy = EconomyService.Instance;
            if (_economy != null) _economy.OnCoinChanged += OnCoinChanged;

            Refresh();
        }

        private void OnDisable()
        {
            if (_economy != null) _economy.OnCoinChanged -= OnCoinChanged;
        }

        private void Update()
        {
            if (_notifyTimer > 0)
            {
                _notifyTimer -= Time.deltaTime;
                if (_notifyTimer <= 0 && txtNotify != null)
                    txtNotify.text = string.Empty;
            }
        }

        private void OnCoinChanged(int amount) => RefreshAffordability();

        public void Refresh()
        {
            ClearItems();

            // Service có thể được tạo SAU khi HUD này Awake (HomeUI.Awake tạo
            // EconomyService/ShopService khi mở scene Home trực tiếp) -> phải
            // resolve lúc dùng, không cache ở Awake.
            var shop = ShopService.Instance;
            if (shop == null)
            {
                Debug.LogError("[ShopUI] Chưa có ShopService — không dựng được item.");
                return;
            }
            if (itemContainer == null || itemPrefab == null)
            {
                Debug.LogError("[ShopUI] Thiếu itemContainer hoặc itemPrefab (ShopItem.prefab).");
                return;
            }

            foreach (var item in shop.GetItems())
            {
                var go = Instantiate(itemPrefab, itemContainer);

                var view = go.GetComponent<ShopItemView>();
                if (view == null)
                {
                    Debug.LogError("[ShopUI] Prefab item thiếu component ShopItemView.");
                    Destroy(go);
                    continue;
                }

                view.Bind(item, shop, ShowNotify);
                _items.Add(view);
            }
        }

        private void ClearItems()
        {
            foreach (var view in _items)
            {
                if (view != null) Destroy(view.gameObject);
            }
            _items.Clear();
        }

        private void RefreshAffordability()
        {
            var shop = ShopService.Instance;
            if (shop == null) return;
            foreach (var view in _items)
            {
                if (view != null) view.RefreshAffordable();
            }
        }

        private void ShowNotify(string message)
        {
            if (txtNotify != null) txtNotify.text = message;
            _notifyTimer = notifyDuration;
        }
    }
}
