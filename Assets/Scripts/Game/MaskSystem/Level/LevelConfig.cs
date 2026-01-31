// ================================================
// MaskSystem - 关卡配置
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 关卡配置 - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "MaskSystem/Level Config", order = 2)]
    public class LevelConfig : ScriptableObject
    {
        [Header("关卡信息")]
        [Tooltip("关卡名称")]
        public string LevelName = "新关卡";

        [Tooltip("关卡描述")]
        [TextArea(2, 4)]
        public string Description;

        [Tooltip("关卡编号")]
        public int LevelIndex = 1;

        [Header("关卡设置")]
        [Tooltip("玩家初始面具（如果玩家没有面具时使用）")]
        public MaskType PlayerInitialMask = MaskType.Horse;

        [Tooltip("关卡开始前的准备时间（秒）")]
        [Range(0f, 5f)]
        public float PrepareTime = 2f;

        [Tooltip("波次之间的间隔时间（秒）")]
        [Range(0f, 5f)]
        public float WaveInterval = 1.5f;

        [Header("敌人波次")]
        [Tooltip("敌人波次列表")]
        [SerializeField]
        private List<WaveConfig> _waves = new List<WaveConfig>();

        /// <summary>
        /// 波次列表
        /// </summary>
        public IReadOnlyList<WaveConfig> Waves => _waves;

        /// <summary>
        /// 波次数量
        /// </summary>
        public int WaveCount => _waves.Count;

        /// <summary>
        /// 获取指定波次配置
        /// </summary>
        public WaveConfig GetWave(int index)
        {
            if (index >= 0 && index < _waves.Count)
                return _waves[index];
            return null;
        }

        /// <summary>
        /// 添加波次
        /// </summary>
        public void AddWave(WaveConfig wave)
        {
            _waves.Add(wave);
        }

        /// <summary>
        /// 移除波次
        /// </summary>
        public void RemoveWave(int index)
        {
            if (index >= 0 && index < _waves.Count)
                _waves.RemoveAt(index);
        }

        /// <summary>
        /// 清空波次
        /// </summary>
        public void ClearWaves()
        {
            _waves.Clear();
        }

        /// <summary>
        /// 创建默认关卡配置（用于运行时测试）
        /// </summary>
        public static LevelConfig CreateDefault()
        {
            var config = CreateInstance<LevelConfig>();
            config.LevelName = "测试关卡";
            config.Description = "自动生成的测试关卡";
            config.LevelIndex = 1;
            config.PrepareTime = 2f;
            config.WaveInterval = 1.5f;

            // 添加默认波次
            config._waves = new List<WaveConfig>
            {
                WaveConfig.CreateDefault(MaskType.Snake),
                WaveConfig.CreateDefault(MaskType.Cat),
                WaveConfig.CreateDefault(MaskType.Bear)
            };

            return config;
        }

        /// <summary>
        /// 创建指定敌人序列的关卡
        /// </summary>
        public static LevelConfig CreateWithEnemies(string levelName, params MaskType[] enemies)
        {
            var config = CreateInstance<LevelConfig>();
            config.LevelName = levelName;
            config.LevelIndex = 1;
            config.PrepareTime = 2f;
            config.WaveInterval = 1.5f;
            config._waves = new List<WaveConfig>();

            foreach (var enemy in enemies)
            {
                config._waves.Add(WaveConfig.CreateDefault(enemy));
            }

            return config;
        }

        /// <summary>
        /// 获取关卡摘要信息
        /// </summary>
        public string GetSummary()
        {
            string enemies = "";
            foreach (var wave in _waves)
            {
                enemies += $"{wave.GetDisplayName()}, ";
            }
            if (enemies.Length > 2)
                enemies = enemies.Substring(0, enemies.Length - 2);

            return $"[{LevelName}] 波次: {WaveCount} | 敌人: {enemies}";
        }

        #region 预设关卡

        /// <summary>
        /// 创建关卡一：快乐森林
        /// </summary>
        public static LevelConfig CreateLevel1_HappyForest()
        {
            var config = CreateInstance<LevelConfig>();
            config.LevelName = "快乐森林";
            config.Description = "欢快、诙谐的森林关卡";
            config.LevelIndex = 1;
            config.PrepareTime = 2f;
            config.WaveInterval = 1.5f;
            config._waves = new List<WaveConfig>
            {
                new WaveConfig
                {
                    EnemyName = "蛇",
                    EnemyType = MaskType.Snake,
                    EnemyHealth = 3,
                    EnemyAttackPower = 1,
                    MinAttackInterval = 2f,
                    MaxAttackInterval = 3.5f,
                    AttackWarningTime = 1f,
                    Description = "引导关卡 - 基础战斗"
                },
                new WaveConfig
                {
                    EnemyName = "猫",
                    EnemyType = MaskType.Cat,
                    EnemyHealth = 3,
                    EnemyAttackPower = 2,
                    MinAttackInterval = 1.5f,
                    MaxAttackInterval = 3f,
                    AttackWarningTime = 0.8f,
                    Description = "猫克制蛇"
                },
                new WaveConfig
                {
                    EnemyName = "熊",
                    EnemyType = MaskType.Bear,
                    EnemyHealth = 4,
                    EnemyAttackPower = 2,
                    MinAttackInterval = 2f,
                    MaxAttackInterval = 4f,
                    AttackWarningTime = 0.8f,
                    Description = "熊克制蛇"
                },
                new WaveConfig
                {
                    EnemyName = "牛",
                    EnemyType = MaskType.Bull,
                    EnemyHealth = 5,
                    EnemyAttackPower = 2,
                    MinAttackInterval = 1.8f,
                    MaxAttackInterval = 3.5f,
                    AttackWarningTime = 0.7f,
                    Description = "牛的破防攻击"
                }
            };
            return config;
        }

        /// <summary>
        /// 创建关卡二：深海
        /// </summary>
        public static LevelConfig CreateLevel2_DeepSea()
        {
            var config = CreateInstance<LevelConfig>();
            config.LevelName = "深海";
            config.Description = "忧伤、孤寂、沉重的哀伤";
            config.LevelIndex = 2;
            config.PrepareTime = 2f;
            config.WaveInterval = 2f;
            config._waves = new List<WaveConfig>
            {
                new WaveConfig
                {
                    EnemyName = "鲸鱼",
                    EnemyType = MaskType.Whale,
                    EnemyHealth = 6,
                    EnemyAttackPower = 1,
                    MinAttackInterval = 2.5f,
                    MaxAttackInterval = 4.5f,
                    AttackWarningTime = 1f,
                    Description = "血厚但攻击力低"
                },
                new WaveConfig
                {
                    EnemyName = "鲨鱼",
                    EnemyType = MaskType.Shark,
                    EnemyHealth = 4,
                    EnemyAttackPower = 3,
                    MinAttackInterval = 1f,
                    MaxAttackInterval = 2f,
                    AttackWarningTime = 0.6f,
                    Description = "高攻击力快速攻击"
                }
            };
            return config;
        }

        /// <summary>
        /// 创建关卡三：天空（最终Boss）
        /// </summary>
        public static LevelConfig CreateLevel3_Sky()
        {
            var config = CreateInstance<LevelConfig>();
            config.LevelName = "天空";
            config.Description = "最终Boss - 龙";
            config.LevelIndex = 3;
            config.PrepareTime = 3f;
            config.WaveInterval = 0f;
            config._waves = new List<WaveConfig>
            {
                new WaveConfig
                {
                    EnemyName = "龙",
                    EnemyType = MaskType.Dragon,
                    EnemyHealth = 10,
                    EnemyAttackPower = 5,
                    MinAttackInterval = 1.5f,
                    MaxAttackInterval = 3f,
                    AttackWarningTime = 0.5f,
                    Description = "最终Boss，血量和攻击都超高"
                }
            };
            return config;
        }

        #endregion
    }
}

