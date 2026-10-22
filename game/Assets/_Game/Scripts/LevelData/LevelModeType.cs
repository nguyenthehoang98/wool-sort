/// <summary>
/// Độ khó của một màn chơi. Tách riêng khỏi `Utils/Configs.cs` để nằm trong
/// assembly `WoolSort.LevelData` — assembly này không phụ thuộc Assembly-CSharp
/// nên test EditMode và tool Editor đều dùng được.
///
/// Giữ nguyên tên và giá trị như bản gốc (Normal = 0, Hard = 1, VeryHard = 2)
/// để map trực tiếp với `levelModeType` trong GeneralConfig của dự án gốc.
/// </summary>
public enum LevelModeType
{
    Normal = 0,
    Hard = 1,
    VeryHard = 2,
}
