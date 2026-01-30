// ================================================
// GameFramework - 生命值组件 (纯数据)
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 生命值组件 - 存储生命值相关数据
    /// </summary>
    [Serializable]
    public class HealthComp : EntityComp<HealthComp>
    {
        /// <summary>
        /// 最大生命值
        /// </summary>
        public float MaxHealth = 100f;
        
        /// <summary>
        /// 当前生命值
        /// </summary>
        public float CurrentHealth = 100f;
        
        /// <summary>
        /// 护盾值
        /// </summary>
        public float Shield;
        
        /// <summary>
        /// 最大护盾值
        /// </summary>
        public float MaxShield;
        
        /// <summary>
        /// 是否无敌
        /// </summary>
        public bool IsInvincible;
        
        /// <summary>
        /// 是否死亡
        /// </summary>
        public bool IsDead;
        
        /// <summary>
        /// 上次受到的伤害
        /// </summary>
        public float LastDamage;
        
        /// <summary>
        /// 上次受伤时间
        /// </summary>
        public float LastDamageTime;
        
        /// <summary>
        /// 累计受到的伤害
        /// </summary>
        public float TotalDamageTaken;
        
        /// <summary>
        /// 累计治疗量
        /// </summary>
        public float TotalHealing;
        
        /// <summary>
        /// 生命百分比 (0~1)
        /// </summary>
        public float HealthPercent => MaxHealth > 0 ? CurrentHealth / MaxHealth : 0f;
        
        /// <summary>
        /// 护盾百分比 (0~1)
        /// </summary>
        public float ShieldPercent => MaxShield > 0 ? Shield / MaxShield : 0f;
        
        /// <summary>
        /// 是否满血
        /// </summary>
        public bool IsFullHealth => CurrentHealth >= MaxHealth;
        
        /// <summary>
        /// 是否低血量 (低于30%)
        /// </summary>
        public bool IsLowHealth => HealthPercent < 0.3f;
        
        public override void Reset()
        {
            MaxHealth = 100f;
            CurrentHealth = 100f;
            Shield = 0f;
            MaxShield = 0f;
            IsInvincible = false;
            IsDead = false;
            LastDamage = 0f;
            LastDamageTime = 0f;
            TotalDamageTaken = 0f;
            TotalHealing = 0f;
        }
        
        public override IEntityComp Clone()
        {
            return new HealthComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                MaxHealth = MaxHealth,
                CurrentHealth = CurrentHealth,
                Shield = Shield,
                MaxShield = MaxShield,
                IsInvincible = IsInvincible,
                IsDead = IsDead,
                LastDamage = LastDamage,
                LastDamageTime = LastDamageTime,
                TotalDamageTaken = TotalDamageTaken,
                TotalHealing = TotalHealing
            };
        }
    }
}
