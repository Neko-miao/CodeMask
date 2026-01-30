// ================================================
// GameFramework - 输入管理器实现
// ================================================

using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 输入管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 50, RequiredStates = new[] { GameState.Global })]
    public class InputMgr : GameComponent, IInputMgr
    {
        private readonly Dictionary<string, List<KeyCode>> _actionMappings = new Dictionary<string, List<KeyCode>>();
        private bool _isInputEnabled = true;
        
        public override string ComponentName => "InputMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 50;
        
        #region Properties
        
        public float Horizontal => _isInputEnabled ? Input.GetAxis("Horizontal") : 0f;
        public float Vertical => _isInputEnabled ? Input.GetAxis("Vertical") : 0f;
        public Vector2 MoveInput => new Vector2(Horizontal, Vertical);
        public bool AnyKeyDown => _isInputEnabled && Input.anyKeyDown;
        public Vector2 MousePosition => Input.mousePosition;
        public Vector2 MouseDelta => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        public float MouseScrollDelta => Input.mouseScrollDelta.y;
        public int TouchCount => Input.touchCount;
        public bool IsInputEnabled => _isInputEnabled;
        
        #endregion
        
        #region Axis
        
        public float GetAxis(string axisName)
        {
            if (!_isInputEnabled) return 0f;
            return Input.GetAxis(axisName);
        }
        
        public float GetAxisRaw(string axisName)
        {
            if (!_isInputEnabled) return 0f;
            return Input.GetAxisRaw(axisName);
        }
        
        #endregion
        
        #region Button
        
        public bool GetButton(string buttonName)
        {
            if (!_isInputEnabled) return false;
            return Input.GetButton(buttonName);
        }
        
        public bool GetButtonDown(string buttonName)
        {
            if (!_isInputEnabled) return false;
            return Input.GetButtonDown(buttonName);
        }
        
        public bool GetButtonUp(string buttonName)
        {
            if (!_isInputEnabled) return false;
            return Input.GetButtonUp(buttonName);
        }
        
        #endregion
        
        #region Key
        
        public bool GetKey(KeyCode key)
        {
            if (!_isInputEnabled) return false;
            return Input.GetKey(key);
        }
        
        public bool GetKeyDown(KeyCode key)
        {
            if (!_isInputEnabled) return false;
            return Input.GetKeyDown(key);
        }
        
        public bool GetKeyUp(KeyCode key)
        {
            if (!_isInputEnabled) return false;
            return Input.GetKeyUp(key);
        }
        
        #endregion
        
        #region Mouse
        
        public bool GetMouseButton(int button)
        {
            if (!_isInputEnabled) return false;
            return Input.GetMouseButton(button);
        }
        
        public bool GetMouseButtonDown(int button)
        {
            if (!_isInputEnabled) return false;
            return Input.GetMouseButtonDown(button);
        }
        
        public bool GetMouseButtonUp(int button)
        {
            if (!_isInputEnabled) return false;
            return Input.GetMouseButtonUp(button);
        }
        
        #endregion
        
        #region Touch
        
        public TouchData GetTouch(int index)
        {
            if (index < 0 || index >= Input.touchCount)
            {
                return default;
            }
            
            var touch = Input.GetTouch(index);
            return new TouchData
            {
                FingerId = touch.fingerId,
                Position = touch.position,
                DeltaPosition = touch.deltaPosition,
                Phase = touch.phase,
                Pressure = touch.pressure
            };
        }
        
        public TouchData[] GetAllTouches()
        {
            var touches = new TouchData[Input.touchCount];
            for (int i = 0; i < Input.touchCount; i++)
            {
                touches[i] = GetTouch(i);
            }
            return touches;
        }
        
        #endregion
        
        #region Action Mapping
        
        public void RegisterAction(string actionName, KeyCode key)
        {
            RegisterAction(actionName, new[] { key });
        }
        
        public void RegisterAction(string actionName, params KeyCode[] keys)
        {
            if (string.IsNullOrEmpty(actionName) || keys == null || keys.Length == 0)
                return;
            
            if (!_actionMappings.TryGetValue(actionName, out var keyList))
            {
                keyList = new List<KeyCode>();
                _actionMappings[actionName] = keyList;
            }
            
            foreach (var key in keys)
            {
                if (!keyList.Contains(key))
                {
                    keyList.Add(key);
                }
            }
        }
        
        public void UnregisterAction(string actionName)
        {
            _actionMappings.Remove(actionName);
        }
        
        public bool GetAction(string actionName)
        {
            if (!_isInputEnabled) return false;
            
            if (_actionMappings.TryGetValue(actionName, out var keys))
            {
                foreach (var key in keys)
                {
                    if (Input.GetKey(key))
                        return true;
                }
            }
            return false;
        }
        
        public bool GetActionDown(string actionName)
        {
            if (!_isInputEnabled) return false;
            
            if (_actionMappings.TryGetValue(actionName, out var keys))
            {
                foreach (var key in keys)
                {
                    if (Input.GetKeyDown(key))
                        return true;
                }
            }
            return false;
        }
        
        public bool GetActionUp(string actionName)
        {
            if (!_isInputEnabled) return false;
            
            if (_actionMappings.TryGetValue(actionName, out var keys))
            {
                foreach (var key in keys)
                {
                    if (Input.GetKeyUp(key))
                        return true;
                }
            }
            return false;
        }
        
        #endregion
        
        #region Settings
        
        public void SetInputEnabled(bool enabled)
        {
            _isInputEnabled = enabled;
        }
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            // 注册默认动作映射
            RegisterAction("Jump", KeyCode.Space);
            RegisterAction("Fire", KeyCode.Mouse0, KeyCode.LeftControl);
            RegisterAction("Cancel", KeyCode.Escape);
            RegisterAction("Submit", KeyCode.Return, KeyCode.KeypadEnter);
        }
        
        protected override void OnShutdown()
        {
            _actionMappings.Clear();
        }
        
        #endregion
    }
}

