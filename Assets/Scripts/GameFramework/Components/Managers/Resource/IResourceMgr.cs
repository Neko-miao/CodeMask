// ================================================
// GameFramework - 资源管理器接口
// ================================================

using System;
using System.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 资源管理器接口
    /// </summary>
    public interface IResourceMgr : IGameComponent
    {
        /// <summary>
        /// 异步加载资源
        /// </summary>
        Task<T> LoadAsync<T>(string path) where T : UnityEngine.Object;
        
        /// <summary>
        /// 异步加载资源 (带回调)
        /// </summary>
        void LoadAsync<T>(string path, Action<T> onComplete) where T : UnityEngine.Object;
        
        /// <summary>
        /// 同步加载资源
        /// </summary>
        T Load<T>(string path) where T : UnityEngine.Object;
        
        /// <summary>
        /// 卸载资源
        /// </summary>
        void Unload(string path);
        
        /// <summary>
        /// 卸载未使用的资源
        /// </summary>
        void UnloadUnused();
        
        /// <summary>
        /// 预加载资源
        /// </summary>
        Task PreloadAsync(string[] paths);
        
        /// <summary>
        /// 预加载资源 (带进度回调)
        /// </summary>
        Task PreloadAsync(string[] paths, Action<float> onProgress);
        
        /// <summary>
        /// 获取加载进度
        /// </summary>
        float GetLoadProgress();
        
        /// <summary>
        /// 检查资源是否已加载
        /// </summary>
        bool IsLoaded(string path);
        
        /// <summary>
        /// 获取已缓存资源数量
        /// </summary>
        int CachedCount { get; }
        
        /// <summary>
        /// 清空所有缓存
        /// </summary>
        void ClearCache();
        
        /// <summary>
        /// 实例化预制体
        /// </summary>
        GameObject Instantiate(string prefabPath, Transform parent = null);
        
        /// <summary>
        /// 实例化预制体
        /// </summary>
        GameObject Instantiate(string prefabPath, Vector3 position, Quaternion rotation, Transform parent = null);
        
        /// <summary>
        /// 异步实例化预制体
        /// </summary>
        Task<GameObject> InstantiateAsync(string prefabPath, Transform parent = null);
    }
}

