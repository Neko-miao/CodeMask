// ================================================
// GameFramework - 世界上下文实现
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Entity;
using UnityEngine;

namespace GameFramework.World
{
    /// <summary>
    /// 世界上下文实现 - 提供高级实体查询和管理
    /// </summary>
    [ComponentInfo(Type = ComponentType.System, Priority = 60, RequiredStates = new[] { GameState.Playing })]
    public class WorldContext : GameComponent, IWorldContext
    {
        private PlayerContext _playerContext;
        private SceneContext _sceneContext;
        private IEntityMgr _entityMgr;
        
        public override string ComponentName => "WorldContext";
        public override ComponentType ComponentType => ComponentType.System;
        public override int Priority => 60;
        
        #region Properties
        
        public IPlayerContext PlayerContext => _playerContext;
        public ISceneContext SceneContext => _sceneContext;
        public IEntityMgr EntityMgr => _entityMgr;
        
        #endregion
        
        #region Events
        
        public event Action<IEntity> OnEntityCreated;
        public event Action<IEntity> OnEntityDestroyed;
        public event Action OnWorldReset;
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            _playerContext = new PlayerContext();
            _sceneContext = new SceneContext();
            
            // 获取 EntityMgr 引用
            _entityMgr = GetComp<IEntityMgr>();
            
            // 订阅实体事件
            if (_entityMgr != null)
            {
                _entityMgr.OnEntityCreated += HandleEntityCreated;
                _entityMgr.OnEntityDestroyed += HandleEntityDestroyed;
            }
        }
        
        protected override void OnShutdown()
        {
            // 取消订阅
            if (_entityMgr != null)
            {
                _entityMgr.OnEntityCreated -= HandleEntityCreated;
                _entityMgr.OnEntityDestroyed -= HandleEntityDestroyed;
            }
            
            _playerContext?.Clear();
            _sceneContext?.ClearSpawnPoints();
        }
        
        #endregion
        
        #region Entity Query
        
        public IEntity GetEntity(int entityId)
        {
            return _entityMgr?.GetEntity(entityId);
        }
        
        public IReadOnlyList<IEntity> GetEntitiesWithComp<T>() where T : class, IEntityComp
        {
            return _entityMgr?.GetEntitiesWithComp<T>() ?? Array.Empty<IEntity>();
        }
        
        public IReadOnlyList<IEntity> GetEntitiesWithTag(EntityTag tag)
        {
            return _entityMgr?.GetEntitiesWithTag(tag) ?? Array.Empty<IEntity>();
        }
        
        public IReadOnlyList<IEntity> GetAllEntities()
        {
            return _entityMgr?.GetAllEntities() ?? Array.Empty<IEntity>();
        }
        
        public IReadOnlyList<IEntity> GetEntitiesInRange(Vector3 position, float radius)
        {
            var result = new List<IEntity>();
            var entities = GetEntitiesWithComp<TransformComp>();
            float radiusSqr = radius * radius;
            
            foreach (var entity in entities)
            {
                if (!entity.IsActive) continue;
                
                var transform = entity.GetComp<TransformComp>();
                if (transform == null) continue;
                
                float distSqr = (transform.Position - position).sqrMagnitude;
                if (distSqr <= radiusSqr)
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        public IEntity GetNearestEntity<T>(Vector3 position) where T : class, IEntityComp
        {
            IEntity nearest = null;
            float nearestDistSqr = float.MaxValue;
            
            var entities = GetEntitiesWithComp<T>();
            foreach (var entity in entities)
            {
                if (!entity.IsActive) continue;
                
                var transform = entity.GetComp<TransformComp>();
                if (transform == null) continue;
                
                float distSqr = (transform.Position - position).sqrMagnitude;
                if (distSqr < nearestDistSqr)
                {
                    nearestDistSqr = distSqr;
                    nearest = entity;
                }
            }
            
            return nearest;
        }
        
        public IReadOnlyList<IEntity> FindEntities(Predicate<IEntity> predicate)
        {
            return _entityMgr?.QueryEntities(predicate) ?? Array.Empty<IEntity>();
        }
        
        #endregion
        
        #region Entity Lifecycle
        
        public IEntity CreateEntity(string name = null)
        {
            return _entityMgr?.CreateEntity(name);
        }
        
        public IEntity CreateEntity(string name, Vector3 position, Quaternion rotation)
        {
            var entity = _entityMgr?.CreateEntity(name);
            if (entity != null)
            {
                var transform = entity.AddComp<TransformComp>();
                transform.Position = position;
                transform.RotationQuat = rotation;
            }
            return entity;
        }
        
        public IEntity SpawnEntity(string prefabPath, Vector3 position, Quaternion rotation)
        {
            var entity = CreateEntity(null, position, rotation);
            if (entity != null)
            {
                var render = entity.AddComp<RenderComp>();
                render.PrefabPath = prefabPath;
            }
            return entity;
        }
        
        public void DestroyEntity(int entityId)
        {
            _entityMgr?.DestroyEntity(entityId);
        }
        
        public void DestroyEntity(IEntity entity)
        {
            if (entity != null)
            {
                _entityMgr?.DestroyEntity(entity.Id);
            }
        }
        
        public void DestroyAllEntities()
        {
            _entityMgr?.DestroyAllEntities();
            _playerContext?.Clear();
            OnWorldReset?.Invoke();
        }
        
        #endregion
        
        #region Player
        
        public IEntity GetLocalPlayer()
        {
            return _playerContext?.LocalPlayer;
        }
        
        public IEntity GetPlayer(int playerId)
        {
            return _playerContext?.GetPlayerEntity(playerId);
        }
        
        public IReadOnlyList<IEntity> GetAllPlayers()
        {
            return _playerContext?.GetAllPlayerEntities() ?? Array.Empty<IEntity>();
        }
        
        public void SetLocalPlayer(IEntity player)
        {
            _playerContext?.SetLocalPlayer(player);
        }
        
        #endregion
        
        #region Private Methods
        
        private void HandleEntityCreated(IEntity entity)
        {
            OnEntityCreated?.Invoke(entity);
        }
        
        private void HandleEntityDestroyed(IEntity entity)
        {
            // 如果是玩家实体，从玩家上下文中移除
            if (_playerContext != null && entity.HasTag(EntityTag.Player))
            {
                _playerContext.UnregisterPlayer(entity.Id);
            }
            
            OnEntityDestroyed?.Invoke(entity);
        }
        
        #endregion
    }
}
