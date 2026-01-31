// ================================================
// Game - 主菜单界面 (MVC - View)
// ================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using GameFramework.UI;

namespace Game.UI
{
    /// <summary>
    /// 主菜单界面 - 只负责UI元素绑定和显示，不持有Controller引用
    /// </summary>
    public class MainMenuView : UIViewBase
    {
        [Header("Buttons")]
        [SerializeField] private Button _startGameBtn;
        [SerializeField] private Button _continueGameBtn;
        [SerializeField] private Button _settingsBtn;
        [SerializeField] private Button _quitBtn;
        
        [Header("Panels")]
        [SerializeField] private GameObject _settingsPanel;
        
        #region Events (供 Controller 绑定)
        
        public event Action OnStartGameClick;
        public event Action OnContinueGameClick;
        public event Action OnSettingsClick;
        public event Action OnQuitClick;
        
        #endregion
        
        protected override void OnInit()
        {
            base.OnInit();
            
            // 绑定按钮点击 -> 触发事件给 Controller
            if (_startGameBtn != null)
                _startGameBtn.onClick.AddListener(() => OnStartGameClick?.Invoke());
            
            if (_continueGameBtn != null)
                _continueGameBtn.onClick.AddListener(() => OnContinueGameClick?.Invoke());
            
            if (_settingsBtn != null)
                _settingsBtn.onClick.AddListener(() => OnSettingsClick?.Invoke());
            
            if (_quitBtn != null)
                _quitBtn.onClick.AddListener(() => OnQuitClick?.Invoke());
            
            Debug.Log("[MainMenuView] Initialized");
        }
        
        protected override void OnDestroyInternal()
        {
            // 移除按钮监听
            if (_startGameBtn != null)
                _startGameBtn.onClick.RemoveAllListeners();
            
            if (_continueGameBtn != null)
                _continueGameBtn.onClick.RemoveAllListeners();
            
            if (_settingsBtn != null)
                _settingsBtn.onClick.RemoveAllListeners();
            
            if (_quitBtn != null)
                _quitBtn.onClick.RemoveAllListeners();
            
            base.OnDestroyInternal();
        }
        
        #region UI Update Methods (供 Controller 调用)
        
        /// <summary>
        /// 更新UI显示
        /// </summary>
        public void UpdateUI(bool hasSaveData, bool settingsEnabled, bool isSettingsPanelOpen)
        {
            if (_continueGameBtn != null)
                _continueGameBtn.interactable = hasSaveData;
            
            if (_settingsBtn != null)
                _settingsBtn.interactable = settingsEnabled;
            
            if (_settingsPanel != null)
                _settingsPanel.SetActive(isSettingsPanelOpen);
        }
        
        #endregion
    }
}
