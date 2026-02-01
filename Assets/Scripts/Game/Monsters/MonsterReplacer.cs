// ================================================
// Game - 怪物替换器
// 定时替换Replacement对象下的怪物模型
// ================================================

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Monsters
{
    /// <summary>
    /// 怪物替换器 - 定时替换Replacement对象
    /// </summary>
    public class MonsterReplacer : MonoBehaviour
    {
        #region Configuration

        [Header("替换配置")]
        [Tooltip("要替换的目标对象")]
        [SerializeField] private Transform _replacement;

        [Tooltip("替换间隔时间（秒）")]
        [SerializeField] private float _replaceInterval = 2f;

        [Tooltip("是否自动开始替换")]
        [SerializeField] private bool _autoStart = true;

        [Tooltip("是否循环替换")]
        [SerializeField] private bool _loop = true;

        [Header("怪物资源配置")]
        [Tooltip("怪物预制体名称列表（Resources/Monsters/下的预制体名称，不含扩展名）")]
        [SerializeField] private List<string> _monsterPrefabNames = new List<string>
        {
            "Cat",
            // 在这里添加更多怪物预制体名称
        };

        [Tooltip("资源路径（相对于Resources文件夹）")]
        [SerializeField] private string _resourcePath = "Monsters";

        [Header("替换选项")]
        [Tooltip("替换顺序：true=顺序替换，false=随机替换")]
        [SerializeField] private bool _sequentialReplace = true;

        [Tooltip("替换时是否保持原有的位置")]
        [SerializeField] private bool _keepPosition = true;

        [Tooltip("替换时是否保持原有的旋转")]
        [SerializeField] private bool _keepRotation = true;

        [Tooltip("替换时是否保持原有的缩放")]
        [SerializeField] private bool _keepScale = true;

        [Tooltip("创建后Y轴向上偏移量（米）")]
        [SerializeField] private float _spawnYOffset = 1f;

        [Header("物理保护")]
        [Tooltip("是否启用最小高度保护")]
        [SerializeField] private bool _enableMinHeightProtection = true;

        [Tooltip("世界坐标Y轴最小高度（米）")]
        [SerializeField] private float _minWorldHeight = 0.2f;

        #endregion

        #region Fields

        private int _currentIndex = 0;
        private bool _isRunning = false;
        private Coroutine _replaceCoroutine;
        private GameObject _currentInstance;

        // 缓存加载的预制体
        private Dictionary<string, GameObject> _prefabCache = new Dictionary<string, GameObject>();

        #endregion

        #region Properties

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => _isRunning;

        /// <summary>
        /// 当前索引
        /// </summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>
        /// 当前实例
        /// </summary>
        public GameObject CurrentInstance => _currentInstance;

        /// <summary>
        /// Replacement对象
        /// </summary>
        public Transform Replacement
        {
            get => _replacement;
            set => _replacement = value;
        }

        #endregion

        #region Events

        /// <summary>
        /// 替换完成事件（参数：新实例，索引）
        /// </summary>
        public event Action<GameObject, int> OnReplaced;

        /// <summary>
        /// 一轮循环完成事件
        /// </summary>
        public event Action OnCycleCompleted;

        #endregion

        #region Unity Lifecycle

        private void Start()
        {
            if (_autoStart && _monsterPrefabNames.Count > 0)
            {
                StartReplacing();
            }
        }

        private void FixedUpdate()
        {
            // 物理保护：强制保持当前实例的世界坐标Y轴不低于最小高度
            if (_enableMinHeightProtection && _currentInstance != null)
            {
                Vector3 worldPos = _currentInstance.transform.position;
                if (worldPos.y < _minWorldHeight)
                {
                    worldPos.y = _minWorldHeight;
                    _currentInstance.transform.position = worldPos;
                }
            }
        }

        private void OnDestroy()
        {
            StopReplacing();
            ClearCache();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 开始替换
        /// </summary>
        public void StartReplacing()
        {
            if (_isRunning) return;
            if (_replacement == null)
            {
                Debug.LogError("[MonsterReplacer] Replacement transform is null!");
                return;
            }
            if (_monsterPrefabNames.Count == 0)
            {
                Debug.LogError("[MonsterReplacer] No monster prefab names configured!");
                return;
            }

            _isRunning = true;
            _replaceCoroutine = StartCoroutine(ReplaceRoutine());
            Debug.Log($"[MonsterReplacer] Started replacing, interval: {_replaceInterval}s, count: {_monsterPrefabNames.Count}");
        }

        /// <summary>
        /// 停止替换
        /// </summary>
        public void StopReplacing()
        {
            if (!_isRunning) return;

            _isRunning = false;
            if (_replaceCoroutine != null)
            {
                StopCoroutine(_replaceCoroutine);
                _replaceCoroutine = null;
            }
            Debug.Log("[MonsterReplacer] Stopped replacing");
        }

        /// <summary>
        /// 立即执行一次替换
        /// </summary>
        public void ReplaceNow()
        {
            DoReplace();
        }

        /// <summary>
        /// 替换为指定索引的怪物
        /// </summary>
        public void ReplaceByIndex(int index)
        {
            if (index < 0 || index >= _monsterPrefabNames.Count)
            {
                Debug.LogWarning($"[MonsterReplacer] Invalid index: {index}");
                return;
            }

            _currentIndex = index;
            DoReplaceAtIndex(index);
        }

        /// <summary>
        /// 替换为指定名称的怪物
        /// </summary>
        public void ReplaceByName(string prefabName)
        {
            int index = _monsterPrefabNames.IndexOf(prefabName);
            if (index >= 0)
            {
                ReplaceByIndex(index);
            }
            else
            {
                Debug.LogWarning($"[MonsterReplacer] Prefab name not found: {prefabName}");
            }
        }

        /// <summary>
        /// 重置索引
        /// </summary>
        public void ResetIndex()
        {
            _currentIndex = 0;
        }

        /// <summary>
        /// 清除当前实例（立即销毁）
        /// </summary>
        public void ClearCurrentInstance()
        {
            if (_currentInstance != null)
            {
                // 使用DestroyImmediate立即销毁，避免新旧怪物同时存在
                DestroyImmediate(_currentInstance);
                _currentInstance = null;
            }
        }

        /// <summary>
        /// 清除预制体缓存
        /// </summary>
        public void ClearCache()
        {
            _prefabCache.Clear();
        }

        /// <summary>
        /// 添加怪物预制体名称
        /// </summary>
        public void AddMonsterPrefab(string prefabName)
        {
            if (!_monsterPrefabNames.Contains(prefabName))
            {
                _monsterPrefabNames.Add(prefabName);
            }
        }

        /// <summary>
        /// 移除怪物预制体名称
        /// </summary>
        public void RemoveMonsterPrefab(string prefabName)
        {
            _monsterPrefabNames.Remove(prefabName);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// 替换协程
        /// </summary>
        private IEnumerator ReplaceRoutine()
        {
            // 立即执行第一次替换
            DoReplace();

            while (_isRunning)
            {
                yield return new WaitForSeconds(_replaceInterval);

                if (!_isRunning) break;

                DoReplace();
            }
        }

        /// <summary>
        /// 执行替换
        /// </summary>
        private void DoReplace()
        {
            if (_monsterPrefabNames.Count == 0) return;

            int index;
            if (_sequentialReplace)
            {
                index = _currentIndex;
                _currentIndex = (_currentIndex + 1) % _monsterPrefabNames.Count;

                // 检查是否完成一轮循环
                if (_currentIndex == 0)
                {
                    OnCycleCompleted?.Invoke();
                    if (!_loop)
                    {
                        StopReplacing();
                    }
                }
            }
            else
            {
                index = UnityEngine.Random.Range(0, _monsterPrefabNames.Count);
            }

            DoReplaceAtIndex(index);
        }

        /// <summary>
        /// 替换为指定索引的预制体
        /// </summary>
        private void DoReplaceAtIndex(int index)
        {
            if (index < 0 || index >= _monsterPrefabNames.Count) return;
            if (_replacement == null) return;

            string prefabName = _monsterPrefabNames[index];
            GameObject prefab = LoadPrefab(prefabName);

            if (prefab == null)
            {
                Debug.LogError($"[MonsterReplacer] Failed to load prefab: {prefabName}");
                return;
            }

            // 保存当前的Transform信息
            Vector3 position = _replacement.position;
            Quaternion rotation = _replacement.rotation;
            Vector3 scale = _replacement.localScale;

            // 销毁当前实例
            ClearCurrentInstance();

            // 实例化新的预制体
            _currentInstance = Instantiate(prefab, _replacement);
            _currentInstance.name = prefabName;

            // 设置Transform
            if (_keepPosition)
            {
                // Y轴向上偏移，防止物理对象掉下去
                _currentInstance.transform.localPosition = new Vector3(0f, _spawnYOffset, 0f);
            }
            if (_keepRotation)
            {
                _currentInstance.transform.localRotation = Quaternion.identity;
            }
            if (_keepScale)
            {
                _currentInstance.transform.localScale = Vector3.one;
            }

            Debug.Log($"[MonsterReplacer] Replaced with: {prefabName} (index: {index})");

            OnReplaced?.Invoke(_currentInstance, index);
        }

        /// <summary>
        /// 加载预制体（带缓存）
        /// </summary>
        private GameObject LoadPrefab(string prefabName)
        {
            // 检查缓存
            if (_prefabCache.TryGetValue(prefabName, out var cached))
            {
                return cached;
            }

            // 从Resources加载
            string path = string.IsNullOrEmpty(_resourcePath) 
                ? prefabName 
                : $"{_resourcePath}/{prefabName}";
                
            var prefab = Resources.Load<GameObject>(path);
            
            if (prefab != null)
            {
                _prefabCache[prefabName] = prefab;
                Debug.Log($"[MonsterReplacer] Loaded prefab: {path}");
            }
            else
            {
                Debug.LogError($"[MonsterReplacer] Prefab not found at: Resources/{path}");
            }

            return prefab;
        }

        #endregion

        #region Editor

#if UNITY_EDITOR
        /// <summary>
        /// 编辑器下预览替换
        /// </summary>
        [ContextMenu("Preview Next Replace")]
        private void EditorPreviewReplace()
        {
            if (_monsterPrefabNames.Count == 0)
            {
                Debug.LogWarning("[MonsterReplacer] No prefab names configured");
                return;
            }
            
            DoReplace();
        }

        /// <summary>
        /// 编辑器下重置
        /// </summary>
        [ContextMenu("Reset Index")]
        private void EditorReset()
        {
            ResetIndex();
            Debug.Log("[MonsterReplacer] Index reset to 0");
        }
#endif

        #endregion
    }
}
