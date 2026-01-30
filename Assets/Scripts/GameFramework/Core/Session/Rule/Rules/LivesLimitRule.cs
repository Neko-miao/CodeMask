// ================================================
// GameFramework - 生命限制规则
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Session
{
    /// <summary>
    /// 生命限制规则
    /// </summary>
    public class LivesLimitRule : SessionRuleBase
    {
        private int _maxLives;
        private int _currentLives;
        
        public override string RuleId => "LivesLimitRule";
        public override string RuleName => "生命限制";
        public override SessionRuleType RuleType => SessionRuleType.LivesLimit;
        
        /// <summary>
        /// 最大生命数
        /// </summary>
        public int MaxLives => _maxLives;
        
        /// <summary>
        /// 当前生命数
        /// </summary>
        public int CurrentLives => _currentLives;
        
        /// <summary>
        /// 生命改变事件
        /// </summary>
        public event Action<int, int> OnLivesChanged;
        
        /// <summary>
        /// 生命耗尽事件
        /// </summary>
        public event Action OnAllLivesLost;
        
        /// <summary>
        /// 失去生命事件
        /// </summary>
        public event Action<int> OnLifeLost;
        
        /// <summary>
        /// 获得生命事件
        /// </summary>
        public event Action<int> OnLifeGained;
        
        public LivesLimitRule(int maxLives)
        {
            _maxLives = maxLives;
        }
        
        public override void OnInit(ISessionContext context)
        {
            base.OnInit(context);
            _currentLives = _maxLives;
        }
        
        public override bool OnCheck()
        {
            return _currentLives <= 0;
        }
        
        public override void OnTrigger()
        {
            base.OnTrigger();
            OnAllLivesLost?.Invoke();
        }
        
        public override void OnReset()
        {
            base.OnReset();
            _currentLives = _maxLives;
        }
        
        /// <summary>
        /// 失去生命
        /// </summary>
        public void LoseLife(int count = 1)
        {
            if (count <= 0 || _currentLives <= 0) return;
            
            int oldLives = _currentLives;
            _currentLives = Mathf.Max(0, _currentLives - count);
            
            if (_context != null)
            {
                _context.DeathCount++;
            }
            
            OnLivesChanged?.Invoke(oldLives, _currentLives);
            OnLifeLost?.Invoke(_currentLives);
            
            // 检查是否生命耗尽
            if (!_isTriggered && OnCheck())
            {
                Trigger();
            }
        }
        
        /// <summary>
        /// 获得生命
        /// </summary>
        public void GainLife(int count = 1)
        {
            if (count <= 0) return;
            
            int oldLives = _currentLives;
            _currentLives = Mathf.Min(_maxLives, _currentLives + count);
            
            OnLivesChanged?.Invoke(oldLives, _currentLives);
            OnLifeGained?.Invoke(_currentLives);
        }
        
        /// <summary>
        /// 设置生命数
        /// </summary>
        public void SetLives(int lives)
        {
            int oldLives = _currentLives;
            _currentLives = Mathf.Clamp(lives, 0, _maxLives);
            OnLivesChanged?.Invoke(oldLives, _currentLives);
        }
        
        /// <summary>
        /// 设置最大生命数
        /// </summary>
        public void SetMaxLives(int maxLives)
        {
            _maxLives = maxLives;
            _currentLives = Mathf.Min(_currentLives, _maxLives);
        }
    }
}

