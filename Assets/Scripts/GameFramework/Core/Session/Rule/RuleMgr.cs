// ================================================
// GameFramework - 规则管理器实现
// ================================================

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameFramework.Session
{
    /// <summary>
    /// 规则管理器实现
    /// </summary>
    public class RuleMgr : IRuleMgr
    {
        private readonly Dictionary<string, ISessionRule> _rules = new Dictionary<string, ISessionRule>();
        private readonly List<ISessionRule> _ruleList = new List<ISessionRule>();
        private readonly List<ISessionRule> _triggeredRules = new List<ISessionRule>();
        private ISessionContext _context;
        
        public IReadOnlyList<ISessionRule> ActiveRules => _ruleList;
        public bool IsEnabled { get; set; } = true;
        
        #region Events
        
        public event Action<ISessionRule> OnRuleAdded;
        public event Action<ISessionRule> OnRuleRemoved;
        public event Action<ISessionRule> OnRuleTriggered;
        public event Action<ISessionRule, bool> OnRuleStateChanged;
        
        #endregion
        
        #region Rule Management
        
        public void AddRule(ISessionRule rule)
        {
            if (rule == null) return;
            
            if (_rules.ContainsKey(rule.RuleId))
            {
                Debug.LogWarning($"[RuleMgr] Rule already exists: {rule.RuleId}");
                return;
            }
            
            _rules[rule.RuleId] = rule;
            _ruleList.Add(rule);
            _ruleList.Sort((a, b) => a.Priority.CompareTo(b.Priority));
            
            rule.OnRuleTriggered += HandleRuleTriggered;
            
            if (_context != null)
            {
                rule.OnInit(_context);
            }
            
            OnRuleAdded?.Invoke(rule);
            
            Debug.Log($"[RuleMgr] Added rule: {rule.RuleId}");
        }
        
        public void RemoveRule(string ruleId)
        {
            if (_rules.TryGetValue(ruleId, out var rule))
            {
                rule.OnRuleTriggered -= HandleRuleTriggered;
                rule.OnDestroy();
                
                _rules.Remove(ruleId);
                _ruleList.Remove(rule);
                _triggeredRules.Remove(rule);
                
                OnRuleRemoved?.Invoke(rule);
                
                Debug.Log($"[RuleMgr] Removed rule: {ruleId}");
            }
        }
        
        public ISessionRule GetRule(string ruleId)
        {
            _rules.TryGetValue(ruleId, out var rule);
            return rule;
        }
        
        public T GetRule<T>() where T : class, ISessionRule
        {
            foreach (var rule in _ruleList)
            {
                if (rule is T typedRule)
                    return typedRule;
            }
            return null;
        }
        
        public bool HasRule(string ruleId)
        {
            return _rules.ContainsKey(ruleId);
        }
        
        public bool HasRule<T>() where T : class, ISessionRule
        {
            return GetRule<T>() != null;
        }
        
        public void EnableRule(string ruleId)
        {
            if (_rules.TryGetValue(ruleId, out var rule))
            {
                rule.IsEnabled = true;
                OnRuleStateChanged?.Invoke(rule, true);
            }
        }
        
        public void DisableRule(string ruleId)
        {
            if (_rules.TryGetValue(ruleId, out var rule))
            {
                rule.IsEnabled = false;
                OnRuleStateChanged?.Invoke(rule, false);
            }
        }
        
        public void ClearRules()
        {
            foreach (var rule in _ruleList)
            {
                rule.OnRuleTriggered -= HandleRuleTriggered;
                rule.OnDestroy();
            }
            
            _rules.Clear();
            _ruleList.Clear();
            _triggeredRules.Clear();
        }
        
        #endregion
        
        #region Rule Check
        
        public void CheckRules()
        {
            if (!IsEnabled) return;
            
            foreach (var rule in _ruleList)
            {
                if (rule.IsEnabled && !rule.IsTriggered)
                {
                    rule.OnCheck();
                }
            }
        }
        
        public bool CheckRule(string ruleId)
        {
            if (_rules.TryGetValue(ruleId, out var rule))
            {
                return rule.OnCheck();
            }
            return false;
        }
        
        public IReadOnlyList<ISessionRule> GetTriggeredRules()
        {
            return _triggeredRules;
        }
        
        #endregion
        
        #region Lifecycle
        
        public void Initialize(ISessionContext context)
        {
            _context = context;
            
            foreach (var rule in _ruleList)
            {
                rule.OnInit(context);
            }
        }
        
        public void Start()
        {
            foreach (var rule in _ruleList)
            {
                rule.OnStart();
            }
        }
        
        public void Tick(float deltaTime)
        {
            if (!IsEnabled) return;
            
            foreach (var rule in _ruleList)
            {
                if (rule.IsEnabled)
                {
                    rule.OnTick(deltaTime);
                }
            }
        }
        
        public void Reset()
        {
            _triggeredRules.Clear();
            
            foreach (var rule in _ruleList)
            {
                rule.OnReset();
            }
        }
        
        public void Destroy()
        {
            ClearRules();
            _context = null;
        }
        
        #endregion
        
        #region Private Methods
        
        private void HandleRuleTriggered(ISessionRule rule)
        {
            if (!_triggeredRules.Contains(rule))
            {
                _triggeredRules.Add(rule);
            }
            
            OnRuleTriggered?.Invoke(rule);
        }
        
        #endregion
    }
}

