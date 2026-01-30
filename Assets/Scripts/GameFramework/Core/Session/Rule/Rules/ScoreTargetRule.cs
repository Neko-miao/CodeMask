// ================================================
// GameFramework - 分数目标规则
// ================================================

using System;

namespace GameFramework.Session
{
    /// <summary>
    /// 分数目标规则
    /// </summary>
    public class ScoreTargetRule : SessionRuleBase
    {
        private int _targetScore;
        private int _currentScore;
        
        public override string RuleId => "ScoreTargetRule";
        public override string RuleName => "分数目标";
        public override SessionRuleType RuleType => SessionRuleType.ScoreTarget;
        
        /// <summary>
        /// 目标分数
        /// </summary>
        public int TargetScore => _targetScore;
        
        /// <summary>
        /// 当前分数
        /// </summary>
        public int CurrentScore => _currentScore;
        
        /// <summary>
        /// 进度 (0~1)
        /// </summary>
        public float Progress => _targetScore > 0 ? (float)_currentScore / _targetScore : 0f;
        
        /// <summary>
        /// 分数改变事件
        /// </summary>
        public event Action<int, int> OnScoreChanged;
        
        /// <summary>
        /// 达到目标事件
        /// </summary>
        public event Action OnScoreReached;
        
        public ScoreTargetRule(int targetScore)
        {
            _targetScore = targetScore;
        }
        
        public override void OnInit(ISessionContext context)
        {
            base.OnInit(context);
            _currentScore = 0;
        }
        
        public override bool OnCheck()
        {
            return _currentScore >= _targetScore;
        }
        
        public override void OnTrigger()
        {
            base.OnTrigger();
            OnScoreReached?.Invoke();
        }
        
        public override void OnReset()
        {
            base.OnReset();
            _currentScore = 0;
        }
        
        /// <summary>
        /// 添加分数
        /// </summary>
        public void AddScore(int score)
        {
            if (score <= 0) return;
            
            int oldScore = _currentScore;
            _currentScore += score;
            
            if (_context != null)
            {
                _context.Score = _currentScore;
            }
            
            OnScoreChanged?.Invoke(oldScore, _currentScore);
            
            // 检查是否达到目标
            if (!_isTriggered && OnCheck())
            {
                Trigger();
            }
        }
        
        /// <summary>
        /// 设置分数
        /// </summary>
        public void SetScore(int score)
        {
            int oldScore = _currentScore;
            _currentScore = score;
            
            if (_context != null)
            {
                _context.Score = _currentScore;
            }
            
            OnScoreChanged?.Invoke(oldScore, _currentScore);
        }
    }
}

