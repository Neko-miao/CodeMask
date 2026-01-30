// ================================================
// GameFramework - UI控制器接口
// ================================================

using System;

namespace GameFramework.UI
{
    /// <summary>
    /// UI控制器接口
    /// </summary>
    public interface IUIController
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize(IUIView view, IUIModel model);
        
        /// <summary>
        /// 绑定事件
        /// </summary>
        void BindEvents();
        
        /// <summary>
        /// 解绑事件
        /// </summary>
        void UnbindEvents();
        
        /// <summary>
        /// 更新视图
        /// </summary>
        void UpdateView();
        
        /// <summary>
        /// 清理
        /// </summary>
        void Clear();
    }
    
    /// <summary>
    /// UI控制器接口 (泛型)
    /// </summary>
    public interface IUIController<TView, TModel> : IUIController 
        where TView : IUIView 
        where TModel : IUIModel
    {
        /// <summary>
        /// 视图
        /// </summary>
        TView View { get; }
        
        /// <summary>
        /// 数据模型
        /// </summary>
        TModel Model { get; }
    }
}

