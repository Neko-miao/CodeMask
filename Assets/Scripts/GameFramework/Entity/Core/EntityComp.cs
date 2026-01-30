// ================================================
// GameFramework - ECS组件基类
// ================================================

using System;

namespace GameFramework.Entity
{
    /// <summary>
    /// ECS组件基类 - 纯数据容器
    /// 继承此类实现具体的组件数据
    /// </summary>
    [Serializable]
    public abstract class EntityComp : IEntityComp
    {
        /// <summary>
        /// 组件类型ID
        /// </summary>
        public abstract int CompTypeId { get; }
        
        /// <summary>
        /// 所属实体ID
        /// </summary>
        public int EntityId { get; set; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        public bool IsEnabled { get; set; } = true;
        
        /// <summary>
        /// 重置组件数据到默认值
        /// </summary>
        public abstract void Reset();
        
        /// <summary>
        /// 复制组件数据
        /// </summary>
        public abstract IEntityComp Clone();
    }
    
    /// <summary>
    /// 泛型组件基类 - 自动分配类型ID
    /// </summary>
    [Serializable]
    public abstract class EntityComp<T> : EntityComp where T : EntityComp<T>, new()
    {
        private static readonly int _typeId = CompType<T>.Id;
        
        public override int CompTypeId => _typeId;
        
        /// <summary>
        /// 获取此组件类型的ID
        /// </summary>
        public static int TypeId => _typeId;
    }
}

