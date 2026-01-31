// ================================================
// Game - 玩家控制器接口
// ================================================

using System;
using UnityEngine;
using GameConfigs;

namespace Game.Modules
{
    /// <summary>
    /// 玩家状态
    /// </summary>
    public enum PlayerState
    {
        None,
        Idle,
        Moving,
        Jumping,
        Falling,
        Attacking,
        Hurt,
        Dead,
        Dashing
    }

    /// <summary>
    /// 玩家运行时属性
    /// </summary>
    [Serializable]
    public class PlayerRuntimeStats
    {
        /// <summary>
        /// 当前生命值
        /// </summary>
        public int CurrentHealth;

        /// <summary>
        /// 最大生命值
        /// </summary>
        public int MaxHealth;

        /// <summary>
        /// 当前攻击力
        /// </summary>
        public int AttackPower;

        /// <summary>
        /// 当前防御力
        /// </summary>
        public int Defense;

        /// <summary>
        /// 当前移动速度
        /// </summary>
        public float MoveSpeed;

        /// <summary>
        /// 是否无敌
        /// </summary>
        public bool IsInvincible;

        /// <summary>
        /// 无敌剩余时间
        /// </summary>
        public float InvincibleTimeRemaining;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive => CurrentHealth > 0;

        /// <summary>
        /// 生命值百分比
        /// </summary>
        public float HealthPercent => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

        /// <summary>
        /// 从基础属性初始化
        /// </summary>
        public void InitFromBaseStats(PlayerBaseStats baseStats)
        {
            MaxHealth = baseStats.maxHealth;
            CurrentHealth = MaxHealth;
            AttackPower = baseStats.attackPower;
            Defense = baseStats.defense;
            MoveSpeed = baseStats.moveSpeed;
            IsInvincible = false;
            InvincibleTimeRemaining = 0f;
        }

        /// <summary>
        /// 重置为满血状态
        /// </summary>
        public void Reset()
        {
            CurrentHealth = MaxHealth;
            IsInvincible = false;
            InvincibleTimeRemaining = 0f;
        }
    }

    /// <summary>
    /// 玩家控制器接口
    /// </summary>
    public interface IPlayerController
    {
        #region 基本属性

        /// <summary>
        /// 玩家GameObject
        /// </summary>
        GameObject GameObject { get; }

        /// <summary>
        /// 玩家Transform
        /// </summary>
        Transform Transform { get; }

        /// <summary>
        /// 角色配置数据
        /// </summary>
        PlayerCharacterData CharacterData { get; }

        /// <summary>
        /// 当前状态
        /// </summary>
        PlayerState State { get; }

        /// <summary>
        /// 运行时属性
        /// </summary>
        PlayerRuntimeStats RuntimeStats { get; }

        /// <summary>
        /// 是否激活
        /// </summary>
        bool IsActive { get; }

        #endregion

        #region 位置相关

        /// <summary>
        /// 当前位置
        /// </summary>
        Vector3 Position { get; set; }

        /// <summary>
        /// 当前旋转
        /// </summary>
        Quaternion Rotation { get; set; }

        /// <summary>
        /// 朝向（1=右，-1=左）
        /// </summary>
        int FacingDirection { get; }

        /// <summary>
        /// 是否在地面上
        /// </summary>
        bool IsGrounded { get; }

        #endregion

        #region 生命值相关

        /// <summary>
        /// 当前血量
        /// </summary>
        int CurrentHealth { get; }

        /// <summary>
        /// 最大血量
        /// </summary>
        int MaxHealth { get; }

        /// <summary>
        /// 血量百分比 (0-1)
        /// </summary>
        float HealthPercent { get; }

        /// <summary>
        /// 是否存活
        /// </summary>
        bool IsAlive { get; }

        /// <summary>
        /// 是否无敌
        /// </summary>
        bool IsInvincible { get; }

        /// <summary>
        /// 受到伤害
        /// </summary>
        /// <param name="damage">伤害值</param>
        /// <param name="source">伤害来源</param>
        void TakeDamage(int damage, GameObject source = null);

        /// <summary>
        /// 治疗
        /// </summary>
        /// <param name="amount">治疗量</param>
        void Heal(int amount);

        /// <summary>
        /// 设置血量
        /// </summary>
        void SetHealth(int health);

        /// <summary>
        /// 设置最大血量
        /// </summary>
        void SetMaxHealth(int maxHealth, bool healToFull = false);

        /// <summary>
        /// 设置无敌状态
        /// </summary>
        void SetInvincible(bool invincible, float duration = 0f);

        #endregion

        #region 移动相关

        /// <summary>
        /// 移动输入
        /// </summary>
        void Move(Vector2 input);

        /// <summary>
        /// 移动到指定位置
        /// </summary>
        void MoveTo(Vector3 targetPosition);

        /// <summary>
        /// 跳跃
        /// </summary>
        void Jump();

        /// <summary>
        /// 冲刺
        /// </summary>
        void Dash();

        /// <summary>
        /// 传送到指定位置
        /// </summary>
        void Teleport(Vector3 position);

        /// <summary>
        /// 设置移动速度
        /// </summary>
        void SetMoveSpeed(float speed);

        #endregion

        #region 动画相关

        /// <summary>
        /// 播放动画
        /// </summary>
        /// <param name="animationType">动画类型</param>
        /// <param name="forceRestart">是否强制重新播放</param>
        void PlayAnimation(PlayerAnimationType animationType, bool forceRestart = false);

        /// <summary>
        /// 获取当前动画类型
        /// </summary>
        PlayerAnimationType CurrentAnimationType { get; }

        /// <summary>
        /// 设置动画速度
        /// </summary>
        void SetAnimationSpeed(float speed);

        /// <summary>
        /// 暂停动画
        /// </summary>
        void PauseAnimation();

        /// <summary>
        /// 恢复动画
        /// </summary>
        void ResumeAnimation();

        #endregion

        #region 战斗相关

        /// <summary>
        /// 攻击
        /// </summary>
        void Attack();

        /// <summary>
        /// 使用技能
        /// </summary>
        void UseSkill(int skillIndex);

        #endregion

        #region 生命周期

        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize(PlayerCharacterData characterData);

        /// <summary>
        /// 重置
        /// </summary>
        void Reset();

        /// <summary>
        /// 激活
        /// </summary>
        void Activate();

        /// <summary>
        /// 停用
        /// </summary>
        void Deactivate();

        /// <summary>
        /// 销毁
        /// </summary>
        void Destroy();

        #endregion

        #region 事件

        /// <summary>
        /// 状态改变事件
        /// </summary>
        event Action<PlayerState, PlayerState> OnStateChanged;

        /// <summary>
        /// 血量改变事件
        /// </summary>
        event Action<int, int> OnHealthChanged;

        /// <summary>
        /// 受伤事件
        /// </summary>
        event Action<int, GameObject> OnDamaged;

        /// <summary>
        /// 死亡事件
        /// </summary>
        event Action OnDeath;

        /// <summary>
        /// 治疗事件
        /// </summary>
        event Action<int> OnHealed;

        /// <summary>
        /// 位置改变事件
        /// </summary>
        event Action<Vector3> OnPositionChanged;

        /// <summary>
        /// 动画改变事件
        /// </summary>
        event Action<PlayerAnimationType> OnAnimationChanged;

        #endregion
    }
}
