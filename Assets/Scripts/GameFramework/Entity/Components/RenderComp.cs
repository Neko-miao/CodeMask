// ================================================
// GameFramework - 渲染组件 (纯数据)
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 渲染组件 - 存储渲染相关数据和Unity对象引用
    /// </summary>
    [Serializable]
    public class RenderComp : EntityComp<RenderComp>
    {
        /// <summary>
        /// 预制体路径
        /// </summary>
        public string PrefabPath;
        
        /// <summary>
        /// GameObject实例 (运行时)
        /// </summary>
        [NonSerialized]
        public GameObject GameObject;
        
        /// <summary>
        /// Transform引用 (运行时)
        /// </summary>
        [NonSerialized]
        public Transform Transform;
        
        /// <summary>
        /// Renderer引用 (运行时)
        /// </summary>
        [NonSerialized]
        public Renderer Renderer;
        
        /// <summary>
        /// Animator引用 (运行时)
        /// </summary>
        [NonSerialized]
        public Animator Animator;
        
        /// <summary>
        /// 是否可见
        /// </summary>
        public bool IsVisible = true;
        
        /// <summary>
        /// 渲染层级
        /// </summary>
        public int SortingOrder;
        
        /// <summary>
        /// 排序层名称
        /// </summary>
        public string SortingLayerName;
        
        /// <summary>
        /// 颜色
        /// </summary>
        public Color Color = Color.white;
        
        /// <summary>
        /// 是否需要同步Transform
        /// </summary>
        public bool SyncTransform = true;
        
        /// <summary>
        /// 是否已实例化
        /// </summary>
        public bool IsInstantiated => GameObject != null;
        
        public override void Reset()
        {
            PrefabPath = null;
            GameObject = null;
            Transform = null;
            Renderer = null;
            Animator = null;
            IsVisible = true;
            SortingOrder = 0;
            SortingLayerName = null;
            Color = Color.white;
            SyncTransform = true;
        }
        
        public override IEntityComp Clone()
        {
            return new RenderComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                PrefabPath = PrefabPath,
                IsVisible = IsVisible,
                SortingOrder = SortingOrder,
                SortingLayerName = SortingLayerName,
                Color = Color,
                SyncTransform = SyncTransform
                // 不复制运行时引用
            };
        }
    }
}

