namespace _Game.Scripts.Meta
{
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using TMPro;
    using UnityEngine;
    using UnityEngine.UI;

    public sealed class ShopUI : MonoBehaviour
    {
        [Header("Container item (content của scroll view)")]
        [SerializeField] private Transform itemContainer;
        [SerializeField] private GameObject itemPrefab; // nếu null, tự dựng item đơn giản bằng code

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

            foreach (var item in _shop.GetItems())
            {
                var go = itemPrefab != null
                    ? Instantiate(itemPrefab, itemContainer)
                    : BuildDefaultItem();
                var data = new ShopItemView { item = item, shop = _shop, economy = _economy, ui = this };
                BindItem(go, data);
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

        private GameObject BuildDefaultItem()
        {
            var root = new GameObject("ShopItem", typeof(RectTransform));
            var rt = root.GetComponent<RectTransform>();
            rt.SetParent(itemContainer, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(300f, 90f);

            var bg = root.AddComponent<Image>();
            bg.color = new Color(0.15f, 0.15f, 0.18f, 1f);

            var layout = new GameObject("Layout", typeof(RectTransform));
            var lrt = layout.GetComponent<RectTransform>();
            lrt.SetParent(root.transform, false);
            lrt.anchorMin = Vector2.zero; lrt.anchorMax = Vector2.one;
            lrt.offsetMin = Vector2.zero; lrt.offsetMax = Vector2.zero;

            var title = new GameObject("Title", typeof(RectTransform));
            var trt = title.GetComponent<RectTransform>();
            trt.SetParent(lrt.transform, false);
            trt.anchorMin = new Vector2(0f, 0.5f); trt.anchorMax = new Vector2(1f, 0.5f);
            trt.offsetMin = new Vector2(10f, -20f); trt.offsetMax = new Vector2(-120f, 20f);
            var titleTxt = title.AddComponent<TextMeshProUGUI>();
            titleTxt.font = TMPro.TMP_Settings.defaultFontAsset;
            titleTxt.fontSize = 20f;
            titleTxt.alignment = TMPro.TextAlignmentOptions.Left;

            var cost = new GameObject("Cost", typeof(RectTransform));
            var crt = cost.GetComponent<RectTransform>();
            crt.SetParent(lrt.transform, false);
            crt.anchorMin = new Vector2(1f, 0.5f); crt.anchorMax = new Vector2(1f, 0.5f);
            crt.pivot = new Vector2(1f, 0.5f);
            crt.offsetMin = new Vector2(-110f, -20f); crt.offsetMax = new Vector2(-10f, 20f);
            var costTxt = cost.AddComponent<TextMeshProUGUI>();
            costTxt.font = TMPro.TMP_Settings.defaultFontAsset;
            costTxt.fontSize = 20f;
            costTxt.alignment = TMPro.TextAlignmentOptions.Right;

            var btn = root.AddComponent<Button>();
            var btnImg = root.GetComponent<Image>(); // dùng bg làm target graphic
            btn.targetGraphic = bg;

            root.AddComponent<ShopItemUI>().Bind(titleTxt, costTxt, btn);
            return root;
        }

        private void BindItem(GameObject go, ShopItemView data)
        {
            var comp = go.GetComponent<ShopItemUI>();
            if (comp == null) comp = go.AddComponent<ShopItemUI>();
            comp.Bind(data);
        }

        private void ShowNotify(string message)
        {
            if (txtNotify != null) txtNotify.text = message;
            _notifyTimer = notifyDuration;
        }

        private sealed class ShopItemUI : MonoBehaviour
        {
            private TMP_Text _title;
            private TMP_Text _cost;
            private Button _btn;
            private ShopItemView _data;

            public void Bind(TMP_Text title, TMP_Text cost, Button btn)
            {
                _title = title; _cost = cost; _btn = btn;
            }

            public void Bind(ShopItemView data)
            {
                _data = data;
                if (_title != null) _title.text = $"{data.item.displayName} {(data.item.boosterType != BoosterType.None ? "+" + data.item.boosterAmount + " " : data.item.coinAmount > 0 ? "+" + data.item.coinAmount + " coin" : "")}";
                if (_cost != null) _cost.text = data.item.coinCost + " coin";
                if (_btn != null)
                {
                    _btn.onClick.RemoveAllListeners();
                    _btn.onClick.AddListener(OnBuy);
                }
                RefreshInteractable();
            }

            private void RefreshInteractable()
            {
                if (_btn == null || _data.shop == null) return;
                _btn.interactable = _data.shop.CanBuy(_data.item.id);
            }

            private void OnBuy()
            {
                if (_data.shop == null) return;
                if (_data.shop.Buy(_data.item.id))
                {
                    _data.ui.ShowNotify($"Mua thành công {_data.item.displayName}");
                    RefreshInteractable();
                }
                else
                {
                    _data.ui.ShowNotify("Không đủ coin để mua");
                }
            }
        }
    }

    internal sealed class ShopItemView
    {
        public ShopItem item;
        public ShopService shop;
        public EconomyService economy;
        public ShopUI ui;
    }
}
