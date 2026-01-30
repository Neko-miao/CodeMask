// ================================================
// GameFramework - ECS管理器实现
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// ECS管理器实现 - 统一管理实体和系统
    /// </summary>
    [ComponentInfo(Type = ComponentType.System, Priority = 50, RequiredStates = new[] { GameState.Global })]
    public class EntityMgr : GameComponent, IEntityMgr
    {
        #region Fields
        
        // 实体管理
        private readonly Dictionary<int, Entity> _entities = new Dictionary<int, Entity>();
        private readonly List<IEntity> _entityList = new List<IEntity>();
        private readonly Queue<Entity> _entityPool = new Queue<Entity>();
        private readonly List<int> _pendingDestroy = new List<int>();
        private int _nextEntityId = 1;
        
        // 系统管理
        private readonly Dictionary<Type, IEntitySystem> _systems = new Dictionary<Type, IEntitySystem>();
        private readonly List<IEntitySystem> _systemList = new List<IEntitySystem>();
        private readonly Dictionary<SystemPhase, List<IEntitySystem>> _phaseSystems = new Dictionary<SystemPhase, List<IEntitySystem>>();
        
        // 系统-实体映射 (缓存每个系统匹配的实体列表)
        private readonly Dictionary<IEntitySystem, List<IEntity>> _systemEntities = new Dictionary<IEntitySystem, List<IEntity>>();
        private bool _systemEntitiesDirty = true;
        
        #endregion
        
        #region Properties
        
        public override string ComponentName => "EntityMgr";
        public override ComponentType ComponentType => ComponentType.System;
        public override int Priority => 50;
        
        public int EntityCount => _entities.Count;
        public int SystemCount => _systems.Count;
        
        #endregion
        
        #region Events
        
        public event Action<IEntity> OnEntityCreated;
        public event Action<IEntity> OnEntityDestroyed;
        public event Action<IEntitySystem> OnSystemRegistered;
        public event Action<IEntitySystem> OnSystemUnregistered;
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            // 初始化阶段系统列表
            foreach (SystemPhase phase in Enum.GetValues(typeof(SystemPhase)))
            {
                _phaseSystems[phase] = new List<IEntitySystem>();
            }
        }
        
        protected override void OnTick(float deltaTime)
        {
            // 处理待销毁的实体
            ProcessPendingDestroy();
            
            // 更新系统-实体映射
            if (_systemEntitiesDirty)
            {
                UpdateSystemEntities();
            }
            
            // 执行 EarlyUpdate 阶段
            ExecutePhase(SystemPhase.EarlyUpdate, deltaTime);
            
            // 执行 Update 阶段
            ExecutePhase(SystemPhase.Update, deltaTime);
        }
        
        protected override void OnLateTick(float deltaTime)
        {
            // 执行 LateUpdate 阶段
            ExecutePhase(SystemPhase.LateUpdate, deltaTime);
            
            // 执行 PreRender 阶段
            ExecutePhase(SystemPhase.PreRender, deltaTime);
            
            // 执行 PostRender 阶段
            ExecutePhase(SystemPhase.PostRender, deltaTime);
        }
        
        protected override void OnFixedTick(float fixedDeltaTime)
        {
            // 执行 FixedUpdate 阶段
            ExecutePhase(SystemPhase.FixedUpdate, fixedDeltaTime);
        }
        
        protected override void OnShutdown()
        {
            // 销毁所有系统
            foreach (var system in _systemList)
            {
                system.OnDestroy();
            }
            _systems.Clear();
            _systemList.Clear();
            _phaseSystems.Clear();
            _systemEntities.Clear();
            
            // 销毁所有实体
            DestroyAllEntities();
            _entityPool.Clear();
        }
        
        #endregion
        
        #region Entity Management
        
        public IEntity CreateEntity(string name = null)
        {
            Entity entity;
            
            // 从池中获取或创建新实体
            if (_entityPool.Count > 0)
            {
                entity = _entityPool.Dequeue();
                entity.Initialize(_nextEntityId++);
            }
            else
            {
                entity = new Entity(_nextEntityId++);
            }
            
            if (!string.IsNullOrEmpty(name))
            {
                entity.Name = name;
            }
            
            _entities[entity.Id] = entity;
            _entityList.Add(entity);
            _systemEntitiesDirty = true;
            
            // 订阅组件变化事件
            entity.OnCompAdded += OnEntityCompChanged;
            entity.OnCompRemoved += OnEntityCompChanged;
            
            OnEntityCreated?.Invoke(entity);
            
            return entity;
        }
        
        public IEntity CreateEntity<T1>(string name = null) where T1 : class, IEntityComp, new()
        {
            var entity = CreateEntity(name);
            entity.AddComp<T1>();
            return entity;
        }
        
        public IEntity CreateEntity<T1, T2>(string name = null)
            where T1 : class, IEntityComp, new()
            where T2 : class, IEntityComp, new()
        {
            var entity = CreateEntity(name);
            entity.AddComp<T1>();
            entity.AddComp<T2>();
            return entity;
        }
        
        public IEntity CreateEntity<T1, T2, T3>(string name = null)
            where T1 : class, IEntityComp, new()
            where T2 : class, IEntityComp, new()
            where T3 : class, IEntityComp, new()
        {
            var entity = CreateEntity(name);
            entity.AddComp<T1>();
            entity.AddComp<T2>();
            entity.AddComp<T3>();
            return entity;
        }
        
        public void DestroyEntity(int entityId)
        {
            if (!_pendingDestroy.Contains(entityId))
            {
                _pendingDestroy.Add(entityId);
                
                if (_entities.TryGetValue(entityId, out var entity))
                {
                    entity.Destroy();
                }
            }
        }
        
        public void DestroyEntity(IEntity entity)
        {
            if (entity != null)
            {
                DestroyEntity(entity.Id);
            }
        }
        
        public void DestroyAllEntities()
        {
            foreach (var entity in _entityList)
            {
                _pendingDestroy.Add(entity.Id);
                ((Entity)entity).Destroy();
            }
            
            ProcessPendingDestroy();
        }
        
        public IEntity GetEntity(int entityId)
        {
            _entities.TryGetValue(entityId, out var entity);
            return entity;
        }
        
        public bool HasEntity(int entityId)
        {
            return _entities.ContainsKey(entityId);
        }
        
        #endregion
        
        #region Entity Query
        
        public IReadOnlyList<IEntity> GetAllEntities()
        {
            return _entityList;
        }
        
        public IReadOnlyList<IEntity> GetEntitiesWithComp<T>() where T : class, IEntityComp
        {
            var result = new List<IEntity>();
            
            foreach (var entity in _entityList)
            {
                if (entity.HasComp<T>())
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        public IReadOnlyList<IEntity> GetEntitiesWithTag(EntityTag tag)
        {
            var result = new List<IEntity>();
            
            foreach (var entity in _entityList)
            {
                if (entity.HasTag(tag))
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        public IReadOnlyList<IEntity> QueryEntities(Predicate<IEntity> predicate)
        {
            var result = new List<IEntity>();
            
            foreach (var entity in _entityList)
            {
                if (predicate(entity))
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        public IReadOnlyList<IEntity> QueryBySignature(ulong requiredSignature, ulong excludedSignature = 0)
        {
            var result = new List<IEntity>();
            
            foreach (var entity in _entityList)
            {
                var sig = entity.CompSignature;
                
                if ((sig & requiredSignature) == requiredSignature &&
                    (excludedSignature == 0 || (sig & excludedSignature) == 0))
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        public void ForEach<T>(Action<IEntity, T> action) where T : class, IEntityComp
        {
            foreach (var entity in _entityList)
            {
                if (entity.TryGetComp<T>(out var comp))
                {
                    action(entity, comp);
                }
            }
        }
        
        public void ForEach<T1, T2>(Action<IEntity, T1, T2> action)
            where T1 : class, IEntityComp
            where T2 : class, IEntityComp
        {
            foreach (var entity in _entityList)
            {
                if (entity.TryGetComp<T1>(out var comp1) && entity.TryGetComp<T2>(out var comp2))
                {
                    action(entity, comp1, comp2);
                }
            }
        }
        
        #endregion
        
        #region System Management
        
        public void RegisterSystem<T>() where T : class, IEntitySystem, new()
        {
            RegisterSystem(new T());
        }
        
        public void RegisterSystem(IEntitySystem system)
        {
            if (system == null) return;
            
            var type = system.GetType();
            
            if (_systems.ContainsKey(type))
            {
                Debug.LogWarning($"[EntityMgr] System {type.Name} already registered");
                return;
            }
            
            _systems[type] = system;
            _systemList.Add(system);
            
            // 添加到对应阶段
            _phaseSystems[system.Phase].Add(system);
            _phaseSystems[system.Phase].Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            // 初始化系统-实体列表
            _systemEntities[system] = new List<IEntity>();
            
            // 初始化系统
            system.OnInit(this);
            
            _systemEntitiesDirty = true;
            
            OnSystemRegistered?.Invoke(system);
            
            Debug.Log($"[EntityMgr] Registered system: {system.SystemName}");
        }
        
        public void UnregisterSystem<T>() where T : class, IEntitySystem
        {
            var type = typeof(T);
            
            if (!_systems.TryGetValue(type, out var system))
                return;
            
            // 通知所有实体离开此系统
            if (_systemEntities.TryGetValue(system, out var entities))
            {
                foreach (var entity in entities)
                {
                    system.OnEntityExit(entity);
                }
            }
            
            system.OnDestroy();
            
            _systems.Remove(type);
            _systemList.Remove(system);
            _phaseSystems[system.Phase].Remove(system);
            _systemEntities.Remove(system);
            
            OnSystemUnregistered?.Invoke(system);
            
            Debug.Log($"[EntityMgr] Unregistered system: {system.SystemName}");
        }
        
        public T GetSystem<T>() where T : class, IEntitySystem
        {
            _systems.TryGetValue(typeof(T), out var system);
            return system as T;
        }
        
        public bool HasSystem<T>() where T : class, IEntitySystem
        {
            return _systems.ContainsKey(typeof(T));
        }
        
        public void EnableSystem<T>() where T : class, IEntitySystem
        {
            var system = GetSystem<T>();
            if (system != null)
            {
                system.IsEnabled = true;
            }
        }
        
        public void DisableSystem<T>() where T : class, IEntitySystem
        {
            var system = GetSystem<T>();
            if (system != null)
            {
                system.IsEnabled = false;
            }
        }
        
        public IReadOnlyList<IEntitySystem> GetAllSystems()
        {
            return _systemList;
        }
        
        #endregion
        
        #region Private Methods
        
        private void ProcessPendingDestroy()
        {
            if (_pendingDestroy.Count == 0) return;
            
            foreach (var entityId in _pendingDestroy)
            {
                if (_entities.TryGetValue(entityId, out var entity))
                {
                    // 取消订阅事件
                    entity.OnCompAdded -= OnEntityCompChanged;
                    entity.OnCompRemoved -= OnEntityCompChanged;
                    
                    // 通知系统实体离开
                    foreach (var system in _systemList)
                    {
                        if (_systemEntities.TryGetValue(system, out var sysEntities))
                        {
                            if (sysEntities.Contains(entity))
                            {
                                system.OnEntityExit(entity);
                                sysEntities.Remove(entity);
                            }
                        }
                    }
                    
                    OnEntityDestroyed?.Invoke(entity);
                    
                    _entities.Remove(entityId);
                    _entityList.Remove(entity);
                    
                    // 重置并回收到池
                    entity.Reset();
                    _entityPool.Enqueue(entity);
                }
            }
            
            _pendingDestroy.Clear();
        }
        
        private void UpdateSystemEntities()
        {
            foreach (var system in _systemList)
            {
                if (!_systemEntities.TryGetValue(system, out var sysEntities))
                {
                    sysEntities = new List<IEntity>();
                    _systemEntities[system] = sysEntities;
                }
                
                var oldEntities = new HashSet<IEntity>(sysEntities);
                sysEntities.Clear();
                
                foreach (var entity in _entityList)
                {
                    if (system.MatchEntity(entity))
                    {
                        sysEntities.Add(entity);
                        
                        // 新进入的实体
                        if (!oldEntities.Contains(entity))
                        {
                            system.OnEntityEnter(entity);
                        }
                        
                        oldEntities.Remove(entity);
                    }
                }
                
                // 离开的实体
                foreach (var entity in oldEntities)
                {
                    system.OnEntityExit(entity);
                }
            }
            
            _systemEntitiesDirty = false;
        }
        
        private void ExecutePhase(SystemPhase phase, float deltaTime)
        {
            var systems = _phaseSystems[phase];
            
            foreach (var system in systems)
            {
                if (!system.IsEnabled) continue;
                
                if (_systemEntities.TryGetValue(system, out var entities))
                {
                    system.OnUpdate(deltaTime, entities);
                }
            }
        }
        
        private void OnEntityCompChanged(IEntity entity, IEntityComp comp)
        {
            _systemEntitiesDirty = true;
        }
        
        #endregion
    }
}
