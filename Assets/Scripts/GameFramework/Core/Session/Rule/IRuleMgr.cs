// ================================================
// GameFramework - 规则管理器接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Session
{
    /// <summary>
    /// 规则管理器接口
    /// </summary>
    public interface IRuleMgr
    {
        #region Properties
        
        /// <summary>
        /// 激活的规则列表
        /// </summary>
        IReadOnlyList<ISessionRule> ActiveRules { get; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }
        
        #endregion
        
        #region Rule Management
        
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
        ISessionRule GetRule(string ruleId);
        
        /// <summary>
        /// 获取规则
        /// </summary>
        T GetRule<T>() where T : class, ISessionRule;
        
        /// <summary>
        /// 检查是否有规则
        /// </summary>
        bool HasRule(string ruleId);
        
        /// <summary>
        /// 检查是否有规则
        /// </summary>
        bool HasRule<T>() where T : class, ISessionRule;
        
        /// <summary>
        /// 启用规则
        /// </summary>
        void EnableRule(string ruleId);
        
        /// <summary>
        /// 禁用规则
        /// </summary>
        void DisableRule(string ruleId);
        
        /// <summary>
        /// 清除所有规则
        /// </summary>
        void ClearRules();
        
        #endregion
        
        #region Rule Check
        
        /// <summary>
        /// 检查所有规则
        /// </summary>
        void CheckRules();
        
        /// <summary>
        /// 检查特定规则
        /// </summary>
        bool CheckRule(string ruleId);
        
        /// <summary>
        /// 获取已触发的规则
        /// </summary>
        IReadOnlyList<ISessionRule> GetTriggeredRules();
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize(ISessionContext context);
        
        /// <summary>
        /// 开始
        /// </summary>
        void Start();
        
        /// <summary>
        /// 更新
        /// </summary>
        void Tick(float deltaTime);
        
        /// <summary>
        /// 重置
        /// </summary>
        void Reset();
        
        /// <summary>
        /// 销毁
        /// </summary>
        void Destroy();
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 规则添加事件
        /// </summary>
        event Action<ISessionRule> OnRuleAdded;
        
        /// <summary>
        /// 规则移除事件
        /// </summary>
        event Action<ISessionRule> OnRuleRemoved;
        
        /// <summary>
        /// 规则触发事件
        /// </summary>
        event Action<ISessionRule> OnRuleTriggered;
        
        /// <summary>
        /// 规则状态改变事件
        /// </summary>
        event Action<ISessionRule, bool> OnRuleStateChanged;
        
        #endregion
    }
}

