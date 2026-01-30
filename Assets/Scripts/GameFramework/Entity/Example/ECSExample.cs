// ================================================
// GameFramework - ECS使用示例
// ================================================

using UnityEngine;
using GameFramework.Core;
using GameFramework.Entity;

namespace GameFramework.Example
{
    /// <summary>
    /// ECS系统使用示例
    /// </summary>
    public class ECSExample : MonoBehaviour
    {
        private IEntityMgr _entityMgr;
        private IEntity _player;
        
        private void Start()
        {
            // 获取EntityMgr
            _entityMgr = GameInstance.Instance?.GetComp<IEntityMgr>();
            if (_entityMgr == null)
            {
                Debug.LogError("EntityMgr not found!");
                return;
            }
            
            // 初始化工厂
            EntityFactory.Initialize(_entityMgr);
            
            // 注册系统
            RegisterSystems();
            
            // 创建实体
            CreateEntities();
        }
        
        private void RegisterSystems()
        {
            // 注册核心系统
            _entityMgr.RegisterSystem<MovementSystem>();
            _entityMgr.RegisterSystem<HealthSystem>();
            _entityMgr.RegisterSystem<RenderSystem>();
            _entityMgr.RegisterSystem<AISystem>();
            _entityMgr.RegisterSystem<InputSystem>();
            
            // 设置生命系统事件
            var healthSystem = _entityMgr.GetSystem<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.OnDeath += OnEntityDeath;
                healthSystem.OnDamage += OnEntityDamage;
            }
            
            // 设置AI系统事件
            var aiSystem = _entityMgr.GetSystem<AISystem>();
            if (aiSystem != null)
            {
                aiSystem.OnStateChanged += OnAIStateChanged;
            }
        }
        
        private void CreateEntities()
        {
            // === 方式1: 使用EntityFactory快速创建 ===
            _player = EntityFactory.CreatePlayer(Vector3.zero, "Prefabs/Player");
            
            // 设置输入控制
            var inputSystem = _entityMgr.GetSystem<InputSystem>();
            inputSystem?.SetControlledEntity(_player);
            
            // === 方式2: 手动创建实体和组件 ===
            var enemy = _entityMgr.CreateEntity("Enemy1");
            enemy.Tags = EntityTag.Enemy;
            
            var transform = enemy.AddComp<TransformComp>();
            transform.Position = new Vector3(10, 0, 10);
            
            var velocity = enemy.AddComp<VelocityComp>();
            velocity.MoveSpeed = 3f;
            
            var health = enemy.AddComp<HealthComp>();
            health.MaxHealth = 50f;
            health.CurrentHealth = 50f;
            
            var ai = enemy.AddComp<AIComp>();
            ai.HomePosition = transform.Position;
            ai.AlertRange = 15f;
            ai.AttackRange = 2f;
            ai.CurrentState = AIState.Patrol;
            
            // 添加渲染
            var render = enemy.AddComp<RenderComp>();
            render.PrefabPath = "Prefabs/Enemy";
            
            // === 方式3: 使用链式创建 ===
            var item = _entityMgr.CreateEntity<TransformComp, RenderComp, TagComp>("HealthPotion");
            item.Tags = EntityTag.Item | EntityTag.Interactable;
            item.GetComp<TransformComp>().Position = new Vector3(5, 0, 5);
            item.GetComp<RenderComp>().PrefabPath = "Prefabs/HealthPotion";
            item.GetComp<TagComp>().PrimaryTag = "health_potion";
        }
        
        /// <summary>
        /// 查询示例
        /// </summary>
        private void QueryExamples()
        {
            // === 基础查询 ===
            
            // 获取所有敌人
            var enemies = _entityMgr.GetEntitiesWithTag(EntityTag.Enemy);
            
            // 获取所有有生命值的实体
            var healthEntities = _entityMgr.GetEntitiesWithComp<HealthComp>();
            
            // === 使用Query构建器 ===
            
            // 获取所有活着的敌人
            var aliveEnemies = _entityMgr.Query()
                .HasTag(EntityTag.Enemy)
                .With<HealthComp>()
                .Where(e => !e.GetComp<HealthComp>().IsDead)
                .ToList();
            
            // 获取玩家附近10米内的敌人
            var nearbyEnemies = _entityMgr.Query()
                .HasTag(EntityTag.Enemy)
                .With<TransformComp>()
                .Near(_player.GetComp<TransformComp>().Position, 10f)
                .ToList();
            
            // 获取最近的可交互物品
            var nearestItem = _entityMgr.Query()
                .HasTag(EntityTag.Interactable)
                .With<TransformComp>()
                .Nearest(_player.GetComp<TransformComp>().Position);
            
            // === ForEach遍历 ===
            
            // 遍历所有有移动能力的实体
            _entityMgr.ForEach<TransformComp, VelocityComp>((entity, transform, velocity) =>
            {
                Debug.Log($"Entity {entity.Name} at {transform.Position} moving at {velocity.Speed}");
            });
            
            // 使用Query构建器遍历
            _entityMgr.Query()
                .HasTag(EntityTag.Enemy)
                .With<HealthComp>()
                .ForEach<HealthComp>((entity, health) =>
                {
                    Debug.Log($"Enemy {entity.Name}: HP {health.CurrentHealth}/{health.MaxHealth}");
                });
        }
        
        /// <summary>
        /// 伤害示例
        /// </summary>
        private void DamageExample()
        {
            var healthSystem = _entityMgr.GetSystem<HealthSystem>();
            if (healthSystem == null) return;
            
            // 对指定实体造成伤害
            healthSystem.ApplyDamage(targetEntityId: 1, damage: 10f, sourceEntityId: _player.Id);
            
            // 使用DamageData
            healthSystem.ApplyDamage(new DamageData
            {
                SourceEntityId = _player.Id,
                TargetEntityId = 1,
                Amount = 25f,
                DamageType = "Fire"
            });
            
            // 治疗
            healthSystem.ApplyHeal(_player.Id, 20f);
            
            // 设置无敌
            healthSystem.SetInvincible(_player.Id, true);
        }
        
        /// <summary>
        /// AI控制示例
        /// </summary>
        private void AIExample()
        {
            var aiSystem = _entityMgr.GetSystem<AISystem>();
            if (aiSystem == null) return;
            
            // 让AI追击玩家
            aiSystem.SetTarget(entityId: 1, targetEntityId: _player.Id);
            
            // 强制设置AI状态
            aiSystem.ForceState(entityId: 1, AIState.Flee);
        }
        
        #region Event Handlers
        
        private void OnEntityDeath(IEntity entity)
        {
            Debug.Log($"Entity {entity.Name} died!");
            
            if (entity.HasTag(EntityTag.Enemy))
            {
                // 敌人死亡，可能掉落物品
                var transform = entity.GetComp<TransformComp>();
                if (transform != null)
                {
                    EntityFactory.CreateItem(transform.Position, "gold", "Prefabs/Gold");
                }
            }
        }
        
        private void OnEntityDamage(DamageData data)
        {
            Debug.Log($"Entity {data.TargetEntityId} took {data.Amount} damage from {data.SourceEntityId}");
        }
        
        private void OnAIStateChanged(IEntity entity, AIState oldState, AIState newState)
        {
            Debug.Log($"AI {entity.Name}: {oldState} -> {newState}");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            // 清理事件订阅
            var healthSystem = _entityMgr?.GetSystem<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.OnDeath -= OnEntityDeath;
                healthSystem.OnDamage -= OnEntityDamage;
            }
            
            var aiSystem = _entityMgr?.GetSystem<AISystem>();
            if (aiSystem != null)
            {
                aiSystem.OnStateChanged -= OnAIStateChanged;
            }
        }
    }
}

