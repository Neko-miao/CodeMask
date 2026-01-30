// ================================================
// GameFramework - 游戏启动器
// ================================================

using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏启动器 - 挂载到场景中的GameObject上以启动游戏
    /// </summary>
    public class GameLauncher : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private bool _autoInitialize = true;
        [SerializeField] private bool _dontDestroyOnLoad = true;
        
        [Header("Initial State")]
        [SerializeField] private GameState _initialState = GameState.Menu;
        
        private void Awake()
        {
            if (_dontDestroyOnLoad)
            {
                DontDestroyOnLoad(gameObject);
            }
            
            if (_autoInitialize)
            {
                Initialize();
            }
        }
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void Initialize()
        {
            // 设置组件注册回调
            GameInstance.Instance.SetRegistrationCallback(ConfigureComponents);
            
            // 初始化GameInstance
            GameInstance.Instance.Initialize();
            
            // 切换到初始状态
            if (_initialState != GameState.Loading)
            {
                GameInstance.Instance.ChangeState(_initialState);
            }
            
            Debug.Log("[GameLauncher] Game initialized");
        }
        
        /// <summary>
        /// 配置组件注册 - 子类可重写此方法来自定义组件注册
        /// </summary>
        protected virtual void ConfigureComponents(IComponentRegistry registry)
        {
            // 注册全局核心组件
            RegisterCoreComponents(registry);
            
            // 注册系统组件
            RegisterSystemComponents(registry);
            
            // 注册自定义组件
            RegisterCustomComponents(registry);
        }
        
        /// <summary>
        /// 注册核心组件
        /// </summary>
        protected virtual void RegisterCoreComponents(IComponentRegistry registry)
        {
            // 核心组件 (全局)
            registry.RegisterGlobal<Components.IEventMgr, Components.EventMgr>(priority: 0);
            registry.RegisterGlobal<Components.ITimerMgr, Components.TimerMgr>(priority: 10);
            registry.RegisterGlobal<Components.IResourceMgr, Components.ResourceMgr>(priority: 20);
            registry.RegisterGlobal<Components.IConfigMgr, Components.ConfigMgr>(priority: 30);
            registry.RegisterGlobal<Components.IAudioMgr, Components.AudioMgr>(priority: 40);
            registry.RegisterGlobal<Components.IInputMgr, Components.InputMgr>(priority: 50);
            registry.RegisterGlobal<Components.IPoolMgr, Components.PoolMgr>(priority: 60);
            registry.RegisterGlobal<Components.ILogMgr, Components.LogMgr>(priority: 70);
        }
        
        /// <summary>
        /// 注册系统组件
        /// </summary>
        protected virtual void RegisterSystemComponents(IComponentRegistry registry)
        {
            // UI管理器 (多状态)
            registry.RegisterForStates<UI.IUIMgr, UI.UIMgr>(
                new[] { GameState.Menu, GameState.Playing, GameState.Paused },
                priority: 100
            );
            
            // 世界上下文 (游戏状态)
            registry.RegisterForState<World.IWorldContext, World.WorldContext>(
                GameState.Playing, 
                priority: 110
            );
            
            // 实体管理器 (游戏状态)
            registry.RegisterForState<Entity.IEntityMgr, Entity.EntityMgr>(
                GameState.Playing, 
                priority: 120
            );
            
            // 单局管理器 (游戏状态)
            registry.RegisterForState<Session.IGameSession, Session.GameSession>(
                GameState.Playing, 
                priority: 130
            );
        }
        
        /// <summary>
        /// 注册自定义组件 - 子类重写此方法来注册游戏特定的组件
        /// </summary>
        protected virtual void RegisterCustomComponents(IComponentRegistry registry)
        {
            // 子类在此注册自定义的Mgr和Mdl
        }
    }
}

