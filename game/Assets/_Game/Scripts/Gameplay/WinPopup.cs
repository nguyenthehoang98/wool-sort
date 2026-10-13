using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        public Button btnAds;
        public TextMeshProUGUI coinText;




        private void Awake()
        {
            ValidateRefs();
            // Popup luôn active trong hierarchy, ẩn/hiện bằng alpha để tránh
            // mọi vấn đề với SetActive (không hiện dù đã gọi SetActive(true)).
            SetVisible(false, true);
        }

        private void ValidateRefs()
        {
            if (canvasGroup == null) Debug.LogWarning("[WinPopup] Chưa kéo canvasGroup.", this);
            if (cardRect == null) Debug.LogWarning("[WinPopup] Chưa kéo cardRect.", this);
            if (messageText == null) Debug.LogWarning("[WinPopup] Chưa kéo messageText.", this);
            if (btnCoin == null) Debug.LogWarning("[WinPopup] Chưa kéo btnCoin.", this);
            if (btnAds == null) Debug.LogWarning("[WinPopup] Chưa kéo btnAds.", this);
            if (coinText == null) Debug.LogWarning("[WinPopup] Chưa kéo coinText.", this);

        }

        public void Show(int coinReward = 60)
        {
            var canvas = GetComponentInParent<Canvas>();
            Debug.Log("[WinPopup.Show] refs: canvasGroup=" + (canvasGroup != null)
                + " cardRect=" + (cardRect != null)
                + " messageText=" + (messageText != null)
                + " btnCoin=" + (btnCoin != null)
                + " btnAds=" + (btnAds != null)
                + " coinText=" + (coinText != null)
    
                + " | activeSelf=" + gameObject.activeSelf
                + " | canvas=" + (canvas != null ? canvas.name + " scale=" + canvas.transform.localScale : "NULL"), this);
            if (canvas != null && canvas.transform.localScale == Vector3.zero)
            {
                Debug.LogWarning("[WinPopup.Show] Canvas scale 0 -> reset về 1.", this);
                canvas.transform.localScale = Vector3.one;
            }

            if (messageText != null) messageText.text = "Great job!";
            if (coinText != null) coinText.text = "x" + coinReward;

            if (btnCoin != null)
            {
                btnCoin.interactable = true;
                btnCoin.onClick.RemoveAllListeners();
                btnCoin.onClick.AddListener(() => GameUIController.Instance.OnWinNext());
            }
            if (btnAds != null)
            {
                btnAds.interactable = true;
                btnAds.onClick.RemoveAllListeners();
                btnAds.onClick.AddListener(() => GameUIController.Instance.OnWinAds());
            }
            if (!gameObject.activeSelf) gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
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

        public void Hide()
        {
            SetVisible(false, false);
        }



        private void SetVisible(bool visible, bool instant)
        {
            if (canvasGroup != null)
            {
                canvasGroup.DOKill();
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
        }
    }
}
