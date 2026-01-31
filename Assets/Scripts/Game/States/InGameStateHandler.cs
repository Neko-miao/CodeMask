// ================================================
// Game - 游戏中状态处理器
// ================================================

using GameFramework.Core;
using GameFramework.UI;
using GameFramework.Session;
using Game.UI;
using Game.Battle;
using UnityEngine;

namespace Game.States
{
    /// <summary>
    /// 游戏中状态处理器 - 管理单局战斗
    /// </summary>
    public class InGameStateHandler : IGameStateHandler
    {
        private BattleUIController _battleUIController;
        private BattleController _battleController;
        
        public void OnEnter()
        {
            Debug.Log("[InGameStateHandler] Entering InGame State");
            
            // 初始化战斗控制器
            _battleController = new BattleController();
            _battleController.Initialize();
            
            // 打开战斗UI，传入战斗控制器
            var uiMgr = GameInstance.Instance.GetComp<IUIMgr>();
            if (uiMgr != null)
            {
                _battleUIController = uiMgr.Open<BattleUIController>(_battleController);
            }
            
            // 开始单局
            StartSession();
        }
        
        public void OnExit()
        {
            Debug.Log("[InGameStateHandler] Exiting InGame State");
            
            // 关闭战斗UI
            var uiMgr = GameInstance.Instance.GetComp<IUIMgr>();
            if (uiMgr != null && _battleUIController != null)
            {
                uiMgr.Close(_battleUIController);
                _battleUIController = null;
            }
            
            // 清理战斗控制器
            _battleController?.Shutdown();
            _battleController = null;
        }
        
        public void OnUpdate(float deltaTime)
        {
            _battleController?.Update(deltaTime);
        }
        
        private void StartSession()
        {
            var session = GameInstance.Instance.GetComp<IGameSession>();
            if (session != null)
            {
                var config = new SessionConfig
                {
                    StartLevelId = 1,
                    AutoStartLevel = true
                };
                
                // 使用协程启动Session，完成后启动战斗
                session.StartSession(config, () =>
                {
                    _battleController?.StartBattle(1);
                });
            }
            else
            {
                // 无Session时直接启动战斗
                _battleController?.StartBattle(1);
            }
        }
    }
}
