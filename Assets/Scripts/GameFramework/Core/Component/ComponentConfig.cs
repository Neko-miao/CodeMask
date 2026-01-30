// ================================================
// GameFramework - 组件配置
// ================================================

using System;

namespace GameFramework.Core
{
    /// <summary>
    /// 组件配置
    /// </summary>
    public class ComponentConfig
    {
        /// <summary>
        /// 接口类型
        /// </summary>
        public Type InterfaceType { get; set; }
        
        /// <summary>
        /// 实现类型
        /// </summary>
        public Type ImplementationType { get; set; }
        
        /// <summary>
        /// 组件分类
        /// </summary>
        public ComponentType Category { get; set; } = ComponentType.Custom;
        
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority { get; set; } = 100;
        
        /// <summary>
        /// 需要注册的状态
        /// </summary>
        public GameState[] RequiredStates { get; set; } = new[] { GameState.Global };
        
        /// <summary>
        /// 依赖的其他组件
        /// </summary>
        public Type[] Dependencies { get; set; } = Array.Empty<Type>();
        
        /// <summary>
        /// 是否单例
        /// </summary>
        public bool IsSingleton { get; set; } = true;
        
        /// <summary>
        /// 是否延迟初始化
        /// </summary>
        public bool LazyInit { get; set; } = false;
        
        /// <summary>
        /// 组件实例 (如果已创建)
        /// </summary>
        public IGameComponent Instance { get; set; }
        
        /// <summary>
        /// 创建组件配置
        /// </summary>
        public static ComponentConfig Create<TInterface, TImplementation>(
            GameState[] states = null,
            int priority = 100,
            Type[] dependencies = null)
            where TInterface : class, IGameComponent
            where TImplementation : class, TInterface, new()
        {
            return new ComponentConfig
            {
                InterfaceType = typeof(TInterface),
                ImplementationType = typeof(TImplementation),
                RequiredStates = states ?? new[] { GameState.Global },
                Priority = priority,
                Dependencies = dependencies ?? Array.Empty<Type>()
            };
        }
    }
}

