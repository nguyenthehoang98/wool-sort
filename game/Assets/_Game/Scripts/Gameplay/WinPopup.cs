using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts;
using _Game.Scripts.UI;

namespace _Game.Scripts.Gameplay
{
    // WinPopup thật trong scene (con của Canvas_2 Overlays).
    // Kéo thả ref trực tiếp trong Inspector — không Find trong code.
    // Thiết kế tham khảo bản gốc: Shade + Card + ImgTitle (Text_Congratulations)
    // + khung tròn (Frame_Picture_GP_3 + Pattern_01) + message + 2 nút
    // BtnCoin (Button_Green + Icon_Coin_S) / BtnAds (Button_Blue + Icon_ADS_1).
    public class WinPopup : MonoBehaviour
    {
        [Header("UI References (kéo thả trong Inspector)")]
        public CanvasGroup canvasGroup;
        public RectTransform cardRect;
        public TextMeshProUGUI messageText;
        public Button btnCoin;
        public Button btnClose;

        private bool _isNextAnimating;

        private void Awake()
        {
            ValidateRefs();
            // Popup luôn active trong hierarchy, ẩn/hiện bằng alpha để tránh
            // mọi vấn đề với SetActive (không hiện dù đã gọi SetActive(true)).
            SetVisible(false, true);
        }

        private void OnEnable()
        {
            // Popup có thể bị SetActive(false) khi bắt đầu level (xem
            // GameManager.PlayStartLevelSequence) rồi SetActive(true) lúc show.
            // Awake chỉ chạy 1 lần nên nếu lần show trước để lại scale 0 thì
            // popup sẽ tàng hình. Chuẩn hoá lại mỗi lần bật.
            if (canvasGroup != null)
                canvasGroup.transform.localScale = Vector3.one;
        }

        private void ValidateRefs()
        {
            if (canvasGroup == null) Debug.LogWarning("[WinPopup] Chưa kéo canvasGroup.", this);
            if (cardRect == null) Debug.LogWarning("[WinPopup] Chưa kéo cardRect.", this);
            if (messageText == null) Debug.LogWarning("[WinPopup] Chưa kéo messageText.", this);
            if (btnCoin == null) Debug.LogWarning("[WinPopup] Chưa kéo btnCoin.", this);
            if (btnClose == null) Debug.LogWarning("[WinPopup] Chưa kéo btnClose.", this);
        }

        public void Show(int coinReward = 60)
        {
            _isNextAnimating = false;
            var canvas = GetComponentInParent<Canvas>();
            Debug.Log("[WinPopup.Show] refs: canvasGroup=" + (canvasGroup != null)
                + " cardRect=" + (cardRect != null)
                + " messageText=" + (messageText != null)
                + " btnCoin=" + (btnCoin != null)
                + " btnClose=" + (btnClose != null)
    
                + " | activeSelf=" + gameObject.activeSelf
                + " | canvas=" + (canvas != null ? canvas.name + " scale=" + canvas.transform.localScale : "NULL"), this);
            if (canvas != null && canvas.transform.localScale == Vector3.zero)
            {
                Debug.LogWarning("[WinPopup.Show] Canvas scale 0 -> reset về 1.", this);
                canvas.transform.localScale = Vector3.one;
            }

            if (messageText != null) messageText.text = "Great job!";
            CoinManager.Instance?.AddCoins(coinReward);

            if (btnCoin != null)
            {
                btnCoin.interactable = true;
                btnCoin.onClick.RemoveAllListeners();
                btnCoin.onClick.AddListener(OnClickNext);
            }
            if (btnClose != null)
            {
                btnClose.interactable = true;
                btnClose.onClick.RemoveAllListeners();
                btnClose.onClick.AddListener(Hide);
            }
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                SetVisible(true, false);
            }
            if (cardRect != null)
            {
                cardRect.DOKill();
                cardRect.localScale = Vector3.one * 0.85f;
                cardRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            }
            Debug.Log("[WinPopup.Show] done. activeSelf=" + gameObject.activeSelf
                + " alpha=" + (canvasGroup != null ? canvasGroup.alpha.ToString() : "NULL")
                + " childCount=" + transform.childCount, this);
        }

        // Nút Next: sinh coin bay từ nút tới coin bar, nảy bar + burst, rồi sang level mới.
        // Nếu thiếu hiệu ứng thì đi tiếp ngay (không chặn flow).
        private void OnClickNext()
        {
            if (_isNextAnimating) return;

            var target = CoinHUD.Instance != null ? CoinHUD.Instance.IconTransform.gameObject : null;
            var splash = SplashItem.Instance;

            if (splash != null && target != null && btnCoin != null)
            {
                _isNextAnimating = true;
                splash.SplashCoin(5, btnCoin.gameObject, target, 0.6f, null,
                    () => { if (CoinHUD.Instance != null) CoinHUD.Instance.ShowVFX(); },
                    () =>
                    {
                        _isNextAnimating = false;
                        GameUIController.Instance.OnWinNext();
                    });
            }
            else
            {
                GameUIController.Instance.OnWinNext();
            }
        }

        public void Hide()
        {
            SetVisible(false, false);
        }



        private void SetVisible(bool visible, bool instant)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                // Kill luôn tween scale còn sót (DOKill trên CanvasGroup chỉ kill
                // tween target = component, không kill tween target = transform).
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
                        // BUG CŨ: chỉ DOFade, thiếu DOScale -> card kẹt ở scale 0
                        // (do trạng thái ẩn set localScale=zero) nên popup vô hình.
                        canvasGroup.alpha = 0f;
                        canvasGroup.transform.localScale = Vector3.one;
                        canvasGroup.DOFade(1f, 0.25f).SetEase(Ease.OutBack);
                        canvasGroup.transform.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
                    }
                }
                else
                {
                    // Ẩn bằng alpha, KHÔNG set scale 0: giữ scale nguyên để lần show
                    // sau không phụ thuộc vào việc tween scale có chạy hay không.
                    canvasGroup.alpha = 0f;
                }
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}
