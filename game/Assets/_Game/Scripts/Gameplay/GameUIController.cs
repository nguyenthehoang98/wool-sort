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
        [SerializeField] private Canvas canvasOverlay;

        [Header("Button")]

        [Header("Slider")][SerializeField] private ScaleSliderSync scaleSlider;

        [Header("Level Info")]
        [SerializeField] private TextMeshProUGUI txtLevel;

        private bool isLockClick;
        private Canvas uiCanvas;
        [Header("Popups (kéo thả trong Inspector)")]
        public WinPopup winPopup;
        public LosePopup losePopup;
        [Tooltip("Popup mua booster (MoreBoosterPopupUI trong scene). Kéo thả vào đây để BoosterItem mở được popup.")]
        public MoreBoosterPopupUI moreBoosterPopup;
        private bool resultPopupShown;
        private bool resultWon;

        [Header("Debug Hotkeys (Editor)")]
        [Tooltip("Bật để bấm W/L hiện Win/Lose popup khi Play trong Editor.")]
        public bool enableDebugHotkeys = true;

        private void Awake()
        {
            Instance = this;
            uiCanvas = canvasGroupBase.GetComponent<Canvas>();
            EnsureCanvasScale(uiCanvas);
            EnsureCanvasScale(canvasOverlay);
            
            if (winPopup == null) Debug.LogError("[GameUIController] Không tìm thấy WinPopup trong scene.", this);
            if (losePopup == null) Debug.LogError("[GameUIController] Không tìm thấy LosePopup trong scene.", this);
        }

        private void Update()
        {
#if UNITY_EDITOR
            if (!enableDebugHotkeys || !Application.isPlaying) return;
            if (Input.GetKeyDown(KeyCode.W)) TestShowWin();
            if (Input.GetKeyDown(KeyCode.L)) TestShowLose();
#endif
        }

        // Canvas root scale 0 làm cả canvas tàng hình (từng xảy ra với
        // Canvas_1/Canvas_2 trong scene). Tự chữa + log để dễ phát hiện.
        private static void EnsureCanvasScale(Canvas c)
        {
            if (c != null && c.transform.localScale == Vector3.zero)
            {
                Debug.LogWarning("[GameUIController] Canvas " + c.name + " scale 0 -> reset về 1.", c);
                c.transform.localScale = Vector3.one;
            }
        }

        [ContextMenu("Test Show WinPopup (W)")]
        public void TestShowWin()
        {
            Debug.Log("[GameUIController] Test hotkey W -> ShowWin. winPopup="
                + (winPopup != null ? winPopup.name : "NULL"), this);
            ShowWin();
        }

        [ContextMenu("Test Show LosePopup (L)")]
        public void TestShowLose()
        {
            Debug.Log("[GameUIController] Test hotkey L -> ShowLose. losePopup="
                + (losePopup != null ? losePopup.name : "NULL"), this);
            ShowLose();
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
            if (boxManager == null)
            {
                ShowWin();
                return;
            }

            // Vào win: tăng tốc in pixel + dây bay để không phải đợi lâu, thay vì
            // chờ nguyên tốc độ in bình thường mới hiện popup.
            if (JamImage.Instance != null)
                JamImage.Instance.RushPrinting();
            if (YarnRopeController.Instance != null)
                YarnRopeController.Instance.RushRopes();

            // Chờ 2 điều kiện rồi mới show WinPopup:
            //  1) box cuối cùng đã đóng xong animation
            //  2) ảnh + dây in đã xong hết (nếu không popup hiện khi dây còn đang in)
            if (boxManager.HasFiredAllBoxesClosed && !boxManager.IsAnyPrinting)
            {
                ShowWin();
                return;
            }

            // Luôn chạy coroutine chờ print: nó tự xử lý cả trường hợp box chưa đóng.
            // Không dùng event OnAllBoxesClosed làm điểm hẹn duy nhất vì event chỉ fire
            // 1 lần và có thể đã fire trước khi ta kịp đăng ký.
            StartCoroutine(WaitPrintDoneThenShowWin(boxManager));

            Debug.Log("Core gameplay level complete; waiting for box close + print done");
        }

        /// <summary>
        /// Chờ tới khi box cuối đóng xong VÀ ảnh in + dây bay lên ảnh xong, rồi mới show WinPopup.
        /// Trong lúc chờ đã bật RushPrinting nên thời gian chờ rất ngắn.
        /// </summary>
        private IEnumerator WaitPrintDoneThenShowWin(BoxManager boxManager)
        {
            // Giai đoạn 1: đợi box cuối đóng (thường chỉ vài chục ms sau khi win).
            float closeDeadline = Time.time + 5f;
            while (boxManager != null
                   && !boxManager.HasFiredAllBoxesClosed
                   && Time.time < closeDeadline)
                yield return null;

            // Giai đoạn 2: đợi in xong. Đã rush nên chỉ còn rất ngắn; timeout rộng
            // để không kẹt màn hình thắng nếu cờ print bị treo.
            float printDeadline = Time.time + 5f;
            while (boxManager != null && boxManager.IsAnyPrinting && Time.time < printDeadline)
                yield return null;

            if (boxManager != null && boxManager.IsAnyPrinting)
                Debug.LogWarning("[GameUIController] Chờ print quá lâu, hiện WinPopup để không kẹt màn hình.", this);

            // Thêm 1 frame để pixel/dây cuối cùng kịp render trước khi popup che lên.
            yield return null;

            if (resultPopupShown) yield break;
            ShowWin();
        }

        private void ShowWin()
        {
            if (winPopup == null)
            {
                Debug.LogWarning("[GameUIController] WinPopup chưa có trong scene.", this);
                return;
            }

            // Chốt chặn cuối: không show khi dây/ảnh còn đang in.
            var boxManager = GameManager.Instance != null ? GameManager.Instance.BoxManager : null;
            if (boxManager != null && boxManager.IsAnyPrinting)
            {
                Debug.Log("[GameUIController] ShowWin bị hoãn: vẫn còn in ảnh/dây.");
                StartCoroutine(WaitPrintDoneThenShowWin(boxManager));
                return;
            }

            winPopup.Show(Configs.WIN_REWARD);
            resultPopupShown = true;
            resultWon = true;
            isLockClick = true;
            if (boardTransform != null)
                boardTransform.localScale = Vector3.one;
            else
                Debug.LogWarning("[GameUIController] boardTransform chưa gán.", this);
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
                // Fallback: nếu anim treo và event không bao giờ fire,
                // vẫn hiện popup sau 2.5s thay vì kẹt màn hình thua.
                StartCoroutine(ShowLoseFallback(slot, show));
            }
            else
            {
                StartCoroutine(ShowLoseAfterDelay());
            }

            Debug.Log("Core gameplay level failed; waiting for final slot animation");
        }

        private IEnumerator ShowLoseFallback(Slot slot, Action show)
        {
            yield return new WaitForSeconds(2.5f);
            if (resultPopupShown) yield break;
            Debug.LogWarning("[GameUIController] OnAnimationFilled quá hạn, hiện LosePopup fallback.", this);
            slot.OnAnimationFilled -= show;
            StartCoroutine(ShowLoseAfterDelay());
        }

        private IEnumerator ShowLoseAfterDelay()
        {
            yield return new WaitForSeconds(0.25f);
            ShowLose();
        }

        private void ShowLose()
        {
            if (resultPopupShown) return;
            if (losePopup == null)
            {
                Debug.LogWarning("[GameUIController] LosePopup chưa có trong scene.", this);
                return;
            }
            int level = GameManager.Instance != null ? GameManager.Instance.CurrentLevel : 1;
            float progress = GameManager.Instance != null ? GameManager.Instance.GetGameProgress() : 0.5f;
            losePopup.Show(level, progress);
            resultPopupShown = true;
            resultWon = false;
            isLockClick = true;
            if (boardTransform != null)
                boardTransform.localScale = Vector3.one;
            else
                Debug.LogWarning("[GameUIController] boardTransform chưa gán.", this);
        }

        // Gỡ ResultPopup sinh từ code: Win/Lose popup nằm sẵn trong
        // Gameplay.unity (con của Canvas_2 Overlays), xem WinPopup/LosePopup.

        public void OnWinNext()
        {
            if (winPopup != null) winPopup.Hide();
            resultPopupShown = false;
            GameManager.Instance.LoadNextLevel();
        }

        // Nút xem ads trên WinPopup (stub: hiện tại đi tiếp luôn, chờ AdsService).
        public void OnWinAds()
        {
            Debug.Log("[WinPopup] Claim x3 via ads (stub) -> next level.");
            if (winPopup != null) winPopup.Hide();
            resultPopupShown = false;
            GameManager.Instance.LoadNextLevel();
        }

        public void OnLoseRetry()
        {
            if (losePopup != null) losePopup.Hide();
            resultPopupShown = false;
            GameManager.Instance.LoadLevel();
        }

        // Nút home trên LosePopup (stub: hiện tại reload level, chờ Home flow).
        public void OnLoseHome()
        {
            Debug.Log("[LosePopup] Home (stub) -> reload level.");
            if (losePopup != null) losePopup.Hide();
            resultPopupShown = false;
            GameManager.Instance.LoadLevel();
        }

        private void HideResultPopup()
        {
            // hide both popups if any are active
            winPopup?.Hide();
            losePopup?.Hide();
            resultPopupShown = false;
        }

        public void ReleaseLockClickUI() => isLockClick = false;

        #endregion

        public void SetAllUIVisible(bool visible)
        {
            canvasGroupBase.alpha = visible ? 1f : 0f;
            canvasGroupBase.interactable = visible;
            canvasGroupBase.blocksRaycasts = visible;
            boardTransform.localScale = Vector3.one;
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

        // Stub thay cho meta-game (ads/shop) chưa có trong bản core này.
        public void ShiftNoAdsForTempBox() { }
        public void ResetNoAdsButtonPosition() { }
    }
}
