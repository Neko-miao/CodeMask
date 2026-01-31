// ================================================
// MaskSystem Visual - 面具视觉数据
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 面具视觉数据 - 定义单个面具的视觉表现
    /// </summary>
    [Serializable]
    public class MaskVisualData
    {
        [Header("基本信息")]
        [Tooltip("面具类型")]
        public MaskType Type;

        [Tooltip("面具显示名称")]
        public string DisplayName;

        [Header("图片资源")]
        [Tooltip("面具图标（用于UI显示）")]
        public Sprite Icon;

        [Tooltip("佩戴面具的角色图（用于场景显示）")]
        public Sprite CharacterSprite;

        [Tooltip("面具单独图片")]
        public Sprite MaskSprite;

        [Header("视觉风格")]
        [Tooltip("主题颜色")]
        public Color ThemeColor = Color.white;

        [Tooltip("发光颜色")]
        public Color GlowColor = Color.white;

        [Tooltip("图标背景颜色")]
        public Color BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);

        [Header("特效")]
        [Tooltip("攻击特效预制体")]
        public GameObject AttackEffect;

        [Tooltip("受击特效预制体")]
        public GameObject HitEffect;

        [Tooltip("切换面具特效预制体")]
        public GameObject SwitchEffect;

        [Header("动画")]
        [Tooltip("待机动画控制器")]
        public RuntimeAnimatorController IdleAnimator;

        [Tooltip("攻击动画控制器")]
        public RuntimeAnimatorController AttackAnimator;

        /// <summary>
        /// 获取显示名称
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(DisplayName))
                return DisplayName;

            var def = MaskRegistry.GetMask(Type);
            return def?.Name ?? Type.ToString();
        }

        /// <summary>
        /// 创建默认视觉数据
        /// </summary>
        public static MaskVisualData CreateDefault(MaskType type)
        {
            var data = new MaskVisualData
            {
                Type = type,
                DisplayName = "",
                ThemeColor = GetDefaultColor(type),
                GlowColor = GetDefaultColor(type),
                BackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f)
            };
            return data;
        }

        /// <summary>
        /// 获取默认主题色
        /// </summary>
        private static Color GetDefaultColor(MaskType type)
        {
            switch (type)
            {
                case MaskType.Cat: return new Color(1f, 0.8f, 0.4f);      // 橙黄
                case MaskType.Snake: return new Color(0.4f, 0.8f, 0.4f);   // 绿
                case MaskType.Bear: return new Color(0.6f, 0.4f, 0.2f);    // 棕
                case MaskType.Horse: return new Color(0.8f, 0.6f, 0.4f);   // 浅棕
                case MaskType.Bull: return new Color(0.8f, 0.2f, 0.2f);    // 红
                case MaskType.Whale: return new Color(0.4f, 0.6f, 0.9f);   // 蓝
                case MaskType.Shark: return new Color(0.5f, 0.5f, 0.6f);   // 灰蓝
                case MaskType.Dragon: return new Color(0.9f, 0.3f, 0.9f);  // 紫
                default: return Color.white;
            }
        }
    }
}

