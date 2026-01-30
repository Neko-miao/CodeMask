// ================================================
// GameFramework - 定时器管理器实现
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 定时器数据
    /// </summary>
    internal class TimerData
    {
        public int Id;
        public float Delay;
        public float Interval;
        public float ElapsedTime;
        public Action Callback;
        public int RepeatCount;
        public int CurrentRepeat;
        public bool IsPaused;
        public bool UseUnscaledTime;
        public bool IsCompleted;
    }
    
    /// <summary>
    /// 定时器管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 10, RequiredStates = new[] { GameState.Global })]
    public class TimerMgr : GameComponent, ITimerMgr
    {
        private readonly Dictionary<int, TimerData> _timers = new Dictionary<int, TimerData>();
        private readonly List<int> _toRemove = new List<int>();
        private int _nextId = 1;
        
        public override string ComponentName => "TimerMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 10;
        
        /// <summary>
        /// 活跃的定时器数量
        /// </summary>
        public int ActiveCount => _timers.Count;
        
        #region Schedule
        
        /// <summary>
        /// 延迟执行
        /// </summary>
        public int Schedule(float delay, Action callback, bool useUnscaledTime = false)
        {
            return ScheduleRepeating(delay, 0f, callback, 1, useUnscaledTime);
        }
        
        /// <summary>
        /// 重复执行
        /// </summary>
        public int ScheduleRepeating(float interval, Action callback, int repeatCount = -1, bool useUnscaledTime = false)
        {
            return ScheduleRepeating(0f, interval, callback, repeatCount, useUnscaledTime);
        }
        
        /// <summary>
        /// 延迟后重复执行
        /// </summary>
        public int ScheduleRepeating(float delay, float interval, Action callback, int repeatCount = -1, bool useUnscaledTime = false)
        {
            if (callback == null)
            {
                Debug.LogWarning("[TimerMgr] Cannot schedule null callback");
                return -1;
            }
            
            var timer = new TimerData
            {
                Id = _nextId++,
                Delay = delay,
                Interval = interval,
                ElapsedTime = 0f,
                Callback = callback,
                RepeatCount = repeatCount,
                CurrentRepeat = 0,
                IsPaused = false,
                UseUnscaledTime = useUnscaledTime,
                IsCompleted = false
            };
            
            _timers[timer.Id] = timer;
            
            return timer.Id;
        }
        
        #endregion
        
        #region Control
        
        /// <summary>
        /// 取消定时器
        /// </summary>
        public void Cancel(int timerId)
        {
            if (_timers.ContainsKey(timerId))
            {
                _timers[timerId].IsCompleted = true;
            }
        }
        
        /// <summary>
        /// 取消所有定时器
        /// </summary>
        public void CancelAll()
        {
            foreach (var timer in _timers.Values)
            {
                timer.IsCompleted = true;
            }
        }
        
        /// <summary>
        /// 暂停定时器
        /// </summary>
        public void Pause(int timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsPaused = true;
            }
        }
        
        /// <summary>
        /// 恢复定时器
        /// </summary>
        public void Resume(int timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                timer.IsPaused = false;
            }
        }
        
        #endregion
        
        #region Query
        
        /// <summary>
        /// 检查定时器是否存在
        /// </summary>
        public bool Exists(int timerId)
        {
            return _timers.ContainsKey(timerId) && !_timers[timerId].IsCompleted;
        }
        
        /// <summary>
        /// 检查定时器是否暂停
        /// </summary>
        public bool IsPaused(int timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                return timer.IsPaused;
            }
            return false;
        }
        
        /// <summary>
        /// 获取定时器剩余时间
        /// </summary>
        public float GetRemainingTime(int timerId)
        {
            if (_timers.TryGetValue(timerId, out var timer))
            {
                float targetTime = timer.CurrentRepeat == 0 ? timer.Delay : timer.Interval;
                return Mathf.Max(0f, targetTime - timer.ElapsedTime);
            }
            return 0f;
        }
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnTick(float deltaTime)
        {
            if (_timers.Count == 0) return;
            
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            _toRemove.Clear();
            
            foreach (var kvp in _timers)
            {
                var timer = kvp.Value;
                
                if (timer.IsCompleted)
                {
                    _toRemove.Add(timer.Id);
                    continue;
                }
                
                if (timer.IsPaused)
                    continue;
                
                float dt = timer.UseUnscaledTime ? unscaledDeltaTime : deltaTime;
                timer.ElapsedTime += dt;
                
                // 检查是否触发
                float targetTime = timer.CurrentRepeat == 0 ? timer.Delay : timer.Interval;
                
                if (timer.ElapsedTime >= targetTime)
                {
                    timer.ElapsedTime -= targetTime;
                    timer.CurrentRepeat++;
                    
                    try
                    {
                        timer.Callback?.Invoke();
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[TimerMgr] Error in timer callback: {e.Message}\n{e.StackTrace}");
                    }
                    
                    // 检查是否完成
                    if (timer.RepeatCount > 0 && timer.CurrentRepeat >= timer.RepeatCount)
                    {
                        timer.IsCompleted = true;
                        _toRemove.Add(timer.Id);
                    }
                }
            }
            
            // 移除已完成的定时器
            foreach (var id in _toRemove)
            {
                _timers.Remove(id);
            }
        }
        
        protected override void OnShutdown()
        {
            CancelAll();
            _timers.Clear();
        }
        
        #endregion
    }
}

