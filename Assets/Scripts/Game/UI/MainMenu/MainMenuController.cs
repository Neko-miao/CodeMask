// ================================================
// Game - 主菜单控制器
// ================================================

using UnityEngine;
using GameFramework.Core;
using GameFramework.UI;

namespace Game.UI
{
    /// <summary>
    /// 主菜单控制器 - MVC核心，持有Model和View
    /// </summary>
    public class MainMenuController : UIControllerBase<MainMenuView, MainMenuModel>
    {
        protected override void OnModelInit(object data)
        {
            Model.Initialize();
        }
        
        protected override void OnBindEvents()
        {
            // 绑定 View 按钮事件
            View.OnStartGameClick += HandleStartGame;
            View.OnContinueGameClick += HandleContinueGame;
            View.OnSettingsClick += HandleSettings;
            View.OnQuitClick += HandleQuit;
        }
        
        protected override void OnUnbindEvents()
        {
            View.OnStartGameClick -= HandleStartGame;
            View.OnContinueGameClick -= HandleContinueGame;
            View.OnSettingsClick -= HandleSettings;
            View.OnQuitClick -= HandleQuit;
        }
        
        protected override void OnUpdateView()
        {
            View.UpdateUI(Model.HasSaveData, Model.SettingsEnabled, Model.IsSettingsPanelOpen);
        }
        
        #region Event Handlers
        
        private void HandleStartGame()
        {
            Debug.Log("[MainMenuController] Start Game");
            GameInstance.Instance.ChangeState(GameState.Playing);
        }
        
        private void HandleContinueGame()
        {
            Debug.Log("[MainMenuController] Continue Game");
            // TODO: 加载存档逻辑
            GameInstance.Instance.ChangeState(GameState.Playing);
        }
        
        private void HandleSettings()
        {
            Debug.Log("[MainMenuController] Toggle Settings");
            Model.ToggleSettingsPanel();
        }
        
        private void HandleQuit()
        {
            Debug.Log("[MainMenuController] Quit");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        #endregion
    }
}
