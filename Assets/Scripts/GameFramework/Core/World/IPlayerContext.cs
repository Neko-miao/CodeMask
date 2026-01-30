// ================================================
// GameFramework - 玩家上下文接口
// ================================================

using System.Collections.Generic;
using GameFramework.Entity;

namespace GameFramework.World
{
    /// <summary>
    /// 玩家上下文接口 - 管理玩家实体
    /// </summary>
    public interface IPlayerContext
    {
        /// <summary>
        /// 本地玩家ID
        /// </summary>
        int LocalPlayerId { get; }
        
        /// <summary>
        /// 本地玩家实体
        /// </summary>
        IEntity LocalPlayer { get; }
        
        /// <summary>
        /// 玩家数量
        /// </summary>
        int PlayerCount { get; }
        
        /// <summary>
        /// 检查是否为本地玩家
        /// </summary>
        bool IsLocalPlayer(IEntity entity);
        
        /// <summary>
        /// 检查是否为本地玩家
        /// </summary>
        bool IsLocalPlayer(int entityId);
        
        /// <summary>
        /// 获取玩家实体
        /// </summary>
        IEntity GetPlayerEntity(int playerId);
        
        /// <summary>
        /// 获取所有玩家实体
        /// </summary>
        IReadOnlyList<IEntity> GetAllPlayerEntities();
        
        /// <summary>
        /// 设置本地玩家
        /// </summary>
        void SetLocalPlayer(IEntity player);
        
        /// <summary>
        /// 注册玩家
        /// </summary>
        void RegisterPlayer(int playerId, IEntity entity);
        
        /// <summary>
        /// 注销玩家
        /// </summary>
        void UnregisterPlayer(int playerId);
        
        /// <summary>
        /// 清空所有玩家
        /// </summary>
        void Clear();
    }
}
