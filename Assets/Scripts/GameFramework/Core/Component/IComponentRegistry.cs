// ================================================
// GameFramework - 组件注册管理接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Core
{
    /// <summary>
    /// 组件注册管理接口
    /// </summary>
    public interface IComponentRegistry
    {
        /// <summary>
        /// 注册组件
        /// </summary>
        void Register<TInterface, TImplementation>(GameState[] states, int priority = 100, Type[] dependencies = null)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new();
        
        /// <summary>
        /// 注册全局组件
        /// </summary>
        void RegisterGlobal<TInterface, TImplementation>(int priority = 100)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new();
        
        /// <summary>
        /// 注册状态组件
        /// </summary>
        void RegisterForState<TInterface, TImplementation>(GameState state, int priority = 100)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new();
        
        /// <summary>
        /// 注册多状态组件
        /// </summary>
        void RegisterForStates<TInterface, TImplementation>(GameState[] states, int priority = 100)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new();
        
        /// <summary>
        /// 注销组件
        /// </summary>
        void Unregister<TInterface>() where TInterface : class, IGameComponent;
        
        /// <summary>
        /// 获取组件配置
        /// </summary>
        ComponentConfig GetConfig<TInterface>() where TInterface : class, IGameComponent;
        
        /// <summary>
        /// 获取组件配置
        /// </summary>
        ComponentConfig GetConfig(Type interfaceType);
        
        /// <summary>
        /// 获取状态对应的组件配置列表
        /// </summary>
        IReadOnlyList<ComponentConfig> GetConfigsForState(GameState state);
        
        /// <summary>
        /// 获取所有组件配置
        /// </summary>
        IReadOnlyList<ComponentConfig> GetAllConfigs();
        
        /// <summary>
        /// 检查是否已注册
        /// </summary>
        bool IsRegistered<TInterface>() where TInterface : class, IGameComponent;
        
        /// <summary>
        /// 检查是否已注册
        /// </summary>
        bool IsRegistered(Type interfaceType);
        
        /// <summary>
        /// 进入状态时触发
        /// </summary>
        void OnGameStateEnter(GameState state);
        
        /// <summary>
        /// 退出状态时触发
        /// </summary>
        void OnGameStateExit(GameState state);
        
        /// <summary>
        /// 状态切换时处理组件
        /// </summary>
        void TransitionComponents(GameState oldState, GameState newState);
    }
}

