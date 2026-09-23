using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Color = UnityEngine.Color;

namespace _KIT.Textures
{
    public static class KitTexture
    {
        #region SPRITE

        public static Sprite CreateSprite(Texture2D texture2D)
        {
            return Sprite.Create(texture2D, new Rect(0, 0, texture2D.width, texture2D.height), new Vector2(0.5f, 0.5f), 72);
        }

        public static Sprite CreateSprite(Color[] colourMap, int width, int height)
        {
            return Sprite.Create(CreateTextureFromColor(colourMap, width, height), new Rect(0, 0, width, height),
                new Vector2(0.5f, 0.5f), 72);
        }

        #endregion

        #region MAKE TEXTURE
        
        /// <summary>
        /// Tạo texture có màu, fill tất các các pixel
        /// </summary>
        public static Texture2D CreateTextureFromColor(Color[] colourMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels(colourMap);
            texture.Apply();
            return texture;
        }

        public static Texture2D CreateTextureFromColor(Color colourMap, int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            Color[] colors = new Color[width * height];
            for (int i = 0; i < colors.Length; i++) colors[i] = colourMap;
            texture.SetPixels(colors);
            texture.Apply();
            return texture;
        }

        /// <summary>
        /// Tạo texture có màu, fill tất các các pixel có 'color.a>0' trước đó
        /// </summary>
        public static Texture2D ReplaceTextureColor(Texture2D texture, Color target)
        {
            Color[] colors = texture.GetPixels();
            for (int i = 0; i < colors.Length; i++)
            {
                Color color = colors[i];
                if (color == target) continue;
                if (color.a == 0) continue;
                colors[i] = target;
            }

            return CreateTextureFromColor(colors, texture.width, texture.height);
        }

        /// <summary>
        /// Gộp nhiều texture vào thành 1 texture
        /// </summary>
        public static Texture2D MergeTextures(List<Texture2D> textures)
        {
            int width = 0;
            int height = 0;
            foreach (var texture in textures)
            {
                if (texture == null) continue;
                if (texture.height > height) height = texture.height;
                if (texture.width > width) width = texture.width;
            }

            Texture2D output = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp
            };
            for (int x = 0; x < output.width; x++)
            {
                for (int y = 0; y < output.height; y++)
                {
                    output.SetPixel(x, y, Color.clear);
                }
            }

            foreach (var texture in textures)
            {
                if (texture == null) continue;
                for (int x = 0; x < texture.width; x++)
                {
                    for (int y = 0; y < texture.height; y++)
                    {
                        Color color = texture.GetPixel(x, y);
                        if (color.a <= 0.1f)
                            continue;
                        output.SetPixel(x, y, color);
                    }
                }
            }

            output.Apply();
            
            return output;
        }
        
        public static Texture2D CropTransparencyOfTexture(Texture2D instance, float alpha)
        {
            int[] FindLimitedTexture(Texture2D texture, float a)
            {
                int FindMinX()
                {
                    for (int x = 0; x < texture.width; x++)
                    for (int y = 0; y < texture.height; y++)
                    {
                        Color color = texture.GetPixel(x, y);
                        if (color.a > a) return x;
                    }

                    return -1;
                }

                int FindMaxX()
                {
                    for (int x = texture.width - 1; x >= 0; x--)
                    for (int y = 0; y < texture.height; y++)
                    {
                        Color color = texture.GetPixel(x, y);
                        if (color.a >= 1) return x;
                    }

                    return -1;
                }

                int FindMinY()
                {
                    for (int y = 0; y < texture.height; y++)
                    for (int x = 0; x < texture.width; x++)
                    {
                        Color color = texture.GetPixel(x, y);
                        if (color.a > a) return y;
                    }

                    return -1;
                }

                int FindMaxY()
                {
                    for (int y = texture.height - 1; y >= 0; y--)
                    for (int x = 0; x < texture.width; x++)
                    {
                        Color color = texture.GetPixel(x, y);
                        if (color.a > a) return y;
                    }

                    return -1;
                }

                return new int[] {FindMinX(), FindMaxX(), FindMinY(), FindMaxY()};
            }

            int[] vector4 = FindLimitedTexture(instance, alpha);
            for (int i = 0; i < vector4.Length; i++)
            {
                if (vector4[i] < 0) return instance;
            }

            int width = vector4[1] - vector4[0];
            int height = vector4[3] - vector4[2];
            Color[] newColors = new Color[width * height];
            for (int x = vector4[0]; x < vector4[1]; x++)
            for (int y = vector4[2]; y < vector4[3]; y++)
            {
                Color color = instance.GetPixel(x, y);
                if (color.a > alpha)
                {
                    newColors[(y - vector4[2]) * width + (x - vector4[0])] = Color.white;
                }
            }

            return CreateTextureFromColor(newColors, width, height);
        }

        public static Texture2D MakeOutlineTexture(Texture2D texture, int outlineWidth, Color outlineColor,
            bool onlyOutline)
        {
            Texture2D outlineTexture = CreateTextureFromColor(texture.GetPixels(), texture.width, texture.height);
            for (int x = 0; x < texture.width; x++)
            {
                for (int y = 0; y < texture.height; y++)
                {
                    if (texture.GetPixel(x, y).a == 0)
                    {
                        bool isOutlinePixel = false;

                        // check if pixel is on the outline
                        for (int i = -outlineWidth; i <= outlineWidth; i++)
                        {
                            for (int j = -outlineWidth; j <= outlineWidth; j++)
                            {
                                int checkX = x + i;
                                int checkY = y + j;

                                if (checkX >= 0 && checkX < texture.width && checkY >= 0 && checkY < texture.height)
                                {
                                    if (texture.GetPixel(checkX, checkY).a > 0)
                                    {
                                        isOutlinePixel = true;
                                        break;
                                    }
                                }
                            }

                            if (isOutlinePixel)
                            {
                                break;
                            }
                        }

                        if (isOutlinePixel)
                        {
                            outlineTexture.SetPixel(x, y, outlineColor);
                        }
                        else
                        {
                            if (onlyOutline) outlineTexture.SetPixel(x, y, Color.clear);
                        }
                    }
                    else
                    {
                        if (onlyOutline) outlineTexture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            outlineTexture.Apply();

            return outlineTexture;
        }

        /// <summary>
        /// Cắt texture thành nhiều texture nhỏ (cắt theo viền đen)
        /// </summary>
        public static Texture2D[] AutoCutTexture(Texture2D originalTexture2D)
        {
            List<Texture2D> results = new List<Texture2D>();
            bool[,] processedPixels = new bool[originalTexture2D.width, originalTexture2D.height];

            for (int i = 0; i < originalTexture2D.width; i++)
            {
                for (int j = 0; j < originalTexture2D.height; j++)
                {
                    if (!processedPixels[i, j])
                    {
                        Color color = originalTexture2D.GetPixel(i, j);
                        processedPixels[i, j] = true;

                        bool isBlack = color.r < 0.75f && color.g < 0.75f && color.b < 0.75f;
                        if (!isBlack && color.a > 0.9f)
                        {
                            // Find the color island that stick to this pixel
                            List<Vector2Int> processingPixels = new List<Vector2Int>();
                            List<Vector2Int> resultPixels = new List<Vector2Int>();
                            processingPixels.Add(new Vector2Int(i, j));

                            while (processingPixels.Count > 0)
                            {
                                Vector2Int currentPixel = processingPixels[0];
                                processingPixels.RemoveAt(0);
                                processedPixels[currentPixel.x, currentPixel.y] = true;

                                if (IsNotBlackAndTransparent(originalTexture2D.GetPixel(currentPixel.x,
                                    currentPixel.y)))
                                {
                                    resultPixels.Add(currentPixel);

                                    if (currentPixel.x > 0)
                                    {
                                        Vector2Int leftPixel = new Vector2Int(currentPixel.x - 1, currentPixel.y);

                                        if (!processedPixels[leftPixel.x, leftPixel.y])
                                        {
                                            processingPixels.Add(leftPixel);
                                            processedPixels[leftPixel.x, leftPixel.y] = true;
                                        }
                                    }

                                    if (currentPixel.x < originalTexture2D.width - 1)
                                    {
                                        Vector2Int rightPixel = new Vector2Int(currentPixel.x + 1, currentPixel.y);

                                        if (!processedPixels[rightPixel.x, rightPixel.y])
                                        {
                                            processingPixels.Add(rightPixel);
                                            processedPixels[rightPixel.x, rightPixel.y] = true;
                                        }
                                    }

                                    if (currentPixel.y > 0)
                                    {
                                        Vector2Int bottomPixel = new Vector2Int(currentPixel.x, currentPixel.y - 1);

                                        if (!processedPixels[bottomPixel.x, bottomPixel.y])
                                        {
                                            processingPixels.Add(bottomPixel);
                                            processedPixels[bottomPixel.x, bottomPixel.y] = true;
                                        }
                                    }

                                    if (currentPixel.y < originalTexture2D.height - 1)
                                    {
                                        Vector2Int topPixel = new Vector2Int(currentPixel.x, currentPixel.y + 1);

                                        if (!processedPixels[topPixel.x, topPixel.y])
                                        {
                                            processingPixels.Add(topPixel);
                                            processedPixels[topPixel.x, topPixel.y] = true;
                                        }
                                    }
                                }
                            }

                            Texture2D texture2D = new Texture2D(originalTexture2D.width, originalTexture2D.height);
                            for (int k = 0; k < originalTexture2D.width; k++)
                            {
                                for (int l = 0; l < originalTexture2D.height; l++)
                                {
                                    texture2D.SetPixel(k, l, new Color(0, 0, 0, 0));
                                }
                            }

                            foreach (var resultPixel in resultPixels)
                            {
                                texture2D.SetPixel(resultPixel.x, resultPixel.y,
                                    originalTexture2D.GetPixel(resultPixel.x, resultPixel.y));
                            }

                            results.Add(texture2D);
                        }
                    }


                }
            }

            return results.ToArray();
        }

        #endregion

        #region READ PIXEL
        
        /// <summary>
        /// Tìm danh sách điểm outline của texture
        /// </summary>
        public static List<Vector2Int> GetTextureOutline(Texture2D texture)
        {
            int width = texture.width;
            int height = texture.height;
            Color32[] pixels = texture.GetPixels32();
            List<Vector2Int> outlinePoints = new List<Vector2Int>();

            // Duyệt qua tất cả các điểm của texture
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Color32 pixel = pixels[index];

                    // Nếu điểm hiện tại không phải là màu nền của texture
                    if (pixel.a > 0)
                    {
                        bool isOutlinePoint = false;
                        // Kiểm tra các điểm xung quanh
                        for (int yOffset = -1; yOffset <= 1; yOffset++)
                        {
                            for (int xOffset = -1; xOffset <= 1; xOffset++)
                            {
                                // Nếu điểm xung quanh là màu nền của texture
                                if (x + xOffset >= 0 && x + xOffset < width && y + yOffset >= 0 && y + yOffset < height)
                                {
                                    if (pixels[(y + yOffset) * width + (x + xOffset)].a == 0)
                                    {
                                        isOutlinePoint = true;
                                        break;
                                    }
                                }
                            }

                            if (isOutlinePoint)
                            {
                                break;
                            }
                        }

                        if (isOutlinePoint)
                        {
                            outlinePoints.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }

            return outlinePoints;
        }

        /// <summary>
        /// Chia texture thành các ô lưới & bỏ qua các pixel có color.a = 0
        /// </summary>
        public static List<(int pointX, int pointY, Color color)> SliceTextureToGrids(Texture2D texture, int column,
            int row)
        {
            Color GetPrimaryColor(Texture2D texture2D, int startX, int startY, int width, int height)
            {
                return texture2D.GetPixel(startY + width / 2, startX + height / 2);
            }
            
            List<(int pointX, int pointY, Color color)> result = new List<(int pointX, int pointY, Color color)>();
            int cellWidth = texture.width / column;
            int cellHeight = texture.height / row;
            for (int y = 0; y < row; y++)
            {
                for (int x = 0; x < column; x++)
                {
                    int startX = y * cellWidth;
                    int startY = x * cellHeight;
                    result.Add((x, y, GetPrimaryColor(texture, startX, startY, cellWidth, cellHeight)));
                }
            }

            return result;
        }
        
        #endregion
        
        public static bool IsNotBlackAndTransparent(Color color)
        {
            bool isBlack = color.r < 0.3f && color.g < 0.3f && color.b < 0.3f;
            bool isTransparent = color.a < 0.1f;
            return !isBlack && !isTransparent;
        }
        
        /// <summary>
        /// Lưu file vào folder (c#)
        /// </summary>
        public static void SaveTexture(Texture2D texture2D, string fullPath)
        {
            byte[] bytes = texture2D.EncodeToPNG();
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory) && !string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllBytes(fullPath, bytes);
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }
    }
}