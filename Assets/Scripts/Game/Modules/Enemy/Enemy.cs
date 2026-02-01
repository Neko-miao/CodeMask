using System.Collections;
using UnityEngine;
using Game.Events;

namespace Game
{
    public class Enemy : MonoBehaviour
    {
        private bool isAttacking = false;       // 是否正在攻击
        private Coroutine attackCoroutine;      // 攻击协程引用

        [Header("攻击设置")]
        [Tooltip("攻击动画名称（Animation组件使用）")]
        [SerializeField] private string attackAnimName = "Attack";

        [Header("组件引用")]
        [Tooltip("Animation组件")]
        [SerializeField] private Animation animationComponent;

        void Start()
        {
            // 获取Animation组件
            if (animationComponent == null)
            {
                animationComponent = GetComponent<Animation>();
            }

            // 订阅节奏创建事件
            SubscribeToEvents();
        }

        void OnDestroy()
        {
            // 取消订阅事件
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 订阅事件
        /// </summary>
        private void SubscribeToEvents()
        {
            RhythmSystem.OnRhythmCreated += OnRhythmCreated;
            Debug.Log("[Enemy] 已订阅 RhythmCreated 事件");
        }

        /// <summary>
        /// 取消订阅事件
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            RhythmSystem.OnRhythmCreated -= OnRhythmCreated;
        }

        /// <summary>
        /// 节奏创建事件处理 - 在第一个Miss点时播放攻击动画
        /// </summary>
        private void OnRhythmCreated(RhythmCreatedEvent evt)
        {
            Debug.Log($"[Enemy] 收到节奏创建事件: ActionType={evt.ActionType}, TimeToReachZone={evt.TimeToReachJudgmentZone:F2}s");
            
            // 开始攻击，传入到达判定区域（第一个Miss点）的时间
            StartAttack(evt.TimeToReachJudgmentZone);
        }

        /// <summary>
        /// 开始攻击动作
        /// </summary>
        /// <param name="timeToReachFirstMiss">节奏到达第一个Miss点的时间</param>
        public void StartAttack(float timeToReachFirstMiss)
        {
            // 开始攻击协程
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }
            attackCoroutine = StartCoroutine(AttackCoroutine(timeToReachFirstMiss));
        }

        /// <summary>
        /// 攻击协程 - 等待节奏到达第一个Miss点时播放动画
        /// </summary>
        private IEnumerator AttackCoroutine(float waitTime)
        {
            isAttacking = true;

            // 确保等待时间有效
            float actualWaitTime = Mathf.Max(0f, waitTime);

            Debug.Log($"[Enemy] 等待 {actualWaitTime:F2}s 后播放攻击动画");

            // 等待节奏到达第一个Miss点
            if (actualWaitTime > 0f)
            {
                yield return new WaitForSeconds(actualWaitTime);
            }

            Debug.Log("[Enemy] 节奏到达第一个Miss点，播放攻击动画");
            
            // 播放攻击动画
            PlayAttackAnimation();

            isAttacking = false;
        }

        /// <summary>
        /// 播放攻击动画（使用Animation组件）
        /// </summary>
        private void PlayAttackAnimation()
        {
            if (animationComponent != null && !string.IsNullOrEmpty(attackAnimName))
            {
                animationComponent.Play(attackAnimName);
                Debug.Log($"[Enemy] 播放攻击动画: {attackAnimName}");
            }
            else
            {
                Debug.LogWarning("[Enemy] Animation组件或动画名称未设置");
            }
        }

        /// <summary>
        /// 停止当前攻击
        /// </summary>
        public void StopAttack()
        {
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            isAttacking = false;
        }

        /// <summary>
        /// 获取是否正在攻击
        /// </summary>
        public bool IsAttacking => isAttacking;

        /// <summary>
        /// 设置Animation组件
        /// </summary>
        public void SetAnimationComponent(Animation anim)
        {
            animationComponent = anim;
        }

        /// <summary>
        /// 设置攻击动画名称
        /// </summary>
        public void SetAttackAnimName(string animName)
        {
            attackAnimName = animName;
        }
    }
}
