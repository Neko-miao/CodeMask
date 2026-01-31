// ================================================
// GameConfigs - 刷怪配置
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConfigs
{
    /// <summary>
    /// 刷怪点配置
    /// </summary>
    [Serializable]
    public class SpawnPointConfig
    {
        [Tooltip("刷怪点名称")]
        public string pointName = "SpawnPoint";

        [Tooltip("刷怪点位置")]
        public Vector3 position;

        [Tooltip("刷怪点旋转")]
        public Vector3 rotation;

        [Tooltip("是否启用")]
        public bool isEnabled = true;
    }

    /// <summary>
    /// 单个刷怪条目配置
    /// </summary>
    [Serializable]
    public class SpawnEntryConfig
    {
        [Tooltip("怪物类型")]
        public MonsterType monsterType;

        [Tooltip("刷怪数量")]
        [Range(1, 100)]
        public int count = 1;

        [Tooltip("刷怪延迟（秒）")]
        [Range(0f, 60f)]
        public float delay = 0f;

        [Tooltip("刷怪间隔（秒）")]
        [Range(0f, 10f)]
        public float interval = 0.5f;

        [Tooltip("指定刷怪点索引，-1表示随机")]
        public int spawnPointIndex = -1;
    }

    /// <summary>
    /// 波次配置
    /// </summary>
    [Serializable]
    public class WaveConfig
    {
        [Tooltip("波次名称")]
        public string waveName = "Wave";

        [Tooltip("波次开始延迟（秒）")]
        [Range(0f, 60f)]
        public float startDelay = 0f;

        [Tooltip("波次持续时间（秒），0表示无限")]
        [Range(0f, 300f)]
        public float duration = 0f;

        [Tooltip("该波次的刷怪条目")]
        public List<SpawnEntryConfig> spawnEntries = new List<SpawnEntryConfig>();

        [Tooltip("波次完成条件：是否需要击杀所有怪物")]
        public bool requireKillAll = true;

        [Tooltip("波次完成后的等待时间（秒）")]
        [Range(0f, 30f)]
        public float completionDelay = 2f;
    }

    /// <summary>
    /// 关卡刷怪总配置
    /// </summary>
    [Serializable]
    public class LevelSpawnConfig
    {
        [Tooltip("刷怪点列表")]
        public List<SpawnPointConfig> spawnPoints = new List<SpawnPointConfig>();

        [Tooltip("波次列表")]
        public List<WaveConfig> waves = new List<WaveConfig>();

        [Tooltip("是否循环波次")]
        public bool loopWaves = false;

        [Tooltip("最大同时存在怪物数")]
        [Range(1, 200)]
        public int maxActiveMonsters = 50;
    }
}
