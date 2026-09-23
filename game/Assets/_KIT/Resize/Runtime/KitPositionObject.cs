using System;
using UnityEngine;

namespace _KIT
{
    /// <summary>
    /// Hỗ trợ cả UI & Object
    /// Nếu là UI thì sẽ chỉnh Rect
    /// Nếu là Object thì chỉnh Transform
    /// </summary>
    [ExecuteInEditMode]
    public class KitPositionObject : MonoBehaviour
    {
        [Serializable]
        private class Data
        {
            public KitSize screen;
            public Vector3 position = Vector3.zero;
        }

        [SerializeField] private Data[] allData = new[]
        {
            new Data
            {
                screen = KitSize.Size_1920x1080_16x9,
                position = new Vector3(0, 0, 0)
            },
            new Data
            {
                screen = KitSize.Size_2732x2048_4x3,
                position = new Vector3(0, 0, 0)
            }
        };

        private void Awake()
        {
            UpdatePosition();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdatePosition();
        }

        private void UpdatePosition()
        {
            if (allData.Length == 0)
                return;
            
            float ratio = Screen.height / (float)Screen.width;
            float minDistance = float.MaxValue;
            Data data = null;
            
            foreach (var config in allData)
            {
                float distance = Mathf.Abs(config.screen.GetRatio() - ratio);
                if (distance < minDistance)
                {
                    data = config;
                    minDistance = distance;
                }
            }
            
            RectTransform rect = transform.GetComponent<RectTransform>();
            if (rect != null)
                rect.anchoredPosition3D = data.position;
            else
                transform.localPosition = data.position;
        }
    }
}