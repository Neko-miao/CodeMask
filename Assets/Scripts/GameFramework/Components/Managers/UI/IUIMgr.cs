// ================================================
// GameFramework - UI管理器接口
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;

namespace GameFramework.UI
{
    /// <summary>
    /// UI管理器接口
    /// </summary>
    public interface IUIMgr : IGameComponent
    {
        #region Properties
        
        /// <summary>
        /// UI 相机
        /// </summary>
        Camera UICamera { get; }
        
        /// <summary>
        /// UI 根节点
        /// </summary>
        Transform UIRoot { get; }
        
        /// <summary>
        /// 根 Canvas
        /// </summary>
        Canvas RootCanvas { get; }
        
        #endregion
        
        #region Open/Close (Controller-Based)
        
        /// <summary>
        /// 打开界面（以 Controller 为核心）
        /// </summary>
        TController Open<TController>(object data = null) where TController : class, IUIController, new();
        
        /// <summary>
        /// 打开界面 (协程)
        /// </summary>
        Coroutine OpenAsync<TController>(Action<TController> onComplete, object data = null) where TController : class, IUIController, new();
        
        /// <summary>
        /// 关闭界面（通过 Controller 类型）
        /// </summary>
        void Close<TController>() where TController : class, IUIController;
        
        /// <summary>
        /// 关闭界面（通过 Controller 实例）
        /// </summary>
        void Close(IUIController controller);
        
        /// <summary>
        /// 关闭界面（通过 View 实例）
        /// </summary>
        void CloseByView(IUIView view);
        
        /// <summary>
        /// 关闭所有界面
        /// </summary>
        void CloseAll(UILayer layer = UILayer.All);
        
        #endregion
        
        #region Query
        
        /// <summary>
        /// 获取 Controller 实例
        /// </summary>
        TController Get<TController>() where TController : class, IUIController;
        
        /// <summary>
        /// 尝试获取 Controller 实例
        /// </summary>
        bool TryGet<TController>(out TController controller) where TController : class, IUIController;
        
        /// <summary>
        /// 检查界面是否打开（通过 Controller 类型）
        /// </summary>
        bool IsOpen<TController>() where TController : class, IUIController;
        
        /// <summary>
        /// 检查界面是否打开（通过 Controller 实例）
        /// </summary>
        bool IsOpen(IUIController controller);
        
        /// <summary>
        /// 获取指定层级的所有 Controller
        /// </summary>
        IReadOnlyList<IUIController> GetControllersByLayer(UILayer layer);
        
        /// <summary>
        /// 获取所有打开的 Controller
        /// </summary>
        IReadOnlyList<IUIController> GetAllOpenControllers();
        
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
        /// 预加载界面 (协程)
        /// </summary>
        Coroutine PreloadAsync<TController>(Action onComplete = null) where TController : class, IUIController, new();
        
        /// <summary>
        /// 预加载界面 (协程)
        /// </summary>
        Coroutine PreloadAsync(string viewPath, Action onComplete = null);
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Controller 打开事件
        /// </summary>
        event Action<IUIController> OnControllerOpened;
        
        /// <summary>
        /// Controller 关闭事件
        /// </summary>
        event Action<IUIController> OnControllerClosed;
        
        #endregion
    }
}
