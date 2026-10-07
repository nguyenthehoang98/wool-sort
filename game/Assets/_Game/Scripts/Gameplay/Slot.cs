// Reusable Slot component for both Box and WaitingArea

using System;
using System.Collections;
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

        public void Initialize(YarnColor color, Box parentBox = null)
        {
            FilledColor = color;
            ParentBox = parentBox;
            Area = parentBox != null ? AreaType.Box : AreaType.Waiting;
            IsFilled = false;
            CurrentPart = null;
            IsAnimationFilled = false;

            if (color != YarnColor.None)
            {
                ApplyColor(color);
            }
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
            ApplyColor(FilledColor);

            if (animator != null)
            {
                animator.Play(ringAppearClip.name);

                // Nếu là double speed thì set speed animator thành 2f
                if (isDoubleSpeed)
                {
                    animator.speed = 2f;
                }

                StartCoroutine(WaitAndTriggerBoxBounce(isDoubleSpeed));
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

            if (animator != null)
            {
                animator.Play(ringDisappearClip.name);

                StartCoroutine(WaitForUnfill(callback));
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