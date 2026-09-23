using System;
using System.Collections.Generic;
using UnityEngine;

namespace _KIT
{
    [ExecuteInEditMode]
    public class KitScaleObject : MonoBehaviour
    {
        [Serializable]
        private class Data
        {
            public KitSize screen;
            public Vector3 scale = Vector3.one;

            public override string ToString()
            {
                return $"{screen}: {scale}";
            }
        }

        [SerializeField] private Data[] allData = new[]
        {
            new Data
            {
                screen = KitSize.Size_1920x1080_16x9,
                scale = new Vector3(1, 1, 1)
            },
            new Data
            {
                screen = KitSize.Size_2732x2048_4x3,
                scale = new Vector3(0.9f, 0.9f)
            }
        };

        private void Awake()
        {
            transform.localScale = GetScale();
        }

        private void OnRectTransformDimensionsChange()
        {
            transform.localScale = GetScale();
        }

        public Vector3 GetScale()
        {
            if (allData.Length == 0)
                return Vector3.one;

            float ratio = Screen.height / (float)Screen.width;
            float minDistance = float.MaxValue;
            Data data = null;

            List<Data> list = new List<Data>(allData);
            list.Sort((a, b) => a.screen.CompareTo(b.screen));

            foreach (var config in allData)
            {
                float distance = Mathf.Abs(config.screen.GetRatio() - ratio);
                if (distance < minDistance)
                {
                    data = config;
                    minDistance = distance;
                }
            }

            return data.scale;
        }
    }
}