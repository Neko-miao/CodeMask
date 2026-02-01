// ================================================
// Game - 游戏事件定义
// ================================================

using UnityEngine;

namespace Game.Events
{
    #region Player Events
    
    /// <summary>
    /// 玩家伤害事件
    /// </summary>
    public struct PlayerDamageEvent
    {
        public int PlayerId;
        public float Damage;
        public Vector3 HitPosition;
    }
    
    /// <summary>
    /// 玩家死亡事件
    /// </summary>
    public struct PlayerDeadEvent
    {
        public int PlayerId;
    }
    
    /// <summary>
    /// 玩家复活事件
    /// </summary>
    public struct PlayerRespawnEvent
    {
        public int PlayerId;
        public Vector3 RespawnPosition;
    }
    
    #endregion
    
    #region Game Events
    
    /// <summary>
    /// 游戏开始事件
    /// </summary>
    public struct GameStartEvent
    {
        public int LevelId;
    }
    
    /// <summary>
    /// 游戏结束事件
    /// </summary>
    public struct GameEndEvent
    {
        public bool IsWin;
        public int Score;
        public float Time;
    }
    
    /// <summary>
    /// 游戏暂停事件
    /// </summary>
    public struct GamePauseEvent
    {
        public bool IsPaused;
    }
    
    #endregion
    
    #region Level Events
    
    /// <summary>
    /// 关卡开始事件
    /// </summary>
    public struct LevelStartEvent
    {
        public int LevelId;
        public string LevelName;
    }
    
    /// <summary>
    /// 关卡完成事件
    /// </summary>
    public struct LevelCompleteEvent
    {
        public int LevelId;
        public int Score;
        public int Stars;
    }
    
    /// <summary>
    /// 关卡失败事件
    /// </summary>
    public struct LevelFailedEvent
    {
        public int LevelId;
        public string Reason;
    }
    
    #endregion
    
    #region Combat Events
    
    /// <summary>
    /// 敌人击杀事件
    /// </summary>
    public struct EnemyKilledEvent
    {
        public int EnemyId;
        public int Score;
        public Vector3 Position;
    }
    
    /// <summary>
    /// 伤害事件
    /// </summary>
    public struct DamageEvent
    {
        public int SourceId;
        public int TargetId;
        public float Damage;
        public bool IsCritical;
    }
    
    #endregion
    
    #region Item Events
    
    /// <summary>
    /// 物品拾取事件
    /// </summary>
    public struct ItemPickupEvent
    {
        public int ItemId;
        public int Count;
    }
    
    /// <summary>
    /// 物品使用事件
    /// </summary>
    public struct ItemUseEvent
    {
        public int ItemId;
        public int Count;
    }
    
    #endregion
    
    #region Rhythm Events
    
    /// <summary>
    /// 节奏触发事件数据
    /// </summary>
    public struct RhythmTriggerEvent
    {
        /// <summary>
        /// 面具类型
        /// </summary>
        public MaskType MaskType;
        
        /// <summary>
        /// 节奏行为类型
        /// </summary>
        public RhythmActionType ActionType;
        
        /// <summary>
        /// 触发结果
        /// </summary>
        public RhythmScoreGrade Result;
        
        public RhythmTriggerEvent(MaskType maskType, RhythmActionType actionType, RhythmScoreGrade result)
        {
            MaskType = maskType;
            ActionType = actionType;
            Result = result;
        }
    }
    
    /// <summary>
    /// 节奏创建事件数据 - 当RhythmSystem创建新的Rhythm时触发
    /// </summary>
    public struct RhythmCreatedEvent
    {
        /// <summary>
        /// 创建的节奏实例
        /// </summary>
        public Rhythm Rhythm;
        
        /// <summary>
        /// 面具类型
        /// </summary>
        public MaskType MaskType;
        
        /// <summary>
        /// 节奏行为类型
        /// </summary>
        public RhythmActionType ActionType;
        
        /// <summary>
        /// 节奏到达判定区域第一个未命中点的时间（秒）
        /// </summary>
        public float TimeToReachJudgmentZone;
        
        /// <summary>
        /// 节奏到达完美判定区间中心的时间（秒）
        /// 敌人攻击动画应该在此时刻结束
        /// </summary>
        public float TimeToReachPerfectZone;
        
        /// <summary>
        /// 节奏的起始位置
        /// </summary>
        public Vector3 SpawnPosition;
        
        public RhythmCreatedEvent(Rhythm rhythm, MaskType maskType, RhythmActionType actionType, float timeToReachZone, float timeToReachPerfect, Vector3 spawnPos)
        {
            Rhythm = rhythm;
            MaskType = maskType;
            ActionType = actionType;
            TimeToReachJudgmentZone = timeToReachZone;
            TimeToReachPerfectZone = timeToReachPerfect;
            SpawnPosition = spawnPos;
        }
    }
    
    #endregion
}

