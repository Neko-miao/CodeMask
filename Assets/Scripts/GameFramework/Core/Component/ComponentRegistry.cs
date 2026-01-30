// ================================================
// GameFramework - 组件注册管理实现
// ================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 组件注册管理实现
    /// </summary>
    public class ComponentRegistry : IComponentRegistry
    {
        private readonly Dictionary<Type, ComponentConfig> _configs = new Dictionary<Type, ComponentConfig>();
        private readonly Dictionary<GameState, List<ComponentConfig>> _stateConfigs = new Dictionary<GameState, List<ComponentConfig>>();
        private readonly IComponentContainer _container;
        
        public ComponentRegistry(IComponentContainer container)
        {
            _container = container;
            
            // 初始化状态配置字典
            foreach (GameState state in Enum.GetValues(typeof(GameState)))
            {
                _stateConfigs[state] = new List<ComponentConfig>();
            }
        }
        
        #region Register Methods
        
        /// <summary>
        /// 注册组件
        /// </summary>
        public void Register<TInterface, TImplementation>(GameState[] states, int priority = 100, Type[] dependencies = null)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new()
        {
            var interfaceType = typeof(TInterface);
            
            if (_configs.ContainsKey(interfaceType))
            {
                Debug.LogWarning($"[ComponentRegistry] Component {interfaceType.Name} already registered, will be replaced.");
                Unregister<TInterface>();
            }
            
            var config = new ComponentConfig
            {
                InterfaceType = interfaceType,
                ImplementationType = typeof(TImplementation),
                Priority = priority,
                RequiredStates = states ?? new[] { GameState.Global },
                Dependencies = dependencies ?? Array.Empty<Type>()
            };
            
            _configs[interfaceType] = config;
            
            // 添加到对应状态列表
            foreach (var state in config.RequiredStates)
            {
                if (!_stateConfigs.ContainsKey(state))
                    _stateConfigs[state] = new List<ComponentConfig>();
                    
                _stateConfigs[state].Add(config);
                _stateConfigs[state].Sort((a, b) => a.Priority.CompareTo(b.Priority));
            }
            
            Debug.Log($"[ComponentRegistry] Registered: {interfaceType.Name} -> {typeof(TImplementation).Name}");
        }
        
        /// <summary>
        /// 注册全局组件
        /// </summary>
        public void RegisterGlobal<TInterface, TImplementation>(int priority = 100)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new()
        {
            Register<TInterface, TImplementation>(new[] { GameState.Global }, priority);
        }
        
        /// <summary>
        /// 注册状态组件
        /// </summary>
        public void RegisterForState<TInterface, TImplementation>(GameState state, int priority = 100)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new()
        {
            Register<TInterface, TImplementation>(new[] { state }, priority);
        }
        
        /// <summary>
        /// 注册多状态组件
        /// </summary>
        public void RegisterForStates<TInterface, TImplementation>(GameState[] states, int priority = 100)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new()
        {
            Register<TInterface, TImplementation>(states, priority);
        }
        
        /// <summary>
        /// 注销组件
        /// </summary>
        public void Unregister<TInterface>() where TInterface : class, IGameComponent
        {
            var interfaceType = typeof(TInterface);
            
            if (!_configs.TryGetValue(interfaceType, out var config))
                return;
            
            // 从状态列表中移除
            foreach (var state in config.RequiredStates)
            {
                if (_stateConfigs.TryGetValue(state, out var list))
                {
                    list.Remove(config);
                }
            }
            
            // 从容器中移除实例
            _container.Remove(interfaceType);
            
            _configs.Remove(interfaceType);
            
            Debug.Log($"[ComponentRegistry] Unregistered: {interfaceType.Name}");
        }
        
        #endregion
        
        #region Query Methods
        
        /// <summary>
        /// 获取组件配置
        /// </summary>
        public ComponentConfig GetConfig<TInterface>() where TInterface : class, IGameComponent
        {
            return GetConfig(typeof(TInterface));
        }
        
        /// <summary>
        /// 获取组件配置
        /// </summary>
        public ComponentConfig GetConfig(Type interfaceType)
        {
            _configs.TryGetValue(interfaceType, out var config);
            return config;
        }
        
        /// <summary>
        /// 获取状态对应的组件配置列表
        /// </summary>
        public IReadOnlyList<ComponentConfig> GetConfigsForState(GameState state)
        {
            if (_stateConfigs.TryGetValue(state, out var list))
                return list;
            return Array.Empty<ComponentConfig>();
        }
        
        /// <summary>
        /// 获取所有组件配置
        /// </summary>
        public IReadOnlyList<ComponentConfig> GetAllConfigs()
        {
            return _configs.Values.ToList();
        }
        
        /// <summary>
        /// 检查是否已注册
        /// </summary>
        public bool IsRegistered<TInterface>() where TInterface : class, IGameComponent
        {
            return IsRegistered(typeof(TInterface));
        }
        
        /// <summary>
        /// 检查是否已注册
        /// </summary>
        public bool IsRegistered(Type interfaceType)
        {
            return _configs.ContainsKey(interfaceType);
        }
        
        #endregion
        
        #region State Transition
        
        /// <summary>
        /// 进入状态时触发
        /// </summary>
        public void OnGameStateEnter(GameState state)
        {
            var configs = GetConfigsForState(state);
            
            foreach (var config in configs)
            {
                if (config.Instance == null)
                {
                    // 创建并注册实例
                    var instance = CreateInstance(config);
                    if (instance != null)
                    {
                        config.Instance = instance;
                        _container.Add(config.InterfaceType, instance);
                        
                        var lifecycle = instance as IComponentLifecycle;
                        lifecycle?.OnRegister();
                        lifecycle?.Initialize();
                        lifecycle?.Start();
                    }
                }
            }
        }
        
        /// <summary>
        /// 退出状态时触发
        /// </summary>
        public void OnGameStateExit(GameState state)
        {
            var configs = GetConfigsForState(state);
            
            // 倒序注销
            for (int i = configs.Count - 1; i >= 0; i--)
            {
                var config = configs[i];
                
                // 检查是否在其他活跃状态中使用
                if (IsUsedInOtherStates(config, state))
                    continue;
                
                if (config.Instance != null)
                {
                    var lifecycle = config.Instance as IComponentLifecycle;
                    lifecycle?.Shutdown();
                    lifecycle?.OnUnregister();
                    
                    _container.Remove(config.InterfaceType);
                    config.Instance = null;
                }
            }
        }
        
        /// <summary>
        /// 状态切换时处理组件
        /// </summary>
        public void TransitionComponents(GameState oldState, GameState newState)
        {
            var oldConfigs = new HashSet<ComponentConfig>(GetConfigsForState(oldState));
            var newConfigs = new HashSet<ComponentConfig>(GetConfigsForState(newState));
            var globalConfigs = new HashSet<ComponentConfig>(GetConfigsForState(GameState.Global));
            
            // 合并全局组件
            oldConfigs.UnionWith(globalConfigs);
            newConfigs.UnionWith(globalConfigs);
            
            // 需要卸载的组件 (旧状态有，新状态没有)
            var toUnload = oldConfigs.Except(newConfigs).OrderByDescending(c => c.Priority).ToList();
            
            // 需要加载的组件 (新状态有，旧状态没有)
            var toLoad = newConfigs.Except(oldConfigs).OrderBy(c => c.Priority).ToList();
            
            // 共享组件 (两个状态都有)
            var shared = oldConfigs.Intersect(newConfigs).ToList();
            
            // 1. 卸载旧组件
            foreach (var config in toUnload)
            {
                if (config.Instance != null)
                {
                    var lifecycle = config.Instance as IComponentLifecycle;
                    lifecycle?.Shutdown();
                    lifecycle?.OnUnregister();
                    
                    _container.Remove(config.InterfaceType);
                    config.Instance = null;
                    
                    Debug.Log($"[ComponentRegistry] Unloaded: {config.InterfaceType.Name}");
                }
            }
            
            // 2. 通知共享组件状态变化
            foreach (var config in shared)
            {
                var lifecycle = config.Instance as IComponentLifecycle;
                lifecycle?.OnGameStateChanged(oldState, newState);
            }
            
            // 3. 加载新组件
            foreach (var config in toLoad)
            {
                if (config.Instance == null)
                {
                    var instance = CreateInstance(config);
                    if (instance != null)
                    {
                        config.Instance = instance;
                        _container.Add(config.InterfaceType, instance);
                        
                        var lifecycle = instance as IComponentLifecycle;
                        lifecycle?.OnRegister();
                        lifecycle?.Initialize();
                        lifecycle?.Start();
                        
                        Debug.Log($"[ComponentRegistry] Loaded: {config.InterfaceType.Name}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private IGameComponent CreateInstance(ComponentConfig config)
        {
            try
            {
                return Activator.CreateInstance(config.ImplementationType) as IGameComponent;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ComponentRegistry] Failed to create instance of {config.ImplementationType.Name}: {e.Message}");
                return null;
            }
        }
        
        private bool IsUsedInOtherStates(ComponentConfig config, GameState excludeState)
        {
            foreach (var state in config.RequiredStates)
            {
                if (state != excludeState && state == GameState.Global)
                    return true;
            }
            return false;
        }
        
        #endregion
    }
}

