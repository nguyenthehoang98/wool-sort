using UnityEngine;

namespace _Game.Scripts
{
    [CreateAssetMenu(menuName = "Configs/GameConfig")]
    public class GameConfig : ScriptableObject
    {
        [Header("Slot")] 
        public int maxWaitingSlots = 7;
        public int initialSlotCount = 5;

        [Header("Spawn Yarn Part Scale")]
        [Tooltip("Scale so với layer trước")]
        public float growScale = 0.96f;
        [Tooltip("Scale ngay khi mới khởi tạo so với khi grow")]
        public float initialInnerScale = 0.8f;
        [Tooltip("Thời gian animation grow (giây)")]
        public float growDuration = 0.3f;
        [Tooltip("Auto Zoom Scale)")]
        public float autoZoomScale = 1.075f;
        
        [Header("Model Drag Rotate")]
        public float rotationSpeed = 0.36f;
        public float autoRotateSpeed = 10f;
        public float idleDelay = 3f;
        public float smoothing = 15f; // Độ mượt (cao hơn = nhanh đạt tới target)

        [Header("Dissolve")] 
        public float dissolveDuration = 1;

        [Header("Hold Threshold")] 
        public float holdThreshold = 0.3f;

        [Header("Start Level Feedback")] 
        public int numRotate = 2;

        public float timeRotate = 2.25f;

        [Header("Fail Offer Level")] 
        public int failOfferLevel = 10;
    }
}