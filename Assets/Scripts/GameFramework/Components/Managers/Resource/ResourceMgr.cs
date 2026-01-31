// ================================================
// GameFramework - 资源管理器实现
// ================================================

using System;
using System.Collections;
using System.Collections.Generic;
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
        private readonly HashSet<string> _loadingPaths = new HashSet<string>();
        private readonly Dictionary<string, List<Action<Object>>> _loadingCallbacks = new Dictionary<string, List<Action<Object>>>();
        private float _loadProgress = 1f;
        
        public override string ComponentName => "ResourceMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 20;
        
        /// <summary>
        /// 已缓存资源数量
        /// </summary>
        public int CachedCount => _cache.Count;
        
        #region Lifecycle
        
        protected override void OnShutdown()
        {
            ClearCache();
        }
        
        #endregion
        
        #region Load
        
        /// <summary>
        /// 异步加载资源 (协程)
        /// </summary>
        public Coroutine LoadAsync<T>(string path, Action<T> onComplete) where T : Object
        {
            return GameInstance.Instance.RunCoroutine(LoadAsyncCoroutine(path, onComplete));
        }
        
        private IEnumerator LoadAsyncCoroutine<T>(string path, Action<T> onComplete) where T : Object
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogWarning("[ResourceMgr] Path is null or empty");
                onComplete?.Invoke(null);
                yield break;
            }
            
            // 检查缓存
            if (_cache.TryGetValue(path, out var cached))
            {
                onComplete?.Invoke(cached as T);
                yield break;
            }
            
            // 检查是否正在加载
            if (_loadingPaths.Contains(path))
            {
                // 添加回调到等待列表
                if (!_loadingCallbacks.TryGetValue(path, out var callbacks))
                {
                    callbacks = new List<Action<Object>>();
                    _loadingCallbacks[path] = callbacks;
                }
                callbacks.Add(obj => onComplete?.Invoke(obj as T));
                
                // 等待加载完成
                while (_loadingPaths.Contains(path))
                {
                    yield return null;
                }
                yield break;
            }
            
            // 标记为正在加载
            _loadingPaths.Add(path);
            
            // 开始加载
            var request = Resources.LoadAsync<T>(path);
            yield return request;
            
            var asset = request.asset;
            if (asset != null)
            {
                _cache[path] = asset;
            }
            else
            {
                Debug.LogWarning($"[ResourceMgr] Failed to load resource: {path}");
            }
            
            // 移除加载状态
            _loadingPaths.Remove(path);
            
            // 触发所有等待的回调
            if (_loadingCallbacks.TryGetValue(path, out var waitingCallbacks))
            {
                foreach (var callback in waitingCallbacks)
                {
                    callback?.Invoke(asset);
                }
                _loadingCallbacks.Remove(path);
            }
            
            onComplete?.Invoke(asset as T);
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
        /// 预加载资源 (协程)
        /// </summary>
        public Coroutine PreloadAsync(string[] paths, Action onComplete = null, Action<float> onProgress = null)
        {
            return GameInstance.Instance.RunCoroutine(PreloadAsyncCoroutine(paths, onComplete, onProgress));
        }
        
        private IEnumerator PreloadAsyncCoroutine(string[] paths, Action onComplete, Action<float> onProgress)
        {
            if (paths == null || paths.Length == 0)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            _loadProgress = 0f;
            int loaded = 0;
            int total = paths.Length;
            
            foreach (var path in paths)
            {
                bool isLoaded = false;
                LoadAsync<Object>(path, _ =>
                {
                    loaded++;
                    _loadProgress = (float)loaded / total;
                    onProgress?.Invoke(_loadProgress);
                    isLoaded = true;
                });
                
                // 等待当前资源加载完成
                while (!isLoaded)
                {
                    yield return null;
                }
            }
            
            _loadProgress = 1f;
            onComplete?.Invoke();
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
        /// 异步实例化预制体 (协程)
        /// </summary>
        public Coroutine InstantiateAsync(string prefabPath, Action<GameObject> onComplete, Transform parent = null)
        {
            return GameInstance.Instance.RunCoroutine(InstantiateAsyncCoroutine(prefabPath, onComplete, parent));
        }
        
        private IEnumerator InstantiateAsyncCoroutine(string prefabPath, Action<GameObject> onComplete, Transform parent)
        {
            GameObject prefab = null;
            bool isLoaded = false;
            
            LoadAsync<GameObject>(prefabPath, result =>
            {
                prefab = result;
                isLoaded = true;
            });
            
            while (!isLoaded)
            {
                yield return null;
            }
            
            if (prefab == null)
            {
                onComplete?.Invoke(null);
                yield break;
            }
            
            var instance = Object.Instantiate(prefab, parent);
            onComplete?.Invoke(instance);
        }
        
        #endregion
    }
}
