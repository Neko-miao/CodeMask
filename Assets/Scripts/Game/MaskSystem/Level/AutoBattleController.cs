// ================================================
// MaskSystem - 自动战斗控制器
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 战斗阶段
    /// </summary>
    public enum BattlePhase
    {
        Idle,           // 空闲（等待下次攻击）
        Warning,        // 预警中（玩家可反击）
        EnemyAttacking, // 敌人攻击中
        PlayerCounter,  // 玩家反击中
        Cooldown        // 冷却中
    }

    /// <summary>
    /// 自动战斗控制器 - 控制敌人自动攻击和玩家反击
    /// </summary>
    public class AutoBattleController
    {
        #region 属性

        /// <summary>
        /// 当前战斗阶段
        /// </summary>
        public BattlePhase CurrentPhase { get; private set; }

        /// <summary>
        /// 当前波次配置
        /// </summary>
        public WaveConfig CurrentWave { get; private set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// 下次攻击倒计时
        /// </summary>
        public float NextAttackTimer { get; private set; }

        /// <summary>
        /// 预警剩余时间
        /// </summary>
        public float WarningTimer { get; private set; }

        /// <summary>
        /// 预警进度 (0-1)
        /// </summary>
        public float WarningProgress => CurrentWave != null && CurrentWave.AttackWarningTime > 0
            ? 1f - (WarningTimer / CurrentWave.AttackWarningTime)
            : 0f;

        /// <summary>
        /// 是否在反击窗口内
        /// </summary>
        public bool IsInCounterWindow => CurrentPhase == BattlePhase.Warning;

        #endregion

        #region 私有字段

        private IMaskSystemAPI _api;
        private float _cooldownTimer;
        private const float COOLDOWN_TIME = 0.5f;

        #endregion

        #region 事件

        /// <summary>
        /// 攻击预警开始
        /// </summary>
        public event Action OnWarningStart;

        /// <summary>
        /// 攻击预警更新 (剩余时间)
        /// </summary>
        public event Action<float> OnWarningUpdate;

        /// <summary>
        /// 敌人攻击
        /// </summary>
        public event Action<CombatResult> OnEnemyAttack;

        /// <summary>
        /// 玩家反击成功
        /// </summary>
        public event Action<CombatResult> OnPlayerCounter;

        /// <summary>
        /// 玩家反击失败（时间窗口外）
        /// </summary>
        public event Action OnCounterFailed;

        /// <summary>
        /// 阶段改变
        /// </summary>
        public event Action<BattlePhase, BattlePhase> OnPhaseChanged;

        #endregion

        #region 构造函数

        public AutoBattleController(IMaskSystemAPI api)
        {
            _api = api;
            CurrentPhase = BattlePhase.Idle;
            IsRunning = false;
            IsPaused = false;
        }

        #endregion

        #region 控制方法

        /// <summary>
        /// 设置当前波次配置并开始
        /// </summary>
        public void StartWave(WaveConfig wave)
        {
            CurrentWave = wave;
            IsRunning = true;
            IsPaused = false;

            // 生成敌人
            _api.SpawnEnemy(wave.EnemyType, wave.EnemyHealth, wave.EnemyAttackPower);

            // 设置下次攻击时间
            ResetAttackTimer();
            ChangePhase(BattlePhase.Idle);

            Debug.Log($"[AutoBattle] 开始波次: {wave.GetDisplayName()}");
        }

        /// <summary>
        /// 停止战斗
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            IsPaused = false;
            ChangePhase(BattlePhase.Idle);
            Debug.Log("[AutoBattle] 停止战斗");
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            IsPaused = true;
            Debug.Log("[AutoBattle] 暂停");
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
            Debug.Log("[AutoBattle] 恢复");
        }

        #endregion

        #region 更新

        /// <summary>
        /// 每帧更新（需要在MonoBehaviour的Update中调用）
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsRunning || IsPaused) return;

            // 检查战斗是否结束
            if (!_api.IsPlayerAlive || !_api.IsEnemyAlive)
            {
                Stop();
                return;
            }

            switch (CurrentPhase)
            {
                case BattlePhase.Idle:
                    UpdateIdlePhase(deltaTime);
                    break;

                case BattlePhase.Warning:
                    UpdateWarningPhase(deltaTime);
                    break;

                case BattlePhase.Cooldown:
                    UpdateCooldownPhase(deltaTime);
                    break;
            }
        }

        private void UpdateIdlePhase(float deltaTime)
        {
            NextAttackTimer -= deltaTime;

            if (NextAttackTimer <= 0)
            {
                // 进入预警阶段
                StartWarning();
            }
        }

        private void UpdateWarningPhase(float deltaTime)
        {
            WarningTimer -= deltaTime;
            OnWarningUpdate?.Invoke(WarningTimer);

            if (WarningTimer <= 0)
            {
                // 预警结束，敌人攻击
                ExecuteEnemyAttack();
            }
        }

        private void UpdateCooldownPhase(float deltaTime)
        {
            _cooldownTimer -= deltaTime;

            if (_cooldownTimer <= 0)
            {
                // 冷却结束，重置攻击计时器
                ResetAttackTimer();
                ChangePhase(BattlePhase.Idle);
            }
        }

        #endregion

        #region 战斗逻辑

        /// <summary>
        /// 开始攻击预警
        /// </summary>
        private void StartWarning()
        {
            WarningTimer = CurrentWave?.AttackWarningTime ?? 0.8f;
            ChangePhase(BattlePhase.Warning);
            OnWarningStart?.Invoke();
            Debug.Log($"[AutoBattle] 攻击预警! 反击窗口: {WarningTimer}秒");
        }

        /// <summary>
        /// 执行敌人攻击
        /// </summary>
        private void ExecuteEnemyAttack()
        {
            ChangePhase(BattlePhase.EnemyAttacking);

            var result = _api.EnemyAttack();
            OnEnemyAttack?.Invoke(result);

            Debug.Log($"[AutoBattle] 敌人攻击! 伤害: {result.Damage}");

            // 进入冷却
            StartCooldown();
        }

        /// <summary>
        /// 玩家尝试反击
        /// </summary>
        public bool TryPlayerCounter()
        {
            if (!IsRunning || IsPaused) return false;

            // 检查是否在反击窗口内
            if (CurrentPhase != BattlePhase.Warning)
            {
                Debug.Log("[AutoBattle] 反击失败 - 不在反击窗口内");
                OnCounterFailed?.Invoke();
                return false;
            }

            // 执行玩家反击
            ChangePhase(BattlePhase.PlayerCounter);

            var result = _api.PlayerAttack();
            OnPlayerCounter?.Invoke(result);

            Debug.Log($"[AutoBattle] 玩家反击成功! 伤害: {result.Damage}");

            // 进入冷却
            StartCooldown();

            return true;
        }

        /// <summary>
        /// 玩家主动攻击（非反击时）
        /// </summary>
        public CombatResult PlayerAttack()
        {
            if (!IsRunning || IsPaused)
            {
                return new CombatResult { Message = "战斗未开始或已暂停" };
            }

            // 如果在预警阶段，视为反击
            if (CurrentPhase == BattlePhase.Warning)
            {
                TryPlayerCounter();
                return new CombatResult { Message = "触发反击" };
            }

            // 普通攻击
            return _api.PlayerAttack();
        }

        /// <summary>
        /// 开始冷却
        /// </summary>
        private void StartCooldown()
        {
            _cooldownTimer = COOLDOWN_TIME;
            ChangePhase(BattlePhase.Cooldown);
        }

        /// <summary>
        /// 重置攻击计时器
        /// </summary>
        private void ResetAttackTimer()
        {
            NextAttackTimer = CurrentWave?.GetRandomAttackInterval() ?? 2f;
            Debug.Log($"[AutoBattle] 下次攻击倒计时: {NextAttackTimer:F1}秒");
        }

        #endregion

        #region 辅助方法

        private void ChangePhase(BattlePhase newPhase)
        {
            if (CurrentPhase == newPhase) return;

            var oldPhase = CurrentPhase;
            CurrentPhase = newPhase;
            OnPhaseChanged?.Invoke(oldPhase, newPhase);
        }

        /// <summary>
        /// 获取状态字符串
        /// </summary>
        public string GetStatusString()
        {
            string status = $"阶段: {CurrentPhase}";

            switch (CurrentPhase)
            {
                case BattlePhase.Idle:
                    status += $" | 下次攻击: {NextAttackTimer:F1}秒";
                    break;
                case BattlePhase.Warning:
                    status += $" | 反击窗口: {WarningTimer:F1}秒 ({WarningProgress * 100:F0}%)";
                    break;
                case BattlePhase.Cooldown:
                    status += $" | 冷却: {_cooldownTimer:F1}秒";
                    break;
            }

            return status;
        }

        #endregion
    }
}

