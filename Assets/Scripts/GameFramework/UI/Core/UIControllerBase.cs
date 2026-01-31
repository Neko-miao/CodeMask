// ================================================
// GameFramework - UI控制器基类
// ================================================

using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// UI控制器基类 - Controller是MVC的核心，持有Model和View的引用
    /// </summary>
    public abstract class UIControllerBase<TView, TModel> : IUIController<TView, TModel>
        where TView : class, IUIView
        where TModel : class, IUIModel, new()
    {
        private TView _view;
        private TModel _model;
        private bool _isInitialized;
        
        #region Properties
        
        /// <summary>
        /// View 路径（默认使用 View 类型名称）
        /// </summary>
        public virtual string ViewPath => $"UI/{typeof(TView).Name}";
        
        public TView View => _view;
        public TModel Model => _model;
        
        IUIView IUIController.View => _view;
        IUIModel IUIController.Model => _model;
        
        public bool IsInitialized => _isInitialized;
        
        #endregion
        
        #region IUIController Implementation
        
        public void Initialize(IUIView view)
        {
            if (_isInitialized)
            {
                Debug.LogWarning($"[{GetType().Name}] Already initialized");
                return;
            }
            
            _view = view as TView;
            if (_view == null)
            {
                throw new ArgumentException($"View type mismatch. Expected {typeof(TView).Name}, got {view?.GetType().Name}");
            }
            
            // 创建 Model
            _model = new TModel();
            
            // 初始化
            OnInitialize();
            
            _isInitialized = true;
        }
        
        public void Open(object data = null)
        {
            if (!_isInitialized)
            {
                Debug.LogError($"[{GetType().Name}] Not initialized");
                return;
            }
            
            // 初始化 Model 数据
            OnModelInit(data);
            
            // 绑定事件
            BindEvents();
            
            // 打开 View
            _view.Open(data);
            
            // 更新视图
            UpdateView();
            
            OnOpen(data);
        }
        
        public void Close()
        {
            if (!_isInitialized) return;
            
            OnClose();
            
            // 解绑事件
            UnbindEvents();
            
            // 关闭 View
            _view?.Close();
        }
        
        public void Refresh()
        {
            if (!_isInitialized) return;
            
            UpdateView();
            _view?.Refresh();
        }
        
        public void Destroy()
        {
            if (!_isInitialized) return;
            
            Close();
            
            OnDestroy();
            
            // 清理 Model
            _model?.Clear();
            _model = null;
            
            // 销毁 View
            if (_view != null)
            {
                _view.Destroy();
                _view = null;
            }
            
            _isInitialized = false;
        }
        
        #endregion
        
        #region Protected Methods
        
        protected virtual void BindEvents()
        {
            if (_model != null)
            {
                _model.OnDataChanged += OnModelDataChanged;
            }
            
            OnBindEvents();
        }
        
        protected virtual void UnbindEvents()
        {
            if (_model != null)
            {
                _model.OnDataChanged -= OnModelDataChanged;
            }
            
            OnUnbindEvents();
        }
        
        protected virtual void UpdateView()
        {
            OnUpdateView();
        }
        
        #endregion
        
        #region Protected Virtual Methods
        
        /// <summary>
        /// 初始化时调用（View 和 Model 已创建）
        /// </summary>
        protected virtual void OnInitialize() { }
        
        /// <summary>
        /// Model 初始化数据（在 Open 之前调用）
        /// </summary>
        protected virtual void OnModelInit(object data) { }
        
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
        /// 打开完成后调用
        /// </summary>
        protected virtual void OnOpen(object data) { }
        
        /// <summary>
        /// 关闭前调用
        /// </summary>
        protected virtual void OnClose() { }
        
        /// <summary>
        /// 销毁前调用
        /// </summary>
        protected virtual void OnDestroy() { }
        
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
        
        /// <summary>
        /// 关闭自己（通过 UIMgr）
        /// </summary>
        protected void CloseSelf()
        {
            GetComp<IUIMgr>()?.Close(this);
        }
        
        #endregion
    }
}

