using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    /// <summary>
    /// Mask的三种状态
    /// 待机状态 关闭MaskPresentation的动画，关闭自身的BoxCollider2d以及Rigidbody2d 关闭按钮
    /// 开启状态 启动上述功能 关闭按钮
    /// 面具状态 待机状态 + 显示按钮
    /// </summary>
    public enum MaskState
    {
        Idle,       // 待机状态
        Active,     // 开启状态
        MaskMode    // 面具状态
    }
    
    public class Mask : MonoBehaviour, IMaskStateController
    {
        [Header("状态引用")]
        [SerializeField] private GameObject maskButton;              // 按钮引用
        [SerializeField] private Animation maskAnimation;            // 动画引用（MaskPresentation的动画）
        
        [Header("抛物线设置")]
        [SerializeField] private float flightDuration = 1f;      // 飞行时间
        [SerializeField] private float maxHeight = 3f;           // 抛物线最大高度
        
        [Header("物理设置")]
        [SerializeField, Range(0f, 1f)] private float bounciness = 0.3f;  // 弹力系数 (0-1)
        [SerializeField, Range(0f, 1f)] private float friction = 0.4f;    // 摩擦力 (0-1)
        
        [Header("测试设置")]
        [SerializeField] private Transform testTarget;           // 测试目标点1引用
        [SerializeField] private Transform testTarget2;          // 测试目标点2引用
        
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;
        
        private bool isFlying = false;
        private Vector2 flightStartPosition;
        private Vector2 targetPosition;
        private float flightTimer = 0f;
        private Vector2 previousPosition;  // 用于碰撞检测
        
        private MaskState currentState = MaskState.Idle;  // 当前状态
        
        // 测试用变量
        private int testStep = 0;                         // 当前测试步骤 (0-3)
        private Vector2 initialPosition;                  // 初始位置（用于重置）
        private Quaternion initialRotation;               // 初始旋转（用于重置）
        
        // 事件（用于接口实现）
        private event Action<MaskState> onMaskStateChanged;
        private event Action onMaskFlightComplete;
        
        /// <summary>
        /// 当前Mask状态
        /// </summary>
        public MaskState CurrentState => currentState;
        
        /// <summary>
        /// 飞行完成时的回调
        /// </summary>
        public Action OnFlightComplete;
        
        /// <summary>
        /// 碰撞到目标时的回调
        /// </summary>
        public Action<Collider2D> OnHitTarget;
        
        /// <summary>
        /// 状态改变时的回调
        /// </summary>
        public Action<MaskState> OnStateChanged;
        
        #region IMaskStateController 接口实现
        
        /// <summary>
        /// 是否正在飞行（接口实现）
        /// </summary>
        bool IMaskStateController.IsFlying => isFlying;
        
        /// <summary>
        /// 状态改变时的事件（接口实现）
        /// </summary>
        event Action<MaskState> IMaskStateController.OnMaskStateChanged
        {
            add => onMaskStateChanged += value;
            remove => onMaskStateChanged -= value;
        }
        
        /// <summary>
        /// 飞行完成时的事件（接口实现）
        /// </summary>
        event Action IMaskStateController.OnMaskFlightComplete
        {
            add => onMaskFlightComplete += value;
            remove => onMaskFlightComplete -= value;
        }
        
        /// <summary>
        /// 操作1：切换到开启状态并飞向目标
        /// </summary>
        public void ActivateAndFlyTo(Vector2 target, float duration = -1f, float height = -1f)
        {
            SetState(MaskState.Active);
            LaunchToTarget(target, duration, height);
        }
        
        /// <summary>
        /// 操作2：切换到面具状态
        /// </summary>
        public void SwitchToMaskMode()
        {
            SetState(MaskState.MaskMode);
        }
        
        /// <summary>
        /// 操作3：切换到开启状态，飞向目标，到达后切换到待机状态
        /// </summary>
        public void ActivateFlyToThenIdle(Vector2 target, float duration = -1f, float height = -1f)
        {
            SetState(MaskState.Active);
            
            // 注册飞行完成回调，到达后切换到待机状态
            OnFlightComplete = () =>
            {
                SetState(MaskState.Idle);
                OnFlightComplete = null;
            };
            
            LaunchToTarget(target, duration, height);
        }
        
        /// <summary>
        /// 操作4：重置到初始位置和旋转，并切换到开启状态
        /// </summary>
        public void ResetAndActivate()
        {
            // 重置位置和旋转
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            
            // 重置Rigidbody2D状态
            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
            
            // 切换到开启状态
            SetState(MaskState.Active);
        }
        
        #endregion

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
            // 记录初始位置和旋转（用于测试重置）
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            
            // 默认设置为待机状态
            SetState(MaskState.Idle);
        }
        
        #region 状态管理
        
        /// <summary>
        /// 设置Mask状态
        /// </summary>
        /// <param name="newState">新状态</param>
        public void SetState(MaskState newState)
        {
            if (currentState == newState) return;
            
            currentState = newState;
            ApplyState(newState);
            OnStateChanged?.Invoke(newState);
            onMaskStateChanged?.Invoke(newState);  // 触发接口事件
        }
        
        /// <summary>
        /// 应用状态设置
        /// </summary>
        private void ApplyState(MaskState state)
        {
            switch (state)
            {
                case MaskState.Idle:
                    ApplyIdleState();
                    break;
                case MaskState.Active:
                    ApplyActiveState();
                    break;
                case MaskState.MaskMode:
                    ApplyMaskModeState();
                    break;
            }
        }
        
        /// <summary>
        /// 待机状态：关闭动画、关闭物理组件、关闭按钮
        /// </summary>
        private void ApplyIdleState()
        {
            // 关闭动画
            SetAnimationEnabled(false);
            // 关闭物理组件
            SetPhysicsEnabled(false);
            // 关闭按钮
            SetButtonVisible(false);
        }
        
        /// <summary>
        /// 开启状态：启动动画、启动物理组件、关闭按钮
        /// </summary>
        private void ApplyActiveState()
        {
            // 开启动画
            SetAnimationEnabled(true);
            // 开启物理组件
            SetPhysicsEnabled(true);
            // 关闭按钮
            SetButtonVisible(false);
        }
        
        /// <summary>
        /// 面具状态：待机状态 + 显示按钮
        /// </summary>
        private void ApplyMaskModeState()
        {
            // 关闭动画
            SetAnimationEnabled(false);
            // 关闭物理组件
            SetPhysicsEnabled(false);
            // 显示按钮
            SetButtonVisible(true);
        }
        
        /// <summary>
        /// 设置动画组件启用状态
        /// </summary>
        private void SetAnimationEnabled(bool enabled)
        {
            if (maskAnimation != null)
            {
                maskAnimation.enabled = enabled;
            }
        }
        
        /// <summary>
        /// 设置物理组件启用状态
        /// </summary>
        private void SetPhysicsEnabled(bool enabled)
        {
            if (boxCollider != null)
            {
                boxCollider.enabled = enabled;
            }
            if (rb != null)
            {
                rb.simulated = enabled;
            }
        }
        
        /// <summary>
        /// 设置按钮可见性
        /// </summary>
        private void SetButtonVisible(bool visible)
        {
            if (maskButton != null)
            {
                maskButton.gameObject.SetActive(visible);
            }
        }
        
        /// <summary>
        /// 切换到待机状态
        /// </summary>
        public void SetIdleState()
        {
            SetState(MaskState.Idle);
        }
        
        /// <summary>
        /// 切换到开启状态
        /// </summary>
        public void SetActiveState()
        {
            SetState(MaskState.Active);
        }
        
        /// <summary>
        /// 切换到面具状态
        /// </summary>
        public void SetMaskModeState()
        {
            SetState(MaskState.MaskMode);
        }
        
        #endregion

        private void Update()
        {
            // 测试代码：按 A 键执行测试步骤
            if (Input.GetKeyDown(KeyCode.A) && !isFlying)
            {
                ExecuteTestStep();
            }
        }
        
        private void FixedUpdate()
        {
            if (isFlying)
            {
                UpdateParabolicFlight();
            }
        }
        
        #region 测试逻辑
        
        /// <summary>
        /// 执行当前测试步骤
        /// </summary>
        private void ExecuteTestStep()
        {
            switch (testStep)
            {
                case 0:
                    // 第一次按A：切换到开启状态，飞向目标1
                    TestStep1_FlyToTarget1();
                    break;
                case 1:
                    // 第二次按A：切换到面具状态
                    TestStep2_SwitchToMaskMode();
                    break;
                case 2:
                    // 第三次按A：切换到开启状态，飞到目标2，到达后切换到待机状态
                    TestStep3_FlyToTarget2();
                    break;
                case 3:
                    // 第四次按A：回到起始位置，重置位置和旋转，切换到开启状态
                    TestStep4_ResetAndActivate();
                    break;
            }
            
            // 步骤递增，循环 0-3
            testStep = (testStep + 1) % 4;
        }
        
        /// <summary>
        /// 测试步骤1：切换到开启状态，飞向目标1
        /// </summary>
        private void TestStep1_FlyToTarget1()
        {
            if (testTarget == null)
            {
                Debug.LogWarning("[Mask] 未设置测试目标1 (testTarget)，请在 Inspector 中拖入目标 Transform!");
                return;
            }
            
            Debug.Log($"[Mask] 步骤1: 切换到开启状态，飞向目标1 {testTarget.name}");
            ActivateAndFlyTo(testTarget.position);
        }
        
        /// <summary>
        /// 测试步骤2：切换到面具状态
        /// </summary>
        private void TestStep2_SwitchToMaskMode()
        {
            Debug.Log("[Mask] 步骤2: 切换到面具状态");
            SwitchToMaskMode();
        }
        
        /// <summary>
        /// 测试步骤3：切换到开启状态，飞到目标2，到达后切换到待机状态
        /// </summary>
        private void TestStep3_FlyToTarget2()
        {
            if (testTarget2 == null)
            {
                Debug.LogWarning("[Mask] 未设置测试目标2 (testTarget2)，请在 Inspector 中拖入目标 Transform!");
                return;
            }
            
            Debug.Log($"[Mask] 步骤3: 切换到开启状态，飞向目标2 {testTarget2.name}，到达后切换到待机状态");
            ActivateFlyToThenIdle(testTarget2.position);
        }
        
        /// <summary>
        /// 测试步骤4：回到起始位置，重置位置和旋转，切换到开启状态
        /// </summary>
        private void TestStep4_ResetAndActivate()
        {
            Debug.Log("[Mask] 步骤4: 回到起始位置，重置位置和旋转，切换到开启状态");
            ResetAndActivate();
        }
        
        /// <summary>
        /// 重置测试步骤（从头开始）
        /// </summary>
        public void ResetTestStep()
        {
            testStep = 0;
            Debug.Log("[Mask] 测试步骤已重置");
        }
        
        #endregion

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
            
            flightStartPosition = transform.position;
            targetPosition = target;
            flightTimer = 0f;
            isFlying = true;
            previousPosition = flightStartPosition;
            
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
            onMaskFlightComplete?.Invoke();  // 触发接口事件
            
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
            Vector2 linearPos = Vector2.Lerp(flightStartPosition, targetPosition, t);
            
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
            Vector2 prevPos = flightStartPosition;
            
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

