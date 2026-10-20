using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _Game.Scripts.Tutorial;
using _KIT;
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
        [SerializeField] private Transform addButton;
        [SerializeField] private float spacing = 0.52f;
        [SerializeField] private Slot waitingSlotPrefab;
        [SerializeField] private GameObject vfxUnlockSlot;

        private int maxSlotCount;
        private int initialSlotCount;
        private float lastTimeThreadSfx;

        private List<MeshRenderer> fadeRenderers = new List<MeshRenderer>();  // tất cả MeshRenderer cần fade

        // reuse 1 block chung
        private MaterialPropertyBlock propBlock;

        private AddWaitingSlotButton addWaitingSlotButton;

        private void Awake()
        {
            Instance = this;

            maxSlotCount = GameManager.Instance.GameConfig.maxWaitingSlots;
            Debug.Log("max slot count = " + maxSlotCount);
            initialSlotCount = GameManager.Instance.GameConfig.initialSlotCount;

            propBlock = new MaterialPropertyBlock();
            addWaitingSlotButton = addButton != null ? addButton.GetComponent<AddWaitingSlotButton>() : null;
            if (addWaitingSlotButton == null)
                Debug.LogError("[WaitingAreaManager] addButton chưa gán hoặc thiếu AddWaitingSlotButton — nút cộng slot sẽ bị ẩn, game vẫn chạy.", this);
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
                    TryFireCleanTutorial();
                    UpdateWarningVFX();
                };
            }

            // 4) add render cần fade từ slot và slot button (bỏ qua null để không NRE khi prefab/scene thiếu wiring)
            fadeRenderers.Clear();
            foreach (var slot in waitingSlots)
            {
                if (slot == null) continue;
                AddFadeRenderer(slot.GetSlotBaseRenderer(), slot.name + ".Base");
                AddFadeRenderer(slot.GetSlotSpoolRenderer(), slot.name + ".Spool");
                AddFadeRenderer(slot.GetSlotShadowRenderer(), slot.name + ".Shadow");
            }
            if (addWaitingSlotButton != null)
            {
                AddFadeRenderer(addWaitingSlotButton.GetSlotBaseRenderer(), "AddButton.Base");
                AddFadeRenderer(addWaitingSlotButton.GetPlusRenderer(), "AddButton.Plus");
            }

            // 5) refresh layout
            RefreshLayout(true);

            UpdateWarningVFX();
        }

        private void AddFadeRenderer(MeshRenderer rend, string label)
        {
            if (rend != null) fadeRenderers.Add(rend);
            else Debug.LogError($"[WaitingAreaManager] thiếu renderer {label} — fade-in sẽ bỏ qua nó, kiểm tra lại prefab.", this);
        }

        public void TryRefillAll()
        {
            foreach (var waitSlot in waitingSlots)
            {
                // Nếu waiting‑slot này đang chứa YarnPart
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
            //    Nếu không fill được (part null, hoặc màu sợi lệch màu slot hộp) thì
            //    trả slot hộp đã "giành" ở TryRefillSlot về SlotManager và dừng: giữ
            //    nguyên waiting-slot để không mất sợi len.
            if (part == null || !boxSlot.Fill(part, true))
            {
                Debug.LogWarning($"[WaitingAreaManager] Refill waiting→box thất bại (màu {color}) — " +
                                 "trả slot hộp về SlotManager để không mất slot.", this);
                SlotManager.Instance.ReleaseSlot(boxSlot);
                return;
            }

            // 2) Clear waiting-slot về state Idle
            waitSlot.UnFill(() =>
            {
                SlotManager.Instance.RegisterSlot(waitSlot);
                UpdateWarningVFX();
                CheckingLose();
            });

            // (ghost handling omitted for brevity)
            YarnRopeController.Instance.SpawnRopeBoard(waitSlot.GetAnchorTransform(), boxSlot.GetAnchorTransform(),
                part.CurrentColor, waitSlot.RingDisappearClip.length);

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

            // add button handling (if any) – kept false in this implementation
            bool canAdd = false;
            if (addButton != null) addButton.gameObject.SetActive(canAdd);
            if (canAdd)
            {
                // omitted for brevity
            }
        }

        public void AddSlot()
        {
            Debug.Log("before add slot - wait slot count = " + waitingSlots.Count);
            if (HasReachMaxSlot())
                return;

            var slot = Instantiate(waitingSlotPrefab, waitingSlotContainer);
            waitingSlots.Add(slot);
            slot.Initialize(YarnColor.None, null);
            SlotManager.Instance.RegisterSlot(slot);

            slot.OnFilled += _ =>
            {
                CheckingLose();
                UpdateWarningVFX();
            };

            // thêm vfx unlock slot (bỏ qua nếu chưa gán trong scene)
            if (vfxUnlockSlot != null)
            {
                var vfx = Instantiate(vfxUnlockSlot, slot.transform);
                Destroy(vfx, 2f);
            }

            RefreshLayout(false);
            UpdateWarningVFX();
        }

        public bool IsAllWaitingSlotsFilled()
        {
            return waitingSlots.All(s => s.IsFilled);
        }

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
                propBlock.SetFloat(ProgressOpacity, 0f);
                rend.SetPropertyBlock(propBlock);
            }
        }

        public void SetWaitingContainerVisible(bool value)
        {
            waitingSlotContainer.gameObject.SetActive(value);
        }

        public IEnumerator FadeIn(float fadeDuration = 0.5f)
        {
            SetWaitingContainerVisible(true);
            float value = 0f;
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
            ).SetEase(Ease.InOutSine).WaitForCompletion();
        }

        private void UpdateWarningVFX()
        {
            Debug.Log("Update Warning VFX");
            foreach (var slot in waitingSlots)
                if (slot.WarningVFX != null)
                    slot.WarningVFX.SetActive(false);

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

        private void TryFireCleanTutorial()
        {
            // Stub: tutorial Clean booster thuộc meta-game, tắt trong bản core này.
        }

        public int GetFilledSlotCount()
        {
            return waitingSlots.Count(s => s.IsFilled);
        }

        private void CheckingLose()
        {
            GameManager.Instance.SetCheckingLose(IsAllWaitingSlotsFilled());
        }
    }
}
