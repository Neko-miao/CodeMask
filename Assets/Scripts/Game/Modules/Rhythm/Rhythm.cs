using UnityEngine;

namespace Game
{
    /// <summary>
    /// 节奏物体 - 挂载在节奏Prefab上，负责向左移动
    /// 使用纯距离计算，不依赖物理组件
    /// </summary>
    public class Rhythm : MonoBehaviour
    {
        /// <summary>
        /// 移动速度
        /// </summary>
        private float moveSpeed = 0f;

        /// <summary>
        /// 是否启用移动
        /// </summary>
        private bool isMoving = false;

        /// <summary>
        /// 行为类型
        /// </summary>
        private RhythmActionType actionType;

        /// <summary>
        /// 面具类型
        /// </summary>
        private MaskType maskType = MaskType.None;

        #region 脉动缩放效果

        /// <summary>
        /// 是否启用脉动效果
        /// </summary>
        [Header("脉动缩放效果")]
        [SerializeField]
        private bool enablePulse = true;

        /// <summary>
        /// 脉动周期（秒），完成一次放大缩小的时间
        /// </summary>
        [SerializeField]
        private float pulseDuration = 1f;

        /// <summary>
        /// 最大缩放倍数
        /// </summary>
        [SerializeField]
        private float pulseMaxScale = 1.2f;

        /// <summary>
        /// 最小缩放倍数
        /// </summary>
        [SerializeField]
        private float pulseMinScale = 1f;

        /// <summary>
        /// 脉动计时器
        /// </summary>
        private float pulseTimer = 0f;

        /// <summary>
        /// 初始缩放值
        /// </summary>
        private Vector3 originalScale;

        /// <summary>
        /// 是否已记录初始缩放值
        /// </summary>
        private bool hasRecordedOriginalScale = false;

        #endregion

        #region Properties

        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        /// <summary>
        /// 是否正在移动
        /// </summary>
        public bool IsMoving => isMoving;

        /// <summary>
        /// 行为类型
        /// </summary>
        public RhythmActionType ActionType => actionType;

        /// <summary>
        /// 面具类型
        /// </summary>
        public MaskType MaskType => maskType;

        /// <summary>
        /// 当前X位置
        /// </summary>
        public float PositionX => transform.position.x;

        #endregion

        /// <summary>
        /// 初始化节奏物体
        /// </summary>
        /// <param name="speed">移动速度</param>
        /// <param name="type">行为类型</param>
        public void Initialize(float speed, RhythmActionType type)
        {
            Initialize(speed, type, MaskType.None);
        }

        /// <summary>
        /// 初始化节奏物体（带面具类型）
        /// </summary>
        /// <param name="speed">移动速度</param>
        /// <param name="type">行为类型</param>
        /// <param name="mask">面具类型</param>
        public void Initialize(float speed, RhythmActionType type, MaskType mask)
        {
            moveSpeed = speed;
            actionType = type;
            maskType = mask;
            isMoving = true;

            // 初始化脉动效果
            InitializePulse();
        }

        /// <summary>
        /// 设置移动速度
        /// </summary>
        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// 开始移动
        /// </summary>
        public void StartMove()
        {
            isMoving = true;
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void StopMove()
        {
            isMoving = false;
        }

        void Update()
        {
            if (!isMoving || moveSpeed <= 0f) return;

            // 向左移动
            transform.position += Vector3.left * moveSpeed * Time.deltaTime;

            // 脉动缩放效果
            UpdatePulseEffect();
        }

        /// <summary>
        /// 初始化脉动效果
        /// </summary>
        private void InitializePulse()
        {
            if (!enablePulse) return;

            // 记录初始缩放值
            originalScale = transform.localScale;
            hasRecordedOriginalScale = true;
            pulseTimer = 0f;
        }

        /// <summary>
        /// 更新脉动缩放效果
        /// </summary>
        private void UpdatePulseEffect()
        {
            if (!enablePulse || pulseDuration <= 0f) return;

            // 记录初始缩放值（兼容未调用Initialize的情况）
            if (!hasRecordedOriginalScale)
            {
                originalScale = transform.localScale;
                hasRecordedOriginalScale = true;
            }

            // 更新计时器
            pulseTimer += Time.deltaTime;

            // 使用正弦函数实现平滑的放大缩小效果
            // 一个完整周期内：0 -> 最大 -> 最小 -> 最大 -> 0
            // 使用 sin 函数，周期为 pulseDuration
            float t = (pulseTimer / pulseDuration) * Mathf.PI * 2f;
            
            // 将 sin 值从 [-1, 1] 映射到 [pulseMinScale, pulseMaxScale]
            float scaleMultiplier = Mathf.Lerp(pulseMinScale, pulseMaxScale, (Mathf.Sin(t) + 1f) * 0.5f);

            // 应用缩放
            transform.localScale = originalScale * scaleMultiplier;
        }

        /// <summary>
        /// 启用脉动效果
        /// </summary>
        /// <param name="duration">脉动周期（秒）</param>
        /// <param name="maxScale">最大缩放倍数</param>
        /// <param name="minScale">最小缩放倍数</param>
        public void EnablePulse(float duration = 1f, float maxScale = 1.2f, float minScale = 1f)
        {
            enablePulse = true;
            pulseDuration = Mathf.Max(0.1f, duration);
            pulseMaxScale = maxScale;
            pulseMinScale = minScale;
            pulseTimer = 0f;
        }

        /// <summary>
        /// 禁用脉动效果
        /// </summary>
        public void DisablePulse()
        {
            enablePulse = false;
            
            // 恢复原始缩放
            if (hasRecordedOriginalScale)
            {
                transform.localScale = originalScale;
            }
        }

        /// <summary>
        /// 设置脉动参数
        /// </summary>
        /// <param name="duration">脉动周期（秒）</param>
        /// <param name="maxScale">最大缩放倍数</param>
        /// <param name="minScale">最小缩放倍数</param>
        public void SetPulseParameters(float duration, float maxScale, float minScale)
        {
            pulseDuration = Mathf.Max(0.1f, duration);
            pulseMaxScale = maxScale;
            pulseMinScale = minScale;
        }
    }
}
