using UnityEngine;

namespace _Game.Scripts.Gameplay
{
    [ExecuteAlways] 
    public class YarnColorManager : MonoBehaviour
    {
        private static YarnColorManager _instance;
        public static YarnColorManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<YarnColorManager>();
                return _instance;
            }
        }
        
        [SerializeField] private YarnColorDatabase colorDatabase;

        void OnEnable()
        {
            // đảm bảo luôn có instance khi component này active
            _instance = this;
        }

        public Material GetYarnMaterial(YarnColor color)
        {
            if (colorDatabase == null)
            {
                Debug.LogError("YarnManager: thiếu YarnColorDatabase!", this);
                return null;
            }
            return colorDatabase.GetMaterial(color);
        }
        
        private Material runtimeTransparent;

        public Material GetTransparentMaterial()
        {
            var baseMat = colorDatabase != null ? colorDatabase.transparentMaterial : null;
            if (baseMat == null)
            {
                Debug.LogError("Thiếu transparentMaterial trong database!");
                return null;
            }
            // ThreadMat_Trans đang dùng shader YarnDissolve (Opaque) nên hold không bao giờ
            // trong suốt được. Dựng bản trong suốt thật (Standard/Transparent) để nhìn xuyên lớp ngoài.
            if (baseMat.shader != null && baseMat.shader.name.Contains("YarnDissolve"))
            {
                if (runtimeTransparent == null)
                {
                    var std = Shader.Find("Standard");
                    if (std == null)
                    {
                        Debug.LogError("Không tìm thấy shader Standard để dựng vật liệu hold trong suốt!");
                        return baseMat;
                    }
                    runtimeTransparent = new Material(std);
                    runtimeTransparent.SetFloat("_Mode", 3f); // Transparent
                    runtimeTransparent.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    runtimeTransparent.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    runtimeTransparent.SetInt("_ZWrite", 0);
                    runtimeTransparent.DisableKeyword("_ALPHATEST_ON");
                    runtimeTransparent.EnableKeyword("_ALPHABLEND_ON");
                    runtimeTransparent.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    runtimeTransparent.renderQueue = 3000;
                    runtimeTransparent.color = new Color(0.65f, 0.99f, 1f, 0.35f);
                }
                return runtimeTransparent;
            }
            return baseMat;
        }
        
        public bool TryGetColorFromMaterial(Material mat, out YarnColor color)
        {
            if (colorDatabase == null)
            {
                color = default;         // gán giá trị mặc định cho out
                return false;
            }

            return colorDatabase.TryGetColor(mat, out color);
        }
        
        public Material GetBasketMaterial(YarnColor color)
        {
            if (colorDatabase == null)
            {
                Debug.LogError("YarnManager: thiếu YarnColorDatabase!", this);
                return null;
            }
            var mat = colorDatabase.GetMaterialBasket(color);
            if (mat == null)
                Debug.LogWarning($"YarnColorManager: thiếu basket material cho màu {color} — rổ/dây bắn ảnh sẽ sai màu!", this);
            return mat;
        }

        public Material GetSlotStringMaterial(YarnColor color)
        {
            if (colorDatabase == null)
            {
                Debug.LogError("YarnManager: thiếu YarnColorDatabase!", this);
                return null;
            }
            var mat = colorDatabase.GetMaterialSlotString(color);
            if (mat == null)
                Debug.LogWarning($"YarnColorManager: thiếu slot-string material cho màu {color} — vòng/dây sẽ sai màu!", this);
            return mat;
        }
    }
}