// ================================================
// MaskSystem Visual - 敌人视图
// ================================================

using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 敌人视图 - 处理敌人的视觉表现
    /// </summary>
    public class EnemyView : CharacterView
    {
        #region 额外组件

        [Header("敌人专属")]
        [Tooltip("敌人名称文本")]
        [SerializeField] private Text nameText;

        [Tooltip("敌人面具类型图标")]
        [SerializeField] private Image maskTypeIcon;

        [Tooltip("预警指示器")]
        [SerializeField] private Image warningIndicator;

        [Tooltip("预警进度条")]
        [SerializeField] private Image warningProgress;

        #endregion

        #region 私有字段

        private GameAssetsConfig _assets;
        private bool _isWarning;

        #endregion

        #region 初始化

        public override void Initialize(GameAssetsConfig assets)
        {
            base.Initialize(assets);
            _assets = assets;
            HideWarning();
        }

        #endregion

        #region 设置

        /// <summary>
        /// 设置敌人信息
        /// </summary>
        public void SetEnemy(string name, MaskType maskType, int health, int maxHealth)
        {
            if (nameText != null)
            {
                nameText.text = name;
            }

            SetMask(maskType, _assets);
            SetHealth(health, maxHealth);

            // 设置面具类型图标
            if (maskTypeIcon != null && _assets != null)
            {
                var visual = _assets.GetMaskVisual(maskType);
                if (visual?.Icon != null)
                {
                    maskTypeIcon.sprite = visual.Icon;
                    maskTypeIcon.color = Color.white;
                }
            }
        }

        /// <summary>
        /// 设置面具
        /// </summary>
        public override void SetMask(MaskType mask, GameAssetsConfig assets)
        {
            base.SetMask(mask, assets);

            // 敌人根据面具类型设置整体色调
            if (assets != null && characterImage != null)
            {
                var visual = assets.GetMaskVisual(mask);
                if (visual != null)
                {
                    // 如果没有角色图，使用主题色
                    if (visual.CharacterSprite == null)
                    {
                        characterImage.color = visual.ThemeColor;
                    }
                }
            }
        }

        #endregion

        #region 预警

        /// <summary>
        /// 显示攻击预警
        /// </summary>
        public void ShowWarning()
        {
            _isWarning = true;

            if (warningIndicator != null)
            {
                warningIndicator.gameObject.SetActive(true);
                StartCoroutine(WarningPulseAnimation());
            }

            if (warningProgress != null)
            {
                warningProgress.gameObject.SetActive(true);
                warningProgress.fillAmount = 0f;
            }
        }

        /// <summary>
        /// 更新预警进度
        /// </summary>
        public void UpdateWarningProgress(float progress)
        {
            if (warningProgress != null)
            {
                warningProgress.fillAmount = progress;

                // 根据进度改变颜色
                warningProgress.color = Color.Lerp(Color.yellow, Color.red, progress);
            }
        }

        /// <summary>
        /// 隐藏预警
        /// </summary>
        public void HideWarning()
        {
            _isWarning = false;

            if (warningIndicator != null)
            {
                warningIndicator.gameObject.SetActive(false);
            }

            if (warningProgress != null)
            {
                warningProgress.gameObject.SetActive(false);
            }
        }

        private System.Collections.IEnumerator WarningPulseAnimation()
        {
            if (warningIndicator == null) yield break;

            Color baseColor = Color.red;
            float pulseSpeed = 5f;
            float elapsed = 0f;

            while (_isWarning)
            {
                elapsed += Time.deltaTime;
                float alpha = (Mathf.Sin(elapsed * pulseSpeed) + 1f) * 0.5f;
                warningIndicator.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha * 0.8f + 0.2f);

                // 缩放脉冲
                float scale = 1f + Mathf.Sin(elapsed * pulseSpeed) * 0.1f;
                warningIndicator.rectTransform.localScale = Vector3.one * scale;

                yield return null;
            }
        }

        #endregion

        #region 动画重写

        public override void PlayAttackAnimation(Vector3 targetDirection)
        {
            // 敌人攻击向左
            base.PlayAttackAnimation(Vector3.left);
            HideWarning();
        }

        /// <summary>
        /// 播放入场动画
        /// </summary>
        public void PlayEntranceAnimation()
        {
            StartCoroutine(EntranceAnimationCoroutine());
        }

        private System.Collections.IEnumerator EntranceAnimationCoroutine()
        {
            if (characterRoot == null) yield break;

            // 从右侧滑入
            Vector3 startPos = originalPosition + new Vector3(300, 0, 0);
            Vector3 endPos = originalPosition;

            characterRoot.anchoredPosition = startPos;
            SetVisible(true);

            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // 缓出效果
                t = 1 - (1 - t) * (1 - t);
                characterRoot.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
                yield return null;
            }

            characterRoot.anchoredPosition = endPos;

            // 弹跳效果
            elapsed = 0f;
            duration = 0.2f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.15f;
                characterRoot.localScale = Vector3.one * scale;
                yield return null;
            }

            characterRoot.localScale = Vector3.one;
        }

        /// <summary>
        /// 播放被击败退场动画
        /// </summary>
        public void PlayDefeatAnimation()
        {
            StartCoroutine(DefeatAnimationCoroutine());
        }

        private System.Collections.IEnumerator DefeatAnimationCoroutine()
        {
            if (characterRoot == null || characterImage == null) yield break;

            // 震动
            float shakeDuration = 0.3f;
            float shakeAmount = 20f;
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                elapsed += Time.deltaTime;
                float x = UnityEngine.Random.Range(-shakeAmount, shakeAmount) * (1 - elapsed / shakeDuration);
                characterRoot.anchoredPosition = originalPosition + new Vector3(x, 0, 0);
                yield return null;
            }

            // 淡出并向右滑出
            Vector3 startPos = originalPosition;
            Vector3 endPos = originalPosition + new Vector3(200, -100, 0);
            Color startColor = characterImage.color;

            float exitDuration = 0.4f;
            elapsed = 0f;

            while (elapsed < exitDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / exitDuration;

                characterRoot.anchoredPosition = Vector3.Lerp(startPos, endPos, t);
                characterImage.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);
                characterRoot.localScale = Vector3.one * (1 - t * 0.5f);
                characterRoot.rotation = Quaternion.Euler(0, 0, -t * 30f);

                yield return null;
            }

            SetVisible(false);
        }

        #endregion

        #region 重置

        public override void ResetView()
        {
            base.ResetView();
            HideWarning();

            if (characterRoot != null)
            {
                characterRoot.rotation = Quaternion.identity;
                characterRoot.localScale = Vector3.one;
            }
        }

        #endregion
    }
}

