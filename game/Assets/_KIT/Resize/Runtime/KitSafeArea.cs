using System;
using System.Collections;
using UnityEngine;

namespace _KIT
{
    [ExecuteInEditMode]
    public class KitSafeArea : MonoBehaviour
    {
        [SerializeField] private bool enable = true;
        
        private void OnEnable()
        {
            StartCoroutine(ApplySafeAreaIE()); 
        }

        private void OnDisable()
        {
            StopCoroutine(ApplySafeAreaIE());
        }

        private void OnRectTransformDimensionsChange()
        {
            if (Application.isPlaying) return;
            if (!gameObject.activeInHierarchy) return;
            StartCoroutine(ApplySafeAreaIE());
        }

        private IEnumerator ApplySafeAreaIE()
        {
            if (!enable)
            {
                yield break;
            }
            
            if (Screen.width == 0 || Screen.height == 0)
            {
                Debug.LogWarning("Screen dimensions are zero, skipping safe area adjustment.");
                yield break;
            }

            yield return new WaitForEndOfFrame();
            
            Rect safeArea = Screen.safeArea;

            Vector2 anchorMin = new Vector2(safeArea.x / Screen.width, safeArea.y / Screen.height);
            Vector2 anchorMax = new Vector2((safeArea.x + safeArea.width) / Screen.width, (safeArea.y + safeArea.height) / Screen.height);

            if (float.IsNaN(anchorMin.x) || float.IsNaN(anchorMin.y) || float.IsNaN(anchorMax.x) || float.IsNaN(anchorMax.y))
            {
                Debug.LogError("Safe Area calculation resulted in NaN values!");
                yield break;
            }
            
            RectTransform rectTransform = GetComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
        }
    }
}