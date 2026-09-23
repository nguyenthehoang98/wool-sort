using _Game.Scripts.Pool;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    public class Box : MonoBehaviour, IPoolable
    {
        [SerializeField] private Animator animator;
        [Header("Animation Clips")]
        [SerializeField] private AnimationClip idleClip;
        [SerializeField] private AnimationClip inClip;
        [SerializeField] private AnimationClip outClip;
        [SerializeField] private AnimationClip changeClip;

        [Header("Mesh")]
        [SerializeField] private MeshRenderer basketMesh;

        [Header("Target Rope")]
        [SerializeField] private Transform targetRope;

        [Tooltip("Slot components under this box")]
        [SerializeField]
        private List<Slot> slots = new List<Slot>();

private YarnColor boxColor;

        public event Action<Box> OnBoxFull; // logic full ngay lập tức
        public event Action<Box> OnBoxClose;   // fire khi slot-animation xong, chuẩn bị đóng nắp

        private Tween scaleBounceTween;

        private Vector3 initialScale;

        private int pendingCloseCount; // count of slots whose animations have finished

        // expose cho manager
        public float InDuration => inClip == null ? 0f : inClip.length;
        public float OutDuration => outClip == null ? 0f : outClip.length;
        public float ChangeDuration => changeClip == null ? 0f : changeClip.length;

        public void PlayInAnimation()
        {
            if (animator != null)
            {
                animator.Play(inClip.name);
            }

        }

        public void PlayOutAnimation()
        {
            if (animator != null)
            {
                animator.Play(outClip.name);
            }

        }

        public void PlayChangeAnimation()
        {
            if (animator != null)
            {
                animator.Play(changeClip.name);
            }
        }

        private GameObject unlockVfx;

        void Awake()
        {
            initialScale = transform.localScale;
        }

        public void Initialize(YarnColor c)
        {
            // 0) reset mọi event handler trước khi subscribe lại
            ResetBoxEvents();

            if (animator != null)
            {
                animator.Play(idleClip.name);
            }

            // Reset trạng thái slot
            foreach (var slot in Slots)
            {
                slot.ResetCallbacks();
                slot.Clear();
            }

            boxColor = c;
            ApplyColor(c);

            // reset counter
            pendingCloseCount = 0;

            foreach (var s in slots)
            {
                s.Initialize(c, this);

                // 1) logic full ngay khi OnFilled
                s.OnFilled += HandleSlotFilled;

                // 2) bounce on filling animation
                s.OnAnimationFilling += HandleSlotAnimationFilling;

                // 3) track final animation end for closing
                s.OnAnimationFilled += HandleSlotAnimationFilled;
            }
        }

        private void HandleSlotFilled(Slot slot)
        {
            if (slots.All(x => x.IsFilled))
            {
                Debug.Log($"Box color {boxColor} full");
                OnBoxFull?.Invoke(this);
            }
        }

        private void HandleSlotAnimationFilling()
        {
            PlayBounceEffect();
        }

        private void HandleSlotAnimationFilled()
        {
            pendingCloseCount++;
            Debug.Log($"Box color {boxColor} | pending close count = {pendingCloseCount}");
            if (pendingCloseCount >= slots.Count)
                OnBoxClose?.Invoke(this);
        }

        private void ApplyColor(YarnColor color)
        {
            var mat = YarnColorManager.Instance.GetBasketMaterial(color);
            basketMesh.material = mat;
        }

        private void PlayBounceEffect()
        {
            Debug.Log("Box | Play Bounce Effect");
            // Nếu đang tween thì không chạy lại, hoặc có thể restart tùy ý
            if (scaleBounceTween != null && scaleBounceTween.IsActive())
            {
                scaleBounceTween.Restart(); // hoặc return nếu muốn chỉ chạy 1 lần
                return;
            }

            Vector3 shrinkScale = initialScale * 0.9f;

            scaleBounceTween = transform.DOScale(shrinkScale, 0.08f)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    // Overshoot bé hơn (1.2 thay vì mặc định ~1.7)
                    transform.DOScale(initialScale, 0.12f).SetEase(Ease.OutBack, 1.2f);
                });
        }

        // Khóa / mở khóa tương tác của tất cả slot
        public void SetSlotsInteractable(bool interactable)
        {
            foreach (var s in slots)
            {
                var col = s.GetComponent<Collider>();
                if (col != null) col.enabled = interactable;
            }
        }

        public List<Slot> Slots => slots;
       
        public YarnColor BoxColor => boxColor;
     
        public void OnSpawned()
        {
            gameObject.SetActive(true);
        }

        public void OnDespawned()
        {
            if (unlockVfx != null)
            {
                Destroy(unlockVfx);
            }

            // phòng trường hợp có subscribe sau Initialize nhưng trước Release
            ResetBoxEvents();

            transform.SetParent(PoolManager.Instance.PoolRoot, worldPositionStays: false);
            gameObject.SetActive(false);
        }

        // Phương thức trả về pool (thay vì Destroy)
        public void Despawn()
        {
            PoolManager.Instance.Release("box", this);
        }

        private void ResetBoxEvents()
        {
            OnBoxFull = null;
            OnBoxClose = null;
        }

        /// <summary>
        /// Số slot đã được fill trong box này.
        /// </summary>
        public int FilledSlotCount => slots.Count(s => s.IsFilled);

        public int RequiredCount => slots.Count(s => !s.IsFilled);

        /// <summary>
        /// Trả về true nếu còn ít nhất 1 slot chưa được fill.
        /// </summary>
        public bool HasAvailableSlot()
        {
            return slots.Any(s => !s.IsFilled);
        }

        public void SetRainbowColor()
        {
            ApplyColor(YarnColor.Rainbow);
        }

        public void SetVfx(GameObject vfx)
        {
            unlockVfx = vfx;
        }

        public bool IsLastBoxSlot(Slot slot)
        {
            return slot == slots.Last();
        }

        public Transform GetTargetRope()
        {
            return targetRope;
        }

        public void DoEffectDisappearAllSlot(float currentWaitOfTime)
        {
            StartCoroutine(DisappearAllSlotCoroutine(currentWaitOfTime));
        }

        private IEnumerator DisappearAllSlotCoroutine(float totalTime)
        {
            // Iterate from last slot to first (Slot 2 -> Slot 0)
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                Slot slot = slots[i];

                float time = Mathf.Max(totalTime / Mathf.Max(1, slots.Count), 0.01f);
                float clipLength = slot.RingDisappearClip != null ? slot.RingDisappearClip.length : time;
                float speed = clipLength / time;

                slot.DoRingDisappearAnim(speed);
                StartCoroutine(UpdateTargetRopePos(slot.GetAnchorTransform(), clipLength));
                yield return new WaitForSeconds(time);
            }
        }

        private IEnumerator UpdateTargetRopePos(Transform target, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                if (targetRope != null && target != null && IsFinite(target.position))
                    targetRope.position = target.position;

                elapsed += Time.deltaTime;
                yield return null; // Đợi đến frame tiếp theo
            }
            // Đảm bảo vị trí cuối cùng được cập nhật
            if (targetRope != null && target != null && IsFinite(target.position))
                targetRope.position = target.position;
        }

        private static bool IsFinite(Vector3 value)
        {
            return IsFinite(value.x) && IsFinite(value.y) && IsFinite(value.z);
        }

        private static bool IsFinite(float value)
        {
            return !float.IsNaN(value) && !float.IsInfinity(value);
        }
    }
}
