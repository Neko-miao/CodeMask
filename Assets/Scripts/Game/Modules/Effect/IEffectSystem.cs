using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 特效系统接口
    /// </summary>
    public interface IEffectSystem
    {
        #region Properties

        /// <summary>
        /// 是否正在播放全屏特效
        /// </summary>
        bool IsPlayingAllScreenEffect { get; }

        /// <summary>
        /// 是否正在播放震动特效
        /// </summary>
        bool IsPlayingShakeEffect { get; }

        /// <summary>
        /// 是否正在时停
        /// </summary>
        bool IsTimeStopped { get; }

        #endregion

        #region Events

        /// <summary>
        /// 全屏特效开始事件
        /// </summary>
        event Action OnAllScreenEffectStart;

        /// <summary>
        /// 全屏特效结束事件
        /// </summary>
        event Action OnAllScreenEffectEnd;

        /// <summary>
        /// 震动特效开始事件
        /// </summary>
        event Action OnShakeEffectStart;

        /// <summary>
        /// 震动特效结束事件
        /// </summary>
        event Action OnShakeEffectEnd;

        /// <summary>
        /// 时停开始事件
        /// </summary>
        event Action OnTimeStopStart;

        /// <summary>
        /// 时停结束事件
        /// </summary>
        event Action OnTimeStopEnd;

        #endregion

        #region All Screen Effect

        /// <summary>
        /// 播放全屏特效
        /// </summary>
        /// <param name="spriteRenderers">要变黑的SpriteRenderer数组</param>
        /// <param name="duration">持续时间（秒）</param>
        void PlayAllScreenEffect(SpriteRenderer[] spriteRenderers, float duration);

        /// <summary>
        /// 停止全屏特效
        /// </summary>
        void StopAllScreenEffect();

        #endregion

        #region Shake Effect

        /// <summary>
        /// 播放震动特效
        /// </summary>
        /// <param name="gameObjects">要震动的GameObject数组</param>
        /// <param name="duration">持续时间（秒）</param>
        void PlayShakeEffect(GameObject[] gameObjects, float duration);

        /// <summary>
        /// 停止震动特效
        /// </summary>
        void StopShakeEffect();

        #endregion

        #region Time Stop Effect

        /// <summary>
        /// 播放时停效果
        /// </summary>
        /// <param name="duration">时停持续时间（秒，真实时间）</param>
        /// <param name="excludedObjects">不参与时停的GameObject数组</param>
        void PlayTimeStop(float duration, GameObject[] excludedObjects = null);

        /// <summary>
        /// 停止时停效果
        /// </summary>
        void StopTimeStop();

        #endregion
    }
}
