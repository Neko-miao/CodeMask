// ================================================
// GameFramework - 游戏时间控制器接口
// ================================================

using System;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏时间控制器接口
    /// </summary>
    public interface IGameTimeController
    {
        #region Time Properties
        
        /// <summary>
        /// 全局时间缩放 (0~N, 默认1)
        /// </summary>
        float GlobalTimeScale { get; }
        
        /// <summary>
        /// 游戏运行总时间 (受缩放影响)
        /// </summary>
        float GameTime { get; }
        
        /// <summary>
        /// 真实运行总时间 (不受缩放影响)
        /// </summary>
        float RealTime { get; }
        
        /// <summary>
        /// 帧间隔 (受缩放影响)
        /// </summary>
        float DeltaTime { get; }
        
        /// <summary>
        /// 帧间隔 (不受缩放影响)
        /// </summary>
        float UnscaledDeltaTime { get; }
        
        /// <summary>
        /// 固定帧间隔 (受缩放影响)
        /// </summary>
        float FixedDeltaTime { get; }
        
        /// <summary>
        /// 固定帧间隔 (不受缩放影响)
        /// </summary>
        float UnscaledFixedDeltaTime { get; }
        
        /// <summary>
        /// 当前帧数
        /// </summary>
        int FrameCount { get; }
        
        /// <summary>
        /// 是否暂停
        /// </summary>
        bool IsPaused { get; }
        
        #endregion
        
        #region Time Control
        
        /// <summary>
        /// 设置全局时间缩放
        /// </summary>
        void SetGlobalTimeScale(float scale);
        
        /// <summary>
        /// 暂停时间
        /// </summary>
        void Pause();
        
        /// <summary>
        /// 恢复时间
        /// </summary>
        void Resume();
        
        /// <summary>
        /// 切换暂停状态
        /// </summary>
        void TogglePause();
        
        /// <summary>
        /// 重置时间
        /// </summary>
        void Reset();
        
        #endregion
        
        #region Component Time Scale
        
        /// <summary>
        /// 设置组件时间缩放
        /// </summary>
        void SetComponentTimeScale<T>(float scale) where T : class, IGameComponent;
        
        /// <summary>
        /// 设置组件时间缩放
        /// </summary>
        void SetComponentTimeScale(Type componentType, float scale);
        
        /// <summary>
        /// 获取组件时间缩放
        /// </summary>
        float GetComponentTimeScale<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 获取组件时间缩放
        /// </summary>
        float GetComponentTimeScale(Type componentType);
        
        /// <summary>
        /// 重置组件时间缩放
        /// </summary>
        void ResetComponentTimeScale<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 获取组件缩放后的DeltaTime
        /// </summary>
        float GetScaledDeltaTime<T>() where T : class, IGameComponent;
        
        #endregion
        
        #region Time Layer
        
        /// <summary>
        /// 创建时间层
        /// </summary>
        ITimeLayer CreateTimeLayer(string layerName);
        
        /// <summary>
        /// 获取时间层
        /// </summary>
        ITimeLayer GetTimeLayer(string layerName);
        
        /// <summary>
        /// 移除时间层
        /// </summary>
        void RemoveTimeLayer(string layerName);
        
        /// <summary>
        /// 设置时间层缩放
        /// </summary>
        void SetTimeLayerScale(string layerName, float scale);
        
        /// <summary>
        /// 获取时间层DeltaTime
        /// </summary>
        float GetTimeLayerDeltaTime(string layerName);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 时间缩放改变事件
        /// </summary>
        event Action<float> OnTimeScaleChanged;
        
        /// <summary>
        /// 暂停事件
        /// </summary>
        event Action OnPaused;
        
        /// <summary>
        /// 恢复事件
        /// </summary>
        event Action OnResumed;
        
        #endregion
    }
}

