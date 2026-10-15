// MoreBoosterPopupUI.cs
// Popup mua booster — phiên bản scene-based (giống WinPopup/LosePopup).
// Đặt sẵn trong scene (con của Canvas_2 Overlays), kéo thả ref trong Inspector.
// Không còn dựng UI bằng code lúc runtime.

using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    public class MoreBoosterPopupUI : MonoBehaviour
    {
        [Header("UI References (kéo thả trong Inspector)")]
        public CanvasGroup canvasGroup;
        public RectTransform cardRect;
        public TextMeshProUGUI titleText;
        public Image contentImage;
        public TextMeshProUGUI descText;
        public Button btnCoin;
        public Button btnAds;
        public Button btnClose;
        public Image coinBtnIcon; // icon coin trên nút BUY
        public Image adsBtnIcon;  // icon ads trên nút FREE

        [Header("Sprites — kéo từ BoosterUI_Picked/Resources")]
        public Sprite cardBg;
        public Sprite buttonBlue;  // nền nút Coin
        public Sprite buttonGreen; // nền nút Ads
        public Sprite buttonClose;
        public Sprite coinIcon;
        public Sprite adsIcon;
        public Sprite[] contentIcons = new Sprite[3]; // AddSlot, Clean, MagicBox

        private BoosterType _currentType;
        private bool _clicked;
        private static MoreBoosterPopupUI _instance;

        /// <summary>
        /// Số frame còn lại phải chặn input gameplay sau khi popup đóng.
        /// Lý do: uGUI xử lý onClick ở PointerUp, LeanTouch fire OnFingerTap CÙNG frame.
        /// Nếu Buy thành công -> Hide() -> cho dùng booster ngay, thì chính cú click
        /// đó "xuyên" xuống và kích hoạt luôn booster lên dây (dùng 2 lần 1 click).
        /// </summary>
        private static int _inputBlockFrames;

        /// <summary>True khi input gameplay đang bị chặn do popup vừa đóng.</summary>
        public static bool IsBlockingGameplayInput => _inputBlockFrames > 0;

        private static readonly string[] Titles = { "ADD SLOT", "CLEAN", "MAGIC SPOOL" };
        private static readonly string[] Descs =
        {
            "Get an extra slot",
            "Clean all wool on slots",
            "Auto complete the first spool"
        };

        private void Awake()
        {
            ValidateRefs();
            // Static có thể giữ giá trị qua lần reload scene (domain reload tắt trong Editor
            // Play mode) -> reset để không khoá input oan ở đầu level.
            _inputBlockFrames = 0;
            // Popup luôn active trong hierarchy, ẩn/hiện bằng alpha
            SetVisible(false, true);
            // Ensure singleton instance
            if (_instance == null)
                _instance = this;
            else if (_instance != this)
                Destroy(gameObject);
        }

        private void Update()
        {
            // Đếm ngược khoá input sau khi popup đóng (xem _inputBlockFrames).
            if (_inputBlockFrames > 0)
                _inputBlockFrames--;
        }

        /// <summary>Static accessor to show popup.</summary>
        public static void Show(BoosterType type)
        {
            if (_instance == null)
            {
                // Try to find existing instance in scene
                _instance = FindObjectOfType<MoreBoosterPopupUI>();
                if (_instance == null)
                {
                    Debug.LogError("[MoreBoosterPopupUI] No instance found in scene. Please place MoreBoosterPopupUI under Canvas_2 Overlays.");
                    return;
                }
            }
            _instance.ShowPopup(type);
        }

        /// <summary>Static accessor to close the popup.</summary>
        public static void CloseCurrent()
        {
            if (_instance != null)
            {
                _instance.Hide();
            }
        }

        /// <summary>Internal instance method to show popup.</summary>
        private void ShowPopup(BoosterType type)
        {
            // Huỷ lượt "dùng booster sau khi đóng" còn treo (nếu user mở lại popup
            // trước khi coroutine cũ chạy xong) để không dùng booster ngoài ý muốn.
            StopAllCoroutines();
            _inputBlockFrames = 0;

            _currentType = type;
            _clicked = false;

            var canvas = GetComponentInParent<Canvas>();
            Debug.Log($"[MoreBoosterPopupUI.Show] type={type} | canvasGroup={canvasGroup != null} cardRect={cardRect != null} | activeSelf={gameObject.activeSelf} | canvas={canvas?.name}");

            if (canvas != null && canvas.transform.localScale == Vector3.zero)
            {
                Debug.LogWarning("[MoreBoosterPopupUI.Show] Canvas scale 0 -> reset về 1.", this);
                canvas.transform.localScale = Vector3.one;
            }

            RefreshUI();

            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (canvasGroup != null) SetVisible(true, false);
            if (cardRect != null)
            {
                cardRect.DOKill();
                cardRect.localScale = Vector3.one * 0.85f;
                cardRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            }
        }

        private void ValidateRefs()
        {
            if (canvasGroup == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo canvasGroup.", this);
            if (cardRect == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo cardRect.", this);
            if (titleText == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo titleText.", this);
            if (contentImage == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo contentImage.", this);
            if (descText == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo descText.", this);
            if (btnCoin == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo btnCoin.", this);
            if (btnAds == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo btnAds.", this);
            if (btnClose == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo btnClose.", this);
            if (coinBtnIcon == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo coinBtnIcon.", this);
            if (adsBtnIcon == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo adsBtnIcon.", this);
        }



        public void Hide()
        {
            Hide(false);
        }

        /// <summary>
        /// Ẩn popup. <paramref name="blockGameplayInput"/> = true khi đóng do mua thành công:
        /// lúc đó khoá input gameplay vài frame để cú click trên nút BUY không
        /// xuyên xuống dây/part (LeanTouch fire tap cùng frame với uGUI PointerUp).
        /// </summary>
        private void Hide(bool blockGameplayInput)
        {
            SetVisible(false, false);
            if (blockGameplayInput)
            {
                // Khoá đủ dài để trải qua cả frame PointerUp hiện tại và frame tap của LeanTouch.
                _inputBlockFrames = 2;
            }
        }

        private void RefreshUI()
        {
            int i = (int)_currentType;

            // Card background
            if (cardRect != null)
            {
                var cardImg = cardRect.GetComponent<Image>();
                if (cardImg != null && cardBg != null)
                {
                    cardImg.sprite = cardBg;
                    cardImg.color = Color.white;
                }
            }

            // Title, Desc
            if (titleText != null) titleText.text = Titles[i];
            if (descText != null) descText.text = Descs[i];

            // Content icon
            if (contentImage != null && contentIcons != null && i < contentIcons.Length && contentIcons[i] != null)
            {
                contentImage.sprite = contentIcons[i];
                contentImage.color = Color.white;
                contentImage.preserveAspect = true;
            }
            else if (contentImage != null)
            {
                contentImage.sprite = null;
                contentImage.color = new Color(0.7f, 0.7f, 0.7f, 1f);
            }

            // Cost
            int cost = Configs.LIST_BOOSTER_COST[i];

            // Button sprites
            if (btnCoin != null)
            {
                var img = btnCoin.GetComponent<Image>();
                if (img != null && buttonBlue != null) { img.sprite = buttonBlue; img.color = Color.white; }
            }
            if (btnAds != null)
            {
                var img = btnAds.GetComponent<Image>();
                if (img != null && buttonGreen != null) { img.sprite = buttonGreen; img.color = Color.white; }
            }
            if (btnClose != null)
            {
                var img = btnClose.GetComponent<Image>();
                if (img != null && buttonClose != null) { img.sprite = buttonClose; img.color = Color.white; }
            }

            // Icons on buttons
            if (coinBtnIcon != null && coinIcon != null) { coinBtnIcon.sprite = coinIcon; coinBtnIcon.color = Color.white; coinBtnIcon.preserveAspect = true; }
            if (adsBtnIcon != null && adsIcon != null) { adsBtnIcon.sprite = adsIcon; adsBtnIcon.color = Color.white; adsBtnIcon.preserveAspect = true; }

            // Button listeners
            if (btnCoin != null)
            {
                btnCoin.interactable = true;
                btnCoin.onClick.RemoveAllListeners();
                btnCoin.onClick.AddListener(BuyWithCoin);
            }
            if (btnAds != null)
            {
                btnAds.interactable = true;
                btnAds.onClick.RemoveAllListeners();
                btnAds.onClick.AddListener(BuyWithAds);
            }
            if (btnClose != null)
            {
                btnClose.interactable = true;
                btnClose.onClick.RemoveAllListeners();
                btnClose.onClick.AddListener(Hide);
            }
        }

        private void BuyWithCoin()
        {
            if (_clicked) return;
            _clicked = true;

            int cost = Configs.LIST_BOOSTER_COST[(int)_currentType];
            if (!CoinManager.Instance.ConsumeCoins(cost))
            {
                Debug.LogWarning($"[MoreBoosterPopupUI] Not enough coins to buy {_currentType}. Required {cost}, have {CoinManager.Instance.GetBalance()}");
                // TODO: có thể thêm shake animation hoặc toast "Not enough coins"
                _clicked = false;
                return;
            }

            Debug.Log($"[MoreBoosterPopupUI] Bought {_currentType} with coins. Remaining: {CoinManager.Instance.GetBalance()}");
            GrantAndUse();
        }

        private void BuyWithAds()
        {
            if (_clicked) return;
            _clicked = true;
            Debug.Log($"[MoreBoosterPopupUI] Buy {_currentType} with ads — stub: coi như success.");
            // TODO(W7): thay bằng AdsService.AuthorizeReward rồi mới grant.
            GrantAndUse();
        }

        private void GrantAndUse()
        {
            // Đóng popup TRƯỚC và khoá input gameplay trong lúc đóng: nếu không,
            // cú click vừa rồi trên nút BUY sẽ xuyên xuống và dùng booster lên dây
            // ngay lập tức (double-action trên 1 click).
            Hide(true);

            if (BoosterManager.Instance == null)
            {
                Debug.LogWarning("[MoreBoosterPopupUI] BoosterManager missing.");
                return;
            }
            if (!BoosterManager.Instance.CanUseBooster(_currentType)) return;

            GameData.Instance.AddCount(_currentType, 1, "buy", "more_booster_popup");
            GameData.Instance.AddCount(_currentType, -1, "use", "use_booster");

            // Trì hoãn việc dùng booster tới khi popup đã đóng hẳn: đợi tween fade
            // (0.25s) + thêm 1 nhịp để chắc chắn không nhận event click của chính
            // thao tác mua.
            StartCoroutine(UseBoosterAfterClose());
        }

        private IEnumerator UseBoosterAfterClose()
        {
            // SetVisible(false,...) đã dựng tween fade+scale đóng popup.
            // KHÔNG tạo tween mới ở đây (sẽ DOKill tween đang chạy và ghi đè alpha),
            // chỉ chờ hết thời lượng tween đóng rồi thêm vài frame đệm.
            yield return new WaitForSeconds(0.3f);

            // Thêm 2 frame: qua frame PointerUp hiện tại + frame tap của LeanTouch.
            yield return null;
            yield return null;

            _clicked = false;
            if (BoosterManager.Instance != null)
                BoosterManager.Instance.UseBooster(_currentType);
        }

        private void SetVisible(bool visible, bool instant)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                // DOKill trên CanvasGroup chỉ kill tween target = component,
                // không kill tween scale (target = transform) -> kill tay.
                canvasGroup.transform.DOKill();
                if (visible)
                {
                    if (instant)
                    {
                        canvasGroup.alpha = 1f;
                        canvasGroup.transform.localScale = Vector3.one;
                    }
                    else
                    {
                        // Set scale = 1 NGAY rồi mới tween để popup không bị tàng hình
                        // khi tween scale không chạy.
                        canvasGroup.alpha = 0f;
                        canvasGroup.transform.localScale = Vector3.one;
                        canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutBack);
                        canvasGroup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
                    }
                }
                else
                {
                    // Ẩn bằng alpha, không set scale 0 để lần show sau luôn hiện được.
                    canvasGroup.alpha = 0f;
                }
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }

#if UNITY_EDITOR
        // Context menu để test nhanh trong Editor
        [ContextMenu("Test Show AddSlot")]
        private void CtxShowAddSlot() => ShowPopup(BoosterType.AddSlot);

        [ContextMenu("Test Show Clean")]
        private void CtxShowClean() => ShowPopup(BoosterType.Clean);

        [ContextMenu("Test Show MagicBox")]
        private void CtxShowMagic() => ShowPopup(BoosterType.MagicBox);

        [ContextMenu("Test Hide")]
        private void CtxHide() => Hide();
#endif
    }
}
