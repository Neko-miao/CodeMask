using System;
using UnityEngine;
using Game.Battle;

namespace Game
{
    /// <summary>
    /// Mask状态：Wearing(穿戴) / Active(激活) / MaskMode(面具待机)
    /// </summary>
    public enum MaskState { Wearing, Active, MaskMode }

    public class Mask : MonoBehaviour
    {
        [Header("面具类型")]
        [SerializeField] private MaskType maskType = MaskType.None;
        
        [Header("图片显示")]
        [SerializeField] 
        [Tooltip("MaskPresentation父物体，子物体按顺序对应MaskType枚举")]
        private Transform maskPresentation;
        
        [Header("按键显示")]
        [SerializeField]
        [Tooltip("按键字母父物体(Btn)，子物体顺序：bg, Q, W, E")]
        private Transform btnParent;
        [SerializeField] private GameObject keyQ;
        [SerializeField] private GameObject keyW;
        [SerializeField] private GameObject keyE;
        
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
        private Transform[] maskIcons;  // 缓存子物体引用
        private KeyCode boundKey = KeyCode.None;  // 绑定的按键

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
        
        // 面具数据缓存
        private MaskData maskData;

        // 公共属性和事件
        public MaskState CurrentState => currentState;
        public MaskType MaskType => maskType;
        public MaskData MaskData => maskData;
        public KeyCode BoundKey => boundKey;
        public bool IsFlying => isFlying;
        public Action OnFlightComplete;
        public Action<MaskState> OnStateChanged;
        public Action<MaskType> OnMaskTypeChanged;
        public Action<Collider2D> OnHitTarget;

        #region Unity生命周期

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            boxCollider = GetComponent<BoxCollider2D>();
            
            // 缓存MaskPresentation下的所有子物体
            CacheMaskIcons();
            
            // 自动获取按键子物体（如果没有手动设置）
            AutoCacheKeyObjects();
            
            SetupPhysicsMaterial();
        }
        
        /// <summary>
        /// 缓存MaskPresentation下的子物体引用
        /// </summary>
        private void CacheMaskIcons()
        {
            if (maskPresentation == null) return;
            
            int childCount = maskPresentation.childCount;
            maskIcons = new Transform[childCount];
            
            for (int i = 0; i < childCount; i++)
            {
                maskIcons[i] = maskPresentation.GetChild(i);
            }
            
            Debug.Log($"[Mask] 缓存了 {childCount} 个Icon子物体");
        }
        
        /// <summary>
        /// 自动获取按键子物体（如果btnParent已设置但Q/W/E未设置）
        /// </summary>
        private void AutoCacheKeyObjects()
        {
            if (btnParent == null) return;
            
            // 按名称查找子物体
            if (keyQ == null) keyQ = btnParent.Find("Q")?.gameObject;
            if (keyW == null) keyW = btnParent.Find("W")?.gameObject;
            if (keyE == null) keyE = btnParent.Find("E")?.gameObject;
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
        /// 直接穿戴到指定位置（瞬移）
        /// </summary>
        public void WearAt(Vector2 position)
        {
            isCheckingMotion = false;
            isFlying = false;
            transform.position = position;
            transform.rotation = Quaternion.identity;
            if (rb != null) { rb.velocity = Vector2.zero; rb.angularVelocity = 0f; }
            SetState(MaskState.Wearing);
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
        
        /// <summary>
        /// 设置绑定的按键，并更新按键字母显示
        /// </summary>
        /// <param name="key">绑定的按键（Q/W/E）</param>
        public void SetBoundKey(KeyCode key)
        {
            boundKey = key;
            UpdateKeyDisplay();
            Debug.Log($"[Mask] 绑定按键: {key}");
        }
        
        /// <summary>
        /// 根据绑定的按键更新字母显示
        /// </summary>
        private void UpdateKeyDisplay()
        {
            // 隐藏所有按键字母
            if (keyQ != null) keyQ.SetActive(false);
            if (keyW != null) keyW.SetActive(false);
            if (keyE != null) keyE.SetActive(false);
            
            // 显示绑定的按键字母
            switch (boundKey)
            {
                case KeyCode.Q:
                    if (keyQ != null) keyQ.SetActive(true);
                    break;
                case KeyCode.W:
                    if (keyW != null) keyW.SetActive(true);
                    break;
                case KeyCode.E:
                    if (keyE != null) keyE.SetActive(true);
                    break;
            }
        }
        
        /// <summary>
        /// 设置面具类型
        /// </summary>
        /// <param name="type">面具类型</param>
        public void SetMaskType(MaskType type)
        {
            maskType = type;
            maskData = MaskConfig.GetMaskData(type);
            
            // 更新显示
            UpdateIconDisplay();
            
            OnMaskTypeChanged?.Invoke(type);
            Debug.Log($"[Mask] 设置类型: {type}, 名称: {maskData?.Name ?? "Unknown"}");
        }
        
        /// <summary>
        /// 根据当前MaskType更新Icon显示（显示对应子物体，隐藏其他）
        /// </summary>
        private void UpdateIconDisplay()
        {
            if (maskIcons == null || maskIcons.Length == 0)
            {
                return;
            }
            
            int typeIndex = (int)maskType;
            
            for (int i = 0; i < maskIcons.Length; i++)
            {
                if (maskIcons[i] != null)
                {
                    maskIcons[i].gameObject.SetActive(i == typeIndex);
                }
            }
        }
        
        /// <summary>
        /// 获取当前显示的Icon Transform
        /// </summary>
        public Transform GetCurrentIcon()
        {
            int typeIndex = (int)maskType;
            if (maskIcons != null && typeIndex >= 0 && typeIndex < maskIcons.Length)
            {
                return maskIcons[typeIndex];
            }
            return null;
        }
        
        /// <summary>
        /// 获取指定MaskType的Icon Transform
        /// </summary>
        public Transform GetIcon(MaskType type)
        {
            int typeIndex = (int)type;
            if (maskIcons != null && typeIndex >= 0 && typeIndex < maskIcons.Length)
            {
                return maskIcons[typeIndex];
            }
            return null;
        }
        
        /// <summary>
        /// 获取面具攻击力
        /// </summary>
        public int GetAttackPower()
        {
            return maskData?.AttackPower ?? 1;
        }
        
        /// <summary>
        /// 获取面具效果类型
        /// </summary>
        public MaskEffectType GetEffectType()
        {
            return maskData?.EffectType ?? MaskEffectType.Attack;
        }
        
        /// <summary>
        /// 检查是否克制目标面具
        /// </summary>
        public bool IsCounterTo(MaskType targetType)
        {
            return MaskConfig.IsCounter(maskType, targetType);
        }

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