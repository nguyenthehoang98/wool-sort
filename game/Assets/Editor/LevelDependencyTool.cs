using System.IO;
using System.Linq;
using UnityEditor;

public static class LevelDependencyTool
{
    public static void ExportLevelDependencies()
    {
        var dependencies = Enumerable.Range(1, 10)
            .SelectMany(i => AssetDatabase.GetDependencies($"Assets/Resources/Levels/Level{i}.prefab", true))
            .Distinct()
            .OrderBy(path => path)
            .ToArray();
        File.WriteAllLines("Temp/level-dependencies.txt", dependencies);
        UnityEngine.Debug.Log($"Level dependency count: {dependencies.Length}; mesh assets: {dependencies.Count(path => path.StartsWith("Assets/_Game/Mesh/Level Mesh/"))}");
    }
}
