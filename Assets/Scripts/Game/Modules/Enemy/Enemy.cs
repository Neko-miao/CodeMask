using System.Collections;
using UnityEngine;
using Game.Events;

namespace Game
{
    public class Enemy : MonoBehaviour
    {
        private bool isAttacking = false;       // 是否正在攻击
        private Coroutine attackCoroutine;      // 攻击协程引用
        private Coroutine returnCoroutine;      // 返回协程引用

        [Header("攻击设置")]
        [Tooltip("攻击动画名称（Animation组件使用）")]
        [SerializeField] private string attackAnimName = "Attack";

        [Header("返回起始点设置")]
        [Tooltip("是否启用自动返回起始点")]
        [SerializeField] private bool enableAutoReturn = true;
        [Tooltip("返回起始点的速度")]
        [SerializeField] private float returnSpeed = 5f;
        [Tooltip("被推离后延迟多久开始返回")]
        [SerializeField] private float returnDelay = 0.5f;
        [Tooltip("到达起始点的距离阈值")]
        [SerializeField] private float arrivalThreshold = 0.1f;
        [Tooltip("离开起点的距离阈值（超过此距离触发返回）")]
        [SerializeField] private float leaveThreshold = 0.05f;

        [Header("组件引用")]
        [Tooltip("Animation组件")]
        [SerializeField] private Animation animationComponent;

        // 起始位置
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool isReturning = false;
        private Coroutine returnDelayCoroutine;  // 延迟返回协程

        void Start()
        {
            // 记录起始位置
            startPosition = transform.position;
            startRotation = transform.rotation;
            
            // 获取Animation组件
            if (animationComponent == null)
            {
                animationComponent = GetComponent<Animation>();
            }

            // 订阅节奏创建事件
            SubscribeToEvents();
        }

        void FixedUpdate()
        {
            // 持续检测位置偏移，触发返回
            CheckPositionAndReturn();
        }

        void OnDestroy()
        {
            // 取消订阅事件
            UnsubscribeFromEvents();
        }

        /// <summary>
        /// 检测位置偏移，触发返回
        /// </summary>
        private void CheckPositionAndReturn()
        {
            if (!enableAutoReturn) return;
            
            float distance = Vector3.Distance(transform.position, startPosition);
            
            // 如果偏离起点且尚未开始返回流程
            if (distance > leaveThreshold && !isReturning && returnDelayCoroutine == null)
            {
                // 开始延迟返回
                returnDelayCoroutine = StartCoroutine(DelayedReturnCoroutine());
            }
        }

        /// <summary>
        /// 延迟后开始返回
        /// </summary>
        private IEnumerator DelayedReturnCoroutine()
        {
            if (returnDelay > 0f)
            {
                yield return new WaitForSeconds(returnDelay);
            }
            
            returnDelayCoroutine = null;
            
            // 再次检查是否仍然偏离起点
            float distance = Vector3.Distance(transform.position, startPosition);
            if (distance > leaveThreshold && !isReturning)
            {
                StartReturn();
            }
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
            // 停止返回协程（如果正在返回）
            StopReturn();
            
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

        #region 返回起始点

        /// <summary>
        /// 开始返回起始点
        /// </summary>
        public void StartReturn()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            returnCoroutine = StartCoroutine(ReturnToStartCoroutine());
        }

        /// <summary>
        /// 停止返回
        /// </summary>
        public void StopReturn()
        {
            if (returnDelayCoroutine != null)
            {
                StopCoroutine(returnDelayCoroutine);
                returnDelayCoroutine = null;
            }
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
            isReturning = false;
        }

        /// <summary>
        /// 返回起始点协程
        /// </summary>
        private IEnumerator ReturnToStartCoroutine()
        {
            // 延迟开始返回
            if (returnDelay > 0f)
            {
                yield return new WaitForSeconds(returnDelay);
            }

            isReturning = true;
            Debug.Log("[Enemy] 开始返回起始点");

            // 移动到起始点
            while (Vector3.Distance(transform.position, startPosition) > arrivalThreshold)
            {
                // 平滑移动
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    startPosition, 
                    returnSpeed * Time.deltaTime
                );
                
                // 平滑旋转
                transform.rotation = Quaternion.Slerp(
                    transform.rotation, 
                    startRotation, 
                    returnSpeed * Time.deltaTime
                );
                
                yield return null;
            }

            // 精确设置到起始位置
            transform.position = startPosition;
            transform.rotation = startRotation;
            
            isReturning = false;
            Debug.Log("[Enemy] 已返回起始点");
        }

        /// <summary>
        /// 立即返回起始点（无动画）
        /// </summary>
        public void ReturnToStartImmediate()
        {
            StopReturn();
            StopAttack();
            transform.position = startPosition;
            transform.rotation = startRotation;
            Debug.Log("[Enemy] 立即返回起始点");
        }

        /// <summary>
        /// 设置新的起始点
        /// </summary>
        public void SetStartPosition(Vector3 position, Quaternion rotation)
        {
            startPosition = position;
            startRotation = rotation;
        }

        /// <summary>
        /// 设置新的起始点为当前位置
        /// </summary>
        public void SetCurrentAsStartPosition()
        {
            startPosition = transform.position;
            startRotation = transform.rotation;
        }

        #endregion

        #region 公共属性

        /// <summary>
        /// 获取是否正在攻击
        /// </summary>
        public bool IsAttacking => isAttacking;

        /// <summary>
        /// 获取是否正在返回
        /// </summary>
        public bool IsReturning => isReturning;

        /// <summary>
        /// 获取起始位置
        /// </summary>
        public Vector3 StartPosition => startPosition;

        #endregion

        #region 设置方法

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

        /// <summary>
        /// 设置是否启用自动返回
        /// </summary>
        public void SetEnableAutoReturn(bool enable)
        {
            enableAutoReturn = enable;
        }

        /// <summary>
        /// 设置返回速度
        /// </summary>
        public void SetReturnSpeed(float speed)
        {
            returnSpeed = speed;
        }

        #endregion
    }
}
