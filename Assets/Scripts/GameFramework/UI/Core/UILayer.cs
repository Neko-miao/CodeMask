// ================================================
// GameFramework - UI层级定义
// ================================================

using System;

namespace GameFramework.UI
{
    /// <summary>
    /// UI层级
    /// </summary>
    [Flags]
    public enum UILayer
    {
        /// <summary>
        /// 所有层
        /// </summary>
        All = -1,
        
        /// <summary>
        /// 背景层
        /// </summary>
        Background = 0,
        
        /// <summary>
        /// 普通层
        /// </summary>
        Normal = 100,
        
        /// <summary>
        /// 弹窗层
        /// </summary>
        Popup = 200,
        
        /// <summary>
        /// 提示层
        /// </summary>
        Toast = 300,
        
        /// <summary>
        /// 引导层
        /// </summary>
        Guide = 400,
        
        /// <summary>
        /// 最顶层
        /// </summary>
        Top = 500
    }
}

