// ================================================
// GameFramework - 单局管理器接口
// ================================================

using System;
using System.Threading.Tasks;
using GameFramework.Core;

namespace GameFramework.Session
{
    /// <summary>
    /// 单局管理器接口
    /// </summary>
    public interface IGameSession : IGameComponent
    {
        #region Properties
        
        /// <summary>
        /// 单局ID
        /// </summary>
        string SessionId { get; }
        
        /// <summary>
        /// 单局状态
        /// </summary>
        SessionState State { get; }
        
        /// <summary>
        /// 单局上下文
        /// </summary>
        ISessionContext Context { get; }
        
        /// <summary>
        /// 当前关卡
        /// </summary>
        ILevel CurrentLevel { get; }
        
        /// <summary>
        /// 关卡管理器
        /// </summary>
        ILevelMgr LevelMgr { get; }
        
        /// <summary>
        /// 规则管理器
        /// </summary>
        IRuleMgr RuleMgr { get; }
        
        /// <summary>
        /// 已进行时间
        /// </summary>
        float ElapsedTime { get; }
        
        /// <summary>
        /// 是否运行中
        /// </summary>
        bool IsRunning { get; }
        
        #endregion
        
        #region Session Control
        
        /// <summary>
        /// 开始单局
        /// </summary>
        Task StartSession(SessionConfig config);
        
        /// <summary>
        /// 结束单局
        /// </summary>
        void EndSession(SessionEndReason reason);
        
        /// <summary>
        /// 暂停单局
        /// </summary>
        void PauseSession();
        
        /// <summary>
        /// 恢复单局
        /// </summary>
        void ResumeSession();
        
        /// <summary>
        /// 重新开始单局
        /// </summary>
        Task RestartSession();
        
        #endregion
        
        #region Level Control
        
        /// <summary>
        /// 加载关卡
        /// </summary>
        Task LoadLevel(int levelId);
        
        /// <summary>
        /// 加载关卡
        /// </summary>
        Task LoadLevel(string levelName);
        
        /// <summary>
        /// 重新加载当前关卡
        /// </summary>
        Task ReloadCurrentLevel();
        
        /// <summary>
        /// 下一关
        /// </summary>
        Task NextLevel();
        
        /// <summary>
        /// 上一关
        /// </summary>
        Task PreviousLevel();
        
        /// <summary>
        /// 获取关卡进度
        /// </summary>
        float GetLevelProgress();
        
        #endregion
        
        #region Rule Control
        
        /// <summary>
        /// 添加规则
        /// </summary>
        void AddRule(ISessionRule rule);
        
        /// <summary>
        /// 移除规则
        /// </summary>
        void RemoveRule(string ruleId);
        
        /// <summary>
        /// 获取规则
        /// </summary>
        T GetRule<T>() where T : class, ISessionRule;
        
        /// <summary>
        /// 启用规则
        /// </summary>
        void EnableRule(string ruleId);
        
        /// <summary>
        /// 禁用规则
        /// </summary>
        void DisableRule(string ruleId);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 单局状态改变事件
        /// </summary>
        event Action<SessionState, SessionState> OnSessionStateChanged;
        
        /// <summary>
        /// 关卡加载完成事件
        /// </summary>
        event Action<ILevel> OnLevelLoaded;
        
        /// <summary>
        /// 关卡完成事件
        /// </summary>
        event Action<ILevel, LevelResult> OnLevelCompleted;
        
        /// <summary>
        /// 规则触发事件
        /// </summary>
        event Action<ISessionRule> OnRuleTriggered;
        
        /// <summary>
        /// 检查点到达事件
        /// </summary>
        event Action<string> OnCheckpointReached;
        
        #endregion
    }
}

