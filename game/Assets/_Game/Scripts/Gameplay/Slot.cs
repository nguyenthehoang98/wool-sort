// Reusable Slot component for both Box and WaitingArea

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using UnityEventBus;

namespace _Game.Scripts.Gameplay
{
    public class Slot : MonoBehaviour
    {
        public enum AreaType
        {
            Box,
            Waiting
        }

        public AreaType Area { get; private set; }

        [SerializeField] private Animator animator;

        [Tooltip("Clip WoodRingAppear từ Animator Controller")]
        [SerializeField] private AnimationClip ringAppearClip;
        [SerializeField] private AnimationClip ringDisappearClip;

        [SerializeField] private MeshRenderer[] allRings;
        [SerializeField] private MeshRenderer slotBaseRenderer;
        [SerializeField] private MeshRenderer slotSpoolRenderer;
        [SerializeField] private MeshRenderer slotShadowRenderer;

        [Header("Vòng co khi yarn bay đi (giữ Cylinder, chỉ co các vòng này)")]
        [SerializeField] private MeshRenderer[] shrinkRings;

        [Header("Warning VFX (Waiting-only)")]
        [SerializeField] private GameObject warningVFX;

        [Header("Target cho top anchor của rope")]
        [SerializeField] private Transform ropeTopAnchor;

        public GameObject WarningVFX => warningVFX;

        public Box ParentBox { get; private set; }
        public bool IsFilled { get; private set; }
        public YarnColor FilledColor { get; private set; }

        public bool IsAnimationFilled { get; private set; }

        /// <summary>
        /// Tham chiếu đến instance YarnPart hiện đang nằm trong slot này (nếu có).
        /// Khi slot trống, CurrentPart = null.
        /// </summary>
        public YarnPart CurrentPart { get; private set; }

        public event Action<Slot> OnFilled;
        public event Action OnAnimationFilling;

        public event Action
            OnAnimationFilled; // dùng cho wating area manager, fill đầy thì show lose & dùng để check đóng box khi slot đã chạy hết anim

        public AnimationClip RingDisappearClip => ringDisappearClip;

        // Scale gốc của các vòng co (để co vòng khi yarn bay đi rồi trả lại khi fill mới).
        // Cylinder (spool) giữ nguyên, không co.
        private readonly Dictionary<Transform, Vector3> ringInitialScales = new Dictionary<Transform, Vector3>();

        public void Initialize(YarnColor color, Box parentBox = null)
        {
            FilledColor = color;
            ParentBox = parentBox;
            Area = parentBox != null ? AreaType.Box : AreaType.Waiting;
            IsFilled = false;
            CurrentPart = null;
            IsAnimationFilled = false;
            RestoreRingsScale();

            if (color != YarnColor.None)
            {
                ApplyColor(color);
            }
        }

        private MeshRenderer[] GetShrinkRings()
        {
            if (shrinkRings != null && shrinkRings.Length > 0) return shrinkRings;
            // Fallback: dùng allRings lọc đúng SM_vong1/2/3 (bỏ vong4/base).
            if (allRings == null) return null;
            return allRings.Where(r => r != null &&
                    (r.name.Contains("vong1") || r.name.Contains("vong2") || r.name.Contains("vong3")))
                .ToArray();
        }

        /// <summary>
        /// Co các vòng nối tiếp SM_vong3 -> vong2 -> vong1 trong duration
        /// (dây bay lên + vòng thu nhỏ cùng lúc). Cylinder giữ nguyên.
        /// </summary>
        public void PlayRingsShrink(float duration)
        {
            var rings = GetShrinkRings();
            if (rings == null) return;
            var ordered = rings.Where(r => r != null).OrderByDescending(r => r.name).ToArray();
            if (ordered.Length == 0) return;
            float step = Mathf.Max(0.01f, duration) / ordered.Length;
            var seq = DOTween.Sequence();
            foreach (var r in ordered)
            {
                var t = r.transform;
                CacheRingScale(t);
                t.DOKill();
                seq.Append(t.DOScale(Vector3.zero, step).SetEase(Ease.OutQuad));
            }
        }

        /// <summary>Trả các vòng về scale gốc (khi fill mới / reuse slot).</summary>
        public void RestoreRingsScale()
        {
            var rings = GetShrinkRings();
            if (rings == null) return;
            foreach (var r in rings)
            {
                if (r == null) continue;
                var t = r.transform;
                CacheRingScale(t);
                t.DOKill();
                t.localScale = ringInitialScales[t];
            }
        }

        private void CacheRingScale(Transform t)
        {
            if (ringInitialScales.ContainsKey(t)) return;
            ringInitialScales[t] = t.localScale.sqrMagnitude > 0.0001f ? t.localScale : Vector3.one;
        }

        public bool CanAccept(YarnColor c)
        {
            if (IsFilled) return false;
            // Box None là box lỗi (không chọn được màu) — không nhận sợi nào để tránh box lẫn 3 màu.
            if (ParentBox != null && ParentBox.BoxColor == YarnColor.None) return false;
            return FilledColor == c || FilledColor == YarnColor.None ||
                   (ParentBox != null && ParentBox.BoxColor == YarnColor.Rainbow);
        }

        /// <summary>
        /// Fill this slot with the given color.
        /// </summary>
        public bool Fill(YarnPart part, bool isDoubleSpeed = false)
        {
            if (part == null) return false;

            var color = part.CurrentColor;
            if (!CanAccept(color)) return false;

            IsFilled = true;
            FilledColor = color;
            CurrentPart = part;
            RestoreRingsScale();
            ApplyColor(FilledColor);

            if (animator != null && ringAppearClip != null)
            {
                animator.Play(ringAppearClip.name);

                // Nếu là double speed thì set speed animator thành 2f
                if (isDoubleSpeed)
                {
                    animator.speed = 2f;
                }

                StartCoroutine(WaitAndTriggerBoxBounce(isDoubleSpeed));
            }
            else
            {
                // Không có animator/clip (VD prefab bị gỡ) thì hoàn thành ngay,
                // nếu không IsAnimationFilled mãi false -> refill waiting->box không bao giờ chạy.
                if (animator == null || ringAppearClip == null)
                    Debug.LogError($"[Slot] {name}: thiếu Animator/RingAppearClip nên bỏ qua animation fill (fill logic vẫn chạy).", this);
                IsAnimationFilled = true;
                OnAnimationFilled?.Invoke();
                if (Area == AreaType.Waiting)
                {
                    WaitingAreaManager.Instance.TryRefillSlot(this);
                }
            }

            // *** Raise sự kiện tương ứng ***
            // TODO - gọi xử lý ở GameManager tương ứng luôn
            if (Area == AreaType.Box)
            {
                GameManager.Instance.HandlePartBoxed(CurrentPart);
            }
            else
            {
                // Đang nằm tạm ở Waiting
                GameManager.Instance.HandlePartPicked(CurrentPart);
            }

            GlobalBus.Send<SlotFilledEvent>(new SlotFilledEvent(this));

            OnFilled?.Invoke(this);
            return true;
        }

        IEnumerator WaitAndTriggerBoxBounce(bool isDoubleSpeed)
        {
            if (ringAppearClip == null)
            {
                IsAnimationFilled = true;
                OnAnimationFilled?.Invoke();
                yield break;
            }
            float duration = ringAppearClip.length;

            if (isDoubleSpeed)
            {
                duration /= 2f; // nếu double speed thì chia đôi thời gian
            }

            yield return new WaitForSeconds(duration);

            //Reset animator speed về 1f
            if (animator != null)
            {
                animator.speed = 1f;
            }

            // Chỉ gọi OnAnimationFilling (box nhún) nếu đây không phải slot cuối cùng của một Box
            // vì hàm này đợi animation nên không check theo logic filled của box được
            if (Area == AreaType.Box && !ParentBox.IsLastBoxSlot(this))
            {
                OnAnimationFilling?.Invoke();

                yield return new WaitForSeconds(0.1f);
            }

            IsAnimationFilled = true;

            OnAnimationFilled?.Invoke();

            // khi xong animation của slot waiting thì check lại 1 lần xem có refill nhảy lên box đc ko
            // xử lý case khi đang fill slot thì có 1 box mới được sinh ra thì phải refill luôn
            if (Area == AreaType.Waiting)
            {
                WaitingAreaManager.Instance.TryRefillSlot(this);
            }
        }

        /// <summary>
        /// Clear this slot for reuse.
        /// </summary>
        public void Clear()
        {
            IsFilled = false;
            IsAnimationFilled = false;
            RestoreRingsScale();
            if (animator != null)
            {
                animator.speed = 1;
                animator.Play("WoodRingIdle");
            }

            if (Area == AreaType.Waiting)
            {
                FilledColor = YarnColor.None;
                CurrentPart = null;
            }
        }

        /// <summary>
        /// Reset hết các event handler để đảm bảo không chồng delegate khi reuse.
        /// </summary>
        public void ResetCallbacks()
        {
            OnFilled = null;
            OnAnimationFilling = null;
            OnAnimationFilled = null;
            if (animator != null)
                animator.Play("WoodRingIdle");
        }


        private void ApplyColor(YarnColor color)
        {
            var mat = YarnColorManager.Instance.GetSlotStringMaterial(color);
            if (mat != null)
                foreach (var ring in allRings)
                {
                    ring.material = mat;
                }
        }

        public Vector3 GetAnchorWorldPosition()
        {
            return (ropeTopAnchor != null ? ropeTopAnchor : transform).position;
        }

        public Transform GetAnchorTransform()
        {
            return ropeTopAnchor != null ? ropeTopAnchor : transform;
        }

        public void ShowIdleStateVisual()
        {
            animator.Play("Idle");
        }

        public void ShowFullStateVisual()
        {
            animator.Play("WoodRingFull");
            OnAnimationFilling?.Invoke();
            IsAnimationFilled = true;

            OnAnimationFilled?.Invoke();
        }

        public MeshRenderer GetSlotBaseRenderer()
        {
            return slotBaseRenderer;
        }

        public MeshRenderer GetSlotSpoolRenderer()
        {
            return slotSpoolRenderer;
        }

        public MeshRenderer GetSlotShadowRenderer()
        {
            return slotShadowRenderer;
        }

        public void UnFill(Action callback)
        {
            IsFilled = false;
            IsAnimationFilled = false;

            if (Area == AreaType.Waiting)
            {
                FilledColor = YarnColor.None;
                CurrentPart = null;
            }

            if (animator != null && ringDisappearClip != null)
            {
                animator.Play(ringDisappearClip.name);
                // 3 vòng thu nhỏ cùng nhịp clip biến mất (đồng bộ với dây bay, Cylinder giữ nguyên).
                PlayRingsShrink(ringDisappearClip.length);

                StartCoroutine(WaitForUnfill(callback));
            }
            else
            {
                // Không có animator/clip thì gọi callback ngay, nếu không slot waiting
                // không bao giờ được đăng ký lại (mất slot, kẹt lose).
                if (animator == null || ringDisappearClip == null)
                    Debug.LogError($"[Slot] {name}: thiếu Animator/RingDisappearClip nên bỏ qua animation unfill.", this);
                callback?.Invoke();
            }
        }

        private IEnumerator WaitForUnfill(Action callback)
        {
            float duration = ringDisappearClip.length;

            yield return new WaitForSeconds(duration);

            callback?.Invoke();

            if (animator != null)
            {
                animator.Play("WoodRingIdle");
            }
        }

        public void DoRingDisappearAnim(float speed)
        {
            if (animator == null || ringDisappearClip == null)
            {
                Debug.LogError($"[Slot] {name}: thiếu Animator/RingDisappearClip nên bỏ qua animation vòng.", this);
                return;
            }
            animator.speed = speed;
            animator.Play(ringDisappearClip.name);
        }
    }

    // SlotFilledEvent.cs
    public class SlotFilledEvent
    {
        public Slot Slot { get; }

        public SlotFilledEvent(Slot slot)
        {
            Slot = slot;
        }
    }
}