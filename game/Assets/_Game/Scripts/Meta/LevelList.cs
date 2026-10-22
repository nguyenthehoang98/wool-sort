namespace _Game.Scripts.Meta
{
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using _Game.Scripts.LevelData;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Hàng nút chọn màn trên trang chủ (port từ LevelHome/ItemLevelHome của dự án gốc).
    ///
    /// Bấm một ô đã mở → chơi đúng màn đó (EconomyService.PlayLevel rồi load scene Gameplay).
    /// Màn chưa mở thì nút không bấm được.
    ///
    /// Danh sách màn lấy từ `LevelCatalog` (Resources/LevelCatalog.asset): số màn và độ khó
    /// (Normal / Hard / VeryHard → đổi sprite ô) không còn hardcode trong prefab nữa.
    /// Khi chưa có catalog thì fallback về `maxLevel` gán trong Inspector.
    /// </summary>
    public class LevelList : MonoBehaviour
    {
        [Header("Nút màn (template để instantiate)")]
        [SerializeField] private LevelButton template;

        [Header("Chỗ chứa các nút")]
        [SerializeField] private RectTransform content;

        [Header("Sprite ô màn (pick từ dự án gốc)")]
        [SerializeField] private Sprite normalSprite;   // Button_Lv_1 - xanh
        [SerializeField] private Sprite currentSprite;  // Button_Lv_2 - be
        [SerializeField] private Sprite hardSprite;    // Button_Lv_3 - tím
        [SerializeField] private Sprite veryHardSprite;// Button_Lv_4 - đỏ

        [Header("Cấu hình")]
        [Tooltip("Fallback khi CHƯA có LevelCatalog. Có catalog thì số màn lấy từ catalog.")]
        [SerializeField] private int maxLevel = 7;
        [SerializeField] private float spacing = 150f;

        private void Awake()
        {
            Build();
        }

        private void Build()
        {
            if (template == null || content == null)
            {
                Debug.LogError("[LevelList] Thiếu template/content — không sinh được danh sách màn.");
                return;
            }

            // dọn nút cũ nếu build lại
            for (int i = content.childCount - 1; i >= 0; i--)
            {
                var child = content.GetChild(i);
                if (child.gameObject == template.gameObject) continue;
                Destroy(child.gameObject);
            }

            var catalog = LevelCatalogProvider.Catalog;
            int current = EconomyService.Instance != null ? EconomyService.Instance.GetLevel() : 1;

            // Danh sách order hiển thị: lấy từ catalog, bỏ màn chưa có asset.
            var orders = CollectPlayableOrders(catalog);
            if (orders.Count == 0) return;

            for (int i = 0; i < orders.Count; i++)
            {
                int lv = orders[i];

                var go = Instantiate(template.gameObject, content);
                go.SetActive(true);

                var btn = go.GetComponent<LevelButton>();
                if (btn == null) continue;

                bool unlocked = lv <= current;
                bool isCurrent = lv == current;
                var mode = catalog != null ? catalog.GetModeTypeByOrder(lv) : LevelModeType.Normal;
                btn.Bind(lv, unlocked, isCurrent, mode,
                         normalSprite, currentSprite, hardSprite, veryHardSprite, OnPickLevel);

                var rt = go.transform as RectTransform;
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2((i - (orders.Count - 1) * 0.5f) * spacing, 0f);
                }
            }
        }

        /// <summary>Các order (1-based) được hiển thị: mọi màn trong catalog, bỏ màn chưa có asset.</summary>
        private List<int> CollectPlayableOrders(LevelCatalog catalog)
        {
            var result = new List<int>();
            if (catalog != null && catalog.TotalLevels > 0)
            {
                foreach (var def in catalog.Levels)
                {
                    if (def == null || !def.HasAsset) continue;
                    result.Add(def.orderIndex);
                }
                if (result.Count > 0) return result;
            }

            // Fallback: chưa có catalog -> dùng maxLevel gán trong Inspector.
            for (int lv = 1; lv <= Mathf.Max(1, maxLevel); lv++) result.Add(lv);
            return result;
        }

        private static void OnPickLevel(int level)
        {
            if (EconomyService.Instance != null) EconomyService.Instance.PlayLevel(level);
            SceneManager.LoadScene("Gameplay");
        }
    }
}
