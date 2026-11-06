using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using _Game.Scripts.Core;

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


        private void Awake()
        {
            ValidateRefs();
            // Popup luôn active trong hierarchy, ẩn/hiện bằng alpha để tránh
            // mọi vấn đề với SetActive (không hiện dù đã gọi SetActive(true)).
            SetVisible(false, true);
        }

        private void OnEnable()
        {
            // Popup bị SetActive(false) lúc bắt đầu level (GameManager.PlayStartLevelSequence)
            // rồi SetActive(true) lúc show. Awake chỉ chạy 1 lần nên chuẩn hoá scale
            // mỗi lần bật để tránh tàng hình do scale 0 còn sót.
            if (canvasGroup != null)
                canvasGroup.transform.localScale = Vector3.one;
        }

        private void ValidateRefs()
        {
            if (canvasGroup == null) Debug.LogWarning("[LosePopup] Chưa kéo canvasGroup.", this);
            if (cardRect == null) Debug.LogWarning("[LosePopup] Chưa kéo cardRect.", this);
            if (levelText == null) Debug.LogWarning("[LosePopup] Chưa kéo levelText.", this);
            if (progressFill == null) Debug.LogWarning("[LosePopup] Chưa kéo progressFill.", this);
            if (btnRestart == null) Debug.LogWarning("[LosePopup] Chưa kéo btnRestart.", this);
            if (btnHome == null) Debug.LogWarning("[LosePopup] Chưa kéo btnHome.", this);

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

                + " | activeSelf=" + gameObject.activeSelf
                + " | canvas=" + (canvas != null ? canvas.name + " scale=" + canvas.transform.localScale : "NULL"), this);
            // Stop any ongoing rope printing when level fails
            YarnRopeController.Instance?.ClearAllRopes();

            if (canvas != null && canvas.transform.localScale == Vector3.zero)
            {
                Debug.LogWarning("[LosePopup.Show] Canvas scale 0 -> reset về 1.", this);
                canvas.transform.localScale = Vector3.one;
            }

            if (levelText != null) levelText.text = "Level " + level + "  " + Mathf.RoundToInt(progress * 100f) + "%";
            if (progressFill != null) progressFill.fillAmount = Mathf.Clamp01(progress);

            AudioService.Instance?.PlayLose();

            if (btnRestart != null)
            {
                btnRestart.interactable = true;
                btnRestart.onClick.RemoveAllListeners();
                btnRestart.onClick.AddListener(() => { AudioService.Instance?.PlayClick(); GameUIController.Instance.OnLoseRetry(); });
            }
            if (btnHome != null)
            {
                btnHome.interactable = true;
                btnHome.onClick.RemoveAllListeners();
                btnHome.onClick.AddListener(() => { AudioService.Instance?.PlayClick(); GameUIController.Instance.OnLoseHome(); });
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
            Debug.Log("[LosePopup.Show] done. activeSelf=" + gameObject.activeSelf
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
                        // Set scale = 1 NGAY rồi mới tween: nếu tween bị chặn/không chạy
                        // (timeScale 0, object vừa bật) popup vẫn hiện đủ scale.
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
    }
}
