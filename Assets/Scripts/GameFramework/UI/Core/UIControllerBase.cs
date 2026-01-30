// ================================================
// GameFramework - UI控制器基类
// ================================================

using System;
using GameFramework.Core;

namespace GameFramework.UI
{
    /// <summary>
    /// UI控制器基类
    /// </summary>
    public abstract class UIControllerBase<TView, TModel> : IUIController<TView, TModel>
        where TView : class, IUIView
        where TModel : class, IUIModel
    {
        private TView _view;
        private TModel _model;
        
        public TView View => _view;
        public TModel Model => _model;
        
        public void Initialize(IUIView view, IUIModel model)
        {
            _view = view as TView;
            _model = model as TModel;
            
            if (_view == null || _model == null)
            {
                throw new ArgumentException("View or Model type mismatch");
            }
            
            OnInitialize();
        }
        
        public virtual void BindEvents()
        {
            if (_model != null)
            {
                _model.OnDataChanged += OnModelDataChanged;
            }
            
            OnBindEvents();
        }
        
        public virtual void UnbindEvents()
        {
            if (_model != null)
            {
                _model.OnDataChanged -= OnModelDataChanged;
            }
            
            OnUnbindEvents();
        }
        
        public virtual void UpdateView()
        {
            OnUpdateView();
        }
        
        public virtual void Clear()
        {
            UnbindEvents();
            OnClear();
            _view = null;
            _model = null;
        }
        
        #region Protected Virtual Methods
        
        /// <summary>
        /// 初始化时调用
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// 绑定事件时调用
        /// </summary>
        protected virtual void OnBindEvents() { }
        
        /// <summary>
        /// 解绑事件时调用
        /// </summary>
        protected virtual void OnUnbindEvents() { }
        
        /// <summary>
        /// 更新视图时调用
        /// </summary>
        protected virtual void OnUpdateView() { }
        
        /// <summary>
        /// 清理时调用
        /// </summary>
        protected virtual void OnClear() { }
        
        /// <summary>
        /// 数据模型改变时调用
        /// </summary>
        protected virtual void OnModelDataChanged()
        {
            UpdateView();
        }
        
        #endregion
        
        #region Helper Methods
        
        /// <summary>
        /// 获取组件
        /// </summary>
        protected T GetComp<T>() where T : class, IGameComponent
        {
            return GameInstance.Instance?.GetComp<T>();
        }
        
        #endregion
    }
}

