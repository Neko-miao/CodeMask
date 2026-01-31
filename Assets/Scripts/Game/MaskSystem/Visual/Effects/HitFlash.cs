// ================================================
// MaskSystem Visual - 受击闪烁效果
// ================================================

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 受击闪烁效果 - 全屏闪烁
    /// </summary>
    public class HitFlash : MonoBehaviour
    {
        #region 配置

        [Header("组件")]
        [SerializeField] private Image flashImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("默认设置")]
        [SerializeField] private Color defaultColor = Color.red;
        [SerializeField] private float defaultDuration = 0.2f;
        [SerializeField] private float maxAlpha = 0.5f;

        #endregion

        #region 私有字段

        private Coroutine _flashCoroutine;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            if (flashImage != null)
            {
                flashImage.color = new Color(defaultColor.r, defaultColor.g, defaultColor.b, 0f);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 触发闪烁
        /// </summary>
        public void Flash()
        {
            Flash(defaultColor, defaultDuration);
        }

        /// <summary>
        /// 触发闪烁（自定义颜色）
        /// </summary>
        public void Flash(Color color)
        {
            Flash(color, defaultDuration);
        }

        /// <summary>
        /// 触发闪烁（自定义参数）
        /// </summary>
        public void Flash(Color color, float duration)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(FlashCoroutine(color, duration));
        }

        /// <summary>
        /// 触发多次闪烁
        /// </summary>
        public void FlashMultiple(Color color, int count, float interval = 0.1f)
        {
            if (_flashCoroutine != null)
            {
                StopCoroutine(_flashCoroutine);
            }

            _flashCoroutine = StartCoroutine(MultipleFlashCoroutine(color, count, interval));
        }

        #endregion

        #region 协程

        private IEnumerator FlashCoroutine(Color color, float duration)
        {
            if (flashImage != null)
            {
                flashImage.color = new Color(color.r, color.g, color.b, maxAlpha);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = maxAlpha;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float alpha = maxAlpha * (1f - t);

                if (flashImage != null)
                {
                    flashImage.color = new Color(color.r, color.g, color.b, alpha);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = alpha;
                }

                yield return null;
            }

            if (flashImage != null)
            {
                flashImage.color = new Color(color.r, color.g, color.b, 0f);
            }

            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            _flashCoroutine = null;
        }

        private IEnumerator MultipleFlashCoroutine(Color color, int count, float interval)
        {
            for (int i = 0; i < count; i++)
            {
                // 闪烁开
                if (flashImage != null)
                {
                    flashImage.color = new Color(color.r, color.g, color.b, maxAlpha);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = maxAlpha;
                }

                yield return new WaitForSeconds(interval);

                // 闪烁关
                if (flashImage != null)
                {
                    flashImage.color = new Color(color.r, color.g, color.b, 0f);
                }

                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 0f;
                }

                yield return new WaitForSeconds(interval);
            }

            _flashCoroutine = null;
        }

        #endregion

        #region 预设效果

        /// <summary>
        /// 红色受击闪烁
        /// </summary>
        public void FlashDamage()
        {
            Flash(Color.red, 0.15f);
        }

        /// <summary>
        /// 绿色治疗闪烁
        /// </summary>
        public void FlashHeal()
        {
            Flash(Color.green, 0.3f);
        }

        /// <summary>
        /// 白色暴击闪烁
        /// </summary>
        public void FlashCritical()
        {
            FlashMultiple(Color.white, 2, 0.05f);
        }

        /// <summary>
        /// 黄色警告闪烁
        /// </summary>
        public void FlashWarning()
        {
            Flash(Color.yellow, 0.2f);
        }

        #endregion
    }
}

