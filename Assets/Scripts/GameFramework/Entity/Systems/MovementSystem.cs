// ================================================
// GameFramework - 移动系统
// ================================================

using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 移动系统 - 处理实体的移动逻辑
    /// 需要: TransformComp, VelocityComp
    /// </summary>
    public class MovementSystem : EntitySystem
    {
        /// <summary>
        /// 重力加速度
        /// </summary>
        public Vector3 Gravity = new Vector3(0, -9.81f, 0);
        
        public override string SystemName => "MovementSystem";
        public override int Priority => 100;
        public override SystemPhase Phase => SystemPhase.FixedUpdate;
        
        protected override void ConfigureRequirements()
        {
            Require<TransformComp>();
            Require<VelocityComp>();
        }
        
        public override void ProcessEntity(IEntity entity, float deltaTime)
        {
            var transform = entity.GetComp<TransformComp>();
            var velocity = entity.GetComp<VelocityComp>();
            
            if (transform == null || velocity == null) return;
            if (!transform.IsEnabled || !velocity.IsEnabled) return;
            
            // 处理输入
            ProcessInput(velocity, deltaTime);
            
            // 应用重力
            if (velocity.UseGravity && !velocity.IsGrounded)
            {
                velocity.LinearVelocity += Gravity * velocity.GravityScale * deltaTime;
            }
            
            // 限制最大速度
            if (velocity.Speed > velocity.MaxSpeed)
            {
                velocity.LinearVelocity = velocity.LinearVelocity.normalized * velocity.MaxSpeed;
            }
            
            // 应用速度到位置
            transform.Position += velocity.LinearVelocity * deltaTime;
            
            // 应用角速度到旋转
            if (velocity.AngularVelocity != Vector3.zero)
            {
                transform.Rotation += velocity.AngularVelocity * deltaTime;
            }
        }
        
        private void ProcessInput(VelocityComp velocity, float deltaTime)
        {
            var input = velocity.MoveInput;
            
            if (input.sqrMagnitude > 0.01f)
            {
                // 有输入时加速
                var targetVelocity = input.normalized * velocity.MoveSpeed;
                var horizontalVelocity = new Vector3(velocity.LinearVelocity.x, 0, velocity.LinearVelocity.z);
                
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    targetVelocity,
                    velocity.Acceleration * deltaTime
                );
                
                velocity.LinearVelocity = new Vector3(
                    horizontalVelocity.x,
                    velocity.LinearVelocity.y,
                    horizontalVelocity.z
                );
            }
            else if (velocity.IsGrounded)
            {
                // 无输入时减速
                var horizontalVelocity = new Vector3(velocity.LinearVelocity.x, 0, velocity.LinearVelocity.z);
                
                horizontalVelocity = Vector3.MoveTowards(
                    horizontalVelocity,
                    Vector3.zero,
                    velocity.Deceleration * deltaTime
                );
                
                velocity.LinearVelocity = new Vector3(
                    horizontalVelocity.x,
                    velocity.LinearVelocity.y,
                    horizontalVelocity.z
                );
            }
        }
    }
}

