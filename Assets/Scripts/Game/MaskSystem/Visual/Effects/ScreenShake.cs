// ================================================
// MaskSystem Visual - 屏幕震动效果
// ================================================

using System.Collections;
using UnityEngine;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 屏幕震动效果
    /// </summary>
    public class ScreenShake : MonoBehaviour
    {
        #region 配置

        [Header("默认设置")]
        [SerializeField] private float defaultDuration = 0.3f;
        [SerializeField] private float defaultMagnitude = 10f;

        [Header("震动目标")]
        [SerializeField] private RectTransform targetTransform;
        [SerializeField] private Camera targetCamera;

        #endregion

        #region 私有字段

        private Vector3 _originalPosition;
        private Coroutine _shakeCoroutine;
        private bool _isShaking;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (targetTransform != null)
            {
                _originalPosition = targetTransform.anchoredPosition;
            }
            else if (targetCamera != null)
            {
                _originalPosition = targetCamera.transform.localPosition;
            }
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 触发屏幕震动
        /// </summary>
        public void Shake()
        {
            Shake(defaultDuration, defaultMagnitude);
        }

        /// <summary>
        /// 触发屏幕震动（自定义参数）
        /// </summary>
        public void Shake(float duration, float magnitude)
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
            }

            _shakeCoroutine = StartCoroutine(ShakeCoroutine(duration, magnitude));
        }

        /// <summary>
        /// 停止震动
        /// </summary>
        public void StopShake()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }

            _isShaking = false;
            ResetPosition();
        }

        #endregion

        #region 协程

        private IEnumerator ShakeCoroutine(float duration, float magnitude)
        {
            _isShaking = true;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // 震动幅度随时间减小
                float currentMagnitude = magnitude * (1f - elapsed / duration);

                // 随机偏移
                float x = Random.Range(-1f, 1f) * currentMagnitude;
                float y = Random.Range(-1f, 1f) * currentMagnitude;

                if (targetTransform != null)
                {
                    targetTransform.anchoredPosition = _originalPosition + new Vector3(x, y, 0);
                }
                else if (targetCamera != null)
                {
                    targetCamera.transform.localPosition = _originalPosition + new Vector3(x, y, 0);
                }

                yield return null;
            }

            _isShaking = false;
            ResetPosition();
            _shakeCoroutine = null;
        }

        private void ResetPosition()
        {
            if (targetTransform != null)
            {
                targetTransform.anchoredPosition = _originalPosition;
            }
            else if (targetCamera != null)
            {
                targetCamera.transform.localPosition = _originalPosition;
            }
        }

        #endregion

        #region 预设震动效果

        /// <summary>
        /// 轻微震动（受击）
        /// </summary>
        public void ShakeLight()
        {
            Shake(0.2f, 5f);
        }

        /// <summary>
        /// 中等震动（重击）
        /// </summary>
        public void ShakeMedium()
        {
            Shake(0.3f, 10f);
        }

        /// <summary>
        /// 强烈震动（暴击）
        /// </summary>
        public void ShakeHeavy()
        {
            Shake(0.5f, 20f);
        }

        #endregion
    }
}

