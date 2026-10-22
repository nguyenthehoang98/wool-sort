namespace _Game.Scripts.LevelData
{
    using System;
    using UnityEngine;

    /// <summary>
    /// Định nghĩa 1 màn chơi trong <see cref="LevelCatalog"/>.
    ///
    /// Phân biệt 2 khái niệm (theo đúng bản gốc):
    /// - <see cref="orderIndex"/>: THỨ TỰ CHƠI (1-based, liên tục). Đây là số
    ///   người chơi thấy trên UI và số lưu trong save.
    /// - <see cref="levelId"/>: ASSET ID — dùng để tìm prefab `Levels/Level{levelId}`
    ///   và ảnh `Image{levelId}`. Trong bản gốc 2 cái này KHÁC nhau (order là
    ///   danh sách xáo trộn 88 mức, id là id thật của prefab).
    ///
    /// Trong target hiện tại chỉ có Level1..Level7 nên `orderIndex == levelId`.
    /// Khi copy đủ content, `referenceOrderIndex` giữ lại vị trí trong danh sách
    /// order gốc để chuyển sang thứ tự gốc mà không phải map lại tay.
    /// </summary>
    [Serializable]
    public class LevelDefinition
    {
        [Tooltip("Thứ tự chơi, 1-based, liên tục không trùng. Số này hiện ra cho người chơi.")]
        public int orderIndex = 1;

        [Tooltip("Asset ID của màn: dùng cho prefab Resources/Levels/Level{id} và ảnh Image{id}.")]
        public int levelId = 1;

        [Tooltip("Vị trí của màn này trong GeneralConfig.levelOrder của dự án gốc (-1 = không có trong danh sách gốc).")]
        public int referenceOrderIndex = -1;

        [Tooltip("Độ khó: Normal / Hard / VeryHard. Lấy từ levelModeType của dự án gốc.")]
        public LevelModeType modeType = LevelModeType.Normal;

        [Tooltip("Prefab màn trong Resources/Levels. Để trống = chưa có asset (level sẽ bị bỏ qua khi chơi).")]
        public GameObject prefab;

        [Tooltip("Seed cố định để random màu lặp lại y hệt mỗi lần chơi (xem W5-04).")]
        public int seed;

        /// <summary>Có asset để chơi được không.</summary>
        public bool HasAsset => prefab != null;

        /// <summary>Đường dẫn Resources của prefab màn (dùng khi không có ref trực tiếp).</summary>
        public string PrefabResourcePath => $"Levels/Level{levelId}";

        /// <summary>Tên ảnh màn (JamImage tìm theo tên này).</summary>
        public string ImageName => $"Level{levelId}";

        /// <summary>Seed mặc định theo levelId — cùng seed + cùng input thì ra cùng kết quả.</summary>
        public static int DefaultSeed(int levelId) => levelId * 7919 + 104729;

        public LevelDefinition Clone()
        {
            return new LevelDefinition
            {
                orderIndex = orderIndex,
                levelId = levelId,
                referenceOrderIndex = referenceOrderIndex,
                modeType = modeType,
                prefab = prefab,
                seed = seed,
            };
        }
    }
}
