using UnityEngine;
using System.Collections.Generic;
using Game.MaskSystem;

namespace Game.MaskSystem.Visual.Placeholder
{
    /// <summary>
    /// 程序化生成占位符资源，用于在没有美术资源时快速测试游戏
    /// </summary>
    public static class PlaceholderAssets
    {
        private static Dictionary<MaskType, Color> _maskColors = new Dictionary<MaskType, Color>
        {
            { MaskType.None, Color.gray },
            { MaskType.Cat, new Color(1f, 0.6f, 0.2f) },      // 橙色
            { MaskType.Snake, new Color(0.2f, 0.8f, 0.2f) },  // 绿色
            { MaskType.Bear, new Color(0.6f, 0.3f, 0.1f) },   // 棕色
            { MaskType.Horse, new Color(0.8f, 0.7f, 0.5f) },  // 米色
            { MaskType.Bull, new Color(0.8f, 0.2f, 0.2f) },   // 红色
            { MaskType.Whale, new Color(0.3f, 0.5f, 0.9f) },  // 蓝色
            { MaskType.Shark, new Color(0.4f, 0.4f, 0.5f) },  // 灰蓝色
            { MaskType.Dragon, new Color(0.9f, 0.1f, 0.9f) }  // 紫色
        };

        private static Dictionary<string, Color> _levelColors = new Dictionary<string, Color>
        {
            { "快乐森林", new Color(0.2f, 0.5f, 0.2f) },
            { "深海", new Color(0.1f, 0.2f, 0.4f) },
            { "天空", new Color(0.5f, 0.7f, 0.9f) }
        };

        /// <summary>
        /// 创建一个带有面具符号的占位符精灵
        /// </summary>
        public static Sprite CreateMaskSprite(MaskType maskType, int size = 128)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color baseColor = GetMaskColor(maskType);
            Color darkColor = baseColor * 0.6f;
            darkColor.a = 1f;

            // 填充背景
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            // 绘制圆形面具
            int centerX = size / 2;
            int centerY = size / 2;
            int radius = size / 2 - 4;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(centerX, centerY));
                    if (dist < radius)
                    {
                        // 渐变效果
                        float t = dist / radius;
                        Color c = Color.Lerp(baseColor, darkColor, t * 0.5f);
                        texture.SetPixel(x, y, c);
                    }
                    else if (dist < radius + 2)
                    {
                        // 边框
                        texture.SetPixel(x, y, Color.black);
                    }
                }
            }

            // 绘制面具特征（简单的眼睛和嘴巴）
            DrawEyes(texture, centerX, centerY, size / 6, maskType);
            DrawMouth(texture, centerX, centerY - size / 6, size / 4, maskType);

            texture.Apply();
            texture.filterMode = FilterMode.Point;

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// 创建角色占位符精灵
        /// </summary>
        public static Sprite CreateCharacterSprite(bool isPlayer, MaskType maskType, int width = 64, int height = 96)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color maskColor = GetMaskColor(maskType);
            Color bodyColor = isPlayer ? new Color(0.3f, 0.6f, 0.9f) : new Color(0.9f, 0.3f, 0.3f);

            // 清空
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }

            // 绘制身体（矩形）
            int bodyLeft = width / 4;
            int bodyRight = width * 3 / 4;
            int bodyTop = height / 2;
            int bodyBottom = 4;

            for (int y = bodyBottom; y < bodyTop; y++)
            {
                for (int x = bodyLeft; x < bodyRight; x++)
                {
                    texture.SetPixel(x, y, bodyColor);
                }
            }

            // 绘制头部（圆形带面具颜色）
            int headCenterX = width / 2;
            int headCenterY = height * 2 / 3;
            int headRadius = width / 3;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(headCenterX, headCenterY));
                    if (dist < headRadius)
                    {
                        texture.SetPixel(x, y, maskColor);
                    }
                    else if (dist < headRadius + 2)
                    {
                        texture.SetPixel(x, y, Color.black);
                    }
                }
            }

            // 绘制简单的眼睛
            DrawSimpleEyes(texture, headCenterX, headCenterY, headRadius / 3);

            texture.Apply();
            texture.filterMode = FilterMode.Point;

            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// 创建背景占位符
        /// </summary>
        public static Sprite CreateBackgroundSprite(string levelName, int width = 1920, int height = 1080)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color baseColor = _levelColors.ContainsKey(levelName) ? _levelColors[levelName] : Color.gray;
            Color topColor = baseColor * 1.3f;
            topColor.a = 1f;

            // 渐变背景
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / height;
                Color c = Color.Lerp(baseColor * 0.5f, topColor, t);
                for (int x = 0; x < width; x++)
                {
                    texture.SetPixel(x, y, c);
                }
            }

            // 添加一些装饰元素
            AddBackgroundDecorations(texture, width, height, levelName);

            texture.Apply();
            texture.filterMode = FilterMode.Bilinear;

            return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
        }

        /// <summary>
        /// 创建简单的UI图标
        /// </summary>
        public static Sprite CreateUIIcon(Color color, int size = 32)
        {
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(size / 2, size / 2));
                    if (dist < size / 2 - 2)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else if (dist < size / 2)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            texture.filterMode = FilterMode.Point;

            return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        public static Color GetMaskColor(MaskType maskType)
        {
            return _maskColors.ContainsKey(maskType) ? _maskColors[maskType] : Color.gray;
        }

        public static string GetMaskSymbol(MaskType maskType)
        {
            switch (maskType)
            {
                case MaskType.Cat: return "猫";
                case MaskType.Snake: return "蛇";
                case MaskType.Bear: return "熊";
                case MaskType.Horse: return "马";
                case MaskType.Bull: return "牛";
                case MaskType.Whale: return "鲸";
                case MaskType.Shark: return "鲨";
                case MaskType.Dragon: return "龙";
                default: return "?";
            }
        }

        private static void DrawEyes(Texture2D texture, int centerX, int centerY, int eyeSpacing, MaskType maskType)
        {
            int eyeSize = eyeSpacing / 2;
            int eyeY = centerY + eyeSpacing / 2;

            // 左眼
            DrawCircle(texture, centerX - eyeSpacing / 2, eyeY, eyeSize, Color.white);
            DrawCircle(texture, centerX - eyeSpacing / 2, eyeY, eyeSize / 2, Color.black);

            // 右眼
            DrawCircle(texture, centerX + eyeSpacing / 2, eyeY, eyeSize, Color.white);
            DrawCircle(texture, centerX + eyeSpacing / 2, eyeY, eyeSize / 2, Color.black);
        }

        private static void DrawSimpleEyes(Texture2D texture, int centerX, int centerY, int eyeSpacing)
        {
            // 左眼
            DrawCircle(texture, centerX - eyeSpacing, centerY + 2, 3, Color.white);
            DrawCircle(texture, centerX - eyeSpacing, centerY + 2, 1, Color.black);

            // 右眼
            DrawCircle(texture, centerX + eyeSpacing, centerY + 2, 3, Color.white);
            DrawCircle(texture, centerX + eyeSpacing, centerY + 2, 1, Color.black);
        }

        private static void DrawMouth(Texture2D texture, int centerX, int centerY, int width, MaskType maskType)
        {
            // 简单的嘴巴线条
            for (int x = centerX - width / 2; x < centerX + width / 2; x++)
            {
                texture.SetPixel(x, centerY, Color.black);
                texture.SetPixel(x, centerY - 1, Color.black);
            }
        }

        private static void DrawCircle(Texture2D texture, int cx, int cy, int radius, Color color)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                for (int x = cx - radius; x <= cx + radius; x++)
                {
                    if (x >= 0 && x < texture.width && y >= 0 && y < texture.height)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                        if (dist <= radius)
                        {
                            texture.SetPixel(x, y, color);
                        }
                    }
                }
            }
        }

        private static void AddBackgroundDecorations(Texture2D texture, int width, int height, string levelName)
        {
            System.Random rand = new System.Random(levelName.GetHashCode());

            if (levelName == "快乐森林")
            {
                // 添加树木剪影
                for (int i = 0; i < 10; i++)
                {
                    int x = rand.Next(0, width);
                    int treeHeight = rand.Next(100, 300);
                    DrawTree(texture, x, 0, treeHeight, new Color(0.1f, 0.3f, 0.1f, 0.5f));
                }
            }
            else if (levelName == "深海")
            {
                // 添加气泡
                for (int i = 0; i < 30; i++)
                {
                    int x = rand.Next(0, width);
                    int y = rand.Next(0, height);
                    int size = rand.Next(5, 20);
                    DrawCircle(texture, x, y, size, new Color(0.6f, 0.8f, 1f, 0.3f));
                }
            }
            else if (levelName == "天空")
            {
                // 添加云朵
                for (int i = 0; i < 8; i++)
                {
                    int x = rand.Next(0, width);
                    int y = rand.Next(height / 2, height);
                    DrawCloud(texture, x, y, rand.Next(50, 150), new Color(1f, 1f, 1f, 0.5f));
                }
            }
        }

        private static void DrawTree(Texture2D texture, int x, int baseY, int height, Color color)
        {
            // 树干
            for (int y = baseY; y < baseY + height / 3; y++)
            {
                for (int dx = -5; dx <= 5; dx++)
                {
                    int px = x + dx;
                    if (px >= 0 && px < texture.width && y >= 0 && y < texture.height)
                    {
                        texture.SetPixel(px, y, color * 0.7f);
                    }
                }
            }

            // 树冠
            int crownY = baseY + height / 3;
            int crownRadius = height / 3;
            DrawCircle(texture, x, crownY + crownRadius, crownRadius, color);
        }

        private static void DrawCloud(Texture2D texture, int x, int y, int size, Color color)
        {
            DrawCircle(texture, x, y, size / 2, color);
            DrawCircle(texture, x - size / 3, y - size / 6, size / 3, color);
            DrawCircle(texture, x + size / 3, y - size / 6, size / 3, color);
        }
    }
}

