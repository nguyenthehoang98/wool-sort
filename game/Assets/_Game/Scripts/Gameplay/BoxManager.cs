using _Game.Scripts.Pool;
using _KIT.Utils;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _KIT;
using UnityEngine;
using Ease = DG.Tweening.Ease;

namespace _Game.Scripts.Gameplay
{
    public class BoxManager : MonoBehaviour
    {
        public static BoxManager Instance { get; private set; }

        [SerializeField] private Transform boxContainerTransform;
        [Tooltip("Khoảng cách giữa các box")] public float spacing = 1.2f;
        [SerializeField] private Box boxPrefab;
        [SerializeField] private GameObject unlockBtnPrefab;
        [SerializeField] private Box magicBoxPrefab;
        [SerializeField] private BoxSpawnPoint[] spawnPoints;
        [SerializeField] private SlotManager slotManager;
        [SerializeField] private WaitingAreaManager waitingArea;
        [SerializeField] private GameObject vfxUnlockBox;

        // fire khi box cuối cùng out‑anim xong và không spawn thêm
        public event Action OnAllBoxesClosed;

        private GameManager gm;
        private int nextUnlockId;
        private int spawnTotalCount; // đã bao nhiêu box được spawn
        private int maxBoxes; // giới hạn tổng box

        private readonly List<Box> activeBoxes = new List<Box>();

        // Count of boxes that have finished out‑animation
        private int closeCount;

        // Ensure OnAllBoxesClosed fires only once
        private bool hasFiredAllClosed;
        public bool HasFiredAllBoxesClosed => hasFiredAllClosed;

        private Box _currentMagicBox;
        private float maxStartDelay = 0.2f;
        // số box đang trong quá trình full->close→spawn
        private int processingBoxCount = 0;
        public int ProcessingBoxCount => processingBoxCount;

        private int printtingPropress;

        // có đang có ít nhất 1 box đang xử lý không
        public bool IsProcessingBox => processingBoxCount > 0;

        // event chỉ fire khi TẤT CẢ các box hiện tại đang xử lý đã xong
        public event Action OnAllCurentBoxProcessingComplete;

        // số lượt spawn kế tiếp sẽ ép chọn box có tổng min‑priority (sau khi dùng booster)
        // Revive: sau khi lấy 1 box theo màu của hàng đợi, thì set giá trị này = 1
        // AddBox: box vừa add và box tiếp theo => set = 2 (trước khi add box)
        // MagicBox: 2 box tiếp theo sau box magic
        private int _minOverrideCount = 0;

        private float currentWaitOfTime;
        
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
            StopAllCoroutines();
            foreach (Transform p in spawnPoints.Select(sp => sp.point))
            {
                foreach (var child in p.Cast<Transform>().ToList())
                {
                    var b = child.GetComponent<Box>();
                    if (b != null) b.Despawn();
                    else Destroy(child.gameObject);
                }
            }
            activeBoxes.Clear();
            _currentMagicBox = null;
            closeCount = 0;
            spawnTotalCount = 0;
            hasFiredAllClosed = false;
            processingBoxCount = 0;
            _minOverrideCount = 0;
            gm = gameManager;
            this.maxBoxes = maxBoxes;
            this.nextUnlockId = initialUnlock;
            for (int i = 0; i < spawnPoints.Length; i++)
                spawnPoints[i].unlocked = (i < initialUnlock);
            foreach (var sp in spawnPoints)
                sp.point.gameObject.SetActive(true);
        }

        public void SpawnAllAtStart()
        {
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                if (spawnTotalCount >= maxBoxes) break;
                if (spawnPoints[i].unlocked)
                    SpawnInitialBox(i);
                else
                {
                    if (GameData.Instance.Level() > 1)
                        Instantiate(unlockBtnPrefab, spawnPoints[i].point);
                    else
                        spawnPoints[i].point.gameObject.SetActive(false);
                }
            }
            Relayout();
        }

        public bool CanUnlockNext() => nextUnlockId < spawnPoints.Length;

        public bool UnlockNext(UnlockBoxType unlockBoxType)
        {
            if (nextUnlockId >= spawnPoints.Length) return false;
            var sp = spawnPoints[nextUnlockId];
            sp.unlocked = true;
            var btn = sp.point.GetComponentInChildren<BoxUnlockButton>();
            if (btn) btn.gameObject.SetActive(false);
            StartCoroutine(SpawnBoxRoutine(nextUnlockId, unlockBoxType, true, false));
            nextUnlockId++;
            return true;
        }

        private void SpawnInitialBox(int idx)
        {
            spawnTotalCount++;
            var sp = spawnPoints[idx];
            var box = PoolManager.Instance.Get<Box>("box");
            box.transform.SetParent(sp.point, false);
            activeBoxes.Add(box);
            if (gm.RealCurrentLevel == 3 && idx == 0)
                box.Initialize(YarnColor.Pink);
            else
                box.Initialize(gm.ChooseNextBoxColor());
            box.SetSlotsInteractable(true);
            slotManager.RegisterSlots(box.Slots);
            System.Action<Box> onFull = null;
            onFull = b =>
            {
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
        }

        private IEnumerator SpawnBoxRoutine(int idx, UnlockBoxType unlockBoxType, bool needVfxUnlock = false, bool needProcess = true)
        {
            if (spawnTotalCount >= maxBoxes) yield break;
            spawnTotalCount++;
            Debug.Log("spawn box total count = " + spawnTotalCount);
            var sp = spawnPoints[idx];
            var box = PoolManager.Instance.Get<Box>("box");
            box.transform.SetParent(sp.point, false);
            if (needVfxUnlock)
            {
                var vfx = Instantiate(vfxUnlockBox, box.transform.GetChild(0));
                box.SetVfx(vfx);
            }
            activeBoxes.Add(box);
            var yarnColor = YarnColor.None;
            bool forceMin = _minOverrideCount > 0;
            if (forceMin)
            {
                yarnColor = gm.ChooseNextBoxColor(true);
                _minOverrideCount--;
            }
            else
            {
                switch (unlockBoxType)
                {
                    case UnlockBoxType.Normal:
                        yarnColor = gm.ChooseNextBoxColor();
                        break;
                    case UnlockBoxType.Revive:
                        yarnColor = gm.ChooseNextBoxColorRevive();
                        AddScheduleMinPrioritySpawn(1);
                        break;
                }
            }
            box.Initialize(yarnColor);
            box.SetSlotsInteractable(false);
            System.Action<Box> onFull = null;
            onFull = b =>
            {
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
            box.PlayInAnimation();
            yield return new WaitForSeconds(box.InDuration);
            slotManager.RegisterSlots(box.Slots);
            waitingArea.TryRefillAll();
            yield return StartCoroutine(RefillFromTempBox(box));
            box.SetSlotsInteractable(true);
            if (needProcess) FinishProcessingBox();
        }

        private void SpawnInitialBoxLegacy(int idx) { /* legacy omitted */ }

        private IEnumerator RefillFromTempBox(Box box)
        {
            var bm = BoosterManager.Instance;
            var tempParts = bm.GetTempParts().ToList();
            var matches = new List<(YarnPart part, Slot boxSlot)>();
            foreach (var part in tempParts)
            {
                if (part.CurrentColor != box.BoxColor) continue;
                var boxSlot = SlotManager.Instance.GetNextAvailableSlot(part.CurrentColor, false);
                if (boxSlot == null) continue;
                matches.Add((part, boxSlot));
            }
            if (matches.Count == 0) yield break;
            int remaining = matches.Count;
            foreach (var (part, boxSlot) in matches)
            {
                boxSlot.Fill(part);
                bm.RemoveTempPart(part);
                YarnRopeController.Instance.SpawnRopeBoard(BoosterManager.Instance.TempBoxContainer,
                    boxSlot.GetAnchorTransform(), part.CurrentColor, 1f);
                remaining--;
                float delay = UnityEngine.Random.Range(0f, maxStartDelay);
                yield return new WaitForSeconds(delay);
            }
            yield return new WaitUntil(() => remaining <= 0);
        }

        private void FinishProcessingBox()
        {
            processingBoxCount = Mathf.Max(0, processingBoxCount - 1);
            if (processingBoxCount == 0)
                this.WaitInvoke(0.25f, () => OnAllCurentBoxProcessingComplete?.Invoke());
        }

        public bool HasMagicBox() => _currentMagicBox != null;

        // --- Magic Box spawning (old code) omitted for brevity ---

        private IEnumerator HandleCloseRoutine(Box box, int idx)
        {
            if (box == null) yield break;
            activeBoxes.Remove(box);
            box.DoEffectDisappearAllSlot(currentWaitOfTime);
            yield return new WaitForSeconds(currentWaitOfTime);
            box.PlayOutAnimation();
            yield return new WaitForSeconds(box.OutDuration);
            this.WaitInvoke(0.1f, () => { box.Despawn(); });
            closeCount++;
            Debug.Log("close box count = " + closeCount);
            if (spawnTotalCount >= maxBoxes)
            {
                spawnPoints[idx].point.gameObject.SetActive(false);
                Relayout();
            }
            if (closeCount == maxBoxes && !hasFiredAllClosed)
            {
                hasFiredAllClosed = true;
                OnAllBoxesClosed?.Invoke();
                SetBoxesVisible(false);
                yield break;
            }
            if (spawnTotalCount < maxBoxes)
                yield return SpawnBoxRoutine(idx, UnlockBoxType.Normal);
            else
                processingBoxCount--;
        }

        public bool HasSpawnLastBox() => spawnTotalCount >= maxBoxes;
        public bool MayRelayoutBox() => spawnTotalCount >= maxBoxes - 4;

        public void Relayout()
        {
            Debug.Log("Relayout Box");
            var boxes = boxContainerTransform.Cast<Transform>().Where(t => t.gameObject.activeSelf).ToList();
            int n = boxes.Count;
            float half = (n - 1) * spacing * 0.5f;
            for (int i = 0; i < n; i++)
            {
                var t = boxes[i];
                float targetX = -half + i * spacing;
                t.DOKill();
                t.DOLocalMoveX(targetX, 0.25f).SetEase(Ease.OutQuad);
            }
        }

        public void SetBoxesVisible(bool value) => boxContainerTransform.gameObject.SetActive(value);

        public IEnumerator AnimateBoxesFlyIn(float duration = 0.5f, float waveDelay = 0f, float offScreenX = 5f)
        {
            var visibleSPs = spawnPoints.Where(sp => sp.point.gameObject.activeSelf && sp.point.childCount > 0).ToList();
            int n = visibleSPs.Count;
            if (n == 0) yield break;
            for (int i = 0; i < n; i++)
            {
                var t = visibleSPs[i].point.GetChild(0);
                Vector3 dir = (i < n / 2) ? Vector3.left : Vector3.right;
                t.position = visibleSPs[i].point.position + dir * offScreenX;
            }
            SetBoxesVisible(true);
            int mid = n / 2 - 1;
            var firstWave = visibleSPs.Skip(mid).Take(2).ToList();
            var secondWave = visibleSPs.Except(firstWave).ToList();
            void FlyIn(Transform boxT, Vector3 target) => boxT.DOMove(target, duration).SetEase(Ease.InOutSine);
            bool hasSecondWave = secondWave.Any();
            if (!hasSecondWave) waveDelay = 0;
            foreach (var sp in firstWave) FlyIn(sp.point.GetChild(0), sp.point.position);
            yield return new WaitForSeconds(duration + waveDelay);
            if (hasSecondWave)
            {
                foreach (var sp in secondWave) FlyIn(sp.point.GetChild(0), sp.point.position);
                yield return new WaitForSeconds(duration);
            }
        }

        public Box GetFirstBox()
        {
            foreach (var sp in spawnPoints)
            {
                if (sp.point.gameObject.activeSelf)
                {
                    var box = sp.point.GetComponentInChildren<Box>(true);
                    if (box != null && box.gameObject.activeInHierarchy && box.HasAvailableSlot())
                        return box;
                }
            }
            return null;
        }

        public void ScheduleMinPrioritySpawn(int count) => _minOverrideCount = count;
        public void AddScheduleMinPrioritySpawn(int added) => _minOverrideCount += added;

        public void PrintImage(Box boxInfo)
        {
            printtingPropress++;
            int pixelCount = JamImage.Instance != null ? JamImage.Instance.GetNextPixelCount(boxInfo.BoxColor) : 0;
            float time = 1;
            foreach (var data in printTimeData)
            {
                if (data.range.x <= pixelCount && data.range.y > pixelCount)
                    time = data.time;
            }
            currentWaitOfTime = time;
            float progress = pixelCount > 0 ? currentWaitOfTime / pixelCount : currentWaitOfTime;
            if (JamImage.Instance != null)
                JamImage.Instance.Print(boxInfo.BoxColor, progress);
            YarnRopeController.Instance.SpawnRopeBoard(boxInfo.GetTargetRope(), JamImage.Instance.Pointer, boxInfo.BoxColor, currentWaitOfTime);
        }

        private void StartLoopPrintSfx()
        {
        }

        public static void StopLoopPrintSfx()
        {
        }

        private List<(int x, int y)> CalculatePoint(List<(int x, int y)> listPoint)
        {
            var groupedByY = listPoint.GroupBy(p => p.y).OrderByDescending(g => g.Key).ToList();
            var result = new List<(int x, int y)>();
            bool leftToRight = true;
            foreach (var group in groupedByY)
            {
                var row = leftToRight ? group.OrderBy(p => p.x) : group.OrderByDescending(p => p.x);
                result.AddRange(row);
                leftToRight = !leftToRight;
            }
            return result;
        }

        private void CheckStopLoooSfx()
        {
            StopLoopPrintSfx();
        }
    }
}
