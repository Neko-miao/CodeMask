// ================================================
// GameFramework - ECS系统基类
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// ECS系统基类 - 包含逻辑，处理符合条件的实体
    /// </summary>
    public abstract class EntitySystem : IEntitySystem
    {
        protected IEntityMgr _entityMgr;
        protected bool _isInitialized;
        protected bool _isEnabled = true;
        
        // 组件要求
        protected ulong _requiredSignature;
        protected ulong _excludedSignature;
        protected EntityTag _requiredTags = EntityTag.None;
        protected EntityTag _excludedTags = EntityTag.None;
        
        #region Properties
        
        public virtual string SystemName => GetType().Name;
        public virtual int Priority => 0;
        public virtual SystemPhase Phase => SystemPhase.Update;
        
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (_isEnabled != value)
                {
                    _isEnabled = value;
                    if (value) OnEnable();
                    else OnDisable();
                }
            }
        }
        
        public bool IsInitialized => _isInitialized;
        
        public ulong RequiredSignature => _requiredSignature;
        public ulong ExcludedSignature => _excludedSignature;
        public EntityTag RequiredTags => _requiredTags;
        public EntityTag ExcludedTags => _excludedTags;
        
        #endregion
        
        #region Lifecycle
        
        public virtual void OnInit(IEntityMgr entityMgr)
        {
            _entityMgr = entityMgr;
            _isInitialized = true;
            
            // 子类在此配置组件要求
            ConfigureRequirements();
        }
        
        public virtual void OnDestroy()
        {
            _entityMgr = null;
            _isInitialized = false;
        }
        
        public virtual void OnEnable() { }
        
        public virtual void OnDisable() { }
        
        #endregion
        
        #region Configuration
        
        /// <summary>
        /// 配置组件要求 - 子类重写此方法
        /// </summary>
        protected virtual void ConfigureRequirements() { }
        
        /// <summary>
        /// 添加必需组件类型
        /// </summary>
        protected void Require<T>() where T : class, IEntityComp
        {
            var typeId = CompType<T>.Id;
            if (typeId < 64)
            {
                _requiredSignature |= (1UL << typeId);
            }
        }
        
        /// <summary>
        /// 添加排除组件类型
        /// </summary>
        protected void Exclude<T>() where T : class, IEntityComp
        {
            var typeId = CompType<T>.Id;
            if (typeId < 64)
            {
                _excludedSignature |= (1UL << typeId);
            }
        }
        
        /// <summary>
        /// 设置必需标签
        /// </summary>
        protected void RequireTag(EntityTag tag)
        {
            _requiredTags |= tag;
        }
        
        /// <summary>
        /// 设置排除标签
        /// </summary>
        protected void ExcludeTag(EntityTag tag)
        {
            _excludedTags |= tag;
        }
        
        #endregion
        
        #region Entity Events
        
        public virtual void OnEntityEnter(IEntity entity) { }
        
        public virtual void OnEntityExit(IEntity entity) { }
        
        #endregion
        
        #region Update
        
        public virtual void OnUpdate(float deltaTime, IReadOnlyList<IEntity> entities)
        {
            if (!_isEnabled) return;
            
            for (int i = 0; i < entities.Count; i++)
            {
                var entity = entities[i];
                if (entity.IsActive && !entity.IsDestroyed)
                {
                    ProcessEntity(entity, deltaTime);
                }
            }
        }
        
        /// <summary>
        /// 处理单个实体 - 子类必须实现
        /// </summary>
        public abstract void ProcessEntity(IEntity entity, float deltaTime);
        
        #endregion
        
        #region Query
        
        public virtual bool MatchEntity(IEntity entity)
        {
            if (entity == null || !entity.IsActive || entity.IsDestroyed)
                return false;
            
            // 检查组件签名
            var sig = entity.CompSignature;
            
            // 必须包含所有必需组件
            if ((sig & _requiredSignature) != _requiredSignature)
                return false;
            
            // 不能包含任何排除组件
            if (_excludedSignature != 0 && (sig & _excludedSignature) != 0)
                return false;
            
            // 检查标签
            if (_requiredTags != EntityTag.None && !entity.HasAllTags(_requiredTags))
                return false;
            
            if (_excludedTags != EntityTag.None && entity.HasAnyTag(_excludedTags))
                return false;
            
            return true;
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// 获取其他系统
        /// </summary>
        protected T GetSystem<T>() where T : class, IEntitySystem
        {
            return _entityMgr?.GetSystem<T>();
        }
        
        /// <summary>
        /// 获取实体
        /// </summary>
        protected IEntity GetEntity(int entityId)
        {
            return _entityMgr?.GetEntity(entityId);
        }
        
        /// <summary>
        /// 创建实体
        /// </summary>
        protected IEntity CreateEntity(string name = null)
        {
            return _entityMgr?.CreateEntity(name);
        }
        
        /// <summary>
        /// 销毁实体
        /// </summary>
        protected void DestroyEntity(int entityId)
        {
            _entityMgr?.DestroyEntity(entityId);
        }
        
        #endregion
    }
}

