// ================================================
// Game - 扩展游戏启动器
// ================================================

using GameFramework.Core;
using Game.Modules;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 扩展游戏启动器 - 注册游戏特定的组件
    /// </summary>
    public class GameLauncherExtended : GameLauncher
    {
        /// <summary>
        /// 注册自定义组件
        /// </summary>
        protected override void RegisterCustomComponents(IComponentRegistry registry)
        {
            base.RegisterCustomComponents(registry);
            
            // 注册玩家模块
            registry.RegisterForState<IPlayerMdl, PlayerMdl>(GameState.Playing, priority: 200);
            
            // 在此添加更多自定义模块...
            // registry.RegisterForState<IBagMdl, BagMdl>(GameState.Playing, priority: 210);
            // registry.RegisterForState<IShopMdl, ShopMdl>(GameState.Playing, priority: 220);
            
            Debug.Log("[GameLauncherExtended] Custom components registered");
        }
    }
}

