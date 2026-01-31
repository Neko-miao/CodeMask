using UnityEngine;

namespace Game
{
    /// <summary>
    /// 节奏物体 - 挂载在节奏Prefab上，负责向左移动
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

        #endregion

        /// <summary>
        /// 初始化节奏物体
        /// </summary>
        /// <param name="speed">移动速度</param>
        /// <param name="type">行为类型</param>
        public void Initialize(float speed, RhythmActionType type)
        {
            moveSpeed = speed;
            actionType = type;
            isMoving = true;
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
        }
    }
}
