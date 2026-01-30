// ================================================
// GameFramework - 变换组件 (纯数据)
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 变换组件 - 存储位置、旋转、缩放数据
    /// </summary>
    [Serializable]
    public class TransformComp : EntityComp<TransformComp>
    {
        /// <summary>
        /// 位置
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// 旋转 (欧拉角)
        /// </summary>
        public Vector3 Rotation;
        
        /// <summary>
        /// 缩放
        /// </summary>
        public Vector3 Scale = Vector3.one;
        
        /// <summary>
        /// 父实体ID (-1表示无父实体)
        /// </summary>
        public int ParentId = -1;
        
        /// <summary>
        /// 获取旋转四元数
        /// </summary>
        public Quaternion RotationQuat
        {
            get => Quaternion.Euler(Rotation);
            set => Rotation = value.eulerAngles;
        }
        
        /// <summary>
        /// 前方向
        /// </summary>
        public Vector3 Forward => RotationQuat * Vector3.forward;
        
        /// <summary>
        /// 右方向
        /// </summary>
        public Vector3 Right => RotationQuat * Vector3.right;
        
        /// <summary>
        /// 上方向
        /// </summary>
        public Vector3 Up => RotationQuat * Vector3.up;
        
        public override void Reset()
        {
            Position = Vector3.zero;
            Rotation = Vector3.zero;
            Scale = Vector3.one;
            ParentId = -1;
        }
        
        public override IEntityComp Clone()
        {
            return new TransformComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                Position = Position,
                Rotation = Rotation,
                Scale = Scale,
                ParentId = ParentId
            };
        }
        
        /// <summary>
        /// 朝向目标
        /// </summary>
        public void LookAt(Vector3 target)
        {
            var direction = target - Position;
            if (direction != Vector3.zero)
            {
                Rotation = Quaternion.LookRotation(direction).eulerAngles;
            }
        }
        
        /// <summary>
        /// 移动
        /// </summary>
        public void Translate(Vector3 delta, bool localSpace = true)
        {
            if (localSpace)
            {
                Position += RotationQuat * delta;
            }
            else
            {
                Position += delta;
            }
        }
        
        /// <summary>
        /// 旋转
        /// </summary>
        public void Rotate(Vector3 eulerAngles)
        {
            Rotation += eulerAngles;
        }
    }
}

