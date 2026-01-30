// ================================================
// GameFramework - 游戏状态定义
// ================================================

namespace GameFramework.Core
{
    /// <summary>
    /// 游戏状态枚举
    /// </summary>
    public enum GameState
    {
        /// <summary>
        /// 全局状态 - 组件始终存在
        /// </summary>
        Global = 0,
        
        /// <summary>
        /// 未初始化
        /// </summary>
        None = 1,
        
        /// <summary>
        /// 初始化中
        /// </summary>
        Init = 2,
        
        /// <summary>
        /// 加载中
        /// </summary>
        Loading = 3,
        
        /// <summary>
        /// 主菜单
        /// </summary>
        Menu = 4,
        
        /// <summary>
        /// 游戏中
        /// </summary>
        Playing = 5,
        
        /// <summary>
        /// 暂停
        /// </summary>
        Paused = 6,
        
        /// <summary>
        /// 游戏结束
        /// </summary>
        GameOver = 7,
        
        /// <summary>
        /// 关闭中
        /// </summary>
        Shutdown = 8
    }
    
    /// <summary>
    /// 组件类型
    /// </summary>
    public enum ComponentType
    {
        /// <summary>
        /// 核心组件
        /// </summary>
        Core = 0,
        
        /// <summary>
        /// 系统组件
        /// </summary>
        System = 1,
        
        /// <summary>
        /// 功能模块
        /// </summary>
        Module = 2,
        
        /// <summary>
        /// 自定义组件
        /// </summary>
        Custom = 3
    }
}

