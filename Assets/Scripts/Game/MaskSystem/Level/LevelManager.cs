// ================================================
// MaskSystem - 关卡管理器
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 关卡状态
    /// </summary>
    public enum LevelState
    {
        None,       // 未开始
        Preparing,  // 准备中
        Playing,    // 进行中
        WaveTransition, // 波次切换中
        Victory,    // 胜利
        Defeat      // 失败
    }

    /// <summary>
    /// 关卡管理器 - 控制整个关卡流程
    /// </summary>
    public class LevelManager
    {
        #region 属性

        /// <summary>
        /// 当前关卡状态
        /// </summary>
        public LevelState State { get; private set; }

        /// <summary>
        /// 当前关卡配置
        /// </summary>
        public LevelConfig CurrentLevel { get; private set; }

        /// <summary>
        /// 当前波次索引
        /// </summary>
        public int CurrentWaveIndex { get; private set; }

        /// <summary>
        /// 当前波次配置
        /// </summary>
        public WaveConfig CurrentWave => CurrentLevel?.GetWave(CurrentWaveIndex);

        /// <summary>
        /// 是否是最后一波
        /// </summary>
        public bool IsLastWave => CurrentLevel != null && CurrentWaveIndex >= CurrentLevel.WaveCount - 1;

        /// <summary>
        /// 总波次数
        /// </summary>
        public int TotalWaves => CurrentLevel?.WaveCount ?? 0;

        /// <summary>
        /// 准备/切换倒计时
        /// </summary>
        public float TransitionTimer { get; private set; }

        /// <summary>
        /// 自动战斗控制器
        /// </summary>
        public AutoBattleController AutoBattle { get; private set; }

        /// <summary>
        /// API接口
        /// </summary>
        public IMaskSystemAPI API { get; private set; }

        #endregion

        #region 事件

        /// <summary>
        /// 关卡状态改变
        /// </summary>
        public event Action<LevelState, LevelState> OnStateChanged;

        /// <summary>
        /// 波次开始
        /// </summary>
        public event Action<int, WaveConfig> OnWaveStart;

        /// <summary>
        /// 波次完成
        /// </summary>
        public event Action<int> OnWaveComplete;

        /// <summary>
        /// 关卡胜利
        /// </summary>
        public event Action OnLevelVictory;

        /// <summary>
        /// 关卡失败
        /// </summary>
        public event Action OnLevelDefeat;

        /// <summary>
        /// 准备/切换倒计时更新
        /// </summary>
        public event Action<float> OnTransitionTimerUpdate;

        #endregion

        #region 构造函数

        public LevelManager(IMaskSystemAPI api = null)
        {
            API = api ?? MaskSystemFacade.Instance;
            AutoBattle = new AutoBattleController(API);

            // 订阅事件
            API.OnEnemyDefeated += HandleEnemyDefeated;
            API.OnPlayerDefeated += HandlePlayerDefeated;

            State = LevelState.None;
            Debug.Log("[LevelManager] 初始化完成");
        }

        #endregion

        #region 关卡控制

        /// <summary>
        /// 加载关卡
        /// </summary>
        public void LoadLevel(LevelConfig level)
        {
            if (level == null)
            {
                Debug.LogError("[LevelManager] 关卡配置为空!");
                return;
            }

            CurrentLevel = level;
            CurrentWaveIndex = 0;

            Debug.Log($"[LevelManager] 加载关卡: {level.LevelName}, 波次数: {level.WaveCount}");
        }

        /// <summary>
        /// 开始关卡
        /// </summary>
        public void StartLevel()
        {
            if (CurrentLevel == null)
            {
                Debug.LogError("[LevelManager] 请先加载关卡!");
                return;
            }

            // 重置玩家
            API.ResetPlayer();

            // 进入准备阶段
            TransitionTimer = CurrentLevel.PrepareTime;
            ChangeState(LevelState.Preparing);

            Debug.Log($"[LevelManager] 开始关卡: {CurrentLevel.LevelName}");
        }

        /// <summary>
        /// 快速开始（加载并开始）
        /// </summary>
        public void QuickStart(LevelConfig level)
        {
            LoadLevel(level);
            StartLevel();
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            AutoBattle?.Pause();
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            AutoBattle?.Resume();
        }

        /// <summary>
        /// 重新开始当前关卡
        /// </summary>
        public void Restart()
        {
            AutoBattle?.Stop();
            CurrentWaveIndex = 0;
            StartLevel();
        }

        /// <summary>
        /// 结束关卡
        /// </summary>
        public void EndLevel()
        {
            AutoBattle?.Stop();
            ChangeState(LevelState.None);
        }

        #endregion

        #region 更新

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Update(float deltaTime)
        {
            switch (State)
            {
                case LevelState.Preparing:
                    UpdatePreparing(deltaTime);
                    break;

                case LevelState.Playing:
                    UpdatePlaying(deltaTime);
                    break;

                case LevelState.WaveTransition:
                    UpdateWaveTransition(deltaTime);
                    break;
            }
        }

        private void UpdatePreparing(float deltaTime)
        {
            TransitionTimer -= deltaTime;
            OnTransitionTimerUpdate?.Invoke(TransitionTimer);

            if (TransitionTimer <= 0)
            {
                // 准备完成，开始第一波
                StartCurrentWave();
            }
        }

        private void UpdatePlaying(float deltaTime)
        {
            AutoBattle?.Update(deltaTime);
        }

        private void UpdateWaveTransition(float deltaTime)
        {
            TransitionTimer -= deltaTime;
            OnTransitionTimerUpdate?.Invoke(TransitionTimer);

            if (TransitionTimer <= 0)
            {
                // 切换完成，开始下一波
                StartCurrentWave();
            }
        }

        #endregion

        #region 波次控制

        /// <summary>
        /// 开始当前波次
        /// </summary>
        private void StartCurrentWave()
        {
            var wave = CurrentWave;
            if (wave == null)
            {
                Debug.LogError($"[LevelManager] 波次配置为空: {CurrentWaveIndex}");
                return;
            }

            ChangeState(LevelState.Playing);
            AutoBattle.StartWave(wave);
            OnWaveStart?.Invoke(CurrentWaveIndex, wave);

            Debug.Log($"[LevelManager] 开始波次 {CurrentWaveIndex + 1}/{TotalWaves}: {wave.GetDisplayName()}");
        }

        /// <summary>
        /// 进入下一波
        /// </summary>
        private void GoToNextWave()
        {
            OnWaveComplete?.Invoke(CurrentWaveIndex);

            if (IsLastWave)
            {
                // 所有波次完成，胜利
                HandleVictory();
            }
            else
            {
                // 进入下一波
                CurrentWaveIndex++;
                TransitionTimer = CurrentLevel.WaveInterval;
                ChangeState(LevelState.WaveTransition);

                Debug.Log($"[LevelManager] 波次切换中... 下一波: {CurrentWaveIndex + 1}");
            }
        }

        #endregion

        #region 玩家输入

        /// <summary>
        /// 玩家尝试反击（Space按键）
        /// </summary>
        public bool TryCounter()
        {
            if (State != LevelState.Playing) return false;
            return AutoBattle?.TryPlayerCounter() ?? false;
        }

        /// <summary>
        /// 玩家切换面具
        /// </summary>
        public bool SwitchMask(int slot)
        {
            return API?.SwitchMask(slot) ?? false;
        }

        #endregion

        #region 事件处理

        private void HandleEnemyDefeated()
        {
            if (State != LevelState.Playing) return;

            Debug.Log("[LevelManager] 敌人被击败!");
            AutoBattle?.Stop();
            GoToNextWave();
        }

        private void HandlePlayerDefeated()
        {
            if (State == LevelState.Defeat || State == LevelState.Victory) return;

            Debug.Log("[LevelManager] 玩家被击败!");
            AutoBattle?.Stop();
            HandleDefeat();
        }

        private void HandleVictory()
        {
            ChangeState(LevelState.Victory);
            OnLevelVictory?.Invoke();
            Debug.Log($"[LevelManager] 关卡胜利! {CurrentLevel?.LevelName}");
        }

        private void HandleDefeat()
        {
            ChangeState(LevelState.Defeat);
            OnLevelDefeat?.Invoke();
            Debug.Log($"[LevelManager] 关卡失败! {CurrentLevel?.LevelName}");
        }

        #endregion

        #region 辅助方法

        private void ChangeState(LevelState newState)
        {
            if (State == newState) return;

            var oldState = State;
            State = newState;
            OnStateChanged?.Invoke(oldState, newState);

            Debug.Log($"[LevelManager] 状态: {oldState} -> {newState}");
        }

        /// <summary>
        /// 获取状态摘要
        /// </summary>
        public string GetStatusString()
        {
            string status = $"关卡: {CurrentLevel?.LevelName ?? "无"} | 状态: {State} | 波次: {CurrentWaveIndex + 1}/{TotalWaves}";

            if (State == LevelState.Preparing || State == LevelState.WaveTransition)
            {
                status += $" | 倒计时: {TransitionTimer:F1}秒";
            }

            if (AutoBattle != null && State == LevelState.Playing)
            {
                status += $"\n{AutoBattle.GetStatusString()}";
            }

            return status;
        }

        #endregion

        #region 清理

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            AutoBattle?.Stop();

            if (API != null)
            {
                API.OnEnemyDefeated -= HandleEnemyDefeated;
                API.OnPlayerDefeated -= HandlePlayerDefeated;
            }

            Debug.Log("[LevelManager] 已清理");
        }

        #endregion
    }
}

