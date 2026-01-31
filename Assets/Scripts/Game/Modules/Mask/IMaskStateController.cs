using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Mask状态控制器接口
    /// 提供四种状态切换操作的抽象接口，方便外部调用
    /// 最方便的调用方式：参考下面步骤1 2 3 4
    /// </summary>
    public interface IMaskStateController
    {
        /// <summary>
        /// 当前Mask状态
        /// </summary>
        MaskState CurrentState { get; }
        
        /// <summary>
        /// 是否正在飞行
        /// </summary>
        bool IsFlying { get; }
        
        /// <summary>
        /// 状态改变时的回调
        /// </summary>
        event Action<MaskState> OnMaskStateChanged;
        
        /// <summary>
        /// 飞行完成时的回调
        /// </summary>
        event Action OnMaskFlightComplete;
        
        /// <summary>
        /// 操作1：切换到开启状态并飞向目标
        /// </summary>
        /// <param name="target">目标位置</param>
        /// <param name="duration">飞行时间（可选，-1使用默认值）</param>
        /// <param name="height">抛物线高度（可选，-1使用默认值）</param>
        void ActivateAndFlyTo(Vector2 target, float duration = -1f, float height = -1f);
        
        /// <summary>
        /// 操作2：切换到面具状态
        /// </summary>
        void SwitchToMaskMode();
        
        /// <summary>
        /// 操作3：切换到开启状态，飞向目标，到达后切换到待机状态
        /// </summary>
        /// <param name="target">目标位置</param>
        /// <param name="duration">飞行时间（可选，-1使用默认值）</param>
        /// <param name="height">抛物线高度（可选，-1使用默认值）</param>
        void ActivateFlyToThenIdle(Vector2 target, float duration = -1f, float height = -1f);
        
        /// <summary>
        /// 操作4：重置到初始位置和旋转，并切换到开启状态
        /// </summary>
        void ResetAndActivate();
        
        /// <summary>
        /// 设置Mask状态
        /// </summary>
        /// <param name="state">目标状态</param>
        void SetState(MaskState state);
        
        /// <summary>
        /// 切换到待机状态
        /// </summary>
        void SetIdleState();
        
        /// <summary>
        /// 切换到开启状态
        /// </summary>
        void SetActiveState();
        
        /// <summary>
        /// 切换到面具状态
        /// </summary>
        void SetMaskModeState();
        
        /// <summary>
        /// 发射Mask沿抛物线飞向目标点
        /// </summary>
        /// <param name="target">目标位置</param>
        /// <param name="duration">飞行时间（可选）</param>
        /// <param name="height">抛物线高度（可选）</param>
        void LaunchToTarget(Vector2 target, float duration = -1f, float height = -1f);
        
        /// <summary>
        /// 停止飞行
        /// </summary>
        void StopFlight();
    }
}
