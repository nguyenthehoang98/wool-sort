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
        
        public Material GetTransparentMaterial()
        {
            if (colorDatabase.transparentMaterial == null)
                Debug.LogError("Thiếu transparentMaterial trong database!");
            return colorDatabase.transparentMaterial;
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
            return colorDatabase.GetMaterialBasket(color);
        }

        public Material GetSlotStringMaterial(YarnColor color)
        {
            if (colorDatabase == null)
            {
                Debug.LogError("YarnManager: thiếu YarnColorDatabase!", this);
                return null;
            }
            return colorDatabase.GetMaterialSlotString(color);
        }
    }
}