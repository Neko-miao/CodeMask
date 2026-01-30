// ================================================
// GameFramework - 碰撞器组件 (纯数据)
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 碰撞器类型
    /// </summary>
    public enum ColliderType
    {
        None = 0,
        Box = 1,
        Sphere = 2,
        Capsule = 3,
        Circle2D = 4,
        Box2D = 5
    }
    
    /// <summary>
    /// 碰撞器组件 - 存储碰撞相关数据
    /// </summary>
    [Serializable]
    public class ColliderComp : EntityComp<ColliderComp>
    {
        /// <summary>
        /// 碰撞器类型
        /// </summary>
        public ColliderType Type = ColliderType.Box;
        
        /// <summary>
        /// 碰撞器中心偏移
        /// </summary>
        public Vector3 Center;
        
        /// <summary>
        /// 尺寸 (Box)
        /// </summary>
        public Vector3 Size = Vector3.one;
        
        /// <summary>
        /// 半径 (Sphere/Circle/Capsule)
        /// </summary>
        public float Radius = 0.5f;
        
        /// <summary>
        /// 高度 (Capsule)
        /// </summary>
        public float Height = 2f;
        
        /// <summary>
        /// 是否触发器
        /// </summary>
        public bool IsTrigger;
        
        /// <summary>
        /// 碰撞层
        /// </summary>
        public int Layer;
        
        /// <summary>
        /// 碰撞掩码
        /// </summary>
        public int CollisionMask = -1;
        
        /// <summary>
        /// Unity Collider引用 (运行时)
        /// </summary>
        [NonSerialized]
        public Collider Collider;
        
        /// <summary>
        /// Unity Collider2D引用 (运行时)
        /// </summary>
        [NonSerialized]
        public Collider2D Collider2D;
        
        public override void Reset()
        {
            Type = ColliderType.Box;
            Center = Vector3.zero;
            Size = Vector3.one;
            Radius = 0.5f;
            Height = 2f;
            IsTrigger = false;
            Layer = 0;
            CollisionMask = -1;
            Collider = null;
            Collider2D = null;
        }
        
        public override IEntityComp Clone()
        {
            return new ColliderComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                Type = Type,
                Center = Center,
                Size = Size,
                Radius = Radius,
                Height = Height,
                IsTrigger = IsTrigger,
                Layer = Layer,
                CollisionMask = CollisionMask
            };
        }
    }
}

