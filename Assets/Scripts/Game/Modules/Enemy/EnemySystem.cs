using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameFramework.Session;

namespace Game
{
    /// <summary>
    /// 敌人系统 - 管理敌人生成，监听关卡事件
    /// </summary>
    public class EnemySystem : MonoBehaviour
    {
        [Header("EnemyLauncher引用（ActorLauncher）")]
        [Tooltip("敌人启动器引用，实际为ActorLauncher")]
        [SerializeField]
        private ActorLauncher enemyLauncher;

        [Header("敌人数据配置")]
        [Tooltip("敌人数据配置（包含所有敌人配置列表）")]
        [SerializeField]
        private EnemyData enemyData;

        [Header("关卡敌人配置")]
        [Tooltip("不同关卡对应的敌人ID映射")]
        [SerializeField]
        private List<LevelEnemyConfig> levelEnemyConfigs = new List<LevelEnemyConfig>();

        /// <summary>
        /// GameSession引用
        /// </summary>
        private IGameSession gameSession;

        /// <summary>
        /// LevelMgr引用
        /// </summary>
        private ILevelMgr levelMgr;

        /// <summary>
        /// 是否已订阅事件
        /// </summary>
        private bool isSubscribed = false;

        #region Properties

        /// <summary>
        /// 获取EnemyLauncher（ActorLauncher）
        /// </summary>
        public ActorLauncher EnemyLauncher => enemyLauncher;

        /// <summary>
        /// 获取敌人数据配置
        /// </summary>
        public EnemyData EnemyData => enemyData;

        #endregion

        void Start()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化系统
        /// </summary>
        public void Initialize()
        {
            // 获取GameSession
            gameSession = GameInstance.Instance?.GetComp<IGameSession>();
            
            if (gameSession != null)
            {
                levelMgr = gameSession.LevelMgr;
                SubscribeToEvents();
            }
            else
            {
                Debug.LogWarning("[EnemySystem] GameSession未找到，将在稍后重试订阅事件");
                // 延迟重试
                Invoke(nameof(RetrySubscribe), 0.5f);
            }

            Debug.Log("[EnemySystem] 初始化完成");
        }

        /// <summary>
        /// 重试订阅事件
        /// </summary>
        private void RetrySubscribe()
        {
            if (isSubscribed) return;

            gameSession = GameInstance.Instance?.GetComp<IGameSession>();
            if (gameSession != null)
            {
                levelMgr = gameSession.LevelMgr;
                SubscribeToEvents();
            }
        }

        /// <summary>
        /// 订阅LevelMgr事件
        /// </summary>
        private void SubscribeToEvents()
        {
            if (isSubscribed) return;

            if (gameSession != null)
            {
                // 订阅关卡完成事件
                gameSession.OnLevelCompleted += OnLevelCompleted;
                
                // 订阅关卡加载完成事件
                gameSession.OnLevelLoaded += OnLevelLoaded;

                isSubscribed = true;
                Debug.Log("[EnemySystem] 已订阅LevelMgr事件");
            }

            // 也可以直接订阅LevelMgr的事件
            if (levelMgr != null)
            {
                levelMgr.OnLevelCompleted += OnLevelMgrLevelCompleted;
                levelMgr.OnLevelStarted += OnLevelStarted;
            }
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            if (!isSubscribed) return;

            if (gameSession != null)
            {
                gameSession.OnLevelCompleted -= OnLevelCompleted;
                gameSession.OnLevelLoaded -= OnLevelLoaded;
            }

            if (levelMgr != null)
            {
                levelMgr.OnLevelCompleted -= OnLevelMgrLevelCompleted;
                levelMgr.OnLevelStarted -= OnLevelStarted;
            }

            isSubscribed = false;
            Debug.Log("[EnemySystem] 已取消订阅LevelMgr事件");
        }

        #region Event Handlers

        /// <summary>
        /// 关卡完成事件处理（来自GameSession）
        /// </summary>
        private void OnLevelCompleted(ILevel level, LevelResult result)
        {
            Debug.Log($"[EnemySystem] 关卡完成: LevelId={level?.LevelId}, IsSuccess={result?.IsSuccess}");

            if (result != null && result.IsSuccess)
            {
                SpawnEnemiesOnLevelComplete(level);
            }
        }

        /// <summary>
        /// 关卡完成事件处理（来自LevelMgr）
        /// </summary>
        private void OnLevelMgrLevelCompleted(ILevel level, LevelResult result)
        {
            // 这里可以添加额外的处理逻辑
            Debug.Log($"[EnemySystem] LevelMgr关卡完成事件: LevelId={level?.LevelId}");
        }

        /// <summary>
        /// 关卡加载完成事件处理
        /// </summary>
        private void OnLevelLoaded(ILevel level)
        {
            Debug.Log($"[EnemySystem] 关卡加载完成: LevelId={level?.LevelId}");
            
            // 根据关卡ID设置对应的敌人数据
            SetEnemyDataForLevel(level?.LevelId ?? 0);
        }

        /// <summary>
        /// 关卡开始事件处理
        /// </summary>
        private void OnLevelStarted(ILevel level)
        {
            Debug.Log($"[EnemySystem] 关卡开始: LevelId={level?.LevelId}");
        }

        #endregion

        #region Enemy Spawning

        /// <summary>
        /// 关卡完成时生成敌人
        /// </summary>
        private void SpawnEnemiesOnLevelComplete(ILevel level)
        {
            if (enemyLauncher == null)
            {
                Debug.LogWarning("[EnemySystem] EnemyLauncher未设置，无法生成敌人");
                return;
            }

            if (enemyData == null)
            {
                Debug.LogWarning("[EnemySystem] EnemyData未设置，无法生成敌人");
                return;
            }

            // 根据关卡ID获取对应的敌人配置
            EnemyConfig configToSpawn = GetEnemyConfigForLevel(level?.LevelId ?? 0);

            if (configToSpawn != null && configToSpawn.enemyPrefab != null)
            {
                // 使用ActorLauncher的SetPrefabAndRespawn方法生成敌人
                enemyLauncher.SetPrefabAndRespawn(configToSpawn.enemyPrefab);
                Debug.Log($"[EnemySystem] 关卡 {level?.LevelId} 完成，生成敌人: {configToSpawn.enemyName}");
            }
            else
            {
                Debug.LogWarning("[EnemySystem] 没有找到对应关卡的敌人配置或Prefab，无法生成敌人");
            }
        }

        /// <summary>
        /// 根据关卡ID获取敌人配置
        /// </summary>
        private EnemyConfig GetEnemyConfigForLevel(int levelId)
        {
            // 先查找关卡配置映射
            foreach (var config in levelEnemyConfigs)
            {
                if (config.levelId == levelId)
                {
                    // 根据敌人ID从EnemyData中获取配置
                    return enemyData?.GetConfigById(config.enemyId);
                }
            }

            // 如果没有找到映射，返回默认的第一个配置
            return enemyData?.GetConfig(0);
        }

        /// <summary>
        /// 根据关卡ID设置敌人数据
        /// </summary>
        private void SetEnemyDataForLevel(int levelId)
        {
            EnemyConfig config = GetEnemyConfigForLevel(levelId);
            if (config != null && config.enemyPrefab != null && enemyLauncher != null)
            {
                enemyLauncher.SetPrefab(config.enemyPrefab, false);
                Debug.Log($"[EnemySystem] 为关卡 {levelId} 设置敌人: {config.enemyName}");
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 手动触发敌人生成
        /// </summary>
        public void SpawnEnemies()
        {
            if (enemyLauncher != null)
            {
                enemyLauncher.RespawnActor();
            }
        }

        /// <summary>
        /// 根据敌人ID生成敌人
        /// </summary>
        public void SpawnEnemyById(int enemyId)
        {
            if (enemyLauncher == null || enemyData == null) return;

            EnemyConfig config = enemyData.GetConfigById(enemyId);
            if (config != null && config.enemyPrefab != null)
            {
                enemyLauncher.SetPrefabAndRespawn(config.enemyPrefab);
                Debug.Log($"[EnemySystem] 生成敌人: {config.enemyName}");
            }
        }

        /// <summary>
        /// 根据敌人名称生成敌人
        /// </summary>
        public void SpawnEnemyByName(string enemyName)
        {
            if (enemyLauncher == null || enemyData == null) return;

            EnemyConfig config = enemyData.GetConfigByName(enemyName);
            if (config != null && config.enemyPrefab != null)
            {
                enemyLauncher.SetPrefabAndRespawn(config.enemyPrefab);
                Debug.Log($"[EnemySystem] 生成敌人: {config.enemyName}");
            }
        }

        /// <summary>
        /// 生成随机敌人
        /// </summary>
        public void SpawnRandomEnemy()
        {
            if (enemyLauncher == null || enemyData == null) return;

            EnemyConfig config = enemyData.GetRandomConfig();
            if (config != null && config.enemyPrefab != null)
            {
                enemyLauncher.SetPrefabAndRespawn(config.enemyPrefab);
                Debug.Log($"[EnemySystem] 随机生成敌人: {config.enemyName}");
            }
        }

        /// <summary>
        /// 根据索引生成敌人
        /// </summary>
        public void SpawnEnemyByIndex(int index)
        {
            if (enemyLauncher == null || enemyData == null) return;

            EnemyConfig config = enemyData.GetConfig(index);
            if (config != null && config.enemyPrefab != null)
            {
                enemyLauncher.SetPrefabAndRespawn(config.enemyPrefab);
                Debug.Log($"[EnemySystem] 生成敌人: {config.enemyName}");
            }
        }

        /// <summary>
        /// 清除所有敌人
        /// </summary>
        public void ClearAllEnemies()
        {
            if (enemyLauncher != null)
            {
                enemyLauncher.DestroySpawnedActor();
            }
        }

        /// <summary>
        /// 设置EnemyLauncher引用（ActorLauncher）
        /// </summary>
        public void SetEnemyLauncher(ActorLauncher launcher)
        {
            enemyLauncher = launcher;
        }

        /// <summary>
        /// 设置敌人数据配置
        /// </summary>
        public void SetEnemyData(EnemyData data)
        {
            enemyData = data;
        }

        /// <summary>
        /// 添加关卡敌人配置
        /// </summary>
        public void AddLevelEnemyConfig(int levelId, int enemyId)
        {
            // 检查是否已存在
            for (int i = 0; i < levelEnemyConfigs.Count; i++)
            {
                if (levelEnemyConfigs[i].levelId == levelId)
                {
                    levelEnemyConfigs[i] = new LevelEnemyConfig { levelId = levelId, enemyId = enemyId };
                    return;
                }
            }

            levelEnemyConfigs.Add(new LevelEnemyConfig { levelId = levelId, enemyId = enemyId });
        }

        /// <summary>
        /// 获取敌人配置
        /// </summary>
        public EnemyConfig GetEnemyConfig(int enemyId)
        {
            return enemyData?.GetConfigById(enemyId);
        }

        #endregion

        void OnDestroy()
        {
            UnsubscribeFromEvents();
        }
    }

    /// <summary>
    /// 关卡敌人配置映射
    /// </summary>
    [Serializable]
    public struct LevelEnemyConfig
    {
        [Tooltip("关卡ID")]
        public int levelId;

        [Tooltip("该关卡对应的敌人ID")]
        public int enemyId;
    }
}
