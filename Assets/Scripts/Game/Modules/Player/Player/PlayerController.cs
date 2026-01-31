// ================================================
// Game - 玩家控制器实现
// ================================================

using System;
using System.Collections;
using UnityEngine;
using GameConfigs;

namespace Game.Modules
{
    /// <summary>
    /// 玩家控制器实现
    /// 挂载在玩家预制体上的组件
    /// </summary>
    public class PlayerController : MonoBehaviour, IPlayerController
    {
        #region 序列化字段

        [Header("组件引用")]
        [SerializeField] private Rigidbody2D _rigidbody2D;
        [SerializeField] private Collider2D _collider2D;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Animator _animator;

        [Header("地面检测")]
        [SerializeField] private Transform _groundCheck;
        [SerializeField] private float _groundCheckRadius = 0.2f;
        [SerializeField] private LayerMask _groundLayer;

        #endregion

        #region 私有字段

        private PlayerCharacterData _characterData;
        private PlayerRuntimeStats _runtimeStats;
        private PlayerState _state = PlayerState.None;
        private PlayerAnimationType _currentAnimationType = PlayerAnimationType.Idle;
        private bool _isActive;
        private int _facingDirection = 1;

        // 2D精灵动画
        private SpriteAnimationData _currentSpriteAnimation;
        private int _currentFrameIndex;
        private float _frameTimer;
        private bool _isAnimationPaused;
        private float _animationSpeed = 1f;

        // 移动
        private Vector2 _moveInput;
        private bool _jumpRequested;
        private bool _dashRequested;
        private float _dashTimer;
        private bool _isDashing;

        // 无敌
        private Coroutine _invincibleCoroutine;

        #endregion

        #region 属性实现

        public GameObject GameObject => gameObject;
        public Transform Transform => transform;
        public PlayerCharacterData CharacterData => _characterData;
        public PlayerState State => _state;
        public PlayerRuntimeStats RuntimeStats => _runtimeStats;
        public bool IsActive => _isActive;

        public Vector3 Position
        {
            get => transform.position;
            set => transform.position = value;
        }

        public Quaternion Rotation
        {
            get => transform.rotation;
            set => transform.rotation = value;
        }

        public int FacingDirection => _facingDirection;

        public bool IsGrounded
        {
            get
            {
                if (_groundCheck == null) return true;
                return Physics2D.OverlapCircle(_groundCheck.position, _groundCheckRadius, _groundLayer);
            }
        }

        public int CurrentHealth => _runtimeStats?.CurrentHealth ?? 0;
        public int MaxHealth => _runtimeStats?.MaxHealth ?? 0;
        public float HealthPercent => _runtimeStats?.HealthPercent ?? 0f;
        public bool IsAlive => _runtimeStats?.IsAlive ?? false;
        public bool IsInvincible => _runtimeStats?.IsInvincible ?? false;

        public PlayerAnimationType CurrentAnimationType => _currentAnimationType;

        #endregion

        #region 事件

        public event Action<PlayerState, PlayerState> OnStateChanged;
        public event Action<int, int> OnHealthChanged;
        public event Action<int, GameObject> OnDamaged;
        public event Action OnDeath;
        public event Action<int> OnHealed;
        public event Action<Vector3> OnPositionChanged;
        public event Action<PlayerAnimationType> OnAnimationChanged;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 自动获取组件
            if (_rigidbody2D == null)
                _rigidbody2D = GetComponent<Rigidbody2D>();
            if (_collider2D == null)
                _collider2D = GetComponent<Collider2D>();
            if (_spriteRenderer == null)
                _spriteRenderer = GetComponent<SpriteRenderer>();
            if (_animator == null)
                _animator = GetComponent<Animator>();

            _runtimeStats = new PlayerRuntimeStats();
        }

        private void Update()
        {
            if (!_isActive) return;

            UpdateInvincible();
            UpdateSpriteAnimation();
            UpdateState();
        }

        private void FixedUpdate()
        {
            if (!_isActive) return;

            UpdateMovement();
        }

        private void OnDrawGizmosSelected()
        {
            // 绘制地面检测范围
            if (_groundCheck != null)
            {
                Gizmos.color = IsGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(_groundCheck.position, _groundCheckRadius);
            }
        }

        #endregion

        #region 初始化

        public void Initialize(PlayerCharacterData characterData)
        {
            _characterData = characterData;

            if (_characterData != null)
            {
                _runtimeStats.InitFromBaseStats(_characterData.baseStats);

                // 设置Animator控制器
                if (!_characterData.use2DSpriteAnimation && _animator != null)
                {
                    _animator.runtimeAnimatorController = _characterData.animatorController;
                }
            }

            ChangeState(PlayerState.Idle);
            PlayAnimation(PlayerAnimationType.Idle);

            Debug.Log($"[PlayerController] Initialized with character: {_characterData?.characterName}");
        }

        public void Reset()
        {
            _runtimeStats?.Reset();
            _moveInput = Vector2.zero;
            _jumpRequested = false;
            _dashRequested = false;
            _isDashing = false;
            _dashTimer = 0f;
            _isAnimationPaused = false;
            _animationSpeed = 1f;

            ChangeState(PlayerState.Idle);
            PlayAnimation(PlayerAnimationType.Idle);
        }

        public void Activate()
        {
            _isActive = true;
            gameObject.SetActive(true);
        }

        public void Deactivate()
        {
            _isActive = false;
            gameObject.SetActive(false);
        }

        public void Destroy()
        {
            _isActive = false;
            if (gameObject != null)
            {
                UnityEngine.Object.Destroy(gameObject);
            }
        }

        #endregion

        #region 生命值操作

        public void TakeDamage(int damage, GameObject source = null)
        {
            if (!IsAlive || IsInvincible) return;

            // 计算实际伤害（考虑防御）
            int actualDamage = Mathf.Max(1, damage - _runtimeStats.Defense);

            int oldHealth = _runtimeStats.CurrentHealth;
            _runtimeStats.CurrentHealth = Mathf.Max(0, _runtimeStats.CurrentHealth - actualDamage);

            Debug.Log($"[PlayerController] TakeDamage: {actualDamage} (raw: {damage}), Health: {oldHealth} -> {_runtimeStats.CurrentHealth}");

            OnHealthChanged?.Invoke(oldHealth, _runtimeStats.CurrentHealth);
            OnDamaged?.Invoke(actualDamage, source);

            if (_runtimeStats.CurrentHealth <= 0)
            {
                Die();
            }
            else
            {
                // 受伤状态
                ChangeState(PlayerState.Hurt);
                PlayAnimation(PlayerAnimationType.Hurt);

                // 受伤后无敌
                if (_characterData != null && _characterData.baseStats.invincibleDuration > 0)
                {
                    SetInvincible(true, _characterData.baseStats.invincibleDuration);
                }
            }
        }

        public void Heal(int amount)
        {
            if (!IsAlive || amount <= 0) return;

            int oldHealth = _runtimeStats.CurrentHealth;
            _runtimeStats.CurrentHealth = Mathf.Min(_runtimeStats.MaxHealth, _runtimeStats.CurrentHealth + amount);

            int actualHeal = _runtimeStats.CurrentHealth - oldHealth;
            if (actualHeal > 0)
            {
                Debug.Log($"[PlayerController] Heal: {actualHeal}, Health: {oldHealth} -> {_runtimeStats.CurrentHealth}");
                OnHealthChanged?.Invoke(oldHealth, _runtimeStats.CurrentHealth);
                OnHealed?.Invoke(actualHeal);
            }
        }

        public void SetHealth(int health)
        {
            int oldHealth = _runtimeStats.CurrentHealth;
            _runtimeStats.CurrentHealth = Mathf.Clamp(health, 0, _runtimeStats.MaxHealth);

            if (oldHealth != _runtimeStats.CurrentHealth)
            {
                OnHealthChanged?.Invoke(oldHealth, _runtimeStats.CurrentHealth);
            }
        }

        public void SetMaxHealth(int maxHealth, bool healToFull = false)
        {
            _runtimeStats.MaxHealth = Mathf.Max(1, maxHealth);

            if (healToFull)
            {
                int oldHealth = _runtimeStats.CurrentHealth;
                _runtimeStats.CurrentHealth = _runtimeStats.MaxHealth;
                OnHealthChanged?.Invoke(oldHealth, _runtimeStats.CurrentHealth);
            }
            else
            {
                _runtimeStats.CurrentHealth = Mathf.Min(_runtimeStats.CurrentHealth, _runtimeStats.MaxHealth);
            }
        }

        public void SetInvincible(bool invincible, float duration = 0f)
        {
            if (_invincibleCoroutine != null)
            {
                StopCoroutine(_invincibleCoroutine);
                _invincibleCoroutine = null;
            }

            _runtimeStats.IsInvincible = invincible;
            _runtimeStats.InvincibleTimeRemaining = duration;

            if (invincible && duration > 0f)
            {
                _invincibleCoroutine = StartCoroutine(InvincibleCoroutine(duration));
            }
        }

        private IEnumerator InvincibleCoroutine(float duration)
        {
            yield return new WaitForSeconds(duration);
            _runtimeStats.IsInvincible = false;
            _runtimeStats.InvincibleTimeRemaining = 0f;
            _invincibleCoroutine = null;
        }

        private void Die()
        {
            ChangeState(PlayerState.Dead);
            PlayAnimation(PlayerAnimationType.Die);
            OnDeath?.Invoke();

            Debug.Log("[PlayerController] Player died");
        }

        #endregion

        #region 移动操作

        public void Move(Vector2 input)
        {
            _moveInput = input;

            // 更新朝向
            if (input.x != 0)
            {
                int newFacing = input.x > 0 ? 1 : -1;
                if (newFacing != _facingDirection)
                {
                    _facingDirection = newFacing;
                    UpdateFacingDirection();
                }
            }
        }

        public void MoveTo(Vector3 targetPosition)
        {
            Vector3 direction = (targetPosition - transform.position).normalized;
            Move(new Vector2(direction.x, direction.y));
        }

        public void Jump()
        {
            if (IsGrounded && _state != PlayerState.Jumping && _state != PlayerState.Dead)
            {
                _jumpRequested = true;
            }
        }

        public void Dash()
        {
            if (!_isDashing && _state != PlayerState.Dead)
            {
                _dashRequested = true;
            }
        }

        public void Teleport(Vector3 position)
        {
            transform.position = position;
            OnPositionChanged?.Invoke(position);
        }

        public void SetMoveSpeed(float speed)
        {
            _runtimeStats.MoveSpeed = speed;
        }

        private void UpdateMovement()
        {
            if (_state == PlayerState.Dead) return;

            float moveSpeed = _runtimeStats.MoveSpeed;

            // 冲刺处理
            if (_dashRequested && !_isDashing)
            {
                _isDashing = true;
                _dashTimer = _characterData?.baseStats.dashDuration ?? 0.2f;
                _dashRequested = false;
                ChangeState(PlayerState.Dashing);
            }

            if (_isDashing)
            {
                moveSpeed *= _characterData?.baseStats.dashSpeedMultiplier ?? 2f;
                _dashTimer -= Time.fixedDeltaTime;

                if (_dashTimer <= 0)
                {
                    _isDashing = false;
                }
            }

            // 水平移动
            if (_rigidbody2D != null)
            {
                Vector2 velocity = _rigidbody2D.velocity;
                velocity.x = _moveInput.x * moveSpeed;

                // 跳跃处理
                if (_jumpRequested)
                {
                    velocity.y = _characterData?.baseStats.jumpForce ?? 8f;
                    _jumpRequested = false;
                    ChangeState(PlayerState.Jumping);
                    PlayAnimation(PlayerAnimationType.Jump);
                }

                _rigidbody2D.velocity = velocity;
            }
        }

        private void UpdateFacingDirection()
        {
            if (_spriteRenderer != null)
            {
                _spriteRenderer.flipX = _facingDirection < 0;
            }
        }

        #endregion

        #region 动画操作

        public void PlayAnimation(PlayerAnimationType animationType, bool forceRestart = false)
        {
            if (!forceRestart && _currentAnimationType == animationType) return;

            _currentAnimationType = animationType;
            OnAnimationChanged?.Invoke(animationType);

            if (_characterData != null && _characterData.use2DSpriteAnimation)
            {
                // 使用2D精灵动画
                _currentSpriteAnimation = _characterData.GetSpriteAnimation(animationType);
                _currentFrameIndex = 0;
                _frameTimer = 0f;

                if (_currentSpriteAnimation != null && _currentSpriteAnimation.frames.Count > 0)
                {
                    UpdateSpriteFrame();
                }
            }
            else if (_animator != null)
            {
                // 使用Animator
                _animator.Play(animationType.ToString());
            }
        }

        public void SetAnimationSpeed(float speed)
        {
            _animationSpeed = speed;

            if (_animator != null)
            {
                _animator.speed = speed;
            }
        }

        public void PauseAnimation()
        {
            _isAnimationPaused = true;

            if (_animator != null)
            {
                _animator.speed = 0f;
            }
        }

        public void ResumeAnimation()
        {
            _isAnimationPaused = false;

            if (_animator != null)
            {
                _animator.speed = _animationSpeed;
            }
        }

        private void UpdateSpriteAnimation()
        {
            if (_isAnimationPaused || _currentSpriteAnimation == null) return;
            if (_currentSpriteAnimation.frames.Count == 0) return;

            float frameTime = 1f / (_currentSpriteAnimation.frameRate * _animationSpeed);
            _frameTimer += Time.deltaTime;

            if (_frameTimer >= frameTime)
            {
                _frameTimer -= frameTime;
                _currentFrameIndex++;

                if (_currentFrameIndex >= _currentSpriteAnimation.frames.Count)
                {
                    if (_currentSpriteAnimation.loop)
                    {
                        _currentFrameIndex = 0;
                    }
                    else
                    {
                        // 动画播放完毕，切换到下一个动画
                        _currentFrameIndex = _currentSpriteAnimation.frames.Count - 1;
                        PlayAnimation(_currentSpriteAnimation.nextAnimation);
                        return;
                    }
                }

                UpdateSpriteFrame();
            }
        }

        private void UpdateSpriteFrame()
        {
            if (_spriteRenderer != null && _currentSpriteAnimation != null &&
                _currentFrameIndex >= 0 && _currentFrameIndex < _currentSpriteAnimation.frames.Count)
            {
                _spriteRenderer.sprite = _currentSpriteAnimation.frames[_currentFrameIndex];
            }
        }

        #endregion

        #region 战斗操作

        public void Attack()
        {
            if (_state == PlayerState.Dead || _state == PlayerState.Attacking) return;

            ChangeState(PlayerState.Attacking);
            PlayAnimation(PlayerAnimationType.Attack);

            // 攻击逻辑由外部系统处理
        }

        public void UseSkill(int skillIndex)
        {
            if (_state == PlayerState.Dead) return;

            PlayerAnimationType skillAnim = skillIndex switch
            {
                1 => PlayerAnimationType.Skill1,
                2 => PlayerAnimationType.Skill2,
                3 => PlayerAnimationType.Skill3,
                _ => PlayerAnimationType.Skill1
            };

            PlayAnimation(skillAnim);
        }

        #endregion

        #region 状态管理

        private void ChangeState(PlayerState newState)
        {
            if (_state == newState) return;

            var oldState = _state;
            _state = newState;

            OnStateChanged?.Invoke(oldState, newState);
        }

        private void UpdateState()
        {
            if (_state == PlayerState.Dead) return;

            // 根据当前情况自动更新状态
            if (_state == PlayerState.Hurt)
            {
                // 受伤状态由动画结束自动恢复
                return;
            }

            if (_state == PlayerState.Attacking)
            {
                // 攻击状态由动画结束自动恢复
                return;
            }

            if (_isDashing)
            {
                // 冲刺中
                return;
            }

            if (!IsGrounded)
            {
                if (_rigidbody2D != null && _rigidbody2D.velocity.y > 0)
                {
                    if (_state != PlayerState.Jumping)
                    {
                        ChangeState(PlayerState.Jumping);
                        PlayAnimation(PlayerAnimationType.Jump);
                    }
                }
                else
                {
                    if (_state != PlayerState.Falling)
                    {
                        ChangeState(PlayerState.Falling);
                        PlayAnimation(PlayerAnimationType.Fall);
                    }
                }
            }
            else
            {
                if (Mathf.Abs(_moveInput.x) > 0.01f)
                {
                    if (_state != PlayerState.Moving)
                    {
                        ChangeState(PlayerState.Moving);
                        PlayAnimation(PlayerAnimationType.Walk);
                    }
                }
                else
                {
                    if (_state != PlayerState.Idle)
                    {
                        ChangeState(PlayerState.Idle);
                        PlayAnimation(PlayerAnimationType.Idle);
                    }
                }
            }
        }

        private void UpdateInvincible()
        {
            if (_runtimeStats.IsInvincible && _runtimeStats.InvincibleTimeRemaining > 0)
            {
                _runtimeStats.InvincibleTimeRemaining -= Time.deltaTime;

                // 闪烁效果
                if (_spriteRenderer != null)
                {
                    float alpha = Mathf.PingPong(Time.time * 10f, 1f) * 0.5f + 0.5f;
                    Color color = _spriteRenderer.color;
                    color.a = alpha;
                    _spriteRenderer.color = color;
                }
            }
            else if (_spriteRenderer != null)
            {
                // 恢复正常
                Color color = _spriteRenderer.color;
                color.a = 1f;
                _spriteRenderer.color = color;
            }
        }

        #endregion
    }
}
