// ================================================
// GameFramework - 对象池管理器接口
// ================================================

using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 对象池管理器接口
    /// </summary>
    public interface IPoolMgr : IGameComponent
    {
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        GameObject Get(string prefabPath);
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        GameObject Get(string prefabPath, Vector3 position, Quaternion rotation);
        
        /// <summary>
        /// 从池中获取对象
        /// </summary>
        GameObject Get(string prefabPath, Transform parent);
        
        /// <summary>
        /// 从池中获取对象并获取组件
        /// </summary>
        T Get<T>(string prefabPath) where T : Component;
        
        /// <summary>
        /// 从池中获取对象并获取组件
        /// </summary>
        T Get<T>(string prefabPath, Vector3 position, Quaternion rotation) where T : Component;
        
        /// <summary>
        /// 归还对象到池中
        /// </summary>
        void Release(GameObject obj);
        
        /// <summary>
        /// 延迟归还对象
        /// </summary>
        void Release(GameObject obj, float delay);
        
        /// <summary>
        /// 预创建对象
        /// </summary>
        void Preload(string prefabPath, int count);
        
        /// <summary>
        /// 清空指定池
        /// </summary>
        void ClearPool(string prefabPath);
        
        /// <summary>
        /// 清空所有池
        /// </summary>
        void ClearAll();
        
        /// <summary>
        /// 获取池中对象数量
        /// </summary>
        int GetPoolCount(string prefabPath);
        
        /// <summary>
        /// 获取活跃对象数量
        /// </summary>
        int GetActiveCount(string prefabPath);
        
        /// <summary>
        /// 设置池的最大容量
        /// </summary>
        void SetPoolCapacity(string prefabPath, int capacity);
    }
}

