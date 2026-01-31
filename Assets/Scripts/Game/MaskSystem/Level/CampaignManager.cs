// ================================================
// MaskSystem - 战役管理器（连续关卡）
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 战役状态
    /// </summary>
    public enum CampaignState
    {
        None,           // 未开始
        Playing,        // 进行中
        LevelTransition,// 关卡切换中
        GameComplete,   // 通关
        GameOver        // 游戏结束
    }

    /// <summary>
    /// 战役管理器 - 管理连续多关卡的游戏流程
    /// </summary>
    public class CampaignManager
    {
        #region 属性

        /// <summary>
        /// 当前战役状态
        /// </summary>
        public CampaignState State { get; private set; }

        /// <summary>
        /// 关卡序列
        /// </summary>
        public IReadOnlyList<LevelConfig> Levels => _levels;

        /// <summary>
        /// 当前关卡索引
        /// </summary>
        public int CurrentLevelIndex { get; private set; }

        /// <summary>
        /// 当前关卡配置
        /// </summary>
        public LevelConfig CurrentLevelConfig => CurrentLevelIndex >= 0 && CurrentLevelIndex < _levels.Count 
            ? _levels[CurrentLevelIndex] 
            : null;

        /// <summary>
        /// 是否是最后一关
        /// </summary>
        public bool IsLastLevel => CurrentLevelIndex >= _levels.Count - 1;

        /// <summary>
        /// 总关卡数
        /// </summary>
        public int TotalLevels => _levels.Count;

        /// <summary>
        /// 关卡管理器
        /// </summary>
        public LevelManager LevelManager { get; private set; }

        /// <summary>
        /// 关卡切换倒计时
        /// </summary>
        public float LevelTransitionTimer { get; private set; }

        /// <summary>
        /// 关卡切换间隔时间
        /// </summary>
        public float LevelTransitionTime { get; set; } = 3f;

        #endregion

        #region 私有字段

        private List<LevelConfig> _levels = new List<LevelConfig>();

        #endregion

        #region 事件

        /// <summary>
        /// 战役状态改变
        /// </summary>
        public event Action<CampaignState, CampaignState> OnCampaignStateChanged;

        /// <summary>
        /// 关卡开始
        /// </summary>
        public event Action<int, LevelConfig> OnLevelStart;

        /// <summary>
        /// 关卡完成
        /// </summary>
        public event Action<int, LevelConfig> OnLevelComplete;

        /// <summary>
        /// 游戏通关
        /// </summary>
        public event Action OnGameComplete;

        /// <summary>
        /// 游戏结束
        /// </summary>
        public event Action OnGameOver;

        /// <summary>
        /// 关卡切换倒计时更新
        /// </summary>
        public event Action<float> OnLevelTransitionTimerUpdate;

        #endregion

        #region 构造函数

        public CampaignManager(IMaskSystemAPI api = null)
        {
            LevelManager = new LevelManager(api);

            // 订阅关卡事件
            LevelManager.OnLevelVictory += HandleLevelVictory;
            LevelManager.OnLevelDefeat += HandleLevelDefeat;

            State = CampaignState.None;
            Debug.Log("[CampaignManager] 初始化完成");
        }

        #endregion

        #region 关卡配置

        /// <summary>
        /// 设置关卡序列
        /// </summary>
        public void SetLevels(params LevelConfig[] levels)
        {
            _levels.Clear();
            _levels.AddRange(levels);
            Debug.Log($"[CampaignManager] 设置关卡序列: {_levels.Count} 个关卡");
        }

        /// <summary>
        /// 添加关卡
        /// </summary>
        public void AddLevel(LevelConfig level)
        {
            if (level != null)
            {
                _levels.Add(level);
            }
        }

        /// <summary>
        /// 使用默认三关卡序列（快乐森林 -> 深海 -> 天空）
        /// </summary>
        public void UseDefaultCampaign()
        {
            _levels.Clear();
            _levels.Add(LevelConfig.CreateLevel1_HappyForest());
            _levels.Add(LevelConfig.CreateLevel2_DeepSea());
            _levels.Add(LevelConfig.CreateLevel3_Sky());
            Debug.Log("[CampaignManager] 使用默认三关卡序列");
        }

        #endregion

        #region 战役控制

        /// <summary>
        /// 开始战役
        /// </summary>
        public void StartCampaign()
        {
            if (_levels.Count == 0)
            {
                Debug.LogError("[CampaignManager] 没有配置关卡!");
                return;
            }

            CurrentLevelIndex = 0;
            ChangeState(CampaignState.Playing);
            StartCurrentLevel();

            Debug.Log($"[CampaignManager] 战役开始! 共 {TotalLevels} 个关卡");
        }

        /// <summary>
        /// 从指定关卡开始
        /// </summary>
        public void StartFromLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _levels.Count)
            {
                Debug.LogError($"[CampaignManager] 无效的关卡索引: {levelIndex}");
                return;
            }

            CurrentLevelIndex = levelIndex;
            ChangeState(CampaignState.Playing);
            StartCurrentLevel();

            Debug.Log($"[CampaignManager] 从关卡 {levelIndex + 1} 开始");
        }

        /// <summary>
        /// 重新开始战役
        /// </summary>
        public void RestartCampaign()
        {
            LevelManager?.EndLevel();
            StartCampaign();
        }

        /// <summary>
        /// 重新开始当前关卡
        /// </summary>
        public void RestartCurrentLevel()
        {
            LevelManager?.Restart();
            ChangeState(CampaignState.Playing);
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            LevelManager?.Pause();
        }

        /// <summary>
        /// 恢复
        /// </summary>
        public void Resume()
        {
            LevelManager?.Resume();
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
                case CampaignState.Playing:
                    LevelManager?.Update(deltaTime);
                    break;

                case CampaignState.LevelTransition:
                    UpdateLevelTransition(deltaTime);
                    break;
            }
        }

        private void UpdateLevelTransition(float deltaTime)
        {
            LevelTransitionTimer -= deltaTime;
            OnLevelTransitionTimerUpdate?.Invoke(LevelTransitionTimer);

            if (LevelTransitionTimer <= 0)
            {
                // 切换到下一关
                GoToNextLevel();
            }
        }

        #endregion

        #region 关卡流程

        private void StartCurrentLevel()
        {
            var level = CurrentLevelConfig;
            if (level == null)
            {
                Debug.LogError($"[CampaignManager] 关卡配置为空: {CurrentLevelIndex}");
                return;
            }

            // 不重置玩家血量和面具，保持连续性
            LevelManager.LoadLevel(level);
            LevelManager.StartLevel();

            OnLevelStart?.Invoke(CurrentLevelIndex, level);
            Debug.Log($"[CampaignManager] 开始关卡 {CurrentLevelIndex + 1}/{TotalLevels}: {level.LevelName}");
        }

        private void GoToNextLevel()
        {
            CurrentLevelIndex++;
            ChangeState(CampaignState.Playing);
            StartCurrentLevel();
        }

        #endregion

        #region 事件处理

        private void HandleLevelVictory()
        {
            var completedLevel = CurrentLevelConfig;
            OnLevelComplete?.Invoke(CurrentLevelIndex, completedLevel);

            Debug.Log($"[CampaignManager] 关卡 {CurrentLevelIndex + 1} 完成: {completedLevel?.LevelName}");

            if (IsLastLevel)
            {
                // 所有关卡完成，游戏通关
                HandleGameComplete();
            }
            else
            {
                // 进入关卡切换
                LevelTransitionTimer = LevelTransitionTime;
                ChangeState(CampaignState.LevelTransition);
                Debug.Log($"[CampaignManager] 关卡切换中... 下一关: {CurrentLevelIndex + 2}");
            }
        }

        private void HandleLevelDefeat()
        {
            ChangeState(CampaignState.GameOver);
            OnGameOver?.Invoke();
            Debug.Log($"[CampaignManager] 游戏结束! 止步于关卡 {CurrentLevelIndex + 1}");
        }

        private void HandleGameComplete()
        {
            ChangeState(CampaignState.GameComplete);
            OnGameComplete?.Invoke();
            Debug.Log("[CampaignManager] 恭喜通关! 所有关卡完成!");
        }

        #endregion

        #region 玩家输入

        /// <summary>
        /// 玩家尝试反击
        /// </summary>
        public bool TryCounter()
        {
            if (State != CampaignState.Playing) return false;
            return LevelManager?.TryCounter() ?? false;
        }

        /// <summary>
        /// 切换面具
        /// </summary>
        public bool SwitchMask(int slot)
        {
            return LevelManager?.SwitchMask(slot) ?? false;
        }

        #endregion

        #region 辅助方法

        private void ChangeState(CampaignState newState)
        {
            if (State == newState) return;

            var oldState = State;
            State = newState;
            OnCampaignStateChanged?.Invoke(oldState, newState);

            Debug.Log($"[CampaignManager] 战役状态: {oldState} -> {newState}");
        }

        /// <summary>
        /// 获取状态摘要
        /// </summary>
        public string GetStatusString()
        {
            string status = $"战役状态: {State} | 关卡: {CurrentLevelIndex + 1}/{TotalLevels}";

            if (State == CampaignState.LevelTransition)
            {
                status += $" | 下一关倒计时: {LevelTransitionTimer:F1}秒";
            }

            if (CurrentLevelConfig != null)
            {
                status += $"\n当前: {CurrentLevelConfig.LevelName}";
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
            if (LevelManager != null)
            {
                LevelManager.OnLevelVictory -= HandleLevelVictory;
                LevelManager.OnLevelDefeat -= HandleLevelDefeat;
                LevelManager.Dispose();
            }

            Debug.Log("[CampaignManager] 已清理");
        }

        #endregion
    }
}

