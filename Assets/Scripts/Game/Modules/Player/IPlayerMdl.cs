// ================================================
// Game - 玩家模块接口
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;
using GameConfigs;

namespace Game.Modules
{
    /// <summary>
    /// 玩家账户数据
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
        public int SelectedCharacterId;
    }
    
    /// <summary>
    /// 玩家模块接口
    /// </summary>
    public interface IPlayerMdl : IGameComponent
    {
        #region 账户属性
        
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
        
        #endregion
        
        #region 主玩家角色
        
        /// <summary>
        /// 主玩家控制器（游戏中的玩家实体）
        /// </summary>
        IPlayerController MainPlayer { get; }
        
        /// <summary>
        /// 主玩家是否存在
        /// </summary>
        bool HasMainPlayer { get; }
        
        /// <summary>
        /// 主玩家是否存活
        /// </summary>
        bool IsMainPlayerAlive { get; }
        
        /// <summary>
        /// 主玩家位置
        /// </summary>
        Vector3 MainPlayerPosition { get; }
        
        /// <summary>
        /// 主玩家当前血量
        /// </summary>
        int MainPlayerHealth { get; }
        
        /// <summary>
        /// 主玩家最大血量
        /// </summary>
        int MainPlayerMaxHealth { get; }
        
        /// <summary>
        /// 主玩家当前模型ID
        /// </summary>
        int MainPlayerModelId { get; }
        
        /// <summary>
        /// 主玩家模型GameObject
        /// </summary>
        GameObject MainPlayerModel { get; }
        
        /// <summary>
        /// 生成主玩家
        /// </summary>
        /// <param name="characterId">角色ID，-1使用默认角色</param>
        /// <param name="spawnPosition">生成位置，null使用配置默认位置或场景生成点</param>
        /// <returns>玩家控制器</returns>
        IPlayerController SpawnMainPlayer(int characterId = -1, Vector3? spawnPosition = null);
        
        /// <summary>
        /// 销毁主玩家
        /// </summary>
        void DestroyMainPlayer();
        
        /// <summary>
        /// 重生主玩家
        /// </summary>
        /// <param name="spawnPosition">重生位置，null使用默认位置</param>
        void RespawnMainPlayer(Vector3? spawnPosition = null);
        
        #endregion
        
        #region 玩家模型管理
        
        /// <summary>
        /// 加载玩家预制体
        /// </summary>
        /// <param name="characterId">角色ID，-1使用默认角色</param>
        /// <returns>玩家预制体</returns>
        GameObject LoadPlayerPrefab(int characterId = -1);
        
        /// <summary>
        /// 加载玩家模型预制体
        /// </summary>
        /// <param name="characterId">角色ID，-1使用默认角色</param>
        /// <param name="modelId">模型ID，-1使用默认模型</param>
        /// <returns>模型预制体</returns>
        GameObject LoadPlayerModelPrefab(int characterId = -1, int modelId = -1);
        
        /// <summary>
        /// 实例化玩家预制体
        /// </summary>
        /// <param name="characterId">角色ID，-1使用默认角色</param>
        /// <param name="position">位置</param>
        /// <param name="rotation">旋转</param>
        /// <param name="parent">父物体</param>
        /// <returns>实例化的玩家GameObject</returns>
        GameObject InstantiatePlayerPrefab(int characterId = -1, Vector3? position = null, Quaternion? rotation = null, Transform parent = null);
        
        /// <summary>
        /// 实例化玩家模型
        /// </summary>
        /// <param name="characterId">角色ID，-1使用默认角色</param>
        /// <param name="modelId">模型ID，-1使用默认模型</param>
        /// <param name="parent">父物体</param>
        /// <returns>实例化的模型GameObject</returns>
        GameObject InstantiatePlayerModel(int characterId = -1, int modelId = -1, Transform parent = null);
        
        /// <summary>
        /// 切换主玩家模型
        /// </summary>
        /// <param name="modelId">模型ID</param>
        /// <returns>切换是否成功</returns>
        bool ChangeMainPlayerModel(int modelId);
        
        /// <summary>
        /// 获取角色的所有可用模型
        /// </summary>
        /// <param name="characterId">角色ID，-1使用当前选中角色</param>
        /// <returns>模型数据列表</returns>
        IReadOnlyList<PlayerModelData> GetAvailableModels(int characterId = -1);
        
        /// <summary>
        /// 获取角色的默认模型数据
        /// </summary>
        /// <param name="characterId">角色ID，-1使用当前选中角色</param>
        /// <returns>默认模型数据</returns>
        PlayerModelData GetDefaultModel(int characterId = -1);
        
        /// <summary>
        /// 根据模型ID获取模型数据
        /// </summary>
        /// <param name="characterId">角色ID，-1使用当前选中角色</param>
        /// <param name="modelId">模型ID</param>
        /// <returns>模型数据</returns>
        PlayerModelData GetModelData(int characterId, int modelId);
        
        #endregion
        
        #region 账户操作
        
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
        /// 选择角色
        /// </summary>
        void SelectCharacter(int characterId);
        
        #endregion
        
        #region 配置访问
        
        /// <summary>
        /// 获取玩家配置
        /// </summary>
        PlayerConfig Config { get; }
        
        /// <summary>
        /// 获取角色数据
        /// </summary>
        PlayerCharacterData GetCharacterData(int characterId);
        
        /// <summary>
        /// 获取当前选中的角色数据
        /// </summary>
        PlayerCharacterData SelectedCharacterData { get; }
        
        #endregion
        
        #region 事件
        
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
        
        /// <summary>
        /// 主玩家生成事件
        /// </summary>
        event Action<IPlayerController> OnMainPlayerSpawned;
        
        /// <summary>
        /// 主玩家销毁事件
        /// </summary>
        event Action OnMainPlayerDestroyed;
        
        /// <summary>
        /// 主玩家死亡事件
        /// </summary>
        event Action OnMainPlayerDeath;
        
        /// <summary>
        /// 主玩家重生事件
        /// </summary>
        event Action<IPlayerController> OnMainPlayerRespawned;
        
        /// <summary>
        /// 主玩家血量改变事件
        /// </summary>
        event Action<int, int> OnMainPlayerHealthChanged;
        
        /// <summary>
        /// 角色选择改变事件
        /// </summary>
        event Action<int> OnCharacterSelected;
        
        /// <summary>
        /// 主玩家模型改变事件
        /// </summary>
        event Action<int, GameObject> OnMainPlayerModelChanged;
        
        #endregion
    }
}
