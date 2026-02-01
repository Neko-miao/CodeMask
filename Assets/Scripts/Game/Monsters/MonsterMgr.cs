// ================================================
// Game - 怪物管理器实现
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GameConfigs;
using GameFramework.Core;

namespace Game.Monsters
{
    /// <summary>
    /// 怪物管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Module, Priority = 50, RequiredStates = new[] { GameState.Playing })]
    public class MonsterMgr : GameComponent, IMonsterMgr
    {
        #region Fields

        private readonly List<Monster> _aliveMonsters = new List<Monster>();
        private readonly Dictionary<int, Monster> _monsterDict = new Dictionary<int, Monster>();
        private int _nextInstanceId = 1;
        private Monster _currentMonster;

        #endregion

        #region Properties

        public override string ComponentName => "MonsterMgr";
        public override ComponentType ComponentType => ComponentType.Module;
        public override int Priority => 50;

        public int AliveMonsterCount => _aliveMonsters.Count;
        public IReadOnlyList<Monster> AliveMonsters => _aliveMonsters;
        public Monster CurrentMonster => _currentMonster;
        public bool HasAliveMonster => _aliveMonsters.Count > 0;

        #endregion

        #region Events

        public event Action<Monster> OnMonsterSpawned;
        public event Action<Monster> OnMonsterDied;
        public event Action OnAllMonstersDefeated;

        #endregion

        #region Monster Creation

        /// <summary>
        /// 创建怪物
        /// </summary>
        public Monster CreateMonster(MonsterType monsterType, Vector3 position, Quaternion rotation)
        {
            var monsterData = ConfigManager.GetMonster(monsterType);
            if (monsterData == null)
            {
                Debug.LogError($"[MonsterMgr] MonsterData not found for type: {monsterType}");
                return null;
            }

            return CreateMonster(monsterData, position, rotation);
        }

        /// <summary>
        /// 创建怪物（使用MonsterData）
        /// </summary>
        public Monster CreateMonster(MonsterData monsterData, Vector3 position, Quaternion rotation)
        {
            if (monsterData == null)
            {
                Debug.LogError("[MonsterMgr] MonsterData is null");
                return null;
            }

            // 加载预制体
            var prefab = LoadMonsterPrefab(monsterData);
            if (prefab == null)
            {
                Debug.LogError($"[MonsterMgr] Failed to load prefab for monster: {monsterData.monsterName}");
                return null;
            }

            // 实例化怪物
            var go = UnityEngine.Object.Instantiate(prefab, position, rotation);
            go.name = $"{monsterData.monsterName}_{_nextInstanceId}";

            // 添加或获取Monster组件
            var monster = go.GetComponent<Monster>();
            if (monster == null)
            {
                monster = go.AddComponent<Monster>();
            }

            // 初始化怪物
            int instanceId = _nextInstanceId++;
            monster.Initialize(monsterData, instanceId);

            // 订阅怪物死亡事件
            monster.OnDeath += HandleMonsterDeath;

            // 添加到管理列表
            _aliveMonsters.Add(monster);
            _monsterDict[instanceId] = monster;

            // 设置为当前怪物（如果没有的话）
            if (_currentMonster == null)
            {
                _currentMonster = monster;
            }

            Debug.Log($"[MonsterMgr] Monster created: {monster.MonsterName} at {position}");

            OnMonsterSpawned?.Invoke(monster);

            return monster;
        }

        #endregion

        #region Monster Management

        /// <summary>
        /// 销毁指定怪物
        /// </summary>
        public void DestroyMonster(Monster monster)
        {
            if (monster == null) return;

            monster.OnDeath -= HandleMonsterDeath;

            _aliveMonsters.Remove(monster);
            _monsterDict.Remove(monster.InstanceId);

            if (_currentMonster == monster)
            {
                _currentMonster = _aliveMonsters.Count > 0 ? _aliveMonsters[0] : null;
            }

            monster.DestroyMonster();

            Debug.Log($"[MonsterMgr] Monster destroyed: {monster.MonsterName}");
        }

        /// <summary>
        /// 销毁所有怪物
        /// </summary>
        public void DestroyAllMonsters()
        {
            var monstersToDestroy = new List<Monster>(_aliveMonsters);
            foreach (var monster in monstersToDestroy)
            {
                DestroyMonster(monster);
            }

            _aliveMonsters.Clear();
            _monsterDict.Clear();
            _currentMonster = null;

            Debug.Log("[MonsterMgr] All monsters destroyed");
        }

        /// <summary>
        /// 根据实例ID获取怪物
        /// </summary>
        public Monster GetMonster(int instanceId)
        {
            _monsterDict.TryGetValue(instanceId, out var monster);
            return monster;
        }

        /// <summary>
        /// 获取指定类型的所有怪物
        /// </summary>
        public List<Monster> GetMonstersByType(MonsterType monsterType)
        {
            var result = new List<Monster>();
            foreach (var monster in _aliveMonsters)
            {
                if (monster.MonsterType == monsterType)
                {
                    result.Add(monster);
                }
            }
            return result;
        }

        #endregion

        #region Model Loading

        /// <summary>
        /// 加载怪物模型预制体
        /// </summary>
        public GameObject LoadMonsterPrefab(MonsterType monsterType)
        {
            return ConfigManager.LoadMonsterPrefab(monsterType);
        }

        /// <summary>
        /// 加载怪物模型预制体（使用MonsterData）
        /// </summary>
        public GameObject LoadMonsterPrefab(MonsterData monsterData)
        {
            return monsterData?.LoadPrefab();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 处理怪物死亡
        /// </summary>
        private void HandleMonsterDeath(Monster monster)
        {
            Debug.Log($"[MonsterMgr] Monster died: {monster.MonsterName}");

            _aliveMonsters.Remove(monster);
            _monsterDict.Remove(monster.InstanceId);

            // 触发死亡事件
            OnMonsterDied?.Invoke(monster);

            // 更新当前怪物
            if (_currentMonster == monster)
            {
                _currentMonster = _aliveMonsters.Count > 0 ? _aliveMonsters[0] : null;
            }

            // 检查是否所有怪物都被消灭
            if (_aliveMonsters.Count == 0)
            {
                Debug.Log("[MonsterMgr] All monsters defeated!");
                OnAllMonstersDefeated?.Invoke();
            }
        }

        #endregion

        #region Lifecycle

        protected override void OnInit()
        {
            Debug.Log("[MonsterMgr] Initialized");
        }

        protected override void OnShutdown()
        {
            DestroyAllMonsters();
            
            OnMonsterSpawned = null;
            OnMonsterDied = null;
            OnAllMonstersDefeated = null;
            
            Debug.Log("[MonsterMgr] Shutdown");
        }

        #endregion
    }
}
