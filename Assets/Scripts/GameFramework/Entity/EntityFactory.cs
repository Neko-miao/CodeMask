// ================================================
// GameFramework - 实体工厂
// ================================================

using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 实体工厂 - 提供便捷的实体创建方法
    /// </summary>
    public static class EntityFactory
    {
        private static IEntityMgr _entityMgr;
        
        /// <summary>
        /// 初始化工厂
        /// </summary>
        public static void Initialize(IEntityMgr entityMgr)
        {
            _entityMgr = entityMgr;
        }
        
        /// <summary>
        /// 检查是否已初始化
        /// </summary>
        public static bool IsInitialized => _entityMgr != null;
        
        #region Basic Creation
        
        /// <summary>
        /// 创建空实体
        /// </summary>
        public static IEntity CreateEmpty(string name = null)
        {
            return _entityMgr?.CreateEntity(name);
        }
        
        /// <summary>
        /// 创建带位置的实体
        /// </summary>
        public static IEntity CreateAt(Vector3 position, string name = null)
        {
            var entity = _entityMgr?.CreateEntity(name);
            if (entity != null)
            {
                var transform = entity.AddComp<TransformComp>();
                transform.Position = position;
            }
            return entity;
        }
        
        /// <summary>
        /// 创建带位置和旋转的实体
        /// </summary>
        public static IEntity CreateAt(Vector3 position, Quaternion rotation, string name = null)
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
        
        #endregion
        
        #region Specialized Creation
        
        /// <summary>
        /// 创建玩家实体
        /// </summary>
        public static IEntity CreatePlayer(Vector3 position, string prefabPath = null)
        {
            var entity = _entityMgr?.CreateEntity("Player");
            if (entity == null) return null;
            
            entity.Tags = EntityTag.Player;
            
            // 基础组件
            var transform = entity.AddComp<TransformComp>();
            transform.Position = position;
            
            entity.AddComp<VelocityComp>();
            entity.AddComp<InputComp>();
            
            var health = entity.AddComp<HealthComp>();
            health.MaxHealth = 100f;
            health.CurrentHealth = 100f;
            
            // 渲染组件
            if (!string.IsNullOrEmpty(prefabPath))
            {
                var render = entity.AddComp<RenderComp>();
                render.PrefabPath = prefabPath;
            }
            
            return entity;
        }
        
        /// <summary>
        /// 创建敌人实体
        /// </summary>
        public static IEntity CreateEnemy(Vector3 position, string prefabPath = null, float maxHealth = 50f)
        {
            var entity = _entityMgr?.CreateEntity("Enemy");
            if (entity == null) return null;
            
            entity.Tags = EntityTag.Enemy;
            
            // 基础组件
            var transform = entity.AddComp<TransformComp>();
            transform.Position = position;
            
            entity.AddComp<VelocityComp>();
            
            var health = entity.AddComp<HealthComp>();
            health.MaxHealth = maxHealth;
            health.CurrentHealth = maxHealth;
            
            // AI组件
            var ai = entity.AddComp<AIComp>();
            ai.HomePosition = position;
            ai.CurrentState = AIState.Idle;
            
            // 渲染组件
            if (!string.IsNullOrEmpty(prefabPath))
            {
                var render = entity.AddComp<RenderComp>();
                render.PrefabPath = prefabPath;
            }
            
            return entity;
        }
        
        /// <summary>
        /// 创建NPC实体
        /// </summary>
        public static IEntity CreateNPC(Vector3 position, string name, string prefabPath = null)
        {
            var entity = _entityMgr?.CreateEntity(name);
            if (entity == null) return null;
            
            entity.Tags = EntityTag.NPC | EntityTag.Interactable;
            
            var transform = entity.AddComp<TransformComp>();
            transform.Position = position;
            
            // 渲染组件
            if (!string.IsNullOrEmpty(prefabPath))
            {
                var render = entity.AddComp<RenderComp>();
                render.PrefabPath = prefabPath;
            }
            
            return entity;
        }
        
        /// <summary>
        /// 创建投射物实体
        /// </summary>
        public static IEntity CreateProjectile(Vector3 position, Vector3 direction, float speed, string prefabPath = null)
        {
            var entity = _entityMgr?.CreateEntity("Projectile");
            if (entity == null) return null;
            
            entity.Tags = EntityTag.Projectile;
            
            var transform = entity.AddComp<TransformComp>();
            transform.Position = position;
            if (direction != Vector3.zero)
            {
                transform.RotationQuat = Quaternion.LookRotation(direction);
            }
            
            var velocity = entity.AddComp<VelocityComp>();
            velocity.LinearVelocity = direction.normalized * speed;
            velocity.UseGravity = false;
            
            // 渲染组件
            if (!string.IsNullOrEmpty(prefabPath))
            {
                var render = entity.AddComp<RenderComp>();
                render.PrefabPath = prefabPath;
            }
            
            return entity;
        }
        
        /// <summary>
        /// 创建道具实体
        /// </summary>
        public static IEntity CreateItem(Vector3 position, string itemId, string prefabPath = null)
        {
            var entity = _entityMgr?.CreateEntity($"Item_{itemId}");
            if (entity == null) return null;
            
            entity.Tags = EntityTag.Item | EntityTag.Interactable;
            
            var transform = entity.AddComp<TransformComp>();
            transform.Position = position;
            
            var tag = entity.AddComp<TagComp>();
            tag.PrimaryTag = itemId;
            
            // 渲染组件
            if (!string.IsNullOrEmpty(prefabPath))
            {
                var render = entity.AddComp<RenderComp>();
                render.PrefabPath = prefabPath;
            }
            
            return entity;
        }
        
        /// <summary>
        /// 创建触发器实体
        /// </summary>
        public static IEntity CreateTrigger(Vector3 position, Vector3 size, string triggerId)
        {
            var entity = _entityMgr?.CreateEntity($"Trigger_{triggerId}");
            if (entity == null) return null;
            
            entity.Tags = EntityTag.Trigger | EntityTag.Static;
            
            var transform = entity.AddComp<TransformComp>();
            transform.Position = position;
            
            var collider = entity.AddComp<ColliderComp>();
            collider.Type = ColliderType.Box;
            collider.Size = size;
            collider.IsTrigger = true;
            
            var tag = entity.AddComp<TagComp>();
            tag.PrimaryTag = triggerId;
            
            return entity;
        }
        
        #endregion
        
        #region Component Helpers
        
        /// <summary>
        /// 为实体添加移动能力
        /// </summary>
        public static void AddMovement(IEntity entity, float moveSpeed = 5f, float maxSpeed = 10f)
        {
            if (entity == null) return;
            
            if (!entity.HasComp<TransformComp>())
            {
                entity.AddComp<TransformComp>();
            }
            
            var velocity = entity.HasComp<VelocityComp>() 
                ? entity.GetComp<VelocityComp>() 
                : entity.AddComp<VelocityComp>();
            
            velocity.MoveSpeed = moveSpeed;
            velocity.MaxSpeed = maxSpeed;
        }
        
        /// <summary>
        /// 为实体添加生命值
        /// </summary>
        public static void AddHealth(IEntity entity, float maxHealth, float currentHealth = -1)
        {
            if (entity == null) return;
            
            var health = entity.HasComp<HealthComp>() 
                ? entity.GetComp<HealthComp>() 
                : entity.AddComp<HealthComp>();
            
            health.MaxHealth = maxHealth;
            health.CurrentHealth = currentHealth < 0 ? maxHealth : currentHealth;
        }
        
        /// <summary>
        /// 为实体添加AI
        /// </summary>
        public static void AddAI(IEntity entity, float alertRange = 15f, float attackRange = 2f)
        {
            if (entity == null) return;
            
            if (!entity.HasComp<TransformComp>())
            {
                entity.AddComp<TransformComp>();
            }
            
            var ai = entity.HasComp<AIComp>() 
                ? entity.GetComp<AIComp>() 
                : entity.AddComp<AIComp>();
            
            ai.AlertRange = alertRange;
            ai.AttackRange = attackRange;
            
            var transform = entity.GetComp<TransformComp>();
            if (transform != null)
            {
                ai.HomePosition = transform.Position;
            }
        }
        
        /// <summary>
        /// 为实体添加渲染
        /// </summary>
        public static void AddRender(IEntity entity, string prefabPath)
        {
            if (entity == null || string.IsNullOrEmpty(prefabPath)) return;
            
            var render = entity.HasComp<RenderComp>() 
                ? entity.GetComp<RenderComp>() 
                : entity.AddComp<RenderComp>();
            
            render.PrefabPath = prefabPath;
        }
        
        #endregion
    }
}

