using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _KIT
{
    public class KitButtonHold : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Transform container;
        [SerializeField] private Vector3 scaleTarget = new Vector2(1.15f, 1.15f);
        [SerializeField] private float scaleDuration = 0.15f;

        public event Action OnBegin;
        public event Action OnHold;
        public event Action OnEnd;
        public float Threshold { get; set; } = 0.02f;

        private bool isHolding;
        private float elapsed;
        private Coroutine coroutine;

        private void OnDisable()
        {
            isHolding = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            elapsed = Threshold;
            isHolding = true; 
            OnBegin?.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            isHolding = false;
            OnEnd?.Invoke();
        }

        private void Update()
        {
            if (isHolding)
            {
                elapsed += Time.deltaTime;
                while (elapsed >= Threshold)
                {
                    elapsed -= Threshold;
                    OnHold?.Invoke();
                }
            }
        }
    }
}