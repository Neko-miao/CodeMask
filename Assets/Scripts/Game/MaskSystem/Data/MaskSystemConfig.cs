// ================================================
// MaskSystem - 系统配置（ScriptableObject）
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 敌人预设配置
    /// </summary>
    [Serializable]
    public class EnemyPreset
    {
        public string Name;
        public MaskType MaskType;
        public int Health;
        public int AttackPower;

        public EnemyPreset(string name, MaskType maskType, int health, int attackPower)
        {
            Name = name;
            MaskType = maskType;
            Health = health;
            AttackPower = attackPower;
        }
    }

    /// <summary>
    /// 面具系统配置 - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MaskSystemConfig", menuName = "MaskSystem/Config", order = 1)]
    public class MaskSystemConfig : ScriptableObject
    {
        [Header("玩家配置")]
        [Tooltip("玩家基础血量")]
        public int PlayerBaseHealth = 3;

        [Tooltip("每个面具增加的最大血量")]
        public int HealthPerMask = 1;

        [Tooltip("玩家最大面具槽位数")]
        public int MaxMaskSlots = 3;

        [Tooltip("玩家初始面具")]
        public MaskType InitialMask = MaskType.Horse;

        [Header("战斗配置")]
        [Tooltip("克制时的伤害倍率")]
        public float CounterDamageMultiplier = 2f;

        [Header("敌人预设")]
        [SerializeField]
        private List<EnemyPreset> _enemyPresets = new List<EnemyPreset>();

        /// <summary>
        /// 获取敌人预设列表
        /// </summary>
        public IReadOnlyList<EnemyPreset> EnemyPresets => _enemyPresets;

        /// <summary>
        /// 获取默认配置实例（运行时使用）
        /// </summary>
        public static MaskSystemConfig CreateDefault()
        {
            var config = CreateInstance<MaskSystemConfig>();
            config.PlayerBaseHealth = 3;
            config.HealthPerMask = 1;
            config.MaxMaskSlots = 3;
            config.InitialMask = MaskType.Horse;
            config.CounterDamageMultiplier = 2f;
            config._enemyPresets = new List<EnemyPreset>
            {
                new EnemyPreset("蛇", MaskType.Snake, 3, 1),
                new EnemyPreset("猫", MaskType.Cat, 3, 2),
                new EnemyPreset("熊", MaskType.Bear, 4, 2),
                new EnemyPreset("牛", MaskType.Bull, 5, 2),
                new EnemyPreset("鲸鱼", MaskType.Whale, 6, 1),
                new EnemyPreset("鲨鱼", MaskType.Shark, 4, 3),
                new EnemyPreset("龙", MaskType.Dragon, 10, 5)
            };
            return config;
        }

        /// <summary>
        /// 根据面具类型获取敌人预设
        /// </summary>
        public EnemyPreset GetEnemyPreset(MaskType maskType)
        {
            foreach (var preset in _enemyPresets)
            {
                if (preset.MaskType == maskType)
                    return preset;
            }
            return null;
        }

        /// <summary>
        /// 根据索引获取敌人预设
        /// </summary>
        public EnemyPreset GetEnemyPreset(int index)
        {
            if (index >= 0 && index < _enemyPresets.Count)
                return _enemyPresets[index];
            return null;
        }
    }
}

