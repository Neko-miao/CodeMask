// ================================================
// GameFramework - UI控制器接口
// ================================================

namespace GameFramework.UI
{
    /// <summary>
    /// UI控制器接口 - Controller是MVC的核心，持有Model和View的引用
    /// </summary>
    public interface IUIController
    {
        /// <summary>
        /// View 路径（用于加载 Prefab）
        /// </summary>
        string ViewPath { get; }
        
        /// <summary>
        /// View 实例
        /// </summary>
        IUIView View { get; }
        
        /// <summary>
        /// Model 实例
        /// </summary>
        IUIModel Model { get; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 初始化 Controller（由 UIMgr 调用，传入加载的 View）
        /// </summary>
        void Initialize(IUIView view);
        
        /// <summary>
        /// 打开 UI
        /// </summary>
        void Open(object data = null);
        
        /// <summary>
        /// 关闭 UI
        /// </summary>
        void Close();
        
        /// <summary>
        /// 刷新视图
        /// </summary>
        void Refresh();
        
        /// <summary>
        /// 销毁 Controller 及其持有的 Model 和 View
        /// </summary>
        void Destroy();
    }
    
    /// <summary>
    /// UI控制器接口 (泛型)
    /// </summary>
    public interface IUIController<TView, TModel> : IUIController 
        where TView : class, IUIView 
        where TModel : class, IUIModel
    {
        /// <summary>
        /// 强类型视图
        /// </summary>
        new TView View { get; }
        
        /// <summary>
        /// 强类型数据模型
        /// </summary>
        new TModel Model { get; }
    }
}

