// ================================================
// GameFramework - UI数据模型基类
// ================================================

using System;

namespace GameFramework.UI
{
    /// <summary>
    /// UI数据模型基类
    /// </summary>
    public abstract class UIModelBase : IUIModel
    {
        public event Action OnDataChanged;
        
        public virtual void Initialize() { }
        
        public virtual void Clear() { }
        
        /// <summary>
        /// 通知数据改变
        /// </summary>
        protected void NotifyDataChanged()
        {
            OnDataChanged?.Invoke();
        }
    }
    
    /// <summary>
    /// UI数据模型基类 (泛型)
    /// </summary>
    public abstract class UIModelBase<T> : UIModelBase, IUIModel<T> where T : class
    {
        private T _data;
        
        public T Data => _data;
        
        public void SetData(T data)
        {
            _data = data;
            OnDataSet(data);
            NotifyDataChanged();
        }
        
        /// <summary>
        /// 数据设置时调用
        /// </summary>
        protected virtual void OnDataSet(T data) { }
        
        public override void Clear()
        {
            _data = null;
        }
    }
}

