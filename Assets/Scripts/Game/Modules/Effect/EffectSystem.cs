using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 特效系统
    /// </summary>
    public class EffectSystem : MonoBehaviour
    {
        [Header("全屏特效设置")]
        [Tooltip("全屏闪烁特效 SpriteRenderer")]
        [SerializeField]
        private SpriteRenderer allScreenRad;

        [Tooltip("全屏特效图层名称")]
        [SerializeField]
        private string allScreenEffectLayer = "AllScreenEffectActor";

        [Header("闪烁设置")]
        [Tooltip("闪烁频率（每秒闪烁次数）")]
        [SerializeField]
        private float flashFrequency = 10f;

        [Header("测试设置")]
        [Tooltip("测试用SpriteRenderer列表")]
        [SerializeField]
        private List<SpriteRenderer> testSpriteRenderers = new List<SpriteRenderer>();

        [Tooltip("测试持续时间（秒）")]
        [SerializeField]
        private float testDuration = 3f;

        [Tooltip("是否启用测试（按R键触发）")]
        [SerializeField]
        private bool enableTest = true;

        /// <summary>
        /// 当前特效协程
        /// </summary>
        private Coroutine allScreenEffectCoroutine;

        /// <summary>
        /// 是否正在播放全屏特效
        /// </summary>
        private bool isPlayingAllScreenEffect = false;

        /// <summary>
        /// 缓存的原始数据
        /// </summary>
        private class SpriteRendererCache
        {
            public SpriteRenderer renderer;
            public Color originalColor;
            public int originalSortingLayerID;
            public int originalSortingOrder;
        }

        /// <summary>
        /// 是否正在播放全屏特效
        /// </summary>
        public bool IsPlayingAllScreenEffect => isPlayingAllScreenEffect;

        #region Events

        /// <summary>
        /// 全屏特效开始事件
        /// </summary>
        public event Action OnAllScreenEffectStart;

        /// <summary>
        /// 全屏特效结束事件
        /// </summary>
        public event Action OnAllScreenEffectEnd;

        #endregion

        /// <summary>
        /// 播放全屏特效
        /// </summary>
        /// <param name="spriteRenderers">要变黑的SpriteRenderer数组</param>
        /// <param name="duration">持续时间（秒）</param>
        public void PlayAllScreenEffect(SpriteRenderer[] spriteRenderers, float duration)
        {
            if (spriteRenderers == null || spriteRenderers.Length == 0)
            {
                Debug.LogWarning("[EffectSystem] SpriteRenderer数组为空");
                return;
            }

            if (duration <= 0f)
            {
                Debug.LogWarning("[EffectSystem] 持续时间必须大于0");
                return;
            }

            // 如果正在播放，先停止
            if (isPlayingAllScreenEffect)
            {
                StopAllScreenEffect();
            }

            allScreenEffectCoroutine = StartCoroutine(AllScreenEffectCoroutine(spriteRenderers, duration));
        }

        /// <summary>
        /// 停止全屏特效
        /// </summary>
        public void StopAllScreenEffect()
        {
            if (allScreenEffectCoroutine != null)
            {
                StopCoroutine(allScreenEffectCoroutine);
                allScreenEffectCoroutine = null;
            }

            // 关闭闪烁特效
            if (allScreenRad != null)
            {
                allScreenRad.gameObject.SetActive(false);
            }

            isPlayingAllScreenEffect = false;
        }

        /// <summary>
        /// 全屏特效协程
        /// </summary>
        private IEnumerator AllScreenEffectCoroutine(SpriteRenderer[] spriteRenderers, float duration)
        {
            isPlayingAllScreenEffect = true;
            OnAllScreenEffectStart?.Invoke();

            // 获取目标图层ID
            int targetSortingLayerID = SortingLayer.NameToID(allScreenEffectLayer);

            // 缓存原始数据并设置为黑色
            List<SpriteRendererCache> cacheList = new List<SpriteRendererCache>();

            foreach (var sr in spriteRenderers)
            {
                if (sr == null) continue;

                // 缓存原始数据
                SpriteRendererCache cache = new SpriteRendererCache
                {
                    renderer = sr,
                    originalColor = sr.color,
                    originalSortingLayerID = sr.sortingLayerID,
                    originalSortingOrder = sr.sortingOrder
                };
                cacheList.Add(cache);

                // 设置为纯黑色
                sr.color = Color.black;

                // 设置图层
                sr.sortingLayerID = targetSortingLayerID;
            }

            // 打开闪烁特效
            if (allScreenRad != null)
            {
                allScreenRad.gameObject.SetActive(true);
            }

            // 开始闪烁
            float elapsed = 0f;
            float flashInterval = 1f / flashFrequency;
            bool isRed = true;

            while (elapsed < duration)
            {
                // 红白闪烁
                if (allScreenRad != null)
                {
                    allScreenRad.color = isRed ? Color.red : Color.white;
                    isRed = !isRed;
                }

                yield return new WaitForSeconds(flashInterval);
                elapsed += flashInterval;
            }

            // 还原所有SpriteRenderer
            foreach (var cache in cacheList)
            {
                if (cache.renderer == null) continue;

                cache.renderer.color = cache.originalColor;
                cache.renderer.sortingLayerID = cache.originalSortingLayerID;
                cache.renderer.sortingOrder = cache.originalSortingOrder;
            }

            // 关闭闪烁特效
            if (allScreenRad != null)
            {
                allScreenRad.gameObject.SetActive(false);
            }

            isPlayingAllScreenEffect = false;
            allScreenEffectCoroutine = null;

            OnAllScreenEffectEnd?.Invoke();
        }

        void OnDestroy()
        {
            StopAllScreenEffect();
        }

        void Update()
        {
            // 按R键触发测试
            if (enableTest && Input.GetKeyDown(KeyCode.R))
            {
                TestAllScreenEffect();
            }
        }

        /// <summary>
        /// 测试全屏特效
        /// </summary>
        public void TestAllScreenEffect()
        {
            if (testSpriteRenderers == null || testSpriteRenderers.Count == 0)
            {
                Debug.LogWarning("[EffectSystem] 测试列表为空，请在Inspector中添加SpriteRenderer");
                return;
            }

            Debug.Log($"[EffectSystem] 测试全屏特效，持续 {testDuration} 秒");
            PlayAllScreenEffect(testSpriteRenderers.ToArray(), testDuration);
        }
    }
}
