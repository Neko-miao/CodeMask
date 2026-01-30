// ================================================
// GameFramework - 单局配置
// ================================================

using System;

namespace GameFramework.Session
{
    /// <summary>
    /// 单局配置
    /// </summary>
    [Serializable]
    public class SessionConfig
    {
        /// <summary>
        /// 起始关卡ID
        /// </summary>
        public int StartLevelId = 1;
        
        /// <summary>
        /// 单局规则
        /// </summary>
        public ISessionRule[] Rules;
        
        /// <summary>
        /// 是否自动开始关卡
        /// </summary>
        public bool AutoStartLevel = true;
        
        /// <summary>
        /// 自定义数据
        /// </summary>
        public object CustomData;
    }
}

