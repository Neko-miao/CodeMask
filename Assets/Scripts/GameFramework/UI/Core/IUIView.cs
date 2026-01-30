// ================================================
// GameFramework - UI视图接口
// ================================================

using System;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// UI视图接口
    /// </summary>
    public interface IUIView
    {
        /// <summary>
        /// 视图名称
        /// </summary>
        string ViewName { get; }
        
        /// <summary>
        /// 视图层级
        /// </summary>
        UILayer Layer { get; }
        
        /// <summary>
        /// 是否可见
        /// </summary>
        bool IsVisible { get; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// GameObject引用
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// RectTransform引用
        /// </summary>
        RectTransform RectTransform { get; }
        
        /// <summary>
        /// Canvas引用
        /// </summary>
        Canvas Canvas { get; }
        
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 打开
        /// </summary>
        void Open(object data = null);
        
        /// <summary>
        /// 关闭
        /// </summary>
        void Close();
        
        /// <summary>
        /// 刷新
        /// </summary>
        void Refresh();
        
        /// <summary>
        /// 销毁
        /// </summary>
        void Destroy();
        
        /// <summary>
        /// 设置可见性
        /// </summary>
        void SetVisible(bool visible);
        
        /// <summary>
        /// 打开事件
        /// </summary>
        event Action<IUIView> OnOpened;
        
        /// <summary>
        /// 关闭事件
        /// </summary>
        event Action<IUIView> OnClosed;
    }
}

