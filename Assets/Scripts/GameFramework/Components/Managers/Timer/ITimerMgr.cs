// ================================================
// GameFramework - 定时器管理器接口
// ================================================

using System;
using GameFramework.Core;

namespace GameFramework.Components
{
    /// <summary>
    /// 定时器管理器接口
    /// </summary>
    public interface ITimerMgr : IGameComponent
    {
        /// <summary>
        /// 延迟执行
        /// </summary>
        /// <param name="delay">延迟时间(秒)</param>
        /// <param name="callback">回调</param>
        /// <param name="useUnscaledTime">是否使用不受缩放影响的时间</param>
        /// <returns>定时器ID</returns>
        int Schedule(float delay, Action callback, bool useUnscaledTime = false);
        
        /// <summary>
        /// 重复执行
        /// </summary>
        /// <param name="interval">间隔时间(秒)</param>
        /// <param name="callback">回调</param>
        /// <param name="repeatCount">重复次数 (-1表示无限)</param>
        /// <param name="useUnscaledTime">是否使用不受缩放影响的时间</param>
        /// <returns>定时器ID</returns>
        int ScheduleRepeating(float interval, Action callback, int repeatCount = -1, bool useUnscaledTime = false);
        
        /// <summary>
        /// 延迟后重复执行
        /// </summary>
        int ScheduleRepeating(float delay, float interval, Action callback, int repeatCount = -1, bool useUnscaledTime = false);
        
        /// <summary>
        /// 取消定时器
        /// </summary>
        void Cancel(int timerId);
        
        /// <summary>
        /// 取消所有定时器
        /// </summary>
        void CancelAll();
        
        /// <summary>
        /// 暂停定时器
        /// </summary>
        void Pause(int timerId);
        
        /// <summary>
        /// 恢复定时器
        /// </summary>
        void Resume(int timerId);
        
        /// <summary>
        /// 检查定时器是否存在
        /// </summary>
        bool Exists(int timerId);
        
        /// <summary>
        /// 检查定时器是否暂停
        /// </summary>
        bool IsPaused(int timerId);
        
        /// <summary>
        /// 获取定时器剩余时间
        /// </summary>
        float GetRemainingTime(int timerId);
        
        /// <summary>
        /// 获取活跃的定时器数量
        /// </summary>
        int ActiveCount { get; }
    }
}

