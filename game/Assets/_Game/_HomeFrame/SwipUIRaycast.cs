using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts
{
    // Vuốt (kéo ngang) trên vùng menu để đổi tab.
    // Port từ dự án gốc. Đã bỏ FrameManager.instance.CountFramesActive()
    // (target chưa có FrameManager) và phần tween trang: ở target HomeUI tự
    // bật/tắt trang, nên vuốt chỉ đổi tab qua MenuBarManager.
    public class SwipeUIRaycast : MonoBehaviour
    {
        [SerializeField] private MenuBarManager menuBarManager;
        [SerializeField] private RectTransform levelHome, homeShop, homeRanking;
        [SerializeField] private RectTransform swipeArea; // Vùng UI cần vuốt
        [SerializeField] private ScrollRect scrollRect; // Vertical Scrollbar

        private Vector2 startTouchPosition;
        private bool isDragging = false;

        [SerializeField] private float swipeThresHold = 500f; // Ngưỡng để nhận diện vuốt
        private const float SWIPE_ANIMATION_DURATION = 0.2f;
        private Tween _tween;

        void Update()
        {
            // 1) Lấy trạng thái input và vị trí pointer
            bool began = Input.GetMouseButtonDown(0)
                         || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
            bool held = Input.GetMouseButton(0)
                         || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary));
            bool ended = Input.GetMouseButtonUp(0)
                         || (Input.touchCount > 0 && (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled));

            Vector2 pointerPos = Input.touchCount > 0
                ? Input.GetTouch(0).position
                : (Vector2)Input.mousePosition;

            if (began && IsPointerOverSwipeArea(pointerPos)) // Khi bắt đầu chạm vào vùng UI
            {
                startTouchPosition = Input.mousePosition;
                isDragging = true;
                if (scrollRect != null) scrollRect.vertical = true;
            }
            else if (held && isDragging) // Khi đang giữ và kéo
            {
                Vector2 delta = (Vector2)Input.mousePosition - startTouchPosition;

                // Vuốt vượt ngưỡng -> chuyển tab ngay lập tức
                if (Mathf.Abs(delta.x) > swipeThresHold)
                {
                    isDragging = false; // Ngăn không cho kéo tiếp
                    KillTween();
                    if (delta.x > 0)
                        MoveRight();
                    else
                        MoveLeft();
                }
            }
            else if (ended && isDragging)
            {
                isDragging = false;
                KillTween();
                if (scrollRect != null) scrollRect.vertical = true; // Bật lại scroll sau khi kết thúc thao tác
            }
        }

        private void KillTween()
        {
            if (_tween != null) _tween.Kill();
            _tween = null;
        }

        // Kiểm tra chuột có nằm trong vùng UI swipeArea không
        private bool IsPointerOverSwipeArea(Vector2 screenPos)
        {
            if (swipeArea == null)
                return false;

            Vector2 localMousePosition;

            // Lấy Camera đang sử dụng trong Canvas
            Canvas canvas = swipeArea.GetComponentInParent<Canvas>();
            Camera uiCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            // Chuyển đổi vị trí chuột sang tọa độ local của swipeArea
            RectTransformUtility.ScreenPointToLocalPointInRectangle(swipeArea, Input.mousePosition, uiCamera, out localMousePosition);

            // Kiểm tra xem vị trí chuột có nằm trong vùng swipeArea không
            return swipeArea.rect.Contains(localMousePosition);
        }

        public void MoveRight()
        {
            if (menuBarManager == null)
                return;

            switch (menuBarManager.OptionMenu)
            {
                case MenuBarOption.Ranking:
                    menuBarManager.OpenHome();
                    break;
                case MenuBarOption.Home:
                    menuBarManager.OpenShop();
                    break;
            }
        }

        public void MoveLeft()
        {
            if (menuBarManager == null)
                return;

            switch (menuBarManager.OptionMenu)
            {
                case MenuBarOption.Shop:
                    menuBarManager.OpenHome();
                    break;
                case MenuBarOption.Home:
                    menuBarManager.OpenRanking();
                    break;
            }
        }
    }
}
