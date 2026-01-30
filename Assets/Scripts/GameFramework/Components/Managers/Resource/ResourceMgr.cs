// ================================================
// GameFramework - 资源管理器实现
// ================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameFramework.Components
{
    /// <summary>
    /// 资源管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 20, RequiredStates = new[] { GameState.Global })]
    public class ResourceMgr : GameComponent, IResourceMgr
    {
        private readonly Dictionary<string, Object> _cache = new Dictionary<string, Object>();
        private readonly Dictionary<string, Task<Object>> _loadingTasks = new Dictionary<string, Task<Object>>();
        private float _loadProgress = 1f;
        
        public override string ComponentName => "ResourceMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 20;
        
        /// <summary>
        /// 已缓存资源数量
        /// </summary>
        public int CachedCount => _cache.Count;
        
        #region Load
        
        /// <summary>
        /// 异步加载资源
        /// </summary>
        public async Task<T> LoadAsync<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[ResourceMgr] Path is null or empty");
                return null;
            }
            
            // 检查缓存
            if (_cache.TryGetValue(path, out var cached))
            {
                return cached as T;
            }
            
            // 检查是否正在加载
            if (_loadingTasks.TryGetValue(path, out var loadingTask))
            {
                var result = await loadingTask;
                return result as T;
            }
            
            // 开始加载
            var request = Resources.LoadAsync<T>(path);
            var tcs = new TaskCompletionSource<Object>();
            
            _loadingTasks[path] = tcs.Task;
            
            request.completed += _ =>
            {
                var asset = request.asset;
                if (asset != null)
                {
                    _cache[path] = asset;
                }
                else
                {
                    Debug.LogWarning($"[ResourceMgr] Failed to load resource: {path}");
                }
                
                _loadingTasks.Remove(path);
                tcs.SetResult(asset);
            };
            
            return await tcs.Task as T;
        }
        
        /// <summary>
        /// 异步加载资源 (带回调)
        /// </summary>
        public async void LoadAsync<T>(string path, Action<T> onComplete) where T : Object
        {
            var result = await LoadAsync<T>(path);
            onComplete?.Invoke(result);
        }
        
        /// <summary>
        /// 同步加载资源
        /// </summary>
        public T Load<T>(string path) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[ResourceMgr] Path is null or empty");
                return null;
            }
            
            // 检查缓存
            if (_cache.TryGetValue(path, out var cached))
            {
                return cached as T;
            }
            
            // 同步加载
            var asset = Resources.Load<T>(path);
            
            if (asset != null)
            {
                _cache[path] = asset;
            }
            else
            {
                Debug.LogWarning($"[ResourceMgr] Failed to load resource: {path}");
            }
            
            return asset;
        }
        
        #endregion
        
        #region Unload
        
        /// <summary>
        /// 卸载资源
        /// </summary>
        public void Unload(string path)
        {
            if (_cache.TryGetValue(path, out var asset))
            {
                _cache.Remove(path);
                
                // 不是GameObject的资源可以直接卸载
                if (!(asset is GameObject))
                {
                    Resources.UnloadAsset(asset);
                }
            }
        }
        
        /// <summary>
        /// 卸载未使用的资源
        /// </summary>
        public void UnloadUnused()
        {
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        
        /// <summary>
        /// 清空所有缓存
        /// </summary>
        public void ClearCache()
        {
            _cache.Clear();
            Resources.UnloadUnusedAssets();
            GC.Collect();
        }
        
        #endregion
        
        #region Preload
        
        /// <summary>
        /// 预加载资源
        /// </summary>
        public Task PreloadAsync(string[] paths)
        {
            return PreloadAsync(paths, null);
        }
        
        /// <summary>
        /// 预加载资源 (带进度回调)
        /// </summary>
        public async Task PreloadAsync(string[] paths, Action<float> onProgress)
        {
            if (paths == null || paths.Length == 0)
                return;
            
            _loadProgress = 0f;
            int loaded = 0;
            int total = paths.Length;
            
            var tasks = new List<Task>();
            
            foreach (var path in paths)
            {
                var task = LoadAsync<Object>(path).ContinueWith(_ =>
                {
                    loaded++;
                    _loadProgress = (float)loaded / total;
                    onProgress?.Invoke(_loadProgress);
                });
                tasks.Add(task);
            }
            
            await Task.WhenAll(tasks);
            _loadProgress = 1f;
        }
        
        /// <summary>
        /// 获取加载进度
        /// </summary>
        public float GetLoadProgress()
        {
            return _loadProgress;
        }
        
        /// <summary>
        /// 检查资源是否已加载
        /// </summary>
        public bool IsLoaded(string path)
        {
            return _cache.ContainsKey(path);
        }
        
        #endregion
        
        #region Instantiate
        
        /// <summary>
        /// 实例化预制体
        /// </summary>
        public GameObject Instantiate(string prefabPath, Transform parent = null)
        {
            var prefab = Load<GameObject>(prefabPath);
            if (prefab == null) return null;
            
            return Object.Instantiate(prefab, parent);
        }
        
        /// <summary>
        /// 实例化预制体
        /// </summary>
        public GameObject Instantiate(string prefabPath, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            var prefab = Load<GameObject>(prefabPath);
            if (prefab == null) return null;
            
            return Object.Instantiate(prefab, position, rotation, parent);
        }
        
        /// <summary>
        /// 异步实例化预制体
        /// </summary>
        public async Task<GameObject> InstantiateAsync(string prefabPath, Transform parent = null)
        {
            var prefab = await LoadAsync<GameObject>(prefabPath);
            if (prefab == null) return null;
            
            return Object.Instantiate(prefab, parent);
        }
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnShutdown()
        {
            ClearCache();
        }
        
        #endregion
    }
}

