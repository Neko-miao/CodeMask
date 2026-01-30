// ================================================
// GameFramework - 单局状态定义
// ================================================

namespace GameFramework.Session
{
    /// <summary>
    /// 单局状态
    /// </summary>
    public enum SessionState
    {
        /// <summary>
        /// 未开始
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 准备中 (加载关卡、初始化规则)
        /// </summary>
        Preparing = 1,
        
        /// <summary>
        /// 进行中
        /// </summary>
        Running = 2,
        
        /// <summary>
        /// 暂停
        /// </summary>
        Paused = 3,
        
        /// <summary>
        /// 结算中
        /// </summary>
        Completing = 4,
        
        /// <summary>
        /// 已结束
        /// </summary>
        Ended = 5
    }
    
    /// <summary>
    /// 单局结束原因
    /// </summary>
    public enum SessionEndReason
    {
        /// <summary>
        /// 正常完成 (胜利)
        /// </summary>
        Completed = 0,
        
        /// <summary>
        /// 失败
        /// </summary>
        Failed = 1,
        
        /// <summary>
        /// 时间到
        /// </summary>
        TimeUp = 2,
        
        /// <summary>
        /// 生命耗尽
        /// </summary>
        AllLivesLost = 3,
        
        /// <summary>
        /// 玩家退出
        /// </summary>
        PlayerQuit = 4,
        
        /// <summary>
        /// 中断
        /// </summary>
        Aborted = 5
    }
    
    /// <summary>
    /// 关卡状态
    /// </summary>
    public enum LevelState
    {
        /// <summary>
        /// 未加载
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 加载中
        /// </summary>
        Loading = 1,
        
        /// <summary>
        /// 已加载
        /// </summary>
        Loaded = 2,
        
        /// <summary>
        /// 进行中
        /// </summary>
        Running = 3,
        
        /// <summary>
        /// 已完成
        /// </summary>
        Completed = 4,
        
        /// <summary>
        /// 已失败
        /// </summary>
        Failed = 5
    }
}

