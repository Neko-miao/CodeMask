// ================================================
// MaskSystem - 敌人对象
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 面具系统敌人对象 - 独立于现有Enemy
    /// </summary>
    public class MaskEnemy
    {
        #region 属性

        /// <summary>
        /// 敌人名称
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// 最大血量
        /// </summary>
        public int MaxHealth { get; private set; }

        /// <summary>
        /// 当前血量
        /// </summary>
        public int CurrentHealth { get; private set; }

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// 敌人的面具类型
        /// </summary>
        public MaskType MaskType { get; private set; }

        /// <summary>
        /// 攻击力
        /// </summary>
        public int AttackPower { get; private set; }

        /// <summary>
        /// 攻击计数（用于特殊机制如牛的破防）
        /// </summary>
        public int AttackCount { get; private set; }

        #endregion

        #region 事件

        /// <summary>
        /// 血量改变事件 (旧值, 新值)
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>
        /// 死亡事件
        /// </summary>
        public event Action OnDeath;

        #endregion

        #region 构造函数

        public MaskEnemy(string name, MaskType maskType, int health, int attackPower)
        {
            Name = name;
            MaskType = maskType;
            MaxHealth = health;
            CurrentHealth = health;
            AttackPower = attackPower;
            AttackCount = 0;

            Debug.Log($"[MaskEnemy] 创建敌人 - 名称: {name}, 面具: {maskType}, 血量: {health}, 攻击力: {attackPower}");
        }

        /// <summary>
        /// 从预设创建敌人
        /// </summary>
        public static MaskEnemy FromPreset(EnemyPreset preset)
        {
            if (preset == null) return null;
            return new MaskEnemy(preset.Name, preset.MaskType, preset.Health, preset.AttackPower);
        }

        #endregion

        #region 战斗操作

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (damage <= 0 || !IsAlive) return;

            int oldHealth = CurrentHealth;
            CurrentHealth = Mathf.Max(0, CurrentHealth - damage);

            Debug.Log($"[MaskEnemy] {Name} 受到 {damage} 点伤害, 血量: {oldHealth} -> {CurrentHealth}");
            OnHealthChanged?.Invoke(oldHealth, CurrentHealth);

            if (!IsAlive)
            {
                Debug.Log($"[MaskEnemy] {Name} 被击败!");
                OnDeath?.Invoke();
            }
        }

        /// <summary>
        /// 执行攻击，返回伤害值
        /// </summary>
        public int PerformAttack()
        {
            AttackCount++;
            int damage = AttackPower;

            // 牛的破防机制：每5次攻击造成额外伤害
            if (MaskType == MaskType.Bull && AttackCount % 5 == 0)
            {
                damage += 1;
                Debug.Log($"[MaskEnemy] {Name} 触发破防攻击! 伤害: {damage}");
            }
            else
            {
                Debug.Log($"[MaskEnemy] {Name} 攻击! 伤害: {damage}, 攻击次数: {AttackCount}");
            }

            return damage;
        }

        /// <summary>
        /// 恢复血量
        /// </summary>
        public void Heal(int amount)
        {
            if (amount <= 0 || !IsAlive) return;

            int oldHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(MaxHealth, CurrentHealth + amount);

            if (oldHealth != CurrentHealth)
            {
                Debug.Log($"[MaskEnemy] {Name} 恢复 {amount} 点血量, 血量: {oldHealth} -> {CurrentHealth}");
                OnHealthChanged?.Invoke(oldHealth, CurrentHealth);
            }
        }

        /// <summary>
        /// 重置状态
        /// </summary>
        public void Reset()
        {
            CurrentHealth = MaxHealth;
            AttackCount = 0;
            Debug.Log($"[MaskEnemy] {Name} 状态重置");
        }

        #endregion

        #region 面具相关

        /// <summary>
        /// 获取面具定义
        /// </summary>
        public MaskDefinition GetMaskDefinition()
        {
            return MaskRegistry.GetMask(MaskType);
        }

        #endregion

        #region 调试信息

        /// <summary>
        /// 获取状态字符串
        /// </summary>
        public string GetStatusString()
        {
            return $"{Name} [{MaskType}] - 血量: {CurrentHealth}/{MaxHealth}, 攻击力: {AttackPower}";
        }

        #endregion
    }
}

