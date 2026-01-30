// ================================================
// GameFramework - 单局规则接口
// ================================================

using System;

namespace GameFramework.Session
{
    /// <summary>
    /// 规则类型
    /// </summary>
    public enum SessionRuleType
    {
        /// <summary>
        /// 胜利条件
        /// </summary>
        WinCondition,
        
        /// <summary>
        /// 失败条件
        /// </summary>
        LoseCondition,
        
        /// <summary>
        /// 时间限制
        /// </summary>
        TimeLimit,
        
        /// <summary>
        /// 分数目标
        /// </summary>
        ScoreTarget,
        
        /// <summary>
        /// 收集目标
        /// </summary>
        CollectTarget,
        
        /// <summary>
        /// 生存目标
        /// </summary>
        SurvivalTarget,
        
        /// <summary>
        /// 消灭目标
        /// </summary>
        EliminateTarget,
        
        /// <summary>
        /// 到达目标
        /// </summary>
        ReachTarget,
        
        /// <summary>
        /// 保护目标
        /// </summary>
        ProtectTarget,
        
        /// <summary>
        /// 护送目标
        /// </summary>
        EscortTarget,
        
        /// <summary>
        /// 资源限制
        /// </summary>
        ResourceLimit,
        
        /// <summary>
        /// 生命限制
        /// </summary>
        LivesLimit,
        
        /// <summary>
        /// 波次系统
        /// </summary>
        WaveSystem,
        
        /// <summary>
        /// 连击系统
        /// </summary>
        ComboSystem,
        
        /// <summary>
        /// Buff/Debuff规则
        /// </summary>
        BuffDebuff,
        
        /// <summary>
        /// 自定义规则
        /// </summary>
        Custom
    }
    
    /// <summary>
    /// 单局规则接口
    /// </summary>
    public interface ISessionRule
    {
        #region Properties
        
        /// <summary>
        /// 规则ID
        /// </summary>
        string RuleId { get; }
        
        /// <summary>
        /// 规则名称
        /// </summary>
        string RuleName { get; }
        
        /// <summary>
        /// 规则类型
        /// </summary>
        SessionRuleType RuleType { get; }
        
        /// <summary>
        /// 优先级
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// 是否已触发
        /// </summary>
        bool IsTriggered { get; }
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// 初始化
        /// </summary>
        void OnInit(ISessionContext context);
        
        /// <summary>
        /// 开始
        /// </summary>
        void OnStart();
        
        /// <summary>
        /// 更新
        /// </summary>
        void OnTick(float deltaTime);
        
        /// <summary>
        /// 检查是否触发
        /// </summary>
        bool OnCheck();
        
        /// <summary>
        /// 触发时执行
        /// </summary>
        void OnTrigger();
        
        /// <summary>
        /// 重置
        /// </summary>
        void OnReset();
        
        /// <summary>
        /// 销毁
        /// </summary>
        void OnDestroy();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 规则触发事件
        /// </summary>
        event Action<ISessionRule> OnRuleTriggered;
        
        #endregion
    }
}

