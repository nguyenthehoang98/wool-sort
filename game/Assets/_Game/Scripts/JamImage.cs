using System.Collections.Generic;
using _Game.Scripts.Gameplay;
using UnityEngine;

namespace _Game.Scripts
{
    /// <summary>
    /// Reveals the 28x28 target image when a box is completed.
    /// The imported image is kept in Resources so the existing Board prefab needs no new wiring.
    /// </summary>
    public sealed class JamImage : MonoBehaviour
    {
        public static JamImage Instance { get; private set; }

        [SerializeField] private SpriteRenderer colorTexture;
        [SerializeField] private Transform pointer;

        private const int GridSize = 28;
        private const int SourceBlockSize = 28;

        private readonly List<YarnColor> cellOwners = new();
        private readonly Dictionary<YarnColor, int> colorCounts = new();
        private Texture2D targetTexture;
        private Texture2D runtimeTexture;
        private Sprite runtimeSprite;
        private Color[] runtimePixels;
        private bool[] revealedCells;
        private bool initialized;

        public Transform Pointer => pointer != null ? pointer : transform;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            colorTexture ??= transform.Find("colorTexture")?.GetComponent<SpriteRenderer>();
            colorTexture ??= GetComponentInChildren<SpriteRenderer>();
        }

        public void Initialize(IReadOnlyList<ColorCountEntry> levelColors, string levelName = null)
        {
            // Load theo level: Resources/Images/{levelName} - LOI NEU KO TIM THAY
            if (!string.IsNullOrEmpty(levelName))
            {
                targetTexture = Resources.Load<Texture2D>($"Images/{levelName}");
                if (targetTexture == null) targetTexture = Resources.Load<Texture2D>($"Images/Level_{levelName}");
                if (targetTexture == null) targetTexture = Resources.Load<Texture2D>($"LevelImages/{levelName}");
            }
            // KHONG CO THEO DÕNG IMAGE3 - LOI NEU KO TIM THAY
            if (targetTexture == null || colorTexture == null)
            {
                Debug.LogError($"JamImage: missing texture for level {levelName}. Please add Resources/Images/{levelName}.png.");
                return;
            }
            // Ho tro anh size bat ky - khong check cung 784 nua, se sample theo ti le
            if (targetTexture.width != GridSize * SourceBlockSize)
            {
                Debug.Log($"JamImage: {levelName} texture {targetTexture.width}x{targetTexture.height} != {GridSize*SourceBlockSize}, se sample theo ti le bilinear.");
            }

            if (runtimeSprite != null)
                Destroy(runtimeSprite);
            if (runtimeTexture != null)
                Destroy(runtimeTexture);

            // The board is a 28x28 pixel grid. The source image stores each grid pixel as a 28x28 block.
            runtimeTexture = new Texture2D(GridSize, GridSize, TextureFormat.RGBA32, false);
            runtimeTexture.name = "BoardImageRuntime";
            runtimeTexture.filterMode = FilterMode.Point;
            runtimeTexture.wrapMode = TextureWrapMode.Clamp;
            runtimePixels = new Color[GridSize * GridSize];
            revealedCells = new bool[GridSize * GridSize];
            runtimeTexture.SetPixels(runtimePixels);
            runtimeTexture.Apply(false, false);

            runtimeSprite = Sprite.Create(
                runtimeTexture,
                new Rect(0, 0, GridSize, GridSize),
                new Vector2(0.5f, 0.5f),
                100f);
            colorTexture.sprite = runtimeSprite;
            colorTexture.color = Color.white;

            BuildCellOwners(levelColors);
            colorCounts.Clear();
            if (levelColors != null)
            {
                foreach (var entry in levelColors)
                    colorCounts[entry.color] = colorCounts.TryGetValue(entry.color, out var count)
                        ? count + entry.count
                        : entry.count;
            }

            initialized = true;
        }

        public int GetNextPixelCount(YarnColor color)
        {
            return BuildNextRevealCells(color).Count;
        }

        public bool Print(YarnColor color, float timePerPixel)
        {
            if (!initialized || runtimePixels == null || targetTexture == null)
                return false;

            var cells = BuildNextRevealCells(color, true);
            if (cells.Count == 0)
                return false;

            StartCoroutine(RevealPixels(cells, Mathf.Max(0.001f, timePerPixel)));
            return true;
        }

        private List<Vector2Int> BuildNextRevealCells(YarnColor color, bool reserve = false)
        {
            var cells = new List<Vector2Int>();
            if (!initialized || !colorCounts.TryGetValue(color, out var yarnCount))
                return cells;

            // Ref game reveals one batch per box. Three yarns fill one box.
            var boxCount = Mathf.Max(1, Mathf.CeilToInt(yarnCount / 3f));
            var totalPixels = 0;
            for (var index = 0; index < cellOwners.Count; index++)
                if (cellOwners[index] == color)
                    totalPixels++;
            var batchSize = Mathf.Max(1, Mathf.CeilToInt((float)totalPixels / boxCount));

            // Match ref JamImage.DoPrintf: top row to bottom row, left to right.
            for (var cellY = GridSize - 1; cellY >= 0 && cells.Count < batchSize; cellY--)
            {
                for (var cellX = 0; cellX < GridSize && cells.Count < batchSize; cellX++)
                {
                    var index = cellY * GridSize + cellX;
                    if (cellOwners[index] == color && !revealedCells[index])
                    {
                        cells.Add(new Vector2Int(cellX, cellY));
                        if (reserve)
                            revealedCells[index] = true;
                    }
                }
            }

            // Match ref BoxManager.CalculatePoint: alternate direction for each row.
            var ordered = new List<Vector2Int>(cells.Count);
            var leftToRight = true;
            for (var cellY = GridSize - 1; cellY >= 0; cellY--)
            {
                var row = cells.FindAll(cell => cell.y == cellY);
                if (row.Count == 0)
                    continue;

                if (!leftToRight)
                    row.Reverse();
                ordered.AddRange(row);
                leftToRight = !leftToRight;
            }

            return ordered;
        }

        private System.Collections.IEnumerator RevealPixels(List<Vector2Int> cells, float timePerPixel)
        {
            foreach (var cell in cells)
            {
                var index = cell.y * GridSize + cell.x;
                MovePointerTo(cell.x, cell.y);
                // Sample theo ti le de ho tro anh size bat ky (36x36, 512x512...)
                int sx = Mathf.Clamp(Mathf.RoundToInt((cell.x + 0.5f) / GridSize * targetTexture.width), 0, targetTexture.width - 1);
                int sy = Mathf.Clamp(Mathf.RoundToInt((cell.y + 0.5f) / GridSize * targetTexture.height), 0, targetTexture.height - 1);
                runtimePixels[index] = targetTexture.GetPixel(sx, sy);
                revealedCells[index] = true;
                runtimeTexture.SetPixel(cell.x, cell.y, runtimePixels[index]);
                runtimeTexture.Apply(false, false);

                yield return new WaitForSeconds(timePerPixel);
            }
        }

        private void MovePointerTo(int cellX, int cellY)
        {
            if (pointer == null || runtimeSprite == null)
                return;

            var bounds = runtimeSprite.bounds;
            var x = Mathf.Lerp(bounds.min.x, bounds.max.x, (cellX + 0.5f) / GridSize);
            var y = Mathf.Lerp(bounds.min.y, bounds.max.y, (cellY + 0.5f) / GridSize);
            pointer.localPosition = new Vector3(x, y, pointer.localPosition.z);
        }

        private void BuildCellOwners(IReadOnlyList<ColorCountEntry> levelColors)
        {
            cellOwners.Clear();
            var availableColors = new List<YarnColor>();
            if (levelColors != null)
            {
                foreach (var entry in levelColors)
                {
                    if (entry.count > 0 && !availableColors.Contains(entry.color))
                        availableColors.Add(entry.color);
                }
            }

            if (availableColors.Count == 0)
            {
                availableColors.Add(YarnColor.Lime);
                availableColors.Add(YarnColor.Yellow);
                availableColors.Add(YarnColor.Pink);
            }

            for (var cellY = 0; cellY < GridSize; cellY++)
            {
                for (var cellX = 0; cellX < GridSize; cellX++)
                {
                    int sx = Mathf.Clamp(Mathf.RoundToInt((cellX + 0.5f) / GridSize * targetTexture.width), 0, targetTexture.width - 1);
                    int sy = Mathf.Clamp(Mathf.RoundToInt((cellY + 0.5f) / GridSize * targetTexture.height), 0, targetTexture.height - 1);
                    var sample = targetTexture.GetPixel(sx, sy);
                    cellOwners.Add(FindClosestColor(sample, availableColors));
                }
            }
        }

        private static YarnColor FindClosestColor(Color sample, List<YarnColor> availableColors)
        {
            var bestColor = availableColors[0];
            var bestDistance = float.MaxValue;
            foreach (var color in availableColors)
            {
                var candidate = GetColor(color);
                var dr = sample.r - candidate.r;
                var dg = sample.g - candidate.g;
                var db = sample.b - candidate.b;
                var distance = dr * dr + dg * dg + db * db;
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    bestColor = color;
                }
            }

            return bestColor;
        }

        private static Color GetColor(YarnColor color)
        {
            // ThreadMat_* uses _MainColor/_DarkThreadColor, not Unity's _Color property.
            // Keep this lookup local so image setup cannot spam shader-property errors.
            return color switch
            {
                YarnColor.Coral => new Color(0.9725f, 0.2745f, 0.2863f),
                YarnColor.Cream => new Color(1f, 0.9176f, 0.7412f),
                YarnColor.Green => new Color(0.4235f, 0.7843f, 0.2196f),
                YarnColor.Lime => new Color(0.6196f, 0.7922f, 0.1529f),
                YarnColor.Yellow => new Color(0.9608f, 0.8157f, 0.1451f),
                YarnColor.Pink => new Color(0.9686f, 0.3294f, 0.6431f),
                _ => Color.white
            };
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
            if (runtimeSprite != null)
                Destroy(runtimeSprite);
            if (runtimeTexture != null)
                Destroy(runtimeTexture);
        }
    }
}
