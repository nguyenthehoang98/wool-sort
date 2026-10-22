namespace _Game.Scripts.LevelData
{
    using System;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>Mức độ của 1 lỗi kiểm tra catalog.</summary>
    public enum LevelCatalogSeverity
    {
        Info = 0,
        Warning = 1,
        Error = 2,
    }

    /// <summary>1 vấn đề khi kiểm tra catalog.</summary>
    public struct LevelCatalogIssue
    {
        public LevelCatalogSeverity severity;
        public string message;
        public int orderIndex;
        public int levelId;

        public override string ToString()
        {
            string where = orderIndex > 0 ? $" [order {orderIndex}, id {levelId}]" : string.Empty;
            return $"[{severity}] {message}{where}";
        }
    }

    /// <summary>
    /// Danh mục màn chơi — nguồn dữ liệu DUY NHẤT cho thứ tự chơi, id màn và độ khó.
    ///
    /// Trước đây code hardcode `Resources.Load("Levels/Level" + currentLevel)` nên
    /// không biết màn nào tồn tại, không biết độ khó, và không thể kiểm tra trước khi build.
    /// Catalog gom hết thông tin đó vào 1 asset để validate được.
    ///
    /// Nguồn: `Assets/Resources/LevelCatalog.asset` (sinh bằng
    /// `Tools/Level/Build Level Catalog` từ `tools/level_reference.json`).
    ///
    /// Runtime API nằm ở đây (không phụ thuộc UnityEditor) nên test EditMode kiểm tra được.
    /// </summary>
    [CreateAssetMenu(fileName = "LevelCatalog", menuName = "Game/Level Catalog", order = 102)]
    public class LevelCatalog : ScriptableObject
    {
        /// <summary>Đường dẫn Resources để load bằng `Resources.Load&lt;LevelCatalog&gt;("LevelCatalog")`.</summary>
        public const string ResourcePath = "LevelCatalog";

        [Tooltip("Danh sách màn, sắp theo orderIndex tăng dần (1-based, liên tục).")]
        [SerializeField] private List<LevelDefinition> levels = new List<LevelDefinition>();

        [Tooltip("Nguồn dữ liệu đã sinh catalog (ví dụ: tools/level_reference.json).")]
        [SerializeField] private string generatedFrom = string.Empty;

        [Tooltip("Thời điểm sinh catalog (UTC ISO-8601).")]
        [SerializeField] private string generatedAtUtc = string.Empty;

        private Dictionary<int, LevelDefinition> _byOrder;
        private Dictionary<int, LevelDefinition> _byLevelId;

        public IReadOnlyList<LevelDefinition> Levels => levels;
        public string GeneratedFrom => generatedFrom;
        public string GeneratedAtUtc => generatedAtUtc;

        /// <summary>Tổng số màn trong catalog (kể cả màn chưa có asset).</summary>
        public int TotalLevels => levels?.Count ?? 0;

        /// <summary>Số màn thực sự chơi được (có prefab).</summary>
        public int PlayableCount
        {
            get
            {
                if (levels == null) return 0;
                int n = 0;
                foreach (var l in levels) if (l != null && l.HasAsset) n++;
                return n;
            }
        }

        // ------------------------------------------------------------------
        // Truy vấn
        // ------------------------------------------------------------------

        public LevelDefinition GetByOrder(int orderIndex)
        {
            EnsureIndex();
            return _byOrder.TryGetValue(orderIndex, out var def) ? def : null;
        }

        public LevelDefinition GetByLevelId(int levelId)
        {
            EnsureIndex();
            return _byLevelId.TryGetValue(levelId, out var def) ? def : null;
        }

        /// <summary>
        /// Asset ID của màn ở vị trí <paramref name="orderIndex"/>.
        /// Nếu catalog không có màn đó → trả về chính orderIndex (fallback hành vi cũ).
        /// </summary>
        public int GetLevelIdByOrder(int orderIndex)
        {
            var def = GetByOrder(orderIndex);
            return def != null ? def.levelId : orderIndex;
        }

        /// <summary>Độ khó của màn ở vị trí order (Normal nếu không tìm thấy).</summary>
        public LevelModeType GetModeTypeByOrder(int orderIndex)
        {
            var def = GetByOrder(orderIndex);
            return def != null ? def.modeType : LevelModeType.Normal;
        }

        public bool IsPlayable(int orderIndex)
        {
            var def = GetByOrder(orderIndex);
            return def != null && def.HasAsset;
        }

        /// <summary>
        /// Order tiếp theo sau <paramref name="currentOrder"/> mà có asset chơi được.
        /// Trả 0 nếu không còn màn nào (không tự quay vòng — caller tự quyết định).
        /// </summary>
        public int GetNextPlayableOrder(int currentOrder)
        {
            if (levels == null) return 0;
            for (int i = 0; i < levels.Count; i++)
            {
                int order = levels[i]?.orderIndex ?? 0;
                if (order > currentOrder && levels[i] != null && levels[i].HasAsset) return order;
            }
            return 0;
        }

        /// <summary>Order đầu tiên có asset chơi được (0 nếu không có).</summary>
        public int GetFirstPlayableOrder()
        {
            if (levels == null) return 0;
            for (int i = 0; i < levels.Count; i++)
                if (levels[i] != null && levels[i].HasAsset) return levels[i].orderIndex;
            return 0;
        }

        /// <summary>Order lớn nhất có asset chơi được (0 nếu không có).</summary>
        public int GetLastPlayableOrder()
        {
            int max = 0;
            if (levels == null) return 0;
            foreach (var l in levels)
                if (l != null && l.HasAsset && l.orderIndex > max) max = l.orderIndex;
            return max;
        }

        // ------------------------------------------------------------------
        // Kiểm tra (thuần logic — không dùng UnityEditor nên test được)
        // ------------------------------------------------------------------

        /// <summary>
        /// Kiểm tra tính toàn vẹn của catalog. Trả về danh sách lỗi (Error) và cảnh báo.
        /// Quy tắc:
        /// - orderIndex phải liên tục từ 1, không trùng.
        /// - levelId không được trùng.
        /// - Mọi màn phải có prefab, và tên prefab phải khớp `Level{levelId}`.
        /// - modeType phải nằm trong khoảng hợp lệ.
        /// </summary>
        public List<LevelCatalogIssue> Validate()
        {
            var issues = new List<LevelCatalogIssue>();
            if (levels == null || levels.Count == 0)
            {
                issues.Add(new LevelCatalogIssue
                {
                    severity = LevelCatalogSeverity.Error,
                    message = "Catalog rỗng — chưa có màn nào.",
                });
                return issues;
            }

            var seenOrder = new HashSet<int>();
            var seenLevelId = new HashSet<int>();
            var sortedOrders = new List<int>();

            foreach (var def in levels)
            {
                if (def == null)
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = "Có entry null trong danh sách màn.",
                    });
                    continue;
                }

                sortedOrders.Add(def.orderIndex);

                if (def.orderIndex <= 0)
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = "orderIndex phải >= 1.",
                        orderIndex = def.orderIndex,
                        levelId = def.levelId,
                    });
                }
                else if (!seenOrder.Add(def.orderIndex))
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = "orderIndex bị trùng.",
                        orderIndex = def.orderIndex,
                        levelId = def.levelId,
                    });
                }

                if (def.levelId <= 0)
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = "levelId phải >= 1.",
                        orderIndex = def.orderIndex,
                        levelId = def.levelId,
                    });
                }
                else if (!seenLevelId.Add(def.levelId))
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = "levelId bị trùng.",
                        orderIndex = def.orderIndex,
                        levelId = def.levelId,
                    });
                }

                if (!Enum.IsDefined(typeof(LevelModeType), def.modeType))
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = $"modeType không hợp lệ: {def.modeType}.",
                        orderIndex = def.orderIndex,
                        levelId = def.levelId,
                    });
                }

                if (!def.HasAsset)
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = "Thiếu prefab màn (level sẽ không chơi được).",
                        orderIndex = def.orderIndex,
                        levelId = def.levelId,
                    });
                }
                else if (!string.IsNullOrEmpty(def.prefab.name) && def.prefab.name != $"Level{def.levelId}")
                {
                    issues.Add(new LevelCatalogIssue
                    {
                        severity = LevelCatalogSeverity.Error,
                        message = $"Tên prefab '{def.prefab.name}' không khớp levelId (cần 'Level{def.levelId}').",
                        orderIndex = def.orderIndex,
                        levelId = def.levelId,
                    });
                }
            }

            sortedOrders.Sort();
            for (int i = 0; i < sortedOrders.Count; i++)
            {
                if (sortedOrders[i] == i + 1) continue;
                issues.Add(new LevelCatalogIssue
                {
                    severity = LevelCatalogSeverity.Error,
                    message = $"orderIndex không liên tục từ 1 — mong đợi {i + 1} nhưng gặp {sortedOrders[i]}.",
                    orderIndex = sortedOrders[i],
                });
                break;
            }

            return issues;
        }

        /// <summary>True nếu không có lỗi mức Error.</summary>
        public bool IsValid()
        {
            foreach (var issue in Validate())
                if (issue.severity == LevelCatalogSeverity.Error) return false;
            return true;
        }

        // ------------------------------------------------------------------
        // Ghi dữ liệu (tool Editor gọi)
        // ------------------------------------------------------------------

        /// <summary>Thay toàn bộ danh sách màn (dùng bởi tool sinh catalog).</summary>
        public void SetLevels(List<LevelDefinition> newLevels, string source = null, string timestampUtc = null)
        {
            levels = newLevels ?? new List<LevelDefinition>();
            levels.Sort((a, b) => (a?.orderIndex ?? 0).CompareTo(b?.orderIndex ?? 0));
            if (source != null) generatedFrom = source;
            if (timestampUtc != null) generatedAtUtc = timestampUtc;
            InvalidateIndex();
        }

        public void InvalidateIndex() => _byOrder = null;

        private void EnsureIndex()
        {
            if (_byOrder != null) return;
            _byOrder = new Dictionary<int, LevelDefinition>();
            _byLevelId = new Dictionary<int, LevelDefinition>();
            if (levels == null) return;
            foreach (var l in levels)
            {
                if (l == null) continue;
                if (!_byOrder.ContainsKey(l.orderIndex)) _byOrder[l.orderIndex] = l;
                if (!_byLevelId.ContainsKey(l.levelId)) _byLevelId[l.levelId] = l;
            }
        }
    }
}
