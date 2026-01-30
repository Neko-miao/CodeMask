// ================================================
// GameFramework - 关卡基类
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Session
{
    /// <summary>
    /// 关卡基类
    /// </summary>
    public abstract class LevelBase : ILevel
    {
        protected readonly List<ILevelObjective> _objectives = new List<ILevelObjective>();
        protected LevelData _data;
        
        #region Properties
        
        public int LevelId { get; protected set; }
        public string LevelName { get; protected set; }
        public ILevelConfig Config { get; protected set; }
        public LevelState State { get; protected set; }
        public virtual float Progress => CalculateProgress();
        public ILevelData Data => _data;
        
        #endregion
        
        public LevelBase(ILevelConfig config)
        {
            Config = config;
            LevelId = config.LevelId;
            LevelName = config.LevelName;
            State = LevelState.None;
            _data = new LevelData();
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
            int stars = 1;
            
            // 基于时间评价
            if (Config.TimeLimit > 0 && _data.ElapsedTime < Config.TimeLimit * 0.5f)
                stars++;
            
            // 基于分数评价
            if (Config.TargetScore > 0 && _data.Score >= Config.TargetScore)
                stars++;
            
            return Mathf.Clamp(stars, 1, 3);
        }
        
        #endregion
    }
}

