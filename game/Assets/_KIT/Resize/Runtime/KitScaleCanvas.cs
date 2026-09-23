using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace _KIT
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(CanvasScaler))]
    public class KitScaleCanvas : MonoBehaviour
    {
        [Serializable]
        private class Data
        {
            public KitSize screen;
            public float scale = 1;
        }

        [SerializeField] private Data[] allData = new[]
        {
            new Data
            {
                screen = KitSize.Size_2160x1080_18x9,
                scale = 0
            },
            new Data
            {
                screen = KitSize.Size_1920x1080_16x9,
                scale = 1
            },
            new Data
            {
                screen = KitSize.Size_2732x2048_4x3,
                scale = 1
            }
        };

        private void Awake()
        {
            UpdateScale();
        }

        private void OnValidate()
        {
            UpdateScale();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateScale();
        }

        private void UpdateScale()
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

            gameObject.GetComponent<CanvasScaler>().matchWidthOrHeight = data == null ? 1f : data.scale;
        }
    }
}