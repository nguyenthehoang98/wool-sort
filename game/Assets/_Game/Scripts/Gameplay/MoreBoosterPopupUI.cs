// MoreBoosterPopupUI.cs
// Popup mua booster dựng bằng code, tương đương MoreBoosterPopup.prefab gốc
// (title ADD SLOT / CLEAN / MAGIC SPOOL, content sprite, desc, nút coin/ads/close).
// Không phụ thuộc KitButton/FrameManager/I2 — dùng Button uGUI thuần để
// tránh lỗi copy prefab. Luồng mua hiện là stub cho bản core:
// cộng +1 rồi trừ -1 và gọi BoosterManager.UseBooster (giữ inventory
// nhất quán, không cần Coin/Ads SDK). Khi có EconomyService/AdsService,
// thay 2 hàm BuyWithCoin/BuyWithAds bằng adapter thật.

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    public sealed class MoreBoosterPopupUI : MonoBehaviour
    {
        private static MoreBoosterPopupUI _instance;

        [Header("Sprites — kéo từ BoosterUI_Picked")]
        public Sprite cardBg;
        public Sprite buttonBlue;
        public Sprite buttonGreen;
        public Sprite buttonClose;
        public Sprite coinIcon;
        public Sprite adsIcon;
        public Sprite[] contentIcons = new Sprite[3];

        private BoosterType _current;
        private bool _clicked;
        private GameObject _root;
        private TextMeshProUGUI _title;
        private Image _content;
        private TextMeshProUGUI _desc;
        private TextMeshProUGUI _cost;
        private Image _cardImg;
        private Image _coinBtnImg;
        public TextMeshProUGUI coinBarText; // UI showing total coins
        private Image _adsBtnImg;
        private Image _closeBtnImg;

        private static readonly string[] Titles = { "ADD SLOT", "CLEAN", "MAGIC SPOOL" };
        private static readonly string[] Descs =
        {
            "Get an extra slot",
            "Clean all wool on slots",
            "Auto complete the first spool"
        };

        public static void Show(BoosterType type)
        {
            if (_instance == null)
            {
                var host = new GameObject("MoreBoosterPopupUI");
                _instance = host.AddComponent<MoreBoosterPopupUI>();
            }
            _instance.Open(type);
        }

        public static bool IsShowing => _instance != null && _instance._root != null && _instance._root.activeSelf;
        public static BoosterType CurrentType => _instance != null ? _instance._current : BoosterType.AddSlot;

        public static void CloseCurrent()
        {
            if (_instance != null) _instance.Hide();
        }

        // Test nhanh từ Inspector (chuột phải component) hoặc Tools menu.
        [ContextMenu("Test Show AddSlot")]
        private void CtxShowAddSlot() => Open(BoosterType.AddSlot);
        [ContextMenu("Test Show Clean")]
        private void CtxShowClean() => Open(BoosterType.Clean);
        [ContextMenu("Test Show MagicBox")]
        private void CtxShowMagic() => Open(BoosterType.MagicBox);

        private void Update()
        {
            if (_root != null && _root.activeSelf && Input.GetKeyDown(KeyCode.Escape))
                Hide();
        }

        private void Open(BoosterType type)
        {
            _current = type;
            _clicked = false;
            ResolveSprites();
            if (_root == null) Build();
            Refresh();
            _root.SetActive(true);
            _root.transform.SetAsLastSibling();
        }

        private void OnEnable()
        {
            if (_instance == null) _instance = this;
        }

        public void Hide()
        {
            if (_root != null) _root.SetActive(false);
            _clicked = false;
        }

        private void ResolveSprites()
        {
            cardBg = cardBg ? cardBg : Resources.Load<Sprite>("BoosterUI/Frame_Popup_2");
            buttonBlue = buttonBlue ? buttonBlue : Resources.Load<Sprite>("BoosterUI/Button_Blue");
            buttonGreen = buttonGreen ? buttonGreen : Resources.Load<Sprite>("BoosterUI/Button_Green");
            buttonClose = buttonClose ? buttonClose : Resources.Load<Sprite>("BoosterUI/Button_Close");
            coinIcon = coinIcon ? coinIcon : Resources.Load<Sprite>("BoosterUI/Icon_Coin_S");
            adsIcon = adsIcon ? adsIcon : Resources.Load<Sprite>("BoosterUI/Icon_ADS");
            if (contentIcons == null || contentIcons.Length < 3)
                contentIcons = new Sprite[3];
            contentIcons[0] = contentIcons[0] ? contentIcons[0] : Resources.Load<Sprite>("BoosterUI/Icon_Slot_3");
            contentIcons[1] = contentIcons[1] ? contentIcons[1] : Resources.Load<Sprite>("BoosterUI/Icon_Broom_2");
            contentIcons[2] = contentIcons[2] ? contentIcons[2] : Resources.Load<Sprite>("BoosterUI/Icon_Spool_2");
        }

        private Transform OverlayParent()
        {
            var overlays = GameObject.Find("Canvas_2 (Overlays)");
            if (overlays != null) return overlays.transform;
            var c = GameObject.Find("Canvas_1 (Base)");
            if (c != null) return c.transform;
            var any = FindObjectOfType<Canvas>();
            if (any != null) return any.transform;
            return transform;
        }

        private void Build()
        {
            var parent = OverlayParent();
            _root = new GameObject("Root", typeof(RectTransform), typeof(CanvasGroup));
            _root.layer = 5;
            var rootRt = (RectTransform)_root.transform;
            rootRt.SetParent(parent, false);
            Stretch(rootRt);

            var shade = new GameObject("Shade", typeof(RectTransform));
            var shadeRt = SetupChild(shade, _root.transform);
            Stretch(shadeRt);
            var shadeImg = shade.AddComponent<Image>();
            shadeImg.color = new Color(0f, 0f, 0f, 0.65f);
            var shadeBtn = shade.AddComponent<Button>();
            shadeBtn.targetGraphic = shadeImg;
            shadeBtn.onClick.AddListener(Hide);

            var card = new GameObject("Card", typeof(RectTransform));
            var cardRt = SetupChild(card, _root.transform);
            cardRt.anchorMin = cardRt.anchorMax = new Vector2(0.5f, 0.5f);
            cardRt.sizeDelta = new Vector2(760f, 980f);
            cardRt.anchoredPosition = Vector2.zero;
            var cardImg = card.AddComponent<Image>();
            _cardImg = cardImg;
            cardImg.sprite = cardBg;
            cardImg.color = cardBg == null ? new Color(0.96f, 0.93f, 0.87f, 1f) : Color.white;

            // Coin bar showing total balance
            coinBarText = MakeText(card.transform, "CoinBar", 28, Color.black, TextAlignmentOptions.Right,
                new Vector2(0.45f, 0.85f), new Vector2(200f, 50f));
            _content = MakeImage(card.transform, "Content", new Vector2(0f, 0.52f), new Vector2(300f, 260f));
            _desc = MakeText(card.transform, "Desc", 30, new Color(0.2f, 0.2f, 0.2f), TextAlignmentOptions.Center,
                new Vector2(0f, 0.28f), new Vector2(640f, 120f));
            _cost = MakeText(card.transform, "Cost", 40, Color.black, TextAlignmentOptions.Center,
                new Vector2(-140f, -0.32f), new Vector2(200f, 80f));

            _coinBtnImg = MakeButton(card.transform, "BtnCoin", buttonBlue, new Vector2(-140f, -0.46f),
                new Vector2(340f, 130f), coinIcon, "BUY", BuyWithCoin);
            _adsBtnImg = MakeButton(card.transform, "BtnAds", buttonGreen, new Vector2(140f, -0.46f),
                new Vector2(340f, 130f), adsIcon, "FREE", BuyWithAds);
            _closeBtnImg = MakeButton(card.transform, "BtnClose", buttonClose, new Vector2(0.42f, 0.44f),
                new Vector2(110f, 110f), null, "X", Hide, anchorMode: true);
        }

        private void Refresh()
        {
            int i = (int)_current;
            ApplySprite(_cardImg, cardBg, new Color(0.96f, 0.93f, 0.87f, 1f));
            ApplySprite(_coinBtnImg, buttonBlue, new Color(0.25f, 0.55f, 0.9f, 1f));
            ApplySprite(_adsBtnImg, buttonGreen, new Color(0.3f, 0.65f, 0.35f, 1f));
            ApplySprite(_closeBtnImg, buttonClose, new Color(0.85f, 0.3f, 0.3f, 1f));
            _title.text = Titles[i];
            _desc.text = Descs[i];
            // Update coin bar with current balance
            if (coinBarText != null)
                coinBarText.text = "Coins: " + CoinManager.Instance.GetBalance();
            if (contentIcons != null && i < contentIcons.Length && contentIcons[i] != null)
            {
                _content.sprite = contentIcons[i];
                _content.color = Color.white;
            }
            else
            {
                _content.sprite = null;
                _content.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            }
        }

        private void BuyWithCoin()
        {
            if (_clicked) return;
            _clicked = true;
            int cost = Configs.LIST_BOOSTER_COST[(int)_current];
            // Attempt to spend coins before granting booster
            if (!CoinManager.Instance.ConsumeCoins(cost))
            {
                Debug.LogWarning($"[MoreBoosterPopupUI] Not enough coins to buy {_current}. Required {cost}, have {CoinManager.Instance.GetBalance()}");
                return;
            }
            Debug.Log($"[MoreBoosterPopupUI] Bought {_current} with coins. Remaining: {CoinManager.Instance.GetBalance()}");
            // TODO(W7): thay bằng EconomyService (check Coin, ConsumeCoin) trước khi grant.
            GrantAndUse();
        }

        private void BuyWithAds()
        {
            if (_clicked) return;
            _clicked = true;
            Debug.Log($"[MoreBoosterPopupUI] Buy {_current} with ads — stub core: coi như success.");
            // TODO(W7): thay bằng AdsService.AuthorizeReward rồi mới grant.
            StartCoroutine(ResetClicked());
            GrantAndUse();
        }

        private void GrantAndUse()
        {
            Hide();
            // Giữ inventory nhất quán: +1 (mua) rồi BoosterItem sẽ -1 khi dùng?
            // Ở đây popup dùng thay cho BoosterItem nên grant rồi dùng ngay.
            if (BoosterManager.Instance == null)
            {
                Debug.LogWarning("[MoreBoosterPopupUI] BoosterManager missing.");
                return;
            }
            if (!BoosterManager.Instance.CanUseBooster(_current)) return;
            GameData.Instance.AddCount(_current, 1, "buy", "more_booster_popup");
            GameData.Instance.AddCount(_current, -1, "use", "use_booster");
            BoosterManager.Instance.UseBooster(_current);
        }

        private IEnumerator ResetClicked()
        {
            yield return new WaitForSeconds(Configs.TIME_RESET_CLICKED);
            _clicked = false;
        }

        private static RectTransform SetupChild(GameObject go, Transform parent)
        {
            go.layer = 5;
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            return rt;
        }

        private static void Stretch(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        private static void ApplySprite(Image img, Sprite sprite, Color fallback)
        {
            if (img == null) return;
            img.sprite = sprite;
            img.color = sprite == null ? fallback : Color.white;
        }

        private static Image MakeImage(Transform parent, string name, Vector2 anchoredRatio, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = SetupChild(go, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = new Vector2(anchoredRatio.x * 380f, anchoredRatio.y * 980f - 490f);
            var img = go.AddComponent<Image>();
            img.preserveAspect = true;
            return img;
        }

        private static TextMeshProUGUI MakeText(Transform parent, string name, float size, Color color,
            TextAlignmentOptions align, Vector2 anchoredRatio, Vector2 sizeDelta)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = SetupChild(go, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = new Vector2(anchoredRatio.x * 760f * 0.5f * 2f * 0.5f, anchoredRatio.y * 980f - 490f);
            // Đơn giản hóa: map tỉ lệ card
            var t = go.AddComponent<TextMeshProUGUI>();
            t.font = TMP_Settings.defaultFontAsset;
            t.fontSize = size;
            t.color = color;
            t.alignment = align;
            t.enableWordWrapping = true;
            return t;
        }

        private static Image MakeButton(Transform parent, string name, Sprite bg, Vector2 anchoredRatio,
            Vector2 size, Sprite icon, string label, UnityEngine.Events.UnityAction onClick, bool anchorMode = false)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = SetupChild(go, parent);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = size;
            rt.anchoredPosition = anchorMode
                ? new Vector2(anchoredRatio.x * 760f, anchoredRatio.y * 980f)
                : new Vector2(anchoredRatio.x, anchoredRatio.y * 980f * 0.5f);
            var img = go.AddComponent<Image>();
            img.sprite = bg;
            img.color = bg == null ? new Color(0.25f, 0.55f, 0.9f, 1f) : Color.white;
            var btn = go.AddComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(onClick);

            if (icon != null)
            {
                var ico = new GameObject("Icon", typeof(RectTransform));
                var icoRt = SetupChild(ico, go.transform);
                icoRt.anchorMin = icoRt.anchorMax = new Vector2(0.5f, 0.5f);
                icoRt.sizeDelta = new Vector2(72f, 72f);
                icoRt.anchoredPosition = new Vector2(-size.x * 0.28f, 0f);
                var icoImg = ico.AddComponent<Image>();
                icoImg.sprite = icon;
                icoImg.preserveAspect = true;
            }

            var lab = new GameObject("Label", typeof(RectTransform));
            var labRt = SetupChild(lab, go.transform);
            Stretch(labRt);
            var txt = lab.AddComponent<TextMeshProUGUI>();
            txt.font = TMP_Settings.defaultFontAsset;
            txt.fontSize = 34f;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            txt.text = label;
            return img;
        }
    }
}
