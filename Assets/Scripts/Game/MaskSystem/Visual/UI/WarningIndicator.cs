// ================================================
// MaskSystem Visual - 预警指示器
// ================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 预警指示器 - 显示敌人攻击预警
    /// </summary>
    public class WarningIndicator : MonoBehaviour
    {
        #region 组件

        [Header("组件")]
        [SerializeField] private Image fillImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text warningText;
        [SerializeField] private Image iconImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("动画设置")]
        [SerializeField] private float pulseSpeed = 5f;
        [SerializeField] private float pulseAmplitude = 0.2f;
        [SerializeField] private Color warningColor = Color.red;
        [SerializeField] private Color safeColor = Color.yellow;

        #endregion

        #region 私有字段

        private bool _isActive;
        private float _progress;
        private Coroutine _animationCoroutine;

        #endregion

        #region 公开方法

        /// <summary>
        /// 显示预警
        /// </summary>
        public void Show()
        {
            gameObject.SetActive(true);
            _isActive = true;
            _progress = 0f;

            if (fillImage != null)
            {
                fillImage.fillAmount = 0f;
            }

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
            }
            _animationCoroutine = StartCoroutine(PulseAnimation());
        }

        /// <summary>
        /// 更新进度
        /// </summary>
        public void UpdateProgress(float progress)
        {
            _progress = Mathf.Clamp01(progress);

            if (fillImage != null)
            {
                fillImage.fillAmount = _progress;
                fillImage.color = Color.Lerp(safeColor, warningColor, _progress);
            }
        }

        /// <summary>
        /// 隐藏预警
        /// </summary>
        public void Hide()
        {
            _isActive = false;

            if (_animationCoroutine != null)
            {
                StopCoroutine(_animationCoroutine);
                _animationCoroutine = null;
            }

            gameObject.SetActive(false);
        }

        /// <summary>
        /// 设置预警文本
        /// </summary>
        public void SetText(string text)
        {
            if (warningText != null)
            {
                warningText.text = text;
            }
        }

        #endregion

        #region 动画

        private IEnumerator PulseAnimation()
        {
            float time = 0f;

            while (_isActive)
            {
                time += Time.deltaTime;

                // 脉冲效果
                float pulse = Mathf.Sin(time * pulseSpeed) * pulseAmplitude;

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0.7f + pulse * 0.3f;
                }

                // 缩放
                float scale = 1f + pulse * 0.5f * _progress;
                transform.localScale = Vector3.one * scale;

                // 文本闪烁
                if (warningText != null && _progress > 0.7f)
                {
                    warningText.color = new Color(1, 1, 1, 0.5f + Mathf.Sin(time * 10f) * 0.5f);
                }

                yield return null;
            }
        }

        #endregion
    }
}

