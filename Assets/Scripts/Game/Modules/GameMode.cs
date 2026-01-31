using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Events;

namespace Game
{
    /// <summary>
    /// 游戏模式 - 监听节奏事件并调度特效
    /// </summary>
    public class GameMode : MonoBehaviour
    {
        [Header("特效调度引用")]
        [Tooltip("攻击特效起点（Perfect/Great时使用）")]
        [SerializeField]
        private Transform attackStartPoint;

        [Tooltip("攻击特效终点（Perfect/Great时使用）")]
        [SerializeField]
        private Transform attackEndPoint;

        [Tooltip("Miss攻击特效起点")]
        [SerializeField]
        private Transform missAttackStartPoint;

        [Tooltip("Miss攻击特效终点")]
        [SerializeField]
        private Transform missAttackEndPoint;

        [Header("全屏特效设置")]
        [Tooltip("全屏特效使用的SpriteRenderer列表")]
        [SerializeField]
        private List<SpriteRenderer> allScreenSpriteRenderers = new List<SpriteRenderer>();

        [Tooltip("全屏特效持续时间")]
        [SerializeField]
        private float allScreenEffectDuration = 0.5f;

        [Header("Perfect连击设置")]
        [Tooltip("触发全屏特效所需的Perfect次数")]
        [SerializeField]
        private int perfectCountForAllScreen = 6;

        [Header("震动特效设置")]
        [Tooltip("震动特效持续时间")]
        [SerializeField]
        private float shakeDuration = 0.3f;

        [Tooltip("攻击目标的SpriteRenderer（用于震动）")]
        [SerializeField]
        private SpriteRenderer attackTargetRenderer;

        [Tooltip("Miss攻击目标的SpriteRenderer（用于震动）")]
        [SerializeField]
        private SpriteRenderer missAttackTargetRenderer;

        /// <summary>
        /// 当前Perfect连击次数
        /// </summary>
        private int _perfectCount = 0;

        void Start()
        {
            // 订阅节奏触发事件
            RhythmTriggerZone.OnRhythmTriggerEvent += OnRhythmTriggered;
            Debug.Log("[GameMode] 成功订阅 RhythmTriggerEvent 事件");
        }

        void OnDestroy()
        {
            // 取消订阅事件
            RhythmTriggerZone.OnRhythmTriggerEvent -= OnRhythmTriggered;
        }

        /// <summary>
        /// 节奏触发事件处理
        /// </summary>
        private void OnRhythmTriggered(RhythmTriggerEvent evt)
        {
            Debug.Log($"[GameMode] 节奏触发: MaskType={evt.MaskType}, ActionType={evt.ActionType}, Result={evt.Result}");

            switch (evt.Result)
            {
                case RhythmScoreGrade.Perfect:
                    HandlePerfect();
                    break;
                case RhythmScoreGrade.Great:
                    HandleGreat();
                    break;
                case RhythmScoreGrade.Miss:
                    HandleMiss();
                    break;
            }
        }

        /// <summary>
        /// 处理完美结果 - 攻击特效 + 连击6次后全屏闪烁
        /// </summary>
        private void HandlePerfect()
        {
            if (EffectSystem.Instance == null)
            {
                Debug.LogWarning("[GameMode] EffectSystem不存在");
                return;
            }

            // 增加Perfect计数
            _perfectCount++;
            Debug.Log($"[GameMode] Perfect! 当前连击: {_perfectCount}/{perfectCountForAllScreen}");

            // 检查是否达到6次，触发全屏特效
            if (_perfectCount >= perfectCountForAllScreen)
            {
                _perfectCount = 0; // 重置计数
                
                if (allScreenSpriteRenderers != null && allScreenSpriteRenderers.Count > 0)
                {
                    Debug.Log("[GameMode] Perfect连击达成! 播放全屏闪烁特效");
                    EffectSystem.Instance.PlayAllScreenEffect(allScreenSpriteRenderers.ToArray(), allScreenEffectDuration);
                }
            }

            // Perfect也播放攻击特效
            if (attackStartPoint == null || attackEndPoint == null)
            {
                Debug.LogWarning("[GameMode] 攻击特效起点或终点未设置");
                return;
            }

            EffectSystem.Instance.PlayAttackEffect(
                attackStartPoint.position, 
                attackEndPoint.position,
                () => OnAttackReachTarget(attackTargetRenderer)
            );
        }

        /// <summary>
        /// 处理良好结果 - 攻击特效
        /// </summary>
        private void HandleGreat()
        {
            // Great打断Perfect连击
            _perfectCount = 0;

            if (EffectSystem.Instance == null)
            {
                Debug.LogWarning("[GameMode] EffectSystem不存在");
                return;
            }

            if (attackStartPoint == null || attackEndPoint == null)
            {
                Debug.LogWarning("[GameMode] 攻击特效起点或终点未设置");
                return;
            }

            Debug.Log("[GameMode] Great! 播放攻击特效");
            EffectSystem.Instance.PlayAttackEffect(
                attackStartPoint.position, 
                attackEndPoint.position,
                () => OnAttackReachTarget(attackTargetRenderer)
            );
        }

        /// <summary>
        /// 处理Miss结果 - Miss攻击特效
        /// </summary>
        private void HandleMiss()
        {
            // Miss打断Perfect连击
            _perfectCount = 0;

            if (EffectSystem.Instance == null)
            {
                Debug.LogWarning("[GameMode] EffectSystem不存在");
                return;
            }

            if (missAttackStartPoint == null || missAttackEndPoint == null)
            {
                Debug.LogWarning("[GameMode] Miss攻击特效起点或终点未设置");
                return;
            }

            Debug.Log("[GameMode] Miss! 播放Miss攻击特效");
            EffectSystem.Instance.PlayAttackEffect(
                missAttackStartPoint.position, 
                missAttackEndPoint.position,
                () => OnAttackReachTarget(missAttackTargetRenderer)
            );
        }

        /// <summary>
        /// 攻击特效抵达目标点回调 - 触发震动
        /// </summary>
        /// <param name="targetRenderer">目标SpriteRenderer</param>
        private void OnAttackReachTarget(SpriteRenderer targetRenderer)
        {
            if (EffectSystem.Instance == null || targetRenderer == null)
            {
                return;
            }

            Debug.Log($"[GameMode] 攻击抵达目标，触发震动特效: {targetRenderer.name}");
            EffectSystem.Instance.PlayShakeEffect(new SpriteRenderer[] { targetRenderer }, shakeDuration);
        }
    }
}
