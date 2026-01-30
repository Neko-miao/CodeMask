// ================================================
// GameFramework - 配置管理器接口
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;

namespace GameFramework.Components
{
    /// <summary>
    /// 配置管理器接口
    /// </summary>
    public interface IConfigMgr : IGameComponent
    {
        /// <summary>
        /// 加载配置表
        /// </summary>
        T LoadConfig<T>(string configName) where T : class;
        
        /// <summary>
        /// 获取配置
        /// </summary>
        T GetConfig<T>() where T : class;
        
        /// <summary>
        /// 获取配置项
        /// </summary>
        T GetConfigItem<T>(int id) where T : class;
        
        /// <summary>
        /// 获取配置项
        /// </summary>
        T GetConfigItem<T>(string key) where T : class;
        
        /// <summary>
        /// 获取所有配置项
        /// </summary>
        IReadOnlyList<T> GetAllConfigItems<T>() where T : class;
        
        /// <summary>
        /// 注册配置
        /// </summary>
        void RegisterConfig<T>(T config) where T : class;
        
        /// <summary>
        /// 注销配置
        /// </summary>
        void UnregisterConfig<T>() where T : class;
        
        /// <summary>
        /// 重新加载配置
        /// </summary>
        void ReloadConfig<T>(string configName) where T : class;
        
        /// <summary>
        /// 重新加载所有配置
        /// </summary>
        void ReloadAll();
        
        /// <summary>
        /// 检查配置是否存在
        /// </summary>
        bool HasConfig<T>() where T : class;
        
        /// <summary>
        /// 清空所有配置
        /// </summary>
        void Clear();
    }
    
    /// <summary>
    /// 配置表基类
    /// </summary>
    public interface IConfigTable<T>
    {
        /// <summary>
        /// 获取配置项
        /// </summary>
        T Get(int id);
        
        /// <summary>
        /// 获取配置项
        /// </summary>
        T Get(string key);
        
        /// <summary>
        /// 获取所有配置项
        /// </summary>
        IReadOnlyList<T> GetAll();
        
        /// <summary>
        /// 配置项数量
        /// </summary>
        int Count { get; }
    }
}

