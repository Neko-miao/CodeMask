// ================================================
// GameFramework - 配置管理器实现
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 配置管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 30, RequiredStates = new[] { GameState.Global })]
    public class ConfigMgr : GameComponent, IConfigMgr
    {
        private readonly Dictionary<Type, object> _configs = new Dictionary<Type, object>();
        private readonly Dictionary<Type, string> _configPaths = new Dictionary<Type, string>();
        
        public override string ComponentName => "ConfigMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 30;
        
        #region Load
        
        /// <summary>
        /// 加载配置表
        /// </summary>
        public T LoadConfig<T>(string configName) where T : class
        {
            var type = typeof(T);
            
            // 尝试从Resources加载JSON配置
            var textAsset = Resources.Load<TextAsset>($"Configs/{configName}");
            
            if (textAsset != null)
            {
                try
                {
                    var config = JsonUtility.FromJson<T>(textAsset.text);
                    _configs[type] = config;
                    _configPaths[type] = configName;
                    Debug.Log($"[ConfigMgr] Loaded config: {configName}");
                    return config;
                }
                catch (Exception e)
                {
                    Debug.LogError($"[ConfigMgr] Failed to parse config {configName}: {e.Message}");
                }
            }
            else
            {
                Debug.LogWarning($"[ConfigMgr] Config not found: Configs/{configName}");
            }
            
            return null;
        }
        
        #endregion
        
        #region Get
        
        /// <summary>
        /// 获取配置
        /// </summary>
        public T GetConfig<T>() where T : class
        {
            if (_configs.TryGetValue(typeof(T), out var config))
            {
                return config as T;
            }
            return null;
        }
        
        /// <summary>
        /// 获取配置项
        /// </summary>
        public T GetConfigItem<T>(int id) where T : class
        {
            var config = GetConfig<IConfigTable<T>>();
            return config?.Get(id);
        }
        
        /// <summary>
        /// 获取配置项
        /// </summary>
        public T GetConfigItem<T>(string key) where T : class
        {
            var config = GetConfig<IConfigTable<T>>();
            return config?.Get(key);
        }
        
        /// <summary>
        /// 获取所有配置项
        /// </summary>
        public IReadOnlyList<T> GetAllConfigItems<T>() where T : class
        {
            var config = GetConfig<IConfigTable<T>>();
            return config?.GetAll();
        }
        
        #endregion
        
        #region Register
        
        /// <summary>
        /// 注册配置
        /// </summary>
        public void RegisterConfig<T>(T config) where T : class
        {
            if (config == null)
            {
                Debug.LogWarning("[ConfigMgr] Cannot register null config");
                return;
            }
            
            _configs[typeof(T)] = config;
            Debug.Log($"[ConfigMgr] Registered config: {typeof(T).Name}");
        }
        
        /// <summary>
        /// 注销配置
        /// </summary>
        public void UnregisterConfig<T>() where T : class
        {
            var type = typeof(T);
            _configs.Remove(type);
            _configPaths.Remove(type);
        }
        
        #endregion
        
        #region Reload
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        public void ReloadConfig<T>(string configName) where T : class
        {
            UnregisterConfig<T>();
            LoadConfig<T>(configName);
        }
        
        /// <summary>
        /// 重新加载所有配置
        /// </summary>
        public void ReloadAll()
        {
            var paths = new Dictionary<Type, string>(_configPaths);
            _configs.Clear();
            _configPaths.Clear();
            
            foreach (var kvp in paths)
            {
                var textAsset = Resources.Load<TextAsset>($"Configs/{kvp.Value}");
                if (textAsset != null)
                {
                    try
                    {
                        var config = JsonUtility.FromJson(textAsset.text, kvp.Key);
                        _configs[kvp.Key] = config;
                        _configPaths[kvp.Key] = kvp.Value;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"[ConfigMgr] Failed to reload config {kvp.Value}: {e.Message}");
                    }
                }
            }
        }
        
        #endregion
        
        #region Query
        
        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        public bool HasConfig<T>() where T : class
        {
            return _configs.ContainsKey(typeof(T));
        }
        
        /// <summary>
        /// 清空所有配置
        /// </summary>
        public void Clear()
        {
            _configs.Clear();
            _configPaths.Clear();
        }
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnShutdown()
        {
            Clear();
        }
        
        #endregion
    }
}

