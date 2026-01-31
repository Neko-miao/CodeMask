// ================================================
// Game - 游戏状态处理器接口
// ================================================

namespace Game.States
{
    /// <summary>
    /// 游戏状态处理器接口
    /// </summary>
    public interface IGameStateHandler
    {
        /// <summary>
        /// 进入状态
        /// </summary>
        void OnEnter();
        
        /// <summary>
        /// 退出状态
        /// </summary>
        void OnExit();
        
        /// <summary>
        /// 更新
        /// </summary>
        void OnUpdate(float deltaTime);
    }
}
