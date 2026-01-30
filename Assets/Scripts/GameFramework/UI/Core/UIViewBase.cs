// ================================================
// GameFramework - UI视图基类
// ================================================

using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// UI视图基类
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public abstract class UIViewBase : MonoBehaviour, IUIView
    {
        [SerializeField] protected UILayer _layer = UILayer.Normal;
        
        private Canvas _canvas;
        private RectTransform _rectTransform;
        private bool _isInitialized;
        private bool _isVisible;
        
        #region Properties
        
        public virtual string ViewName => GetType().Name;
        public UILayer Layer => _layer;
        public bool IsVisible => _isVisible;
        public bool IsInitialized => _isInitialized;
        public GameObject GameObject => gameObject;
        public RectTransform RectTransform => _rectTransform;
        public Canvas Canvas => _canvas;
        
        #endregion
        
        #region Events
        
        public event Action<IUIView> OnOpened;
        public event Action<IUIView> OnClosed;
        
        #endregion
        
        #region Unity Lifecycle
        
        protected virtual void Awake()
        {
            _canvas = GetComponent<Canvas>();
            _rectTransform = GetComponent<RectTransform>();
        }
        
        protected virtual void OnDestroy()
        {
            OnDestroyInternal();
        }
        
        #endregion
        
        #region IUIView Implementation
        
        public void Initialize()
        {
            if (_isInitialized) return;
            
            OnInit();
            _isInitialized = true;
        }
        
        public void Open(object data = null)
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            
            gameObject.SetActive(true);
            _isVisible = true;
            
            OnOpen(data);
            OnOpened?.Invoke(this);
        }
        
        public void Close()
        {
            if (!_isVisible) return;
            
            OnClose();
            
            _isVisible = false;
            gameObject.SetActive(false);
            
            OnClosed?.Invoke(this);
        }
        
        public void Refresh()
        {
            if (!_isVisible) return;
            OnRefresh();
        }
        
        public void Destroy()
        {
            Close();
            OnDestroyInternal();
            Destroy(gameObject);
        }
        
        public void SetVisible(bool visible)
        {
            if (_isVisible == visible) return;
            
            _isVisible = visible;
            gameObject.SetActive(visible);
            
            OnVisibilityChanged(visible);
        }
        
        #endregion
        
        #region Protected Virtual Methods
        
        /// <summary>
        /// 初始化时调用 (只调用一次)
        /// </summary>
        protected virtual void OnInit() { }
        
        /// <summary>
        /// 打开时调用
        /// </summary>
        protected virtual void OnOpen(object data) { }
        
        /// <summary>
        /// 关闭时调用
        /// </summary>
        protected virtual void OnClose() { }
        
        /// <summary>
        /// 刷新时调用
        /// </summary>
        protected virtual void OnRefresh() { }
        
        /// <summary>
        /// 销毁时调用
        /// </summary>
        protected virtual void OnDestroyInternal() { }
        
        /// <summary>
        /// 可见性改变时调用
        /// </summary>
        protected virtual void OnVisibilityChanged(bool visible) { }
        
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
        /// 关闭自己
        /// </summary>
        protected void CloseSelf()
        {
            GetComp<IUIMgr>()?.Close(this);
        }
        
        #endregion
    }
}

