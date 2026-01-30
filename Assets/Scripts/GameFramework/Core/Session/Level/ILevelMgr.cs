// ================================================
// GameFramework - 关卡管理器接口
// ================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GameFramework.Session
{
    /// <summary>
    /// 关卡管理器接口
    /// </summary>
    public interface ILevelMgr
    {
        #region Properties
        
        /// <summary>
        /// 当前关卡
        /// </summary>
        ILevel CurrentLevel { get; }
        
        /// <summary>
        /// 当前关卡ID
        /// </summary>
        int CurrentLevelId { get; }
        
        /// <summary>
        /// 总关卡数
        /// </summary>
        int TotalLevels { get; }
        
        /// <summary>
        /// 已解锁关卡数
        /// </summary>
        int UnlockedLevels { get; }
        
        /// <summary>
        /// 是否加载中
        /// </summary>
        bool IsLoading { get; }
        
        #endregion
        
        #region Level Operations
        
        /// <summary>
        /// 加载关卡
        /// </summary>
        Task LoadLevel(int levelId);
        
        /// <summary>
        /// 加载关卡
        /// </summary>
        Task LoadLevel(ILevelConfig config);
        
        /// <summary>
        /// 卸载当前关卡
        /// </summary>
        void UnloadCurrentLevel();
        
        /// <summary>
        /// 重新加载关卡
        /// </summary>
        Task ReloadLevel();
        
        /// <summary>
        /// 开始当前关卡
        /// </summary>
        void StartLevel();
        
        /// <summary>
        /// 完成当前关卡
        /// </summary>
        void CompleteLevel(LevelResult result);
        
        #endregion
        
        #region Level Config
        
        /// <summary>
        /// 注册关卡配置
        /// </summary>
        void RegisterLevelConfig(ILevelConfig config);
        
        /// <summary>
        /// 获取关卡配置
        /// </summary>
        ILevelConfig GetLevelConfig(int levelId);
        
        /// <summary>
        /// 获取所有关卡配置
        /// </summary>
        IReadOnlyList<ILevelConfig> GetAllLevelConfigs();
        
        #endregion
        
        #region Progress
        
        /// <summary>
        /// 获取关卡进度
        /// </summary>
        float GetProgress();
        
        /// <summary>
        /// 设置检查点
        /// </summary>
        void SetCheckpoint(string checkpointId);
        
        /// <summary>
        /// 获取检查点
        /// </summary>
        string GetCheckpoint();
        
        /// <summary>
        /// 清除检查点
        /// </summary>
        void ClearCheckpoint();
        
        #endregion
        
        #region Unlock
        
        /// <summary>
        /// 解锁关卡
        /// </summary>
        void UnlockLevel(int levelId);
        
        /// <summary>
        /// 检查关卡是否已解锁
        /// </summary>
        bool IsLevelUnlocked(int levelId);
        
        /// <summary>
        /// 获取解锁条件
        /// </summary>
        IUnlockCondition GetUnlockCondition(int levelId);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 关卡开始加载事件
        /// </summary>
        event Action<int> OnLevelLoadStart;
        
        /// <summary>
        /// 加载进度更新事件
        /// </summary>
        event Action<float> OnLevelLoadProgress;
        
        /// <summary>
        /// 关卡加载完成事件
        /// </summary>
        event Action<ILevel> OnLevelLoadComplete;
        
        /// <summary>
        /// 关卡开始事件
        /// </summary>
        event Action<ILevel> OnLevelStarted;
        
        /// <summary>
        /// 关卡完成事件
        /// </summary>
        event Action<ILevel, LevelResult> OnLevelCompleted;
        
        /// <summary>
        /// 关卡卸载事件
        /// </summary>
        event Action<ILevel> OnLevelUnloaded;
        
        /// <summary>
        /// 关卡解锁事件
        /// </summary>
        event Action<int> OnLevelUnlocked;
        
        #endregion
    }
}

