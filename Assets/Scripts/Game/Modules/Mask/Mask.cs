using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Mask : MonoBehaviour
    {
        [Header("抛物线设置")]
        [SerializeField] private float flightDuration = 1f;      // 飞行时间
        [SerializeField] private float maxHeight = 3f;           // 抛物线最大高度
        
        [Header("物理设置")]
        [SerializeField, Range(0f, 1f)] private float bounciness = 0.3f;  // 弹力系数 (0-1)
        [SerializeField, Range(0f, 1f)] private float friction = 0.4f;    // 摩擦力 (0-1)
        
        [Header("测试设置")]
        [SerializeField] private Transform testTarget;           // 测试目标点引用
        
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        
        private bool isFlying = false;
        private Vector2 startPosition;
        private Vector2 targetPosition;
        private float flightTimer = 0f;
        private Vector2 previousPosition;  // 用于碰撞检测
        
        /// <summary>
        /// 飞行完成时的回调
        /// </summary>
        public Action OnFlightComplete;
        
        /// <summary>
        /// 碰撞到目标时的回调
        /// </summary>
        public Action<Collider2D> OnHitTarget;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            
            // 设置物理材质，添加弹力
            SetupPhysicsMaterial();
        }
        
        /// <summary>
        /// 设置物理材质（弹力和摩擦力）
        /// </summary>
        private void SetupPhysicsMaterial()
        {
            // 创建物理材质
            PhysicsMaterial2D physicsMaterial = new PhysicsMaterial2D("MaskBounceMaterial");
            physicsMaterial.bounciness = bounciness;
            physicsMaterial.friction = friction;
            
            // 应用到 Rigidbody2D
            if (rb != null)
            {
                rb.sharedMaterial = physicsMaterial;
                // 设置连续碰撞检测，防止高速穿透
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            }
            
            // 也应用到 Collider（确保碰撞时生效）
            if (boxCollider != null)
            {
                boxCollider.sharedMaterial = physicsMaterial;
            }
        }

        private void Start()
        {
            
        }

        private void Update()
        {
            // 测试代码：按 A 键向目标飞去
            if (Input.GetKeyDown(KeyCode.A) && !isFlying)
            {
                TestLaunch();
            }
        }
        
        private void FixedUpdate()
        {
            if (isFlying)
            {
                UpdateParabolicFlight();
            }
        }
        
        /// <summary>
        /// 测试发射（按 A 键触发）
        /// </summary>
        private void TestLaunch()
        {
            if (testTarget != null)
            {
                Debug.Log($"[Mask] 向目标 {testTarget.name} 发射!");
                LaunchToTarget(testTarget.position);
            }
            else
            {
                Debug.LogWarning("[Mask] 未设置测试目标 (testTarget)，请在 Inspector 中拖入目标 Transform!");
            }
        }

        /// <summary>
        /// 发射 Mask 沿抛物线飞向目标点
        /// </summary>
        /// <param name="target">目标位置</param>
        /// <param name="duration">飞行时间（可选）</param>
        /// <param name="height">抛物线高度（可选）</param>
        public void LaunchToTarget(Vector2 target, float duration = -1f, float height = -1f)
        {
            if (duration > 0)
                flightDuration = duration;
            if (height > 0)
                maxHeight = height;
            
            startPosition = transform.position;
            targetPosition = target;
            flightTimer = 0f;
            isFlying = true;
            previousPosition = startPosition;
            
            // 保持 Dynamic 模式但禁用重力，使用 MovePosition 移动以保留碰撞检测
            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        /// <summary>
        /// 更新抛物线飞行
        /// </summary>
        private void UpdateParabolicFlight()
        {
            flightTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(flightTimer / flightDuration);
            
            // 计算抛物线位置
            Vector2 targetPos = CalculateParabolicPosition(t);
            
            // 使用 MovePosition 保留物理碰撞检测
            if (rb != null)
            {
                rb.MovePosition(targetPos);
            }
            else
            {
                transform.position = targetPos;
            }
            
            // 计算并设置旋转（朝向运动方向）
            Vector2 direction = (targetPos - previousPosition).normalized;
            if (direction != Vector2.zero)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                if (rb != null)
                {
                    rb.MoveRotation(angle);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }
            previousPosition = targetPos;
            
            // 飞行结束
            if (t >= 1f)
            {
                EndFlight();
            }
        }
        
        /// <summary>
        /// 结束飞行，恢复正常物理状态
        /// </summary>
        private void EndFlight()
        {
            isFlying = false;
            OnFlightComplete?.Invoke();
            
            // 恢复重力
            if (rb != null)
            {
                rb.gravityScale = 1f;
            }
        }

        /// <summary>
        /// 计算抛物线上的位置
        /// </summary>
        /// <param name="t">归一化时间 [0, 1]</param>
        /// <returns>抛物线上的位置</returns>
        private Vector2 CalculateParabolicPosition(float t)
        {
            // 线性插值 X 和 Y 的基础位置
            Vector2 linearPos = Vector2.Lerp(startPosition, targetPosition, t);
            
            // 抛物线高度：使用 sin 函数创建平滑的抛物线
            // 或者使用经典的抛物线公式：h = 4 * maxHeight * t * (1 - t)
            float parabolicHeight = 4f * maxHeight * t * (1f - t);
            
            // 将高度添加到 Y 坐标
            linearPos.y += parabolicHeight;
            
            return linearPos;
        }

        /// <summary>
        /// 碰撞检测
        /// </summary>
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (isFlying)
            {
                OnHitTarget?.Invoke(other);
            }
        }

        /// <summary>
        /// 碰撞检测（用于非 Trigger 碰撞器）
        /// </summary>
        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (isFlying)
            {
                // 碰到东西，停止抛物线飞行，让物理引擎接管
                EndFlight();
                OnHitTarget?.Invoke(collision.collider);
            }
        }

        /// <summary>
        /// 停止飞行
        /// </summary>
        public void StopFlight()
        {
            if (isFlying)
            {
                EndFlight();
            }
        }

        /// <summary>
        /// 是否正在飞行
        /// </summary>
        public bool IsFlying => isFlying;

#if UNITY_EDITOR
        /// <summary>
        /// 在编辑器中绘制抛物线轨迹（调试用）
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (!Application.isPlaying) return;
            if (!isFlying) return;
            
            Gizmos.color = Color.yellow;
            Vector2 prevPos = startPosition;
            
            for (int i = 1; i <= 20; i++)
            {
                float t = i / 20f;
                Vector2 pos = CalculateParabolicPosition(t);
                Gizmos.DrawLine(prevPos, pos);
                prevPos = pos;
            }
            
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(targetPosition, 0.2f);
        }
#endif
    }
}

