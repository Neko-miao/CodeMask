// ================================================
// GameFramework - 单局上下文实现
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Session
{
    /// <summary>
    /// 单局上下文实现
    /// </summary>
    public class SessionContext : ISessionContext
    {
        private readonly Dictionary<string, object> _customData = new Dictionary<string, object>();
        
        #region Properties
        
        public string SessionId { get; private set; }
        public float StartTime { get; private set; }
        public float ElapsedTime { get; set; }
        public int CurrentLevelId { get; set; }
        public float LevelProgress { get; set; }
        public int Score { get; set; }
        public int MaxCombo { get; set; }
        public int CollectedCount { get; set; }
        public int KillCount { get; set; }
        public int DeathCount { get; set; }
        
        #endregion
        
        public SessionContext()
        {
            Reset();
        }
        
        #region Custom Data
        
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
        
        public bool HasData(string key)
        {
            return _customData.ContainsKey(key);
        }
        
        public void RemoveData(string key)
        {
            _customData.Remove(key);
        }
        
        #endregion
        
        public void Reset()
        {
            SessionId = Guid.NewGuid().ToString();
            StartTime = Time.time;
            ElapsedTime = 0f;
            CurrentLevelId = 0;
            LevelProgress = 0f;
            Score = 0;
            MaxCombo = 0;
            CollectedCount = 0;
            KillCount = 0;
            DeathCount = 0;
            _customData.Clear();
        }
    }
}

