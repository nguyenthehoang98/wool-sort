namespace _Game.Scripts.Meta
{
    using System;
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using TMPro;
    using UnityEngine;

    /// <summary>Ref ảnh cho một gói trong shop: khớp `ShopItem.id`.</summary>
    [Serializable]
    public sealed class ShopIconRef
    {
        public string id;
        public Sprite sprite;
    }

    // Shop local: chỉ gói mua bằng coin (gói coin + booster).
    // Không IAP, không IAA (không xem ads để nhận thưởng).
    // Item được instantiate từ prefab `ShopItem.prefab`.
    public sealed class ShopUI : MonoBehaviour
    {
        [Header("Container item (content của scroll view)")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemPrefab; // ShopItem.prefab

        [Header("Icon từng gói (kéo sprite vào đây, khớp id)")]
        [SerializeField] private List<ShopIconRef> icons = new List<ShopIconRef>();

        [Header("Thông báo mua")]
        [SerializeField] private TMP_Text txtNotify;
        [SerializeField] private float notifyDuration = 2f;

        private readonly List<ShopItemView> _items = new List<ShopItemView>();
        private float _notifyTimer;

        private void OnEnable()
        {
            Refresh();
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
                var sprite = ResolveIcon(item.id);
                if (sprite != null)
                {
                    item.icon = sprite;
                }
                else
                {
                    Debug.LogWarning($"[ShopUI] Chưa gán icon cho gói '{item.id}' — kéo sprite vào Inspector (ShopUI > Icons).");
                }

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

        private Sprite ResolveIcon(string id)
        {
            if (icons == null) return null;
            for (int i = 0; i < icons.Count; i++)
            {
                var entry = icons[i];
                if (entry != null && entry.id == id) return entry.sprite;
            }
            return null;
        }

        private void ClearItems()
        {
            foreach (var view in _items)
            {
                if (view != null) Destroy(view.gameObject);
            }
            _items.Clear();
        }

        private void ShowNotify(string message)
        {
            if (txtNotify != null) txtNotify.text = message;
            _notifyTimer = notifyDuration;
        }
    }
}
