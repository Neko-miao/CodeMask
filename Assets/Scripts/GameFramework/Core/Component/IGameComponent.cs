// ================================================
// GameFramework - 游戏组件接口
// ================================================

using System;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏组件接口 - 所有Mgr和Mdl的基础接口
    /// </summary>
    public interface IGameComponent
    {
        /// <summary>
        /// 组件名称
        /// </summary>
        string ComponentName { get; }
        
        /// <summary>
        /// 组件类型
        /// </summary>
        ComponentType ComponentType { get; }
        
        /// <summary>
        /// 优先级 (数值越小越先执行)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; }
        
        /// <summary>
        /// 组件专属时间缩放
        /// </summary>
        float TimeScale { get; }
        
        /// <summary>
        /// 设置启用状态
        /// </summary>
        void SetEnabled(bool enabled);
        
        /// <summary>
        /// 设置时间缩放
        /// </summary>
        void SetTimeScale(float scale);
    }
    
    /// <summary>
    /// 可更新的组件接口
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// 帧更新
        /// </summary>
        void Tick(float deltaTime);
        
        /// <summary>
        /// 延迟更新
        /// </summary>
        void LateTick(float deltaTime);
        
        /// <summary>
        /// 固定更新
        /// </summary>
        void FixedTick(float fixedDeltaTime);
    }
    
    /// <summary>
    /// 组件生命周期接口
    /// </summary>
    public interface IComponentLifecycle
    {
        /// <summary>
        /// 注册时调用
        /// </summary>
        void OnRegister();
        
        /// <summary>
        /// 注销时调用
        /// </summary>
        void OnUnregister();
        
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 启动
        /// </summary>
        void Start();
        
        /// <summary>
        /// 关闭
        /// </summary>
        void Shutdown();
        
        /// <summary>
        /// 游戏状态改变时
        /// </summary>
        void OnGameStateChanged(GameState oldState, GameState newState);
        
        /// <summary>
        /// 暂停时
        /// </summary>
        void OnPause();
        
        /// <summary>
        /// 恢复时
        /// </summary>
        void OnResume();
    }
}

