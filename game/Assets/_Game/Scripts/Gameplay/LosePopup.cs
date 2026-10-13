using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    // LosePopup thật trong scene (con của Canvas_2 Overlays).
    // Kéo thả ref trực tiếp trong Inspector — không Find trong code.
    // Thiết kế tham khảo bản gốc: Shade + Card + ImgTitle (Text_OhNooo)
    // + TxtLevelProgress + thanh progress (Frame_Progress_1/2)
    // + BtnRestart (Button_Green) / BtnHome (Button_Yellow + Icon_Home_3).
    public class LosePopup : MonoBehaviour
    {
        [Header("UI References (kéo thả trong Inspector)")]
        public CanvasGroup canvasGroup;
        public RectTransform cardRect;
        public TextMeshProUGUI levelText;
        public Image progressFill;
        public Button btnRestart;
        public Button btnHome;
        [Tooltip("Ảnh tròn chụp màn hình lúc thua (con ImgCircle/ImgPhoto).")]
        public Image photoImage;

        private Sprite lastPhoto;

        private void Awake()
        {
            ValidateRefs();
            // Popup luôn active trong hierarchy, ẩn/hiện bằng alpha để tránh
            // mọi vấn đề với SetActive (không hiện dù đã gọi SetActive(true)).
            SetVisible(false, true);
        }

        private void ValidateRefs()
        {
            if (canvasGroup == null) Debug.LogWarning("[LosePopup] Chưa kéo canvasGroup.", this);
            if (cardRect == null) Debug.LogWarning("[LosePopup] Chưa kéo cardRect.", this);
            if (levelText == null) Debug.LogWarning("[LosePopup] Chưa kéo levelText.", this);
            if (progressFill == null) Debug.LogWarning("[LosePopup] Chưa kéo progressFill.", this);
            if (btnRestart == null) Debug.LogWarning("[LosePopup] Chưa kéo btnRestart.", this);
            if (btnHome == null) Debug.LogWarning("[LosePopup] Chưa kéo btnHome.", this);
            if (photoImage == null) Debug.LogWarning("[LosePopup] Chưa kéo photoImage.", this);
        }

        public void Show(int level = 1, float progress = 0.5f)
        {
            var canvas = GetComponentInParent<Canvas>();
            Debug.Log("[LosePopup.Show] refs: canvasGroup=" + (canvasGroup != null)
                + " cardRect=" + (cardRect != null)
                + " levelText=" + (levelText != null)
                + " progressFill=" + (progressFill != null)
                + " btnRestart=" + (btnRestart != null)
                + " btnHome=" + (btnHome != null)
                + " photoImage=" + (photoImage != null)
                + " | activeSelf=" + gameObject.activeSelf
                + " | canvas=" + (canvas != null ? canvas.name + " scale=" + canvas.transform.localScale : "NULL"), this);
            if (canvas != null && canvas.transform.localScale == Vector3.zero)
            {
                Debug.LogWarning("[LosePopup.Show] Canvas scale 0 -> reset về 1.", this);
                canvas.transform.localScale = Vector3.one;
            }

            if (levelText != null) levelText.text = "Level " + level + "  " + Mathf.RoundToInt(progress * 100f) + "%";
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(progress);
            RefreshPhoto();
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
            Debug.Log("[LosePopup.Show] done. activeSelf=" + gameObject.activeSelf
                + " alpha=" + (canvasGroup != null ? canvasGroup.alpha.ToString() : "NULL")
                + " childCount=" + transform.childCount, this);
        }

        public void Hide()
        {
            SetVisible(false, false);
        }

        // Chụp gameplay lúc này làm ảnh tròn. Gọi trong Show, trước khi board ẩn.
        private void RefreshPhoto()
        {
            if (photoImage == null) return;
            if (lastPhoto != null)
            {
                if (lastPhoto.texture != null) Destroy(lastPhoto.texture);
                Destroy(lastPhoto);
                lastPhoto = null;
            }
            var cam = GameManager.Instance != null ? GameManager.Instance.OrthoCam : null;
            lastPhoto = PopupPhoto.Capture(cam);
            if (lastPhoto != null)
            {
                photoImage.sprite = lastPhoto;
                photoImage.preserveAspect = true;
                photoImage.color = Color.white;
            }
            else
            {
                var c = photoImage.color;
                c.a = 0f;
                photoImage.color = c;
            }
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
