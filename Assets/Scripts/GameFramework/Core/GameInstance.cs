// ================================================
// GameFramework - 游戏实例实现
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏实例实现 - 游戏总控制器
    /// </summary>
    public class GameInstance : MonoBehaviour, IGameInstance
    {
        #region Singleton
        
        private static GameInstance _instance;
        
        /// <summary>
        /// 单例实例
        /// </summary>
        public static GameInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[GameInstance]");
                    _instance = go.AddComponent<GameInstance>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }
        
        #endregion
        
        #region Fields
        
        private GameState _currentState = GameState.None;
        private GameState _previousState = GameState.None;
        private bool _isInitialized = false;
        private bool _isRunning = false;
        
        private GameTimeController _timeController;
        private ComponentContainer _componentContainer;
        private ComponentRegistry _componentRegistry;
        
        private Action<IComponentRegistry> _registrationCallback;
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 当前游戏状态
        /// </summary>
        public GameState CurrentState => _currentState;
        
        /// <summary>
        /// 上一个游戏状态
        /// </summary>
        public GameState PreviousState => _previousState;
        
        /// <summary>
        /// 时间控制器
        /// </summary>
        public IGameTimeController TimeController => _timeController;
        
        /// <summary>
        /// 组件注册管理器
        /// </summary>
        public IComponentRegistry Registry => _componentRegistry;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized => _isInitialized;
        
        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 游戏状态改变事件
        /// </summary>
        public event Action<GameState, GameState> OnStateChanged;
        
        /// <summary>
        /// 游戏初始化完成事件
        /// </summary>
        public event Action OnInitialized;
        
        /// <summary>
        /// 游戏关闭事件
        /// </summary>
        public event Action OnShutdown;
        
        /// <summary>
        /// 游戏暂停事件
        /// </summary>
        public event Action OnPaused;
        
        /// <summary>
        /// 游戏恢复事件
        /// </summary>
        public event Action OnResumed;
        
        #endregion
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Update()
        {
            if (!_isRunning) return;
            
            // 更新时间控制器
            _timeController?.Update();
            
            // 更新所有组件
            float deltaTime = _timeController?.DeltaTime ?? Time.deltaTime;
            var components = _componentContainer?.GetAll();
            if (components != null)
            {
                foreach (var comp in components)
                {
                    if (comp is ITickable tickable && comp.IsEnabled)
                    {
                        tickable.Tick(deltaTime);
                    }
                }
            }
        }
        
        private void LateUpdate()
        {
            if (!_isRunning) return;
            
            float deltaTime = _timeController?.DeltaTime ?? Time.deltaTime;
            var components = _componentContainer?.GetAll();
            if (components != null)
            {
                foreach (var comp in components)
                {
                    if (comp is ITickable tickable && comp.IsEnabled)
                    {
                        tickable.LateTick(deltaTime);
                    }
                }
            }
        }
        
        private void FixedUpdate()
        {
            if (!_isRunning) return;
            
            _timeController?.FixedUpdate();
            
            float fixedDeltaTime = _timeController?.FixedDeltaTime ?? Time.fixedDeltaTime;
            var components = _componentContainer?.GetAll();
            if (components != null)
            {
                foreach (var comp in components)
                {
                    if (comp is ITickable tickable && comp.IsEnabled)
                    {
                        tickable.FixedTick(fixedDeltaTime);
                    }
                }
            }
        }
        
        private void OnDestroy()
        {
            if (_instance == this)
            {
                Shutdown();
                _instance = null;
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
                OnPaused?.Invoke();
            else
                OnResumed?.Invoke();
        }
        
        private void OnApplicationQuit()
        {
            Shutdown();
        }
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// 设置组件注册回调
        /// </summary>
        public void SetRegistrationCallback(Action<IComponentRegistry> callback)
        {
            _registrationCallback = callback;
        }
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized)
            {
                Debug.LogWarning("[GameInstance] Already initialized");
                return;
            }
            
            Debug.Log("[GameInstance] Initializing...");
            
            ChangeState(GameState.Init);
            
            // 创建核心系统
            _timeController = new GameTimeController();
            _componentContainer = new ComponentContainer();
            _componentRegistry = new ComponentRegistry(_componentContainer);
            
            // 调用注册回调
            _registrationCallback?.Invoke(_componentRegistry);
            
            // 加载全局组件
            _componentRegistry.OnGameStateEnter(GameState.Global);
            
            _isInitialized = true;
            _isRunning = true;
            
            ChangeState(GameState.Loading);
            
            OnInitialized?.Invoke();
            
            Debug.Log("[GameInstance] Initialized successfully");
        }
        
        /// <summary>
        /// 关闭游戏
        /// </summary>
        public void Shutdown()
        {
            if (!_isInitialized) return;
            
            Debug.Log("[GameInstance] Shutting down...");
            
            ChangeState(GameState.Shutdown);
            
            _isRunning = false;
            
            // 关闭所有组件
            var components = _componentContainer?.GetAll();
            if (components != null)
            {
                for (int i = components.Count - 1; i >= 0; i--)
                {
                    var lifecycle = components[i] as IComponentLifecycle;
                    lifecycle?.Shutdown();
                    lifecycle?.OnUnregister();
                }
            }
            
            _componentContainer?.Clear();
            _timeController?.Reset();
            
            _isInitialized = false;
            
            OnShutdown?.Invoke();
            
            Debug.Log("[GameInstance] Shutdown complete");
        }
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        public void Pause()
        {
            if (_currentState == GameState.Paused) return;
            
            _timeController?.Pause();
            
            var components = _componentContainer?.GetAll();
            if (components != null)
            {
                foreach (var comp in components)
                {
                    (comp as IComponentLifecycle)?.OnPause();
                }
            }
            
            ChangeState(GameState.Paused);
            OnPaused?.Invoke();
            
            Debug.Log("[GameInstance] Game paused");
        }
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        public void Resume()
        {
            if (_currentState != GameState.Paused) return;
            
            _timeController?.Resume();
            
            var components = _componentContainer?.GetAll();
            if (components != null)
            {
                foreach (var comp in components)
                {
                    (comp as IComponentLifecycle)?.OnResume();
                }
            }
            
            ChangeState(_previousState != GameState.Paused ? _previousState : GameState.Playing);
            OnResumed?.Invoke();
            
            Debug.Log("[GameInstance] Game resumed");
        }
        
        /// <summary>
        /// 重启游戏
        /// </summary>
        public void Restart()
        {
            Debug.Log("[GameInstance] Restarting...");
            
            Shutdown();
            
            _timeController?.Reset();
            
            Initialize();
        }
        
        #endregion
        
        #region State Management
        
        /// <summary>
        /// 切换游戏状态
        /// </summary>
        public void ChangeState(GameState newState)
        {
            if (_currentState == newState) return;
            
            var oldState = _currentState;
            _previousState = oldState;
            _currentState = newState;
            
            Debug.Log($"[GameInstance] State changed: {oldState} -> {newState}");
            
            // 处理组件的状态切换
            if (_componentRegistry != null && oldState != GameState.None)
            {
                _componentRegistry.TransitionComponents(oldState, newState);
            }
            
            OnStateChanged?.Invoke(oldState, newState);
        }
        
        #endregion
        
        #region Component Access
        
        /// <summary>
        /// 获取组件
        /// </summary>
        public T GetComp<T>() where T : class, IGameComponent
        {
            return _componentContainer?.Get<T>();
        }
        
        /// <summary>
        /// 尝试获取组件
        /// </summary>
        public bool TryGetComp<T>(out T component) where T : class, IGameComponent
        {
            component = _componentContainer?.Get<T>();
            return component != null;
        }
        
        /// <summary>
        /// 检查组件是否存在
        /// </summary>
        public bool HasComp<T>() where T : class, IGameComponent
        {
            return _componentContainer?.Has<T>() ?? false;
        }
        
        /// <summary>
        /// 动态添加组件
        /// </summary>
        public void AddComp<TInterface, TImplementation>()
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new()
        {
            var instance = new TImplementation();
            _componentContainer?.Add<TInterface>(instance);
            
            var lifecycle = instance as IComponentLifecycle;
            lifecycle?.OnRegister();
            lifecycle?.Initialize();
            lifecycle?.Start();
            
            Debug.Log($"[GameInstance] Component added: {typeof(TInterface).Name}");
        }
        
        /// <summary>
        /// 动态移除组件
        /// </summary>
        public void RemoveComp<T>() where T : class, IGameComponent
        {
            var comp = _componentContainer?.Get<T>();
            if (comp != null)
            {
                var lifecycle = comp as IComponentLifecycle;
                lifecycle?.Shutdown();
                lifecycle?.OnUnregister();
                
                _componentContainer?.Remove<T>();
                
                Debug.Log($"[GameInstance] Component removed: {typeof(T).Name}");
            }
        }
        
        /// <summary>
        /// 启用组件
        /// </summary>
        public void EnableComp<T>() where T : class, IGameComponent
        {
            var comp = _componentContainer?.Get<T>();
            comp?.SetEnabled(true);
        }
        
        /// <summary>
        /// 禁用组件
        /// </summary>
        public void DisableComp<T>() where T : class, IGameComponent
        {
            var comp = _componentContainer?.Get<T>();
            comp?.SetEnabled(false);
        }
        
        #endregion
    }
}

