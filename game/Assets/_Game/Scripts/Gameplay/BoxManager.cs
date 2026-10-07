using _Game.Scripts;
using _Game.Scripts.Pool;
using _KIT.Utils;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Ease = DG.Tweening.Ease;

namespace _Game.Scripts.Gameplay
{
    public class BoxManager : MonoBehaviour
    {
        public static BoxManager Instance { get; private set; }

        [SerializeField] private Transform boxContainerTransform;
        [SerializeField] private Transform boardTarget;
        [Tooltip("Khoảng cách giữa các box")] public float spacing = 1.2f;
        [SerializeField] private Box boxPrefab;
        [SerializeField] private BoxSpawnPoint[] spawnPoints;
        [SerializeField] private SlotManager slotManager;
        [SerializeField] private WaitingAreaManager waitingArea;

        // fire khi box cuối cùng out‐anim xong và không spawn thêm
        public event Action OnAllBoxesClosed;

        private GameManager gm;
        private int spawnTotalCount; // đã bao nhiêu box được spawn
        private int maxBoxes; // giới hạn tổng box

        private readonly List<Box> activeBoxes = new List<Box>();

        // Count of boxes that have finished out-animation
        private int closeCount;

        // Ensure OnAllBoxesClosed fires only once
        private bool hasFiredAllClosed;
        public bool HasFiredAllBoxesClosed => hasFiredAllClosed;

        // số box đang trong quá trình full->close→spawn
        private int processingBoxCount = 0;
        public int ProcessingBoxCount => processingBoxCount;


        // có đang có ít nhất 1 box đang xử lý không
        public bool IsProcessingBox => processingBoxCount > 0;

        // event chỉ fire khi TẤT CẢ các box hiện tại đang xử lý đã xong
        public event Action OnAllCurentBoxProcessingComplete;

        // số lượt spawn kế tiếp sẽ ép chọn box có tổng min‐priority (sau khi dùng booster)
        // Revive: sau khi lấy 1 box theo màu của hàng đợi, thì set giá trị này = 1
        // AddBox: box vừa add và box tiếp theo => set = 2 (trước khi add box)
        // MagicBox: 2 box tiếp theo sau box magic
        // Image printing was removed; keep the slot-disappear phase finite.
        private float currentWaitOfTime = 1f;
        
        [SerializeField] private Data[] printTimeData;
        
        [Serializable]
        public struct Data
        {
            public Vector2Int range;
            public float time;
        }

        private void Awake()
        {
            Instance = this;

            PoolManager.Instance.Register<Box>("box", boxPrefab.gameObject);
        }

        private void OnDisable()
        {
            StopLoopPrintSfx();
        }

        /// <summary>
        /// Đếm số slot chưa fill của tất cả Box đang active có màu col.
        /// </summary>
        public int CountMissingSlots(YarnColor col)
        {
            int skip = 0;
            foreach (var box in activeBoxes)
            {
                if (box.BoxColor != col) continue;
                foreach (var s in box.Slots)
                    if (!s.IsFilled)
                        skip++;
            }

            return skip;
        }

        /// <summary>
        /// @gameManager: để gọi ChooseNextBoxColor()
        /// @initialUnlock: số box mở khóa ban đầu (vẫn spawn bình thường)
        /// @maxBoxes: tổng số box tối đa cho level
        /// </summary>
        public void Initialize(GameManager gameManager, int initialUnlock, int maxBoxes)
        {
            StopLoopPrintSfx();
            
            // 1) Dừng mọi Coroutine đang chạy để tránh tình trạng box cũ vẫn out-anim, spawn-anim...
            StopAllCoroutines();

            // 2 + 3) Đưa tất cả box về pool, còn thì destroy
            foreach (Transform p in spawnPoints.Select(sp => sp.point))
            {
                foreach (var child in p.Cast<Transform>().ToList())
                {
                    var b = child.GetComponent<Box>();
                    if (b != null) b.Despawn();
                    else Destroy(child.gameObject);
                }
            }

            // 4) Reset lại state nội bộ
            activeBoxes.Clear();
            closeCount = 0;
            spawnTotalCount = 0;
            hasFiredAllClosed = false;
            processingBoxCount = 0;

            // 5) Thiết lập lại biến cho lần spawn mới
            gm = gameManager;
            this.maxBoxes = maxBoxes;

            // đánh dấu unlocked cho những spawnPoint đầu
            for (int i = 0; i < spawnPoints.Length; i++)
                spawnPoints[i].unlocked = (i < initialUnlock);

            foreach (var sp in spawnPoints)
                sp.point.gameObject.SetActive(true);
        }

        public void SpawnAllAtStart()
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnTotalCount >= maxBoxes)
                    break;

                if (spawnPoints[i].unlocked)
                    SpawnInitialBox(i);
                else
                {
                    spawnPoints[i].point.gameObject.SetActive(false);
                }
            }
            Relayout();
        }

        private void SpawnInitialBox(int idx)
        {
            // không animation, + count ngay lập tức
            spawnTotalCount++;
            var sp = spawnPoints[idx];

            // GET từ pool + parent về đúng spawnPoint
            var box = PoolManager.Instance.Get<Box>("box");
            box.transform.SetParent(sp.point, false);

            activeBoxes.Add(box);

            var initColor = gm.ChooseNextBoxColor();
            box.Initialize(initColor);

            System.Action<Box> onFull = null;
            onFull = b =>
            {
                // bắt đầu 1 phiên xử lý của 1 box
                processingBoxCount++;

                b.OnBoxFull -= onFull;
                slotManager.UnregisterSlots(b.Slots);
                b.SetSlotsInteractable(false);
            };
            box.OnBoxFull += onFull;

            System.Action<Box> onClose = null;
            onClose = b =>
            {
                b.OnBoxClose -= onClose;
                PrintImage(b);
                StartCoroutine(HandleCloseRoutine(b, idx));
            };
            box.OnBoxClose += onClose;

            // None phải check SAU khi đã đăng ký OnBoxClose để AutoCloseEmptyBox có chuỗi đóng mà chạy.
            if (initColor == YarnColor.None)
            {
                Debug.LogError("BoxManager: box đầu game đã None (config màu lẻ?) — tự đóng để không kẹt game!", box);
                StartCoroutine(AutoCloseEmptyBox(box, idx));
                return;
            }

            box.SetSlotsInteractable(true);
            slotManager.RegisterSlots(box.Slots);
        }

        // bình thường khi sinh box mới là kết thúc quá trình process của box cũ
        // nhưng trong trường hợp là unlock box thì cần check để không 
        private IEnumerator SpawnBoxRoutine(int idx, bool needProcess = true)
        {
            // tạm thời không chặn spawn box khi thua, để nếu revive không bị lỗi ko có box ở vị trí đó
            // if (gm.IsGameOver) yield break;

            if (spawnTotalCount >= maxBoxes)
                yield break;

            // runtime spawn: có animation
            spawnTotalCount++;
            Debug.Log("spawn box total count = " + spawnTotalCount);
            var sp = spawnPoints[idx];

            // GET từ pool + parent
            var box = PoolManager.Instance.Get<Box>("box");
            box.transform.SetParent(sp.point, false);

            activeBoxes.Add(box);

            //Xu ly logic chon mau Next Box theo Normal hay Revive
            var yarnColor = YarnColor.None;

            yarnColor = gm.ChooseNextBoxColor();

            box.Initialize(yarnColor);
            box.SetSlotsInteractable(false);

            System.Action<Box> onFull = null;
            onFull = b =>
            {
                // bắt đầu 1 phiên xử lý của 1 box
                processingBoxCount++;

                b.OnBoxFull -= onFull;
                slotManager.UnregisterSlots(b.Slots);
                b.SetSlotsInteractable(false);
            };
            box.OnBoxFull += onFull;

            System.Action<Box> onClose = null;
            onClose = b =>
            {
                b.OnBoxClose -= onClose;
                PrintImage(b);
                StartCoroutine(HandleCloseRoutine(b, idx));
            };
            box.OnBoxClose += onClose;

            // Hết sợi thật sự (không còn màu nào) — đóng box rỗng để giữ nhịp close/spawn/win,
            // thay vì để box None nhận bừa 3 màu lạ rồi tô sai ảnh. Check SAU đăng ký OnBoxClose.
            if (yarnColor == YarnColor.None)
            {
                Debug.LogError("BoxManager: sinh box None (hết sợi hoặc config màu lẻ?) — tự đóng box rỗng!", box);
                StartCoroutine(AutoCloseEmptyBox(box, idx));
                yield break;
            }

            box.PlayInAnimation();
            yield return new WaitForSeconds(box.InDuration);

            slotManager.RegisterSlots(box.Slots);

            // confirm logic từ GD, ưu tiên fill từ slot hàng đợi trước rồi mới fill từ temp box
            waitingArea.TryRefillAll();
            box.SetSlotsInteractable(true);

            // với các box do unlock hoặc revive thì mới sinh ra lần đầu sẽ không trừ process
            if (needProcess)
            {
                FinishProcessingBox();
            }
        }


        /// <summary>
        /// Giả lập fill animation cho box rỗng (None) để nó đi đúng chuỗi đóng/spawn/win
        /// mà không tô ảnh, không nhận sợi bừa.
        /// </summary>
        private IEnumerator AutoCloseEmptyBox(Box box, int idx)
        {
            yield return null; // đợi subscribe OnBoxClose xong
            if (box == null) yield break;
            foreach (var s in box.Slots)
                s.ShowFullStateVisual(); // -> pendingCloseCount -> OnBoxClose -> PrintImage(None: skip) -> HandleCloseRoutine
        }

        // Chạy BasketOut rồi destroy + respawn tiếp
        private IEnumerator HandleCloseRoutine(Box box, int idx)
        {
            if (box == null) yield break;

            // 1) Remove khỏi active list ngay để list luôn phản ánh box còn tồn tại
            activeBoxes.Remove(box);

            // 2) Play out-animation
            box.DoEffectDisappearAllSlot(currentWaitOfTime);
            yield return new WaitForSeconds(currentWaitOfTime);

            box.PlayOutAnimation();
            yield return new WaitForSeconds(box.OutDuration);

            // 3) Đưa về pool, đợi 1 lúc để khi spawn mới thì không lấy luôn box này
            this.WaitInvoke(0.1f, () => { box.Despawn(); });

            // 4) Sau khi box out-anim hoàn tất, tăng close count
            closeCount++;
            Debug.Log("close box count = " + closeCount);

            // khi đóng box xong mà không sinh box nữa thì relayout
            if (spawnTotalCount >= maxBoxes)
            {
                // khi không sinh nữa thì ẩn luôn vị trí spawn point trc khi relayout
                spawnPoints[idx].point.gameObject.SetActive(false);
                Relayout();
            }

            // 5) Nếu đã đóng hết maxBoxes thì đây đúng là box cuối, fire Win
            if (closeCount == maxBoxes && !hasFiredAllClosed)
            {
                hasFiredAllClosed = true;
                FinishProcessingBox();
                OnAllBoxesClosed?.Invoke();
                SetBoxesVisible(false);
                yield break;
            }

            // 6) Nếu chưa hết limit thì tiếp tục spawn
            if (spawnTotalCount < maxBoxes)
            {
                yield return SpawnBoxRoutine(idx);
            }
            else
            {
                // nếu không sinh thêm box thì quá trình xử lý box đã xong
                FinishProcessingBox();
            }
        }

        private void FinishProcessingBox()
        {
            processingBoxCount = Mathf.Max(0, processingBoxCount - 1);
            if (processingBoxCount == 0)
                this.WaitInvoke(0.25f, () => OnAllCurentBoxProcessingComplete?.Invoke());
        }

        public bool HasSpawnLastBox()
        {
            return spawnTotalCount >= maxBoxes;
        }

        public bool MayRelayoutBox()
        {
            return spawnTotalCount >= maxBoxes - 4;
        }

        /// <summary>
        /// Gọi mỗi khi bạn Spawn or Destroy một Box/UnlockButton
        /// Để tối ưu thì chỉ khi nào số box + unlock btn khác 4 mới cần gọi
        /// </summary>
        public void Relayout()
        {
            Debug.Log("Relayout Box");
            // Lấy tất cả các child active (box + unlock-button), sau thêm hộp cầu vồng thì cũng thêm vào list như này
            var boxes = boxContainerTransform
                .Cast<Transform>()
                .Where(t => t.gameObject.activeSelf)
                .ToList();

            int n = boxes.Count;
            float half = (n - 1) * spacing * 0.5f;

            for (int i = 0; i < n; i++)
            {
                var t = boxes[i];
                float targetX = -half + i * spacing;

                t.DOKill();
                t.DOLocalMoveX(targetX, 0.25f)
                    .SetEase(Ease.OutQuad);
            }
        }

        public void SetBoxesVisible(bool value)
        {
            boxContainerTransform.gameObject.SetActive(value);
        }

        public IEnumerator AnimateBoxesFlyIn(
            float duration = 0.5f,
            float waveDelay = 0f,
            float offScreenX = 5f
        )
        {
            // 1) Lấy danh sách các spawnPoint đang hiển thị và có ít nhất 1 child (box hoặc unlock button)
            var visibleSPs = spawnPoints
                .Where(sp => sp.point.gameObject.activeSelf && sp.point.childCount > 0)
                .ToList();
            int n = visibleSPs.Count;
            if (n == 0) yield break;

            // 2) Đẩy tất cả các box ra ngoài màn hình
            for (int i = 0; i < n; i++)
            {
                var t = visibleSPs[i].point.GetChild(0);
                Vector3 dir = (i < n / 2) ? Vector3.left : Vector3.right;
                t.position = visibleSPs[i].point.position + dir * offScreenX;
            }

            // 3) Hiện container (đã Relayout trước đó rồi)
            SetBoxesVisible(true);

            // 4) Chia làm 2 wave: 2 BOX giữa trước, còn lại sau
            int mid = n / 2 - 1;
            var firstWave = visibleSPs.Skip(mid).Take(2).ToList();
            var secondWave = visibleSPs.Except(firstWave).ToList();

            // Helper cho fly-in
            void FlyIn(Transform boxT, Vector3 target)
            {
                boxT.DOMove(target, duration).SetEase(Ease.InOutSine);
            }

            bool hasSecondWave = secondWave.Any();
            if (!hasSecondWave) waveDelay = 0;

            // 5) Wave 1
            foreach (var sp in firstWave)
                FlyIn(sp.point.GetChild(0), sp.point.position);
            yield return new WaitForSeconds(duration + waveDelay);

            // 6) Wave 2
            if (hasSecondWave)
            {
                foreach (var sp in secondWave)
                    FlyIn(sp.point.GetChild(0), sp.point.position);
                yield return new WaitForSeconds(duration);
            }
        }

        // lấy box đầu tiên bên trái, nếu các slot đã fill hết thì lấy box tiếp theo
        public Box GetFirstBox()
        {
            foreach (var sp in spawnPoints)
            {
                // nếu spawn point còn active
                if (sp.point.gameObject.activeSelf)
                {
                    var box = sp.point.GetComponentInChildren<Box>(true);
                    if (box != null && box.gameObject.activeInHierarchy && box.HasAvailableSlot())
                        return box;
                }
            }

            return null;
        }

        public bool IsBoxMoving(Box box)
        {
            return DOTween.IsTweening(box.transform.parent);
        }

        public void PrintImage(Box boxInfo)
        {
            if (boxInfo == null)
                return;

            // Box None là box lỗi (thường do 1 màu lẻ, không chia hết cho 3) — bỏ qua tô ảnh/dây
            // để không tô sai ô, box sẽ tự đóng ở AutoCloseEmptyBox.
            if (boxInfo.BoxColor == YarnColor.None)
            {
                Debug.LogError("PrintImage: BoxColor=None (thiếu màu — kiểm tra config: mỗi màu phải chia hết cho 3!) — bỏ qua tô ảnh.", boxInfo);
                currentWaitOfTime = 0.3f;
                return;
            }

            // Validate: 3 sợi trong box phải cùng màu logic với BoxColor,
            // nếu không ảnh sẽ tô sai ô.
            var filledColors = boxInfo.Slots
                .Where(s => s != null && s.IsFilled)
                .Select(s => s.FilledColor)
                .Distinct()
                .ToList();
            if (filledColors.Count > 1)
            {
                Debug.LogError($"PrintImage: box lẫn {filledColors.Count} màu [{string.Join(",", filledColors)}] nhưng BoxColor={boxInfo.BoxColor} — ảnh sẽ tô sai!", boxInfo);
            }
            else if (filledColors.Count == 1 && filledColors[0] != boxInfo.BoxColor
                && boxInfo.BoxColor != YarnColor.Rainbow)
            {
                Debug.LogError($"PrintImage: slot màu {filledColors[0]} nhưng BoxColor={boxInfo.BoxColor} — ảnh sẽ tô sai ô!", boxInfo);
            }
            if (boxInfo.BoxColor == YarnColor.Rainbow)
            {
                Debug.LogError("PrintImage: BoxColor=Rainbow không có dữ liệu bake ảnh — kiểm tra luồng Magic Box!", boxInfo);
            }

            // Match ref: each completed box reveals only its next pixel batch.
            var image = JamImage.Instance;
            var pixelCount = image?.GetNextPixelCount(boxInfo.BoxColor) ?? 0;
            if (image != null && pixelCount > 0)
            {
                currentWaitOfTime = GetPrintTime(pixelCount);
                image.Print(boxInfo.BoxColor, currentWaitOfTime / pixelCount);
            }
            else
            {
                _Game.Scripts.LevelDebugLog.LogError($"PrintImage: màu {boxInfo.BoxColor} không lấy được ô ảnh nào (thiếu texture Resources/Images hoặc lệch colorCountEntries)!");
                Debug.LogError($"PrintImage: màu {boxInfo.BoxColor} không lấy được ô ảnh nào (thiếu texture Resources/Images hoặc lệch colorCountEntries)!", boxInfo);
                currentWaitOfTime = 1f;
            }

            if (YarnRopeController.Instance == null || pixelCount == 0)
                return;

            // The rope follows the board pointer as each yarn pixel is revealed.
            var target = image != null
                ? image.Pointer
                : (boardTarget != null ? boardTarget : GameObject.Find("Board")?.transform);
            var start = boxInfo.GetTargetRope();
            if (target == null || start == null)
            {
                Debug.LogWarning("Cannot send box rope to board: missing target.");
                return;
            }

            YarnRopeController.Instance.SpawnRopeBoard(start, target, boxInfo.BoxColor, currentWaitOfTime);
        }

        private float GetPrintTime(int pixelCount)
        {
            foreach (var data in printTimeData)
            {
                if (data.range.x <= pixelCount && data.range.y > pixelCount)
                    return data.time;
            }

            return 1f;
        }

        public static void StopLoopPrintSfx() { }
    }
}
