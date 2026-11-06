// MoreBoosterPopupUI.cs
// Popup mua booster — phiên bản scene-based (giống WinPopup/LosePopup).
// Đặt sẵn trong scene (con của Canvas_2 Overlays), kéo thả ref trong Inspector.
// Không còn dựng UI bằng code lúc runtime.

using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core;

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
        public Button btnClose;

        [Header("Sprites — dùng sẵn trên prefab (Inspector)")]
        public Sprite[] contentIcons = new Sprite[3]; // AddSlot, Clean, MagicBox

        private BoosterType _currentType;
        private bool _clicked;
        private static MoreBoosterPopupUI _instance;

        /// <summary>
        /// True khi đã bị instance khác thay thế và sắp bị Destroy.
        /// Instance bị Destroy vẫn có thể bị gọi Show() trong frame đó -> phải chặn,
        /// nếu không sẽ mở popup trên object đã hỏng.
        /// </summary>
        private bool _replaced;

        /// <summary>
        /// Số frame còn lại phải chặn input gameplay sau khi popup đóng.
        /// Lý do: uGUI xử lý onClick ở PointerUp, LeanTouch fire OnFingerTap CÙNG frame.
        /// Nếu Buy thành công -> Hide() -> cho dùng booster ngay, thì chính cú click
        /// đó "xuyên" xuống và kích hoạt luôn booster lên dây (dùng 2 lần 1 click).
        /// </summary>
        private static int _inputBlockFrames;

        /// <summary>True khi input gameplay đang bị chặn do popup vừa đóng.</summary>
        public static bool IsBlockingGameplayInput => _inputBlockFrames > 0;

        /// <summary>Instance hiện tại (để nơi khác gán hoặc kiểm tra nhanh).</summary>
        public static MoreBoosterPopupUI Instance => _instance;

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

            // Đăng ký instance: nếu ĐÃ có instance sống khác thì nhường (tránh 2 popup
            // tranh nhau). Dùng so sánh kiểu "còn sống" thay vì != null vì instance cũ
            // có thể đã bị Destroy nhưng biến static vẫn giữ tham chiếu "giả null".
            if (_instance == null || _instance == this)
            {
                _instance = this;
                _replaced = false;
            }
            else
            {
                Debug.LogWarning("[MoreBoosterPopupUI] Có nhiều hơn 1 popup trong scene — object này sẽ bị huỷ. Chỉ giữ 1 cái.", this);
                _replaced = true;
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            // Nhả static để lần Show sau không trỏ vào object đã chết.
            if (_instance == this)
                _instance = null;
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
            var target = ResolveInstance();
            if (target == null)
            {
                Debug.LogError("[MoreBoosterPopupUI] Không tìm thấy popup trong scene. " +
                               "Hãy kéo MoreBoosterPopupUI (con của Canvas_2 Overlays) vào ô 'moreBoosterPopup' " +
                               "trên GameUIController, hoặc đặt nó trong scene.");
                return;
            }
            target.ShowPopup(type);
        }

        /// <summary>
        /// Mở popup trên chính instance này (dùng khi đã có ref kéo thả trong Inspector,
        /// không cần qua singleton/Find).
        /// </summary>
        public void ShowFor(BoosterType type)
        {
            // Ref kéo thả có thể trỏ vào object đã bị huỷ.
            if (this == null) return;

            // Đảm bảo đây là instance được Show() dùng tới.
            if (_instance == null || _instance == this)
                _instance = this;

            ShowPopup(type);
        }

        /// <summary>
        /// Lấy instance đang sống. Tự dọn tham chiếu đã bị Destroy và tự tìm lại
        /// trong scene nếu cần.
        /// </summary>
        private static MoreBoosterPopupUI ResolveInstance()
        {
            // Lưu ý: UnityEngine.Object có "fake null" — object đã Destroy thì == null
            // trả true, nên phép so sánh này tự loại instance đã chết.
            if (_instance != null)
                return _instance;

            // Ưu tiên ref kéo thả trong Inspector (không phụ thuộc Find).
            var wired = GameUIController.Instance != null
                ? GameUIController.Instance.moreBoosterPopup
                : null;
            if (wired != null)
            {
                _instance = wired;
                return _instance;
            }

            _instance = FindObjectOfType<MoreBoosterPopupUI>(true);
            return _instance;
        }

        /// <summary>Static accessor to close the popup.</summary>
        public static void CloseCurrent()
        {
            var target = ResolveInstance();
            if (target != null)
            {
                target.Hide();
            }
        }

        /// <summary>Internal instance method to show popup.</summary>
        private void ShowPopup(BoosterType type)
        {
            // Object đã bị thay thế/huỷ thì không mở nữa.
            if (_replaced || this == null) return;

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
            if (btnClose == null) Debug.LogWarning("[MoreBoosterPopupUI] Chưa kéo btnClose.", this);
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
            // index 0 = AddSlot, 1 = Clean, 2 = MagicBox — clamp để không lỗi index
            int i = Mathf.Clamp((int)_currentType, 0, Titles.Length - 1);

            // Card background: dùng sẵn sprite trên prefab (cardRect đã có Image component)

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
            int cost = Configs.GetBoosterCost(_currentType);

            // Button sprites: dùng sẵn trên prefab (btnCoin / btnClose đã có Image component)

            // Button listeners
            if (btnCoin != null)
            {
                btnCoin.interactable = true;
                btnCoin.onClick.RemoveAllListeners();
                btnCoin.onClick.AddListener(BuyWithCoin);
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

            int cost = Configs.GetBoosterCost(_currentType);
            if (!CoinManager.Instance.ConsumeCoins(cost))
            {
                Debug.LogWarning($"[MoreBoosterPopupUI] Not enough coins to buy {_currentType}. Required {cost}, have {CoinManager.Instance.GetBalance()}");
                // TODO: có thể thêm shake animation hoặc toast "Not enough coins"
                _clicked = false;
                return;
            }

            AudioService.Instance?.PlayBoosterBuy();
            Debug.Log($"[MoreBoosterPopupUI] Bought {_currentType} with coins. Remaining: {CoinManager.Instance.GetBalance()}");
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

            EconomyService.Instance.AddBooster(_currentType, 1, "buy", "more_booster_popup");
            EconomyService.Instance.SpendBooster(_currentType, 1, "use", "use_booster");

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
