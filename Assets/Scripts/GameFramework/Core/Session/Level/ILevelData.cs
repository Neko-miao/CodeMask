// ================================================
// GameFramework - 关卡运行时数据接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Session
{
    /// <summary>
    /// 关卡运行时数据接口
    /// </summary>
    public interface ILevelData
    {
        /// <summary>
        /// 已进行时间
        /// </summary>
        float ElapsedTime { get; set; }
        
        /// <summary>
        /// 当前分数
        /// </summary>
        int Score { get; set; }
        
        /// <summary>
        /// 当前检查点
        /// </summary>
        string CurrentCheckpoint { get; set; }
        
        /// <summary>
        /// 受伤次数
        /// </summary>
        int DamageCount { get; set; }
        
        /// <summary>
        /// 获取自定义数据
        /// </summary>
        T GetData<T>(string key);
        
        /// <summary>
        /// 设置自定义数据
        /// </summary>
        void SetData<T>(string key, T value);
        
        /// <summary>
        /// 重置数据
        /// </summary>
        void Reset();
    }
    
    /// <summary>
    /// 关卡运行时数据实现
    /// </summary>
    public class LevelRuntimeData : ILevelData
    {
        private readonly Dictionary<string, object> _customData = new Dictionary<string, object>();
        
        public float ElapsedTime { get; set; }
        public int Score { get; set; }
        public string CurrentCheckpoint { get; set; }
        public int DamageCount { get; set; }
        
        public T GetData<T>(string key)
        {
            if (_customData.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            return default;
        }
        
        public void SetData<T>(string key, T value)
        {
            _customData[key] = value;
        }
        
        public void Reset()
        {
            ElapsedTime = 0f;
            Score = 0;
            CurrentCheckpoint = null;
            DamageCount = 0;
            _customData.Clear();
        }
    }
}
