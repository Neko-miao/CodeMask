// ================================================
// GameFramework - ECS管理器接口
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.Entity
{
    /// <summary>
    /// ECS管理器接口 - 统一管理实体和系统
    /// </summary>
    public interface IEntityMgr : IGameComponent
    {
        #region Entity Management
        
        /// <summary>
        /// 创建实体
        /// </summary>
        IEntity CreateEntity(string name = null);
        
        /// <summary>
        /// 创建实体并添加组件
        /// </summary>
        IEntity CreateEntity<T1>(string name = null) 
            where T1 : class, IEntityComp, new();
        
        /// <summary>
        /// 创建实体并添加组件
        /// </summary>
        IEntity CreateEntity<T1, T2>(string name = null) 
            where T1 : class, IEntityComp, new()
            where T2 : class, IEntityComp, new();
        
        /// <summary>
        /// 创建实体并添加组件
        /// </summary>
        IEntity CreateEntity<T1, T2, T3>(string name = null) 
            where T1 : class, IEntityComp, new()
            where T2 : class, IEntityComp, new()
            where T3 : class, IEntityComp, new();
        
        /// <summary>
        /// 销毁实体
        /// </summary>
        void DestroyEntity(int entityId);
        
        /// <summary>
        /// 销毁实体
        /// </summary>
        void DestroyEntity(IEntity entity);
        
        /// <summary>
        /// 销毁所有实体
        /// </summary>
        void DestroyAllEntities();
        
        /// <summary>
        /// 获取实体
        /// </summary>
        IEntity GetEntity(int entityId);
        
        /// <summary>
        /// 检查实体是否存在
        /// </summary>
        bool HasEntity(int entityId);
        
        /// <summary>
        /// 实体总数
        /// </summary>
        int EntityCount { get; }
        
        #endregion
        
        #region Entity Query
        
        /// <summary>
        /// 获取所有实体
        /// </summary>
        IReadOnlyList<IEntity> GetAllEntities();
        
        /// <summary>
        /// 获取拥有指定组件的所有实体
        /// </summary>
        IReadOnlyList<IEntity> GetEntitiesWithComp<T>() where T : class, IEntityComp;
        
        /// <summary>
        /// 获取拥有指定标签的所有实体
        /// </summary>
        IReadOnlyList<IEntity> GetEntitiesWithTag(EntityTag tag);
        
        /// <summary>
        /// 查询实体 (自定义条件)
        /// </summary>
        IReadOnlyList<IEntity> QueryEntities(Predicate<IEntity> predicate);
        
        /// <summary>
        /// 根据组件签名查询实体
        /// </summary>
        IReadOnlyList<IEntity> QueryBySignature(ulong requiredSignature, ulong excludedSignature = 0);
        
        /// <summary>
        /// 遍历所有拥有指定组件的实体
        /// </summary>
        void ForEach<T>(Action<IEntity, T> action) where T : class, IEntityComp;
        
        /// <summary>
        /// 遍历所有拥有指定组件的实体
        /// </summary>
        void ForEach<T1, T2>(Action<IEntity, T1, T2> action) 
            where T1 : class, IEntityComp 
            where T2 : class, IEntityComp;
        
        #endregion
        
        #region System Management
        
        /// <summary>
        /// 注册系统
        /// </summary>
        void RegisterSystem<T>() where T : class, IEntitySystem, new();
        
        /// <summary>
        /// 注册系统 (使用实例)
        /// </summary>
        void RegisterSystem(IEntitySystem system);
        
        /// <summary>
        /// 注销系统
        /// </summary>
        void UnregisterSystem<T>() where T : class, IEntitySystem;
        
        /// <summary>
        /// 获取系统
        /// </summary>
        T GetSystem<T>() where T : class, IEntitySystem;
        
        /// <summary>
        /// 检查是否有系统
        /// </summary>
        bool HasSystem<T>() where T : class, IEntitySystem;
        
        /// <summary>
        /// 启用系统
        /// </summary>
        void EnableSystem<T>() where T : class, IEntitySystem;
        
        /// <summary>
        /// 禁用系统
        /// </summary>
        void DisableSystem<T>() where T : class, IEntitySystem;
        
        /// <summary>
        /// 获取所有系统
        /// </summary>
        IReadOnlyList<IEntitySystem> GetAllSystems();
        
        /// <summary>
        /// 系统数量
        /// </summary>
        int SystemCount { get; }
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 实体创建事件
        /// </summary>
        event Action<IEntity> OnEntityCreated;
        
        /// <summary>
        /// 实体销毁事件
        /// </summary>
        event Action<IEntity> OnEntityDestroyed;
        
        /// <summary>
        /// 系统注册事件
        /// </summary>
        event Action<IEntitySystem> OnSystemRegistered;
        
        /// <summary>
        /// 系统注销事件
        /// </summary>
        event Action<IEntitySystem> OnSystemUnregistered;
        
        #endregion
    }
}
