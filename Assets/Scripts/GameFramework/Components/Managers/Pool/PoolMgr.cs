// ================================================
// GameFramework - 对象池管理器实现
// ================================================

using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 对象池数据
    /// </summary>
    internal class PoolData
    {
        public string PrefabPath;
        public GameObject Prefab;
        public Queue<GameObject> PooledObjects = new Queue<GameObject>();
        public HashSet<GameObject> ActiveObjects = new HashSet<GameObject>();
        public Transform PoolRoot;
        public int Capacity = 100;
    }
    
    /// <summary>
    /// 对象池管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 60, RequiredStates = new[] { GameState.Global })]
    public class PoolMgr : GameComponent, IPoolMgr
    {
        private readonly Dictionary<string, PoolData> _pools = new Dictionary<string, PoolData>();
        private readonly Dictionary<GameObject, string> _objectPoolMap = new Dictionary<GameObject, string>();
        private Transform _poolRoot;
        
        private const int DEFAULT_CAPACITY = 100;
        
        public override string ComponentName => "PoolMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 60;
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            var go = new GameObject("[PoolMgr]");
            Object.DontDestroyOnLoad(go);
            _poolRoot = go.transform;
        }
        
        protected override void OnShutdown()
        {
            ClearAll();
            
            if (_poolRoot != null)
            {
                Object.Destroy(_poolRoot.gameObject);
            }
        }
        
        #endregion
        
        #region Get
        
        public GameObject Get(string prefabPath)
        {
            return Get(prefabPath, Vector3.zero, Quaternion.identity);
        }
        
        public GameObject Get(string prefabPath, Transform parent)
        {
            var obj = Get(prefabPath);
            if (obj != null)
            {
                obj.transform.SetParent(parent, false);
            }
            return obj;
        }
        
        public GameObject Get(string prefabPath, Vector3 position, Quaternion rotation)
        {
            var pool = GetOrCreatePool(prefabPath);
            if (pool == null || pool.Prefab == null) return null;
            
            GameObject obj;
            
            if (pool.PooledObjects.Count > 0)
            {
                obj = pool.PooledObjects.Dequeue();
                obj.transform.position = position;
                obj.transform.rotation = rotation;
            }
            else
            {
                obj = Object.Instantiate(pool.Prefab, position, rotation);
                _objectPoolMap[obj] = prefabPath;
            }
            
            obj.transform.SetParent(null);
            obj.SetActive(true);
            pool.ActiveObjects.Add(obj);
            
            // 触发池对象事件
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnSpawn();
            
            return obj;
        }
        
        public T Get<T>(string prefabPath) where T : Component
        {
            var obj = Get(prefabPath);
            return obj?.GetComponent<T>();
        }
        
        public T Get<T>(string prefabPath, Vector3 position, Quaternion rotation) where T : Component
        {
            var obj = Get(prefabPath, position, rotation);
            return obj?.GetComponent<T>();
        }
        
        #endregion
        
        #region Release
        
        public void Release(GameObject obj)
        {
            if (obj == null) return;
            
            if (!_objectPoolMap.TryGetValue(obj, out var prefabPath))
            {
                // 不是池对象，直接销毁
                Object.Destroy(obj);
                return;
            }
            
            if (!_pools.TryGetValue(prefabPath, out var pool))
            {
                Object.Destroy(obj);
                return;
            }
            
            // 触发池对象事件
            var poolable = obj.GetComponent<IPoolable>();
            poolable?.OnDespawn();
            
            pool.ActiveObjects.Remove(obj);
            
            // 检查容量
            if (pool.PooledObjects.Count >= pool.Capacity)
            {
                _objectPoolMap.Remove(obj);
                Object.Destroy(obj);
                return;
            }
            
            obj.SetActive(false);
            obj.transform.SetParent(pool.PoolRoot);
            pool.PooledObjects.Enqueue(obj);
        }
        
        public void Release(GameObject obj, float delay)
        {
            if (obj == null) return;
            
            var timerMgr = GetComp<ITimerMgr>();
            if (timerMgr != null)
            {
                timerMgr.Schedule(delay, () => Release(obj));
            }
            else
            {
                Release(obj);
            }
        }
        
        #endregion
        
        #region Preload
        
        public void Preload(string prefabPath, int count)
        {
            var pool = GetOrCreatePool(prefabPath);
            if (pool == null || pool.Prefab == null) return;
            
            for (int i = 0; i < count; i++)
            {
                if (pool.PooledObjects.Count >= pool.Capacity)
                    break;
                
                var obj = Object.Instantiate(pool.Prefab, pool.PoolRoot);
                obj.SetActive(false);
                _objectPoolMap[obj] = prefabPath;
                pool.PooledObjects.Enqueue(obj);
            }
            
            Debug.Log($"[PoolMgr] Preloaded {count} objects for: {prefabPath}");
        }
        
        #endregion
        
        #region Clear
        
        public void ClearPool(string prefabPath)
        {
            if (!_pools.TryGetValue(prefabPath, out var pool))
                return;
            
            // 销毁池中的对象
            while (pool.PooledObjects.Count > 0)
            {
                var obj = pool.PooledObjects.Dequeue();
                if (obj != null)
                {
                    _objectPoolMap.Remove(obj);
                    Object.Destroy(obj);
                }
            }
            
            // 销毁活跃的对象
            foreach (var obj in pool.ActiveObjects)
            {
                if (obj != null)
                {
                    _objectPoolMap.Remove(obj);
                    Object.Destroy(obj);
                }
            }
            pool.ActiveObjects.Clear();
            
            // 销毁池根节点
            if (pool.PoolRoot != null)
            {
                Object.Destroy(pool.PoolRoot.gameObject);
            }
            
            _pools.Remove(prefabPath);
        }
        
        public void ClearAll()
        {
            var paths = new List<string>(_pools.Keys);
            foreach (var path in paths)
            {
                ClearPool(path);
            }
            _pools.Clear();
            _objectPoolMap.Clear();
        }
        
        #endregion
        
        #region Query
        
        public int GetPoolCount(string prefabPath)
        {
            if (_pools.TryGetValue(prefabPath, out var pool))
            {
                return pool.PooledObjects.Count;
            }
            return 0;
        }
        
        public int GetActiveCount(string prefabPath)
        {
            if (_pools.TryGetValue(prefabPath, out var pool))
            {
                return pool.ActiveObjects.Count;
            }
            return 0;
        }
        
        public void SetPoolCapacity(string prefabPath, int capacity)
        {
            var pool = GetOrCreatePool(prefabPath);
            if (pool != null)
            {
                pool.Capacity = capacity;
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private PoolData GetOrCreatePool(string prefabPath)
        {
            if (_pools.TryGetValue(prefabPath, out var pool))
                return pool;
            
            // 加载预制体
            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[PoolMgr] Failed to load prefab: {prefabPath}");
                return null;
            }
            
            // 创建池根节点
            var rootGo = new GameObject($"Pool_{prefabPath.Replace("/", "_")}");
            rootGo.transform.SetParent(_poolRoot);
            
            pool = new PoolData
            {
                PrefabPath = prefabPath,
                Prefab = prefab,
                PoolRoot = rootGo.transform,
                Capacity = DEFAULT_CAPACITY
            };
            
            _pools[prefabPath] = pool;
            
            return pool;
        }
        
        #endregion
    }
    
    /// <summary>
    /// 可池化对象接口
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// 从池中取出时调用
        /// </summary>
        void OnSpawn();
        
        /// <summary>
        /// 归还到池中时调用
        /// </summary>
        void OnDespawn();
    }
}

