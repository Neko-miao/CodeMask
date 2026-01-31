// ================================================
// GameFramework - 关卡基类
// ================================================

using System;
using System.Collections.Generic;
using GameConfigs;
using UnityEngine;

namespace GameFramework.Session
{
    /// <summary>
    /// 关卡基类
    /// </summary>
    public abstract class LevelBase : ILevel
    {
        protected readonly List<ILevelObjective> _objectives = new List<ILevelObjective>();
        protected LevelRuntimeData _data;
        
        #region Properties
        
        public int LevelId { get; protected set; }
        public string LevelName { get; protected set; }
        public LevelData Config { get; protected set; }
        public LevelState State { get; protected set; }
        public virtual float Progress => CalculateProgress();
        public ILevelData Data => _data;
        
        #endregion
        
        public LevelBase(LevelData config)
        {
            Config = config;
            LevelId = config.levelId;
            LevelName = config.levelName;
            State = LevelState.None;
            _data = new LevelRuntimeData();
        }
        
        #region Lifecycle
        
        public virtual void OnLoad()
        {
            State = LevelState.Loaded;
            InitializeObjectives();
        }
        
        public virtual void OnStart()
        {
            State = LevelState.Running;
            _data.Reset();
            
            foreach (var objective in _objectives)
            {
                objective.Initialize();
            }
        }
        
        public virtual void OnUpdate(float deltaTime)
        {
            if (State != LevelState.Running) return;
            
            _data.ElapsedTime += deltaTime;
        }
        
        public virtual void OnPause()
        {
            // 子类可重写
        }
        
        public virtual void OnResume()
        {
            // 子类可重写
        }
        
        public virtual void OnComplete(LevelResult result)
        {
            State = result.IsSuccess ? LevelState.Completed : LevelState.Failed;
        }
        
        public virtual void OnUnload()
        {
            State = LevelState.None;
            _objectives.Clear();
        }
        
        #endregion
        
        #region Control
        
        public void Start()
        {
            OnStart();
        }
        
        public void Complete(LevelResult result)
        {
            OnComplete(result);
        }
        
        public void Fail(string reason)
        {
            Complete(new LevelResult
            {
                IsSuccess = false,
                FailReason = reason,
                Score = _data.Score,
                CompletionTime = _data.ElapsedTime
            });
        }
        
        public void Reset()
        {
            _data.Reset();
            foreach (var objective in _objectives)
            {
                objective.Reset();
            }
            State = LevelState.Loaded;
        }
        
        #endregion
        
        #region Objectives
        
        /// <summary>
        /// 初始化目标 (子类重写)
        /// </summary>
        protected virtual void InitializeObjectives()
        {
            // 子类可在此添加目标
        }
        
        /// <summary>
        /// 添加目标
        /// </summary>
        protected void AddObjective(ILevelObjective objective)
        {
            _objectives.Add(objective);
            objective.OnCompleted += OnObjectiveCompleted;
        }
        
        /// <summary>
        /// 目标完成回调
        /// </summary>
        protected virtual void OnObjectiveCompleted(ILevelObjective objective)
        {
            if (AreAllObjectivesComplete())
            {
                Complete(new LevelResult
                {
                    IsSuccess = true,
                    Score = _data.Score,
                    CompletionTime = _data.ElapsedTime,
                    Stars = CalculateStars()
                });
            }
        }
        
        public IReadOnlyList<ILevelObjective> GetObjectives()
        {
            return _objectives;
        }
        
        public void CompleteObjective(string objectiveId)
        {
            foreach (var obj in _objectives)
            {
                if (obj.ObjectiveId == objectiveId)
                {
                    obj.Complete();
                    break;
                }
            }
        }
        
        public float GetObjectiveProgress(string objectiveId)
        {
            foreach (var obj in _objectives)
            {
                if (obj.ObjectiveId == objectiveId)
                {
                    return obj.Progress;
                }
            }
            return 0f;
        }
        
        public bool AreAllObjectivesComplete()
        {
            foreach (var obj in _objectives)
            {
                if (obj.Type == ObjectiveType.Primary && !obj.IsCompleted)
                {
                    return false;
                }
            }
            return _objectives.Count > 0;
        }
        
        #endregion
        
        #region Protected Methods
        
        protected virtual float CalculateProgress()
        {
            if (_objectives.Count == 0) return 0f;
            
            float totalProgress = 0f;
            int primaryCount = 0;
            
            foreach (var obj in _objectives)
            {
                if (obj.Type == ObjectiveType.Primary)
                {
                    totalProgress += obj.Progress;
                    primaryCount++;
                }
            }
            
            return primaryCount > 0 ? totalProgress / primaryCount : 0f;
        }
        
        protected virtual int CalculateStars()
        {
            var starCondition = Config?.starCondition;
            if (starCondition == null)
            {
                // 默认计算方式
                int stars = 1;
                if (Config.timeLimit > 0 && _data.ElapsedTime < Config.timeLimit * 0.5f)
                    stars++;
                if (Config.reward != null && _data.Score >= Config.reward.gold)
                    stars++;
                return Mathf.Clamp(stars, 1, 3);
            }
            
            // 使用配置的星级条件计算
            int resultStars = 1;
            
            // 二星条件
            if (starCondition.timeLimitForTwoStar <= 0 || _data.ElapsedTime <= starCondition.timeLimitForTwoStar)
            {
                resultStars = 2;
            }
            
            // 三星条件
            bool timeOk = starCondition.timeLimitForThreeStar <= 0 || _data.ElapsedTime <= starCondition.timeLimitForThreeStar;
            bool damageOk = starCondition.maxDamageCountForThreeStar < 0 || _data.DamageCount <= starCondition.maxDamageCountForThreeStar;
            
            if (timeOk && damageOk)
            {
                resultStars = 3;
            }
            
            return resultStars;
        }
        
        #endregion
    }
}
