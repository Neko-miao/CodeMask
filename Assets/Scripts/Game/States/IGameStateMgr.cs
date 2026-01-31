// ================================================
// Game - 游戏状态管理器接口
// ================================================

using GameFramework.Core;

namespace Game.States
{
    /// <summary>
    /// 游戏状态管理器接口
    /// </summary>
    public interface IGameStateMgr : IGameComponent
    {
        /// <summary>
        /// 注册状态处理器
        /// </summary>
        void RegisterHandler(GameState state, IGameStateHandler handler);
        
        /// <summary>
        /// 取消注册状态处理器
        /// </summary>
        void UnregisterHandler(GameState state);
        
        /// <summary>
        /// 获取当前状态处理器
        /// </summary>
        IGameStateHandler GetCurrentHandler();
    }
}
