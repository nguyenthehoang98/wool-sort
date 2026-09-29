using _Game.Scripts.Utils;
using DG.Tweening;
using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts.Gameplay
{
    public class GameUIController : MonoBehaviour
    {
        public static GameUIController Instance { get; private set; }

        [Header("3D Object")]
        [SerializeField] private Transform boardTransform;
        [SerializeField] private Transform boxContainerTransform;
        [SerializeField] private Transform waitingAreaTransform;
        [SerializeField] private RectTransform topAnchor;
        [SerializeField] private RectTransform botAnchor;
        [SerializeField] private Camera orthoCam;
        [SerializeField] private Camera mainCam;
        [Range(0f, 1f)][SerializeField] private float boardOffsetPercent;
        [Range(0f, 1f)][SerializeField] private float boxOffsetPercent;      // ví dụ 0.2 = 20% từ trên xuống
        [Range(0f, 1f)][SerializeField] private float waitingOffsetPercent;  // ví dụ 0.3 = 30% từ trên xuống
        [SerializeField] private RectTransform levelTopAnchor;
        [SerializeField] private RectTransform levelBotAnchor;
        [SerializeField] public Transform levelContainer;

        [Header("Canvas")]
        [SerializeField] private CanvasGroup canvasGroupBase;

        [Header("Button")]

        [Header("Slider")][SerializeField] private ScaleSliderSync scaleSlider;

        [Header("Level Info")]
        [SerializeField] private TextMeshProUGUI txtLevel;

        private bool isLockClick;
        private Canvas uiCanvas;
        private GameObject resultPopup;
        private CanvasGroup resultPopupGroup;
        private RectTransform resultPopupCard;
        private TextMeshProUGUI resultTitle;
        private TextMeshProUGUI resultMessage;
        private TextMeshProUGUI resultActionLabel;
        private Button resultActionButton;
        private bool resultPopupShown;
        private bool resultWon;

        private void Awake()
        {
            Instance = this;

            uiCanvas = canvasGroupBase.GetComponent<Canvas>();
            CreateResultPopup();
        }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Mỗi khi load level thì phải khởi tạo lại 1 số thành phần UI theo lv
        public void InitUI(int lv)
        {
            HideResultPopup();
            ResetData();

            scaleSlider.SetTargetScale(GameManager.Instance.GetTargetLevel());
            //thumbnailController.SetupThumbnails(lv);

            txtLevel.gameObject.SetActive(true);
            txtLevel.text = string.Format("LEVEL {0}", lv);
            /*txtLevelVeryHard.SetParamLocalize(lv.ToString());
            txtLevelHard.SetParamLocalize(lv.ToString());
            txtLevelVeryHard.SetParamLocalize(lv.ToString());*/

            isLockClick = false;


        }

        private void ResetData() { }

        public void SetGamePlayObjectPosition()
        {
            FollowAnchor(boardTransform, boardOffsetPercent);
            FollowAnchor(boxContainerTransform, boxOffsetPercent);
            FollowAnchor(waitingAreaTransform, waitingOffsetPercent);

        }

        /// <summary>
        /// Đặt target3D nằm tương ứng tỉ lệ offset giữa topAnchor và bottomAnchor
        /// Thay vì set anchor theo vị trí của box và waiting ở màn chuẩn thì dùng cách này box và waiting sẽ xếp tương đối ở khoảng trống của UI không bị cách xa nhau quá trên các màn dài
        /// </summary>
        private void FollowAnchor(Transform target3D, float offsetPercent // 0 = đúng topAnchor, 1 = đúng bottomAnchor
        )
        {
            // 1) Lấy screen-space của 2 anchor
            Camera uiCam = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : uiCanvas.worldCamera;
            Vector2 topSP = RectTransformUtility.WorldToScreenPoint(uiCam, topAnchor.position);
            Vector2 bottomSP = RectTransformUtility.WorldToScreenPoint(uiCam, botAnchor.position);

            // 2) Nội suy
            Vector2 screenPoint = Vector2.Lerp(topSP, bottomSP, offsetPercent);

            // 3) Chiếu ray lên plane Z của target3D
            var plane = new Plane(Vector3.forward, new Vector3(0, 0, target3D.position.z));
            var ray = orthoCam.ScreenPointToRay(screenPoint);
            if (plane.Raycast(ray, out float enter))
                target3D.position = ray.GetPoint(enter);
        }

        public void PositionLevelContainer(GameObject level)
        {
            // 1. Chọn camera UI (null nếu Overlay)
            Camera camUI = uiCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                ? null
                : uiCanvas.worldCamera;

            // 2. Lấy screen‐space của cả hai anchor
            Vector3 topScreen = RectTransformUtility.WorldToScreenPoint(camUI, levelTopAnchor.position);
            Vector3 botScreen = RectTransformUtility.WorldToScreenPoint(camUI, levelBotAnchor.position);
            Debug.Log("Top Screen = " + topScreen);
            Debug.Log("Bot Screen = " + botScreen);

            // chuẩn bị z để convert world
            float worldZ = levelContainer.position.z;
            float screenZ = mainCam.WorldToScreenPoint(new Vector3(0, 0, worldZ)).z;
            topScreen.z = botScreen.z = screenZ;

            // 4. Tính trung điểm trên screen và convert về world
            Vector3 midScreen = (topScreen + botScreen) * 0.5f;
            Debug.Log("Mid Screen = " + midScreen);
            Vector3 midWorld = mainCam.ScreenToWorldPoint(new Vector3(midScreen.x, midScreen.y, screenZ));
            Debug.Log("Mid World = " + midWorld);

            // 5. Gán vị trí
            levelContainer.position = midWorld;

            level.transform.SetParent(levelContainer, false);

            // debug: spawn image tại midScreen
            // SpawnDebugDot(midScreen, camUI);
        }

        void SpawnDebugDot(Vector2 screenPos, Camera camUI)
        {
            var go = new GameObject("DebugDot", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(uiCanvas.transform, false);

            var rt = go.GetComponent<RectTransform>();
            // convert screenPoint → localPoint trên Canvas
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiCanvas.transform as RectTransform,
                screenPos,
                camUI,
                out Vector2 localPoint
            );

            // đặt anchor giữa và gán localPoint
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = localPoint;
            rt.sizeDelta = new Vector2(20, 20); // kích thước debug

            var img = go.GetComponent<Image>();
            img.color = Color.red;
        }

        #region Win/Lose

        public void HandleWin() => HandleWinDelayed();

        public void HandleWinDelayed()
        {
            isLockClick = true;
            if (resultPopupShown)
                return;

            var boxManager = GameManager.Instance != null ? GameManager.Instance.BoxManager : null;
            if (boxManager == null || boxManager.HasFiredAllBoxesClosed)
            {
                ShowResultPopup(true);
                return;
            }

            Action show = null;
            show = () =>
            {
                boxManager.OnAllBoxesClosed -= show;
                ShowResultPopup(true);
            };
            boxManager.OnAllBoxesClosed += show;
            Debug.Log("Core gameplay level complete; waiting for final box animation");
        }

        public void HandleLose() => HandleLoseDelayed(null);

        public void HandleLoseDelayed(Slot slot)
        {
            isLockClick = true;
            if (resultPopupShown)
                return;

            if (slot != null && !slot.IsAnimationFilled)
            {
                Action show = null;
                show = () =>
                {
                    slot.OnAnimationFilled -= show;
                    StartCoroutine(ShowLoseAfterDelay());
                };
                slot.OnAnimationFilled += show;
            }
            else
            {
                StartCoroutine(ShowLoseAfterDelay());
            }

            Debug.Log("Core gameplay level failed; waiting for final slot animation");
        }

        private IEnumerator ShowLoseAfterDelay()
        {
            yield return new WaitForSeconds(0.25f);
            ShowResultPopup(false);
        }

        private void ShowResultPopup(bool won)
        {
            if (resultPopup == null)
                return;

            resultPopupShown = true;
            resultWon = won;
            isLockClick = true;
            boardTransform.localScale = Vector3.zero;
            resultTitle.text = won ? "YOU WIN" : "LEVEL FAILED";
            resultMessage.text = won ? "Great job!" : "Try again and sort the yarn.";
            resultActionLabel.text = won ? "NEXT" : "RETRY";
            resultActionButton.interactable = true;
            resultActionButton.onClick.RemoveAllListeners();
            resultActionButton.onClick.AddListener(OnResultAction);

            resultPopup.SetActive(true);
            resultPopupGroup.alpha = 0f;
            resultPopupCard.localScale = Vector3.one * 0.85f;
            resultPopupGroup.DOFade(1f, 0.2f);
            resultPopupCard.DOScale(1f, 0.25f).SetEase(Ease.OutBack);
        }

        private void OnResultAction()
        {
            if (resultActionButton != null)
                resultActionButton.interactable = false;

            HideResultPopup();
            if (resultWon)
                GameManager.Instance.LoadNextLevel();
            else
                GameManager.Instance.LoadLevel();
        }

        private void HideResultPopup()
        {
            if (resultPopup == null)
                return;

            resultPopupGroup?.DOKill();
            resultPopupCard?.DOKill();
            resultPopupShown = false;
            resultPopup.SetActive(false);
        }

        private void CreateResultPopup()
        {
            if (uiCanvas == null || resultPopup != null)
                return;

            resultPopup = new GameObject("ResultPopup", typeof(RectTransform), typeof(CanvasGroup));
            resultPopup.transform.SetParent(uiCanvas.transform, false);
            resultPopup.transform.SetAsLastSibling();
            var popupRect = resultPopup.GetComponent<RectTransform>();
            popupRect.anchorMin = Vector2.zero;
            popupRect.anchorMax = Vector2.one;
            popupRect.offsetMin = Vector2.zero;
            popupRect.offsetMax = Vector2.zero;
            resultPopupGroup = resultPopup.GetComponent<CanvasGroup>();

            var shade = CreateImage("Shade", resultPopup.transform, new Color(0f, 0f, 0f, 0.65f));
            Stretch(shade.rectTransform);

            var card = CreateImage("Card", resultPopup.transform, new Color(0.96f, 0.92f, 0.86f, 1f));
            resultPopupCard = card.rectTransform;
            resultPopupCard.anchorMin = resultPopupCard.anchorMax = new Vector2(0.5f, 0.5f);
            resultPopupCard.sizeDelta = new Vector2(720f, 620f);

            resultTitle = CreateText("Title", resultPopupCard, 56, Color.black, TextAlignmentOptions.Center);
            resultTitle.rectTransform.anchorMin = new Vector2(0f, 0.62f);
            resultTitle.rectTransform.anchorMax = new Vector2(1f, 0.88f);
            resultTitle.rectTransform.offsetMin = resultTitle.rectTransform.offsetMax = Vector2.zero;

            resultMessage = CreateText("Message", resultPopupCard, 30, new Color(0.15f, 0.15f, 0.15f), TextAlignmentOptions.Center);
            resultMessage.rectTransform.anchorMin = new Vector2(0.08f, 0.43f);
            resultMessage.rectTransform.anchorMax = new Vector2(0.92f, 0.62f);
            resultMessage.rectTransform.offsetMin = resultMessage.rectTransform.offsetMax = Vector2.zero;

            var buttonImage = CreateImage("ActionButton", resultPopupCard, new Color(0.2f, 0.55f, 0.9f, 1f));
            var buttonRect = buttonImage.rectTransform;
            buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(0.5f, 0.2f);
            buttonRect.sizeDelta = new Vector2(420f, 120f);
            resultActionButton = buttonImage.gameObject.AddComponent<Button>();
            resultActionButton.targetGraphic = buttonImage;

            resultActionLabel = CreateText("Label", buttonRect, 34, Color.white, TextAlignmentOptions.Center);
            Stretch(resultActionLabel.rectTransform);
            resultPopup.SetActive(false);
        }

        private static Image CreateImage(string name, Transform parent, Color color)
        {
            var image = new GameObject(name, typeof(RectTransform), typeof(Image)).GetComponent<Image>();
            image.transform.SetParent(parent, false);
            image.color = color;
            return image;
        }

        private static TextMeshProUGUI CreateText(string name, Transform parent, float fontSize, Color color, TextAlignmentOptions alignment)
        {
            var text = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
            text.transform.SetParent(parent, false);
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.color = color;
            text.alignment = alignment;
            text.enableWordWrapping = true;
            return text;
        }

        private static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        public void ReleaseLockClickUI() => isLockClick = false;

        #endregion

        public void SetAllUIVisible(bool visible)
        {
            canvasGroupBase.alpha = visible ? 1f : 0f;
            canvasGroupBase.interactable = visible;
            canvasGroupBase.blocksRaycasts = visible;
            boardTransform.localScale = visible ? Vector3.one : Vector3.zero;
        }

        /// <summary>
        /// Fade in toàn bộ UI trong uiFadeDuration.
        /// </summary>
        public IEnumerator FadeInAllUI(float uiFadeDuration)
        {
            canvasGroupBase.blocksRaycasts = true;
            yield return canvasGroupBase
                .DOFade(1f, uiFadeDuration)
                .SetEase(DG.Tweening.Ease.InOutSine)
                .WaitForCompletion();
            canvasGroupBase.interactable = true;
            boardTransform.localScale = Vector3.one;
        }

        public void FadeInBoard()
        {
            boardTransform.DOScale(1f, 0.125f);
        }

        public void EnableSliderInteract(bool value)
        {
            scaleSlider.scaleSlider.interactable = value;
        }

                public void PlayWarningLose(bool value) { }
    }
}
