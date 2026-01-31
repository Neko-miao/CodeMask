using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 节奏行为类型
    /// </summary>
    public enum RhythmActionType
    {
        /// <summary>
        /// 进攻
        /// </summary>
        Attack,

        /// <summary>
        /// 防御
        /// </summary>
        Defense,

        /// <summary>
        /// 发呆
        /// </summary>
        Idle
    }

    /// <summary>
    /// 单个节奏配置 - 只包含Prefab和行为类型
    /// </summary>
    [Serializable]
    public class RhythmConfig
    {
        [Tooltip("节奏Prefab")]
        public GameObject prefab;

        [Tooltip("行为类型")]
        public RhythmActionType actionType;
    }

    /// <summary>
    /// 节奏数据 - ScriptableObject，缓存不同Prefab的列表
    /// </summary>
    [CreateAssetMenu(fileName = "RhythmData", menuName = "Game/Rhythm/RhythmData", order = 1)]
    public class RhythmData : ScriptableObject
    {
        [Header("节奏配置列表")]
        [Tooltip("所有节奏配置")]
        [SerializeField]
        private List<RhythmConfig> rhythmConfigs = new List<RhythmConfig>();

        #region Properties

        /// <summary>
        /// 获取所有节奏配置
        /// </summary>
        public IReadOnlyList<RhythmConfig> RhythmConfigs => rhythmConfigs;

        /// <summary>
        /// 配置数量
        /// </summary>
        public int Count => rhythmConfigs.Count;

        #endregion

        #region Public Methods

        /// <summary>
        /// 根据索引获取配置
        /// </summary>
        public RhythmConfig GetConfig(int index)
        {
            if (index >= 0 && index < rhythmConfigs.Count)
            {
                return rhythmConfigs[index];
            }
            return null;
        }

        /// <summary>
        /// 根据行为类型获取配置
        /// </summary>
        public RhythmConfig GetConfigByActionType(RhythmActionType actionType)
        {
            foreach (var config in rhythmConfigs)
            {
                if (config.actionType == actionType)
                {
                    return config;
                }
            }
            return null;
        }

        /// <summary>
        /// 根据行为类型获取所有配置
        /// </summary>
        public List<RhythmConfig> GetAllConfigsByActionType(RhythmActionType actionType)
        {
            List<RhythmConfig> result = new List<RhythmConfig>();
            foreach (var config in rhythmConfigs)
            {
                if (config.actionType == actionType)
                {
                    result.Add(config);
                }
            }
            return result;
        }

        /// <summary>
        /// 获取进攻类型的配置
        /// </summary>
        public RhythmConfig GetAttackConfig()
        {
            return GetConfigByActionType(RhythmActionType.Attack);
        }

        /// <summary>
        /// 获取防御类型的配置
        /// </summary>
        public RhythmConfig GetDefenseConfig()
        {
            return GetConfigByActionType(RhythmActionType.Defense);
        }

        /// <summary>
        /// 获取发呆类型的配置
        /// </summary>
        public RhythmConfig GetIdleConfig()
        {
            return GetConfigByActionType(RhythmActionType.Idle);
        }

        #endregion
    }
}
