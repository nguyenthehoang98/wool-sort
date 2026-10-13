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

        [Header("Giỏ tạm (Clean)")]
        [Tooltip("Vị trí nghỉ của giỏ tạm. Giỏ luôn đứng ở đây khi không dùng Clean.")]
        [SerializeField] private Transform tempBoxIdleAnchor;
        [Tooltip("Vị trí giỏ ra giữ len khi Clean, thu xong sẽ quay về Idle.")]
        [SerializeField] private Transform tempBoxShowAnchor;
        [SerializeField] private float tempBoxMoveDuration = 0.4f;
        [Tooltip("Khoảng cách thời gian giữa các sợi bay vào giỏ.")]
        [SerializeField] private float ghostStagger = 0.08f;
        [Tooltip("Thời gian dây bay từ slot vào giỏ.")]
        [SerializeField] private float ropeFlyDuration = 0.5f;

        private bool isLockByUsingBooster;
        public bool IsLockByUsingBooster => isLockByUsingBooster;

        // Danh sách giữ các piece tạm
        private readonly List<YarnPart> _tempParts = new List<YarnPart>();

        public Transform TempBoxContainer => tempBoxContainer;

        private Vector3 IdlePosition => tempBoxIdleAnchor != null ? tempBoxIdleAnchor.position : tempBoxContainer.position;
        private Vector3 ShowPosition => tempBoxShowAnchor != null ? tempBoxShowAnchor.position : IdlePosition;

        private void Awake()
        {
            Instance = this;

            // Tự dựng hàng nút booster uGUI nếu scene chưa có (tránh sửa scene tay).
            if (FindObjectOfType<BoosterBarUI>() == null)
                new GameObject("BoosterBarUI").AddComponent<BoosterBarUI>();

            if (tempBoxContainer == null)
            {
                Debug.LogError("[BoosterManager] tempBoxContainer chưa gán — booster Clean sẽ chạy rút gọn (không hiện giỏ tạm).", this);
                return;
            }
            if (tempBoxIdleAnchor == null)
                Debug.LogWarning("[BoosterManager] tempBoxIdleAnchor chưa gán — giỏ tạm dùng vị trí đặt trong scene làm chỗ nghỉ.", this);
            tempBoxContainer.gameObject.SetActive(true);
            tempBoxContainer.position = IdlePosition;
            if (boxCamera == null)
                Debug.LogError("[BoosterManager] boxCamera chưa gán — Clean sẽ bay thẳng vào vị trí, kiểm tra lại scene.", this);
        }

        public void Reset()
        {
            isLockByUsingBooster = false;
            _tempParts.Clear();

            if (tempBoxContainer != null)
            {
                tempBoxContainer.DOKill();
                tempBoxContainer.position = IdlePosition;
            }
            if (broomSweep != null) broomSweep.Show(false);
        }

        public bool CanUseBooster(BoosterType type, bool needShowToolTip = true)
        {
            var waiting = WaitingAreaManager.Instance;
            if (waiting == null)
            {
                Debug.LogWarning("[BoosterManager] WaitingAreaManager chưa có trong scene, cho phép dùng để test UI.", this);
                return true;
            }
            switch(type)
            {
                case BoosterType.AddSlot:
                    bool flag = waiting.HasReachMaxSlot();
                    if(flag)
                    {
                        if(needShowToolTip)
                        {
                            ShowTooltip("No more slots available");
                        }
                    }

                    return !flag;
                case BoosterType.Clean:
                    bool flag2 = waiting.IsAllEmpty();
                    if(flag2)
                    {
                        if(needShowToolTip)
                        {
                            ShowTooltip("Waiting area is empty");
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

        private static void ShowTooltip(string message)
        {
            // KitTooltipManager thường nằm ở scene Entry/Home (DontDestroyOnLoad),
            // mở thẳng scene Gameplay thì Instance null -> fallback log, không crash.
            var tooltip = KitTooltipManager.Instance;
            if (tooltip != null)
                tooltip.Show(message);
            else
                Debug.Log($"[Booster Tooltip] {message}");
        }

        public void UseBooster(BoosterType type, bool isRevive = false)
        {            if(isLockByUsingBooster)
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
                SendWaitingYarnsToTempBox(() => ReturnTempBox(OnCleanComplete));
            });
        }

        // Mỗi sợi trên waiting area nối bằng dây (RopeBoard) bay từ slot vào giỏ, lần lượt cách nhau ghostStagger.
        private void SendWaitingYarnsToTempBox(Action onAllComplete)
        {
            if (WaitingAreaManager.Instance == null || tempBoxContainer == null)
            {
                onAllComplete?.Invoke();
                return;
            }

            var waitingSlots = WaitingAreaManager.Instance.GetWaitingSlots();
            int index = 0;
            foreach (var waitSlot in waitingSlots)
            {
                if (!waitSlot.IsFilled)
                    continue;

                var part = waitSlot.CurrentPart;
                var color = waitSlot.FilledColor;
                var fromAnchor = waitSlot.GetAnchorTransform();
                _tempParts.Add(part);

                waitSlot.Clear();
                SlotManager.Instance.RegisterSlot(waitSlot);

                float delay = index * ghostStagger;
                this.WaitInvoke(delay, () =>
                    YarnRopeController.Instance.SpawnRopeBoard(fromAnchor, tempBoxContainer, color, ropeFlyDuration));
                index++;
            }

            if (index == 0)
            {
                onAllComplete?.Invoke();
                return;
            }

            this.WaitInvoke((index - 1) * ghostStagger + ropeFlyDuration, () => onAllComplete?.Invoke());
        }

        private void ShowTempBoxEntry(Action onComplete)
        {
            if (tempBoxContainer == null)
            {
                onComplete?.Invoke();
                return;
            }
            tempBoxContainer.gameObject.SetActive(true);
            tempBoxContainer.DOKill();
            tempBoxContainer.DOMove(ShowPosition, tempBoxMoveDuration)
                .SetEase(DG.Tweening.Ease.OutBack)
                .OnComplete(() => onComplete?.Invoke());
        }

        private void ReturnTempBox(Action onComplete)
        {
            if (tempBoxContainer == null)
            {
                onComplete?.Invoke();
                return;
            }
            tempBoxContainer.DOKill();
            tempBoxContainer.DOMove(IdlePosition, tempBoxMoveDuration)
                .SetEase(DG.Tweening.Ease.InOutSine)
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

        //Courtine delay: giỏ tạm không ẩn nữa, chỉ đảm bảo đứng đúng chỗ nghỉ và báo UI
        public IEnumerator HideTempBoxContainer(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (tempBoxContainer != null && !DOTween.IsTweening(tempBoxContainer)) tempBoxContainer.position = IdlePosition;
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
