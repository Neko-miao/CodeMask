// ================================================
// Game - 怪物管理器接口
// ================================================

using System;
using System.Collections.Generic;
using GameConfigs;
using GameFramework.Core;

namespace Game.Monsters
{
    /// <summary>
    /// 怪物管理器接口
    /// </summary>
    public interface IMonsterMgr : IGameComponent
    {
        #region Properties

        /// <summary>
        /// 当前存活的怪物数量
        /// </summary>
        int AliveMonsterCount { get; }

        /// <summary>
        /// 当前存活的怪物列表
        /// </summary>
        IReadOnlyList<Monster> AliveMonsters { get; }

        /// <summary>
        /// 当前活跃的怪物（正在战斗的怪物）
        /// </summary>
        Monster CurrentMonster { get; }

        /// <summary>
        /// 是否有存活的怪物
        /// </summary>
        bool HasAliveMonster { get; }

        #endregion

        #region Events

        /// <summary>
        /// 怪物生成事件
        /// </summary>
        event Action<Monster> OnMonsterSpawned;

        /// <summary>
        /// 怪物死亡事件
        /// </summary>
        event Action<Monster> OnMonsterDied;

        /// <summary>
        /// 所有怪物被消灭事件
        /// </summary>
        event Action OnAllMonstersDefeated;

        #endregion

        #region Monster Creation

        /// <summary>
        /// 创建怪物
        /// </summary>
        /// <param name="monsterType">怪物类型</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">生成旋转</param>
        /// <returns>创建的怪物实例</returns>
        Monster CreateMonster(MonsterType monsterType, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation);

        /// <summary>
        /// 创建怪物（使用MonsterData）
        /// </summary>
        /// <param name="monsterData">怪物数据</param>
        /// <param name="position">生成位置</param>
        /// <param name="rotation">生成旋转</param>
        /// <returns>创建的怪物实例</returns>
        Monster CreateMonster(MonsterData monsterData, UnityEngine.Vector3 position, UnityEngine.Quaternion rotation);

        #endregion

        #region Monster Management

        /// <summary>
        /// 销毁指定怪物
        /// </summary>
        void DestroyMonster(Monster monster);

        /// <summary>
        /// 销毁所有怪物
        /// </summary>
        void DestroyAllMonsters();

        /// <summary>
        /// 根据实例ID获取怪物
        /// </summary>
        Monster GetMonster(int instanceId);

        /// <summary>
        /// 获取指定类型的所有怪物
        /// </summary>
        List<Monster> GetMonstersByType(MonsterType monsterType);

        #endregion

        #region Model Loading

        /// <summary>
        /// 加载怪物模型预制体
        /// </summary>
        /// <param name="monsterType">怪物类型</param>
        /// <returns>预制体GameObject</returns>
        UnityEngine.GameObject LoadMonsterPrefab(MonsterType monsterType);

        /// <summary>
        /// 加载怪物模型预制体（使用MonsterData）
        /// </summary>
        /// <param name="monsterData">怪物数据</param>
        /// <returns>预制体GameObject</returns>
        UnityEngine.GameObject LoadMonsterPrefab(MonsterData monsterData);

        #endregion
    }
}
