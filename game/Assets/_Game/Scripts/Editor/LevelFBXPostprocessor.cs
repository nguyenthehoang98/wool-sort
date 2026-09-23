using UnityEditor;

namespace _Game.Scripts.Editor
{
    public class LevelFBXPostprocessor : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            var importer = assetImporter as ModelImporter;
            if (importer == null) return;

            string folderPath = "Assets/_Game/Mesh/";
            if (!assetPath.StartsWith(folderPath)) return; // Chỉ áp dụng trong folder này
            
            string fileName = System.IO.Path.GetFileNameWithoutExtension(assetPath);
            if (!fileName.StartsWith("Level")) return;

            // Tự động cấu hình
            importer.importAnimation = false;
            importer.meshCompression = ModelImporterMeshCompression.High;
            importer.isReadable = true;
        }
    }
}