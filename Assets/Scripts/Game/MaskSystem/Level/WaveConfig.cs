// ================================================
// MaskSystem - 波次配置
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 敌人波次配置 - 定义单波敌人的属性
    /// </summary>
    [Serializable]
    public class WaveConfig
    {
        [Header("敌人信息")]
        [Tooltip("敌人名称（可选，留空使用默认名称）")]
        public string EnemyName;

        [Tooltip("敌人面具类型")]
        public MaskType EnemyType = MaskType.Snake;

        [Tooltip("敌人血量")]
        [Range(1, 20)]
        public int EnemyHealth = 3;

        [Tooltip("敌人攻击力")]
        [Range(1, 10)]
        public int EnemyAttackPower = 1;

        [Header("攻击节奏")]
        [Tooltip("最小攻击间隔（秒）")]
        [Range(0.5f, 10f)]
        public float MinAttackInterval = 1.5f;

        [Tooltip("最大攻击间隔（秒）")]
        [Range(1f, 15f)]
        public float MaxAttackInterval = 3f;

        [Tooltip("攻击预警时间（秒）- 玩家反击窗口")]
        [Range(0.3f, 2f)]
        public float AttackWarningTime = 0.8f;

        [Header("额外设置")]
        [Tooltip("波次描述（用于显示）")]
        public string Description;

        /// <summary>
        /// 获取随机攻击间隔
        /// </summary>
        public float GetRandomAttackInterval()
        {
            return UnityEngine.Random.Range(MinAttackInterval, MaxAttackInterval);
        }

        /// <summary>
        /// 获取敌人显示名称
        /// </summary>
        public string GetDisplayName()
        {
            if (!string.IsNullOrEmpty(EnemyName))
                return EnemyName;

            var maskDef = MaskRegistry.GetMask(EnemyType);
            return maskDef?.Name ?? EnemyType.ToString();
        }

        /// <summary>
        /// 创建默认波次配置
        /// </summary>
        public static WaveConfig CreateDefault(MaskType enemyType)
        {
            var preset = GetPresetForMaskType(enemyType);
            return new WaveConfig
            {
                EnemyType = enemyType,
                EnemyHealth = preset.health,
                EnemyAttackPower = preset.attackPower,
                MinAttackInterval = preset.minInterval,
                MaxAttackInterval = preset.maxInterval,
                AttackWarningTime = 0.8f
            };
        }

        /// <summary>
        /// 根据面具类型获取预设属性
        /// </summary>
        private static (int health, int attackPower, float minInterval, float maxInterval) GetPresetForMaskType(MaskType type)
        {
            switch (type)
            {
                case MaskType.Snake:
                    return (3, 1, 1.5f, 3f);
                case MaskType.Cat:
                    return (3, 2, 1.2f, 2.5f);
                case MaskType.Bear:
                    return (4, 2, 2f, 4f);
                case MaskType.Bull:
                    return (5, 2, 1.8f, 3.5f);
                case MaskType.Horse:
                    return (4, 1, 2f, 3.5f);
                case MaskType.Whale:
                    return (6, 1, 2.5f, 4.5f);
                case MaskType.Shark:
                    return (4, 3, 1f, 2f);
                case MaskType.Dragon:
                    return (10, 5, 1.5f, 3f);
                default:
                    return (3, 1, 2f, 3f);
            }
        }
    }
}

