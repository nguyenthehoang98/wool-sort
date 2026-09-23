using Lean.Touch;
using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    [RequireComponent(typeof(Transform))]
    public class ModelDragRotate : MonoBehaviour
    {
        private float rotationSpeed;
        private float autoRotateSpeed;
        private float idleDelay;
        private float smoothing; // Độ mượt (cao hơn = nhanh đạt tới target)

        private Quaternion targetRotation;
        private Camera mainCamera;
        private bool isDragging = false;
        private int draggingFingerIndex = -1;
        private float idleTimer = 0f;
        private bool autoRotating = false;

        private bool allowAutoRotate = false;

        void Awake()
        {
            mainCamera = Camera.main;
            targetRotation = transform.rotation;

            rotationSpeed = GameManager.Instance.GameConfig.rotationSpeed;
            autoRotateSpeed = GameManager.Instance.GameConfig.autoRotateSpeed;
            idleDelay = GameManager.Instance.GameConfig.idleDelay;
            smoothing = GameManager.Instance.GameConfig.smoothing;

            allowAutoRotate = false;
        }

        void OnEnable()
        {
            LeanTouch.OnFingerDown += HandleFingerDown;
            LeanTouch.OnFingerUp += HandleFingerUp;
            LeanTouch.OnFingerUpdate += HandleFingerUpdate;
        }

        void OnDisable()
        {
            LeanTouch.OnFingerDown -= HandleFingerDown;
            LeanTouch.OnFingerUp -= HandleFingerUp;
            LeanTouch.OnFingerUpdate -= HandleFingerUpdate;
        }

        private void HandleFingerDown(LeanFinger finger)
        {
            // Khi ngón chạm xuống và không phải trên UI, bắt đầu drag
            if (finger.IsOverGui == false)
            {
                isDragging = true;
                draggingFingerIndex = finger.Index;
            }
        }

        private void HandleFingerUp(LeanFinger finger)
        {
            // Khi ngón nhấc lên, nếu là ngón chúng ta đang theo dõi thì stop drag
            if (finger.Index == draggingFingerIndex)
            {
                isDragging = false;
                draggingFingerIndex = -1;
            }
        }

        private void HandleFingerUpdate(LeanFinger finger)
        {
            // Chỉ rotate khi đang drag và đúng ngón index
            if (isDragging == false || finger.Index != draggingFingerIndex)
                return;

            // Bổ sung: nếu đang có nhiều hơn 1 ngón, thoát
            if (LeanTouch.Fingers.Count != 1)
                return;

            // Lấy delta (pixel delta để cross-device ổn định)
            Vector2 delta = finger.ScreenDelta;

            // Xoay quanh Y
            Quaternion yRot = Quaternion.AngleAxis(-delta.x * rotationSpeed, mainCamera.transform.up);

            // Xoay quanh X
            Quaternion xRot = Quaternion.AngleAxis(delta.y * rotationSpeed, mainCamera.transform.right);

            // Tính rotation mục tiêu
            targetRotation = yRot * xRot * targetRotation;
        }

        void Update()
        {
            if (!allowAutoRotate) return;
            if (!GameManager.Instance.CanInteractWithGame()) return;

            // Determine if we should auto-rotate
            bool shouldAutoRotate = !isDragging && LeanTouch.Fingers.Count == 0;

            if (shouldAutoRotate)
            {
                idleTimer += Time.deltaTime;
                if (idleTimer >= idleDelay)
                {
                    autoRotating = true;

                    // --- AutoRotate: use Transform.Rotate ---
                    // Only rotate around Y axis
                    transform.Rotate(0f, autoRotateSpeed * Time.deltaTime, 0f, Space.World);

                    // Keep targetRotation in sync with current rotation
                    targetRotation = transform.rotation;
                }
            }
            else
            {
                idleTimer = 0;
                autoRotating = false;
            }

            // --- ManualRotate: use Slerp as before ---
            if (!autoRotating)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * smoothing);
            }
        }

        // gọi từ GameManager khi xong animation đầu level
        public void EnableAutoRotate(bool value)
        {
            allowAutoRotate = value;
            if (value)
            {
                idleTimer = 0f;
                targetRotation = transform.rotation;
            }
        }
    }
}