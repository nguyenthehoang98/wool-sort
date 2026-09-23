using System;
using _Game.Scripts.Gameplay;
using Lean.Common;
using UnityEngine;
using UnityEngine.UI;

namespace _Game.Scripts
{
    public class ScaleSliderSync : MonoBehaviour
    {
        [SerializeField] private Image imgFillAmount;

        public Slider scaleSlider;

        private Transform targetLevelTransform;
        private float minScale;
        private float maxScale;
        private Vector3 _baseScale;

        private bool hasInit = false;

        public void SetTargetScale(Transform target)
        {
            Debug.Log("set target scale");
            targetLevelTransform = target;
            _baseScale = targetLevelTransform.localScale;

            var leanScale = targetLevelTransform.GetComponent<LeanConstrainScale>();
            float min = leanScale.MinimumScale.x;
            float max = leanScale.MaximumScale.x;

            Init(min, max);

            hasInit = true;
        }

        private void Init(float min, float max)
        {
            minScale = min;
            maxScale = max;

            scaleSlider.minValue = 0f;
            scaleSlider.maxValue = 1f;
            scaleSlider.value = Mathf.InverseLerp(minScale, maxScale, 1f);
            scaleSlider.onValueChanged.RemoveAllListeners();
            scaleSlider.onValueChanged.AddListener(OnSliderChanged);
        }

        void Update()
        {
            if (!hasInit) return;
            if (!GameManager.Instance.CanInteractNotCheckAnimStartLevel() || targetLevelTransform == null) return;

            // khi pinch thay đổi localScale, đồng bộ Slider
            float curr = targetLevelTransform.localScale.x / _baseScale.x;
            float t = Mathf.InverseLerp(minScale, maxScale, curr);
            if (!Mathf.Approximately(scaleSlider.value, t))
            {
                scaleSlider.SetValueWithoutNotify(t);
                imgFillAmount.fillAmount = t;
            }
        }

        void OnSliderChanged(float t)
        {
            float s = Mathf.Lerp(minScale, maxScale, t);
            targetLevelTransform.localScale = _baseScale * s;

            imgFillAmount.fillAmount = scaleSlider.value;
        }
    }
}
