// ================================================
// GameFramework - 生命系统
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 伤害数据
    /// </summary>
    public struct DamageData
    {
        public int SourceEntityId;
        public int TargetEntityId;
        public float Amount;
        public Vector3 HitPoint;
        public Vector3 HitNormal;
        public string DamageType;
    }
    
    /// <summary>
    /// 治疗数据
    /// </summary>
    public struct HealData
    {
        public int SourceEntityId;
        public int TargetEntityId;
        public float Amount;
        public string HealType;
    }
    
    /// <summary>
    /// 生命系统 - 处理伤害、治疗、死亡逻辑
    /// 需要: HealthComp
    /// </summary>
    public class HealthSystem : EntitySystem
    {
        public override string SystemName => "HealthSystem";
        public override int Priority => 200;
        public override SystemPhase Phase => SystemPhase.Update;
        
        #region Events
        
        /// <summary>
        /// 伤害事件
        /// </summary>
        public event Action<DamageData> OnDamage;
        
        /// <summary>
        /// 治疗事件
        /// </summary>
        public event Action<HealData> OnHeal;
        
        /// <summary>
        /// 死亡事件
        /// </summary>
        public event Action<IEntity> OnDeath;
        
        /// <summary>
        /// 复活事件
        /// </summary>
        public event Action<IEntity> OnRevive;
        
        #endregion
        
        protected override void ConfigureRequirements()
        {
            Require<HealthComp>();
        }
        
        public override void ProcessEntity(IEntity entity, float deltaTime)
        {
            // 生命系统主要通过 ApplyDamage/ApplyHeal 方法处理
            // 这里可以做一些周期性检查
        }
        
        #region Public Methods
        
        /// <summary>
        /// 应用伤害
        /// </summary>
        public void ApplyDamage(int targetEntityId, float damage, int sourceEntityId = -1)
        {
            ApplyDamage(new DamageData
            {
                SourceEntityId = sourceEntityId,
                TargetEntityId = targetEntityId,
                Amount = damage
            });
        }
        
        /// <summary>
        /// 应用伤害
        /// </summary>
        public void ApplyDamage(DamageData data)
        {
            var entity = GetEntity(data.TargetEntityId);
            if (entity == null) return;
            
            var health = entity.GetComp<HealthComp>();
            if (health == null || !health.IsEnabled) return;
            if (health.IsDead || health.IsInvincible) return;
            
            float actualDamage = data.Amount;
            
            // 先扣护盾
            if (health.Shield > 0)
            {
                float shieldDamage = Mathf.Min(health.Shield, actualDamage);
                health.Shield -= shieldDamage;
                actualDamage -= shieldDamage;
            }
            
            // 再扣血
            if (actualDamage > 0)
            {
                health.CurrentHealth -= actualDamage;
                health.CurrentHealth = Mathf.Max(0, health.CurrentHealth);
            }
            
            health.LastDamage = data.Amount;
            health.LastDamageTime = Time.time;
            health.TotalDamageTaken += data.Amount;
            
            OnDamage?.Invoke(data);
            
            // 检查死亡
            if (health.CurrentHealth <= 0 && !health.IsDead)
            {
                health.IsDead = true;
                OnDeath?.Invoke(entity);
            }
        }
        
        /// <summary>
        /// 应用治疗
        /// </summary>
        public void ApplyHeal(int targetEntityId, float amount, int sourceEntityId = -1)
        {
            ApplyHeal(new HealData
            {
                SourceEntityId = sourceEntityId,
                TargetEntityId = targetEntityId,
                Amount = amount
            });
        }
        
        /// <summary>
        /// 应用治疗
        /// </summary>
        public void ApplyHeal(HealData data)
        {
            var entity = GetEntity(data.TargetEntityId);
            if (entity == null) return;
            
            var health = entity.GetComp<HealthComp>();
            if (health == null || !health.IsEnabled) return;
            if (health.IsDead) return;
            
            float actualHeal = Mathf.Min(data.Amount, health.MaxHealth - health.CurrentHealth);
            health.CurrentHealth += actualHeal;
            health.TotalHealing += actualHeal;
            
            OnHeal?.Invoke(data);
        }
        
        /// <summary>
        /// 恢复护盾
        /// </summary>
        public void RestoreShield(int entityId, float amount)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var health = entity.GetComp<HealthComp>();
            if (health == null || !health.IsEnabled) return;
            
            health.Shield = Mathf.Min(health.Shield + amount, health.MaxShield);
        }
        
        /// <summary>
        /// 击杀实体
        /// </summary>
        public void Kill(int entityId)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var health = entity.GetComp<HealthComp>();
            if (health == null || health.IsDead) return;
            
            health.CurrentHealth = 0;
            health.IsDead = true;
            
            OnDeath?.Invoke(entity);
        }
        
        /// <summary>
        /// 复活实体
        /// </summary>
        public void Revive(int entityId, float healthPercent = 1f)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var health = entity.GetComp<HealthComp>();
            if (health == null || !health.IsDead) return;
            
            health.IsDead = false;
            health.CurrentHealth = health.MaxHealth * Mathf.Clamp01(healthPercent);
            health.Shield = 0;
            
            OnRevive?.Invoke(entity);
        }
        
        /// <summary>
        /// 设置无敌状态
        /// </summary>
        public void SetInvincible(int entityId, bool invincible)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var health = entity.GetComp<HealthComp>();
            if (health != null)
            {
                health.IsInvincible = invincible;
            }
        }
        
        #endregion
    }
}

