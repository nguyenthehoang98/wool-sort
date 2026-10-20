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

        private ShopService _shop;
        private EconomyService _economy;
        private readonly List<GameObject> _spawnedItems = new List<GameObject>();
        private float _notifyTimer;

        private void Awake()
        {
            _shop = ShopService.Instance;
            _economy = EconomyService.Instance;
        }

        private void OnEnable() => Refresh();

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
            if (_shop == null || itemContainer == null) return;

            if (itemPrefab == null)
            {
                Debug.LogError("[ShopUI] Chưa gán itemPrefab (ShopItem.prefab).");
                return;
            }

            foreach (var item in _shop.GetItems())
            {
                var go = Instantiate(itemPrefab, itemContainer);

                var view = go.GetComponent<ShopItemView>();
                if (view == null)
                {
                    Debug.LogError("[ShopUI] Prefab item thiếu component ShopItemView.");
                    Destroy(go);
                    continue;
                }

                view.Bind(item, _shop, ShowNotify);
                _spawnedItems.Add(go);
            }
        }

        private void ClearItems()
        {
            foreach (var go in _spawnedItems)
            {
                if (go != null) Destroy(go);
            }
            _spawnedItems.Clear();
        }

        private void ShowNotify(string message)
        {
            if (txtNotify != null) txtNotify.text = message;
            _notifyTimer = notifyDuration;
        }
    }
}
