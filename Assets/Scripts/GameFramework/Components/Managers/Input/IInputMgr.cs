// ================================================
// GameFramework - 输入管理器接口
// ================================================

using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 触摸数据
    /// </summary>
    public struct TouchData
    {
        public int FingerId;
        public Vector2 Position;
        public Vector2 DeltaPosition;
        public TouchPhase Phase;
        public float Pressure;
    }
    
    /// <summary>
    /// 输入管理器接口
    /// </summary>
    public interface IInputMgr : IGameComponent
    {
        #region Axis
        
        /// <summary>
        /// 获取轴向输入
        /// </summary>
        float GetAxis(string axisName);
        
        /// <summary>
        /// 获取轴向输入 (无平滑)
        /// </summary>
        float GetAxisRaw(string axisName);
        
        /// <summary>
        /// 获取水平输入
        /// </summary>
        float Horizontal { get; }
        
        /// <summary>
        /// 获取垂直输入
        /// </summary>
        float Vertical { get; }
        
        /// <summary>
        /// 获取移动输入向量
        /// </summary>
        Vector2 MoveInput { get; }
        
        #endregion
        
        #region Button
        
        /// <summary>
        /// 获取按钮状态
        /// </summary>
        bool GetButton(string buttonName);
        
        /// <summary>
        /// 获取按钮按下
        /// </summary>
        bool GetButtonDown(string buttonName);
        
        /// <summary>
        /// 获取按钮释放
        /// </summary>
        bool GetButtonUp(string buttonName);
        
        #endregion
        
        #region Key
        
        /// <summary>
        /// 获取按键状态
        /// </summary>
        bool GetKey(KeyCode key);
        
        /// <summary>
        /// 获取按键按下
        /// </summary>
        bool GetKeyDown(KeyCode key);
        
        /// <summary>
        /// 获取按键释放
        /// </summary>
        bool GetKeyUp(KeyCode key);
        
        /// <summary>
        /// 是否有任意键按下
        /// </summary>
        bool AnyKeyDown { get; }
        
        #endregion
        
        #region Mouse
        
        /// <summary>
        /// 获取鼠标位置
        /// </summary>
        Vector2 MousePosition { get; }
        
        /// <summary>
        /// 获取鼠标移动增量
        /// </summary>
        Vector2 MouseDelta { get; }
        
        /// <summary>
        /// 获取鼠标滚轮增量
        /// </summary>
        float MouseScrollDelta { get; }
        
        /// <summary>
        /// 获取鼠标按键状态
        /// </summary>
        bool GetMouseButton(int button);
        
        /// <summary>
        /// 获取鼠标按键按下
        /// </summary>
        bool GetMouseButtonDown(int button);
        
        /// <summary>
        /// 获取鼠标按键释放
        /// </summary>
        bool GetMouseButtonUp(int button);
        
        #endregion
        
        #region Touch
        
        /// <summary>
        /// 触摸数量
        /// </summary>
        int TouchCount { get; }
        
        /// <summary>
        /// 获取触摸数据
        /// </summary>
        TouchData GetTouch(int index);
        
        /// <summary>
        /// 获取所有触摸数据
        /// </summary>
        TouchData[] GetAllTouches();
        
        #endregion
        
        #region Action Mapping
        
        /// <summary>
        /// 注册输入动作
        /// </summary>
        void RegisterAction(string actionName, KeyCode key);
        
        /// <summary>
        /// 注册输入动作 (多个按键)
        /// </summary>
        void RegisterAction(string actionName, params KeyCode[] keys);
        
        /// <summary>
        /// 注销输入动作
        /// </summary>
        void UnregisterAction(string actionName);
        
        /// <summary>
        /// 获取动作状态
        /// </summary>
        bool GetAction(string actionName);
        
        /// <summary>
        /// 获取动作按下
        /// </summary>
        bool GetActionDown(string actionName);
        
        /// <summary>
        /// 获取动作释放
        /// </summary>
        bool GetActionUp(string actionName);
        
        #endregion
        
        #region Settings
        
        /// <summary>
        /// 启用/禁用输入
        /// </summary>
        void SetInputEnabled(bool enabled);
        
        /// <summary>
        /// 输入是否启用
        /// </summary>
        bool IsInputEnabled { get; }
        
        #endregion
    }
}

