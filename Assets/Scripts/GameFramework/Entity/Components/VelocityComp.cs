// ================================================
// GameFramework - 速度组件 (纯数据)
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 速度组件 - 存储移动相关数据
    /// </summary>
    [Serializable]
    public class VelocityComp : EntityComp<VelocityComp>
    {
        /// <summary>
        /// 线速度
        /// </summary>
        public Vector3 LinearVelocity;
        
        /// <summary>
        /// 角速度 (欧拉角/秒)
        /// </summary>
        public Vector3 AngularVelocity;
        
        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed = 5f;
        
        /// <summary>
        /// 最大速度
        /// </summary>
        public float MaxSpeed = 10f;
        
        /// <summary>
        /// 加速度
        /// </summary>
        public float Acceleration = 20f;
        
        /// <summary>
        /// 减速度 (摩擦力)
        /// </summary>
        public float Deceleration = 15f;
        
        /// <summary>
        /// 旋转速度 (度/秒)
        /// </summary>
        public float RotationSpeed = 360f;
        
        /// <summary>
        /// 重力缩放
        /// </summary>
        public float GravityScale = 1f;
        
        /// <summary>
        /// 是否受重力影响
        /// </summary>
        public bool UseGravity = true;
        
        /// <summary>
        /// 是否在地面上
        /// </summary>
        public bool IsGrounded;
        
        /// <summary>
        /// 移动输入
        /// </summary>
        public Vector3 MoveInput;
        
        /// <summary>
        /// 当前速度大小
        /// </summary>
        public float Speed => LinearVelocity.magnitude;
        
        /// <summary>
        /// 水平速度
        /// </summary>
        public Vector3 HorizontalVelocity => new Vector3(LinearVelocity.x, 0, LinearVelocity.z);
        
        /// <summary>
        /// 水平速度大小
        /// </summary>
        public float HorizontalSpeed => HorizontalVelocity.magnitude;
        
        public override void Reset()
        {
            LinearVelocity = Vector3.zero;
            AngularVelocity = Vector3.zero;
            MoveSpeed = 5f;
            MaxSpeed = 10f;
            Acceleration = 20f;
            Deceleration = 15f;
            RotationSpeed = 360f;
            GravityScale = 1f;
            UseGravity = true;
            IsGrounded = false;
            MoveInput = Vector3.zero;
        }
        
        public override IEntityComp Clone()
        {
            return new VelocityComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                LinearVelocity = LinearVelocity,
                AngularVelocity = AngularVelocity,
                MoveSpeed = MoveSpeed,
                MaxSpeed = MaxSpeed,
                Acceleration = Acceleration,
                Deceleration = Deceleration,
                RotationSpeed = RotationSpeed,
                GravityScale = GravityScale,
                UseGravity = UseGravity,
                IsGrounded = IsGrounded,
                MoveInput = MoveInput
            };
        }
    }
}

