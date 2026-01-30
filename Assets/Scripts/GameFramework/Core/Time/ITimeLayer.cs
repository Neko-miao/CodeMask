// ================================================
// GameFramework - 时间层接口
// ================================================

namespace GameFramework.Core
{
    /// <summary>
    /// 时间层接口
    /// </summary>
    public interface ITimeLayer
    {
        /// <summary>
        /// 时间层名称
        /// </summary>
        string Name { get; }
        
        /// <summary>
        /// 时间缩放
        /// </summary>
        float TimeScale { get; set; }
        
        /// <summary>
        /// 是否暂停
        /// </summary>
        bool IsPaused { get; set; }
        
        /// <summary>
        /// 获取缩放后的DeltaTime
        /// </summary>
        float GetDeltaTime(float baseDeltaTime);
        
        /// <summary>
        /// 获取缩放后的FixedDeltaTime
        /// </summary>
        float GetFixedDeltaTime(float baseFixedDeltaTime);
    }
}

