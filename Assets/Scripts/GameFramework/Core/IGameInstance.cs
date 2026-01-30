// ================================================
// GameFramework - 游戏实例接口
// ================================================

using System;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏实例接口 - 游戏总控制器
    /// </summary>
    public interface IGameInstance
    {
        #region Properties
        
        /// <summary>
        /// 当前游戏状态
        /// </summary>
        GameState CurrentState { get; }
        
        /// <summary>
        /// 上一个游戏状态
        /// </summary>
        GameState PreviousState { get; }
        
        /// <summary>
        /// 时间控制器
        /// </summary>
        IGameTimeController TimeController { get; }
        
        /// <summary>
        /// 组件注册管理器
        /// </summary>
        IComponentRegistry Registry { get; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 是否正在运行
        /// </summary>
        bool IsRunning { get; }
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 关闭游戏
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// 暂停游戏
        /// </summary>
        void Pause();
        
        /// <summary>
        /// 恢复游戏
        /// </summary>
        void Resume();
        
        /// <summary>
        /// 重启游戏
        /// </summary>
        void Restart();
        
        #endregion
        
        #region State Management
        
        /// <summary>
        /// 切换游戏状态
        /// </summary>
        void ChangeState(GameState newState);
        
        #endregion
        
        #region Component Access
        
        /// <summary>
        /// 获取组件 (统一接口)
        /// </summary>
        T GetComp<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 尝试获取组件
        /// </summary>
        bool TryGetComp<T>(out T component) where T : class, IGameComponent;
        
        /// <summary>
        /// 检查组件是否存在
        /// </summary>
        bool HasComp<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 动态添加组件
        /// </summary>
        void AddComp<TInterface, TImplementation>()
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new();
        
        /// <summary>
        /// 动态移除组件
        /// </summary>
        void RemoveComp<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 启用组件
        /// </summary>
        void EnableComp<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 禁用组件
        /// </summary>
        void DisableComp<T>() where T : class, IGameComponent;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 游戏状态改变事件
        /// </summary>
        event Action<GameState, GameState> OnStateChanged;
        
        /// <summary>
        /// 游戏初始化完成事件
        /// </summary>
        event Action OnInitialized;
        
        /// <summary>
        /// 游戏关闭事件
        /// </summary>
        event Action OnShutdown;
        
        /// <summary>
        /// 游戏暂停事件
        /// </summary>
        event Action OnPaused;
        
        /// <summary>
        /// 游戏恢复事件
        /// </summary>
        event Action OnResumed;
        
        #endregion
    }
}

