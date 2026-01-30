// ================================================
// Game - 玩家模块接口
// ================================================

using System;
using GameFramework.Core;

namespace Game.Modules
{
    /// <summary>
    /// 玩家数据
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public int PlayerId;
        public string PlayerName;
        public int Level;
        public int Exp;
        public int ExpToNextLevel;
        public long Currency;
        public int VipLevel;
    }
    
    /// <summary>
    /// 玩家模块接口
    /// </summary>
    public interface IPlayerMdl : IGameComponent
    {
        /// <summary>
        /// 玩家ID
        /// </summary>
        int PlayerId { get; }
        
        /// <summary>
        /// 玩家名称
        /// </summary>
        string PlayerName { get; }
        
        /// <summary>
        /// 等级
        /// </summary>
        int Level { get; }
        
        /// <summary>
        /// 经验值
        /// </summary>
        int Exp { get; }
        
        /// <summary>
        /// 升级所需经验
        /// </summary>
        int ExpToNextLevel { get; }
        
        /// <summary>
        /// 货币
        /// </summary>
        long Currency { get; }
        
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        PlayerData GetPlayerData();
        
        /// <summary>
        /// 设置玩家数据
        /// </summary>
        void SetPlayerData(PlayerData data);
        
        /// <summary>
        /// 添加经验
        /// </summary>
        void AddExp(int amount);
        
        /// <summary>
        /// 添加货币
        /// </summary>
        void AddCurrency(long amount);
        
        /// <summary>
        /// 消耗货币
        /// </summary>
        bool ConsumeCurrency(long amount);
        
        /// <summary>
        /// 设置名称
        /// </summary>
        void SetName(string name);
        
        /// <summary>
        /// 经验改变事件
        /// </summary>
        event Action<int, int> OnExpChanged;
        
        /// <summary>
        /// 升级事件
        /// </summary>
        event Action<int> OnLevelUp;
        
        /// <summary>
        /// 货币改变事件
        /// </summary>
        event Action<long> OnCurrencyChanged;
    }
}

