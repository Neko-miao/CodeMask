// ================================================
// GameFramework - 单局规则基类
// ================================================

using System;
using GameFramework.Core;

namespace GameFramework.Session
{
    /// <summary>
    /// 单局规则基类
    /// </summary>
    public abstract class SessionRuleBase : ISessionRule
    {
        protected ISessionContext _context;
        protected bool _isTriggered;
        
        #region Properties
        
        public virtual string RuleId => GetType().Name;
        public virtual string RuleName => GetType().Name;
        public abstract SessionRuleType RuleType { get; }
        public virtual int Priority => 0;
        public bool IsEnabled { get; set; } = true;
        public bool IsTriggered => _isTriggered;
        
        #endregion
        
        #region Events
        
        public event Action<ISessionRule> OnRuleTriggered;
        
        #endregion
        
        #region Lifecycle
        
        public virtual void OnInit(ISessionContext context)
        {
            _context = context;
            _isTriggered = false;
        }
        
        public virtual void OnStart()
        {
            // 子类可重写
        }
        
        public virtual void OnTick(float deltaTime)
        {
            if (!IsEnabled || _isTriggered) return;
            
            if (OnCheck())
            {
                Trigger();
            }
        }
        
        public abstract bool OnCheck();
        
        public virtual void OnTrigger()
        {
            // 子类可重写
        }
        
        public virtual void OnReset()
        {
            _isTriggered = false;
        }
        
        public virtual void OnDestroy()
        {
            _context = null;
        }
        
        #endregion
        
        #region Protected Methods
        
        protected void Trigger()
        {
            if (_isTriggered) return;
            
            _isTriggered = true;
            OnTrigger();
            OnRuleTriggered?.Invoke(this);
        }
        
        protected T GetComp<T>() where T : class, IGameComponent
        {
            return GameInstance.Instance?.GetComp<T>();
        }
        
        #endregion
    }
}

