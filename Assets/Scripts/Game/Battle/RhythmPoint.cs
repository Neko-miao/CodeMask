// ================================================
// Game - 节奏点数据
// ================================================

using System;

namespace Game.Battle
{
    /// <summary>
    /// 节奏点数据
    /// </summary>
    [Serializable]
    public class RhythmPoint
    {
        /// <summary>
        /// 节奏点类型
        /// </summary>
        public RhythmPointType Type;
        
        /// <summary>
        /// 到达时间
        /// </summary>
        public float Time;
        
        /// <summary>
        /// 是否已处理
        /// </summary>
        public bool IsProcessed;
        
        /// <summary>
        /// 完美卡点时间窗口 (前后多少秒内算完美)
        /// </summary>
        public const float PERFECT_WINDOW = 0.05f;
        
        /// <summary>
        /// 一般卡点时间窗口
        /// </summary>
        public const float NORMAL_WINDOW = 0.15f;
        
        public RhythmPoint(RhythmPointType type, float time)
        {
            Type = type;
            Time = time;
            IsProcessed = false;
        }
        
        /// <summary>
        /// 判断卡点结果
        /// </summary>
        public HitResult GetHitResult(float inputTime)
        {
            float diff = Math.Abs(inputTime - Time);
            
            if (diff <= PERFECT_WINDOW)
                return HitResult.Perfect;
            else if (diff <= NORMAL_WINDOW)
                return HitResult.Normal;
            else
                return HitResult.Miss;
        }
    }
}
