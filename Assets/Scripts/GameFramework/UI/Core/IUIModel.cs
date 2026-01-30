// ================================================
// GameFramework - UI数据模型接口
// ================================================

using System;

namespace GameFramework.UI
{
    /// <summary>
    /// UI数据模型接口
    /// </summary>
    public interface IUIModel
    {
        /// <summary>
        /// 初始化
        /// </summary>
        void Initialize();
        
        /// <summary>
        /// 清理
        /// </summary>
        void Clear();
        
        /// <summary>
        /// 数据改变事件
        /// </summary>
        event Action OnDataChanged;
    }
    
    /// <summary>
    /// UI数据模型接口 (泛型)
    /// </summary>
    public interface IUIModel<T> : IUIModel where T : class
    {
        /// <summary>
        /// 数据
        /// </summary>
        T Data { get; }
        
        /// <summary>
        /// 设置数据
        /// </summary>
        void SetData(T data);
    }
}

