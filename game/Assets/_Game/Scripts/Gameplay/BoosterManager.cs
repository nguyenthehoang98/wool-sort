using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _KIT;
using _KIT.Utils;
using DG.Tweening;
using UnityEngine;
using Ease = _KIT.Ease;

namespace _Game.Scripts.Gameplay
{
    public class BoosterManager : MonoBehaviour
    {
        public static BoosterManager Instance { get; private set; }

        [Header("Canvas & Target UI")]
        [Tooltip("Camera được gán làm Render Camera cho Canvas Screen-Space-Camera")]
        [SerializeField] private Camera boxCamera;
        [Tooltip("RectTransform của TempBox trong Canvas Screen-Space-Camera")]
        [SerializeField] private RectTransform tempBoxContainer;

        [Header("Spine Sweep")]
        [SerializeField] private BroomSweep broomSweep;
        [SerializeField] private float postSweepDelay = 0.1f;

        private bool isLockByUsingBooster;
        public bool IsLockByUsingBooster => isLockByUsingBooster;
        private Vector3 _tempBoxOriginalPos;

        // Danh sách giữ các piece tạm
        private readonly List<YarnPart> _tempParts = new List<YarnPart>();
        private readonly List<YarnGhost> _activeGhosts = new List<YarnGhost>();

        public Transform TempBoxContainer => tempBoxContainer;

        private void Awake()
        {
            Instance = this;

            if (tempBoxContainer != null) tempBoxContainer.gameObject.SetActive(false);
            else Debug.LogError("[BoosterManager] tempBoxContainer chưa gán — booster Clean sẽ chạy rút gọn (không hiện TempBox).", this);
            if (boxCamera == null)
                Debug.LogError("[BoosterManager] boxCamera chưa gán — Clean sẽ bay thẳng vào vị trí, kiểm tra lại scene.", this);
        }

        public void Reset()
        {
            isLockByUsingBooster = false;
            _tempParts.Clear();

            if (tempBoxContainer != null) tempBoxContainer.gameObject.SetActive(false);
            if (broomSweep != null) broomSweep.Show(false);
        }

        public void SetTempBoxOriginalPosition()
        {
            if (tempBoxContainer != null) _tempBoxOriginalPos = tempBoxContainer.position;
        }

        public bool CanUseBooster(BoosterType type, bool needShowToolTip = true)
        {
            switch(type)
            {
                case BoosterType.AddSlot:
                    bool flag = WaitingAreaManager.Instance.HasReachMaxSlot();
                    if(flag)
                    {
                        if(needShowToolTip)
                        {
                            KitTooltipManager.Instance.Show("No more slots available");
                        }
                    }

                    return !flag;
                case BoosterType.Clean:
                    bool flag2 = WaitingAreaManager.Instance.IsAllEmpty();
                    if(flag2)
                    {
                        if(needShowToolTip)
                        {
                            KitTooltipManager.Instance.Show("Waiting area is empty");
                        }
                    }
                    return !flag2;
                case BoosterType.MagicBox:
                    // // điều kiện sử dụng: đã sinh hết box nên không thể sinh thêm box thay thế được
                    // bool flag3 = BoxManager.Instance.HasSpawnLastBox();
                    // if (flag3)
                    // {
                    //     KitTooltipManager.Instance.Show("There are no box left to spawn");
                    // }
                    // //
                    // // bool flag4 = BoxManager.Instance.HasMagicBox();
                    // // if (flag4)
                    // // {
                    // //     KitTooltipManager.Instance.Show("A magic box is already active!");
                    // // }
                    //
                    return true;
                default:
                    return false;
            }
        }

        public void UseBooster(BoosterType type, bool isRevive = false)
        {
            if(isLockByUsingBooster)
                return;

            switch(type)
            {
                case BoosterType.AddSlot:
                    UseBoosterAddSlot();
                    break;
                case BoosterType.Clean:
                    UseBoosterClean();
                    break;
                case BoosterType.MagicBox:
                    UseBoosterMagicBox();
                    break;
            }
        }

        private void UseBoosterAddSlot()
        {
            WaitingAreaManager.Instance.AddSlot();
        }

        private void UseBoosterClean()
        {
            // set biến này để check khóa click vào các part và dùng booster khác cho đến khi dùng xong booster
            // tương đương với các cuộn len anim xong đến box tạm
            isLockByUsingBooster = true;

            // Bắt đầu chơi broom animation ngay
            if(broomSweep != null)
            {
                broomSweep.Show(true);
                broomSweep.PlaySweep(() =>
                {
                    broomSweep.Show(false);
                });
            }

            GameUIController.Instance.ShiftNoAdsForTempBox();
            WaitingAreaManager.Instance.ForceHideWarning();
            StartCoroutine(DelayedThenClean());
        }

        private IEnumerator DelayedThenClean()
        {
            yield return new WaitForSeconds(postSweepDelay);

            // Hiển thị Temp-Box
            ShowTempBoxEntry(() =>
            {
                SpawnAllGhosts();
                AnimateAllGhosts(OnCleanComplete);
            });
        }

        private void SpawnAllGhosts()
        {
            _activeGhosts.Clear();

            // 1) Clear tất cả slot đang chứa yarn trên waiting area
            var waitingSlots = WaitingAreaManager.Instance.GetWaitingSlots();
            foreach(var waitSlot in waitingSlots)
            {
                if(!waitSlot.IsFilled)
                    continue;
                var part = waitSlot.CurrentPart;

                // lấy trc khi clear
                var color = waitSlot.FilledColor;
                _tempParts.Add(part);

                waitSlot.Clear();
                SlotManager.Instance.RegisterSlot(waitSlot);

                // tạo ghost qua service, gom logic màu/sizing/parent bên trong
                var ghost = YarnGhostService.Instance.Create(
                    waitSlot.transform.position,
                    waitSlot.transform.rotation,
                    waitSlot.transform.lossyScale,
                    color);

                _activeGhosts.Add(ghost);
            }
        }

        private void AnimateAllGhosts(Action onAllComplete)
        {
            int remaining = _activeGhosts.Count;
            if(remaining == 0)
            {
                onAllComplete?.Invoke();
                return;
            }

            foreach(var ghost in _activeGhosts)
            {
                YarnGhostService.Instance.FlyToTempBox(
                    ghost,
                    tempBoxContainer != null ? tempBoxContainer.position : ghost != null ? ghost.transform.position : Vector3.zero,
                    boxCamera,
                    () =>
                    {
                        if(--remaining == 0)
                            onAllComplete?.Invoke();
                    });
            }

            _activeGhosts.Clear();
        }

        private void ShowTempBoxEntry(Action onComplete)
        {
            // Thiếu wiring scene thì bỏ qua animation, vẫn chạy logic Clean.
            if (tempBoxContainer == null || boxCamera == null)
            {
                onComplete?.Invoke();
                return;
            }
            tempBoxContainer.gameObject.SetActive(true);
            // start ở ngoài màn trái
            Vector3 startPos = _tempBoxOriginalPos + boxCamera.transform.right * 2f;
            tempBoxContainer.position = startPos;

            // bay vào vị trí gốc
            tempBoxContainer
                .DOMove(_tempBoxOriginalPos, 0.4f)
                .SetEase(DG.Tweening.Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void OnCleanComplete()
        {
            isLockByUsingBooster = false;
        }

        private void OnSpawnMagicBoxComplete()
        {
            isLockByUsingBooster = false;
        }

        private void UseBoosterMagicBox()
        {
            Debug.Log("UseBoosterMagicBox");
            isLockByUsingBooster = true;

            // var magic = BoxManager.Instance.SpawnMagicBox(OnSpawnMagicBoxComplete);
            //
            // GlobalBus.Subscribe(this);

            // 1) Lấy box đầu tiên trong queue
            var box = BoxManager.Instance.GetFirstBox();
            if(box == null)
            {
                isLockByUsingBooster = false;
                return;
            }

            var originalColor = box.BoxColor;
            Debug.Log("First Box Color = " + originalColor);
            int missing = box.RequiredCount;
            Debug.Log("Magic box - cần lấy " + missing + " yarn part");

            // 2) Chuyển visual sang rainbow
            box.PlayChangeAnimation();

            this.WaitInvoke(0.7f, box.SetRainbowColor);

            // 3) chờ ChangeDuration rồi hút piece
            StartCoroutine(ProcessMagicBoxAfterAnim(box, originalColor, missing));

            // Đối với Magic Box, sau khi sử dụng và hoàn thành box hiện tại, 02 box spawn tiếp theo có màu tổng Priority nhỏ nhất
            BoxManager.Instance.AddScheduleMinPrioritySpawn(2);
        }

        private IEnumerator ProcessMagicBoxAfterAnim(Box box, YarnColor originalColor, int missing)
        {
            // chờ cho animation BasketChange chạy xong
            yield return new WaitForSeconds(box.ChangeDuration);

            for(int i = 0; i < missing; i++)
            {
                var infos = GameManager.Instance.GetNextInfosForMagicBox(originalColor, 1, out int miss);
                if(infos.Count == 0)
                    break;
                var info = infos[0];

                YarnPart part;
                if(!GameManager.Instance.TryGetPartByInfo(info, out part))
                    part = YarnPartLayerSpawner.Instance.SpawnForInfo(info);

                // tránh game bị stuck, vì có case không tìm được root info của info và không create được part
                if(part == null)
                {
                    isLockByUsingBooster = false;
                    yield break;
                }

                // Chỉ lấy slot ở box first đã chọn, không tìm ở box khác
                var slot = SlotManager.Instance.GetNextAvailableSlotInBox(originalColor, box);
                if(slot == null)
                    continue;

                // TODO - cân nhắc là sinh mọi trường hợp hay chỉ priority <= 2 là đủ
                if(info.Priority <= 2)
                {
                    YarnPartLayerSpawner.Instance.SpawnInnerOnClick(part);
                }

                GameManager.Instance.UpdateCountYarnRemain();
                part.ForceDissolve(info.Priority == 1);
                slot.Fill(part);
                YarnRopeController.Instance.SpawnRope(part, slot);
                GameManager.Instance.ConsumeYarnPart(part);
            }

            isLockByUsingBooster = false;
        }

        public List<YarnPart> GetTempParts() => _tempParts;
        public void RemoveTempPart(YarnPart p)
        {
            _tempParts.Remove(p);
            if (_tempParts.Count <= 0)
            {
                StartCoroutine(HideTempBoxContainer(1f));
            }
        }

        //Courtine delay xu ly hide tempBoxContainer
        public IEnumerator HideTempBoxContainer(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (tempBoxContainer != null) tempBoxContainer.gameObject.SetActive(false);
            GameUIController.Instance.ResetNoAdsButtonPosition();
        }

        public Vector3 tempBoxPosition => tempBoxContainer != null ? tempBoxContainer.position : Vector3.zero;

        public void InitPositionBroomSweep(float y)
        {
            if (broomSweep == null) return;
            broomSweep.transform.position = new Vector3(broomSweep.transform.position.x, y, broomSweep.transform.position.z);
        }

        // old code magic box
        //     public void React(in YarnClickedMessage e)
        //     {
        //         GlobalBus.UnSubscribe(this);
        //         
        //         isLockByUsingBooster = true;
        //         
        //         Debug.Log("Booster Manager | Magic Box | Xử lý khi tap 1 part");
        //         var clickedColor = e.YarnPart.CurrentColor;
        //         
        //         // lấy được màu rồi => tìm 2 yarn có màu này để xử lý
        //         // ưu tiên lấy từ ngoài vào trong trên model, sau đó mới lấy ở box
        //         int missing;
        //         var infos = GameManager.Instance
        //             .GetNextInfosForMagicBox(clickedColor, 2, out missing);
        //         
        //         // debug ngay tại chỗ
        //         var layers = infos.Select(i => i.LayerIndex);
        //         Debug.Log($"[MagicBox] Tìm được {infos.Count} info, layerIndex = {string.Join(", ", layers)}; thiếu = {missing}");
        //         
        //         
        //         var extraParts = infos.Select(info =>
        //         {
        //             if (!GameManager.Instance.TryGetPartByInfo(info, out var p))
        //                 p = YarnPartLayerSpawner.Instance.SpawnForInfo(info);
        //             return p;
        //         }).ToList();
        //         
        //         // 3) Lauch coroutine để xử lý tuần tự
        //         StartCoroutine(ProcessMagicExtras(infos, extraParts, clickedColor, missing));
        //     }
        //
        //     private IEnumerator ProcessMagicExtras(List<YarnPartInfo> infos, List<YarnPart> parts, YarnColor color, int missing)
        //     {
        //         float delay = UnityEngine.Random.Range(0.1f, 0.2f);
        //
        //         // xử lý từng part
        //         for (int i = 0; i < parts.Count; i++)
        //         {
        //             var part = parts[i];
        //             var info = infos[i];
        //             
        //             yield return new WaitForSeconds(delay);
        //             var slot = SlotManager.Instance.GetNextAvailableSlot(color, false);
        //             if (slot == null) continue;
        //
        //             // nếu part này đang ở lớp ngoài cùng, thì cần show part bên trong khi nó biến mất
        //             if (info.Priority == 1)
        //             {
        //                 part.ShowChildPart(true);
        //             }
        //             
        //             part.ForceDissolve();
        //
        //             slot.Fill(part);
        //             YarnRopeController.Instance.SpawnRope(part, slot);
        //             
        //             GameManager.Instance.ConsumeYarnPart(part);
        //         }
        //
        //         // // cuối cùng, nếu còn thiếu thì fallback
        //         // if (missing > 0)
        //         // {
        //         //     yield return new WaitForSeconds(delay);
        //         //     WaitingAreaManager.Instance.HandleMagicFallback(color, missing);
        //         // }
        //
        //         isLockByUsingBooster = false;
        //     }
        //
    }
}
