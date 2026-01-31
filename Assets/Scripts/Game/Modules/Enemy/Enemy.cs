using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    public class Enemy : MonoBehaviour
    {
        private Vector2 startPosition;          // 起始位置
        private Rigidbody2D rb;                 // Rigidbody2D组件
        private bool isReturning = false;       // 是否正在返回
        private Coroutine returnCoroutine;      // 返回协程引用
        
        [SerializeField] private float returnDelay = 0.5f;      // 返回延迟时间
        [SerializeField] private float returnSpeed = 5f;        // 返回速度
        [SerializeField] private float positionThreshold = 0.1f; // 位置判断阈值

        void Start()
        {
            // 记录起始位置
            startPosition = transform.position;
            // 获取Rigidbody2D组件
            rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
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
                rb.velocity = direction * returnSpeed;
                yield return null;
            }
            
            // 到达起始位置，停止移动
            rb.velocity = Vector2.zero;
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
        /// 立即返回起始位置（不等待延迟）
        /// </summary>
        public void ReturnImmediately()
        {
            if (returnCoroutine != null)
            {
                StopCoroutine(returnCoroutine);
            }
            rb.velocity = Vector2.zero;
            transform.position = startPosition;
            isReturning = false;
        }
    }
}
