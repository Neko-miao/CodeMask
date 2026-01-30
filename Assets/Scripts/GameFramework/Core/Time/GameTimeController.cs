// ================================================
// GameFramework - 游戏时间控制器实现
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏时间控制器实现
    /// </summary>
    public class GameTimeController : IGameTimeController
    {
        #region Fields
        
        private float _globalTimeScale = 1f;
        private float _gameTime = 0f;
        private float _realTime = 0f;
        private float _deltaTime = 0f;
        private float _unscaledDeltaTime = 0f;
        private float _fixedDeltaTime = 0f;
        private float _unscaledFixedDeltaTime = 0f;
        private int _frameCount = 0;
        private bool _isPaused = false;
        private float _timeScaleBeforePause = 1f;
        
        private readonly Dictionary<Type, float> _componentTimeScales = new Dictionary<Type, float>();
        private readonly Dictionary<string, ITimeLayer> _timeLayers = new Dictionary<string, ITimeLayer>();
        
        #endregion
        
        #region Properties
        
        /// <summary>
        /// 全局时间缩放
        /// </summary>
        public float GlobalTimeScale => _globalTimeScale;
        
        /// <summary>
        /// 游戏运行总时间
        /// </summary>
        public float GameTime => _gameTime;
        
        /// <summary>
        /// 真实运行总时间
        /// </summary>
        public float RealTime => _realTime;
        
        /// <summary>
        /// 帧间隔
        /// </summary>
        public float DeltaTime => _deltaTime;
        
        /// <summary>
        /// 帧间隔 (不受缩放影响)
        /// </summary>
        public float UnscaledDeltaTime => _unscaledDeltaTime;
        
        /// <summary>
        /// 固定帧间隔
        /// </summary>
        public float FixedDeltaTime => _fixedDeltaTime;
        
        /// <summary>
        /// 固定帧间隔 (不受缩放影响)
        /// </summary>
        public float UnscaledFixedDeltaTime => _unscaledFixedDeltaTime;
        
        /// <summary>
        /// 当前帧数
        /// </summary>
        public int FrameCount => _frameCount;
        
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool IsPaused => _isPaused;
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 时间缩放改变事件
        /// </summary>
        public event Action<float> OnTimeScaleChanged;
        
        /// <summary>
        /// 暂停事件
        /// </summary>
        public event Action OnPaused;
        
        /// <summary>
        /// 恢复事件
        /// </summary>
        public event Action OnResumed;
        
        #endregion
        
        #region Update
        
        /// <summary>
        /// 每帧更新 (由GameInstance调用)
        /// </summary>
        public void Update()
        {
            _unscaledDeltaTime = Time.unscaledDeltaTime;
            _deltaTime = _unscaledDeltaTime * _globalTimeScale;
            
            _realTime += _unscaledDeltaTime;
            _gameTime += _deltaTime;
            
            _frameCount = Time.frameCount;
        }
        
        /// <summary>
        /// 固定更新 (由GameInstance调用)
        /// </summary>
        public void FixedUpdate()
        {
            _unscaledFixedDeltaTime = Time.fixedUnscaledDeltaTime;
            _fixedDeltaTime = _unscaledFixedDeltaTime * _globalTimeScale;
        }
        
        #endregion
        
        #region Time Control
        
        /// <summary>
        /// 设置全局时间缩放
        /// </summary>
        public void SetGlobalTimeScale(float scale)
        {
            scale = Mathf.Max(0f, scale);
            
            if (Mathf.Approximately(_globalTimeScale, scale))
                return;
            
            _globalTimeScale = scale;
            Time.timeScale = scale;
            
            OnTimeScaleChanged?.Invoke(scale);
            
            Debug.Log($"[GameTimeController] Global time scale set to: {scale}");
        }
        
        /// <summary>
        /// 暂停时间
        /// </summary>
        public void Pause()
        {
            if (_isPaused) return;
            
            _timeScaleBeforePause = _globalTimeScale;
            _isPaused = true;
            SetGlobalTimeScale(0f);
            
            OnPaused?.Invoke();
            
            Debug.Log("[GameTimeController] Time paused");
        }
        
        /// <summary>
        /// 恢复时间
        /// </summary>
        public void Resume()
        {
            if (!_isPaused) return;
            
            _isPaused = false;
            SetGlobalTimeScale(_timeScaleBeforePause);
            
            OnResumed?.Invoke();
            
            Debug.Log("[GameTimeController] Time resumed");
        }
        
        /// <summary>
        /// 切换暂停状态
        /// </summary>
        public void TogglePause()
        {
            if (_isPaused)
                Resume();
            else
                Pause();
        }
        
        /// <summary>
        /// 重置时间
        /// </summary>
        public void Reset()
        {
            _globalTimeScale = 1f;
            _gameTime = 0f;
            _realTime = 0f;
            _isPaused = false;
            _timeScaleBeforePause = 1f;
            
            Time.timeScale = 1f;
            
            _componentTimeScales.Clear();
            _timeLayers.Clear();
            
            Debug.Log("[GameTimeController] Time reset");
        }
        
        #endregion
        
        #region Component Time Scale
        
        /// <summary>
        /// 设置组件时间缩放
        /// </summary>
        public void SetComponentTimeScale<T>(float scale) where T : class, IGameComponent
        {
            SetComponentTimeScale(typeof(T), scale);
        }
        
        /// <summary>
        /// 设置组件时间缩放
        /// </summary>
        public void SetComponentTimeScale(Type componentType, float scale)
        {
            scale = Mathf.Max(0f, scale);
            _componentTimeScales[componentType] = scale;
        }
        
        /// <summary>
        /// 获取组件时间缩放
        /// </summary>
        public float GetComponentTimeScale<T>() where T : class, IGameComponent
        {
            return GetComponentTimeScale(typeof(T));
        }
        
        /// <summary>
        /// 获取组件时间缩放
        /// </summary>
        public float GetComponentTimeScale(Type componentType)
        {
            if (_componentTimeScales.TryGetValue(componentType, out var scale))
                return scale;
            return 1f;
        }
        
        /// <summary>
        /// 重置组件时间缩放
        /// </summary>
        public void ResetComponentTimeScale<T>() where T : class, IGameComponent
        {
            _componentTimeScales.Remove(typeof(T));
        }
        
        /// <summary>
        /// 获取组件缩放后的DeltaTime
        /// </summary>
        public float GetScaledDeltaTime<T>() where T : class, IGameComponent
        {
            float componentScale = GetComponentTimeScale<T>();
            return _deltaTime * componentScale;
        }
        
        #endregion
        
        #region Time Layer
        
        /// <summary>
        /// 创建时间层
        /// </summary>
        public ITimeLayer CreateTimeLayer(string layerName)
        {
            if (_timeLayers.ContainsKey(layerName))
            {
                Debug.LogWarning($"[GameTimeController] Time layer '{layerName}' already exists");
                return _timeLayers[layerName];
            }
            
            var layer = new TimeLayer(layerName);
            _timeLayers[layerName] = layer;
            
            Debug.Log($"[GameTimeController] Created time layer: {layerName}");
            return layer;
        }
        
        /// <summary>
        /// 获取时间层
        /// </summary>
        public ITimeLayer GetTimeLayer(string layerName)
        {
            _timeLayers.TryGetValue(layerName, out var layer);
            return layer;
        }
        
        /// <summary>
        /// 移除时间层
        /// </summary>
        public void RemoveTimeLayer(string layerName)
        {
            if (_timeLayers.Remove(layerName))
            {
                Debug.Log($"[GameTimeController] Removed time layer: {layerName}");
            }
        }
        
        /// <summary>
        /// 设置时间层缩放
        /// </summary>
        public void SetTimeLayerScale(string layerName, float scale)
        {
            if (_timeLayers.TryGetValue(layerName, out var layer))
            {
                layer.TimeScale = Mathf.Max(0f, scale);
            }
        }
        
        /// <summary>
        /// 获取时间层DeltaTime
        /// </summary>
        public float GetTimeLayerDeltaTime(string layerName)
        {
            if (_timeLayers.TryGetValue(layerName, out var layer))
            {
                return layer.GetDeltaTime(_deltaTime);
            }
            return _deltaTime;
        }
        
        #endregion
    }
}

