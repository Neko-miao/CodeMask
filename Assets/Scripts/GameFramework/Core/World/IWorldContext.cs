// ================================================
// GameFramework - 世界上下文接口
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Entity;
using UnityEngine;

namespace GameFramework.World
{
    /// <summary>
    /// 世界上下文接口 - 提供高级实体查询和管理
    /// 通过 IEntityMgr 进行底层实体操作
    /// </summary>
    public interface IWorldContext : IGameComponent
    {
        #region Sub Contexts
        
        /// <summary>
        /// 玩家上下文
        /// </summary>
        IPlayerContext PlayerContext { get; }
        
        /// <summary>
        /// 场景上下文
        /// </summary>
        ISceneContext SceneContext { get; }
        
        /// <summary>
        /// 实体管理器
        /// </summary>
        IEntityMgr EntityMgr { get; }
        
        #endregion
        
        #region Entity Query
        
        /// <summary>
        /// 通过ID获取实体
        /// </summary>
        IEntity GetEntity(int entityId);
        
        /// <summary>
        /// 获取所有包含指定组件的实体
        /// </summary>
        IReadOnlyList<IEntity> GetEntitiesWithComp<T>() where T : class, IEntityComp;
        
        /// <summary>
        /// 获取所有包含指定标签的实体
        /// </summary>
        IReadOnlyList<IEntity> GetEntitiesWithTag(EntityTag tag);
        
        /// <summary>
        /// 获取所有实体
        /// </summary>
        IReadOnlyList<IEntity> GetAllEntities();
        
        /// <summary>
        /// 获取范围内的实体
        /// </summary>
        IReadOnlyList<IEntity> GetEntitiesInRange(Vector3 position, float radius);
        
        /// <summary>
        /// 获取最近的带有指定组件的实体
        /// </summary>
        IEntity GetNearestEntity<T>(Vector3 position) where T : class, IEntityComp;
        
        /// <summary>
        /// 条件查询实体
        /// </summary>
        IReadOnlyList<IEntity> FindEntities(Predicate<IEntity> predicate);
        
        #endregion
        
        #region Entity Lifecycle
        
        /// <summary>
        /// 创建实体
        /// </summary>
        IEntity CreateEntity(string name = null);
        
        /// <summary>
        /// 创建实体并初始化位置
        /// </summary>
        IEntity CreateEntity(string name, Vector3 position, Quaternion rotation);
        
        /// <summary>
        /// 生成带渲染的实体
        /// </summary>
        IEntity SpawnEntity(string prefabPath, Vector3 position, Quaternion rotation);
        
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
        
        #endregion
        
        #region Player
        
        /// <summary>
        /// 获取本地玩家
        /// </summary>
        IEntity GetLocalPlayer();
        
        /// <summary>
        /// 获取指定玩家
        /// </summary>
        IEntity GetPlayer(int playerId);
        
        /// <summary>
        /// 获取所有玩家
        /// </summary>
        IReadOnlyList<IEntity> GetAllPlayers();
        
        /// <summary>
        /// 设置本地玩家
        /// </summary>
        void SetLocalPlayer(IEntity player);
        
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
        /// 世界重置事件
        /// </summary>
        event Action OnWorldReset;
        
        #endregion
    }
}
