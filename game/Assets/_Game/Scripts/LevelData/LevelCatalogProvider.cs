namespace _Game.Scripts.LevelData
{
    using UnityEngine;

    /// <summary>
    /// Nơi duy nhất runtime lấy <see cref="LevelCatalog"/>.
    ///
    /// Catalog nằm ở `Resources/LevelCatalog.asset` nên load được mà không cần
    /// gắn reference trong scene (giống `Resources/Levels/` và `Resources/LevelColorMatrix/`).
    /// Nếu asset chưa được sinh (project mới, hoặc tool chưa chạy) thì `Catalog == null`
    /// và caller phải fallback về hành vi cũ — xem <see cref="HasCatalog"/>.
    /// </summary>
    public static class LevelCatalogProvider
    {
        private static LevelCatalog _catalog;

        /// <summary>
        /// Catalog đã load (null nếu chưa có asset trong Resources).
        ///
        /// Luôn kiểm tra lại khi `_catalog` bị Unity "giết" (asset bị regenerate trong
        /// Editor) — dùng toán tử `==` của Unity nên object đã destroy cũng bằng null.
        /// `Resources.Load` đã được Unity cache nên gọi lại không tốn.
        /// </summary>
        public static LevelCatalog Catalog
        {
            get
            {
                if (_catalog == null)
                    _catalog = Resources.Load<LevelCatalog>(LevelCatalog.ResourcePath);
                return _catalog;
            }
        }

        public static bool HasCatalog => Catalog != null;

        /// <summary>Xoá cache — chỉ dùng trong test hoặc sau khi sinh lại catalog lúc play.</summary>
        public static void ResetCache()
        {
            _catalog = null;
        }
    }
}
