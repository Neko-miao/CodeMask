using System;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Mask状态：Wearing(穿戴) / Active(激活) / MaskMode(面具待机)
    /// </summary>
    public enum MaskState { Wearing, Active, MaskMode }

    public class Mask : MonoBehaviour
    {
        [Header("状态")]
        [SerializeField] private MaskState currentState = MaskState.Active;

        [Header("组件引用")]
        [SerializeField] private GameObject maskButton;
        [SerializeField] private Animation maskAnimation;

        [Header("飞行设置")]
        [SerializeField] private float flightDuration = 1f;
        [SerializeField] private float maxHeight = 3f;

        [Header("物理设置")]
        [SerializeField, Range(0f, 1f)] private float bounciness = 0.3f;
        [SerializeField, Range(0f, 1f)] private float friction = 0.4f;

        [Header("运动检测")]
        [SerializeField] private float motionThreshold = 0.05f;
        [SerializeField] private float stillTimeRequired = 0.3f;

        [Header("穿戴跟随")]
        [SerializeField] private Transform wearingFollowTarget;

        // 组件缓存
        private Rigidbody2D rb;
        private BoxCollider2D boxCollider;

        // 飞行状态
        private bool isFlying;
        private Vector2 flightStart, flightTarget;
        private float flightTimer;

        // 运动检测
        private bool isCheckingMotion;
        private float stillTimer;

        // 初始状态（用于重置）
        private Vector2 initialPosition;
        private Quaternion initialRotation;

        // 公共属性和事件
        public MaskState CurrentState => currentState;
        public bool IsFlying => isFlying;
        public Action OnFlightComplete;
        public Action<MaskState> OnStateChanged;
        public Action<Collider2D> OnHitTarget;

        #region Unity生命周期

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            SetupPhysicsMaterial();
        }

        void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
        }

        void LateUpdate()
        {
            if (currentState == MaskState.Wearing && wearingFollowTarget != null)
            {
                transform.position = wearingFollowTarget.position;
                transform.rotation = Quaternion.identity;
            }
        }

        void FixedUpdate()
        {
            if (isFlying) UpdateFlight();
            if (isCheckingMotion) CheckMotion();
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 激活并飞向目标（发射用，落地后自动变MaskMode）
        /// </summary>
        public void ActivateAndFlyTo(Vector2 target, float duration = -1f, float height = -1f)
        {
            SetState(MaskState.Active);
            LaunchToTarget(target, duration, height);
        }

        /// <summary>
        /// 飞向目标并穿戴（按键穿戴用）
        /// </summary>
        public void FlyToAndWear(Vector2 target, float duration = -1f, float height = -1f)
        {
            isCheckingMotion = false;
            SetState(MaskState.Active);
            OnFlightComplete = () =>
            {
                SetState(MaskState.Wearing);
                OnFlightComplete = null;
            };
            LaunchToTarget(target, duration, height);
        }

        /// <summary>
        /// 重置到初始位置并激活
        /// </summary>
        public void ResetAndActivate()
        {
            transform.position = initialPosition;
            transform.rotation = initialRotation;
            isCheckingMotion = false;
            if (rb != null) { rb.velocity = Vector2.zero; rb.angularVelocity = 0f; }
            SetState(MaskState.Active);
        }

        public void StopFlight()
        {
            if (isFlying) EndFlight();
        }

        public void SetWearingFollowTarget(Transform target) => wearingFollowTarget = target;

        #endregion

        #region 状态管理

        public void SetState(MaskState newState)
        {
            if (currentState == newState) return;
            currentState = newState;
            ApplyState(newState);
            OnStateChanged?.Invoke(newState);
        }

        private void ApplyState(MaskState state)
        {
            bool isActive = state == MaskState.Active;
            bool isMaskMode = state == MaskState.MaskMode;
            bool isWearing = state == MaskState.Wearing;

            if (maskAnimation != null) maskAnimation.enabled = isActive;
            if (boxCollider != null) boxCollider.enabled = isActive;
            if (rb != null) rb.simulated = isActive;
            if (maskButton != null) maskButton.SetActive(isMaskMode);
            if (isWearing) transform.rotation = Quaternion.identity;
        }

        #endregion

        #region 飞行逻辑

        private void LaunchToTarget(Vector2 target, float duration = -1f, float height = -1f)
        {
            if (duration > 0) flightDuration = duration;
            if (height > 0) maxHeight = height;

            flightStart = transform.position;
            flightTarget = target;
            flightTimer = 0f;
            isFlying = true;

            if (rb != null)
            {
                rb.gravityScale = 0f;
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        private void UpdateFlight()
        {
            flightTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(flightTimer / flightDuration);

            Vector2 linear = Vector2.Lerp(flightStart, flightTarget, t);
            float parabola = 4f * maxHeight * t * (1f - t);
            Vector2 pos = new Vector2(linear.x, linear.y + parabola);

            if (rb != null) rb.MovePosition(pos);
            else transform.position = pos;

            Vector2 dir = pos - (Vector2)transform.position;
            if (dir.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (rb != null) rb.MoveRotation(angle);
                else transform.rotation = Quaternion.Euler(0, 0, angle);
            }

            if (t >= 1f) EndFlight();
        }

        private void EndFlight()
        {
            isFlying = false;
            if (rb != null) rb.gravityScale = 1f;
            OnFlightComplete?.Invoke();

            if (currentState == MaskState.Active)
            {
                isCheckingMotion = true;
                stillTimer = 0f;
            }
        }

        #endregion

        #region 运动检测

        private void CheckMotion()
        {
            if (rb == null) return;

            float speed = rb.velocity.magnitude;
            float angularSpeed = Mathf.Abs(rb.angularVelocity);

            if (speed < motionThreshold && angularSpeed < motionThreshold * 10f)
            {
                stillTimer += Time.fixedDeltaTime;
                if (stillTimer >= stillTimeRequired)
                {
                    isCheckingMotion = false;
                    SetState(MaskState.MaskMode);
                }
            }
            else stillTimer = 0f;
        }

        #endregion

        #region 物理与碰撞

        private void SetupPhysicsMaterial()
        {
            var mat = new PhysicsMaterial2D("MaskMaterial") { bounciness = bounciness, friction = friction };
            if (rb != null) { rb.sharedMaterial = mat; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; }
            if (boxCollider != null) boxCollider.sharedMaterial = mat;
        }

        void OnTriggerEnter2D(Collider2D other) { if (isFlying) OnHitTarget?.Invoke(other); }
        void OnCollisionEnter2D(Collision2D collision)
        {
            if (isFlying) { EndFlight(); OnHitTarget?.Invoke(collision.collider); }
        }

        #endregion
    }
}