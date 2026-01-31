// ================================================
// MaskSystem - 对外API接口定义
// ================================================

using System;
using System.Collections.Generic;

namespace Game.MaskSystem
{
    /// <summary>
    /// 面具系统对外API接口
    /// 其他模块通过此接口与面具系统交互，实现完全解耦
    /// </summary>
    public interface IMaskSystemAPI
    {
        #region 玩家状态查询

        /// <summary>
        /// 获取当前佩戴的面具
        /// </summary>
        MaskType GetCurrentMask();

        /// <summary>
        /// 获取当前面具槽位索引
        /// </summary>
        int GetCurrentSlot();

        /// <summary>
        /// 获取拥有的所有面具
        /// </summary>
        IReadOnlyList<MaskType> GetOwnedMasks();

        /// <summary>
        /// 获取玩家当前血量
        /// </summary>
        int GetPlayerHealth();

        /// <summary>
        /// 获取玩家最大血量
        /// </summary>
        int GetPlayerMaxHealth();

        /// <summary>
        /// 玩家是否存活
        /// </summary>
        bool IsPlayerAlive { get; }

        #endregion

        #region 敌人状态查询

        /// <summary>
        /// 获取当前敌人的面具类型
        /// </summary>
        MaskType GetEnemyMask();

        /// <summary>
        /// 获取敌人当前血量
        /// </summary>
        int GetEnemyHealth();

        /// <summary>
        /// 获取敌人最大血量
        /// </summary>
        int GetEnemyMaxHealth();

        /// <summary>
        /// 获取敌人名称
        /// </summary>
        string GetEnemyName();

        /// <summary>
        /// 敌人是否存活
        /// </summary>
        bool IsEnemyAlive { get; }

        /// <summary>
        /// 是否有敌人
        /// </summary>
        bool HasEnemy { get; }

        #endregion

        #region 面具操作

        /// <summary>
        /// 切换面具到指定槽位 (0=Q, 1=W, 2=E)
        /// </summary>
        /// <param name="slot">槽位索引</param>
        /// <returns>是否切换成功</returns>
        bool SwitchMask(int slot);

        /// <summary>
        /// 获取面具定义
        /// </summary>
        MaskDefinition GetMaskDefinition(MaskType maskType);

        #endregion

        #region 战斗指令

        /// <summary>
        /// 玩家攻击敌人
        /// </summary>
        /// <returns>战斗结果</returns>
        CombatResult PlayerAttack();

        /// <summary>
        /// 敌人攻击玩家
        /// </summary>
        /// <returns>战斗结果</returns>
        CombatResult EnemyAttack();

        /// <summary>
        /// 强制击败当前敌人（获得面具）
        /// </summary>
        void DefeatCurrentEnemy();

        /// <summary>
        /// 生成新敌人
        /// </summary>
        /// <param name="maskType">面具类型</param>
        /// <param name="health">血量</param>
        /// <param name="attackPower">攻击力</param>
        void SpawnEnemy(MaskType maskType, int health, int attackPower);

        /// <summary>
        /// 根据面具类型生成敌人（使用预设配置）
        /// </summary>
        void SpawnEnemyByType(MaskType maskType);

        #endregion

        #region 玩家操作

        /// <summary>
        /// 玩家恢复血量
        /// </summary>
        void HealPlayer(int amount);

        /// <summary>
        /// 玩家受到伤害
        /// </summary>
        void DamagePlayer(int amount);

        /// <summary>
        /// 重置玩家状态
        /// </summary>
        void ResetPlayer();

        #endregion

        #region 系统操作

        /// <summary>
        /// 初始化系统
        /// </summary>
        void Initialize();

        /// <summary>
        /// 获取战斗状态摘要
        /// </summary>
        string GetBattleStatus();

        #endregion

        #region 事件

        /// <summary>
        /// 面具改变事件 (旧面具, 新面具)
        /// </summary>
        event Action<MaskType, MaskType> OnMaskChanged;

        /// <summary>
        /// 玩家血量改变事件 (旧值, 新值)
        /// </summary>
        event Action<int, int> OnPlayerHealthChanged;

        /// <summary>
        /// 敌人血量改变事件 (旧值, 新值)
        /// </summary>
        event Action<int, int> OnEnemyHealthChanged;

        /// <summary>
        /// 获得面具事件
        /// </summary>
        event Action<MaskType> OnMaskAcquired;

        /// <summary>
        /// 敌人被击败事件
        /// </summary>
        event Action OnEnemyDefeated;

        /// <summary>
        /// 玩家被击败事件
        /// </summary>
        event Action OnPlayerDefeated;

        /// <summary>
        /// 敌人生成事件
        /// </summary>
        event Action<MaskType> OnEnemySpawned;

        /// <summary>
        /// 玩家攻击事件
        /// </summary>
        event Action<CombatResult> OnPlayerAttacked;

        /// <summary>
        /// 敌人攻击事件
        /// </summary>
        event Action<CombatResult> OnEnemyAttacked;

        #endregion
    }
}

