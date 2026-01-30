// ================================================
// GameFramework - 关卡目标接口
// ================================================

using System;

namespace GameFramework.Session
{
    /// <summary>
    /// 目标类型
    /// </summary>
    public enum ObjectiveType
    {
        /// <summary>
        /// 主要目标
        /// </summary>
        Primary,
        
        /// <summary>
        /// 次要目标
        /// </summary>
        Secondary,
        
        /// <summary>
        /// 可选目标
        /// </summary>
        Optional,
        
        /// <summary>
        /// 隐藏目标
        /// </summary>
        Hidden
    }
    
    /// <summary>
    /// 关卡目标接口
    /// </summary>
    public interface ILevelObjective
    {
        /// <summary>
        /// 目标ID
        /// </summary>
        string ObjectiveId { get; }
        
        /// <summary>
        /// 目标名称
        /// </summary>
        string ObjectiveName { get; }
        
        /// <summary>
        /// 目标描述
        /// </summary>
        string Description { get; }
        
        /// <summary>
        /// 目标类型
        /// </summary>
        ObjectiveType Type { get; }
        
        /// <summary>
        /// 是否完成
        /// </summary>
        bool IsCompleted { get; }
        
        /// <summary>
        /// 进度 (0~1)
        /// </summary>
        float Progress { get; }
        
        /// <summary>
        /// 当前值
        /// </summary>
        int CurrentValue { get; }
        
        /// <summary>
        /// 目标值
        /// </summary>
        int TargetValue { get; }
        
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 更新进度
        /// </summary>
        void UpdateProgress(int value);
        
        /// <summary>
        /// 增加进度
        /// </summary>
        void AddProgress(int amount);
        
        /// <summary>
        /// 完成目标
        /// </summary>
        void Complete();
        
        /// <summary>
        /// 重置目标
        /// </summary>
        void Reset();
        
        /// <summary>
        /// 目标完成事件
        /// </summary>
        event Action<ILevelObjective> OnCompleted;
        
        /// <summary>
        /// 进度更新事件
        /// </summary>
        event Action<ILevelObjective, float> OnProgressChanged;
    }
    
    /// <summary>
    /// 关卡目标基类
    /// </summary>
    public class LevelObjective : ILevelObjective
    {
        public string ObjectiveId { get; protected set; }
        public string ObjectiveName { get; protected set; }
        public string Description { get; protected set; }
        public ObjectiveType Type { get; protected set; }
        public bool IsCompleted { get; protected set; }
        public int CurrentValue { get; protected set; }
        public int TargetValue { get; protected set; }
        
        public float Progress => TargetValue > 0 ? (float)CurrentValue / TargetValue : 0f;
        
        public event Action<ILevelObjective> OnCompleted;
        public event Action<ILevelObjective, float> OnProgressChanged;
        
        public LevelObjective(string id, string name, int targetValue, ObjectiveType type = ObjectiveType.Primary)
        {
            ObjectiveId = id;
            ObjectiveName = name;
            TargetValue = targetValue;
            Type = type;
        }
        
        public virtual void Initialize()
        {
            CurrentValue = 0;
            IsCompleted = false;
        }
        
        public virtual void UpdateProgress(int value)
        {
            CurrentValue = Math.Min(value, TargetValue);
            OnProgressChanged?.Invoke(this, Progress);
            
            if (CurrentValue >= TargetValue && !IsCompleted)
            {
                Complete();
            }
        }
        
        public virtual void AddProgress(int amount)
        {
            UpdateProgress(CurrentValue + amount);
        }
        
        public virtual void Complete()
        {
            if (IsCompleted) return;
            
            IsCompleted = true;
            CurrentValue = TargetValue;
            OnCompleted?.Invoke(this);
        }
        
        public virtual void Reset()
        {
            CurrentValue = 0;
            IsCompleted = false;
        }
    }
}

