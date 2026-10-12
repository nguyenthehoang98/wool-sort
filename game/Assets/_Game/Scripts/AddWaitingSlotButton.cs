using _Game.Scripts.Gameplay;
using Lean.Touch;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _Game.Scripts
{
    public class AddWaitingSlotButton : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private LayerMask boxLayerMask;
        [SerializeField] private MeshRenderer slotRenderer;
        [SerializeField] private MeshRenderer plusRenderer;

        void OnEnable() => LeanTouch.OnFingerTap += HandleTap;
        void OnDisable() => LeanTouch.OnFingerTap -= HandleTap;

        private void HandleTap(LeanFinger finger)
        {
            // Nếu tap vào UI thì bỏ qua
            if (finger.IsOverGui) return;
            if (!GameManager.Instance.CanInteractWithGame()) return;

            // Raycast từ OrthoCam, chỉ vào layer Box
            var ray = finger.GetRay(targetCamera);
            if (Physics.Raycast(ray, out var hit, Mathf.Infinity, boxLayerMask) &&
                hit.collider.gameObject == gameObject)
            {
                Debug.LogWarning("[Stub] AddWaitingSlotButton tap (shop chưa có trong bản này).");
            }
        }

        public MeshRenderer GetSlotBaseRenderer()
        {
            return slotRenderer;
        }

        public MeshRenderer GetPlusRenderer()
        {
            return plusRenderer;
        }
    }
}
