// ================================================
// Game - 怪物实体类
// ================================================

using System;
using UnityEngine;
using GameConfigs;

namespace Game.Monsters
{
    /// <summary>
    /// 怪物状态
    /// </summary>
    public enum MonsterState
    {
        None,
        Idle,
        Attacking,
        Damaged,
        Dead
    }

    /// <summary>
    /// 怪物实体 - 附加到怪物GameObject上的组件
    /// </summary>
    public class Monster : MonoBehaviour
    {
        #region Fields

        [Header("怪物配置")]
        [Tooltip("怪物类型")]
        [SerializeField] private MonsterType _monsterType;

        [Header("运行时状态")]
        [SerializeField] private int _currentHealth;
        [SerializeField] private MonsterState _state = MonsterState.None;

        private MonsterData _monsterData;
        private int _instanceId;
        private Animation _animationComponent;
        private Animator _animator;

        #endregion

        #region Properties

        /// <summary>
        /// 怪物类型
        /// </summary>
        public MonsterType MonsterType => _monsterType;

        /// <summary>
        /// 怪物数据
        /// </summary>
        public MonsterData MonsterData => _monsterData;

        /// <summary>
        /// 怪物实例ID
        /// </summary>
        public int InstanceId => _instanceId;

        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHealth => _currentHealth;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth => _monsterData?.health ?? 100;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive => _currentHealth > 0 && _state != MonsterState.Dead;

        /// <summary>
        /// 当前状态
        /// </summary>
        public MonsterState State => _state;

        /// <summary>
        /// 怪物名称
        /// </summary>
        public string MonsterName => _monsterData?.monsterName ?? _monsterType.ToString();

        #endregion

        #region Events

        /// <summary>
        /// 怪物死亡事件
        /// </summary>
        public event Action<Monster> OnDeath;

        /// <summary>
        /// 怪物受伤事件
        /// </summary>
        public event Action<Monster, int> OnDamaged;

        /// <summary>
        /// 状态变化事件
        /// </summary>
        public event Action<Monster, MonsterState, MonsterState> OnStateChanged;

        #endregion

        #region Initialization

        /// <summary>
        /// 初始化怪物
        /// </summary>
        public void Initialize(MonsterType monsterType, int instanceId)
        {
            _monsterType = monsterType;
            _instanceId = instanceId;

            // 从配置获取怪物数据
            _monsterData = ConfigManager.GetMonster(monsterType);
            
            if (_monsterData == null)
            {
                Debug.LogWarning($"[Monster] MonsterData not found for type: {monsterType}");
                _currentHealth = 100;
            }
            else
            {
                _currentHealth = _monsterData.health;
            }

            // 获取动画组件
            _animationComponent = GetComponent<Animation>();
            _animator = GetComponent<Animator>();

            ChangeState(MonsterState.Idle);

            Debug.Log($"[Monster] Initialized: {MonsterName} (ID: {instanceId}, HP: {_currentHealth})");
        }

        /// <summary>
        /// 使用已有的MonsterData初始化
        /// </summary>
        public void Initialize(MonsterData monsterData, int instanceId)
        {
            _monsterData = monsterData;
            _monsterType = monsterData?.monsterType ?? MonsterType.None;
            _instanceId = instanceId;
            _currentHealth = monsterData?.health ?? 100;

            // 获取动画组件
            _animationComponent = GetComponent<Animation>();
            _animator = GetComponent<Animator>();

            ChangeState(MonsterState.Idle);

            Debug.Log($"[Monster] Initialized with data: {MonsterName} (ID: {instanceId}, HP: {_currentHealth})");
        }

        #endregion

        #region Combat

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(int damage)
        {
            if (!IsAlive) return;

            int actualDamage = Mathf.Max(0, damage - (_monsterData?.defense ?? 0));
            _currentHealth = Mathf.Max(0, _currentHealth - actualDamage);

            Debug.Log($"[Monster] {MonsterName} took {actualDamage} damage, HP: {_currentHealth}/{MaxHealth}");

            OnDamaged?.Invoke(this, actualDamage);

            if (_currentHealth <= 0)
            {
                Die();
            }
            else
            {
                ChangeState(MonsterState.Damaged);
                // 受伤后回到Idle状态
                Invoke(nameof(ReturnToIdle), 0.5f);
            }
        }

        /// <summary>
        /// 执行攻击
        /// </summary>
        public int PerformAttack()
        {
            if (!IsAlive) return 0;

            ChangeState(MonsterState.Attacking);
            
            int damage = _monsterData?.attack ?? 10;
            
            // 播放攻击动画
            PlayAnimation("Attack");

            // 攻击后回到Idle状态
            Invoke(nameof(ReturnToIdle), 0.5f);

            return damage;
        }

        /// <summary>
        /// 死亡
        /// </summary>
        private void Die()
        {
            if (_state == MonsterState.Dead) return;

            ChangeState(MonsterState.Dead);
            
            Debug.Log($"[Monster] {MonsterName} died!");

            // 播放死亡动画
            PlayAnimation("Death");

            OnDeath?.Invoke(this);
        }

        /// <summary>
        /// 返回Idle状态
        /// </summary>
        private void ReturnToIdle()
        {
            if (IsAlive && _state != MonsterState.Idle)
            {
                ChangeState(MonsterState.Idle);
            }
        }

        #endregion

        #region Animation

        /// <summary>
        /// 播放动画
        /// </summary>
        private void PlayAnimation(string animName)
        {
            if (_animator != null)
            {
                _animator.Play(animName);
            }
            else if (_animationComponent != null)
            {
                _animationComponent.Play(animName);
            }
        }

        #endregion

        #region State Management

        /// <summary>
        /// 改变状态
        /// </summary>
        private void ChangeState(MonsterState newState)
        {
            if (_state == newState) return;

            var oldState = _state;
            _state = newState;

            OnStateChanged?.Invoke(this, oldState, newState);
        }

        #endregion

        #region Rewards

        /// <summary>
        /// 获取击杀奖励分数
        /// </summary>
        public int GetScoreReward() => _monsterData?.scoreReward ?? 10;

        /// <summary>
        /// 获取击杀奖励金币
        /// </summary>
        public int GetGoldReward() => _monsterData?.goldReward ?? 5;

        /// <summary>
        /// 获取击杀奖励经验
        /// </summary>
        public int GetExpReward() => _monsterData?.expReward ?? 10;

        /// <summary>
        /// 是否为Boss
        /// </summary>
        public bool IsBoss => _monsterData?.isBoss ?? false;

        #endregion

        #region Cleanup

        /// <summary>
        /// 销毁怪物
        /// </summary>
        public void DestroyMonster()
        {
            // 取消所有延迟调用
            CancelInvoke();

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            // 清理事件
            OnDeath = null;
            OnDamaged = null;
            OnStateChanged = null;
        }

        #endregion
    }
}
