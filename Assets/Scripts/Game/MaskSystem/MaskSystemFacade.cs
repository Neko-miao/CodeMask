// ================================================
// MaskSystem - 门面类（统一入口）
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 面具系统门面类 - 实现IMaskSystemAPI接口，提供统一的系统入口
    /// 外部调用者只需要通过此类与系统交互
    /// </summary>
    public class MaskSystemFacade : IMaskSystemAPI
    {
        #region 单例

        private static MaskSystemFacade _instance;
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static MaskSystemFacade Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MaskSystemFacade();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 创建新实例（非单例模式使用）
        /// </summary>
        public static MaskSystemFacade CreateNew(MaskSystemConfig config = null)
        {
            return new MaskSystemFacade(config);
        }

        #endregion

        #region 私有字段

        private CombatManager _combatManager;
        private MaskSystemConfig _config;
        private bool _initialized;

        #endregion

        #region 构造函数

        public MaskSystemFacade(MaskSystemConfig config = null)
        {
            _config = config;
            Initialize();
        }

        #endregion

        #region 初始化

        public void Initialize()
        {
            if (_initialized)
            {
                Debug.Log("[MaskSystemFacade] 已初始化，跳过");
                return;
            }

            _combatManager = new CombatManager(_config);

            // 订阅内部事件并转发
            SubscribeEvents();

            _initialized = true;
            Debug.Log("[MaskSystemFacade] 初始化完成");
        }

        private void SubscribeEvents()
        {
            if (_combatManager?.Player != null)
            {
                _combatManager.Player.OnHealthChanged += (oldVal, newVal) => OnPlayerHealthChanged?.Invoke(oldVal, newVal);
                _combatManager.Player.OnMaskChanged += (oldMask, newMask) => OnMaskChanged?.Invoke(oldMask, newMask);
                _combatManager.Player.OnMaskAcquired += (mask) => OnMaskAcquired?.Invoke(mask);
            }

            _combatManager.OnEnemyDefeated += (enemy) => OnEnemyDefeated?.Invoke();
            _combatManager.OnPlayerDefeated += () => OnPlayerDefeated?.Invoke();
            _combatManager.OnEnemySpawned += (enemy) =>
            {
                // 订阅新敌人的血量事件
                enemy.OnHealthChanged += (oldVal, newVal) => OnEnemyHealthChanged?.Invoke(oldVal, newVal);
                OnEnemySpawned?.Invoke(enemy.MaskType);
            };
            _combatManager.OnPlayerAttack += (result) => OnPlayerAttacked?.Invoke(result);
            _combatManager.OnEnemyAttack += (result) => OnEnemyAttacked?.Invoke(result);
        }

        #endregion

        #region 玩家状态查询

        public MaskType GetCurrentMask()
        {
            return _combatManager?.Player?.CurrentMask ?? MaskType.None;
        }

        public int GetCurrentSlot()
        {
            return _combatManager?.Player?.CurrentSlot ?? 0;
        }

        public IReadOnlyList<MaskType> GetOwnedMasks()
        {
            return _combatManager?.Player?.OwnedMasks ?? Array.Empty<MaskType>();
        }

        public int GetPlayerHealth()
        {
            return _combatManager?.Player?.CurrentHealth ?? 0;
        }

        public int GetPlayerMaxHealth()
        {
            return _combatManager?.Player?.MaxHealth ?? 0;
        }

        public bool IsPlayerAlive => _combatManager?.Player?.IsAlive ?? false;

        #endregion

        #region 敌人状态查询

        public MaskType GetEnemyMask()
        {
            return _combatManager?.CurrentEnemy?.MaskType ?? MaskType.None;
        }

        public int GetEnemyHealth()
        {
            return _combatManager?.CurrentEnemy?.CurrentHealth ?? 0;
        }

        public int GetEnemyMaxHealth()
        {
            return _combatManager?.CurrentEnemy?.MaxHealth ?? 0;
        }

        public string GetEnemyName()
        {
            return _combatManager?.CurrentEnemy?.Name ?? "无";
        }

        public bool IsEnemyAlive => _combatManager?.CurrentEnemy?.IsAlive ?? false;

        public bool HasEnemy => _combatManager?.CurrentEnemy != null;

        #endregion

        #region 面具操作

        public bool SwitchMask(int slot)
        {
            return _combatManager?.SwitchMask(slot) ?? false;
        }

        public MaskDefinition GetMaskDefinition(MaskType maskType)
        {
            return MaskRegistry.GetMask(maskType);
        }

        #endregion

        #region 战斗指令

        public CombatResult PlayerAttack()
        {
            return _combatManager?.PlayerAttack() ?? new CombatResult { Message = "系统未初始化" };
        }

        public CombatResult EnemyAttack()
        {
            return _combatManager?.EnemyAttack() ?? new CombatResult { Message = "系统未初始化" };
        }

        public void DefeatCurrentEnemy()
        {
            _combatManager?.DefeatCurrentEnemy();
        }

        public void SpawnEnemy(MaskType maskType, int health, int attackPower)
        {
            _combatManager?.SpawnEnemy(maskType, health, attackPower);
        }

        public void SpawnEnemyByType(MaskType maskType)
        {
            _combatManager?.SpawnEnemyByMaskType(maskType);
        }

        #endregion

        #region 玩家操作

        public void HealPlayer(int amount)
        {
            _combatManager?.Player?.Heal(amount);
        }

        public void DamagePlayer(int amount)
        {
            _combatManager?.Player?.TakeDamage(amount);
        }

        public void ResetPlayer()
        {
            _combatManager?.Player?.Reset();
        }

        #endregion

        #region 系统操作

        public string GetBattleStatus()
        {
            return _combatManager?.GetBattleStatus() ?? "系统未初始化";
        }

        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            _combatManager?.Dispose();
            _combatManager = null;
            _initialized = false;

            if (_instance == this)
            {
                _instance = null;
            }

            Debug.Log("[MaskSystemFacade] 已清理");
        }

        #endregion

        #region 事件

        public event Action<MaskType, MaskType> OnMaskChanged;
        public event Action<int, int> OnPlayerHealthChanged;
        public event Action<int, int> OnEnemyHealthChanged;
        public event Action<MaskType> OnMaskAcquired;
        public event Action OnEnemyDefeated;
        public event Action OnPlayerDefeated;
        public event Action<MaskType> OnEnemySpawned;
        public event Action<CombatResult> OnPlayerAttacked;
        public event Action<CombatResult> OnEnemyAttacked;

        #endregion
    }
}

