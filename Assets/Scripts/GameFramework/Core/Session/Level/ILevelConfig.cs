// ================================================
// GameFramework - 关卡配置接口
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Session
{
    /// <summary>
    /// 关卡配置接口
    /// </summary>
    public interface ILevelConfig
    {
        /// <summary>
        /// 关卡ID
        /// </summary>
        int LevelId { get; }
        
        /// <summary>
        /// 关卡名称
        /// </summary>
        string LevelName { get; }
        
        /// <summary>
        /// 关卡描述
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 场景名称
        /// </summary>
        string SceneName { get; }
        
        /// <summary>
        /// 时间限制 (秒, 0表示无限制)
        /// </summary>
        float TimeLimit { get; }
        
        /// <summary>
        /// 目标分数
        /// </summary>
        int TargetScore { get; }
        
        /// <summary>
        /// 难度等级
        /// </summary>
        int Difficulty { get; }
        
        /// <summary>
        /// 解锁条件
        /// </summary>
        IUnlockCondition UnlockCondition { get; }
    }
    
    /// <summary>
    /// 解锁条件接口
    /// </summary>
    public interface IUnlockCondition
    {
        /// <summary>
        /// 检查是否满足解锁条件
        /// </summary>
        bool IsMet();
        
        /// <summary>
        /// 获取条件描述
        /// </summary>
        string GetDescription();
    }
    
    /// <summary>
    /// 关卡配置实现
    /// </summary>
    [Serializable]
    public class LevelConfig : ILevelConfig
    {
        [SerializeField] private int _levelId;
        [SerializeField] private string _levelName;
        [SerializeField] private string _description;
        [SerializeField] private string _sceneName;
        [SerializeField] private float _timeLimit;
        [SerializeField] private int _targetScore;
        [SerializeField] private int _difficulty;
        
        public int LevelId => _levelId;
        public string LevelName => _levelName;
        public string Description => _description;
        public string SceneName => _sceneName;
        public float TimeLimit => _timeLimit;
        public int TargetScore => _targetScore;
        public int Difficulty => _difficulty;
        public IUnlockCondition UnlockCondition => null;
        
        public LevelConfig(int levelId, string levelName, string sceneName)
        {
            _levelId = levelId;
            _levelName = levelName;
            _sceneName = sceneName;
        }
    }
}

