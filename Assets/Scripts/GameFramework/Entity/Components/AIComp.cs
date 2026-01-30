// ================================================
// GameFramework - AI组件 (纯数据)
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// AI状态
    /// </summary>
    public enum AIState
    {
        Idle = 0,
        Patrol = 1,
        Chase = 2,
        Attack = 3,
        Flee = 4,
        Dead = 5,
        Custom = 100
    }
    
    /// <summary>
    /// AI组件 - 存储AI行为数据
    /// </summary>
    [Serializable]
    public class AIComp : EntityComp<AIComp>
    {
        /// <summary>
        /// 当前AI状态
        /// </summary>
        public AIState CurrentState = AIState.Idle;
        
        /// <summary>
        /// 上一个状态
        /// </summary>
        public AIState PreviousState;
        
        /// <summary>
        /// 状态进入时间
        /// </summary>
        public float StateEnterTime;
        
        /// <summary>
        /// 目标实体ID
        /// </summary>
        public int TargetEntityId = -1;
        
        /// <summary>
        /// 目标位置
        /// </summary>
        public Vector3 TargetPosition;
        
        /// <summary>
        /// 起始位置 (巡逻点)
        /// </summary>
        public Vector3 HomePosition;
        
        /// <summary>
        /// 巡逻范围
        /// </summary>
        public float PatrolRadius = 10f;
        
        /// <summary>
        /// 警觉范围
        /// </summary>
        public float AlertRange = 15f;
        
        /// <summary>
        /// 攻击范围
        /// </summary>
        public float AttackRange = 2f;
        
        /// <summary>
        /// 视野角度
        /// </summary>
        public float ViewAngle = 120f;
        
        /// <summary>
        /// 攻击冷却时间
        /// </summary>
        public float AttackCooldown = 1f;
        
        /// <summary>
        /// 上次攻击时间
        /// </summary>
        public float LastAttackTime;
        
        /// <summary>
        /// 警觉度 (0~1)
        /// </summary>
        public float Alertness;
        
        /// <summary>
        /// 自定义状态ID
        /// </summary>
        public int CustomStateId;
        
        /// <summary>
        /// 状态持续时间
        /// </summary>
        public float StateDuration => Time.time - StateEnterTime;
        
        /// <summary>
        /// 是否可以攻击
        /// </summary>
        public bool CanAttack => Time.time >= LastAttackTime + AttackCooldown;
        
        /// <summary>
        /// 是否有目标
        /// </summary>
        public bool HasTarget => TargetEntityId >= 0;
        
        public override void Reset()
        {
            CurrentState = AIState.Idle;
            PreviousState = AIState.Idle;
            StateEnterTime = 0f;
            TargetEntityId = -1;
            TargetPosition = Vector3.zero;
            HomePosition = Vector3.zero;
            PatrolRadius = 10f;
            AlertRange = 15f;
            AttackRange = 2f;
            ViewAngle = 120f;
            AttackCooldown = 1f;
            LastAttackTime = 0f;
            Alertness = 0f;
            CustomStateId = 0;
        }
        
        public override IEntityComp Clone()
        {
            return new AIComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                CurrentState = CurrentState,
                PreviousState = PreviousState,
                StateEnterTime = StateEnterTime,
                TargetEntityId = TargetEntityId,
                TargetPosition = TargetPosition,
                HomePosition = HomePosition,
                PatrolRadius = PatrolRadius,
                AlertRange = AlertRange,
                AttackRange = AttackRange,
                ViewAngle = ViewAngle,
                AttackCooldown = AttackCooldown,
                LastAttackTime = LastAttackTime,
                Alertness = Alertness,
                CustomStateId = CustomStateId
            };
        }
        
        /// <summary>
        /// 切换状态
        /// </summary>
        public void SetState(AIState newState)
        {
            if (CurrentState == newState) return;
            
            PreviousState = CurrentState;
            CurrentState = newState;
            StateEnterTime = Time.time;
        }
    }
}

