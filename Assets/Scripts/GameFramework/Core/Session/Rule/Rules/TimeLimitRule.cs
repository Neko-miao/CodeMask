// ================================================
// GameFramework - 时间限制规则
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Session
{
    /// <summary>
    /// 时间限制规则
    /// </summary>
    public class TimeLimitRule : SessionRuleBase
    {
        private float _timeLimit;
        private float _warningTime;
        private float _remainingTime;
        private bool _warningTriggered;
        
        public override string RuleId => "TimeLimitRule";
        public override string RuleName => "时间限制";
        public override SessionRuleType RuleType => SessionRuleType.TimeLimit;
        
        /// <summary>
        /// 时间限制 (秒)
        /// </summary>
        public float TimeLimit => _timeLimit;
        
        /// <summary>
        /// 剩余时间
        /// </summary>
        public float RemainingTime => _remainingTime;
        
        /// <summary>
        /// 警告时间
        /// </summary>
        public float WarningTime => _warningTime;
        
        /// <summary>
        /// 时间警告事件
        /// </summary>
        public event Action<float> OnTimeWarning;
        
        /// <summary>
        /// 时间到事件
        /// </summary>
        public event Action OnTimeUp;
        
        /// <summary>
        /// 时间更新事件
        /// </summary>
        public event Action<float> OnTimeUpdated;
        
        public TimeLimitRule(float timeLimit, float warningTime = 30f)
        {
            _timeLimit = timeLimit;
            _warningTime = warningTime;
        }
        
        public override void OnInit(ISessionContext context)
        {
            base.OnInit(context);
            _remainingTime = _timeLimit;
            _warningTriggered = false;
        }
        
        public override void OnTick(float deltaTime)
        {
            if (!IsEnabled || _isTriggered) return;
            
            _remainingTime -= deltaTime;
            _remainingTime = Mathf.Max(0f, _remainingTime);
            
            OnTimeUpdated?.Invoke(_remainingTime);
            
            // 检查警告
            if (!_warningTriggered && _remainingTime <= _warningTime)
            {
                _warningTriggered = true;
                OnTimeWarning?.Invoke(_remainingTime);
            }
            
            base.OnTick(deltaTime);
        }
        
        public override bool OnCheck()
        {
            return _remainingTime <= 0f;
        }
        
        public override void OnTrigger()
        {
            base.OnTrigger();
            OnTimeUp?.Invoke();
        }
        
        public override void OnReset()
        {
            base.OnReset();
            _remainingTime = _timeLimit;
            _warningTriggered = false;
        }
        
        /// <summary>
        /// 添加时间
        /// </summary>
        public void AddTime(float seconds)
        {
            _remainingTime += seconds;
            OnTimeUpdated?.Invoke(_remainingTime);
        }
        
        /// <summary>
        /// 设置时间
        /// </summary>
        public void SetTime(float seconds)
        {
            _remainingTime = seconds;
            OnTimeUpdated?.Invoke(_remainingTime);
        }
    }
}

