// ================================================
// Game - 刷怪器
// ================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameConfigs;
using GameFramework.Core;

namespace Game.Monsters
{
    /// <summary>
    /// 刷怪器状态
    /// </summary>
    public enum SpawnerState
    {
        Idle,
        Spawning,
        WaitingForClear,
        Completed,
        Paused
    }

    /// <summary>
    /// 刷怪器 - 负责根据配置刷怪，支持打完一只再出下一只的逻辑
    /// </summary>
    public class MonsterSpawner : MonoBehaviour
    {
        #region Fields

        [Header("刷怪配置")]
        [Tooltip("刷怪点列表")]
        [SerializeField] private List<Transform> _spawnPoints = new List<Transform>();

        [Tooltip("默认刷怪位置")]
        [SerializeField] private Vector3 _defaultSpawnPosition = Vector3.zero;

        [Tooltip("默认刷怪旋转")]
        [SerializeField] private Vector3 _defaultSpawnRotation = Vector3.zero;

        private IMonsterMgr _monsterMgr;
        private LevelSpawnConfig _spawnConfig;
        private SpawnerState _state = SpawnerState.Idle;
        
        // 当前刷怪进度
        private int _currentWaveIndex;
        private int _currentEntryIndex;
        private int _currentSpawnedInEntry;
        private int _totalMonstersSpawned;
        private int _totalMonstersKilled;

        // 等待刷怪的队列
        private Queue<SpawnEntryConfig> _pendingSpawns = new Queue<SpawnEntryConfig>();
        private SpawnEntryConfig _currentEntry;

        #endregion

        #region Properties

        /// <summary>
        /// 当前状态
        /// </summary>
        public SpawnerState State => _state;

        /// <summary>
        /// 当前波次索引
        /// </summary>
        public int CurrentWaveIndex => _currentWaveIndex;

        /// <summary>
        /// 总共生成的怪物数
        /// </summary>
        public int TotalMonstersSpawned => _totalMonstersSpawned;

        /// <summary>
        /// 总共击杀的怪物数
        /// </summary>
        public int TotalMonstersKilled => _totalMonstersKilled;

        /// <summary>
        /// 是否完成所有波次
        /// </summary>
        public bool IsCompleted => _state == SpawnerState.Completed;

        #endregion

        #region Events

        /// <summary>
        /// 波次开始事件
        /// </summary>
        public event Action<int, WaveConfig> OnWaveStarted;

        /// <summary>
        /// 波次完成事件
        /// </summary>
        public event Action<int, WaveConfig> OnWaveCompleted;

        /// <summary>
        /// 所有波次完成事件
        /// </summary>
        public event Action OnAllWavesCompleted;

        /// <summary>
        /// 怪物生成事件
        /// </summary>
        public event Action<Monster, int, int> OnMonsterSpawned; // monster, waveIndex, totalSpawned

        #endregion

        #region Initialization

        private void Awake()
        {
            // 获取MonsterMgr
            _monsterMgr = GameInstance.Instance?.GetComp<IMonsterMgr>();
        }

        private void Start()
        {
            if (_monsterMgr == null)
            {
                _monsterMgr = GameInstance.Instance?.GetComp<IMonsterMgr>();
            }
        }

        /// <summary>
        /// 设置怪物管理器
        /// </summary>
        public void SetMonsterMgr(IMonsterMgr monsterMgr)
        {
            _monsterMgr = monsterMgr;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 根据关卡ID开始刷怪
        /// </summary>
        public void StartSpawning(int levelId)
        {
            var levelData = ConfigManager.GetLevel(levelId);
            if (levelData == null)
            {
                Debug.LogError($"[MonsterSpawner] Level {levelId} not found");
                return;
            }

            StartSpawning(levelData.spawnConfig);
        }

        /// <summary>
        /// 根据刷怪配置开始刷怪
        /// </summary>
        public void StartSpawning(LevelSpawnConfig spawnConfig)
        {
            if (spawnConfig == null)
            {
                Debug.LogError("[MonsterSpawner] SpawnConfig is null");
                return;
            }

            if (_monsterMgr == null)
            {
                _monsterMgr = GameInstance.Instance?.GetComp<IMonsterMgr>();
                if (_monsterMgr == null)
                {
                    Debug.LogError("[MonsterSpawner] MonsterMgr not found");
                    return;
                }
            }

            _spawnConfig = spawnConfig;
            ResetProgress();

            // 订阅怪物死亡事件
            _monsterMgr.OnMonsterDied += HandleMonsterDied;

            Debug.Log($"[MonsterSpawner] Start spawning, {_spawnConfig.waves.Count} waves");

            // 开始第一波
            StartNextWave();
        }

        /// <summary>
        /// 停止刷怪
        /// </summary>
        public void StopSpawning()
        {
            StopAllCoroutines();
            _state = SpawnerState.Idle;
            _pendingSpawns.Clear();

            if (_monsterMgr != null)
            {
                _monsterMgr.OnMonsterDied -= HandleMonsterDied;
            }

            Debug.Log("[MonsterSpawner] Spawning stopped");
        }

        /// <summary>
        /// 暂停刷怪
        /// </summary>
        public void PauseSpawning()
        {
            if (_state == SpawnerState.Spawning)
            {
                _state = SpawnerState.Paused;
                Debug.Log("[MonsterSpawner] Spawning paused");
            }
        }

        /// <summary>
        /// 恢复刷怪
        /// </summary>
        public void ResumeSpawning()
        {
            if (_state == SpawnerState.Paused)
            {
                _state = SpawnerState.Spawning;
                TrySpawnNextMonster();
                Debug.Log("[MonsterSpawner] Spawning resumed");
            }
        }

        /// <summary>
        /// 添加刷怪点
        /// </summary>
        public void AddSpawnPoint(Transform point)
        {
            if (point != null && !_spawnPoints.Contains(point))
            {
                _spawnPoints.Add(point);
            }
        }

        /// <summary>
        /// 设置默认刷怪位置
        /// </summary>
        public void SetDefaultSpawnPosition(Vector3 position, Vector3 rotation)
        {
            _defaultSpawnPosition = position;
            _defaultSpawnRotation = rotation;
        }

        #endregion

        #region Wave Management

        /// <summary>
        /// 开始下一波
        /// </summary>
        private void StartNextWave()
        {
            if (_spawnConfig == null || _spawnConfig.waves.Count == 0)
            {
                CompleteAllWaves();
                return;
            }

            if (_currentWaveIndex >= _spawnConfig.waves.Count)
            {
                if (_spawnConfig.loopWaves && _spawnConfig.waves.Count > 0)
                {
                    _currentWaveIndex = 0;
                }
                else
                {
                    CompleteAllWaves();
                    return;
                }
            }

            var wave = _spawnConfig.waves[_currentWaveIndex];
            _currentEntryIndex = 0;
            _currentSpawnedInEntry = 0;

            Debug.Log($"[MonsterSpawner] Starting wave {_currentWaveIndex + 1}: {wave.waveName}");

            OnWaveStarted?.Invoke(_currentWaveIndex, wave);

            // 如果有延迟，等待后开始
            if (wave.startDelay > 0)
            {
                StartCoroutine(StartWaveAfterDelay(wave.startDelay));
            }
            else
            {
                BeginWaveSpawning();
            }
        }

        private IEnumerator StartWaveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            BeginWaveSpawning();
        }

        /// <summary>
        /// 开始波次刷怪
        /// </summary>
        private void BeginWaveSpawning()
        {
            _state = SpawnerState.Spawning;
            PrepareSpawnEntries();
            TrySpawnNextMonster();
        }

        /// <summary>
        /// 准备刷怪条目
        /// </summary>
        private void PrepareSpawnEntries()
        {
            _pendingSpawns.Clear();
            
            if (_currentWaveIndex >= _spawnConfig.waves.Count) return;
            
            var wave = _spawnConfig.waves[_currentWaveIndex];
            foreach (var entry in wave.spawnEntries)
            {
                // 根据count添加多个相同条目
                for (int i = 0; i < entry.count; i++)
                {
                    _pendingSpawns.Enqueue(entry);
                }
            }
        }

        /// <summary>
        /// 完成当前波次
        /// </summary>
        private void CompleteCurrentWave()
        {
            if (_currentWaveIndex < _spawnConfig.waves.Count)
            {
                var wave = _spawnConfig.waves[_currentWaveIndex];
                Debug.Log($"[MonsterSpawner] Wave {_currentWaveIndex + 1} completed: {wave.waveName}");
                OnWaveCompleted?.Invoke(_currentWaveIndex, wave);

                // 等待后开始下一波
                _currentWaveIndex++;
                
                if (wave.completionDelay > 0)
                {
                    StartCoroutine(StartNextWaveAfterDelay(wave.completionDelay));
                }
                else
                {
                    StartNextWave();
                }
            }
        }

        private IEnumerator StartNextWaveAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            StartNextWave();
        }

        /// <summary>
        /// 完成所有波次
        /// </summary>
        private void CompleteAllWaves()
        {
            _state = SpawnerState.Completed;
            
            if (_monsterMgr != null)
            {
                _monsterMgr.OnMonsterDied -= HandleMonsterDied;
            }

            Debug.Log("[MonsterSpawner] All waves completed!");
            OnAllWavesCompleted?.Invoke();
        }

        #endregion

        #region Monster Spawning

        /// <summary>
        /// 尝试生成下一只怪物（打完一只再出下一只）
        /// </summary>
        private void TrySpawnNextMonster()
        {
            if (_state != SpawnerState.Spawning) return;

            // 如果还有存活的怪物，等待
            if (_monsterMgr != null && _monsterMgr.HasAliveMonster)
            {
                _state = SpawnerState.WaitingForClear;
                return;
            }

            // 检查是否还有待刷怪物
            if (_pendingSpawns.Count == 0)
            {
                // 当前波次完成
                CompleteCurrentWave();
                return;
            }

            // 生成下一只怪物
            var entry = _pendingSpawns.Dequeue();
            SpawnMonster(entry);
        }

        /// <summary>
        /// 生成怪物
        /// </summary>
        private void SpawnMonster(SpawnEntryConfig entry)
        {
            if (_monsterMgr == null) return;

            // 获取刷怪位置
            Vector3 position = GetSpawnPosition(entry.spawnPointIndex);
            Quaternion rotation = GetSpawnRotation(entry.spawnPointIndex);

            // 检查最大同时存在数
            if (_spawnConfig != null && _monsterMgr.AliveMonsterCount >= _spawnConfig.maxActiveMonsters)
            {
                // 超过上限，等待
                _state = SpawnerState.WaitingForClear;
                _pendingSpawns.Enqueue(entry); // 放回队列
                return;
            }

            // 使用延迟生成
            if (entry.delay > 0)
            {
                StartCoroutine(SpawnMonsterAfterDelay(entry, position, rotation, entry.delay));
            }
            else
            {
                DoSpawnMonster(entry.monsterType, position, rotation);
            }
        }

        private IEnumerator SpawnMonsterAfterDelay(SpawnEntryConfig entry, Vector3 position, Quaternion rotation, float delay)
        {
            yield return new WaitForSeconds(delay);
            
            if (_state == SpawnerState.Spawning || _state == SpawnerState.WaitingForClear)
            {
                DoSpawnMonster(entry.monsterType, position, rotation);
            }
        }

        /// <summary>
        /// 执行怪物生成
        /// </summary>
        private void DoSpawnMonster(MonsterType monsterType, Vector3 position, Quaternion rotation)
        {
            var monster = _monsterMgr.CreateMonster(monsterType, position, rotation);
            
            if (monster != null)
            {
                _totalMonstersSpawned++;
                Debug.Log($"[MonsterSpawner] Monster spawned: {monster.MonsterName}, Total: {_totalMonstersSpawned}");
                OnMonsterSpawned?.Invoke(monster, _currentWaveIndex, _totalMonstersSpawned);
            }
        }

        /// <summary>
        /// 获取刷怪位置
        /// </summary>
        private Vector3 GetSpawnPosition(int spawnPointIndex)
        {
            // 使用配置中的刷怪点
            if (_spawnConfig != null && _spawnConfig.spawnPoints.Count > 0)
            {
                int index = spawnPointIndex;
                if (index < 0 || index >= _spawnConfig.spawnPoints.Count)
                {
                    index = UnityEngine.Random.Range(0, _spawnConfig.spawnPoints.Count);
                }
                
                var pointConfig = _spawnConfig.spawnPoints[index];
                if (pointConfig.isEnabled)
                {
                    return pointConfig.position;
                }
            }

            // 使用场景中的Transform刷怪点
            if (_spawnPoints.Count > 0)
            {
                int index = spawnPointIndex;
                if (index < 0 || index >= _spawnPoints.Count)
                {
                    index = UnityEngine.Random.Range(0, _spawnPoints.Count);
                }
                return _spawnPoints[index].position;
            }

            // 使用默认位置
            return _defaultSpawnPosition;
        }

        /// <summary>
        /// 获取刷怪旋转
        /// </summary>
        private Quaternion GetSpawnRotation(int spawnPointIndex)
        {
            // 使用配置中的刷怪点
            if (_spawnConfig != null && _spawnConfig.spawnPoints.Count > 0)
            {
                int index = spawnPointIndex;
                if (index < 0 || index >= _spawnConfig.spawnPoints.Count)
                {
                    index = UnityEngine.Random.Range(0, _spawnConfig.spawnPoints.Count);
                }
                
                var pointConfig = _spawnConfig.spawnPoints[index];
                if (pointConfig.isEnabled)
                {
                    return Quaternion.Euler(pointConfig.rotation);
                }
            }

            // 使用场景中的Transform刷怪点
            if (_spawnPoints.Count > 0)
            {
                int index = spawnPointIndex;
                if (index < 0 || index >= _spawnPoints.Count)
                {
                    index = UnityEngine.Random.Range(0, _spawnPoints.Count);
                }
                return _spawnPoints[index].rotation;
            }

            // 使用默认旋转
            return Quaternion.Euler(_defaultSpawnRotation);
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// 处理怪物死亡事件
        /// </summary>
        private void HandleMonsterDied(Monster monster)
        {
            _totalMonstersKilled++;
            Debug.Log($"[MonsterSpawner] Monster killed: {monster.MonsterName}, Total killed: {_totalMonstersKilled}");

            // 如果在等待清场状态，尝试生成下一只
            if (_state == SpawnerState.WaitingForClear)
            {
                _state = SpawnerState.Spawning;
                TrySpawnNextMonster();
            }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 重置进度
        /// </summary>
        private void ResetProgress()
        {
            _currentWaveIndex = 0;
            _currentEntryIndex = 0;
            _currentSpawnedInEntry = 0;
            _totalMonstersSpawned = 0;
            _totalMonstersKilled = 0;
            _pendingSpawns.Clear();
            _state = SpawnerState.Idle;
        }

        #endregion

        #region Cleanup

        private void OnDestroy()
        {
            StopSpawning();
            
            OnWaveStarted = null;
            OnWaveCompleted = null;
            OnAllWavesCompleted = null;
            OnMonsterSpawned = null;
        }

        #endregion
    }
}
