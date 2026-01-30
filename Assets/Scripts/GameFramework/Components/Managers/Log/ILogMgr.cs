// ================================================
// GameFramework - 日志管理器接口
// ================================================

using System;
using GameFramework.Core;

namespace GameFramework.Components
{
    /// <summary>
    /// 日志级别
    /// </summary>
    public enum LogLevel
    {
        Debug = 0,
        Info = 1,
        Warning = 2,
        Error = 3,
        None = 4
    }
    
    /// <summary>
    /// 日志管理器接口
    /// </summary>
    public interface ILogMgr : IGameComponent
    {
        /// <summary>
        /// 调试日志
        /// </summary>
        void Debug(string message);
        
        /// <summary>
        /// 调试日志 (带格式化)
        /// </summary>
        void Debug(string format, params object[] args);
        
        /// <summary>
        /// 信息日志
        /// </summary>
        void Info(string message);
        
        /// <summary>
        /// 信息日志 (带格式化)
        /// </summary>
        void Info(string format, params object[] args);
        
        /// <summary>
        /// 警告日志
        /// </summary>
        void Warning(string message);
        
        /// <summary>
        /// 警告日志 (带格式化)
        /// </summary>
        void Warning(string format, params object[] args);
        
        /// <summary>
        /// 错误日志
        /// </summary>
        void Error(string message);
        
        /// <summary>
        /// 错误日志 (带格式化)
        /// </summary>
        void Error(string format, params object[] args);
        
        /// <summary>
        /// 带标签的日志
        /// </summary>
        void Log(LogLevel level, string tag, string message);
        
        /// <summary>
        /// 设置日志级别
        /// </summary>
        void SetLogLevel(LogLevel level);
        
        /// <summary>
        /// 获取当前日志级别
        /// </summary>
        LogLevel GetLogLevel();
        
        /// <summary>
        /// 启用/禁用标签
        /// </summary>
        void SetTagEnabled(string tag, bool enabled);
        
        /// <summary>
        /// 检查标签是否启用
        /// </summary>
        bool IsTagEnabled(string tag);
        
        /// <summary>
        /// 添加日志监听器
        /// </summary>
        void AddListener(Action<LogLevel, string, string> listener);
        
        /// <summary>
        /// 移除日志监听器
        /// </summary>
        void RemoveListener(Action<LogLevel, string, string> listener);
    }
}

