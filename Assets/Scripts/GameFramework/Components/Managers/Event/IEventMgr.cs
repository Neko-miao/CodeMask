// ================================================
// GameFramework - 事件管理器接口
// ================================================

using System;
using GameFramework.Core;

namespace GameFramework.Components
{
    /// <summary>
    /// 事件管理器接口
    /// </summary>
    public interface IEventMgr : IGameComponent
    {
        /// <summary>
        /// 订阅事件
        /// </summary>
        void Subscribe<T>(Action<T> handler) where T : struct;
        
        /// <summary>
        /// 订阅事件 (带优先级)
        /// </summary>
        void Subscribe<T>(Action<T> handler, int priority) where T : struct;
        
        /// <summary>
        /// 取消订阅
        /// </summary>
        void Unsubscribe<T>(Action<T> handler) where T : struct;
        
        /// <summary>
        /// 发布事件
        /// </summary>
        void Publish<T>(T eventData) where T : struct;
        
        /// <summary>
        /// 延迟发布事件 (下一帧)
        /// </summary>
        void PublishDelayed<T>(T eventData) where T : struct;
        
        /// <summary>
        /// 清空所有订阅
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 清空指定类型的订阅
        /// </summary>
        void Clear<T>() where T : struct;
        
        /// <summary>
        /// 检查是否有订阅者
        /// </summary>
        bool HasSubscribers<T>() where T : struct;
        
        /// <summary>
        /// 获取订阅者数量
        /// </summary>
        int GetSubscriberCount<T>() where T : struct;
    }
}

