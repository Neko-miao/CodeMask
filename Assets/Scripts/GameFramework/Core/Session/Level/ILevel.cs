// ================================================
// GameFramework - 关卡接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Session
{
    /// <summary>
    /// 关卡结果
    /// </summary>
    public class LevelResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess;
        
        /// <summary>
        /// 分数
        /// </summary>
        public int Score;
        
        /// <summary>
        /// 星级评价 (1-3)
        /// </summary>
        public int Stars;
        
        /// <summary>
        /// 完成时间
        /// </summary>
        public float CompletionTime;
        
        /// <summary>
        /// 失败原因
        /// </summary>
        public string FailReason;
    }
    
    /// <summary>
    /// 关卡接口
    /// </summary>
    public interface ILevel
    {
        #region Properties
        
        /// <summary>
        /// 关卡ID
        /// </summary>
        int LevelId { get; }
        
        /// <summary>
        /// 关卡名称
        /// </summary>
        string LevelName { get; }
        
        /// <summary>
        /// 关卡配置
        /// </summary>
        ILevelConfig Config { get; }
        
        /// <summary>
        /// 关卡状态
        /// </summary>
        LevelState State { get; }
        
        /// <summary>
        /// 关卡进度 (0~1)
        /// </summary>
        float Progress { get; }
        
        /// <summary>
        /// 关卡运行时数据
        /// </summary>
        ILevelData Data { get; }
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// 加载时
        /// </summary>
        void OnLoad();
        
        /// <summary>
        /// 开始时
        /// </summary>
        void OnStart();
        
        /// <summary>
        /// 更新
        /// </summary>
        void OnUpdate(float deltaTime);
        
        /// <summary>
        /// 暂停时
        /// </summary>
        void OnPause();
        
        /// <summary>
        /// 恢复时
        /// </summary>
        void OnResume();
        
        /// <summary>
        /// 完成时
        /// </summary>
        void OnComplete(LevelResult result);
        
        /// <summary>
        /// 卸载时
        /// </summary>
        void OnUnload();
        
        #endregion
        
        #region Control
        
        /// <summary>
        /// 开始关卡
        /// </summary>
        void Start();
        
        /// <summary>
        /// 完成关卡
        /// </summary>
        void Complete(LevelResult result);
        
        /// <summary>
        /// 关卡失败
        /// </summary>
        void Fail(string reason);
        
        /// <summary>
        /// 重置关卡
        /// </summary>
        void Reset();
        
        #endregion
        
        #region Objectives
        
        /// <summary>
        /// 获取所有目标
        /// </summary>
        IReadOnlyList<ILevelObjective> GetObjectives();
        
        /// <summary>
        /// 完成目标
        /// </summary>
        void CompleteObjective(string objectiveId);
        
        /// <summary>
        /// 获取目标进度
        /// </summary>
        float GetObjectiveProgress(string objectiveId);
        
        /// <summary>
        /// 是否所有目标完成
        /// </summary>
        bool AreAllObjectivesComplete();
        
        #endregion
    }
}

