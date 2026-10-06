using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts
{
    // 21 màu normal
    public enum YarnColor
    {
        Blue = 0,
        Brown = 1,
        Charcoal = 2,
        Coral = 3,
        Cream = 4,
        Crimson = 5,
        Deep = 6,
        Forest = 7,
        Gray = 8,
        Green = 9,
        Lime = 10,
        Magenta = 11,
        Orange = 12,
        Pink = 13,
        Plum = 14,
        Purple = 15,
        Sky = 16,
        Snow = 17,
        Steel = 18,
        Teal = 19,
        Yellow = 20,
        
        None = 999,
        Rainbow = 1000
    }

    [System.Serializable]
    public struct ColorMaterialPair
    {
        public YarnColor color;
        public Material material;
    }

    [CreateAssetMenu(
        fileName = "YarnColorDatabase",
        menuName = "Game/Yarn Color Database",
        order = 100)]
    public class YarnColorDatabase : ScriptableObject
    {
        [Tooltip("Khai báo từng cặp color ↔ material")]
        public ColorMaterialPair[] colorMaterialPairs;
        
        [Header("Basket Material")]
        public ColorMaterialPair[] basketColorMaterialPairs;
        
        [Header("Slot Material")]
        public ColorMaterialPair[] slotStringColorMaterialPairs;
        
        [Header("Special Materials")]
        [Tooltip("Material dùng khi chuyển sang chế độ trong suốt")]
        public Material transparentMaterial;
        
        // lookup nhanh
        private Dictionary<YarnColor, Material> _colorToMat;
        private Dictionary<Material, YarnColor> _matToColor;
        
        private Dictionary<YarnColor, Material> _colorToMatBasket;
        private Dictionary<YarnColor, Material> _colorToMatSlotString;

        // OnEnable vẫn build khi  được load
        private void OnEnable()
        {
            BuildLookup();
        }

        // Gọi lại khi cần
        private void EnsureLookup()
        {
            if (_matToColor == null || _colorToMat == null
                || _colorToMatBasket == null || _colorToMatSlotString == null)
                BuildLookup();
        }


        private void BuildLookup()
        {
            _colorToMat = new Dictionary<YarnColor, Material>();
            _matToColor = new Dictionary<Material, YarnColor>();
            _colorToMatBasket = new Dictionary<YarnColor, Material>();
            _colorToMatSlotString = new Dictionary<YarnColor, Material>();
            
            if (colorMaterialPairs != null)
            {
                foreach (var p in colorMaterialPairs)
                {
                    if (p.material == null) continue;
                    // add or overwrite
                    _colorToMat[p.color] = p.material;
                    if (!_matToColor.ContainsKey(p.material))
                        _matToColor.Add(p.material, p.color);
                }
            }

            if (basketColorMaterialPairs != null)
            {
                foreach (var p in basketColorMaterialPairs)
                {
                    if (p.material == null) continue;
                    // add or overwrite
                    _colorToMatBasket[p.color] = p.material;
                }
            }

            if (slotStringColorMaterialPairs != null)
            {
                foreach (var p in slotStringColorMaterialPairs)
                {
                    if (p.material == null) continue;
                    // add or overwrite
                    _colorToMatSlotString[p.color] = p.material;
                }
            }
        }

        /// <summary>
        /// Lấy material từ YarnColor
        /// </summary>
        public Material GetMaterial(YarnColor color)
        {
            EnsureLookup();
            _colorToMat.TryGetValue(color, out var mat);
            return mat;
        }

        /// <summary>
        /// Map ngược từ material về YarnColor
        /// </summary>
        public bool TryGetColor(Material material, out YarnColor color)
        {
            color = default;
            if (material == null) return false;
            EnsureLookup();
            return _matToColor.TryGetValue(material, out color);
        }
        
        public Material GetMaterialBasket(YarnColor color)
        {
            EnsureLookup();
            if (_colorToMatBasket == null) return null;
            _colorToMatBasket.TryGetValue(color, out var mat);
            return mat;
        }

        public Material GetMaterialSlotString(YarnColor color)
        {
            EnsureLookup();
            if (_colorToMatSlotString == null) return null;
            _colorToMatSlotString.TryGetValue(color, out var mat);
            return mat;
        }
    }
}