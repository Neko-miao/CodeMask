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
}

