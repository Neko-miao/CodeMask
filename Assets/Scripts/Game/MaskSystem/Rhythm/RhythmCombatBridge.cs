// ================================================
// MaskSystem - 节奏战斗桥接
// 连接节奏判定结果与战斗系统
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem.Rhythm
{
    /// <summary>
    /// 战斗效果结果
    /// </summary>
    public struct CombatEffectResult
    {
        public JudgeResult JudgeResult;
        public int PlayerDamage;
        public int EnemyDamage;
        public bool PlayerHealed;
        public bool Blocked;
        public bool CounterAttack;
        public bool Dodged;
        public string Message;

        public override string ToString()
        {
            return $"[{JudgeResult}] 玩家伤害:{PlayerDamage} 敌人伤害:{EnemyDamage} {Message}";
        }
    }

    /// <summary>
    /// 节奏战斗桥接 - 将节奏判定转换为战斗效果
    /// </summary>
    public class RhythmCombatBridge
    {
        #region 配置

        /// <summary>
        /// 关联的战斗管理器
        /// </summary>
        public CombatManager Combat { get; private set; }

        /// <summary>
        /// 关联的节奏管理器
        /// </summary>
        public RhythmManager Rhythm { get; private set; }

        /// <summary>
        /// 格挡反击伤害倍率
        /// </summary>
        public float BlockCounterMultiplier { get; set; } = 2f;

        /// <summary>
        /// 是否启用首次切换面具的额外效果
        /// </summary>
        public bool EnableFirstSwitchBonus { get; set; } = true;

        #endregion

        #region 状态

        /// <summary>
        /// 是否正在格挡反击状态
        /// </summary>
        public bool IsBlockCounterReady { get; private set; }

        /// <summary>
        /// 连续完美计数（用于触发攻击）
        /// </summary>
        public int PerfectCount { get; private set; }

        /// <summary>
        /// 上次切换的面具（用于首次切换奖励）
        /// </summary>
        private MaskType _lastSwitchedMask = MaskType.None;

        /// <summary>
        /// 本场战斗中已使用的首次切换面具
        /// </summary>
        private System.Collections.Generic.HashSet<MaskType> _usedFirstSwitchMasks = new System.Collections.Generic.HashSet<MaskType>();

        #endregion

        #region 事件

        /// <summary>
        /// 战斗效果触发事件
        /// </summary>
        public event Action<CombatEffectResult> OnCombatEffect;

        /// <summary>
        /// 三连完美攻击触发事件
        /// </summary>
        public event Action<int> OnTriplePerfectAttack; // 参数为造成的伤害

        /// <summary>
        /// 格挡反击准备事件
        /// </summary>
        public event Action OnBlockCounterReady;

        /// <summary>
        /// 首次切换面具奖励事件
        /// </summary>
        public event Action<MaskType, string> OnFirstSwitchBonus; // 面具类型和奖励描述

        #endregion

        #region 初始化

        public RhythmCombatBridge(CombatManager combat, RhythmManager rhythm)
        {
            Combat = combat;
            Rhythm = rhythm;

            // 绑定节奏判定事件
            rhythm.OnJudge += HandleRhythmJudge;
            rhythm.OnPerfectTripleAttack += HandleTriplePerfectAttack;

            Debug.Log("[RhythmCombatBridge] 桥接初始化完成");
        }

        public void Dispose()
        {
            if (Rhythm != null)
            {
                Rhythm.OnJudge -= HandleRhythmJudge;
                Rhythm.OnPerfectTripleAttack -= HandleTriplePerfectAttack;
            }
        }

        /// <summary>
        /// 重置战斗状态
        /// </summary>
        public void ResetBattleState()
        {
            IsBlockCounterReady = false;
            PerfectCount = 0;
            _usedFirstSwitchMasks.Clear();
            _lastSwitchedMask = MaskType.None;
        }

        #endregion

        #region 判定处理

        private void HandleRhythmJudge(JudgeResultData judgeResult)
        {
            if (Combat == null || Combat.Player == null || Combat.CurrentEnemy == null)
            {
                Debug.LogWarning("[RhythmCombatBridge] 战斗未初始化");
                return;
            }

            // 处理面具切换
            if (judgeResult.WasMaskSwitch && judgeResult.MaskSlot >= 0)
            {
                HandleMaskSwitch(judgeResult.MaskSlot);
            }

            // 根据判定结果处理战斗效果
            var effectResult = ProcessJudgeResult(judgeResult);
            
            // 触发事件
            OnCombatEffect?.Invoke(effectResult);
        }

        private CombatEffectResult ProcessJudgeResult(JudgeResultData judgeResult)
        {
            var result = new CombatEffectResult
            {
                JudgeResult = judgeResult.Result
            };

            var player = Combat.Player;
            var enemy = Combat.CurrentEnemy;
            var playerMask = MaskRegistry.GetMask(player.CurrentMask);
            var noteType = judgeResult.Note.Type;

            switch (judgeResult.Result)
            {
                case JudgeResult.Perfect:
                    result = ProcessPerfectHit(judgeResult, playerMask, noteType);
                    PerfectCount++;
                    break;

                case JudgeResult.Normal:
                    result = ProcessNormalHit(judgeResult, playerMask, noteType);
                    PerfectCount = 0; // 重置完美连击
                    break;

                case JudgeResult.Miss:
                    result = ProcessMiss(judgeResult, noteType);
                    PerfectCount = 0;
                    IsBlockCounterReady = false; // 失误取消格挡反击
                    break;
            }

            return result;
        }

        /// <summary>
        /// 处理完美卡点
        /// </summary>
        private CombatEffectResult ProcessPerfectHit(JudgeResultData judgeResult, MaskDefinition playerMask, NoteType noteType)
        {
            var result = new CombatEffectResult { JudgeResult = JudgeResult.Perfect };
            var player = Combat.Player;
            var enemy = Combat.CurrentEnemy;

            // 检查是否在格挡反击状态
            if (IsBlockCounterReady)
            {
                // 格挡反击！
                int counterDamage = Mathf.RoundToInt(player.PerformAttack(enemy.MaskType) * BlockCounterMultiplier);
                enemy.TakeDamage(counterDamage);
                result.EnemyDamage = counterDamage;
                result.CounterAttack = true;
                result.Message = $"格挡反击! 造成 {counterDamage} 点伤害!";
                IsBlockCounterReady = false;
                return result;
            }

            switch (noteType)
            {
                case NoteType.Attack:
                case NoteType.Rage:
                    // 完美卡点：敌人受到伤害
                    if (playerMask?.EffectType == MaskEffectType.Attack)
                    {
                        // 攻击型面具：检查克制
                        bool isCounter = MaskRegistry.IsCounter(player.CurrentMask, enemy.MaskType);
                        int damage = player.PerformAttack(enemy.MaskType);
                        if (isCounter)
                        {
                            damage *= 2;
                            result.Message = $"克制攻击! 造成 {damage} 点伤害!";
                        }
                        else
                        {
                            result.Message = $"完美攻击! 造成 {damage} 点伤害!";
                        }
                        enemy.TakeDamage(damage);
                        result.EnemyDamage = damage;
                    }
                    else if (playerMask?.EffectType == MaskEffectType.Dodge)
                    {
                        // 闪避型面具：完美闪避狂暴攻击
                        result.Dodged = true;
                        result.Message = "完美闪避!";
                        // 同时反击
                        int damage = 1;
                        enemy.TakeDamage(damage);
                        result.EnemyDamage = damage;
                    }
                    else if (playerMask?.EffectType == MaskEffectType.Heal)
                    {
                        // 回血型面具：完美卡点回复1点体力
                        player.Heal(1);
                        result.PlayerHealed = true;
                        result.Message = "完美回复! +1 体力!";
                    }
                    else
                    {
                        // 默认：造成伤害
                        int damage = player.PerformAttack(enemy.MaskType);
                        enemy.TakeDamage(damage);
                        result.EnemyDamage = damage;
                        result.Message = $"完美! 造成 {damage} 点伤害!";
                    }
                    break;

                case NoteType.Defense:
                    // 防御节点完美卡点：进入格挡反击状态
                    if (playerMask?.EffectType == MaskEffectType.Block)
                    {
                        IsBlockCounterReady = true;
                        result.Blocked = true;
                        result.Message = "完美格挡! 下次攻击将反击!";
                        OnBlockCounterReady?.Invoke();
                    }
                    else
                    {
                        result.Blocked = true;
                        result.Message = "完美格挡!";
                    }
                    break;

                case NoteType.Idle:
                    result.Message = "休息";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 处理普通卡点
        /// </summary>
        private CombatEffectResult ProcessNormalHit(JudgeResultData judgeResult, MaskDefinition playerMask, NoteType noteType)
        {
            var result = new CombatEffectResult { JudgeResult = JudgeResult.Normal };
            var player = Combat.Player;
            var enemy = Combat.CurrentEnemy;

            switch (noteType)
            {
                case NoteType.Attack:
                    // 普通卡点：双方都受伤
                    if (playerMask?.EffectType == MaskEffectType.Dodge)
                    {
                        // 闪避型：不受攻击伤害
                        result.Dodged = true;
                        int damage = 1;
                        enemy.TakeDamage(damage);
                        result.EnemyDamage = damage;
                        result.Message = "闪避! 反击造成伤害!";
                    }
                    else
                    {
                        // 双方互伤
                        int playerDamage = judgeResult.Note.Damage;
                        int enemyDamage = 1;
                        player.TakeDamage(playerDamage);
                        enemy.TakeDamage(enemyDamage);
                        result.PlayerDamage = playerDamage;
                        result.EnemyDamage = enemyDamage;
                        result.Message = $"交换伤害! 你-{playerDamage}, 敌-{enemyDamage}";
                    }
                    break;

                case NoteType.Rage:
                    // 狂暴攻击普通卡点：玩家受到更多伤害
                    if (playerMask?.EffectType == MaskEffectType.Dodge)
                    {
                        // 闪避型无法完全闪避狂暴
                        int damage = 1;
                        player.TakeDamage(damage);
                        result.PlayerDamage = damage;
                        result.Message = "狂暴攻击! 闪避减伤!";
                    }
                    else
                    {
                        int damage = judgeResult.Note.Damage;
                        player.TakeDamage(damage);
                        result.PlayerDamage = damage;
                        result.Message = $"狂暴攻击! 受到 {damage} 点伤害!";
                    }
                    break;

                case NoteType.Defense:
                    // 格挡
                    result.Blocked = true;
                    result.Message = "格挡成功";
                    break;

                case NoteType.Idle:
                    result.Message = "休息";
                    break;
            }

            return result;
        }

        /// <summary>
        /// 处理失误
        /// </summary>
        private CombatEffectResult ProcessMiss(JudgeResultData judgeResult, NoteType noteType)
        {
            var result = new CombatEffectResult { JudgeResult = JudgeResult.Miss };
            var player = Combat.Player;

            switch (noteType)
            {
                case NoteType.Attack:
                case NoteType.Rage:
                    // Miss：玩家受伤
                    int damage = judgeResult.Note.Damage;
                    if (noteType == NoteType.Rage) damage += 1; // 狂暴额外伤害
                    player.TakeDamage(damage);
                    result.PlayerDamage = damage;
                    result.Message = $"失误! 受到 {damage} 点伤害!";
                    break;

                case NoteType.Defense:
                    // 防御失误也受伤
                    int defDamage = 1;
                    player.TakeDamage(defDamage);
                    result.PlayerDamage = defDamage;
                    result.Message = "防御失败! 受到伤害!";
                    break;

                case NoteType.Idle:
                    result.Message = "失误";
                    break;
            }

            return result;
        }

        #endregion

        #region 三连完美攻击

        private void HandleTriplePerfectAttack()
        {
            if (Combat == null || Combat.CurrentEnemy == null || !Combat.CurrentEnemy.IsAlive)
                return;

            // 随机选择一只跟随的面具动物执行攻击
            var ownedMasks = Combat.Player.OwnedMasks;
            if (ownedMasks.Count == 0) return;

            int randomIndex = UnityEngine.Random.Range(0, ownedMasks.Count);
            var attackingMask = ownedMasks[randomIndex];
            var maskDef = MaskRegistry.GetMask(attackingMask);

            // 计算伤害（基础伤害 + 面具攻击力）
            int damage = 2 + (maskDef?.AttackPower ?? 1);

            // 检查克制
            if (MaskRegistry.IsCounter(attackingMask, Combat.CurrentEnemy.MaskType))
            {
                damage *= 2;
                Debug.Log($"[RhythmCombatBridge] 三连完美攻击克制! {attackingMask} -> {Combat.CurrentEnemy.MaskType}");
            }

            Combat.CurrentEnemy.TakeDamage(damage);

            Debug.Log($"[RhythmCombatBridge] 三连完美! {maskDef?.Name ?? attackingMask.ToString()} 攻击造成 {damage} 伤害!");
            OnTriplePerfectAttack?.Invoke(damage);
        }

        #endregion

        #region 面具切换

        private void HandleMaskSwitch(int slot)
        {
            if (!Combat.SwitchMask(slot))
                return;

            var newMask = Combat.Player.CurrentMask;
            
            // 检查首次切换奖励
            if (EnableFirstSwitchBonus && !_usedFirstSwitchMasks.Contains(newMask))
            {
                _usedFirstSwitchMasks.Add(newMask);
                ApplyFirstSwitchBonus(newMask);
            }

            _lastSwitchedMask = newMask;
            Debug.Log($"[RhythmCombatBridge] 切换面具到: {newMask}");
        }

        private void ApplyFirstSwitchBonus(MaskType mask)
        {
            var maskDef = MaskRegistry.GetMask(mask);
            string bonus = "";

            // 根据面具类型给予不同的首次切换奖励
            switch (maskDef?.EffectType)
            {
                case MaskEffectType.Attack:
                    // 攻击型：立即对敌人造成1点伤害
                    if (Combat.CurrentEnemy != null && Combat.CurrentEnemy.IsAlive)
                    {
                        Combat.CurrentEnemy.TakeDamage(1);
                        bonus = "首次切换奖励: 对敌人造成1点伤害!";
                    }
                    break;

                case MaskEffectType.Dodge:
                    // 闪避型：下一次攻击无效
                    bonus = "首次切换奖励: 获得闪避状态!";
                    break;

                case MaskEffectType.Block:
                    // 格挡型：立即进入格挡反击状态
                    IsBlockCounterReady = true;
                    bonus = "首次切换奖励: 获得格挡反击状态!";
                    OnBlockCounterReady?.Invoke();
                    break;

                case MaskEffectType.Heal:
                    // 回血型：立即回复1点体力
                    Combat.Player.Heal(1);
                    bonus = "首次切换奖励: 回复1点体力!";
                    break;
            }

            if (!string.IsNullOrEmpty(bonus))
            {
                Debug.Log($"[RhythmCombatBridge] {bonus}");
                OnFirstSwitchBonus?.Invoke(mask, bonus);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取当前面具对敌人的效果描述
        /// </summary>
        public string GetMaskEffectDescription()
        {
            if (Combat?.Player == null || Combat?.CurrentEnemy == null)
                return "";

            var playerMask = Combat.Player.CurrentMask;
            var enemyMask = Combat.CurrentEnemy.MaskType;
            var maskDef = MaskRegistry.GetMask(playerMask);

            string desc = $"{maskDef?.Name ?? playerMask.ToString()}: ";

            switch (maskDef?.EffectType)
            {
                case MaskEffectType.Attack:
                    bool isCounter = MaskRegistry.IsCounter(playerMask, enemyMask);
                    desc += isCounter ? "克制敌人! 双倍伤害" : "普通攻击";
                    break;
                case MaskEffectType.Dodge:
                    desc += "闪避型 - 普通卡点免疫攻击";
                    break;
                case MaskEffectType.Block:
                    desc += "格挡型 - 完美格挡后反击";
                    break;
                case MaskEffectType.Heal:
                    desc += "回血型 - 完美卡点回复体力";
                    break;
            }

            return desc;
        }

        /// <summary>
        /// 获取推荐的面具
        /// </summary>
        public MaskType GetRecommendedMask()
        {
            if (Combat?.CurrentEnemy == null) return MaskType.None;

            var enemyMask = Combat.CurrentEnemy.MaskType;
            var ownedMasks = Combat.Player.OwnedMasks;

            // 寻找克制敌人的面具
            foreach (var mask in ownedMasks)
            {
                if (MaskRegistry.IsCounter(mask, enemyMask))
                    return mask;
            }

            // 没有克制的就返回第一个
            return ownedMasks.Count > 0 ? ownedMasks[0] : MaskType.None;
        }

        #endregion
    }
}

