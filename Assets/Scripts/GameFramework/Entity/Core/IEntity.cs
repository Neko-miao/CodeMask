// ================================================
// GameFramework - ECS实体接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Entity
{
    /// <summary>
    /// 实体标签 - 用于快速分类和查询
    /// </summary>
    [Flags]
    public enum EntityTag : uint
    {
        None = 0,
        Player = 1 << 0,
        Enemy = 1 << 1,
        NPC = 1 << 2,
        Projectile = 1 << 3,
        Item = 1 << 4,
        Trigger = 1 << 5,
        Static = 1 << 6,
        Dynamic = 1 << 7,
        Destructible = 1 << 8,
        Interactable = 1 << 9,
        // 可扩展...
        All = uint.MaxValue
    }
    
    /// <summary>
    /// ECS实体接口 - 实体只是一个ID和组件容器
    /// Entity不包含任何逻辑，只负责组件的存储和访问
    /// </summary>
    public interface IEntity
    {
        #region Identity
        
        /// <summary>
        /// 实体唯一ID
        /// </summary>
        int Id { get; }
        
        /// <summary>
        /// 实体名称 (可选，用于调试)
        /// </summary>
        string Name { get; set; }
        
        /// <summary>
        /// 实体标签 (位掩码，用于快速过滤)
        /// </summary>
        EntityTag Tags { get; set; }
        
        /// <summary>
        /// 是否激活
        /// </summary>
        bool IsActive { get; set; }
        
        /// <summary>
        /// 是否已销毁
        /// </summary>
        bool IsDestroyed { get; }
        
        /// <summary>
        /// 实体版本号 (用于检测实体是否被回收复用)
        /// </summary>
        int Version { get; }
        
        #endregion
        
        #region Component Access
        
        /// <summary>
        /// 添加组件
        /// </summary>
        T AddComp<T>() where T : class, IEntityComp, new();
        
        /// <summary>
        /// 添加组件 (使用已有实例)
        /// </summary>
        void AddComp(IEntityComp comp);
        
        /// <summary>
        /// 获取组件
        /// </summary>
        T GetComp<T>() where T : class, IEntityComp;
        
        /// <summary>
        /// 尝试获取组件
        /// </summary>
        bool TryGetComp<T>(out T comp) where T : class, IEntityComp;
        
        /// <summary>
        /// 检查是否有组件
        /// </summary>
        bool HasComp<T>() where T : class, IEntityComp;
        
        /// <summary>
        /// 检查是否有指定类型ID的组件
        /// </summary>
        bool HasComp(int compTypeId);
        
        /// <summary>
        /// 移除组件
        /// </summary>
        bool RemoveComp<T>() where T : class, IEntityComp;
        
        /// <summary>
        /// 移除组件
        /// </summary>
        bool RemoveComp(int compTypeId);
        
        /// <summary>
        /// 获取所有组件
        /// </summary>
        IReadOnlyList<IEntityComp> GetAllComps();
        
        /// <summary>
        /// 组件数量
        /// </summary>
        int CompCount { get; }
        
        /// <summary>
        /// 组件签名 (位掩码，标识拥有哪些组件类型)
        /// </summary>
        ulong CompSignature { get; }
        
        #endregion
        
        #region Tag Operations
        
        /// <summary>
        /// 添加标签
        /// </summary>
        void AddTag(EntityTag tag);
        
        /// <summary>
        /// 移除标签
        /// </summary>
        void RemoveTag(EntityTag tag);
        
        /// <summary>
        /// 是否有标签
        /// </summary>
        bool HasTag(EntityTag tag);
        
        /// <summary>
        /// 是否有任意一个标签
        /// </summary>
        bool HasAnyTag(EntityTag tags);
        
        /// <summary>
        /// 是否有所有标签
        /// </summary>
        bool HasAllTags(EntityTag tags);
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// 标记为销毁 (延迟到帧末处理)
        /// </summary>
        void Destroy();
        
        /// <summary>
        /// 重置实体 (用于对象池回收)
        /// </summary>
        void Reset();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 组件添加事件
        /// </summary>
        event Action<IEntity, IEntityComp> OnCompAdded;
        
        /// <summary>
        /// 组件移除事件
        /// </summary>
        event Action<IEntity, IEntityComp> OnCompRemoved;
        
        #endregion
    }
}
