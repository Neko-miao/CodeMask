// ================================================
// GameFramework - 事件管理器实现
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 事件处理器包装
    /// </summary>
    internal class EventHandler<T> : IComparable<EventHandler<T>>
    {
        public Action<T> Handler { get; }
        public int Priority { get; }
        
        public EventHandler(Action<T> handler, int priority)
        {
            Handler = handler;
            Priority = priority;
        }
        
        public int CompareTo(EventHandler<T> other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
    
    /// <summary>
    /// 事件管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 0, RequiredStates = new[] { GameState.Global })]
    public class EventMgr : GameComponent, IEventMgr
    {
        private readonly Dictionary<Type, object> _handlers = new Dictionary<Type, object>();
        private readonly Queue<Action> _delayedEvents = new Queue<Action>();
        private bool _isProcessingDelayed = false;
        
        public override string ComponentName => "EventMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 0;
        
        #region Subscribe/Unsubscribe
        
        /// <summary>
        /// 订阅事件
        /// </summary>
        public void Subscribe<T>(Action<T> handler) where T : struct
        {
            Subscribe(handler, 0);
        }
        
        /// <summary>
        /// 订阅事件 (带优先级)
        /// </summary>
        public void Subscribe<T>(Action<T> handler, int priority) where T : struct
        {
            if (handler == null)
            {
                Debug.LogWarning("[EventMgr] Cannot subscribe null handler");
                return;
            }
            
            var type = typeof(T);
            
            if (!_handlers.TryGetValue(type, out var list))
            {
                list = new List<EventHandler<T>>();
                _handlers[type] = list;
            }
            
            var handlerList = (List<EventHandler<T>>)list;
            
            // 检查是否已经订阅
            foreach (var h in handlerList)
            {
                if (h.Handler == handler)
                {
                    Debug.LogWarning($"[EventMgr] Handler already subscribed for {type.Name}");
                    return;
                }
            }
            
            handlerList.Add(new EventHandler<T>(handler, priority));
            handlerList.Sort();
        }
        
        /// <summary>
        /// 取消订阅
        /// </summary>
        public void Unsubscribe<T>(Action<T> handler) where T : struct
        {
            if (handler == null) return;
            
            var type = typeof(T);
            
            if (_handlers.TryGetValue(type, out var list))
            {
                var handlerList = (List<EventHandler<T>>)list;
                handlerList.RemoveAll(h => h.Handler == handler);
            }
        }
        
        #endregion
        
        #region Publish
        
        /// <summary>
        /// 发布事件
        /// </summary>
        public void Publish<T>(T eventData) where T : struct
        {
            var type = typeof(T);
            
            if (!_handlers.TryGetValue(type, out var list))
                return;
            
            var handlerList = (List<EventHandler<T>>)list;
            
            // 复制列表以避免在迭代时修改
            var handlers = new List<EventHandler<T>>(handlerList);
            
            foreach (var handler in handlers)
            {
                try
                {
                    handler.Handler?.Invoke(eventData);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventMgr] Error handling event {type.Name}: {e.Message}\n{e.StackTrace}");
                }
            }
        }
        
        /// <summary>
        /// 延迟发布事件
        /// </summary>
        public void PublishDelayed<T>(T eventData) where T : struct
        {
            _delayedEvents.Enqueue(() => Publish(eventData));
        }
        
        #endregion
        
        #region Clear
        
        /// <summary>
        /// 清空所有订阅
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
            _delayedEvents.Clear();
        }
        
        /// <summary>
        /// 清空指定类型的订阅
        /// </summary>
        public void Clear<T>() where T : struct
        {
            _handlers.Remove(typeof(T));
        }
        
        #endregion
        
        #region Query
        
        /// <summary>
        /// 检查是否有订阅者
        /// </summary>
        public bool HasSubscribers<T>() where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
            {
                var handlerList = (List<EventHandler<T>>)list;
                return handlerList.Count > 0;
            }
            return false;
        }
        
        /// <summary>
        /// 获取订阅者数量
        /// </summary>
        public int GetSubscriberCount<T>() where T : struct
        {
            if (_handlers.TryGetValue(typeof(T), out var list))
            {
                var handlerList = (List<EventHandler<T>>)list;
                return handlerList.Count;
            }
            return 0;
        }
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnTick(float deltaTime)
        {
            ProcessDelayedEvents();
        }
        
        private void ProcessDelayedEvents()
        {
            if (_isProcessingDelayed || _delayedEvents.Count == 0)
                return;
            
            _isProcessingDelayed = true;
            
            int count = _delayedEvents.Count;
            for (int i = 0; i < count; i++)
            {
                var action = _delayedEvents.Dequeue();
                try
                {
                    action?.Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[EventMgr] Error processing delayed event: {e.Message}");
                }
            }
            
            _isProcessingDelayed = false;
        }
        
        protected override void OnShutdown()
        {
            Clear();
        }
        
        #endregion
    }
}

