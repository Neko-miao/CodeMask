// ================================================
// GameFramework - 单局上下文接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Session
{
    /// <summary>
    /// 单局上下文接口
    /// </summary>
    public interface ISessionContext
    {
        /// <summary>
        /// 单局ID
        /// </summary>
        string SessionId { get; }
        
        /// <summary>
        /// 开始时间
        /// </summary>
        float StartTime { get; }
        
        /// <summary>
        /// 已进行时间
        /// </summary>
        float ElapsedTime { get; }
        
        /// <summary>
        /// 当前关卡ID
        /// </summary>
        int CurrentLevelId { get; }
        
        /// <summary>
        /// 关卡进度 (0~1)
        /// </summary>
        float LevelProgress { get; }
        
        /// <summary>
        /// 当前分数
        /// </summary>
        int Score { get; set; }
        
        /// <summary>
        /// 最高连击数
        /// </summary>
        int MaxCombo { get; set; }
        
        /// <summary>
        /// 收集物数量
        /// </summary>
        int CollectedCount { get; set; }
        
        /// <summary>
        /// 击杀数
        /// </summary>
        int KillCount { get; set; }
        
        /// <summary>
        /// 死亡次数
        /// </summary>
        int DeathCount { get; set; }
        
        /// <summary>
        /// 获取自定义数据
        /// </summary>
        T GetData<T>(string key);
        
        /// <summary>
        /// 设置自定义数据
        /// </summary>
        void SetData<T>(string key, T value);
        
        /// <summary>
        /// 是否有自定义数据
        /// </summary>
        bool HasData(string key);
        
        /// <summary>
        /// 移除自定义数据
        /// </summary>
        void RemoveData(string key);
        
        /// <summary>
        /// 重置上下文
        /// </summary>
        void Reset();
    }
}

