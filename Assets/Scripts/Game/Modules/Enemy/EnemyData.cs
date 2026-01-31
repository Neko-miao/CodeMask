using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 单个敌人配置
    /// </summary>
    [Serializable]
    public class EnemyConfig
    {
        [Header("基本信息")]
        [Tooltip("敌人ID")]
        public int enemyId;

        [Tooltip("敌人名称")]
        public string enemyName;

        [Tooltip("敌人描述")]
        [TextArea(2, 4)]
        public string description;

        [Header("Prefab设置")]
        [Tooltip("敌人Prefab")]
        public GameObject enemyPrefab;

        [Header("属性设置")]
        [Tooltip("最大生命值")]
        public float maxHealth = 100f;

        [Tooltip("攻击力")]
        public float attackPower = 10f;

        [Tooltip("攻击速度")]
        public float attackSpeed = 1f; // 每几个节拍攻击一次
    }

    /// <summary>
    /// 敌人数据配置 - ScriptableObject，存储所有敌人配置列表
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Game/Enemy/EnemyData", order = 1)]
    public class EnemyData : ScriptableObject
    {
        [Header("敌人配置列表")]
        [Tooltip("所有敌人配置")]
        [SerializeField]
        private List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();

        #region Properties

        /// <summary>
        /// 获取所有敌人配置
        /// </summary>
        public IReadOnlyList<EnemyConfig> EnemyConfigs => enemyConfigs;

        /// <summary>
        /// 敌人配置数量
        /// </summary>
        public int Count => enemyConfigs.Count;

        #endregion

        #region Public Methods

        /// <summary>
        /// 根据索引获取敌人配置
        /// </summary>
        public EnemyConfig GetConfig(int index)
        {
            if (index >= 0 && index < enemyConfigs.Count)
            {
                return enemyConfigs[index];
            }
            return null;
        }

        /// <summary>
        /// 根据敌人ID获取敌人配置
        /// </summary>
        public EnemyConfig GetConfigById(int enemyId)
        {
            foreach (var config in enemyConfigs)
            {
                if (config.enemyId == enemyId)
                {
                    return config;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据敌人名称获取敌人配置
        /// </summary>
        public EnemyConfig GetConfigByName(string enemyName)
        {
            foreach (var config in enemyConfigs)
            {
                if (config.enemyName == enemyName)
                {
                    return config;
                }
            }
            return null;
        }

        /// <summary>
        /// 获取随机敌人配置
        /// </summary>
        public EnemyConfig GetRandomConfig()
        {
            if (enemyConfigs.Count == 0)
            {
                return null;
            }
            int randomIndex = UnityEngine.Random.Range(0, enemyConfigs.Count);
            return enemyConfigs[randomIndex];
        }

        /// <summary>
        /// 检查是否包含指定ID的敌人配置
        /// </summary>
        public bool ContainsId(int enemyId)
        {
            foreach (var config in enemyConfigs)
            {
                if (config.enemyId == enemyId)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion
    }
}
