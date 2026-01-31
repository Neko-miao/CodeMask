// ================================================
// MaskSystem - 战斗管理器
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 战斗结果
    /// </summary>
    public struct CombatResult
    {
        public int Damage;
        public bool IsCounter;
        public bool TargetDefeated;
        public string Message;

        public override string ToString()
        {
            return $"伤害: {Damage}, 克制: {IsCounter}, 击败: {TargetDefeated} - {Message}";
        }
    }

    /// <summary>
    /// 战斗管理器 - 处理玩家与敌人之间的战斗逻辑
    /// </summary>
    public class CombatManager
    {
        #region 属性

        /// <summary>
        /// 玩家对象
        /// </summary>
        public MaskPlayer Player { get; private set; }

        /// <summary>
        /// 当前敌人
        /// </summary>
        public MaskEnemy CurrentEnemy { get; private set; }

        /// <summary>
        /// 战斗是否进行中
        /// </summary>
        public bool IsBattleActive => Player != null && Player.IsAlive && CurrentEnemy != null && CurrentEnemy.IsAlive;

        /// <summary>
        /// 系统配置
        /// </summary>
        public MaskSystemConfig Config { get; private set; }

        #endregion

        #region 事件

        /// <summary>
        /// 玩家攻击事件
        /// </summary>
        public event Action<CombatResult> OnPlayerAttack;

        /// <summary>
        /// 敌人攻击事件
        /// </summary>
        public event Action<CombatResult> OnEnemyAttack;

        /// <summary>
        /// 敌人被击败事件
        /// </summary>
        public event Action<MaskEnemy> OnEnemyDefeated;

        /// <summary>
        /// 玩家被击败事件
        /// </summary>
        public event Action OnPlayerDefeated;

        /// <summary>
        /// 新敌人生成事件
        /// </summary>
        public event Action<MaskEnemy> OnEnemySpawned;

        #endregion

        #region 构造函数

        public CombatManager(MaskSystemConfig config = null)
        {
            Config = config ?? MaskSystemConfig.CreateDefault();
            Player = new MaskPlayer(Config);

            // 监听玩家死亡
            Player.OnDeath += HandlePlayerDeath;

            Debug.Log("[CombatManager] 战斗管理器初始化完成");
        }

        #endregion

        #region 敌人管理

        /// <summary>
        /// 生成敌人
        /// </summary>
        public MaskEnemy SpawnEnemy(MaskType maskType, int health, int attackPower)
        {
            // 清理旧敌人事件
            if (CurrentEnemy != null)
            {
                CurrentEnemy.OnDeath -= HandleEnemyDeath;
            }

            var maskDef = MaskRegistry.GetMask(maskType);
            string name = maskDef?.Name ?? maskType.ToString();

            CurrentEnemy = new MaskEnemy(name, maskType, health, attackPower);
            CurrentEnemy.OnDeath += HandleEnemyDeath;

            Debug.Log($"[CombatManager] 生成敌人: {CurrentEnemy.GetStatusString()}");
            OnEnemySpawned?.Invoke(CurrentEnemy);

            return CurrentEnemy;
        }

        /// <summary>
        /// 从预设生成敌人
        /// </summary>
        public MaskEnemy SpawnEnemyFromPreset(int presetIndex)
        {
            var preset = Config?.GetEnemyPreset(presetIndex);
            if (preset == null)
            {
                Debug.LogWarning($"[CombatManager] 找不到预设索引: {presetIndex}");
                return null;
            }

            return SpawnEnemy(preset.MaskType, preset.Health, preset.AttackPower);
        }

        /// <summary>
        /// 从面具类型生成敌人
        /// </summary>
        public MaskEnemy SpawnEnemyByMaskType(MaskType maskType)
        {
            var preset = Config?.GetEnemyPreset(maskType);
            if (preset != null)
            {
                return SpawnEnemy(preset.MaskType, preset.Health, preset.AttackPower);
            }

            // 使用默认值
            var maskDef = MaskRegistry.GetMask(maskType);
            return SpawnEnemy(maskType, 3, maskDef?.AttackPower ?? 1);
        }

        /// <summary>
        /// 强制击败当前敌人（测试用）
        /// </summary>
        public void DefeatCurrentEnemy()
        {
            if (CurrentEnemy == null || !CurrentEnemy.IsAlive)
            {
                Debug.LogWarning("[CombatManager] 没有活着的敌人可击败");
                return;
            }

            // 直接造成致命伤害
            CurrentEnemy.TakeDamage(CurrentEnemy.CurrentHealth);
        }

        #endregion

        #region 战斗操作

        /// <summary>
        /// 玩家攻击敌人
        /// </summary>
        public CombatResult PlayerAttack()
        {
            var result = new CombatResult();

            if (Player == null || !Player.IsAlive)
            {
                result.Message = "玩家已死亡";
                Debug.LogWarning($"[CombatManager] {result.Message}");
                return result;
            }

            if (CurrentEnemy == null || !CurrentEnemy.IsAlive)
            {
                result.Message = "没有敌人可攻击";
                Debug.LogWarning($"[CombatManager] {result.Message}");
                return result;
            }

            // 计算伤害
            result.Damage = Player.PerformAttack(CurrentEnemy.MaskType);
            result.IsCounter = MaskRegistry.IsCounter(Player.CurrentMask, CurrentEnemy.MaskType);

            // 造成伤害
            CurrentEnemy.TakeDamage(result.Damage);
            result.TargetDefeated = !CurrentEnemy.IsAlive;

            result.Message = $"玩家使用 {Player.CurrentMask} 攻击 {CurrentEnemy.Name}";
            if (result.IsCounter)
            {
                result.Message += " (克制!)";
            }

            Debug.Log($"[CombatManager] {result}");
            OnPlayerAttack?.Invoke(result);

            return result;
        }

        /// <summary>
        /// 敌人攻击玩家
        /// </summary>
        public CombatResult EnemyAttack()
        {
            var result = new CombatResult();

            if (CurrentEnemy == null || !CurrentEnemy.IsAlive)
            {
                result.Message = "敌人已死亡";
                Debug.LogWarning($"[CombatManager] {result.Message}");
                return result;
            }

            if (Player == null || !Player.IsAlive)
            {
                result.Message = "玩家已死亡";
                Debug.LogWarning($"[CombatManager] {result.Message}");
                return result;
            }

            // 计算伤害
            result.Damage = CurrentEnemy.PerformAttack();
            result.IsCounter = MaskRegistry.IsCounter(CurrentEnemy.MaskType, Player.CurrentMask);
            
            if (result.IsCounter)
            {
                result.Damage = Mathf.RoundToInt(result.Damage * (Config?.CounterDamageMultiplier ?? 2f));
            }

            // 检查闪避
            var playerMaskDef = Player.GetCurrentMaskDefinition();
            if (playerMaskDef?.EffectType == MaskEffectType.Dodge)
            {
                result.Damage = Mathf.Max(0, result.Damage - 1);
                result.Message = $"{CurrentEnemy.Name} 攻击被闪避部分伤害";
            }
            else
            {
                result.Message = $"{CurrentEnemy.Name} 攻击玩家";
            }

            // 造成伤害
            Player.TakeDamage(result.Damage);
            result.TargetDefeated = !Player.IsAlive;

            if (result.IsCounter)
            {
                result.Message += " (克制!)";
            }

            Debug.Log($"[CombatManager] {result}");
            OnEnemyAttack?.Invoke(result);

            return result;
        }

        #endregion

        #region 面具操作

        /// <summary>
        /// 切换面具
        /// </summary>
        public bool SwitchMask(int slot)
        {
            return Player?.SwitchToSlot(slot) ?? false;
        }

        #endregion

        #region 事件处理

        private void HandleEnemyDeath()
        {
            if (CurrentEnemy == null) return;

            Debug.Log($"[CombatManager] 敌人 {CurrentEnemy.Name} 被击败，玩家获得面具: {CurrentEnemy.MaskType}");
            
            // 玩家获得敌人的面具
            Player.AddMask(CurrentEnemy.MaskType);

            OnEnemyDefeated?.Invoke(CurrentEnemy);
        }

        private void HandlePlayerDeath()
        {
            Debug.Log("[CombatManager] 玩家被击败!");
            OnPlayerDefeated?.Invoke();
        }

        #endregion

        #region 状态查询

        /// <summary>
        /// 获取战斗状态摘要
        /// </summary>
        public string GetBattleStatus()
        {
            string playerStatus = Player?.GetStatusString() ?? "无玩家";
            string enemyStatus = CurrentEnemy?.GetStatusString() ?? "无敌人";
            return $"=== 战斗状态 ===\n玩家: {playerStatus}\n敌人: {enemyStatus}";
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            if (Player != null)
            {
                Player.OnDeath -= HandlePlayerDeath;
            }

            if (CurrentEnemy != null)
            {
                CurrentEnemy.OnDeath -= HandleEnemyDeath;
            }

            Player = null;
            CurrentEnemy = null;

            Debug.Log("[CombatManager] 已清理");
        }

        #endregion
    }
}

