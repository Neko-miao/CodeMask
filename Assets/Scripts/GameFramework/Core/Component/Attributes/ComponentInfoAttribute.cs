// ================================================
// GameFramework - 组件信息特性
// ================================================

using System;

namespace GameFramework.Core
{
    /// <summary>
    /// 组件信息特性 - 用于标注组件的元数据
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ComponentInfoAttribute : Attribute
    {
        /// <summary>
        /// 组件类型
        /// </summary>
        public ComponentType Type { get; set; } = ComponentType.Custom;
        
        /// <summary>
        /// 优先级 (数值越小越先执行)
        /// </summary>
        public int Priority { get; set; } = 100;
        
        /// <summary>
        /// 需要注册的游戏状态
        /// </summary>
        public GameState[] RequiredStates { get; set; } = new[] { GameState.Global };
        
        /// <summary>
        /// 依赖的组件类型
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
    }
}

