// ================================================
// MaskSystem Visual - 关卡切换UI
// ================================================

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 关卡切换UI - 处理关卡之间的转场效果
    /// </summary>
    public class LevelTransitionUI : MonoBehaviour
    {
        #region 组件

        [Header("转场遮罩")]
        [SerializeField] private Image fadeImage;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("关卡标题")]
        [SerializeField] private GameObject titlePanel;
        [SerializeField] private Text levelNameText;
        [SerializeField] private Text levelDescText;
        [SerializeField] private Text levelNumberText;

        [Header("倒计时")]
        [SerializeField] private Text countdownText;

        [Header("动画设置")]
        [SerializeField] private float fadeInDuration = 0.5f;
        [SerializeField] private float fadeOutDuration = 0.5f;
        [SerializeField] private float titleDisplayDuration = 2f;

        #endregion

        #region 事件

        public event Action OnTransitionComplete;

        #endregion

        #region 公开方法

        /// <summary>
        /// 播放关卡转场
        /// </summary>
        public void PlayLevelTransition(string levelName, string description, int levelNumber, Color transitionColor)
        {
            StartCoroutine(LevelTransitionCoroutine(levelName, description, levelNumber, transitionColor));
        }

        /// <summary>
        /// 播放淡入
        /// </summary>
        public void FadeIn(Color color, float duration = -1)
        {
            if (duration < 0) duration = fadeInDuration;
            StartCoroutine(FadeCoroutine(0f, 1f, color, duration));
        }

        /// <summary>
        /// 播放淡出
        /// </summary>
        public void FadeOut(float duration = -1)
        {
            if (duration < 0) duration = fadeOutDuration;
            StartCoroutine(FadeCoroutine(1f, 0f, fadeImage?.color ?? Color.black, duration));
        }

        /// <summary>
        /// 显示倒计时
        /// </summary>
        public void ShowCountdown(float seconds)
        {
            StartCoroutine(CountdownCoroutine(seconds));
        }

        /// <summary>
        /// 隐藏所有
        /// </summary>
        public void Hide()
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }

            if (titlePanel != null)
            {
                titlePanel.SetActive(false);
            }

            if (countdownText != null)
            {
                countdownText.gameObject.SetActive(false);
            }
        }

        #endregion

        #region 协程

        private IEnumerator LevelTransitionCoroutine(string levelName, string description, int levelNumber, Color transitionColor)
        {
            // 淡入
            yield return FadeCoroutine(0f, 1f, transitionColor, fadeInDuration);

            // 显示标题
            if (titlePanel != null)
            {
                titlePanel.SetActive(true);

                if (levelNameText != null)
                {
                    levelNameText.text = levelName;
                }

                if (levelDescText != null)
                {
                    levelDescText.text = description;
                }

                if (levelNumberText != null)
                {
                    levelNumberText.text = $"关卡 {levelNumber}";
                }

                // 标题动画
                yield return TitleAnimationCoroutine();
            }

            // 等待显示
            yield return new WaitForSeconds(titleDisplayDuration);

            // 隐藏标题
            if (titlePanel != null)
            {
                titlePanel.SetActive(false);
            }

            // 淡出
            yield return FadeCoroutine(1f, 0f, transitionColor, fadeOutDuration);

            OnTransitionComplete?.Invoke();
        }

        private IEnumerator FadeCoroutine(float from, float to, Color color, float duration)
        {
            if (fadeImage != null)
            {
                fadeImage.color = color;
            }

            if (canvasGroup == null) yield break;

            float elapsed = 0f;
            canvasGroup.alpha = from;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(from, to, t);
                yield return null;
            }

            canvasGroup.alpha = to;
        }

        private IEnumerator TitleAnimationCoroutine()
        {
            if (titlePanel == null) yield break;

            // 缩放动画
            titlePanel.transform.localScale = Vector3.one * 0.5f;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // 弹性效果
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
                titlePanel.transform.localScale = Vector3.one * scale * t;
                yield return null;
            }

            titlePanel.transform.localScale = Vector3.one;
        }

        private IEnumerator CountdownCoroutine(float seconds)
        {
            if (countdownText == null) yield break;

            countdownText.gameObject.SetActive(true);
            float remaining = seconds;

            while (remaining > 0)
            {
                int display = Mathf.CeilToInt(remaining);
                countdownText.text = display.ToString();

                // 数字动画
                float scale = 1f + (remaining - Mathf.Floor(remaining)) * 0.3f;
                countdownText.transform.localScale = Vector3.one * scale;

                remaining -= Time.deltaTime;
                yield return null;
            }

            countdownText.text = "开始!";
            yield return new WaitForSeconds(0.5f);
            countdownText.gameObject.SetActive(false);
        }

        #endregion
    }
}

