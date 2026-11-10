// GameManager.cs

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts;
using _Game.Scripts.Core;
using _Game.Scripts.LevelData;
using _Game.Scripts.Utils;
using _KIT.Utils;
using DG.Tweening;
using Lean.Common;
using Lean.Touch;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEventBus;
using Random = UnityEngine.Random;

namespace _Game.Scripts.Gameplay
{
    [DefaultExecutionOrder(-100)] // Ưu tiên chạy sớm
    public class GameManager : MonoBehaviour, IListener<YarnClickedMessage>, IListener<SlotFilledEvent>
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameConfig gameConfig;
        [SerializeField] private Camera orthoCam;

        public Camera OrthoCam => orthoCam;

        [Header("Auto-Zoom & Reposition")] [Tooltip("Sau mỗi mốc % còn lại (75%,50%,25%)")] [SerializeField]
        private float[] autoZoomThresholds = new float[] { 0.75f, 0.5f, 0.25f };

        private int currentZoomThresholdIndex = 0;

        [SerializeField] private BoxManager boxManager;
        public BoxManager BoxManager => boxManager;
        [SerializeField] int boxUnlockStart = 2;

        [SerializeField] private Transform decorationRoot;

        private int countYarnRemain;
        public int CountYarnRemain => countYarnRemain;
        private int currentNotUnlockBoxId;

        private int currentLevel;
        public int CurrentLevel => currentLevel;

        [Header("Test (chỉ dùng lúc dev)")]
        [Tooltip("Level load khi bấm Play. Mặc định 1. Để 2 để test Level2 không cần chơi Level1.")]
        [SerializeField]
        private int debugStartLevel = 1;

        private int realCurrentLevel;
        public int RealCurrentLevel => realCurrentLevel;

        private bool isGameOver;
        private int lastYarnTapFrame = -1;
        public bool IsGameOver => isGameOver;

        private bool isCheckingLose;
        public bool IsCheckingLose => isCheckingLose;
        public void SetCheckingLose(bool value) => isCheckingLose = value;

        private bool isAnimatingStartLevel;
        public bool IsAnimatingStartLevel => isAnimatingStartLevel;

        private bool isAutoZoom;
        public bool IsAutoZoom => isAutoZoom;

        private LevelYarnInfoHolder currentLevelHolder;

        //    Mỗi màu map -> danh sách tất cả YarnPartInfo (bao gồm layer 1,2,3…)
        private Dictionary<YarnColor, List<YarnPartInfo>> piecesByColor
            = new Dictionary<YarnColor, List<YarnPartInfo>>();

        // 3) Giữ map từ YarnPart (GameObject) vào YarnPartInfo tương ứng, không thay đổi trực tiếp mà phải sử dụng hàm Link và Unlink
        private Dictionary<YarnPart, YarnPartInfo> runtimeLink
            = new Dictionary<YarnPart, YarnPartInfo>();

        private Dictionary<YarnPartInfo, YarnPart> infoToPart = new Dictionary<YarnPartInfo, YarnPart>();

        private int initialYarnCount;
        public int InitialYarnCount => initialYarnCount;
        private int maxBoxes; // tổng số box cho phép

        public List<YarnColor> RandomColorPool { get; private set; }

        private float maxScale;

        private float scaleTweenDuration = 0.5f;

        // cache list để tái dùng mỗi lần tính center
        private readonly List<YarnPart> partCache = new List<YarnPart>();

        private Vector3 centerToDraw;
        private const float gizmoOuterRadius = 0.5f;
        private const float gizmoInnerRadius = 0.1f;

        private Transform targetLevelTransform;
        private Transform targetModelTransform;


        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject); // chỉ cho phép 1 instance
                return;
            }

            Instance = this;
        }

        void Start()
        {
            currentLevel = Mathf.Max(1, debugStartLevel);
            // Entry/Home lưu level hiện tại vào EconomyService -> ưu tiên level đó để
            // nút PLAY trên Home mở đúng level. debugStartLevel > 1 là override
            // thủ công để dev test một level cụ thể.
            if (currentLevel <= 1 && EconomyService.Instance != null)
            {
                // Level được CHỌN từ danh sách chọn màn (ưu tiên cao nhất)
                var picked = EconomyService.Instance.GetPlayLevel();
                EconomyService.Instance.ClearPlayLevel();
                if (picked > 1) currentLevel = picked;

                var savedLevel = EconomyService.Instance.GetLevel();
                if (currentLevel <= 1 && savedLevel > 1) currentLevel = savedLevel;
            }
            InitEmptyScreen();

            this.WaitNextFrame(LoadLevel);
            BoxManager.Instance.OnAllCurentBoxProcessingComplete += HandleAllBoxProcessed;
        }


        /// <summary>
        /// Load level, method quan trọng
        /// Cần phải chú ý reset tất cả các biến của GameManager và các Manager khác khi khởi tạo
        /// </summary>
        public void LoadLevel()
        {
            ClearDecorationRoot();
            if (currentLevel < 1)
                currentLevel = 1;

            // BGM Gameplay do AudioBootstrap phát khi load scene (không phát ở đây để khỏi restart 2 lần).

            // Thứ tự chơi (currentLevel) -> asset id (realCurrentLevel) lấy từ LevelCatalog.
            // Không có catalog (chưa sinh asset) thì fallback: id = thứ tự, đúng hành vi cũ.
            var catalog = LevelCatalogProvider.Catalog;
            var definition = catalog != null ? catalog.GetByOrder(currentLevel) : null;
            realCurrentLevel = definition != null ? definition.levelId : currentLevel;
            _Game.Scripts.LevelDebugLog.Init($"Level{realCurrentLevel}");

            if (currentLevelHolder != null)
            {
                Destroy(currentLevelHolder.gameObject);
            }

            var levelPath = $"Levels/Level{realCurrentLevel}";
            var levelPrefab = definition != null && definition.HasAsset
                ? definition.prefab
                : Resources.Load<GameObject>(levelPath);
            if (levelPrefab == null)
            {
                Debug.LogError($"Missing Resources/{levelPath}.prefab");
                return;
            }

            var tempLevel = Instantiate(levelPrefab);
            GameUIController.Instance.PositionLevelContainer(tempLevel);
            currentLevelHolder = tempLevel.GetComponent<LevelYarnInfoHolder>();
            var levelNameForImage = $"Level{realCurrentLevel}";
            JamImage.Instance?.Initialize(currentLevelHolder.colorCountEntries, levelNameForImage);
            targetLevelTransform = currentLevelHolder.transform;
            targetModelTransform = targetLevelTransform.GetChild(0);

            // --- 1) Khởi tạo RandomColorPool từ levelHolder - phải có random pool trc dể build data
            RandomColorPool = new List<YarnColor>();
            foreach (var e in currentLevelHolder.randomColorEntries)
                for (int i = 0; i < e.count; i++)
                    RandomColorPool.Add(e.color);

            // --- 2) Build toàn bộ dữ liệu YarnPartInfo cho mọi layer ---
            ClearLink();
            BuildAllPartData();

            LogUtils.LogPiecesByColor(piecesByColor);
            VerifyInitialCounts();

            currentLevelHolder.GetComponent<YarnPartLayerSpawner>().Init(); // gọi sau để khi spawn inner thì link được

            // --- 3) Thiết lập countYarnRemain & UI ---
            countYarnRemain = currentLevelHolder.totalYarnPartCount;
            initialYarnCount = countYarnRemain;

            maxBoxes = initialYarnCount / 3;
            isGameOver = false;
            isAutoZoom = false;
            isCheckingLose = false;

            // --- 4) Reset toàn bộ các manager
            // Slot manager phải xử lý trước, sau đó mới đăng ký lại slot của waiting và box
            SlotManager.Instance.UnregisterAllSlots();
            WaitingAreaManager.Instance.Initialize();

            // --- Spawn các Box ban đầu, có reset trạng thái của các box ---
            boxManager.Initialize(this, boxUnlockStart, maxBoxes);
            boxManager.OnAllBoxesClosed -= HandleAllBoxesClosed;
            boxManager.OnAllBoxesClosed += HandleAllBoxesClosed;
            boxManager.SpawnAllAtStart();

            // Level mới: xoá giỏ tạm của Clean ở level trước. _tempParts giữ YarnPart của
            // level cũ (đã bị Destroy khi load level) -> RefillFromTempBox sẽ ném lỗi và
            // làm mất slot của box mới, len cùng màu bị đẩy xuống waiting.
            if (BoosterManager.Instance != null)
                BoosterManager.Instance.Reset();

            YarnRopeController.Instance.ClearAllRopes();
            // Level mới: trả tốc độ in/dây về mặc định (lượt win trước đã tăng tốc).
            YarnRopeController.Instance.ResetRopeSpeed();
            if (JamImage.Instance != null)
                JamImage.Instance.ResetPrintingSpeed();

            // --- 5) Sau đó khởi tạo UI
            GameUIController.Instance.InitUI(currentLevel);

            maxScale = targetLevelTransform.GetComponent<LeanConstrainScale>().MaximumScale.x;
            currentZoomThresholdIndex = 0;

            // setup các vị trí gameplay object
            GameUIController.Instance.SetGamePlayObjectPosition();

            StartCoroutine(PlayStartLevelSequence());
        }

        public void LoadNextLevel()
        {
            var nextLevel = currentLevel + 1;
            var catalog = LevelCatalogProvider.Catalog;
            if (catalog != null && catalog.TotalLevels > 0)
            {
                // Catalog là nguồn chính: nhảy tới màn kế tiếp CÓ asset chơi được.
                // Hết màn thì quay vòng về màn đầu tiên.
                var next = catalog.GetNextPlayableOrder(currentLevel);
                nextLevel = next > 0 ? next : catalog.GetFirstPlayableOrder();
                if (nextLevel <= 0) nextLevel = 1;
            }
            else if (Resources.Load<GameObject>($"Levels/Level{nextLevel}") == null)
                nextLevel = 1;

            currentLevel = nextLevel;
            // Lưu tiến độ để Entry/Home đọc lại level hiện tại.
            if (EconomyService.Instance != null)
                EconomyService.Instance.SetLevel(nextLevel);
            LoadLevel();
        }

        private void InitEmptyScreen()
        {
            GameUIController.Instance.SetAllUIVisible(false);
            BoxManager.Instance.SetBoxesVisible(false);
            WaitingAreaManager.Instance.SetWaitingContainerVisible(false);
        }

        // sequence hiệu ứng đầu level
        private IEnumerator PlayStartLevelSequence()
        {
            isAnimatingStartLevel = true;

            // Tiếng vào màn (transition)
            AudioService.Instance?.PlayLevelStart();

            // 0) Prep: ẩn UI, block input, ẩn box + waiting area
            GameUIController.Instance.SetAllUIVisible(false);
            // Ensure popups are hidden at level start
            if (GameUIController.Instance != null)
            {
                if (GameUIController.Instance.winPopup != null)
                    GameUIController.Instance.winPopup.gameObject.SetActive(false);
                if (GameUIController.Instance.losePopup != null)
                    GameUIController.Instance.losePopup.gameObject.SetActive(false);
            }
            BoxManager.Instance.SetBoxesVisible(false);
            WaitingAreaManager.Instance.SetWaitingSlotsVisible(false);

            var lean = LeanTouch.Instance;
            lean.UseMouse = false;
            lean.UseTouch = false;

            // 1) Model bắt đầu ở scale 85%
            var initialScale = targetLevelTransform.localScale;
            var startScale = initialScale * 0.85f;
            var midScale = initialScale * 1.1f;
            targetLevelTransform.localScale = startScale;

            // 2) Quay đúng 1 vòng, nhanh rồi chậm, đồng thời scale lên 95% — 1s
            yield return targetLevelTransform
                .DORotate(360f * GameConfig.numRotate * Vector3.up, GameConfig.timeRotate, RotateMode.LocalAxisAdd)
                .SetEase(DG.Tweening.Ease.InOutCubic)
                .WaitForCompletion();

            // 3) Sau khi quay xong mới scale từ 85% → 95% (0.5s)
            yield return targetLevelTransform
                .DOScale(midScale, 0.35f)
                .SetEase(DG.Tweening.Ease.InOutQuad)
                .WaitForCompletion();

            // 4) Box bay vào giữa từ hai bên (0.5s) và fade in waiting
            bool boxDone = false;
            bool waitDone = false;

            float timeFly = 0.75f;
            StartCoroutine(RunAndFlag(BoxManager.Instance.AnimateBoxesFlyIn(timeFly / 2), () => boxDone = true));
            float timeFade = timeFly;
            if (currentLevel == 1)
            {
                timeFade /= 2;
            }

            StartCoroutine(RunAndFlag(WaitingAreaManager.Instance.FadeIn(timeFade), () => waitDone = true));

            yield return new WaitUntil(() => boxDone && waitDone);
            yield return new WaitForSeconds(0.2f);

            GameUIController.Instance.FadeInBoard();
            yield return new WaitForSeconds(0.3f);

            yield return GameUIController.Instance.FadeInAllUI(0.25f);

            // yield return model
            //     .DOScale(initialScale, 0.25f)
            //     .SetEase(DG.Tweening.Ease.InOutSine)
            //     .WaitForCompletion();

            lean.UseTouch = true;
            lean.UseMouse = true;

            // 6) Bật lại auto-rotate sau khi xong hết
            var rotator = currentLevelHolder.GetComponent<ModelDragRotate>();
            if (rotator != null)
                rotator.EnableAutoRotate(true);

            isAnimatingStartLevel = false;

        }

        public void EnableAutoRotate(bool value)
        {
            var rotator = currentLevelHolder.GetComponent<ModelDragRotate>();
            if (rotator != null)
                rotator.EnableAutoRotate(value);
        }

        // Helper chung: chạy routine, xong thì gọi onComplete
        private IEnumerator RunAndFlag(IEnumerator routine, Action onComplete)
        {
            yield return routine;
            onComplete?.Invoke();
        }

        public void ReloadCurrentScene()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public GameConfig GameConfig => gameConfig;

        public Transform GetTargetLevel()
        {
            return targetLevelTransform;
        }

        public Transform GetTargetModel()
        {
            return targetModelTransform;
        }

        public Transform GetDecorationRoot()
        {
            return decorationRoot;
        }

        private void OnEnable()
        {
            GlobalBus.Subscribe(this);
        }

        private void OnDisable()
        {
            GlobalBus.UnSubscribe(this);
        }

        public void React(in YarnClickedMessage e)
        {
            if (isGameOver)
                return;

            // KHÔNG giảm countYarnRemain ở đây - chỉ giảm khi sợi thực sự bị tiêu hao (trong ConsumeYarnPart)
            // Điều này tránh double-count: click -> waiting area (--), sau đó waiting -> box (--) -> count âm -> win sớm

        }

        private void CheckAutoZoomAndReposition()
        {
            if (currentZoomThresholdIndex >= autoZoomThresholds.Length)
                return;
            Debug.Log("CheckAutoZoomAndReposition");

            float percentLeft = (float)countYarnRemain / initialYarnCount;
            Debug.Log("Percent left = " + percentLeft);
            if (percentLeft <= autoZoomThresholds[currentZoomThresholdIndex])
            {
                ApplyAutoZoomAndReposition();
                currentZoomThresholdIndex++;
                LogUtils.Log("ApplyAutoZoomAndReposition | currentZoomThresholdIndex = " + currentZoomThresholdIndex,
                    Color.magenta);
            }
        }

        private void ApplyAutoZoomAndReposition()
        {
            targetLevelTransform.DOKill();
            var leanPinchScale = targetLevelTransform.GetComponent<LeanPinchScale>();

            // 1) Zoom bằng scale, lấy scale hiện tại lúc đó để tính scale mới
            var currentScaleFactor = targetLevelTransform.localScale.x;
            var targetScale = Mathf.Min(currentScaleFactor * GameConfig.autoZoomScale, maxScale);

            isAutoZoom = true;
            targetLevelTransform.DOScale(targetScale, scaleTweenDuration)
                .SetEase(DG.Tweening.Ease.OutQuad)
                .OnStart(() =>
                {
                    GameUIController.Instance.EnableSliderInteract(false);
                    leanPinchScale.enabled = false;
                })
                .OnComplete(() =>
                {
                    isAutoZoom = false;
                    // enable lại scale-features nếu cần
                    GameUIController.Instance.EnableSliderInteract(true);
                    leanPinchScale.enabled = true;
                });

            // --- Reposition tween (KHÓA CỨNG: chỉ zoom, model đứng yên.
            // Muốn dời lại thì bật AutoZoomRecenter = true.)
            const bool AutoZoomRecenter = false;
            if (!AutoZoomRecenter)
                return;
            // 1) tính center (hết part hoặc còn ≤2 part sót thì giữ nguyên vị trí)
            if (!TryGetActiveCenter(out Vector3 centerLocal, out int centerCount) || centerCount <= 2)
            {
                _Game.Scripts.LevelDebugLog.Log(
                    $"[AutoZoom] Recenter bỏ qua (centerCount={centerCount}) — giữ nguyên vị trí.");
                return;
            }

            _Game.Scripts.LevelDebugLog.Log(
                $"[AutoZoom] Dời model theo {centerCount} part, tâm=({centerLocal.x:F2},{centerLocal.y:F2}).");
            centerToDraw = targetModelTransform.TransformPoint(centerLocal);

            // 2) Tính vị trí mới 
            Vector3 targetLocalPos = new Vector3(-centerLocal.x, -centerLocal.y, 0);

            // 3) tween
            targetModelTransform.DOLocalMove(targetLocalPos, scaleTweenDuration)
                .SetEase(DG.Tweening.Ease.OutQuad);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
                return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(centerToDraw, gizmoOuterRadius);
            Gizmos.DrawSphere(centerToDraw, gizmoInnerRadius);
        }
#endif

        /// <summary>
        /// Tính tâm các part ngoài đang hoạt động. Hết part thì trả false
        /// (caller giữ nguyên vị trí, không được rào về origin).
        /// </summary>
        private bool TryGetActiveCenter(out Vector3 centerLocal, out int partCount)
        {
            centerLocal = Vector3.zero;
            partCount = 0;
            partCache.Clear();
            // Lấy tất cả YarnPart active dưới model
            partCache.AddRange(
                targetModelTransform.GetComponentsInChildren<YarnPart>(false)
            );

            // 1) Lấy list ưu tiên (priority=1), nếu trống thì tất cả
            var outerParts = partCache
                .Where(p =>
                {
                    if (runtimeLink.TryGetValue(p, out var info))
                        return info.Priority == 1;
                    return false;
                })
                .ToList();

            var useList = outerParts.Count > 0 ? outerParts : partCache;
            // 2) Vẫn không có phần tử nào -> báo hết, caller tự giữ vị trí cũ
            if (useList.Count == 0)
                return false;
            partCount = useList.Count;

            // 3) Tính trung bình world-space (vì nếu dùng local thì bị ảnh hưởng bởi group)
            Vector3 sum = Vector3.zero;
            foreach (var p in useList)
                sum += p.transform.position;
            Vector3 worldCenter = sum / useList.Count;

            // 4) Convert về local của targetModelTransform, và zero luôn z
            var local = targetModelTransform.InverseTransformPoint(worldCenter);
            centerLocal = new Vector3(local.x, local.y, 0f);
            return true;
        }

        public void React(in SlotFilledEvent e)
        {
            Debug.Log("GameManager React SlotFilledEvent");
            if (isGameOver)
                return;

            var slot = e.Slot;

            // (A) Lose: nếu vừa fill vào Waiting và tất cả waiting slots đã full,
            //     nhưng vẫn còn piece trên Shape -> LoseLevel.
            if (slot.Area == Slot.AreaType.Waiting && WaitingAreaManager.Instance.IsAllWaitingSlotsFilled())
            {
                // Slot chờ vừa nhận sợi nhưng animation (ring appear) CHƯA xong.
                // Sợi đó còn có thể được đẩy vào hộp: WaitingAreaManager.TryRefillSlot()
                // chạy SAU khi animation kết thúc. Kết luận thua ở đây là quá sớm —
                // sẽ báo thua trước khi sợi kịp vào hộp (đúng triệu chứng: bấm đúng
                // màu trên thanh hộp nhưng vẫn bị báo thua). Trì hoãn lại, sau khi
                // animation xong và đã thử refill sẽ check lại (RecheckLoseAfterWaitingAnimation).
                if (!slot.IsAnimationFilled)
                {
                    Debug.Log("[GameManager] Vùng chờ đầy nhưng slot vừa nhận sợi còn đang animation → trì hoãn check thua.");
                    isCheckingLose = true;
                    return;
                }

                Debug.Log("Box process = " + BoxManager.Instance.ProcessingBoxCount);
                // nếu không có box nào đang xử lý → lose ngay
                if (!BoxManager.Instance.IsProcessingBox)
                {
                    LoseLevel(e.Slot);
                }
                else
                {
                    isCheckingLose = true;
                }

                // ngược lại: đợi hết xử lý, callback sẽ re-check
                return;
            }

            // (B) Win: nếu vừa fill vào Box và remainingPieces==0 -> WinLevel
            if (slot.Area == Slot.AreaType.Box && countYarnRemain <= 0 && boxManager.HasSpawnLastBox())
            {
                WinLevel();
            }
        }

        /// <summary>
        /// Check lại thua sau khi animation của ô chờ vừa xong VÀ đã thử đẩy sợi vào hộp.
        /// Gọi từ WaitingAreaManager.TryRefillSlot — đây là lúc duy nhất biết chắc sợi
        /// đó còn vào được hộp hay không.
        /// </summary>
        public void RecheckLoseAfterWaitingAnimation(Slot slot)
        {
            if (isGameOver) return;

            if (!WaitingAreaManager.Instance.IsAllWaitingSlotsFilled())
            {
                // Đã có chỗ trống (sợi vào hộp rồi) → không thua.
                isCheckingLose = false;
                return;
            }

            // Vùng chờ vẫn đầy. Nếu đang có hộp xử lý thì đợi hộp xong
            // (HandleAllBoxProcessed sẽ check lại) — không kết luận thua vội.
            if (BoxManager.Instance.IsProcessingBox)
            {
                isCheckingLose = true;
                return;
            }

            Debug.Log("[GameManager] Vùng chờ đầy và không hộp nào nhận được sợi → thua.");
            LoseLevel(slot);
        }

        private void HandleAllBoxProcessed()
        {
            Debug.Log("HandleAllBoxProcessed");
            // khi tất cả box đã xong close+spawn, nếu vẫn full
            if (!isGameOver
                && WaitingAreaManager.Instance.IsAllWaitingSlotsFilled())
            {
                LoseLevel(null);
            }

            // nếu không thua, process xong rồi mà đang checking lose thì bỏ checking lose để user bấm lại được
            if (isCheckingLose)
            {
                if (!WaitingAreaManager.Instance.IsAllWaitingSlotsFilled())
                {
                    isCheckingLose = false;
                }
            }
        }

        private void HandleAllBoxesClosed()
        {
            if (isGameOver)
                return;

            if (WaitingAreaManager.Instance.IsAllWaitingSlotsFilled())
            {
                LoseLevel(null);
                return;
            }

            WinLevel();
        }

        private void WinLevel()
        {
            isGameOver = true;
            Debug.Log("=== YOU WIN LEVEL ===");
            GameUIController.Instance.HandleWinDelayed();
        }

        public void CheatWinLevel() => WinLevel();

        private void LoseLevel(Slot slot)
        {
            isGameOver = true;
            Debug.Log("=== YOU LOSE LEVEL ===");
            GameUIController.Instance.HandleLoseDelayed(slot);
        }

        #region Build Yarn Part Info Data

        // === BuildAllPartData ===
        private void BuildAllPartData()
        {
            // 1) Khởi tạo piecesByColor (với key = mọi YarnColor, list rỗng)
            piecesByColor.Clear();
            foreach (YarnColor col in System.Enum.GetValues(typeof(YarnColor)))
            {
                if (col == YarnColor.None)
                    continue;
                piecesByColor[col] = new List<YarnPartInfo>();
            }

            // 2) Với mỗi YarnPart gốc (layerIndex = 1) đã đặt trong LevelYarnInfoHolder.allBaseYarnParts
            //    Ta khởi tạo tương ứng một YarnPartInfo, sau đó đệ quy xây toàn bộ cây “inner”
            foreach (var rootPart in currentLevelHolder.allBaseYarnParts)
            {
                if (rootPart == null)
                {
                    Debug.LogError(
                        $"[GameManager] allBaseYarnParts chứa phần tử NULL (có thể bạn kéo nhầm decor không có YarnPart vào list). Mở prefab Level{realCurrentLevel}, kiểm tra Holder -> All Base Yarn Parts, xóa slot None.",
                        currentLevelHolder);
                    continue;
                }

                // 2a) Mỗi rootPart hiện tại là Layer 1, màu = rootPart.initColor
                var rootInfo = new YarnPartInfo(rootPart.initColor, 1, null);
                if (!piecesByColor.TryGetValue(rootInfo.Color, out var list))
                {
                    Debug.LogError(
                        $"[GameManager] {rootPart.name} có màu {rootInfo.Color} không hợp lệ (None?). Bỏ qua để không crash.",
                        rootPart);
                    continue;
                }

                list.Add(rootInfo);

                // 2b) Lưu link giữa runtime GameObject (nếu đã Spawn) và rootInfo
                //     Nhưng nhớ là layer 1 đã Spawn sẵn (YarnPart gốc).
                LinkPartInfo(rootPart, rootInfo);

                // 2c) Đệ quy tạo dữ liệu cho inner layers (2,3,…)
                BuildInnerPartData(rootInfo, rootPart, 2);
            }
        }

        /// <summary>
        /// Đệ quy tạo YarnPartInfo cho mọi tầng inner bên trong sourcePart (layerIndex gốc = 1).
        /// - parentInfo: YarnPartInfo tương ứng parent
        /// - parentVisual: YarnPart thật trên scene hoặc kiểu prefab mẫu (để gọi ChooseInnerColor)
        /// - nextLayer: thứ tự layer cần tạo (2, 3, …)
        /// </summary>
        private void BuildInnerPartData(YarnPartInfo parentInfo, YarnPart parentVisual, int nextLayer)
        {
            // Nếu parentVisual đã null nghĩa là không tồn tại scene object (unlikely), ta vẫn có thể dựa vào parentInfo
            // Nhưng YarnPartLayerSpawner cần một instance YarnPart để chọn màu inner (có thể tái sử dụng parentVisual).
            // Ở đây ta giả định parentVisual không null khi nextLayer <= parentVisual.totalInnerCount + 1

            // 1) Kiểm tra nếu đã vượt số layer được phép (totalInnerCount + 1)
            if (parentVisual == null)
                return;
            if (nextLayer > parentVisual.totalInnerCount + 1)
                return;

            // 2) Tính màu cho inner layer này theo logic của YarnPartLayerSpawner
            YarnColor chosenColor = ChooseInnerColorForData(parentVisual, nextLayer);

            // 3) Khởi tạo YarnPartInfo cho inner layer này
            var info = new YarnPartInfo(chosenColor, nextLayer, parentInfo);
            if (!piecesByColor.TryGetValue(chosenColor, out var colorList))
            {
                Debug.LogError($"[GameManager] inner layer màu {chosenColor} không hợp lệ (None?). Bỏ qua.",
                    parentVisual);
                return;
            }

            colorList.Add(info);

            // 4) (Chưa có GameObject tương ứng) → ta chỉ dừng ở mức dữ liệu, không Instantiate actual YarnPart
            //    Việc spawn actual GameObject sẽ diễn ra khi user click; 
            //    Lúc đó YarnPart.OnPartSpawned sẽ bind vào runtimeLink để đồng bộ với info.

            // 5) Tiếp tục đệ quy sang inner layer kế tiếp
            //    Nhưng để chọn màu inner của layer (nextLayer + 1), ta cần một “YarnPart prefab giả” để bỏ qua child hierarchy.
            //    Dễ nhất là tái sử dụng parentVisual.childPart (nếu có) hoặc 
            //    chỉ truyền parentVisual (vì ChooseInnerColorForData chỉ cần ancestor + innerColorEntries).
            BuildInnerPartData(info, parentVisual, nextLayer + 1);
        }

        /// <summary>
        /// Tái sử dụng logic chọn màu inner từ YarnPartLayerSpawner.
        /// Vì hàm ChooseInnerColor private, ta copy tương tự vào đây.
        /// </summary>
        private YarnColor ChooseInnerColorForData(YarnPart source, int layerIndex)
        {
            int idx = layerIndex - 2; // layerIndex=2 -> idx=0
            var ancestor = (source != null && source.ancestor != null) ? source.ancestor : source;
            // 1) Nếu Fixed:
            if (ancestor != null &&
                ancestor.innerColorTypes != null &&
                idx >= 0 &&
                idx < ancestor.innerColorTypes.Count &&
                ancestor.innerColorTypes[idx] == InnerColorType.Fixed)
            {
                if (idx < ancestor.innerFixedColors.Count)
                    return ancestor.innerFixedColors[idx];
            }

            // 2) Ngược lại nếu Random và pool chưa cạn:
            var pool = RandomColorPool;

            if (pool != null && pool.Count > 0)
            {
                int randomIndex = Random.Range(0, pool.Count);
                YarnColor chosen = pool[randomIndex];
                pool.RemoveAt(randomIndex);
                return chosen;
            }

            Debug.LogError("Random pool màu có vấn đề => phải xem lại config level");
            // 3) Nếu pool hết thì về default (None)
            return YarnColor.None;
        }

        #endregion


        // =========== Phần tính priority list & chọn màu box ===========

        /// <summary>
        /// Duyệt toàn bộ YarnPartInfo trong piecesByColor, build Dictionary<YarnColor, List<int>> priority list.
        /// </summary>
        private Dictionary<YarnColor, List<int>> CollectPriorityLists()
        {
            var result = new Dictionary<YarnColor, List<int>>();
            foreach (YarnColor col in System.Enum.GetValues(typeof(YarnColor)))
            {
                if (col == YarnColor.None)
                    continue;
                result[col] = new List<int>();
            }

            foreach (var kv in piecesByColor)
            {
                var col = kv.Key;
                foreach (var info in kv.Value)
                {
                    int p = info.Priority;
                    if (p >= 0)
                        result[col].Add(p);
                }
            }

            // Sort tăng dần
            foreach (var col in result.Keys)
                result[col].Sort();

            return result;
        }

        private int CountMissingSlotsInBoxes(YarnColor col)
        {
            // delegate về BoxManager
            return boxManager.CountMissingSlots(col);
        }

        public YarnColor ChooseNextBoxColor(bool forceMinPriority = false)
        {
            LogUtils.Log("-------------------------------------------------------------", Color.magenta);
            LogUtils.Log(
                $"[ChooseNextBoxColor] BEGIN, countYarnRemain={countYarnRemain}, initialYarnCount={initialYarnCount}",
                Color.magenta);

            var priLists = CollectPriorityLists();
            // === DEBUG: in toàn bộ priLists ra Console ===
            LogUtils.LogNonEmptyPriorityLists(priLists);

            // 1) Tính sum3 cho mỗi màu
            var sum3Dict = new Dictionary<YarnColor, int>();

            foreach (var kv in priLists)
            {
                var col = kv.Key;
                var list = kv.Value;

                // skip list không có phần từ
                if (list == null || list.Count == 0)
                    continue;

                // skip các phần tử có màu trùng với ô slot trống của box đang có
                int skip = CountMissingSlotsInBoxes(col);

                if (list.Count < skip + 3)
                {
                    Debug.Log("Skip màu " + col);
                    continue;
                }

                int s = 0;
                for (int i = skip; i < skip + 3; i++)
                    s += list[i];
                sum3Dict[col] = s;
            }

            LogUtils.LogDictionaryInline(sum3Dict, Color.green);


            // 2) Xác định phase dựa trên % piece đã dùng
            //    boxSpawnThresholds: List<int> diện tích [%], tổng = 100
            var thr = currentLevelHolder.boxSpawnThresholds;
            int[] cum = new int[4];
            cum[0] = thr[0];
            for (int i = 1; i < 4; i++)
                cum[i] = cum[i - 1] + thr[i];

            // phần trăm đã dùng = used*100 / total
            int used = initialYarnCount - countYarnRemain;
            int percent = initialYarnCount > 0
                ? Mathf.FloorToInt(used * 100f / initialYarnCount)
                : 100;
            int phase = 0;
            while (phase < 4 && percent > cum[phase])
                phase++;

            Debug.Log($"[ChooseNextBoxColor] used={used}/{initialYarnCount}, percent={percent}%, phase={phase + 1}");

            // 3) Lấy sorted màu theo sum3 tăng dần
            var sorted = sum3Dict
                .GroupBy(kv => kv.Value)
                .OrderBy(g => g.Key)
                .SelectMany(g =>
                        g.Select(kv => kv.Key)
                            .OrderBy(_ =>
                                Random.value) // xáo ngẫu nhiên trong nhóm cùng giá trị, để tránh tình trạng luôn lấy theo thứ tự bảng chữ cái
                )
                .ToList();

            LogUtils.LogListInline(sorted, Color.green);
            int total = sorted.Count;
            List<YarnColor> pool;

            // 4.1) SPECIAL OVERRIDE: nếu true, trả màu nhỏ nhất luôn
            if (forceMinPriority)
            {
                if (total == 0)
                {
                    var fb = GetMaxRemainingColor();
                    Debug.LogError($"[ChooseNextBoxColor - SPECIAL MIN PRIORITY] sorted rỗng — fallback {fb}.");
                    return fb;
                }

                LogUtils.Log("[ChooseNextBoxColor - SPECIAL MIN PRIORITY] RESULT = " + sorted[0], Color.magenta);
                LogUtils.Log("-------------------------------------------------------------", Color.magenta);
                return sorted[0];
            }

            // 4.2) Chọn pool theo phase
            switch (phase)
            {
                case 0: // Mở: 3 màu min
                    pool = sorted.Take(Mathf.Min(3, total)).ToList();
                    break;
                case 1: // Thân: 50% màu min
                    pool = sorted.Take(Mathf.CeilToInt(total * 0.5f)).ToList();
                    break;
                case 2: // Cao trào: bỏ 30%, giữ 75%
                {
                    int skipCount = Mathf.CeilToInt(total * 0.3f); // xóa 30% dưới cùng
                    int remainCount = total - skipCount; // phần còn lại
                    int takeCount = Mathf.CeilToInt(remainCount * 0.75f); // lấy 75% của phần còn lại
                    pool = sorted
                        .Skip(skipCount)
                        .Take(Mathf.Max(0, takeCount))
                        .ToList();
                }
                    break;
                case 3: // Kết: 75% màu min
                    pool = sorted.Take(Mathf.CeilToInt(total * 0.75f)).ToList();
                    break;
                default:
                    pool = sorted;
                    break;
            }

            // 5) Đảm bảo ít nhất 3 màu và màu đó có basket material
            // (tránh box sinh ra rổ trắng/dây sai màu khi bắn lên ảnh)
            pool = pool.Where(c => YarnColorManager.Instance.GetBasketMaterial(c) != null).ToList();

            if (pool.Count < 3)
            {
                // Fallback: lấy 3 màu có sum3 nhỏ nhất từ sorted (ưu tiên màu có material)
                var validSorted = sorted.Where(c => YarnColorManager.Instance.GetBasketMaterial(c) != null).ToList();
                if (validSorted.Count >= 3)
                    pool = validSorted.Take(3).ToList();
                else
                    pool = sorted.Take(Mathf.Min(3, total)).ToList();
                Debug.Log($"[ChooseNextBoxColor] Fallback ensure ≥3 colors: pool now = [{string.Join(",", pool)}]");
            }

            Debug.Log($"[ChooseNextBoxColor] pool(phase {phase + 1}) = [{string.Join(",", pool)}]");

            if (pool.Count == 0)
            {
                // Không màu nào đủ 3 sợi mới (thường do 1 màu lẻ, không chia hết cho 3).
                // Chẩn đoán kẹt: còn-lại vs giữ-chỗ từng màu để biết sót ở đâu.
                var dbg = new System.Text.StringBuilder("[ChooseNextBoxColor] pool rỗng — tồn kho: ");
                foreach (var kv in priLists)
                {
                    if (kv.Value == null || kv.Value.Count == 0) continue;
                    dbg.Append($"{kv.Key}(con-lai={kv.Value.Count},giu-cho={CountMissingSlotsInBoxes(kv.Key)}) ");
                }

                _Game.Scripts.LevelDebugLog.LogError(dbg + "— kiểm tra config: mỗi màu phải chia hết cho 3!");
                // Fallback đỡ hại nhất: màu còn nhiều nhất mà đang có box mở dở
                // (sợi có chỗ để vào), nếu không thì màu còn nhiều nhất.
                var fallback = ChooseStrandFallback(priLists);
                return fallback;
            }

            int idx = Random.Range(0, pool.Count);
            LogUtils.Log("[ChooseNextBoxColor] RESULT = " + pool[idx], Color.magenta);
            LogUtils.Log("-------------------------------------------------------------", Color.magenta);
            return pool[idx];
        }

        /// <summary>
        /// Chọn màu box cho revive.add_box: ưu tiên màu đang kẹt nhiều nhất ở waiting area
        /// để box mới có chỗ chứa ngay; fallback về màu còn nhiều nhất rồi tới logic thường.
        /// </summary>
        public YarnColor ChooseNextBoxColorRevive()
        {
            var waiting = WaitingAreaManager.Instance != null ? WaitingAreaManager.Instance.GetWaitingSlots() : null;
            if (waiting != null)
            {
                YarnColor best = YarnColor.None;
                int bestCount = 0;
                foreach (YarnColor col in System.Enum.GetValues(typeof(YarnColor)))
                {
                    if (col == YarnColor.None || col == YarnColor.Rainbow) continue;
                    int c = 0;
                    foreach (var s in waiting)
                        if (s != null && s.IsFilled && s.FilledColor == col) c++;
                    if (c > bestCount && YarnColorManager.Instance.GetBasketMaterial(col) != null)
                    {
                        bestCount = c;
                        best = col;
                    }
                }
                if (best != YarnColor.None && bestCount > 0) return best;
            }
            var fallback = GetMaxRemainingColor();
            return fallback != YarnColor.None ? fallback : ChooseNextBoxColor();
        }

        /// <summary>
        /// Phân xử runtime-vs-config ngay đầu game: so runtime build được (piecesByColor)
        /// với holder khai báo (colorCountEntries + totalYarnPartCount).
        /// Lệch ở đâu thì thằng đó báo sai — đọc log [START-CHECK].
        /// </summary>
        private void VerifyInitialCounts()
        {
            var runtime = new Dictionary<YarnColor, int>();
            foreach (var kv in piecesByColor)
                runtime[kv.Key] = kv.Value != null ? kv.Value.Count : 0;
            var declared = new Dictionary<YarnColor, int>();
            if (currentLevelHolder.colorCountEntries != null)
                foreach (var e in currentLevelHolder.colorCountEntries)
                    declared[e.color] = e.count;
            bool ok = true;
            foreach (YarnColor c in System.Enum.GetValues(typeof(YarnColor)))
            {
                runtime.TryGetValue(c, out int r);
                declared.TryGetValue(c, out int d);
                if (r != d)
                {
                    ok = false;
                    _Game.Scripts.LevelDebugLog.LogError(
                        $"[START-CHECK] {currentLevelHolder.name}: màu {c} runtime={r} != holder={d} (holder stale hoặc build sai).");
                }
            }

            int runtimeTotal = 0;
            foreach (var v in runtime.Values) runtimeTotal += v;
            if (runtimeTotal != currentLevelHolder.totalYarnPartCount)
            {
                ok = false;
                _Game.Scripts.LevelDebugLog.LogError(
                    $"[START-CHECK] {currentLevelHolder.name}: tổng runtime={runtimeTotal} != holder.totalYarnPartCount={currentLevelHolder.totalYarnPartCount} (totalYarnPartCount stale — mở prefab cho OnValidate chạy lại hoặc bấm Validate).");
            }

            // Pool thừa (supply > demand): còn màu trong pool không bao giờ spawn -> sợi ma, tổng ảo.
            // Bắt tại đầu game thay vì đợi cuối game lệch box. Sửa bằng tab Validate -> Sửa pool random.
            if (RandomColorPool != null && RandomColorPool.Count > 0)
            {
                ok = false;
                _Game.Scripts.LevelDebugLog.LogError(
                    $"[POOL-THỪA] {currentLevelHolder.name}: còn {RandomColorPool.Count} màu trong pool không có slot Random nào dùng tới (supply > demand) — holder đang đếm sợi ma! Vào Level Studio / tab 3 Validate -> Sửa pool random.");
            }

            if (ok)
                _Game.Scripts.LevelDebugLog.Log(
                    $"[START-CHECK] {currentLevelHolder.name}: runtime khớp holder ({runtimeTotal} sợi).");
        }

        /// <summary>
        /// Chụp tồn kho giữa game: Play tới lúc nghi -> Pause -> chuột phải GameManager ->
        /// "Debug Ton Kho". missing != 0 nghĩa là sợi mất đi đâu đó ngoài box (runtime leak).
        /// </summary>
        [ContextMenu("Debug Ton Kho")]
        public void DebugStocktake()
        {
            var onBoard = new Dictionary<YarnColor, int>();
            var inQueue = new Dictionary<YarnColor, int>();
            var inBox = new Dictionary<YarnColor, int>();
            foreach (var kv in piecesByColor)
            foreach (var info in kv.Value)
            {
                var d = info.State == PartState.OnBoard ? onBoard : info.State == PartState.InQueue ? inQueue : inBox;
                if (!d.ContainsKey(kv.Key)) d[kv.Key] = 0;
                d[kv.Key]++;
            }

            var sb = new System.Text.StringBuilder(
                $"[STOCKTAKE] initial={initialYarnCount} remain={countYarnRemain}:\n");
            var all = new HashSet<YarnColor>(onBoard.Keys);
            all.UnionWith(inQueue.Keys);
            all.UnionWith(inBox.Keys);
            if (currentLevelHolder.colorCountEntries != null)
                foreach (var e in currentLevelHolder.colorCountEntries)
                    all.Add(e.color);
            foreach (var c in all)
            {
                onBoard.TryGetValue(c, out int b);
                inQueue.TryGetValue(c, out int q);
                inBox.TryGetValue(c, out int x);
                int init = 0;
                if (currentLevelHolder.colorCountEntries != null)
                    foreach (var e in currentLevelHolder.colorCountEntries)
                        if (e.color == c)
                            init += e.count;
                int missing = init - (b + q + x);
                sb.AppendLine(
                    $"  {c}: init={init} board={b} queue={q} box={x} mat-tich={missing}{(missing != 0 ? " <<< LECH" : "")}");
            }

            _Game.Scripts.LevelDebugLog.Log(sb.ToString());
        }

        /// <summary>
        /// Màu còn nhiều sợi nhất (priority >= 0, tức OnBoard + InQueue) và có basket material.
        /// Trả None chỉ khi thật sự không còn sợi nào.
        /// </summary>
        public YarnColor GetMaxRemainingColor()
        {
            var priLists = CollectPriorityLists();
            YarnColor best = YarnColor.None;
            int bestCount = 0;
            foreach (var kv in priLists)
            {
                if (kv.Key == YarnColor.None || kv.Key == YarnColor.Rainbow) continue;
                if (kv.Value == null || kv.Value.Count == 0) continue;
                if (YarnColorManager.Instance.GetBasketMaterial(kv.Key) == null) continue;
                if (kv.Value.Count > bestCount)
                {
                    bestCount = kv.Value.Count;
                    best = kv.Key;
                }
            }

            return best;
        }

        /// <summary>
        /// Fallback khi pool rỗng (sót lẻ cuối game): ưu tiên màu còn nhiều nhất mà đang
        /// có box mở dở (sợi mới tap có chỗ để vào ngay), nếu không thì màu còn nhiều nhất.
        /// Vẫn log cảnh báo vì đây là tình huống lẽ ra không xảy ra khi config %3 chuẩn.
        /// </summary>
        private YarnColor ChooseStrandFallback(Dictionary<YarnColor, List<int>> priLists)
        {
            YarnColor best = YarnColor.None;
            int bestScore = -1;
            foreach (var kv in priLists)
            {
                if (kv.Key == YarnColor.None || kv.Key == YarnColor.Rainbow) continue;
                if (kv.Value == null || kv.Value.Count == 0) continue;
                if (YarnColorManager.Instance.GetBasketMaterial(kv.Key) == null) continue;
                int remain = kv.Value.Count;
                int reserved = CountMissingSlotsInBoxes(kv.Key);
                // +1000 điểm nếu đang có box dở để sợi vào tiếp được ngay.
                int score = remain + (reserved > 0 ? 1000 : 0);
                if (score > bestScore)
                {
                    bestScore = score;
                    best = kv.Key;
                }
            }

            if (best == YarnColor.None)
                _Game.Scripts.LevelDebugLog.LogError(
                    "[ChooseNextBoxColor] fallback cũng None (hết sợi thật?). Kiểm tra lose/win.");
            else
                _Game.Scripts.LevelDebugLog.LogWarning($"[ChooseNextBoxColor] fallback strand = {best}.");
            return best;
        }

        /// <summary>
        /// Lấy tối đa <paramref name="count"/> info còn OnBoard của màu cho MagicBox.
        /// (Thay cho bản meta-game đầy đủ; đủ để booster biên dịch và chạy ở bản core.)
        /// </summary>
        public List<YarnPartInfo> GetNextInfosForMagicBox(YarnColor color, int count, out int missing)
        {
            var result = new List<YarnPartInfo>();
            if (piecesByColor.TryGetValue(color, out var list) && list != null)
            {
                foreach (var info in list.Where(i => i.State == PartState.OnBoard).OrderBy(i => i.LayerIndex))
                {
                    if (result.Count >= count) break;
                    result.Add(info);
                }
            }
            missing = Mathf.Max(0, count - result.Count);
            return result;
        }

        /// <summary>
        /// Đếm lại số sợi còn lại từ dữ liệu (đồng bộ sau khi MagicBox hút sợi).
        /// </summary>
        public void UpdateCountYarnRemain()
        {
            int total = 0;
            foreach (var kv in piecesByColor)
                if (kv.Value != null) total += kv.Value.Count;
            countYarnRemain = Mathf.Max(0, total);
        }

        public void HandlePartPicked(YarnPart part)
        {
            if (runtimeLink.TryGetValue(part, out var info))
            {
                info.MarkPickedToQueue(); // State = InQueue, priority = 0
            }
        }

        public void HandlePartBoxed(YarnPart part)
        {
            if (!runtimeLink.TryGetValue(part, out var info))
                return;
            info.MarkFilledIntoBox(); // State = InBox, priority = -1
        }

        public float GetGameProgress()
        {
            var gameProgress = 1f - (float)countYarnRemain / (float)currentLevelHolder.totalYarnPartCount;
            return gameProgress;
        }

        /// <summary>
        /// Consume một YarnPart: dissolve visual, update tree và xóa data.
        /// </summary>
        public void ConsumeYarnPart(YarnPart part)
        {
            // 1) Lấy info gốc
            if (!TryGetInfoByPart(part, out var info))
                return;

            // 2) Cập nhật cây dữ liệu: reparent các info con lên parentInfo
            var parentInfo = info.Parent;
            if (parentInfo != null)
            {
                foreach (var childInfo in info.Children)
                {
                    childInfo.Parent = parentInfo;
                    parentInfo.Children.Add(childInfo);
                }

                parentInfo.Children.Remove(info);

                // 3) Cập nhật liên kết YarnPart.childPart **nếu** đã spawn sẵn
                if (TryGetPartByInfo(parentInfo, out var parentPart))
                {
                    // tìm lớp kế tiếp thật sự còn OnBoard
                    var nextInfo = parentInfo.Children
                        .Where(c => c.State == PartState.OnBoard)
                        .OrderBy(c => c.LayerIndex)
                        .FirstOrDefault();

                    if (nextInfo != null && TryGetPartByInfo(nextInfo, out var nextPart))
                    {
                        // chỉ gán nếu nextPart đã tồn tại
                        parentPart.childPart = nextPart;
                        nextPart.gameObject.SetActive(true);
                    }
                    else
                    {
                        // không có phần con → clear link
                        parentPart.childPart = null;
                    }
                }
            }

            // 4) Xóa link và remove info khỏi nhóm màu
            UnlinkPart(part);
            piecesByColor[info.Color].Remove(info);

            // Giảm số sợi còn lại khi sợi thực sự bị tiêu hao (dissolve/boxed)
            countYarnRemain = Mathf.Max(0, countYarnRemain - 1);
        }

        public bool CanInteractWithGame()
        {
            // MoreBoosterPopupUI khoá input vài frame sau khi đóng do mua thành công:
            // uGUI xử lý onClick ở PointerUp, LeanTouch fire OnFingerTap cùng frame nên
            // nếu không khoá thì cú click mua sẽ xuyên xuống và kích hoạt dây/part.
            if (MoreBoosterPopupUI.IsBlockingGameplayInput)
                return false;

            return !isGameOver && !isCheckingLose && !isAnimatingStartLevel;
        }

        public bool TryBeginYarnTap()
        {
            if (!CanInteractWithGame() || lastYarnTapFrame == Time.frameCount)
                return false;

            lastYarnTapFrame = Time.frameCount;
            return true;
        }

        public bool CanInteractNotCheckAnimStartLevel()
        {
            return !isGameOver && !isCheckingLose;
        }

        #region Link Part and Info

        // Link Part and Info Method
        public void LinkPartInfo(YarnPart part, YarnPartInfo info)
        {
            runtimeLink[part] = info;
            infoToPart[info] = part;
        }

        public void UnlinkPart(YarnPart part)
        {
            if (!runtimeLink.TryGetValue(part, out var info))
                return;

            // Nếu info.Parent == null => đây là root, giữ lại mapping cho spawn sau này
            if (info.Parent != null)
            {
                runtimeLink.Remove(part);
                infoToPart.Remove(info);
            }
        }

        // 2.2. TryGetPartByInfo  
        public bool TryGetPartByInfo(YarnPartInfo info, out YarnPart part)
            => infoToPart.TryGetValue(info, out part);

        // 2.3. TryGetInfoByPart  
        public bool TryGetInfoByPart(YarnPart part, out YarnPartInfo info)
            => runtimeLink.TryGetValue(part, out info);

        private void ClearLink()
        {
            runtimeLink.Clear();
            infoToPart.Clear();

        }

        #endregion


        #region Log

        private int GetMaxBuiltLayer(YarnPartInfo info)
        {
            int max = info.LayerIndex;
            foreach (var c in info.Children)
                max = Mathf.Max(max, GetMaxBuiltLayer(c));
            return max;
        }

        private void LogInnerBuildSummary()
        {
            foreach (var root in currentLevelHolder.allBaseYarnParts)
            {
                if (root.totalInnerCount <= 1)
                    continue;
                if (!runtimeLink.TryGetValue(root, out var rootInfo))
                    continue;

                // 1) Gom số lượng node theo LayerIndex bằng BFS
                var counts = new Dictionary<int, int>();
                var queue = new Queue<YarnPartInfo>();
                queue.Enqueue(rootInfo);
                while (queue.Count > 0)
                {
                    var info = queue.Dequeue();
                    foreach (var child in info.Children)
                    {
                        // tăng đếm cho layer của child
                        if (!counts.ContainsKey(child.LayerIndex))
                            counts[child.LayerIndex] = 0;
                        counts[child.LayerIndex]++;
                        queue.Enqueue(child);
                    }
                }

                // 2) Xác định layer cao nhất đã sinh
                int maxLayer = counts.Keys.Any() ? counts.Keys.Max() : rootInfo.LayerIndex;

                // 3) Log tổng số tầng (không tính layer gốc)
                int builtLayers = maxLayer - rootInfo.LayerIndex;
                Debug.Log(
                    $"[BuildSummary] {root.name}: totalInnerCount={root.totalInnerCount}, built layers={builtLayers}");

                // 4) Log chi tiết mỗi layer
                //    từ layer gốc+1 tới maxLayer, hiển thị count (0 nếu không có)
                for (int layer = rootInfo.LayerIndex + 1; layer <= maxLayer; layer++)
                {
                    int c = counts.ContainsKey(layer) ? counts[layer] : 0;
                    Debug.Log($"[BuildSummary] {root.name}: layer {layer} → child count = {c}");
                }
            }
        }

        #endregion

        private void ClearDecorationRoot()
        {
            for (int i = decorationRoot.childCount - 1; i >= 0; i--)
            {
                var child = decorationRoot.GetChild(i);
                if (child != null)
                    Destroy(child.gameObject);
            }
        }

        public int GetCountYarnRemain()
        {
            return countYarnRemain;
        }
    }
}

