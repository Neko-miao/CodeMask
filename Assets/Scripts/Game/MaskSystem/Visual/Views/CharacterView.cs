// ================================================
// MaskSystem Visual - 角色视图基类
// ================================================

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 角色视图基类 - 处理角色的视觉表现
    /// </summary>
    public abstract class CharacterView : MonoBehaviour
    {
        #region 组件引用

        [Header("组件引用")]
        [Tooltip("角色图片")]
        [SerializeField] protected Image characterImage;

        [Tooltip("面具图片")]
        [SerializeField] protected Image maskImage;

        [Tooltip("血条填充")]
        [SerializeField] protected Image healthBarFill;

        [Tooltip("血条背景")]
        [SerializeField] protected Image healthBarBackground;

        [Tooltip("角色根节点（用于动画）")]
        [SerializeField] protected RectTransform characterRoot;

        [Tooltip("特效生成点")]
        [SerializeField] protected Transform effectSpawnPoint;

        #endregion

        #region 配置

        [Header("动画配置")]
        [Tooltip("攻击时的移动距离")]
        [SerializeField] protected float attackMoveDistance = 50f;

        [Tooltip("攻击动画时长")]
        [SerializeField] protected float attackDuration = 0.3f;

        [Tooltip("受击闪烁次数")]
        [SerializeField] protected int hitFlashCount = 3;

        [Tooltip("受击闪烁间隔")]
        [SerializeField] protected float hitFlashInterval = 0.1f;

        #endregion

        #region 状态

        protected MaskType currentMask;
        protected int currentHealth;
        protected int maxHealth;
        protected Vector3 originalPosition;
        protected bool isAnimating;

        #endregion

        #region 事件

        public event Action OnAttackAnimationComplete;
        public event Action OnHitAnimationComplete;

        #endregion

        #region Unity生命周期

        protected virtual void Awake()
        {
            if (characterRoot != null)
            {
                originalPosition = characterRoot.anchoredPosition;
            }
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 初始化视图
        /// </summary>
        public virtual void Initialize(GameAssetsConfig assets)
        {
            // 子类实现
        }

        /// <summary>
        /// 设置面具
        /// </summary>
        public virtual void SetMask(MaskType mask, GameAssetsConfig assets)
        {
            currentMask = mask;

            if (assets != null)
            {
                var visual = assets.GetMaskVisual(mask);
                if (visual != null)
                {
                    if (characterImage != null && visual.CharacterSprite != null)
                    {
                        characterImage.sprite = visual.CharacterSprite;
                        characterImage.color = Color.white;
                    }

                    if (maskImage != null && visual.MaskSprite != null)
                    {
                        maskImage.sprite = visual.MaskSprite;
                        maskImage.color = Color.white;
                    }
                    else if (maskImage != null && visual.Icon != null)
                    {
                        maskImage.sprite = visual.Icon;
                        maskImage.color = Color.white;
                    }
                }
            }
        }

        /// <summary>
        /// 设置血量
        /// </summary>
        public virtual void SetHealth(int current, int max)
        {
            currentHealth = current;
            maxHealth = max;

            if (healthBarFill != null)
            {
                float ratio = max > 0 ? (float)current / max : 0f;
                healthBarFill.fillAmount = ratio;

                // 根据血量比例改变颜色
                if (ratio > 0.5f)
                    healthBarFill.color = Color.Lerp(Color.yellow, Color.green, (ratio - 0.5f) * 2);
                else
                    healthBarFill.color = Color.Lerp(Color.red, Color.yellow, ratio * 2);
            }
        }

        /// <summary>
        /// 播放攻击动画
        /// </summary>
        public virtual void PlayAttackAnimation(Vector3 targetDirection)
        {
            if (isAnimating) return;
            StartCoroutine(AttackAnimationCoroutine(targetDirection));
        }

        /// <summary>
        /// 播放受击动画
        /// </summary>
        public virtual void PlayHitAnimation()
        {
            if (isAnimating) return;
            StartCoroutine(HitAnimationCoroutine());
        }

        /// <summary>
        /// 播放死亡动画
        /// </summary>
        public virtual void PlayDeathAnimation()
        {
            StartCoroutine(DeathAnimationCoroutine());
        }

        /// <summary>
        /// 生成特效
        /// </summary>
        public virtual void SpawnEffect(GameObject effectPrefab)
        {
            if (effectPrefab == null) return;

            var spawnPos = effectSpawnPoint != null ? effectSpawnPoint.position : transform.position;
            var effect = Instantiate(effectPrefab, spawnPos, Quaternion.identity);
            Destroy(effect, 2f);
        }

        /// <summary>
        /// 设置可见性
        /// </summary>
        public virtual void SetVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        /// <summary>
        /// 重置到初始状态
        /// </summary>
        public virtual void ResetView()
        {
            if (characterRoot != null)
            {
                characterRoot.anchoredPosition = originalPosition;
            }

            if (characterImage != null)
            {
                characterImage.color = Color.white;
            }

            isAnimating = false;
        }

        #endregion

        #region 动画协程

        protected virtual System.Collections.IEnumerator AttackAnimationCoroutine(Vector3 direction)
        {
            isAnimating = true;

            if (characterRoot != null)
            {
                Vector3 startPos = originalPosition;
                Vector3 attackPos = startPos + direction.normalized * attackMoveDistance;

                // 前冲
                float elapsed = 0f;
                float halfDuration = attackDuration * 0.4f;
                while (elapsed < halfDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / halfDuration;
                    t = t * t; // 加速
                    characterRoot.anchoredPosition = Vector3.Lerp(startPos, attackPos, t);
                    yield return null;
                }

                // 回退
                elapsed = 0f;
                float returnDuration = attackDuration * 0.6f;
                while (elapsed < returnDuration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / returnDuration;
                    t = 1 - (1 - t) * (1 - t); // 减速
                    characterRoot.anchoredPosition = Vector3.Lerp(attackPos, startPos, t);
                    yield return null;
                }

                characterRoot.anchoredPosition = startPos;
            }

            isAnimating = false;
            OnAttackAnimationComplete?.Invoke();
        }

        protected virtual System.Collections.IEnumerator HitAnimationCoroutine()
        {
            isAnimating = true;

            if (characterImage != null)
            {
                Color originalColor = characterImage.color;

                for (int i = 0; i < hitFlashCount; i++)
                {
                    characterImage.color = Color.red;
                    yield return new WaitForSeconds(hitFlashInterval);
                    characterImage.color = originalColor;
                    yield return new WaitForSeconds(hitFlashInterval);
                }
            }

            // 轻微震动
            if (characterRoot != null)
            {
                Vector3 startPos = originalPosition;
                float shakeAmount = 10f;
                float shakeDuration = 0.2f;
                float elapsed = 0f;

                while (elapsed < shakeDuration)
                {
                    elapsed += Time.deltaTime;
                    float x = UnityEngine.Random.Range(-shakeAmount, shakeAmount) * (1 - elapsed / shakeDuration);
                    characterRoot.anchoredPosition = startPos + new Vector3(x, 0, 0);
                    yield return null;
                }

                characterRoot.anchoredPosition = startPos;
            }

            isAnimating = false;
            OnHitAnimationComplete?.Invoke();
        }

        protected virtual System.Collections.IEnumerator DeathAnimationCoroutine()
        {
            isAnimating = true;

            if (characterImage != null)
            {
                float duration = 0.5f;
                float elapsed = 0f;
                Color startColor = characterImage.color;
                Vector3 startScale = characterRoot != null ? characterRoot.localScale : Vector3.one;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / duration;

                    // 淡出
                    characterImage.color = new Color(startColor.r, startColor.g, startColor.b, 1 - t);

                    // 缩小
                    if (characterRoot != null)
                    {
                        characterRoot.localScale = startScale * (1 - t * 0.5f);
                    }

                    yield return null;
                }

                SetVisible(false);
            }

            isAnimating = false;
        }

        #endregion
    }
}

