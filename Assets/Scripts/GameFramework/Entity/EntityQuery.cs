// ================================================
// GameFramework - 实体查询器
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 实体查询构建器 - 提供链式查询API
    /// </summary>
    public class EntityQuery
    {
        private readonly IEntityMgr _entityMgr;
        private ulong _requiredSignature;
        private ulong _excludedSignature;
        private EntityTag _requiredTags = EntityTag.None;
        private EntityTag _excludedTags = EntityTag.None;
        private Predicate<IEntity> _customFilter;
        private Vector3? _nearPosition;
        private float _nearRadius;
        
        public EntityQuery(IEntityMgr entityMgr)
        {
            _entityMgr = entityMgr;
        }
        
        #region Filter Methods
        
        /// <summary>
        /// 必须包含组件
        /// </summary>
        public EntityQuery With<T>() where T : class, IEntityComp
        {
            var typeId = CompType<T>.Id;
            if (typeId < 64)
            {
                _requiredSignature |= (1UL << typeId);
            }
            return this;
        }
        
        /// <summary>
        /// 不能包含组件
        /// </summary>
        public EntityQuery Without<T>() where T : class, IEntityComp
        {
            var typeId = CompType<T>.Id;
            if (typeId < 64)
            {
                _excludedSignature |= (1UL << typeId);
            }
            return this;
        }
        
        /// <summary>
        /// 必须有标签
        /// </summary>
        public EntityQuery HasTag(EntityTag tag)
        {
            _requiredTags |= tag;
            return this;
        }
        
        /// <summary>
        /// 不能有标签
        /// </summary>
        public EntityQuery NotTag(EntityTag tag)
        {
            _excludedTags |= tag;
            return this;
        }
        
        /// <summary>
        /// 在指定位置附近
        /// </summary>
        public EntityQuery Near(Vector3 position, float radius)
        {
            _nearPosition = position;
            _nearRadius = radius;
            return this;
        }
        
        /// <summary>
        /// 自定义过滤器
        /// </summary>
        public EntityQuery Where(Predicate<IEntity> filter)
        {
            _customFilter = filter;
            return this;
        }
        
        #endregion
        
        #region Execute Methods
        
        /// <summary>
        /// 获取所有匹配的实体
        /// </summary>
        public IReadOnlyList<IEntity> ToList()
        {
            var all = _entityMgr.GetAllEntities();
            var result = new List<IEntity>();
            
            foreach (var entity in all)
            {
                if (MatchEntity(entity))
                {
                    result.Add(entity);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 获取第一个匹配的实体
        /// </summary>
        public IEntity First()
        {
            var all = _entityMgr.GetAllEntities();
            
            foreach (var entity in all)
            {
                if (MatchEntity(entity))
                {
                    return entity;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取最近的匹配实体
        /// </summary>
        public IEntity Nearest(Vector3 position)
        {
            var all = _entityMgr.GetAllEntities();
            IEntity nearest = null;
            float nearestDistSqr = float.MaxValue;
            
            foreach (var entity in all)
            {
                if (!MatchEntity(entity)) continue;
                
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
        
        /// <summary>
        /// 计算匹配实体数量
        /// </summary>
        public int Count()
        {
            var all = _entityMgr.GetAllEntities();
            int count = 0;
            
            foreach (var entity in all)
            {
                if (MatchEntity(entity))
                {
                    count++;
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// 检查是否有匹配的实体
        /// </summary>
        public bool Any()
        {
            return First() != null;
        }
        
        /// <summary>
        /// 遍历所有匹配的实体
        /// </summary>
        public void ForEach(Action<IEntity> action)
        {
            var all = _entityMgr.GetAllEntities();
            
            foreach (var entity in all)
            {
                if (MatchEntity(entity))
                {
                    action(entity);
                }
            }
        }
        
        /// <summary>
        /// 遍历所有匹配的实体 (带组件)
        /// </summary>
        public void ForEach<T>(Action<IEntity, T> action) where T : class, IEntityComp
        {
            // 确保要求有这个组件
            With<T>();
            
            var all = _entityMgr.GetAllEntities();
            
            foreach (var entity in all)
            {
                if (MatchEntity(entity))
                {
                    var comp = entity.GetComp<T>();
                    if (comp != null)
                    {
                        action(entity, comp);
                    }
                }
            }
        }
        
        /// <summary>
        /// 遍历所有匹配的实体 (带两个组件)
        /// </summary>
        public void ForEach<T1, T2>(Action<IEntity, T1, T2> action) 
            where T1 : class, IEntityComp 
            where T2 : class, IEntityComp
        {
            With<T1>();
            With<T2>();
            
            var all = _entityMgr.GetAllEntities();
            
            foreach (var entity in all)
            {
                if (MatchEntity(entity))
                {
                    var comp1 = entity.GetComp<T1>();
                    var comp2 = entity.GetComp<T2>();
                    if (comp1 != null && comp2 != null)
                    {
                        action(entity, comp1, comp2);
                    }
                }
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private bool MatchEntity(IEntity entity)
        {
            if (entity == null || !entity.IsActive || entity.IsDestroyed)
                return false;
            
            // 检查组件签名
            var sig = entity.CompSignature;
            
            if ((sig & _requiredSignature) != _requiredSignature)
                return false;
            
            if (_excludedSignature != 0 && (sig & _excludedSignature) != 0)
                return false;
            
            // 检查标签
            if (_requiredTags != EntityTag.None && !entity.HasAllTags(_requiredTags))
                return false;
            
            if (_excludedTags != EntityTag.None && entity.HasAnyTag(_excludedTags))
                return false;
            
            // 检查距离
            if (_nearPosition.HasValue)
            {
                var transform = entity.GetComp<TransformComp>();
                if (transform == null) return false;
                
                float distSqr = (transform.Position - _nearPosition.Value).sqrMagnitude;
                if (distSqr > _nearRadius * _nearRadius)
                    return false;
            }
            
            // 自定义过滤器
            if (_customFilter != null && !_customFilter(entity))
                return false;
            
            return true;
        }
        
        #endregion
    }
    
    /// <summary>
    /// EntityMgr扩展方法
    /// </summary>
    public static class EntityMgrQueryExtensions
    {
        /// <summary>
        /// 创建查询构建器
        /// </summary>
        public static EntityQuery Query(this IEntityMgr entityMgr)
        {
            return new EntityQuery(entityMgr);
        }
    }
}

