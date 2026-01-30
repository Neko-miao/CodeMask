// ================================================
// GameFramework - 组件容器实现
// ================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 组件容器实现
    /// </summary>
    public class ComponentContainer : IComponentContainer
    {
        private readonly Dictionary<Type, IGameComponent> _components = new Dictionary<Type, IGameComponent>();
        private readonly List<IGameComponent> _componentList = new List<IGameComponent>();
        private bool _isDirty = false;
        
        /// <summary>
        /// 组件数量
        /// </summary>
        public int Count => _components.Count;
        
        #region Add/Remove
        
        /// <summary>
        /// 添加组件
        /// </summary>
        public void Add(Type interfaceType, IGameComponent component)
        {
            if (component == null)
            {
                Debug.LogError($"[ComponentContainer] Cannot add null component for {interfaceType.Name}");
                return;
            }
            
            if (_components.ContainsKey(interfaceType))
            {
                Debug.LogWarning($"[ComponentContainer] Component {interfaceType.Name} already exists, will be replaced.");
                Remove(interfaceType);
            }
            
            _components[interfaceType] = component;
            _isDirty = true;
        }
        
        /// <summary>
        /// 添加组件
        /// </summary>
        public void Add<TInterface>(IGameComponent component) where TInterface : class, IGameComponent
        {
            Add(typeof(TInterface), component);
        }
        
        /// <summary>
        /// 移除组件
        /// </summary>
        public void Remove(Type interfaceType)
        {
            if (_components.Remove(interfaceType))
            {
                _isDirty = true;
            }
        }
        
        /// <summary>
        /// 移除组件
        /// </summary>
        public void Remove<TInterface>() where TInterface : class, IGameComponent
        {
            Remove(typeof(TInterface));
        }
        
        #endregion
        
        #region Get
        
        /// <summary>
        /// 获取组件
        /// </summary>
        public T Get<T>() where T : class, IGameComponent
        {
            if (_components.TryGetValue(typeof(T), out var component))
            {
                return component as T;
            }
            return null;
        }
        
        /// <summary>
        /// 获取组件
        /// </summary>
        public IGameComponent Get(Type interfaceType)
        {
            _components.TryGetValue(interfaceType, out var component);
            return component;
        }
        
        /// <summary>
        /// 尝试获取组件
        /// </summary>
        public bool TryGet<T>(out T component) where T : class, IGameComponent
        {
            if (_components.TryGetValue(typeof(T), out var comp))
            {
                component = comp as T;
                return component != null;
            }
            component = null;
            return false;
        }
        
        /// <summary>
        /// 尝试获取组件
        /// </summary>
        public bool TryGet(Type interfaceType, out IGameComponent component)
        {
            return _components.TryGetValue(interfaceType, out component);
        }
        
        /// <summary>
        /// 检查组件是否存在
        /// </summary>
        public bool Has<T>() where T : class, IGameComponent
        {
            return _components.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// 检查组件是否存在
        /// </summary>
        public bool Has(Type interfaceType)
        {
            return _components.ContainsKey(interfaceType);
        }
        
        #endregion
        
        #region GetAll
        
        /// <summary>
        /// 获取所有组件
        /// </summary>
        public IReadOnlyList<IGameComponent> GetAll()
        {
            if (_isDirty)
            {
                _componentList.Clear();
                _componentList.AddRange(_components.Values);
                _componentList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                _isDirty = false;
            }
            return _componentList;
        }
        
        /// <summary>
        /// 获取所有指定类型的组件
        /// </summary>
        public IReadOnlyList<T> GetAll<T>() where T : class, IGameComponent
        {
            return _components.Values.OfType<T>().ToList();
        }
        
        #endregion
        
        /// <summary>
        /// 清空所有组件
        /// </summary>
        public void Clear()
        {
            _components.Clear();
            _componentList.Clear();
            _isDirty = false;
        }
    }
}

