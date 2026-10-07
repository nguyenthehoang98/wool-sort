using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts
{
    /// <summary>
    /// 1 màu trong ma trận: ID màu + màu hiển thị + danh sách ô thuộc màu đó.
    /// Trong Inspector thấy ngay màu nào gồm những ô nào.
    /// LƯU Ý màu phụ: cùng 1 colorId (màu gốc để matching) có thể có NHIỀU
    /// entry khác displayColor (VD Pink nhạt + Pink đậm shading đều tính cho
    /// Pink). Runtime: GetColorId = màu gốc, GetDisplayColor = màu phụ từng ô.
    /// </summary>
    [System.Serializable]
    public struct ColorCellEntry
    {
        [Tooltip("ID màu GỐC (để matching gameplay: Blue, Green...). Màu phụ vẫn ghi màu gốc ở đây.")]
        public YarnColor colorId;
        [Tooltip("Màu hiển thị từng ô lúc reveal (màu phụ được giữ nguyên ở đây).")]
        public Color displayColor;
        [Tooltip("Danh sách ô (x, y) thuộc entry này. y = 0 là hàng dưới.")]
        public List<Vector2Int> cells;
    }

    /// <summary>
    /// Ma trận màu đã extract từ ảnh level, lưu DẠNG THƯA (sparse):
    /// mỗi màu 1 entry gồm ID màu + danh sách ô — nhẹ hơn nhiều so với
    /// mảng phẳng 784 ô, và mở Inspector là đọc được màu nào ở đâu.
    /// Runtime JamImage đọc file này để chia ô thay vì đọc trực tiếp texture.
    /// Đặt tại: Assets/Resources/LevelColorMatrix/{LevelName}.asset
    /// </summary>
    [CreateAssetMenu(
        fileName = "LevelColorMatrix",
        menuName = "Game/Level Color Matrix",
        order = 103)]
    public class LevelColorMatrix : ScriptableObject
    {
        /// <summary>Lưới chuẩn toàn game (đổi 1 chỗ này là đổi khắp runtime + tool).</summary>
        public const int DefaultGrid = 84;

        public string levelName;
        public int gridSize = DefaultGrid;
        public List<ColorCellEntry> entries = new List<ColorCellEntry>();

        [Header("Tầng phẳng (tool tự bake — runtime đọc ở đây)")]
        [Tooltip("ownerId từng ô, index = y * gridSize + x. Tool tự bake sau Import/Extract/Vá.")]
        public int[] ownerIds = new int[0];
        [Tooltip("Màu hiển thị từng ô, index = y * gridSize + x.")]
        public Color32[] displayPx = new Color32[0];

        // Lookup runtime, không serialize.
        private Dictionary<Vector2Int, int> _lookup;

        public int TotalCellCount
        {
            get
            {
                if (HasFlat) return gridSize * gridSize;
                int n = 0;
                if (entries != null)
                    foreach (var e in entries)
                        if (e.cells != null) n += e.cells.Count;
                return n;
            }
        }

        public bool IsValid => gridSize > 0
            && (HasFlat || (entries != null && entries.Count > 0 && TotalCellCount == gridSize * gridSize));

        public bool HasDisplayColors => IsValid;

        /// <summary>Tầng phẳng đủ dùng không (đúng grid*grid cả 2 mảng).</summary>
        public bool HasFlat =>
            ownerIds != null && displayPx != null &&
            gridSize > 0 &&
            ownerIds.Length == gridSize * gridSize &&
            displayPx.Length == ownerIds.Length;

        /// <summary>
        /// Bake tầng phẳng từ entries (tool gọi sau mỗi lần ghi).
        /// index = y * gridSize + x; y = index / gridSize; x = index % gridSize.
        /// </summary>
        public void BakeFlat()
        {
            int n = gridSize * gridSize;
            ownerIds = new int[n];
            displayPx = new Color32[n];
            for (int i = 0; i < n; i++) { ownerIds[i] = (int)YarnColor.None; displayPx[i] = new Color32(255, 255, 255, 255); }
            if (entries == null) return;
            foreach (var e in entries)
            {
                if (e.cells == null) continue;
                var px = e.displayColor;
                px.a = 1f;
                var c32 = (Color32)px;
                foreach (var cell in e.cells)
                {
                    if (cell.x < 0 || cell.y < 0 || cell.x >= gridSize || cell.y >= gridSize) continue;
                    int idx = cell.y * gridSize + cell.x;
                    ownerIds[idx] = (int)e.colorId;
                    displayPx[idx] = c32;
                }
            }
            _lookup = null;
        }

        /// <summary>
        /// Bake tầng phẳng TRỰC TIẾP từ mảng (không qua list Vector2).
        /// owners[i]/displays[i] với i = y * gridSize + x. Tool gọi sau mỗi lần ghi.
        /// </summary>
        public void BakeFlatArrays(YarnColor[] owners, Color[] displays)
        {
            int n = gridSize * gridSize;
            ownerIds = new int[n];
            displayPx = new Color32[n];
            for (int i = 0; i < n; i++)
            {
                ownerIds[i] = (owners != null && i < owners.Length) ? (int)owners[i] : (int)YarnColor.None;
                var c = (displays != null && i < displays.Length) ? displays[i] : Color.white;
                c.a = 1f;
                displayPx[i] = (Color32)c;
            }
            if (entries == null) entries = new List<ColorCellEntry>();
            else entries.Clear(); // bỏ list Vector2 — phẳng là nguồn thật
            _lookup = null;
        }

        private void EnsureLookup()
        {
            if (_lookup != null) return;
            BuildLookup();
        }

        /// <summary>
        /// Xóa cache lookup để lần đọc tới build lại từ entries hiện tại.
        /// BẮT BUỘC gọi sau khi code Editor sửa entries trực tiếp (Extract tool),
        /// nếu không GetColorId/GetDisplayColor vẫn trả dữ liệu cũ trong session.
        /// </summary>
        public void InvalidateLookup()
        {
            _lookup = null;
        }

        private void BuildLookup()
        {
            _lookup = new Dictionary<Vector2Int, int>();
            if (entries == null) return;
            for (int i = 0; i < entries.Count; i++)
            {
                var cells = entries[i].cells;
                if (cells == null) continue;
                foreach (var c in cells)
                    _lookup[c] = i;
            }
        }

        public YarnColor GetColorId(int x, int y)
        {
            if (!IsValid) return YarnColor.None;
            x = Mathf.Clamp(x, 0, gridSize - 1);
            y = Mathf.Clamp(y, 0, gridSize - 1);
            if (HasFlat) return (YarnColor)ownerIds[y * gridSize + x];
            EnsureLookup();
            if (_lookup.TryGetValue(new Vector2Int(x, y), out int i)
                && i >= 0 && i < entries.Count)
                return entries[i].colorId;
            return YarnColor.None;
        }

        public Color GetDisplayColor(int x, int y)
        {
            if (!IsValid) return Color.white;
            x = Mathf.Clamp(x, 0, gridSize - 1);
            y = Mathf.Clamp(y, 0, gridSize - 1);
            if (HasFlat)
            {
                var c = (Color)displayPx[y * gridSize + x];
                c.a = 1f;
                return c;
            }
            EnsureLookup();
            if (_lookup.TryGetValue(new Vector2Int(x, y), out int i)
                && i >= 0 && i < entries.Count)
            {
                var c = entries[i].displayColor;
                c.a = 1f;
                return c;
            }
            return Color.white;
        }

        /// <summary>Các màu có trong ma trận (đếm thẳng mảng id phẳng).</summary>
        public List<YarnColor> GetColors()
        {
            var list = new List<YarnColor>();
            if (HasFlat)
            {
                foreach (var id in ownerIds)
                {
                    var c = (YarnColor)id;
                    if (!list.Contains(c)) list.Add(c);
                }
                list.Sort((a, b) => ((int)a).CompareTo((int)b));
                return list;
            }
            if (entries == null) return list;
            foreach (var e in entries)
                if (!list.Contains(e.colorId)) list.Add(e.colorId);
            return list;
        }

        /// <summary>Số ô của 1 màu GỐC (đếm thẳng mảng id phẳng).</summary>
        public int GetCellCount(YarnColor color)
        {
            if (HasFlat)
            {
                int n = 0;
                int id = (int)color;
                foreach (var o in ownerIds)
                    if (o == id) n++;
                return n;
            }
            if (entries == null) return 0;
            int m = 0;
            foreach (var e in entries)
                if (e.colorId == color) m += e.cells != null ? e.cells.Count : 0;
            return m;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Đổi dữ liệu -> build lại lookup lần đọc tới.
            _lookup = null;
        }
#endif
    }

    /// <summary>
    /// Helper dùng chung cho mọi tool extract (kể cả tool window tự viết):
    /// chuẩn màu tham chiếu + tìm màu len gần nhất trong dataset + dựng entries.
    /// Truyền YarnColorDatabase đang mở trong Editor (không dùng Instance).
    /// </summary>
    public static class LevelColorMatrixUtility
    {
        /// <summary>Màu chuẩn của 1 YarnColor: đọc ThreadMat._MainColor, rớt xuống bảng cứng.</summary>
        public static Color RefColor(YarnColorDatabase db, YarnColor color)
        {
            var mat = db != null ? db.GetMaterial(color) : null;
            if (mat != null)
            {
                if (mat.HasProperty("_MainColor")) return mat.GetColor("_MainColor");
                if (mat.HasProperty("_Color")) return mat.GetColor("_Color");
            }
            return color switch
            {
                YarnColor.Blue => new Color(0.033f, 0.57f, 1f),
                YarnColor.Brown => new Color(0.48f, 0.30f, 0.18f),
                YarnColor.Charcoal => new Color(0.20f, 0.20f, 0.22f),
                YarnColor.Coral => new Color(0.9725f, 0.2745f, 0.2863f),
                YarnColor.Cream => new Color(1f, 0.9176f, 0.7412f),
                YarnColor.Crimson => new Color(0.75f, 0.10f, 0.20f),
                YarnColor.Deep => new Color(0.10f, 0.15f, 0.35f),
                YarnColor.Forest => new Color(0.13f, 0.42f, 0.23f),
                YarnColor.Gray => new Color(0.55f, 0.55f, 0.55f),
                YarnColor.Green => new Color(0.4235f, 0.7843f, 0.2196f),
                YarnColor.Lime => new Color(0.6196f, 0.7922f, 0.1529f),
                YarnColor.Magenta => new Color(0.90f, 0.15f, 0.70f),
                YarnColor.Orange => new Color(1f, 0.55f, 0.10f),
                YarnColor.Pink => new Color(0.9686f, 0.3294f, 0.6431f),
                YarnColor.Plum => new Color(0.55f, 0.25f, 0.45f),
                YarnColor.Purple => new Color(0.55f, 0.25f, 0.85f),
                YarnColor.Sky => new Color(0.45f, 0.75f, 1f),
                YarnColor.Snow => new Color(0.96f, 0.96f, 0.98f),
                YarnColor.Steel => new Color(0.55f, 0.62f, 0.70f),
                YarnColor.Teal => new Color(0.10f, 0.65f, 0.65f),
                YarnColor.Yellow => new Color(0.9608f, 0.8157f, 0.1451f),
                _ => Color.white
            };
        }

        /// <summary>
        /// Pixel không khớp 100% dataset thì duyệt tìm màu gần nhất (khoảng cách RGB).
        /// dataset = các màu level đang dùng (VD từ holder.colorCountEntries).
        /// </summary>
        public static YarnColor FindClosest(Color sample, List<YarnColor> dataset, YarnColorDatabase db)
        {
            if (dataset == null || dataset.Count == 0) return YarnColor.None;
            YarnColor best = dataset[0];
            float bestD = float.MaxValue;
            foreach (var c in dataset)
            {
                var cand = RefColor(db, c);
                float dr = sample.r - cand.r, dg = sample.g - cand.g, dbc = sample.b - cand.b;
                float d = dr * dr + dg * dg + dbc * dbc;
                if (d < bestD) { bestD = d; best = c; }
            }
            return best;
        }

        /// <summary>
        /// Dựng entries sparse từ mảng phẳng id từng ô (index = y * grid + x).
        /// Màu hiển thị = màu chuẩn của yarn (1 yarn 1 sắc).
        /// Sắp xếp theo int màu để file ổn định, dễ đọc.
        /// </summary>
        public static List<ColorCellEntry> BuildEntries(int[] ids, int grid, YarnColorDatabase db)
        {
            var map = new Dictionary<YarnColor, List<Vector2Int>>();
            for (int y = 0; y < grid; y++)
                for (int x = 0; x < grid; x++)
                {
                    var c = (YarnColor)ids[y * grid + x];
                    if (!map.TryGetValue(c, out var list))
                    {
                        list = new List<Vector2Int>();
                        map[c] = list;
                    }
                    list.Add(new Vector2Int(x, y));
                }

            var entries = new List<ColorCellEntry>();
            foreach (var kv in map)
            {
                var show = RefColor(db, kv.Key);
                show.a = 1f;
                entries.Add(new ColorCellEntry
                {
                    colorId = kv.Key,
                    displayColor = show,
                    cells = kv.Value
                });
            }
            entries.Sort((a, b) => ((int)a.colorId).CompareTo((int)b.colorId));
            return entries;
        }

        /// <summary>
        /// Dựng entries GIỮ MÀU PHỤ: cùng owner (màu gốc) nhưng khác display
        /// thì tách entry riêng. In lên vẫn là màu phụ, nhưng matching
        /// (GetColorId / GetCellCount / JamImage.BuildCellOwnersFromMatrix)
        /// đều tính cho màu gốc.
        /// ownerIds[y*grid+x] = YarnColor gốc, displays[y*grid+x] = màu in ra.
        /// </summary>
        public static List<ColorCellEntry> BuildEntriesPreservingDisplay(
            YarnColor[] ownerIds, Color[] displays, int grid)
        {
            var map = new Dictionary<(YarnColor, Color), List<Vector2Int>>();
            for (int y = 0; y < grid; y++)
                for (int x = 0; x < grid; x++)
                {
                    int i = y * grid + x;
                    var key = (ownerIds[i], displays[i]);
                    if (!map.TryGetValue(key, out var list))
                    {
                        list = new List<Vector2Int>();
                        map[key] = list;
                    }
                    list.Add(new Vector2Int(x, y));
                }

            var entries = new List<ColorCellEntry>();
            foreach (var kv in map)
            {
                var show = kv.Key.Item2;
                show.a = 1f;
                entries.Add(new ColorCellEntry
                {
                    colorId = kv.Key.Item1,
                    displayColor = show,
                    cells = kv.Value
                });
            }
            entries.Sort((a, b) =>
            {
                int c = ((int)a.colorId).CompareTo((int)b.colorId);
                if (c != 0) return c;
                var ca = a.displayColor;
                var cb = b.displayColor;
                int h = (ca.r + ca.g * 2f + ca.b * 3f).CompareTo(cb.r + cb.g * 2f + cb.b * 3f);
                return h;
            });
            return entries;
        }
    }
}
