// ================================================
// GameFramework - 单局管理器实现
// ================================================

using System;
using System.Collections;
using System.Collections.Generic;
using GameConfigs;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.Session
{
    /// <summary>
    /// 单局管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.System, Priority = 130, RequiredStates = new[] { GameState.Playing })]
    public class GameSession : GameComponent, IGameSession
    {
        private SessionState _state = SessionState.None;
        private SessionContext _context;
        private LevelMgr _levelMgr;
        private RuleMgr _ruleMgr;
        private SessionConfig _currentConfig;
        
        public override string ComponentName => "GameSession";
        public override ComponentType ComponentType => ComponentType.System;
        public override int Priority => 130;
        
        #region Properties
        
        public string SessionId => _context?.SessionId ?? string.Empty;
        public SessionState State => _state;
        public ISessionContext Context => _context;
        public ILevel CurrentLevel => _levelMgr?.CurrentLevel;
        public ILevelMgr LevelMgr => _levelMgr;
        public IRuleMgr RuleMgr => _ruleMgr;
        public float ElapsedTime => _context?.ElapsedTime ?? 0f;
        public bool IsRunning => _state == SessionState.Running;
        
        #endregion
        
        #region Events
        
        public event Action<SessionState, SessionState> OnSessionStateChanged;
        public event Action<ILevel> OnLevelLoaded;
        public event Action<ILevel, LevelResult> OnLevelCompleted;
        public event Action<ISessionRule> OnRuleTriggered;
        public event Action<string> OnCheckpointReached;
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            _context = new SessionContext();
            _levelMgr = new LevelMgr();
            _ruleMgr = new RuleMgr();
            
            // 订阅事件
            _levelMgr.OnLevelLoadComplete += level => OnLevelLoaded?.Invoke(level);
            _levelMgr.OnLevelCompleted += (level, result) => OnLevelCompleted?.Invoke(level, result);
            _ruleMgr.OnRuleTriggered += rule => OnRuleTriggered?.Invoke(rule);
        }
        
        protected override void OnTick(float deltaTime)
        {
            if (_state != SessionState.Running) return;
            
            // 更新上下文时间
            _context.ElapsedTime += deltaTime;
            
            // 更新规则
            _ruleMgr.Tick(deltaTime);
            
            // 更新关卡
            CurrentLevel?.OnUpdate(deltaTime);
        }
        
        protected override void OnShutdown()
        {
            if (_state != SessionState.None && _state != SessionState.Ended)
            {
                EndSession(SessionEndReason.Aborted);
            }
            
            _ruleMgr?.Destroy();
            _levelMgr?.UnloadCurrentLevel();
        }
        
        #endregion
        
        #region Session Control
        
        public Coroutine StartSession(SessionConfig config, Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(StartSessionCoroutine(config, onComplete));
        }
        
        private IEnumerator StartSessionCoroutine(SessionConfig config, Action onComplete)
        {
            if (_state != SessionState.None && _state != SessionState.Ended)
            {
                Debug.LogWarning("[GameSession] Session already in progress");
                onComplete?.Invoke();
                yield break;
            }
            
            Debug.Log("[GameSession] Starting session...");
            
            _currentConfig = config;
            ChangeState(SessionState.Preparing);
            
            // 重置上下文
            _context.Reset();
            _context.CurrentLevelId = config.StartLevelId;
            
            // 初始化规则
            _ruleMgr.Initialize(_context);
            
            // 添加规则
            if (config.Rules != null)
            {
                foreach (var rule in config.Rules)
                {
                    _ruleMgr.AddRule(rule);
                }
            }
            
            // 加载起始关卡
            if (config.AutoStartLevel)
            {
                bool isLoaded = false;
                _levelMgr.LoadLevel(config.StartLevelId, () => isLoaded = true);
                
                while (!isLoaded)
                {
                    yield return null;
                }
            }
            
            // 启动规则
            _ruleMgr.Start();
            
            ChangeState(SessionState.Running);
            
            Debug.Log("[GameSession] Session started");
            onComplete?.Invoke();
        }
        
        public void EndSession(SessionEndReason reason)
        {
            if (_state == SessionState.None || _state == SessionState.Ended)
                return;
            
            Debug.Log($"[GameSession] Ending session, reason: {reason}");
            
            ChangeState(SessionState.Completing);
            
            // 停止规则
            _ruleMgr.IsEnabled = false;
            
            // 完成当前关卡
            if (CurrentLevel != null && CurrentLevel.State == LevelState.Running)
            {
                var result = new LevelResult
                {
                    IsSuccess = reason == SessionEndReason.Completed,
                    Score = _context.Score,
                    CompletionTime = _context.ElapsedTime,
                    FailReason = reason.ToString()
                };
                CurrentLevel.OnComplete(result);
            }
            
            ChangeState(SessionState.Ended);
            
            Debug.Log("[GameSession] Session ended");
        }
        
        public void PauseSession()
        {
            if (_state != SessionState.Running) return;
            
            ChangeState(SessionState.Paused);
            CurrentLevel?.OnPause();
            
            Debug.Log("[GameSession] Session paused");
        }
        
        public void ResumeSession()
        {
            if (_state != SessionState.Paused) return;
            
            ChangeState(SessionState.Running);
            CurrentLevel?.OnResume();
            
            Debug.Log("[GameSession] Session resumed");
        }
        
        public Coroutine RestartSession(Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(RestartSessionCoroutine(onComplete));
        }
        
        private IEnumerator RestartSessionCoroutine(Action onComplete)
        {
            if (_currentConfig == null)
            {
                Debug.LogWarning("[GameSession] No config to restart");
                onComplete?.Invoke();
                yield break;
            }
            
            Debug.Log("[GameSession] Restarting session...");
            
            // 清理当前会话
            _ruleMgr.Reset();
            _levelMgr.UnloadCurrentLevel();
            
            // 重新开始
            ChangeState(SessionState.None);
            
            bool isStarted = false;
            StartSession(_currentConfig, () => isStarted = true);
            
            while (!isStarted)
            {
                yield return null;
            }
            
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Level Control
        
        public Coroutine LoadLevel(int levelId, Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(LoadLevelCoroutine(levelId, onComplete));
        }
        
        private IEnumerator LoadLevelCoroutine(int levelId, Action onComplete)
        {
            bool isLoaded = false;
            _levelMgr.LoadLevel(levelId, () => isLoaded = true);
            
            while (!isLoaded)
            {
                yield return null;
            }
            
            _context.CurrentLevelId = levelId;
            onComplete?.Invoke();
        }
        
        public Coroutine LoadLevel(string levelName, Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(LoadLevelByNameCoroutine(levelName, onComplete));
        }
        
        private IEnumerator LoadLevelByNameCoroutine(string levelName, Action onComplete)
        {
            var configs = _levelMgr.GetAllLevelConfigs();
            foreach (var config in configs)
            {
                if (config.levelName == levelName)
                {
                    bool isLoaded = false;
                    LoadLevel(config.levelId, () => isLoaded = true);
                    
                    while (!isLoaded)
                    {
                        yield return null;
                    }
                    
                    onComplete?.Invoke();
                    yield break;
                }
            }
            
            Debug.LogWarning($"[GameSession] Level not found: {levelName}");
            onComplete?.Invoke();
        }
        
        public Coroutine ReloadCurrentLevel(Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(ReloadCurrentLevelCoroutine(onComplete));
        }
        
        private IEnumerator ReloadCurrentLevelCoroutine(Action onComplete)
        {
            bool isLoaded = false;
            _levelMgr.ReloadLevel(() => isLoaded = true);
            
            while (!isLoaded)
            {
                yield return null;
            }
            
            onComplete?.Invoke();
        }
        
        public Coroutine NextLevel(Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(NextLevelCoroutine(onComplete));
        }
        
        private IEnumerator NextLevelCoroutine(Action onComplete)
        {
            int nextLevelId = _context.CurrentLevelId + 1;
            if (_levelMgr.IsLevelUnlocked(nextLevelId))
            {
                bool isLoaded = false;
                LoadLevel(nextLevelId, () => isLoaded = true);
                
                while (!isLoaded)
                {
                    yield return null;
                }
            }
            else
            {
                Debug.LogWarning($"[GameSession] Level {nextLevelId} is locked");
            }
            
            onComplete?.Invoke();
        }
        
        public Coroutine PreviousLevel(Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(PreviousLevelCoroutine(onComplete));
        }
        
        private IEnumerator PreviousLevelCoroutine(Action onComplete)
        {
            int prevLevelId = _context.CurrentLevelId - 1;
            if (prevLevelId > 0)
            {
                bool isLoaded = false;
                LoadLevel(prevLevelId, () => isLoaded = true);
                
                while (!isLoaded)
                {
                    yield return null;
                }
            }
            
            onComplete?.Invoke();
        }
        
        public float GetLevelProgress()
        {
            return CurrentLevel?.Progress ?? 0f;
        }
        
        #endregion
        
        #region Rule Control
        
        public void AddRule(ISessionRule rule)
        {
            _ruleMgr.AddRule(rule);
            
            if (_state == SessionState.Running)
            {
                rule.OnInit(_context);
                rule.OnStart();
            }
        }
        
        public void RemoveRule(string ruleId)
        {
            _ruleMgr.RemoveRule(ruleId);
        }
        
        public T GetRule<T>() where T : class, ISessionRule
        {
            return _ruleMgr.GetRule<T>();
        }
        
        public void EnableRule(string ruleId)
        {
            _ruleMgr.EnableRule(ruleId);
        }
        
        public void DisableRule(string ruleId)
        {
            _ruleMgr.DisableRule(ruleId);
        }
        
        #endregion
        
        #region Private Methods
        
        private void ChangeState(SessionState newState)
        {
            if (_state == newState) return;
            
            var oldState = _state;
            _state = newState;
            
            Debug.Log($"[GameSession] State changed: {oldState} -> {newState}");
            OnSessionStateChanged?.Invoke(oldState, newState);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 关卡管理器实现
    /// </summary>
    public class LevelMgr : ILevelMgr
    {
        private readonly Dictionary<int, LevelData> _levelConfigs = new Dictionary<int, LevelData>();
        private readonly List<LevelData> _configList = new List<LevelData>();
        private readonly HashSet<int> _unlockedLevels = new HashSet<int>();
        
        private ILevel _currentLevel;
        private bool _isLoading;
        private string _currentCheckpoint;
        
        #region Properties
        
        public ILevel CurrentLevel => _currentLevel;
        public int CurrentLevelId => _currentLevel?.LevelId ?? 0;
        public int TotalLevels => _configList.Count;
        public int UnlockedLevels => _unlockedLevels.Count;
        public bool IsLoading => _isLoading;
        
        #endregion
        
        #region Events
        
        public event Action<int> OnLevelLoadStart;
        public event Action<float> OnLevelLoadProgress;
        public event Action<ILevel> OnLevelLoadComplete;
        public event Action<ILevel> OnLevelStarted;
        public event Action<ILevel, LevelResult> OnLevelCompleted;
        public event Action<ILevel> OnLevelUnloaded;
        public event Action<int> OnLevelUnlocked;
        
        #endregion
        
        public LevelMgr()
        {
            // 从 GameConfigs 加载配置
            LoadFromGameConfigs();
            
            // 默认解锁第一关
            _unlockedLevels.Add(1);
        }
        
        /// <summary>
        /// 从 GameConfigs.ConfigManager 加载关卡配置
        /// </summary>
        private void LoadFromGameConfigs()
        {
            var levelConfig = ConfigManager.LevelConfig;
            if (levelConfig == null)
            {
                Debug.LogWarning("[LevelMgr] LevelConfig not found in Resources/Configs");
                return;
            }
            
            foreach (var levelData in levelConfig.levels)
            {
                _levelConfigs[levelData.levelId] = levelData;
                _configList.Add(levelData);
                
                // 处理默认解锁
                if (levelData.isUnlockedByDefault)
                {
                    _unlockedLevels.Add(levelData.levelId);
                }
            }
            
            _configList.Sort((a, b) => a.levelId.CompareTo(b.levelId));
            Debug.Log($"[LevelMgr] Loaded {_configList.Count} levels from GameConfigs");
        }
        
        #region Level Operations
        
        public Coroutine LoadLevel(int levelId, Action onComplete = null)
        {
            if (!_levelConfigs.TryGetValue(levelId, out var config))
            {
                Debug.LogError($"[LevelMgr] Level config not found: {levelId}");
                onComplete?.Invoke();
                return null;
            }
            
            return LoadLevel(config, onComplete);
        }
        
        public Coroutine LoadLevel(LevelData config, Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(LoadLevelCoroutine(config, onComplete));
        }
        
        private IEnumerator LoadLevelCoroutine(LevelData config, Action onComplete)
        {
            if (config == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            _isLoading = true;
            OnLevelLoadStart?.Invoke(config.levelId);
            
            // 卸载当前关卡
            UnloadCurrentLevel();
            
            // 加载场景
            if (!string.IsNullOrEmpty(config.sceneName))
            {
                var operation = SceneManager.LoadSceneAsync(config.sceneName, LoadSceneMode.Additive);
                while (!operation.isDone)
                {
                    OnLevelLoadProgress?.Invoke(operation.progress);
                    yield return null;
                }
            }
            
            // 创建关卡实例
            _currentLevel = CreateLevelInstance(config);
            _currentLevel.OnLoad();
            
            _isLoading = false;
            OnLevelLoadComplete?.Invoke(_currentLevel);
            
            Debug.Log($"[LevelMgr] Level loaded: {config.levelId}");
            onComplete?.Invoke();
        }
        
        public void UnloadCurrentLevel()
        {
            if (_currentLevel == null) return;
            
            var level = _currentLevel;
            level.OnUnload();
            
            // 卸载场景
            if (level.Config != null && !string.IsNullOrEmpty(level.Config.sceneName))
            {
                SceneManager.UnloadSceneAsync(level.Config.sceneName);
            }
            
            _currentLevel = null;
            OnLevelUnloaded?.Invoke(level);
        }
        
        public Coroutine ReloadLevel(Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(ReloadLevelCoroutine(onComplete));
        }
        
        private IEnumerator ReloadLevelCoroutine(Action onComplete)
        {
            if (_currentLevel == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            int levelId = _currentLevel.LevelId;
            bool isLoaded = false;
            LoadLevel(levelId, () => isLoaded = true);
            
            while (!isLoaded)
            {
                yield return null;
            }
            
            onComplete?.Invoke();
        }
        
        public void StartLevel()
        {
            if (_currentLevel == null) return;
            
            _currentLevel.Start();
            OnLevelStarted?.Invoke(_currentLevel);
        }
        
        public void CompleteLevel(LevelResult result)
        {
            if (_currentLevel == null) return;
            
            _currentLevel.Complete(result);
            OnLevelCompleted?.Invoke(_currentLevel, result);
            
            // 自动解锁下一关
            if (result.IsSuccess)
            {
                UnlockLevel(_currentLevel.LevelId + 1);
            }
        }
        
        #endregion
        
        #region Level Config
        
        public LevelData GetLevelConfig(int levelId)
        {
            _levelConfigs.TryGetValue(levelId, out var config);
            return config;
        }
        
        public IReadOnlyList<LevelData> GetAllLevelConfigs()
        {
            return _configList;
        }
        
        /// <summary>
        /// 获取关卡的刷怪配置
        /// </summary>
        public LevelSpawnConfig GetSpawnConfig(int levelId)
        {
            var config = GetLevelConfig(levelId);
            return config?.spawnConfig;
        }
        
        /// <summary>
        /// 获取关卡的奖励配置
        /// </summary>
        public LevelReward GetRewardConfig(int levelId)
        {
            var config = GetLevelConfig(levelId);
            return config?.reward;
        }
        
        /// <summary>
        /// 获取关卡的星级评价条件
        /// </summary>
        public StarCondition GetStarCondition(int levelId)
        {
            var config = GetLevelConfig(levelId);
            return config?.starCondition;
        }
        
        /// <summary>
        /// 获取当前关卡的刷怪配置
        /// </summary>
        public LevelSpawnConfig GetCurrentSpawnConfig()
        {
            return _currentLevel != null ? GetSpawnConfig(_currentLevel.LevelId) : null;
        }
        
        /// <summary>
        /// 获取当前关卡的奖励配置
        /// </summary>
        public LevelReward GetCurrentRewardConfig()
        {
            return _currentLevel != null ? GetRewardConfig(_currentLevel.LevelId) : null;
        }
        
        #endregion
        
        #region Progress
        
        public float GetProgress()
        {
            return _currentLevel?.Progress ?? 0f;
        }
        
        public void SetCheckpoint(string checkpointId)
        {
            _currentCheckpoint = checkpointId;
            if (_currentLevel?.Data != null)
            {
                _currentLevel.Data.CurrentCheckpoint = checkpointId;
            }
        }
        
        public string GetCheckpoint()
        {
            return _currentCheckpoint;
        }
        
        public void ClearCheckpoint()
        {
            _currentCheckpoint = null;
            if (_currentLevel?.Data != null)
            {
                _currentLevel.Data.CurrentCheckpoint = null;
            }
        }
        
        #endregion
        
        #region Unlock
        
        public void UnlockLevel(int levelId)
        {
            if (_unlockedLevels.Add(levelId))
            {
                OnLevelUnlocked?.Invoke(levelId);
            }
        }
        
        public bool IsLevelUnlocked(int levelId)
        {
            return _unlockedLevels.Contains(levelId);
        }
        
        #endregion
        
        #region Private Methods
        
        private ILevel CreateLevelInstance(LevelData config)
        {
            // 这里可以根据配置创建不同类型的关卡
            // 默认使用DefaultLevel
            return new DefaultLevel(config);
        }
        
        #endregion
    }
    
    /// <summary>
    /// 默认关卡实现
    /// </summary>
    public class DefaultLevel : LevelBase
    {
        public DefaultLevel(LevelData config) : base(config)
        {
        }
    }
}
