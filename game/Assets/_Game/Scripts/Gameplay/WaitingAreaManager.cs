using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using Ease = DG.Tweening.Ease;

namespace _Game.Scripts.Gameplay
{
    public class WaitingAreaManager : MonoBehaviour
    {
        public static WaitingAreaManager Instance { get; private set; }

        [SerializeField] private Transform waitingSlotContainer;
        [SerializeField] private List<Slot> waitingSlots;
        [SerializeField] private float spacing = 0.52f;
        [SerializeField] private Slot waitingSlotPrefab;

        private int maxSlotCount;
        private int initialSlotCount;
        private float lastTimeThreadSfx;

        private List<MeshRenderer> fadeRenderers = new List<MeshRenderer>();  // tất cả MeshRenderer cần fade

        // reuse 1 block chung
        private MaterialPropertyBlock propBlock;


        private void Awake()
        {
            Instance = this;

            maxSlotCount = GameManager.Instance.GameConfig.maxWaitingSlots;
            Debug.Log("max slot count = " + maxSlotCount);
            initialSlotCount = GameManager.Instance.GameConfig.initialSlotCount;

            propBlock = new MaterialPropertyBlock();
        }

        public void Initialize()
        {
            // 1) Dừng mọi Coroutine (nếu có)
            StopAllCoroutines();

            // 2) Unregister & destroy tất cả slot cũ
            SlotManager.Instance.UnregisterSlots(waitingSlots);
            foreach (var s in waitingSlots)
                Destroy(s.gameObject);
            waitingSlots.Clear();

            // 3) Tạo lại đúng initialSlotCount slot, do tạo lại nên không cần reset các callback của slot
            for (int i = 0; i < initialSlotCount; i++)
            {
                var slot = Instantiate(waitingSlotPrefab, waitingSlotContainer);
                slot.Initialize(YarnColor.None, null);
                waitingSlots.Add(slot);
                SlotManager.Instance.RegisterSlot(slot);
            }

            foreach (var slot in waitingSlots)
            {
                slot.OnFilled += _ =>
                {
                    CheckingLose();
                    UpdateWarningVFX();
                };
            }

            // 4) add render cần fade từ slot và slot button
            fadeRenderers.Clear();
            foreach (var slot in waitingSlots)
            {
                fadeRenderers.Add(slot.GetSlotBaseRenderer());
                fadeRenderers.Add(slot.GetSlotSpoolRenderer());
                fadeRenderers.Add(slot.GetSlotShadowRenderer());
            }

            // 5) refresh layout
            RefreshLayout(true);

            UpdateWarningVFX();
        }

        public void TryRefillAll()
        {
            foreach (var waitSlot in waitingSlots)
            {
                // Nếu waiting‐slot này đang chứa YarnPart
                // chỉ xử lý nếu đã chạy xong animation fill, chứ không xử lý khi anim đang diễn
                TryRefillSlot(waitSlot);
            }
        }

        public void TryRefillSlot(Slot checkSlot)
        {
            if (checkSlot.IsFilled && checkSlot.IsAnimationFilled)
            {
                var yc = checkSlot.FilledColor;
                // Nếu box có slot phù hợp
                // Lấy slot từ box
                var boxSlot = SlotManager.Instance.GetNextAvailableSlot(yc, false);
                if (boxSlot != null)
                {
                    // animate ghost jump và fill
                    AutoRefillCoroutine(checkSlot, boxSlot, yc);
                }
            }
        }

        private void AutoRefillCoroutine(Slot waitSlot, Slot boxSlot, YarnColor color)
        {
            // 0) Lấy YarnPart để logic
            var part = waitSlot.CurrentPart;

            // 1) Chạy logic fill ngay, mọi đếm, refill, win/lose v.v.
            if (part == null || !boxSlot.Fill(part, true)) return;

            // 2) Clear waiting-slot về state Idle
            waitSlot.UnFill(() =>
            {
                SlotManager.Instance.RegisterSlot(waitSlot);

                UpdateWarningVFX();
                CheckingLose();
            });

            //var ghost = YarnGhostService.Instance.Create(
            //    waitSlot.transform.position,
            //    waitSlot.transform.rotation,
            //    waitSlot.transform.lossyScale,
            //    color);
            //YarnGhostService.Instance.TryJumpToSlot(ghost, boxSlot, () =>
            //{
            //    // sau khi jump xong mới show full
            //    boxSlot.ShowFullStateVisual();
            //});

            float ropeTime = 0.5f;
            if (waitSlot.RingDisappearClip != null)
                ropeTime = waitSlot.RingDisappearClip.length;
            else
                Debug.LogError($"[Refill] {waitSlot.name}: RingDisappearClip null nên dây waiting->box bay {ropeTime}s mặc định (thay vì đúng nhịp animation).", waitSlot);
            Debug.Log($"[Refill] Bắn dây {waitSlot.name} -> {boxSlot.name} màu {part.CurrentColor} trong {ropeTime}s.");
            YarnRopeController.Instance.SpawnRopeBoard(waitSlot.GetAnchorTransform(), boxSlot.GetAnchorTransform()
                , part.CurrentColor, ropeTime);

            if (Time.time - lastTimeThreadSfx > 0.1f) // check multiple play sfx
            {
                lastTimeThreadSfx = Time.time;
            }

            UpdateWarningVFX();
            CheckingLose();
        }

        private void RefreshLayout(bool forInit)
        {
            int total = waitingSlots.Count;
            float startX = -(total - 1) * spacing * 0.5f;
            float duration = 0.25f;

            for (int i = 0; i < waitingSlots.Count; i++)
            {
                var slot = waitingSlots[i].transform;
                Vector3 targetPos = new Vector3(startX + i * spacing, 0, 0);

                if (forInit)
                {
                    waitingSlots[i].transform.localPosition = targetPos;
                }
                else
                {
                    // Nếu đây là slot mới (là phần tử cuối trong danh sách), set ngay vị trí
                    if (i == waitingSlots.Count - 1)
                    {
                        slot.DOKill();
                        slot.localPosition = targetPos;
                    }
                    else
                    {
                        slot.DOKill();
                        slot.DOLocalMove(targetPos, duration).SetEase(DG.Tweening.Ease.OutQuad);
                    }
                }
            }
        }


        // check “vùng chờ đã đầy và tất cả slot đều filled”
        public bool IsAllWaitingSlotsFilled()
        {
            return waitingSlots.All(s => s.IsFilled);
        }


        // check “vùng chờ tất cả slot đều trống"
        public bool IsAllEmpty()
        {
            return waitingSlots.All(s => !s.IsFilled);
        }

        public bool HasReachMaxSlot()
        {
            return waitingSlots.Count >= maxSlotCount;
        }

        public List<Slot> GetWaitingSlots()
        {
            return waitingSlots;
        }

        static readonly int ProgressOpacity = Shader.PropertyToID("_Opacity");
        public void SetWaitingSlotsVisible(bool value)
        {
            foreach (var rend in fadeRenderers)
            {
                rend.GetPropertyBlock(propBlock);
                // thiết lập opacity xuống 0
                propBlock.SetFloat(ProgressOpacity, 0f);
                // nếu shader có nhiều material, override này áp dụng cho cả materials
                rend.SetPropertyBlock(propBlock);
            }
        }

        public void SetWaitingContainerVisible(bool value)
        {
            waitingSlotContainer.gameObject.SetActive(value);
        }

        // <summary>
        /// Fade toàn bộ các mesh từ opacity=0 → 1 trong fadeDuration.
        /// MaterialPropertyBlock sẽ áp dụng cho mọi material trên mỗi renderer.
        /// </summary>
        public IEnumerator FadeIn(float fadeDuration = 0.5f)
        {
            SetWaitingContainerVisible(true);
            float value = 0f;

            // Tween từ 0 → 1, mỗi frame set block
            yield return DOTween.To(
                    () => value,
                    v =>
                    {
                        value = v;
                        foreach (var rend in fadeRenderers)
                        {
                            rend.GetPropertyBlock(propBlock);
                            propBlock.SetFloat(ProgressOpacity, value);
                            rend.SetPropertyBlock(propBlock);
                        }
                    },
                    1f,
                    fadeDuration
                )
                .SetEase(Ease.InOutSine)
                .WaitForCompletion();
        }

        private void UpdateWarningVFX()
        {
            Debug.Log("Update Warning VFX");

            // 1) tắt hết
            foreach (var slot in waitingSlots)
                if (slot.WarningVFX != null)
                    slot.WarningVFX.SetActive(false);

            // 2) chỉ bật khi đúng 1 slot trống
            var free = waitingSlots.Where(s => !s.IsFilled).ToList();
            Debug.Log("free count = " + free.Count);

            if (free.Count == 1 && free[0].WarningVFX != null)
            {
                free[0].WarningVFX.SetActive(true);
                GameUIController.Instance.PlayWarningLose(true);
            }
            else
            {
                GameUIController.Instance.PlayWarningLose(false);
            }
        }

        public void ForceHideWarning()
        {
            foreach (var slot in waitingSlots)
                if (slot.WarningVFX != null)
                    slot.WarningVFX.SetActive(false);

            GameUIController.Instance.PlayWarningLose(false);
        }

        /// <summary>
        /// Trả về số lượng slot trong waitingSlots đang ở trạng thái filled.
        /// </summary>
        public int GetFilledSlotCount()
        {
            return waitingSlots.Count(s => s.IsFilled);
        }

        // khi slot waiting được filled hoặc được refill vào box thì check lại (không kiểm tra khi add slot booster)
        private void CheckingLose()
        {
            GameManager.Instance.SetCheckingLose(IsAllWaitingSlotsFilled());
        }
    }
}
