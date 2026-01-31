// ================================================
// Game - 战斗者基类
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// 战斗者基类
    /// </summary>
    public abstract class BattleFighter
    {
        /// <summary>
        /// 最大体力
        /// </summary>
        public int MaxHealth { get; protected set; }
        
        /// <summary>
        /// 当前体力
        /// </summary>
        public int CurrentHealth { get; protected set; }
        
        /// <summary>
        /// 当前面具
        /// </summary>
        public MaskType CurrentMask { get; protected set; }
        
        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;
        
        /// <summary>
        /// 体力改变事件
        /// </summary>
        public event Action<int, int> OnHealthChanged;
        
        /// <summary>
        /// 面具改变事件
        /// </summary>
        public event Action<MaskType, MaskType> OnMaskChanged;
        
        /// <summary>
        /// 死亡事件
        /// </summary>
        public event Action OnDeath;
        
        public BattleFighter(int maxHealth, MaskType initialMask)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            CurrentMask = initialMask;
        }
        
        /// <summary>
        /// 受到伤害
        /// </summary>
        public virtual void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive) return;
            
            int oldHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);
            
            Debug.Log($"[{GetType().Name}] Took {damage} damage. Health: {oldHealth} -> {CurrentHealth}");
            
            OnHealthChanged?.Invoke(oldHealth, CurrentHealth);
            
            if (!IsAlive)
            {
                OnDeath?.Invoke();
            }
        }
        
        /// <summary>
        /// 恢复体力
        /// </summary>
        public virtual void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive) return;
            
            int oldHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);
            
            Debug.Log($"[{GetType().Name}] Healed {amount}. Health: {oldHealth} -> {CurrentHealth}");
            
            OnHealthChanged?.Invoke(oldHealth, CurrentHealth);
        }
        
        /// <summary>
        /// 切换面具
        /// </summary>
        public virtual void ChangeMask(MaskType newMask)
        {
            if (CurrentMask == newMask) return;
            
            var oldMask = CurrentMask;
            CurrentMask = newMask;
            
            Debug.Log($"[{GetType().Name}] Mask changed: {oldMask} -> {newMask}");
            
            OnMaskChanged?.Invoke(oldMask, newMask);
        }
        
        /// <summary>
        /// 重置
        /// </summary>
        public virtual void Reset()
        {
            CurrentHealth = MaxHealth;
        }
    }
    
    /// <summary>
    /// 玩家战斗者
    /// </summary>
    public class PlayerFighter : BattleFighter
    {
        /// <summary>
        /// 持有的面具列表
        /// </summary>
        public List<MaskType> OwnedMasks { get; private set; }
        
        /// <summary>
        /// 完美卡点计数
        /// </summary>
        public int PerfectHitCount { get; private set; }
        
        /// <summary>
        /// 当前面具槽位索引
        /// </summary>
        public int CurrentMaskSlot { get; private set; }
        
        /// <summary>
        /// 基础体力
        /// </summary>
        private const int BASE_HEALTH = 3;
        
        public PlayerFighter() : base(BASE_HEALTH, MaskType.Horse)
        {
            OwnedMasks = new List<MaskType> { MaskType.Horse };
            CurrentMaskSlot = 0;
        }
        
        /// <summary>
        /// 添加面具
        /// </summary>
        public void AddMask(MaskType mask)
        {
            if (!OwnedMasks.Contains(mask))
            {
                OwnedMasks.Add(mask);
                // 每有一个面具，最大血量+1
                MaxHealth = BASE_HEALTH + OwnedMasks.Count;
                CurrentHealth = Mathf.Min(CurrentHealth + 1, MaxHealth);
                Debug.Log($"[PlayerFighter] Added mask: {mask}. MaxHealth: {MaxHealth}");
            }
        }
        
        /// <summary>
        /// 切换到指定槽位的面具 (Q=0, W=1, E=2)
        /// </summary>
        public void SwitchToSlot(int slot)
        {
            if (slot < 0 || slot >= OwnedMasks.Count) return;
            
            CurrentMaskSlot = slot;
            ChangeMask(OwnedMasks[slot]);
        }
        
        /// <summary>
        /// 记录完美卡点
        /// </summary>
        public void RecordPerfectHit()
        {
            PerfectHitCount++;
            
            // 触发三次完美卡点后，执行一次攻击
            if (PerfectHitCount >= 3)
            {
                PerfectHitCount = 0;
            }
        }
        
        public override void Reset()
        {
            base.Reset();
            PerfectHitCount = 0;
            CurrentMaskSlot = 0;
            if (OwnedMasks.Count > 0)
            {
                CurrentMask = OwnedMasks[0];
            }
        }
    }
    
    /// <summary>
    /// 敌人战斗者
    /// </summary>
    public class EnemyFighter : BattleFighter
    {
        /// <summary>
        /// 敌人名称
        /// </summary>
        public string Name { get; private set; }
        
        /// <summary>
        /// 攻击伤害
        /// </summary>
        public int AttackDamage { get; private set; }
        
        /// <summary>
        /// 攻击计数 (用于破防等特殊机制)
        /// </summary>
        public int AttackCount { get; private set; }
        
        public EnemyFighter(string name, int health, MaskType mask, int damage) 
            : base(health, mask)
        {
            Name = name;
            AttackDamage = damage;
            AttackCount = 0;
        }
        
        /// <summary>
        /// 执行攻击
        /// </summary>
        public int PerformAttack()
        {
            AttackCount++;
            return AttackDamage;
        }
        
        public override void Reset()
        {
            base.Reset();
            AttackCount = 0;
        }
    }
}
