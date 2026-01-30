// ================================================
// GameFramework - 游戏组件基类
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏组件基类 - 所有Mgr和Mdl的基类
    /// </summary>
    public abstract class GameComponent : IGameComponent, ITickable, IComponentLifecycle
    {
        #region Properties
        
        /// <summary>
        /// 组件名称
        /// </summary>
        public virtual string ComponentName => GetType().Name;
        
        /// <summary>
        /// 组件类型
        /// </summary>
        public virtual ComponentType ComponentType => ComponentType.Custom;
        
        /// <summary>
        /// 优先级
        /// </summary>
        public virtual int Priority => 100;
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        public bool IsInitialized { get; private set; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; private set; } = true;
        
        /// <summary>
        /// 时间缩放
        /// </summary>
        public float TimeScale { get; private set; } = 1f;
        
        /// <summary>
        /// GameInstance引用
        /// </summary>
        protected IGameInstance Game => GameInstance.Instance;
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 设置启用状态
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (IsEnabled == enabled) return;
            IsEnabled = enabled;
            
            if (enabled)
                OnEnabled();
            else
                OnDisabled();
        }
        
        /// <summary>
        /// 设置时间缩放
        /// </summary>
        public void SetTimeScale(float scale)
        {
            TimeScale = Mathf.Max(0f, scale);
        }
        
        #endregion
        
        #region Lifecycle - IComponentLifecycle
        
        /// <summary>
        /// 注册时调用
        /// </summary>
        public void OnRegister()
        {
            OnRegisterInternal();
        }
        
        /// <summary>
        /// 注销时调用
        /// </summary>
        public void OnUnregister()
        {
            OnUnregisterInternal();
        }
        
        /// <summary>
        /// 初始化
        /// </summary>
        public void Initialize()
        {
            if (IsInitialized) return;
            
            OnInit();
            IsInitialized = true;
        }
        
        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            OnStart();
        }
        
        /// <summary>
        /// 关闭
        /// </summary>
        public void Shutdown()
        {
            OnShutdown();
            IsInitialized = false;
        }
        
        /// <summary>
        /// 游戏状态改变
        /// </summary>
        public void OnGameStateChanged(GameState oldState, GameState newState)
        {
            OnStateChanged(oldState, newState);
        }
        
        /// <summary>
        /// 暂停
        /// </summary>
        public void OnPause()
        {
            OnPauseInternal();
        }
        
        /// <summary>
        /// 恢复
        /// </summary>
        public void OnResume()
        {
            OnResumeInternal();
        }
        
        #endregion
        
        #region Tick - ITickable
        
        /// <summary>
        /// 帧更新
        /// </summary>
        public void Tick(float deltaTime)
        {
            if (!IsEnabled) return;
            OnTick(deltaTime * TimeScale);
        }
        
        /// <summary>
        /// 延迟更新
        /// </summary>
        public void LateTick(float deltaTime)
        {
            if (!IsEnabled) return;
            OnLateTick(deltaTime * TimeScale);
        }
        
        /// <summary>
        /// 固定更新
        /// </summary>
        public void FixedTick(float fixedDeltaTime)
        {
            if (!IsEnabled) return;
            OnFixedTick(fixedDeltaTime * TimeScale);
        }
        
        #endregion
        
        #region Protected Virtual Methods
        
        /// <summary>
        /// 注册时
        /// </summary>
        protected virtual void OnRegisterInternal() { }
        
        /// <summary>
        /// 注销时
        /// </summary>
        protected virtual void OnUnregisterInternal() { }
        
        /// <summary>
        /// 初始化
        /// </summary>
        protected virtual void OnInit() { }
        
        /// <summary>
        /// 启动
        /// </summary>
        protected virtual void OnStart() { }
        
        /// <summary>
        /// 关闭
        /// </summary>
        protected virtual void OnShutdown() { }
        
        /// <summary>
        /// 帧更新
        /// </summary>
        protected virtual void OnTick(float deltaTime) { }
        
        /// <summary>
        /// 延迟更新
        /// </summary>
        protected virtual void OnLateTick(float deltaTime) { }
        
        /// <summary>
        /// 固定更新
        /// </summary>
        protected virtual void OnFixedTick(float fixedDeltaTime) { }
        
        /// <summary>
        /// 启用时
        /// </summary>
        protected virtual void OnEnabled() { }
        
        /// <summary>
        /// 禁用时
        /// </summary>
        protected virtual void OnDisabled() { }
        
        /// <summary>
        /// 状态改变
        /// </summary>
        protected virtual void OnStateChanged(GameState oldState, GameState newState) { }
        
        /// <summary>
        /// 暂停时
        /// </summary>
        protected virtual void OnPauseInternal() { }
        
        /// <summary>
        /// 恢复时
        /// </summary>
        protected virtual void OnResumeInternal() { }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// 获取其他组件
        /// </summary>
        protected T GetComp<T>() where T : class, IGameComponent
        {
            return Game?.GetComp<T>();
        }
        
        /// <summary>
        /// 尝试获取其他组件
        /// </summary>
        protected bool TryGetComp<T>(out T component) where T : class, IGameComponent
        {
            component = Game?.GetComp<T>();
            return component != null;
        }
        
        /// <summary>
        /// 检查组件是否存在
        /// </summary>
        protected bool HasComp<T>() where T : class, IGameComponent
        {
            return Game?.HasComp<T>() ?? false;
        }
        
        #endregion
    }
}

