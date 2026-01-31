// ================================================
// Game - 主菜单状态处理器
// ================================================

using GameFramework.Core;
using GameFramework.UI;
using Game.UI;
using UnityEngine;

namespace Game.States
{
    /// <summary>
    /// 主菜单状态处理器
    /// </summary>
    public class MainMenuStateHandler : IGameStateHandler
    {
        private MainMenuController _mainMenuController;
        
        public void OnEnter()
        {
            Debug.Log("[MainMenuStateHandler] Entering Main Menu State");
            
            // 打开主菜单UI
            var uiMgr = GameInstance.Instance.GetComp<IUIMgr>();
            if (uiMgr != null)
            {
                _mainMenuController = uiMgr.Open<MainMenuController>();
            }
        }
        
        public void OnExit()
        {
            Debug.Log("[MainMenuStateHandler] Exiting Main Menu State");
            
            // 关闭主菜单UI
            var uiMgr = GameInstance.Instance.GetComp<IUIMgr>();
            if (uiMgr != null && _mainMenuController != null)
            {
                uiMgr.Close(_mainMenuController);
                _mainMenuController = null;
            }
        }
        
        public void OnUpdate(float deltaTime)
        {
            // 主菜单状态不需要特殊更新逻辑
        }
    }
}
