using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    // LosePopup thật trong scene (con của Canvas_2 Overlays).
    // Thiết kế tham khảo bản gốc: Shade + Card + ImgTitle (Text_OhNooo)
    // + TxtLevelProgress + thanh progress (Frame_Progress_1/2)
    // + BtnRestart (Button_Green) / BtnHome (Button_Yellow + Icon_Home_3).
    public class LosePopup : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform cardRect;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image progressFill;
        [SerializeField] private Button btnRestart;
        [SerializeField] private Button btnHome;

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

        public void Show(int level = 1, float progress = 0.5f)
        {
            if (levelText != null) levelText.text = "Level " + level + "  " + Mathf.RoundToInt(progress * 100f) + "%";
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(progress);
            if (btnRestart != null)
            {
                btnRestart.interactable = true;
                btnRestart.onClick.RemoveAllListeners();
                btnRestart.onClick.AddListener(() => GameUIController.Instance.OnLoseRetry());
            }
            if (btnHome != null)
            {
                btnHome.interactable = true;
                btnHome.onClick.RemoveAllListeners();
                btnHome.onClick.AddListener(() => GameUIController.Instance.OnLoseHome());
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
