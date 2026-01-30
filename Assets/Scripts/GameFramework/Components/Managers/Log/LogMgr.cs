// ================================================
// GameFramework - 日志管理器实现
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 日志管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 70, RequiredStates = new[] { GameState.Global })]
    public class LogMgr : GameComponent, ILogMgr
    {
        private LogLevel _logLevel = LogLevel.Debug;
        private readonly HashSet<string> _disabledTags = new HashSet<string>();
        private readonly List<Action<LogLevel, string, string>> _listeners = new List<Action<LogLevel, string, string>>();
        
        private const string DEFAULT_TAG = "Game";
        
        public override string ComponentName => "LogMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 70;
        
        #region Log Methods
        
        public void Debug(string message)
        {
            Log(LogLevel.Debug, DEFAULT_TAG, message);
        }
        
        public void Debug(string format, params object[] args)
        {
            Log(LogLevel.Debug, DEFAULT_TAG, string.Format(format, args));
        }
        
        public void Info(string message)
        {
            Log(LogLevel.Info, DEFAULT_TAG, message);
        }
        
        public void Info(string format, params object[] args)
        {
            Log(LogLevel.Info, DEFAULT_TAG, string.Format(format, args));
        }
        
        public void Warning(string message)
        {
            Log(LogLevel.Warning, DEFAULT_TAG, message);
        }
        
        public void Warning(string format, params object[] args)
        {
            Log(LogLevel.Warning, DEFAULT_TAG, string.Format(format, args));
        }
        
        public void Error(string message)
        {
            Log(LogLevel.Error, DEFAULT_TAG, message);
        }
        
        public void Error(string format, params object[] args)
        {
            Log(LogLevel.Error, DEFAULT_TAG, string.Format(format, args));
        }
        
        public void Log(LogLevel level, string tag, string message)
        {
            // 检查日志级别
            if (level < _logLevel)
                return;
            
            // 检查标签是否禁用
            if (_disabledTags.Contains(tag))
                return;
            
            string formattedMessage = $"[{tag}] {message}";
            
            // 输出到Unity控制台
            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Info:
                    UnityEngine.Debug.Log(formattedMessage);
                    break;
                case LogLevel.Warning:
                    UnityEngine.Debug.LogWarning(formattedMessage);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(formattedMessage);
                    break;
            }
            
            // 通知监听器
            foreach (var listener in _listeners)
            {
                try
                {
                    listener?.Invoke(level, tag, message);
                }
                catch (Exception e)
                {
                    UnityEngine.Debug.LogError($"[LogMgr] Listener error: {e.Message}");
                }
            }
        }
        
        #endregion
        
        #region Settings
        
        public void SetLogLevel(LogLevel level)
        {
            _logLevel = level;
        }
        
        public LogLevel GetLogLevel()
        {
            return _logLevel;
        }
        
        public void SetTagEnabled(string tag, bool enabled)
        {
            if (enabled)
                _disabledTags.Remove(tag);
            else
                _disabledTags.Add(tag);
        }
        
        public bool IsTagEnabled(string tag)
        {
            return !_disabledTags.Contains(tag);
        }
        
        #endregion
        
        #region Listeners
        
        public void AddListener(Action<LogLevel, string, string> listener)
        {
            if (listener != null && !_listeners.Contains(listener))
            {
                _listeners.Add(listener);
            }
        }
        
        public void RemoveListener(Action<LogLevel, string, string> listener)
        {
            _listeners.Remove(listener);
        }
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            #if UNITY_EDITOR || DEVELOPMENT_BUILD
            _logLevel = LogLevel.Debug;
            #else
            _logLevel = LogLevel.Warning;
            #endif
        }
        
        protected override void OnShutdown()
        {
            _listeners.Clear();
            _disabledTags.Clear();
        }
        
        #endregion
    }
}

