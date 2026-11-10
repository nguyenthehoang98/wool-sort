namespace _Game.Scripts.Meta
{
    using System.Collections.Generic;
    using _Game.Scripts.Core;
    using _Game.Scripts.LevelData;
    using UnityEngine;

    /// <summary>
    /// Cột ô hiển thị màn xếp DỌC trên trang chủ (port từ LevelHome/ItemLevelHome của dự án gốc).
    ///
    /// Các ô màn (<see cref="LevelButton"/>) được ĐẶT SẴN trong prefab/scene để chỉnh
    /// tay bằng Inspector. Script KHÔNG Instantiate/Destroy gì lúc chạy: nó chỉ đẩy dữ
    /// liệu (số màn, khoá/mở, sprite nền) vào các ô có sẵn và ẩn các ô dư.
    ///
    /// Số ô hiển thị = <see cref="maxVisibleItems"/> (mặc định 4). Cửa sổ trượt theo màn
    /// hiện tại: 1 màn trước + màn hiện tại + 2 màn sau, tự kẹp ở đầu/cuối danh sách.
    /// Thứ tự HIỂN THỊ ngược với thứ tự màn: màn cao hơn nằm ở ô trên.
    ///
    /// Danh sách chỉ để HIỂN THỊ tiến độ / độ khó — không bấm được. Vào màn chơi bằng
    /// nút Start trên Home.
    ///
    /// Danh sách CỐ ĐỊNH: không có ScrollRect / mask / viewport nên không kéo hay scroll
    /// được; các ô do `VerticalLayoutGroup` trên Holder xếp sẵn.
    ///
    /// Danh sách màn lấy từ `LevelCatalog` (Resources/LevelCatalog.asset): số màn và độ khó
    /// (Normal / Hard / VeryHard → đổi sprite ô) không còn hardcode trong prefab nữa.
    /// Khi chưa có catalog thì fallback về `maxLevel` gán trong Inspector.
    /// </summary>
    public class LevelList : MonoBehaviour
    {
        [Header("Các ô màn đặt sẵn (theo thứ tự hiển thị)")]
        [Tooltip("Để trống = tự lấy các LevelButton con của Content theo thứ tự trong Hierarchy.")]
        [SerializeField] private LevelButton[] items;

        [Header("Chỗ chứa các nút")]
        [SerializeField] private RectTransform content;

        [Header("Sprite ô màn (pick từ dự án gốc)")]
        [SerializeField] private Sprite normalSprite;   // Button_Lv_1 - xanh
        [SerializeField] private Sprite currentSprite;  // Button_Lv_2 - be
        [SerializeField] private Sprite hardSprite;    // Button_Lv_3 - tím
        [SerializeField] private Sprite veryHardSprite;// Button_Lv_4 - đỏ

        [Header("Cấu hình")]
        [Tooltip("Số ô hiển thị tối đa. Ô dư trong danh sách sẽ bị ẩn.")]
        [SerializeField] private int maxVisibleItems = 4;
        [Tooltip("Fallback khi CHƯA có LevelCatalog. Có catalog thì số màn lấy từ catalog.")]
        [SerializeField] private int maxLevel = 7;
        // Khoảng cách / cách xếp ô do `VerticalLayoutGroup` trên Holder quyết định.

        private readonly List<LevelButton> _items = new List<LevelButton>();

        private void Awake()
        {
            Bind();
        }

        /// <summary>Đẩy dữ liệu màn vào các ô đặt sẵn. Gọi lại được khi màn hiện tại đổi.</summary>
        public void Bind()
        {
            CollectItems();
            if (_items.Count == 0)
            {
                Debug.LogError("[LevelList] Chưa có LevelButton nào được đặt sẵn — không hiển thị được danh sách màn.");
                return;
            }

            var catalog = LevelCatalogProvider.Catalog;
            int current = EconomyService.Instance != null ? EconomyService.Instance.GetLevel() : 1;

            // Danh sách order hiển thị: lấy từ catalog, bỏ màn chưa có asset.
            var orders = CollectPlayableOrders(catalog);
            int count = Mathf.Min(Mathf.Min(Mathf.Max(1, maxVisibleItems), _items.Count), orders.Count);

            if (count <= 0)
            {
                SetAllVisible(false);
                return;
            }

            // Cửa sổ trượt: 1 màn trước + màn hiện tại + 2 màn sau (theo thứ tự tăng dần),
            // kẹp lại để luôn đủ `count` ô khi ở đầu/cuối danh sách.
            int currentIndex = orders.IndexOf(current);
            int start = Mathf.Clamp(currentIndex - 1, 0, orders.Count - count);

            for (int i = 0; i < _items.Count; i++)
            {
                var btn = _items[i];
                if (btn == null) continue;

                bool visible = i < count;
                btn.gameObject.SetActive(visible);
                if (!visible) continue;

                // Danh sách xếp NGƯỢC: ô trên cùng là màn cao nhất trong cửa sổ,
                // màn hiện tại nằm phía dưới và tiến dần lên khi chơi.
                int lv = orders[start + (count - 1 - i)];
                bool isCurrent = lv == current;
                var mode = catalog != null ? catalog.GetModeTypeByOrder(lv) : LevelModeType.Normal;

                btn.Bind(lv, isCurrent, mode,
                         normalSprite, currentSprite, hardSprite, veryHardSprite);
            }
        }

        /// <summary>Lấy danh sách ô: ưu tiên mảng gán trong Inspector, trống thì lấy con của Content.</summary>
        private void CollectItems()
        {
            _items.Clear();

            if (items != null)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (items[i] != null) _items.Add(items[i]);
                }
            }

            if (_items.Count > 0 || content == null) return;

            for (int i = 0; i < content.childCount; i++)
            {
                var btn = content.GetChild(i).GetComponent<LevelButton>();
                if (btn != null) _items.Add(btn);
            }
        }

        private void SetAllVisible(bool visible)
        {
            for (int i = 0; i < _items.Count; i++)
            {
                if (_items[i] != null) _items[i].gameObject.SetActive(visible);
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
    }
}
