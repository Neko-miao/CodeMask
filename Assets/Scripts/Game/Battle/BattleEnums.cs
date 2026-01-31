// ================================================
// Game - 面具类型枚举
// ================================================

namespace Game.Battle
{
    /// <summary>
    /// 面具类型 - 基于策划案中的动物面具系统
    /// </summary>
    public enum MaskType
    {
        /// <summary>
        /// 无面具
        /// </summary>
        None = 0,
        
        /// <summary>
        /// 猫面具 - 闪避型
        /// </summary>
        Cat = 1,
        
        /// <summary>
        /// 蛇面具 - 攻击型
        /// </summary>
        Snake = 2,
        
        /// <summary>
        /// 熊面具 - 格挡型
        /// </summary>
        Bear = 3,
        
        /// <summary>
        /// 马面具 - 防守型
        /// </summary>
        Horse = 4,
        
        /// <summary>
        /// 牛面具 - 破防型
        /// </summary>
        Bull = 5,
        
        /// <summary>
        /// 鲸鱼面具 - 血厚
        /// </summary>
        Whale = 6,
        
        /// <summary>
        /// 鲨鱼面具 - 攻击力高
        /// </summary>
        Shark = 7,
        
        /// <summary>
        /// 龙面具 - 最终Boss
        /// </summary>
        Dragon = 8
    }
    
    /// <summary>
    /// 面具效果类型
    /// </summary>
    public enum MaskEffectType
    {
        /// <summary>
        /// 攻击 - 克制时双倍伤害、破防
        /// </summary>
        Attack,
        
        /// <summary>
        /// 闪避 - 一般卡点不会受到攻击伤害，完美卡点不会受到狂暴伤害
        /// </summary>
        Dodge,
        
        /// <summary>
        /// 格挡 - 格挡成功后，下一个卡点变成格挡反击，造成大量伤害
        /// </summary>
        Block,
        
        /// <summary>
        /// 回血 - 完美卡点恢复一点体力
        /// </summary>
        Heal
    }
    
    /// <summary>
    /// 节奏点类型
    /// </summary>
    public enum RhythmPointType
    {
        /// <summary>
        /// 进攻节奏点
        /// </summary>
        Attack,
        
        /// <summary>
        /// 防守节奏点
        /// </summary>
        Defend,
        
        /// <summary>
        /// 特殊节奏点 (狂暴等)
        /// </summary>
        Special
    }
    
    /// <summary>
    /// 卡点结果
    /// </summary>
    public enum HitResult
    {
        /// <summary>
        /// 未命中
        /// </summary>
        Miss,
        
        /// <summary>
        /// 一般卡点
        /// </summary>
        Normal,
        
        /// <summary>
        /// 完美卡点
        /// </summary>
        Perfect
    }
}
