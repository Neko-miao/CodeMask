// ================================================
// GameFramework - 单局管理器实现
// ================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        
        public async Task StartSession(SessionConfig config)
        {
            if (_state != SessionState.None && _state != SessionState.Ended)
            {
                Debug.LogWarning("[GameSession] Session already in progress");
                return;
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
                await LoadLevel(config.StartLevelId);
            }
            
            // 启动规则
            _ruleMgr.Start();
            
            ChangeState(SessionState.Running);
            
            Debug.Log("[GameSession] Session started");
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
        
        public async Task RestartSession()
        {
            if (_currentConfig == null)
            {
                Debug.LogWarning("[GameSession] No config to restart");
                return;
            }
            
            Debug.Log("[GameSession] Restarting session...");
            
            // 清理当前会话
            _ruleMgr.Reset();
            _levelMgr.UnloadCurrentLevel();
            
            // 重新开始
            ChangeState(SessionState.None);
            await StartSession(_currentConfig);
        }
        
        #endregion
        
        #region Level Control
        
        public async Task LoadLevel(int levelId)
        {
            await _levelMgr.LoadLevel(levelId);
            _context.CurrentLevelId = levelId;
        }
        
        public async Task LoadLevel(string levelName)
        {
            var configs = _levelMgr.GetAllLevelConfigs();
            foreach (var config in configs)
            {
                if (config.LevelName == levelName)
                {
                    await LoadLevel(config.LevelId);
                    return;
                }
            }
            
            Debug.LogWarning($"[GameSession] Level not found: {levelName}");
        }
        
        public async Task ReloadCurrentLevel()
        {
            await _levelMgr.ReloadLevel();
        }
        
        public async Task NextLevel()
        {
            int nextLevelId = _context.CurrentLevelId + 1;
            if (_levelMgr.IsLevelUnlocked(nextLevelId))
            {
                await LoadLevel(nextLevelId);
            }
            else
            {
                Debug.LogWarning($"[GameSession] Level {nextLevelId} is locked");
            }
        }
        
        public async Task PreviousLevel()
        {
            int prevLevelId = _context.CurrentLevelId - 1;
            if (prevLevelId > 0)
            {
                await LoadLevel(prevLevelId);
            }
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
        private readonly Dictionary<int, ILevelConfig> _levelConfigs = new Dictionary<int, ILevelConfig>();
        private readonly List<ILevelConfig> _configList = new List<ILevelConfig>();
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
            // 默认解锁第一关
            _unlockedLevels.Add(1);
        }
        
        #region Level Operations
        
        public async Task LoadLevel(int levelId)
        {
            if (!_levelConfigs.TryGetValue(levelId, out var config))
            {
                // 如果没有注册配置，创建一个默认配置
                config = new LevelConfig(levelId, $"Level_{levelId}", $"Level_{levelId}");
                RegisterLevelConfig(config);
            }
            
            await LoadLevel(config);
        }
        
        public async Task LoadLevel(ILevelConfig config)
        {
            if (config == null) return;
            
            _isLoading = true;
            OnLevelLoadStart?.Invoke(config.LevelId);
            
            // 卸载当前关卡
            UnloadCurrentLevel();
            
            // 加载场景
            if (!string.IsNullOrEmpty(config.SceneName))
            {
                var operation = SceneManager.LoadSceneAsync(config.SceneName, LoadSceneMode.Additive);
                while (!operation.isDone)
                {
                    OnLevelLoadProgress?.Invoke(operation.progress);
                    await Task.Yield();
                }
            }
            
            // 创建关卡实例
            _currentLevel = CreateLevelInstance(config);
            _currentLevel.OnLoad();
            
            _isLoading = false;
            OnLevelLoadComplete?.Invoke(_currentLevel);
            
            Debug.Log($"[LevelMgr] Level loaded: {config.LevelId}");
        }
        
        public void UnloadCurrentLevel()
        {
            if (_currentLevel == null) return;
            
            var level = _currentLevel;
            level.OnUnload();
            
            // 卸载场景
            if (level.Config != null && !string.IsNullOrEmpty(level.Config.SceneName))
            {
                SceneManager.UnloadSceneAsync(level.Config.SceneName);
            }
            
            _currentLevel = null;
            OnLevelUnloaded?.Invoke(level);
        }
        
        public async Task ReloadLevel()
        {
            if (_currentLevel == null) return;
            
            int levelId = _currentLevel.LevelId;
            await LoadLevel(levelId);
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
        
        public void RegisterLevelConfig(ILevelConfig config)
        {
            if (config == null) return;
            
            _levelConfigs[config.LevelId] = config;
            if (!_configList.Contains(config))
            {
                _configList.Add(config);
                _configList.Sort((a, b) => a.LevelId.CompareTo(b.LevelId));
            }
        }
        
        public ILevelConfig GetLevelConfig(int levelId)
        {
            _levelConfigs.TryGetValue(levelId, out var config);
            return config;
        }
        
        public IReadOnlyList<ILevelConfig> GetAllLevelConfigs()
        {
            return _configList;
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
        
        public IUnlockCondition GetUnlockCondition(int levelId)
        {
            var config = GetLevelConfig(levelId);
            return config?.UnlockCondition;
        }
        
        #endregion
        
        #region Private Methods
        
        private ILevel CreateLevelInstance(ILevelConfig config)
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
        public DefaultLevel(ILevelConfig config) : base(config)
        {
        }
    }
}

