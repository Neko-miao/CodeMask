// ================================================
// MaskSystem Visual - 游戏资源配置
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 游戏资源配置 - ScriptableObject
    /// 用于配置所有游戏视觉资源
    /// </summary>
    [CreateAssetMenu(fileName = "GameAssetsConfig", menuName = "MaskSystem/Game Assets Config", order = 10)]
    public class GameAssetsConfig : ScriptableObject
    {
        [Header("面具资源")]
        [Tooltip("所有面具的视觉数据")]
        [SerializeField]
        private List<MaskVisualData> _maskVisuals = new List<MaskVisualData>();

        [Header("关卡资源")]
        [Tooltip("所有关卡的视觉数据")]
        [SerializeField]
        private List<LevelVisualData> _levelVisuals = new List<LevelVisualData>();

        [Header("通用UI资源")]
        [Tooltip("血条填充图")]
        public Sprite HealthBarFill;

        [Tooltip("血条背景图")]
        public Sprite HealthBarBackground;

        [Tooltip("预警条填充图")]
        public Sprite WarningBarFill;

        [Tooltip("面具槽位背景")]
        public Sprite MaskSlotBackground;

        [Tooltip("面具槽位选中框")]
        public Sprite MaskSlotSelected;

        [Header("通用特效")]
        [Tooltip("受击特效")]
        public GameObject DefaultHitEffect;

        [Tooltip("治疗特效")]
        public GameObject HealEffect;

        [Tooltip("升级特效")]
        public GameObject LevelUpEffect;

        [Header("通用音效")]
        [Tooltip("攻击音效")]
        public AudioClip AttackSound;

        [Tooltip("受击音效")]
        public AudioClip HitSound;

        [Tooltip("切换面具音效")]
        public AudioClip SwitchMaskSound;

        [Tooltip("胜利音效")]
        public AudioClip VictorySound;

        [Tooltip("失败音效")]
        public AudioClip DefeatSound;

        [Header("字体")]
        [Tooltip("主字体")]
        public Font MainFont;

        [Tooltip("数字字体")]
        public Font NumberFont;

        #region 面具资源访问

        /// <summary>
        /// 获取面具视觉数据
        /// </summary>
        public MaskVisualData GetMaskVisual(MaskType type)
        {
            foreach (var visual in _maskVisuals)
            {
                if (visual.Type == type)
                    return visual;
            }
            return null;
        }

        /// <summary>
        /// 获取所有面具视觉数据
        /// </summary>
        public IReadOnlyList<MaskVisualData> MaskVisuals => _maskVisuals;

        /// <summary>
        /// 添加面具视觉数据
        /// </summary>
        public void AddMaskVisual(MaskVisualData visual)
        {
            // 检查是否已存在
            for (int i = 0; i < _maskVisuals.Count; i++)
            {
                if (_maskVisuals[i].Type == visual.Type)
                {
                    _maskVisuals[i] = visual;
                    return;
                }
            }
            _maskVisuals.Add(visual);
        }

        /// <summary>
        /// 移除面具视觉数据
        /// </summary>
        public void RemoveMaskVisual(MaskType type)
        {
            _maskVisuals.RemoveAll(v => v.Type == type);
        }

        #endregion

        #region 关卡资源访问

        /// <summary>
        /// 获取关卡视觉数据
        /// </summary>
        public LevelVisualData GetLevelVisual(string levelName)
        {
            foreach (var visual in _levelVisuals)
            {
                if (visual.LevelName == levelName)
                    return visual;
            }
            return null;
        }

        /// <summary>
        /// 获取关卡视觉数据（按索引）
        /// </summary>
        public LevelVisualData GetLevelVisual(int index)
        {
            if (index >= 0 && index < _levelVisuals.Count)
                return _levelVisuals[index];
            return null;
        }

        /// <summary>
        /// 获取所有关卡视觉数据
        /// </summary>
        public IReadOnlyList<LevelVisualData> LevelVisuals => _levelVisuals;

        /// <summary>
        /// 添加关卡视觉数据
        /// </summary>
        public void AddLevelVisual(LevelVisualData visual)
        {
            _levelVisuals.Add(visual);
        }

        #endregion

        #region 创建默认配置

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static GameAssetsConfig CreateDefault()
        {
            var config = CreateInstance<GameAssetsConfig>();

            // 添加默认面具视觉数据
            config._maskVisuals = new List<MaskVisualData>
            {
                MaskVisualData.CreateDefault(MaskType.Cat),
                MaskVisualData.CreateDefault(MaskType.Snake),
                MaskVisualData.CreateDefault(MaskType.Bear),
                MaskVisualData.CreateDefault(MaskType.Horse),
                MaskVisualData.CreateDefault(MaskType.Bull),
                MaskVisualData.CreateDefault(MaskType.Whale),
                MaskVisualData.CreateDefault(MaskType.Shark),
                MaskVisualData.CreateDefault(MaskType.Dragon)
            };

            // 添加默认关卡视觉数据
            config._levelVisuals = new List<LevelVisualData>
            {
                LevelVisualData.CreateHappyForest(),
                LevelVisualData.CreateDeepSea(),
                LevelVisualData.CreateSky()
            };

            return config;
        }

        /// <summary>
        /// 初始化默认值（用于Inspector中Reset）
        /// </summary>
        private void Reset()
        {
            _maskVisuals = new List<MaskVisualData>
            {
                MaskVisualData.CreateDefault(MaskType.Cat),
                MaskVisualData.CreateDefault(MaskType.Snake),
                MaskVisualData.CreateDefault(MaskType.Bear),
                MaskVisualData.CreateDefault(MaskType.Horse),
                MaskVisualData.CreateDefault(MaskType.Bull),
                MaskVisualData.CreateDefault(MaskType.Whale),
                MaskVisualData.CreateDefault(MaskType.Shark),
                MaskVisualData.CreateDefault(MaskType.Dragon)
            };

            _levelVisuals = new List<LevelVisualData>
            {
                LevelVisualData.CreateHappyForest(),
                LevelVisualData.CreateDeepSea(),
                LevelVisualData.CreateSky()
            };
        }

        #endregion
    }
}

