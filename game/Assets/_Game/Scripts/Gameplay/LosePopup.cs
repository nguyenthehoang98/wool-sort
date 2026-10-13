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
            if (levelText == null)
            {
                var t = transform.Find("TxtLevel");
                if (t != null) levelText = t.GetComponent<TextMeshProUGUI>();
            }
            if (progressFill == null)
            {
                var f = transform.Find("ProgressBack/ProgressFill");
                if (f != null) progressFill = f.GetComponent<Image>();
            }
            if (btnRestart == null)
            {
                var r = transform.Find("BtnRestart");
                if (r != null) btnRestart = r.GetComponent<Button>();
            }
            if (btnHome == null)
            {
                var h = transform.Find("BtnHome");
                if (h != null) btnHome = h.GetComponent<Button>();
            }
        }

        public void Show(int level = 1, float progress = 0.5f)
        {
            ResolveRefs();
            // Chẩn đoán: log chính xác ref nào còn thiếu + trạng thái render.
            var canvas = GetComponentInParent<Canvas>();
            Debug.Log("[LosePopup.Show] refs: canvasGroup=" + (canvasGroup != null)
                + " cardRect=" + (cardRect != null)
                + " levelText=" + (levelText != null)
                + " progressFill=" + (progressFill != null)
                + " btnRestart=" + (btnRestart != null)
                + " btnHome=" + (btnHome != null)
                + " | activeSelf=" + gameObject.activeSelf
                + " | canvas=" + (canvas != null ? canvas.name + " scale=" + canvas.transform.localScale : "NULL"), this);
            if (canvas != null && canvas.transform.localScale == Vector3.zero)
            {
                Debug.LogWarning("[LosePopup.Show] Canvas scale 0 -> reset về 1.", this);
                canvas.transform.localScale = Vector3.one;
            }

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
            Debug.Log("[LosePopup.Show] done. activeSelf=" + gameObject.activeSelf
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
