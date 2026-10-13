using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    // WinPopup thật trong scene (con của Canvas_2 Overlays).
    // Thiết kế tham khảo bản gốc: Shade + Card + ImgTitle (Text_Congratulations)
    // + khung tròn (Frame_Picture_GP_3 + Pattern_01) + message + 2 nút
    // BtnCoin (Button_Green + Icon_Coin_S) / BtnAds (Button_Blue + Icon_ADS_1).
    public class WinPopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cardRect;
        [SerializeField] private TextMeshProUGUI messageText;
        [SerializeField] private Button btnCoin;
        [SerializeField] private Button btnAds;
        [SerializeField] private TextMeshProUGUI coinText;

        private void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (cardRect == null)
            {
                var card = transform.Find("Card");
                if (card != null) cardRect = (RectTransform)card;
            }
            gameObject.SetActive(false);
        }

        public void Show(int coinReward = 60)
        {
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
            gameObject.SetActive(true);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.DOFade(1f, 0.2f);
            }
            if (cardRect != null)
            {
                cardRect.localScale = Vector3.one * 0.85f;
                cardRect.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
            }
        }

        public void Hide()
        {
            if (canvasGroup != null) canvasGroup.DOKill();
            if (cardRect != null) cardRect.DOKill();
            gameObject.SetActive(false);
        }
    }
}
