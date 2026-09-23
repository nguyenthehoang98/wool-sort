using _Game.Scripts.Gameplay;
using UnityEngine;

// namespace where YarnPart lives

namespace _Game.Scripts.Tutorial
{
    [RequireComponent(typeof(YarnPart))]
    public class TutorialTargetMarker : MonoBehaviour
    {
        public enum MarkerCategory
        {
            FillWool,
            PreviewLayer,
            // ... sau này nếu có thêm tutorial khác
        }
        
        [Tooltip("Chọn tutorial nào sẽ dùng marker này")]
        public MarkerCategory category;
        
        [Tooltip("Order for tutorial clicks (0=first)")]
        public int order;
    }
}