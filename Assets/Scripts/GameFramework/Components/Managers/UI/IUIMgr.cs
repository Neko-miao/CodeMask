// ================================================
// GameFramework - UI管理器接口
// ================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Core;

namespace GameFramework.UI
{
    /// <summary>
    /// UI管理器接口
    /// </summary>
    public interface IUIMgr : IGameComponent
    {
        #region Open/Close
        
        /// <summary>
        /// 打开界面
        /// </summary>
        T Open<T>(object data = null) where T : class, IUIView;
        
        /// <summary>
        /// 打开界面 (异步)
        /// </summary>
        Task<T> OpenAsync<T>(object data = null) where T : class, IUIView;
        
        /// <summary>
        /// 关闭界面
        /// </summary>
        void Close<T>() where T : class, IUIView;
        
        /// <summary>
        /// 关闭界面
        /// </summary>
        void Close(IUIView view);
        
        /// <summary>
        /// 关闭所有界面
        /// </summary>
        void CloseAll(UILayer layer = UILayer.All);
        
        #endregion
        
        #region Query
        
        /// <summary>
        /// 获取界面实例
        /// </summary>
        T Get<T>() where T : class, IUIView;
        
        /// <summary>
        /// 尝试获取界面实例
        /// </summary>
        bool TryGet<T>(out T view) where T : class, IUIView;
        
        /// <summary>
        /// 检查界面是否打开
        /// </summary>
        bool IsOpen<T>() where T : class, IUIView;
        
        /// <summary>
        /// 检查界面是否打开
        /// </summary>
        bool IsOpen(IUIView view);
        
        /// <summary>
        /// 获取指定层级的所有界面
        /// </summary>
        IReadOnlyList<IUIView> GetViewsByLayer(UILayer layer);
        
        /// <summary>
        /// 获取所有打开的界面
        /// </summary>
        IReadOnlyList<IUIView> GetAllOpenViews();
        
        #endregion
        
        #region Layer Management
        
        /// <summary>
        /// 设置层级排序
        /// </summary>
        void SetLayerOrder(UILayer layer, int order);
        
        /// <summary>
        /// 获取层级排序
        /// </summary>
        int GetLayerOrder(UILayer layer);
        
        #endregion
        
        #region Preload
        
        /// <summary>
        /// 预加载界面
        /// </summary>
        Task PreloadAsync<T>() where T : class, IUIView;
        
        /// <summary>
        /// 预加载界面
        /// </summary>
        Task PreloadAsync(string viewPath);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// 界面打开事件
        /// </summary>
        event Action<IUIView> OnViewOpened;
        
        /// <summary>
        /// 界面关闭事件
        /// </summary>
        event Action<IUIView> OnViewClosed;
        
        #endregion
    }
}

