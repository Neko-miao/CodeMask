// ================================================
// MaskSystem Visual - 关卡视觉数据
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 关卡视觉数据 - 定义关卡的视觉表现
    /// </summary>
    [Serializable]
    public class LevelVisualData
    {
        [Header("基本信息")]
        [Tooltip("关卡名称")]
        public string LevelName;

        [Tooltip("关卡描述")]
        [TextArea(2, 4)]
        public string Description;

        [Header("背景")]
        [Tooltip("背景图片")]
        public Sprite Background;

        [Tooltip("背景颜色（无背景图时使用）")]
        public Color BackgroundColor = new Color(0.1f, 0.1f, 0.2f);

        [Tooltip("前景装饰图（可选）")]
        public Sprite ForegroundDecor;

        [Header("氛围")]
        [Tooltip("环境光颜色")]
        public Color AmbientColor = Color.white;

        [Tooltip("雾效颜色")]
        public Color FogColor = new Color(0.5f, 0.5f, 0.5f, 0f);

        [Tooltip("粒子特效预制体（如飘落的树叶、雪花等）")]
        public GameObject AmbientParticles;

        [Header("音效")]
        [Tooltip("背景音乐")]
        public AudioClip BGM;

        [Tooltip("环境音效")]
        public AudioClip AmbientSound;

        [Tooltip("BGM音量")]
        [Range(0f, 1f)]
        public float BGMVolume = 0.7f;

        [Header("转场")]
        [Tooltip("进入关卡的标题图")]
        public Sprite TitleImage;

        [Tooltip("转场颜色")]
        public Color TransitionColor = Color.black;

        /// <summary>
        /// 创建快乐森林关卡视觉数据
        /// </summary>
        public static LevelVisualData CreateHappyForest()
        {
            return new LevelVisualData
            {
                LevelName = "快乐森林",
                Description = "欢快、诙谐的森林关卡",
                BackgroundColor = new Color(0.2f, 0.4f, 0.2f),
                AmbientColor = new Color(1f, 1f, 0.9f),
                FogColor = new Color(0.3f, 0.5f, 0.3f, 0.2f),
                TransitionColor = new Color(0.1f, 0.3f, 0.1f),
                BGMVolume = 0.7f
            };
        }

        /// <summary>
        /// 创建深海关卡视觉数据
        /// </summary>
        public static LevelVisualData CreateDeepSea()
        {
            return new LevelVisualData
            {
                LevelName = "深海",
                Description = "忧伤、孤寂、沉重的哀伤",
                BackgroundColor = new Color(0.05f, 0.1f, 0.3f),
                AmbientColor = new Color(0.6f, 0.7f, 1f),
                FogColor = new Color(0.1f, 0.2f, 0.4f, 0.4f),
                TransitionColor = new Color(0.02f, 0.05f, 0.15f),
                BGMVolume = 0.6f
            };
        }

        /// <summary>
        /// 创建天空关卡视觉数据
        /// </summary>
        public static LevelVisualData CreateSky()
        {
            return new LevelVisualData
            {
                LevelName = "天空",
                Description = "最终Boss - 龙",
                BackgroundColor = new Color(0.4f, 0.3f, 0.5f),
                AmbientColor = new Color(1f, 0.9f, 1f),
                FogColor = new Color(0.5f, 0.4f, 0.6f, 0.3f),
                TransitionColor = new Color(0.2f, 0.1f, 0.3f),
                BGMVolume = 0.8f
            };
        }
    }
}

