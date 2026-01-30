// ================================================
// GameFramework - AI系统
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// AI系统 - 处理AI行为逻辑
    /// 需要: TransformComp, AIComp
    /// </summary>
    public class AISystem : EntitySystem
    {
        public override string SystemName => "AISystem";
        public override int Priority => 150;
        public override SystemPhase Phase => SystemPhase.Update;
        
        #region Events
        
        /// <summary>
        /// 状态改变事件
        /// </summary>
        public event Action<IEntity, AIState, AIState> OnStateChanged;
        
        /// <summary>
        /// 目标检测事件
        /// </summary>
        public event Action<IEntity, IEntity> OnTargetDetected;
        
        /// <summary>
        /// 目标丢失事件
        /// </summary>
        public event Action<IEntity> OnTargetLost;
        
        #endregion
        
        protected override void ConfigureRequirements()
        {
            Require<TransformComp>();
            Require<AIComp>();
        }
        
        public override void ProcessEntity(IEntity entity, float deltaTime)
        {
            var transform = entity.GetComp<TransformComp>();
            var ai = entity.GetComp<AIComp>();
            
            if (transform == null || ai == null) return;
            if (!ai.IsEnabled) return;
            
            // 更新AI状态
            UpdateAI(entity, transform, ai, deltaTime);
        }
        
        protected virtual void UpdateAI(IEntity entity, TransformComp transform, AIComp ai, float deltaTime)
        {
            // 检测目标
            UpdateTargetDetection(entity, transform, ai);
            
            // 根据状态执行行为
            switch (ai.CurrentState)
            {
                case AIState.Idle:
                    ProcessIdle(entity, transform, ai, deltaTime);
                    break;
                case AIState.Patrol:
                    ProcessPatrol(entity, transform, ai, deltaTime);
                    break;
                case AIState.Chase:
                    ProcessChase(entity, transform, ai, deltaTime);
                    break;
                case AIState.Attack:
                    ProcessAttack(entity, transform, ai, deltaTime);
                    break;
                case AIState.Flee:
                    ProcessFlee(entity, transform, ai, deltaTime);
                    break;
            }
        }
        
        #region Target Detection
        
        protected virtual void UpdateTargetDetection(IEntity entity, TransformComp transform, AIComp ai)
        {
            // 如果有目标，检查是否还有效
            if (ai.HasTarget)
            {
                var target = GetEntity(ai.TargetEntityId);
                if (target == null || !target.IsActive || target.IsDestroyed)
                {
                    ai.TargetEntityId = -1;
                    OnTargetLost?.Invoke(entity);
                }
                else
                {
                    // 检查距离是否超出范围
                    var targetTransform = target.GetComp<TransformComp>();
                    if (targetTransform != null)
                    {
                        float dist = Vector3.Distance(transform.Position, targetTransform.Position);
                        if (dist > ai.AlertRange * 1.5f)
                        {
                            ai.TargetEntityId = -1;
                            OnTargetLost?.Invoke(entity);
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 检测范围内的目标
        /// </summary>
        public IEntity DetectTarget(IEntity self, EntityTag targetTag, float range)
        {
            var selfTransform = self.GetComp<TransformComp>();
            if (selfTransform == null) return null;
            
            var targets = _entityMgr.GetEntitiesWithTag(targetTag);
            IEntity nearest = null;
            float nearestDist = float.MaxValue;
            
            foreach (var target in targets)
            {
                if (target == self || !target.IsActive) continue;
                
                var targetTransform = target.GetComp<TransformComp>();
                if (targetTransform == null) continue;
                
                float dist = Vector3.Distance(selfTransform.Position, targetTransform.Position);
                if (dist < range && dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = target;
                }
            }
            
            return nearest;
        }
        
        /// <summary>
        /// 设置目标
        /// </summary>
        public void SetTarget(int entityId, int targetEntityId)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var ai = entity.GetComp<AIComp>();
            if (ai == null) return;
            
            var target = GetEntity(targetEntityId);
            if (target == null) return;
            
            ai.TargetEntityId = targetEntityId;
            OnTargetDetected?.Invoke(entity, target);
        }
        
        #endregion
        
        #region State Processing
        
        protected virtual void ProcessIdle(IEntity entity, TransformComp transform, AIComp ai, float deltaTime)
        {
            // 空闲状态：可以切换到巡逻
            if (ai.StateDuration > 3f)
            {
                ChangeState(entity, ai, AIState.Patrol);
            }
        }
        
        protected virtual void ProcessPatrol(IEntity entity, TransformComp transform, AIComp ai, float deltaTime)
        {
            // 巡逻状态：如果发现目标则切换到追击
            if (ai.HasTarget)
            {
                ChangeState(entity, ai, AIState.Chase);
                return;
            }
            
            // 更新移动输入
            var velocity = entity.GetComp<VelocityComp>();
            if (velocity != null)
            {
                // 简单的随机巡逻
                if ((transform.Position - ai.TargetPosition).sqrMagnitude < 1f)
                {
                    // 到达目标点，设置新的目标
                    var randomOffset = new Vector3(
                        UnityEngine.Random.Range(-ai.PatrolRadius, ai.PatrolRadius),
                        0,
                        UnityEngine.Random.Range(-ai.PatrolRadius, ai.PatrolRadius)
                    );
                    ai.TargetPosition = ai.HomePosition + randomOffset;
                }
                
                // 朝目标移动
                var dir = (ai.TargetPosition - transform.Position).normalized;
                velocity.MoveInput = new Vector3(dir.x, 0, dir.z);
            }
        }
        
        protected virtual void ProcessChase(IEntity entity, TransformComp transform, AIComp ai, float deltaTime)
        {
            if (!ai.HasTarget)
            {
                ChangeState(entity, ai, AIState.Patrol);
                return;
            }
            
            var target = GetEntity(ai.TargetEntityId);
            var targetTransform = target?.GetComp<TransformComp>();
            
            if (targetTransform == null)
            {
                ai.TargetEntityId = -1;
                ChangeState(entity, ai, AIState.Patrol);
                return;
            }
            
            float dist = Vector3.Distance(transform.Position, targetTransform.Position);
            
            // 进入攻击范围
            if (dist <= ai.AttackRange)
            {
                ChangeState(entity, ai, AIState.Attack);
                return;
            }
            
            // 追击
            var velocity = entity.GetComp<VelocityComp>();
            if (velocity != null)
            {
                var dir = (targetTransform.Position - transform.Position).normalized;
                velocity.MoveInput = new Vector3(dir.x, 0, dir.z);
            }
        }
        
        protected virtual void ProcessAttack(IEntity entity, TransformComp transform, AIComp ai, float deltaTime)
        {
            if (!ai.HasTarget)
            {
                ChangeState(entity, ai, AIState.Patrol);
                return;
            }
            
            var target = GetEntity(ai.TargetEntityId);
            var targetTransform = target?.GetComp<TransformComp>();
            
            if (targetTransform == null)
            {
                ai.TargetEntityId = -1;
                ChangeState(entity, ai, AIState.Patrol);
                return;
            }
            
            float dist = Vector3.Distance(transform.Position, targetTransform.Position);
            
            // 超出攻击范围，继续追击
            if (dist > ai.AttackRange * 1.2f)
            {
                ChangeState(entity, ai, AIState.Chase);
                return;
            }
            
            // 停止移动
            var velocity = entity.GetComp<VelocityComp>();
            if (velocity != null)
            {
                velocity.MoveInput = Vector3.zero;
            }
            
            // 面向目标
            transform.LookAt(targetTransform.Position);
        }
        
        protected virtual void ProcessFlee(IEntity entity, TransformComp transform, AIComp ai, float deltaTime)
        {
            if (!ai.HasTarget)
            {
                ChangeState(entity, ai, AIState.Patrol);
                return;
            }
            
            var target = GetEntity(ai.TargetEntityId);
            var targetTransform = target?.GetComp<TransformComp>();
            
            if (targetTransform == null)
            {
                ai.TargetEntityId = -1;
                ChangeState(entity, ai, AIState.Patrol);
                return;
            }
            
            // 远离目标
            var velocity = entity.GetComp<VelocityComp>();
            if (velocity != null)
            {
                var dir = (transform.Position - targetTransform.Position).normalized;
                velocity.MoveInput = new Vector3(dir.x, 0, dir.z);
            }
            
            // 逃离到安全距离后回到巡逻
            float dist = Vector3.Distance(transform.Position, targetTransform.Position);
            if (dist > ai.AlertRange * 2f)
            {
                ai.TargetEntityId = -1;
                ChangeState(entity, ai, AIState.Patrol);
            }
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 改变AI状态
        /// </summary>
        public void ChangeState(IEntity entity, AIComp ai, AIState newState)
        {
            if (ai.CurrentState == newState) return;
            
            var oldState = ai.CurrentState;
            ai.SetState(newState);
            
            OnStateChanged?.Invoke(entity, oldState, newState);
        }
        
        /// <summary>
        /// 强制设置状态
        /// </summary>
        public void ForceState(int entityId, AIState state)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var ai = entity.GetComp<AIComp>();
            if (ai != null)
            {
                ChangeState(entity, ai, state);
            }
        }
        
        #endregion
    }
}

