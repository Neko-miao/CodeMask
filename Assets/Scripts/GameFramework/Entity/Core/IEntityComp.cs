// ================================================
// GameFramework - ECS组件接口
// ================================================

using System;

namespace GameFramework.Entity
{
    /// <summary>
    /// ECS组件接口 - 纯数据标记接口
    /// Component只包含数据，不包含逻辑
    /// </summary>
    public interface IEntityComp
    {
        /// <summary>
        /// 组件类型ID (用于快速查找)
        /// </summary>
        int CompTypeId { get; }
        
        /// <summary>
        /// 所属实体ID
        /// </summary>
        int EntityId { get; set; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// 重置组件数据到默认值
        /// </summary>
        void Reset();
        
        /// <summary>
        /// 复制组件数据
        /// </summary>
        IEntityComp Clone();
    }
    
    /// <summary>
    /// 组件类型注册器 - 用于分配组件类型ID
    /// </summary>
    public static class CompTypeRegistry
    {
        private static int _nextTypeId = 0;
        private static readonly object _lock = new object();
        
        /// <summary>
        /// 获取下一个组件类型ID
        /// </summary>
        public static int GetNextTypeId()
        {
            lock (_lock)
            {
                return _nextTypeId++;
            }
        }
        
        /// <summary>
        /// 重置类型ID计数器 (仅用于测试)
        /// </summary>
        public static void Reset()
        {
            lock (_lock)
            {
                _nextTypeId = 0;
            }
        }
    }
    
    /// <summary>
    /// 组件类型ID持有者 - 每个组件类型一个唯一ID
    /// </summary>
    public static class CompType<T> where T : IEntityComp
    {
        public static readonly int Id = CompTypeRegistry.GetNextTypeId();
    }
}
