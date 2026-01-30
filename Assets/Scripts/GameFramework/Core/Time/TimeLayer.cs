// ================================================
// GameFramework - 时间层实现
// ================================================

namespace GameFramework.Core
{
    /// <summary>
    /// 时间层实现
    /// </summary>
    public class TimeLayer : ITimeLayer
    {
        /// <summary>
        /// 时间层名称
        /// </summary>
        public string Name { get; }
        
        /// <summary>
        /// 时间缩放
        /// </summary>
        public float TimeScale { get; set; } = 1f;
        
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused { get; set; } = false;
        
        public TimeLayer(string name)
        {
            Name = name;
        }
        
        /// <summary>
        /// 获取缩放后的DeltaTime
        /// </summary>
        public float GetDeltaTime(float baseDeltaTime)
        {
            if (IsPaused) return 0f;
            return baseDeltaTime * TimeScale;
        }
        
        /// <summary>
        /// 获取缩放后的FixedDeltaTime
        /// </summary>
        public float GetFixedDeltaTime(float baseFixedDeltaTime)
        {
            if (IsPaused) return 0f;
            return baseFixedDeltaTime * TimeScale;
        }
    }
}

