using System;
using System.IO;
using UnityEngine;

namespace _Game.Scripts
{
    /// <summary>
    /// Ghi log chẩn đoán level ra file để dev chơi xong gửi lại phân tích.
    /// File: {persistentDataPath}/woolsort-debug-<level>-<time>.log (đường dẫn in ra Console lúc mở level).
    /// Tự ghi: START-CHECK đầu game, pool rỗng, PrintImage lỗi. Vừa ghi file vừa hiện Console.
    /// </summary>
    public static class LevelDebugLog
    {
        private static string filePath;
        public static string FilePath => filePath;

        public static void Init(string levelName)
        {
            try
            {
                // Chạy trong Editor: lưu vào thư mục project để lấy dễ (đọc trực tiếp được).
                // Build thật: lưu vào persistentDataPath.
                string dir;
                if (Application.isEditor)
                    dir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "WoolSortDebugLogs"));
                else
                    dir = Application.persistentDataPath;
                Directory.CreateDirectory(dir);
                string stamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                filePath = Path.Combine(dir, $"woolsort-debug-{levelName}-{stamp}.log");
                File.WriteAllText(filePath,
                    $"[SESSION] {levelName} at {DateTime.Now}\n");
                Debug.Log($"[LevelDebugLog] Ghi log ra file: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogWarning("[LevelDebugLog] Không tạo được file log: " + ex.Message);
            }
        }

        public static void Log(string line)
        {
            Debug.Log(line);
            Append("[LOG] " + line);
        }

        public static void LogWarning(string line)
        {
            Debug.LogWarning(line);
            Append("[WARN] " + line);
        }

        public static void LogError(string line)
        {
            Debug.LogError(line);
            Append("[ERROR] " + line);
        }

        private static void Append(string line)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            try
            {
                File.AppendAllText(filePath, $"[{DateTime.Now:HH:mm:ss}] {line}\n");
            }
            catch
            {
                // Không để log file làm crash game.
            }
        }
    }
}
