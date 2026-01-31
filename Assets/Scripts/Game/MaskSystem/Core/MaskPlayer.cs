// ================================================
// MaskSystem - 玩家对象
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 面具系统玩家对象 - 独立于现有PlayerMdl
    /// </summary>
    public class MaskPlayer
    {
        #region 常量

        private const int BASE_HEALTH = 3;
        private const int HEALTH_PER_MASK = 1;
        private const int MAX_MASK_SLOTS = 3;

        #endregion

        #region 属性

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
        /// 当前佩戴的面具
        /// </summary>
        public MaskType CurrentMask { get; private set; }

        /// <summary>
        /// 当前面具槽位索引
        /// </summary>
        public int CurrentSlot { get; private set; }

        /// <summary>
        /// 拥有的面具列表
        /// </summary>
        public IReadOnlyList<MaskType> OwnedMasks => _ownedMasks;

        #endregion

        #region 私有字段

        private List<MaskType> _ownedMasks;
        private MaskSystemConfig _config;

        #endregion

        #region 事件

        /// <summary>
        /// 血量改变事件 (旧值, 新值)
        /// </summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>
        /// 面具改变事件 (旧面具, 新面具)
        /// </summary>
        public event Action<MaskType, MaskType> OnMaskChanged;

        /// <summary>
        /// 获得面具事件
        /// </summary>
        public event Action<MaskType> OnMaskAcquired;

        /// <summary>
        /// 死亡事件
        /// </summary>
        public event Action OnDeath;

        #endregion

        #region 构造函数

        public MaskPlayer(MaskSystemConfig config = null)
        {
            _config = config;
            _ownedMasks = new List<MaskType>();
            
            // 使用配置或默认值
            int baseHealth = config?.PlayerBaseHealth ?? BASE_HEALTH;
            MaskType initialMask = config?.InitialMask ?? MaskType.Horse;

            MaxHealth = baseHealth;
            CurrentHealth = MaxHealth;
            CurrentSlot = 0;

            // 添加初始面具
            AddMask(initialMask, silent: true);
            CurrentMask = initialMask;

            Debug.Log($"[MaskPlayer] 创建玩家 - 血量: {CurrentHealth}/{MaxHealth}, 初始面具: {CurrentMask}");
        }

        #endregion

        #region 面具操作

        /// <summary>
        /// 添加面具
        /// </summary>
        public bool AddMask(MaskType mask, bool silent = false)
        {
            if (mask == MaskType.None)
                return false;

            if (_ownedMasks.Contains(mask))
            {
                Debug.Log($"[MaskPlayer] 已拥有面具: {mask}");
                return false;
            }

            int maxSlots = _config?.MaxMaskSlots ?? MAX_MASK_SLOTS;
            if (_ownedMasks.Count >= maxSlots)
            {
                Debug.LogWarning($"[MaskPlayer] 面具槽位已满 ({maxSlots})");
                return false;
            }

            _ownedMasks.Add(mask);

            // 每有一个面具，最大血量+1
            int healthPerMask = _config?.HealthPerMask ?? HEALTH_PER_MASK;
            MaxHealth = (_config?.PlayerBaseHealth ?? BASE_HEALTH) + _ownedMasks.Count * healthPerMask;

            // 获得面具时回复1点血
            int oldHealth = CurrentHealth;
            CurrentHealth = Mathf.Min(CurrentHealth + 1, MaxHealth);

            if (!silent)
            {
                Debug.Log($"[MaskPlayer] 获得面具: {mask}, 最大血量: {MaxHealth}");
                OnMaskAcquired?.Invoke(mask);
                if (oldHealth != CurrentHealth)
                {
                    OnHealthChanged?.Invoke(oldHealth, CurrentHealth);
                }
            }

            return true;
        }

        /// <summary>
        /// 切换到指定槽位的面具 (Q=0, W=1, E=2)
        /// </summary>
        public bool SwitchToSlot(int slot)
        {
            if (slot < 0 || slot >= _ownedMasks.Count)
            {
                Debug.LogWarning($"[MaskPlayer] 无效的槽位: {slot}, 当前拥有 {_ownedMasks.Count} 个面具");
                return false;
            }

            MaskType newMask = _ownedMasks[slot];
            if (CurrentMask == newMask)
            {
                Debug.Log($"[MaskPlayer] 已经佩戴此面具: {newMask}");
                return false;
            }

            MaskType oldMask = CurrentMask;
            CurrentSlot = slot;
            CurrentMask = newMask;

            Debug.Log($"[MaskPlayer] 切换面具: {oldMask} -> {newMask} (槽位 {slot})");
            OnMaskChanged?.Invoke(oldMask, newMask);

            return true;
        }

        /// <summary>
        /// 获取当前面具的定义
        /// </summary>
        public MaskDefinition GetCurrentMaskDefinition()
        {
            return MaskRegistry.GetMask(CurrentMask);
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

            Debug.Log($"[MaskPlayer] 受到 {damage} 点伤害, 血量: {oldHealth} -> {CurrentHealth}");
            OnHealthChanged?.Invoke(oldHealth, CurrentHealth);

            if (!IsAlive)
            {
                Debug.Log("[MaskPlayer] 玩家死亡!");
                OnDeath?.Invoke();
            }
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
                Debug.Log($"[MaskPlayer] 恢复 {amount} 点血量, 血量: {oldHealth} -> {CurrentHealth}");
                OnHealthChanged?.Invoke(oldHealth, CurrentHealth);
            }
        }

        /// <summary>
        /// 执行攻击，返回伤害值
        /// </summary>
        public int PerformAttack(MaskType targetMask)
        {
            var maskDef = GetCurrentMaskDefinition();
            int baseDamage = maskDef?.AttackPower ?? 1;

            // 检查克制关系
            bool isCounter = MaskRegistry.IsCounter(CurrentMask, targetMask);
            float multiplier = isCounter ? (_config?.CounterDamageMultiplier ?? 2f) : 1f;
            int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

            Debug.Log($"[MaskPlayer] 攻击! 基础伤害: {baseDamage}, 克制: {isCounter}, 最终伤害: {finalDamage}");
            return finalDamage;
        }

        /// <summary>
        /// 重置玩家状态
        /// </summary>
        public void Reset()
        {
            CurrentHealth = MaxHealth;
            CurrentSlot = 0;
            if (_ownedMasks.Count > 0)
            {
                CurrentMask = _ownedMasks[0];
            }
            Debug.Log("[MaskPlayer] 状态重置");
        }

        #endregion

        #region 调试信息

        /// <summary>
        /// 获取状态字符串
        /// </summary>
        public string GetStatusString()
        {
            string masks = string.Join(", ", _ownedMasks);
            return $"血量: {CurrentHealth}/{MaxHealth} | 当前面具: {CurrentMask} | 拥有面具: [{masks}]";
        }

        #endregion
    }
}

