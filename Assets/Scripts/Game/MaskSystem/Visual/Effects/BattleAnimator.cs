// ================================================
// MaskSystem Visual - 战斗动画控制器
// ================================================

using System;
using System.Collections;
using UnityEngine;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 战斗动画控制器 - 协调战斗中的各种动画效果
    /// </summary>
    public class BattleAnimator : MonoBehaviour
    {
        #region 组件引用

        [Header("特效组件")]
        [SerializeField] private ScreenShake screenShake;
        [SerializeField] private HitFlash hitFlash;

        [Header("角色视图")]
        [SerializeField] private PlayerView playerView;
        [SerializeField] private EnemyView enemyView;

        [Header("特效预制体")]
        [SerializeField] private GameObject hitEffectPrefab;
        [SerializeField] private GameObject criticalEffectPrefab;
        [SerializeField] private GameObject healEffectPrefab;
        [SerializeField] private GameObject counterEffectPrefab;

        [Header("时间控制")]
        [SerializeField] private float hitStopDuration = 0.05f;

        #endregion

        #region 事件

        public event Action OnAnimationComplete;

        #endregion

        #region 玩家动画

        /// <summary>
        /// 播放玩家攻击动画
        /// </summary>
        public void PlayPlayerAttack(bool isCritical = false)
        {
            StartCoroutine(PlayerAttackSequence(isCritical));
        }

        private IEnumerator PlayerAttackSequence(bool isCritical)
        {
            // 玩家前冲
            playerView?.PlayAttackAnimation(Vector3.right);

            // 等待一点时间
            yield return new WaitForSeconds(0.15f);

            // 敌人受击
            enemyView?.PlayHitAnimation();

            // 生成特效
            if (enemyView != null)
            {
                var effectPrefab = isCritical ? criticalEffectPrefab : hitEffectPrefab;
                if (effectPrefab != null)
                {
                    var effect = Instantiate(effectPrefab, enemyView.transform.position, Quaternion.identity);
                    Destroy(effect, 1f);
                }
            }

            // 屏幕震动
            if (screenShake != null)
            {
                if (isCritical)
                    screenShake.ShakeHeavy();
                else
                    screenShake.ShakeLight();
            }

            // 闪烁
            if (hitFlash != null && isCritical)
            {
                hitFlash.FlashCritical();
            }

            // 顿帧
            if (hitStopDuration > 0)
            {
                yield return StartCoroutine(HitStopCoroutine());
            }

            OnAnimationComplete?.Invoke();
        }

        /// <summary>
        /// 播放玩家受击动画
        /// </summary>
        public void PlayPlayerHit(int damage)
        {
            StartCoroutine(PlayerHitSequence(damage));
        }

        private IEnumerator PlayerHitSequence(int damage)
        {
            // 敌人攻击
            enemyView?.PlayAttackAnimation(Vector3.left);

            yield return new WaitForSeconds(0.15f);

            // 玩家受击
            playerView?.PlayHitAnimation();

            // 生成特效
            if (playerView != null && hitEffectPrefab != null)
            {
                var effect = Instantiate(hitEffectPrefab, playerView.transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }

            // 屏幕效果
            if (screenShake != null)
            {
                screenShake.ShakeMedium();
            }

            if (hitFlash != null)
            {
                hitFlash.FlashDamage();
            }

            // 顿帧
            if (hitStopDuration > 0)
            {
                yield return StartCoroutine(HitStopCoroutine());
            }

            OnAnimationComplete?.Invoke();
        }

        /// <summary>
        /// 播放玩家反击动画
        /// </summary>
        public void PlayPlayerCounter()
        {
            StartCoroutine(PlayerCounterSequence());
        }

        private IEnumerator PlayerCounterSequence()
        {
            // 显示反击特效
            if (counterEffectPrefab != null && playerView != null)
            {
                var effect = Instantiate(counterEffectPrefab, playerView.transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }

            // 短暂延迟
            yield return new WaitForSeconds(0.1f);

            // 玩家快速攻击
            playerView?.PlayAttackAnimation(Vector3.right);

            yield return new WaitForSeconds(0.1f);

            // 敌人被击退
            enemyView?.PlayHitAnimation();

            // 屏幕效果
            if (screenShake != null)
            {
                screenShake.ShakeMedium();
            }

            if (hitFlash != null)
            {
                hitFlash.Flash(Color.yellow, 0.1f);
            }

            OnAnimationComplete?.Invoke();
        }

        #endregion

        #region 敌人动画

        /// <summary>
        /// 播放敌人入场动画
        /// </summary>
        public void PlayEnemyEntrance()
        {
            enemyView?.PlayEntranceAnimation();
        }

        /// <summary>
        /// 播放敌人退场动画
        /// </summary>
        public void PlayEnemyDefeat()
        {
            StartCoroutine(EnemyDefeatSequence());
        }

        private IEnumerator EnemyDefeatSequence()
        {
            enemyView?.PlayDefeatAnimation();

            // 屏幕效果
            if (screenShake != null)
            {
                screenShake.ShakeHeavy();
            }

            if (hitFlash != null)
            {
                hitFlash.Flash(Color.white, 0.3f);
            }

            yield return new WaitForSeconds(0.5f);

            OnAnimationComplete?.Invoke();
        }

        #endregion

        #region 特殊效果

        /// <summary>
        /// 播放治疗效果
        /// </summary>
        public void PlayHealEffect()
        {
            if (playerView != null && healEffectPrefab != null)
            {
                var effect = Instantiate(healEffectPrefab, playerView.transform.position, Quaternion.identity);
                Destroy(effect, 1f);
            }

            if (hitFlash != null)
            {
                hitFlash.FlashHeal();
            }
        }

        /// <summary>
        /// 播放顿帧效果
        /// </summary>
        private IEnumerator HitStopCoroutine()
        {
            Time.timeScale = 0.1f;
            yield return new WaitForSecondsRealtime(hitStopDuration);
            Time.timeScale = 1f;
        }

        #endregion

        #region 通用方法

        /// <summary>
        /// 在指定位置生成特效
        /// </summary>
        public void SpawnEffect(GameObject prefab, Vector3 position, float duration = 1f)
        {
            if (prefab == null) return;

            var effect = Instantiate(prefab, position, Quaternion.identity);
            Destroy(effect, duration);
        }

        /// <summary>
        /// 触发屏幕震动
        /// </summary>
        public void TriggerScreenShake(float duration = 0.3f, float magnitude = 10f)
        {
            screenShake?.Shake(duration, magnitude);
        }

        /// <summary>
        /// 触发屏幕闪烁
        /// </summary>
        public void TriggerFlash(Color color, float duration = 0.2f)
        {
            hitFlash?.Flash(color, duration);
        }

        #endregion
    }
}

