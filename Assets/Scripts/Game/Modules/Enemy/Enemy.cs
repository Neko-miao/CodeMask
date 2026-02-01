using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Events;

namespace Game
{
    public class Enemy : MonoBehaviour
    {
        private Vector2 startPosition;          // 起始位置
        private Rigidbody2D rb;                 // Rigidbody2D组件
        private bool isReturning = false;       // 是否正在返回
        private Coroutine returnCoroutine;      // 返回协程引用
        private Coroutine attackCoroutine;      // 攻击协程引用
        private bool isAttacking = false;       // 是否正在攻击
        
        [Header("返回设置")]
        [SerializeField] private float returnDelay = 0.5f;      // 返回延迟时间
        [SerializeField] private float returnSpeed = 5f;        // 返回速度
        [SerializeField] private float positionThreshold = 0.1f; // 位置判断阈值

        [Header("攻击设置")]
        [Tooltip("攻击目标（玩家角色）的Transform")]
        [SerializeField] private Transform attackTarget;
        
        [Tooltip("攻击时与目标的距离偏移（敌人停在目标面前的距离）")]
        [SerializeField] private float attackDistanceOffset = 1.5f;
        
        [Tooltip("攻击动画触发器名称")]
        [SerializeField] private string attackAnimTrigger = "Attack";
        
        [Tooltip("攻击动画的持续时间（秒）")]
        [SerializeField] private float attackAnimDuration = 0.5f;

        [Header("组件引用")]
        [SerializeField] private Animator animator;

        void Start()
        {
            // 记录起始位置
            startPosition = transform.position;
            // 获取Rigidbody2D组件
            rb = GetComponent<Rigidbody2D>();
            // 获取Animator组件
            if (animator == null)
            {
                animator = GetComponent<Animator>();
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
        /// 节奏创建事件处理 - 执行攻击动作
        /// </summary>
        private void OnRhythmCreated(RhythmCreatedEvent evt)
        {
            Debug.Log($"[Enemy] 收到节奏创建事件: ActionType={evt.ActionType}, TimeToReachZone={evt.TimeToReachJudgmentZone:F2}s, TimeToReachPerfect={evt.TimeToReachPerfectZone:F2}s");
            
            // 如果正在攻击中，不响应新的攻击事件（或者可以根据需求选择打断当前攻击）
            if (isAttacking)
            {
                Debug.Log("[Enemy] 当前正在攻击中，忽略新的攻击事件");
                return;
            }

            // 开始攻击，传入到达完美区间的时间
            StartAttack(evt.TimeToReachPerfectZone);
        }

        /// <summary>
        /// 开始攻击动作
        /// </summary>
        /// <param name="timeToReachPerfect">节奏到达完美区间的时间</param>
        public void StartAttack(float timeToReachPerfect)
        {
            if (attackTarget == null)
            {
                Debug.LogWarning("[Enemy] 攻击目标未设置，无法执行攻击");
                return;
            }

            // 停止返回协程
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
                returnCoroutine = null;
            }
            isReturning = false;

            // 开始攻击协程
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
            }
            attackCoroutine = StartCoroutine(AttackCoroutine(timeToReachPerfect));
        }

        /// <summary>
        /// 攻击协程 - 移动到目标面前并播放攻击动画
        /// 移动时间 = 原时间/2，动画结束时刻 = 节奏到达完美区间时刻
        /// </summary>
        private IEnumerator AttackCoroutine(float timeToReachPerfect)
        {
            isAttacking = true;

            // 计算目标位置（在攻击目标面前）
            Vector2 targetPosition = CalculateAttackPosition();
            Vector2 currentPos = transform.position;
            
            // 移动时间 = 到达完美区间时间 / 2
            float moveTime = timeToReachPerfect / 2f;
            // 动画等待时间 = 到达完美区间时间 - 移动时间 - 动画持续时间
            // 这样动画结束时刻正好是节奏到达完美区间的时刻
            float waitBeforeAnim = timeToReachPerfect - moveTime - attackAnimDuration;
            
            // 确保时间有效
            moveTime = Mathf.Max(0.1f, moveTime);
            waitBeforeAnim = Mathf.Max(0f, waitBeforeAnim);
            
            float elapsedTime = 0f;

            Debug.Log($"[Enemy] 开始攻击: 移动时间={moveTime:F2}s, 等待时间={waitBeforeAnim:F2}s, 动画时间={attackAnimDuration:F2}s, 总时间={timeToReachPerfect:F2}s");

            // 平滑移动到目标位置
            while (elapsedTime < moveTime)
            {
                elapsedTime += Time.deltaTime;
                float t = Mathf.Clamp01(elapsedTime / moveTime);
                
                // 使用缓动函数使移动更自然
                float easedT = EaseOutQuad(t);
                
                Vector2 newPos = Vector2.Lerp(currentPos, targetPosition, easedT);
                
                if (rb != null)
                {
                    rb.MovePosition(newPos);
                }
                else
                {
                    transform.position = newPos;
                }
                
                yield return null;
            }

            // 确保到达目标位置
            if (rb != null)
            {
                rb.MovePosition(targetPosition);
                rb.velocity = Vector2.zero;
            }
            else
            {
                transform.position = targetPosition;
            }

            Debug.Log("[Enemy] 到达攻击位置");

            // 等待一段时间再播放动画，确保动画结束时刻正好是节奏到达完美区间的时刻
            if (waitBeforeAnim > 0f)
            {
                Debug.Log($"[Enemy] 等待 {waitBeforeAnim:F2}s 后播放动画");
                yield return new WaitForSeconds(waitBeforeAnim);
            }

            Debug.Log("[Enemy] 播放攻击动画");
            
            // 播放攻击动画
            PlayAttackAnimation();

            // 等待攻击动画播放完成
            yield return new WaitForSeconds(attackAnimDuration);

            isAttacking = false;

            Debug.Log("[Enemy] 攻击完成，立即返回起始位置");

            // 攻击完成后立即返回起始位置
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            transform.position = startPosition;
        }

        /// <summary>
        /// 计算攻击位置（在目标面前）
        /// </summary>
        private Vector2 CalculateAttackPosition()
        {
            if (attackTarget == null)
            {
                return startPosition;
            }

            Vector2 targetPos = attackTarget.position;
            Vector2 myPos = startPosition;
            
            // 计算方向（从敌人起始位置指向目标）
            Vector2 direction = (targetPos - myPos).normalized;
            
            // 在目标位置前方停下
            Vector2 attackPos = targetPos - direction * attackDistanceOffset;
            
            return attackPos;
        }

        /// <summary>
        /// 播放攻击动画
        /// </summary>
        private void PlayAttackAnimation()
        {
            if (animator != null && !string.IsNullOrEmpty(attackAnimTrigger))
            {
                animator.SetTrigger(attackAnimTrigger);
                Debug.Log($"[Enemy] 触发攻击动画: {attackAnimTrigger}");
            }
        }

        /// <summary>
        /// 缓动函数 - 二次方缓出
        /// </summary>
        private float EaseOutQuad(float t)
        {
            return 1f - (1f - t) * (1f - t);
        }

        void Update()
        {
            // 如果正在攻击，不执行自动返回检测
            if (isAttacking) return;

            // 检测是否偏离起始位置
            if (!IsAtStartPosition() && !isReturning)
            {
                // 开始返回协程
                if (returnCoroutine != null)
                {
                    StopCoroutine(returnCoroutine);
                }
                returnCoroutine = StartCoroutine(ReturnToStartPosition());
            }
        }

        /// <summary>
        /// 检测是否在起始位置
        /// </summary>
        private bool IsAtStartPosition()
        {
            return Vector2.Distance(transform.position, startPosition) <= positionThreshold;
        }

        /// <summary>
        /// 延迟后返回起始位置的协程
        /// </summary>
        private IEnumerator ReturnToStartPosition()
        {
            isReturning = true;
            
            // 等待0.5秒
            yield return new WaitForSeconds(returnDelay);
            
            // 移动到起始位置
            while (!IsAtStartPosition())
            {
                Vector2 direction = (startPosition - (Vector2)transform.position).normalized;
                if (rb != null)
                {
                    rb.velocity = direction * returnSpeed;
                }
                else
                {
                    transform.position = (Vector2)transform.position + direction * returnSpeed * Time.deltaTime;
                }
                yield return null;
            }
            
            // 到达起始位置，停止移动
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            transform.position = startPosition;
            isReturning = false;
        }

        /// <summary>
        /// 重置起始位置（如果需要动态设置起始位置）
        /// </summary>
        public void SetStartPosition(Vector2 newStartPosition)
        {
            startPosition = newStartPosition;
        }

        /// <summary>
        /// 设置攻击目标
        /// </summary>
        public void SetAttackTarget(Transform target)
        {
            attackTarget = target;
        }

        /// <summary>
        /// 立即返回起始位置（不等待延迟）
        /// </summary>
        public void ReturnImmediately()
        {
            // 停止攻击协程
            if (attackCoroutine != null)
            {
                StopCoroutine(attackCoroutine);
                attackCoroutine = null;
            }
            isAttacking = false;

            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
            }
            transform.position = startPosition;
            isReturning = false;
        }

        /// <summary>
        /// 获取是否正在攻击
        /// </summary>
        public bool IsAttacking => isAttacking;
    }
}
