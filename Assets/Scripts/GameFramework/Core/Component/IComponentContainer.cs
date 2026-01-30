// ================================================
// GameFramework - 组件容器接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Core
{
    /// <summary>
    /// 组件容器接口
    /// </summary>
    public interface IComponentContainer
    {
        /// <summary>
        /// 添加组件
        /// </summary>
        void Add(Type interfaceType, IGameComponent component);
        
        /// <summary>
        /// 添加组件
        /// </summary>
        void Add<TInterface>(IGameComponent component) where TInterface : class, IGameComponent;
        
        /// <summary>
        /// 移除组件
        /// </summary>
        void Remove(Type interfaceType);
        
        /// <summary>
        /// 移除组件
        /// </summary>
        void Remove<TInterface>() where TInterface : class, IGameComponent;
        
        /// <summary>
        /// 获取组件
        /// </summary>
        T Get<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 获取组件
        /// </summary>
        IGameComponent Get(Type interfaceType);
        
        /// <summary>
        /// 尝试获取组件
        /// </summary>
        bool TryGet<T>(out T component) where T : class, IGameComponent;
        
        /// <summary>
        /// 尝试获取组件
        /// </summary>
        bool TryGet(Type interfaceType, out IGameComponent component);
        
        /// <summary>
        /// 检查组件是否存在
        /// </summary>
        bool Has<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 检查组件是否存在
        /// </summary>
        bool Has(Type interfaceType);
        
        /// <summary>
        /// 获取所有组件
        /// </summary>
        IReadOnlyList<IGameComponent> GetAll();
        
        /// <summary>
        /// 获取所有指定类型的组件
        /// </summary>
        IReadOnlyList<T> GetAll<T>() where T : class, IGameComponent;
        
        /// <summary>
        /// 清空所有组件
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 组件数量
        /// </summary>
        int Count { get; }
    }
}

