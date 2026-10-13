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
            ResolveRefs();
            gameObject.SetActive(false);
        }

        private void ResolveRefs()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            if (cardRect == null)
            {
                var card = transform.Find("Card");
                if (card != null) cardRect = card as RectTransform;
            }
            if (messageText == null) messageText = FindText("TxtMessage");
            if (coinText == null) coinText = FindText("BtnCoin/TxtCoin");
            if (btnCoin == null) btnCoin = FindButton("BtnCoin");
            if (btnAds == null) btnAds = FindButton("BtnAds");
        }

        private TextMeshProUGUI FindText(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<TextMeshProUGUI>() : null;
        }

        private Button FindButton(string path)
        {
            var t = transform.Find(path);
            return t != null ? t.GetComponent<Button>() : null;
        }

        public void Show(int coinReward = 60)
        {
            ResolveRefs();
            // Chẩn đoán: log chính xác ref nào còn thiếu + trạng thái render.
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
            Debug.Log("[WinPopup.Show] done. activeSelf=" + gameObject.activeSelf
                + " childCount=" + transform.childCount, this);
        }

        public void Hide()
        {
            if (canvasGroup != null) canvasGroup.DOKill();
            if (cardRect != null) cardRect.DOKill();
            gameObject.SetActive(false);
        }
    }
}
