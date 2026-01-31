using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 特效系统 - 单例模式
    /// </summary>
    public class EffectSystem : MonoBehaviour, IEffectSystem
    {
        #region Singleton

        private static EffectSystem instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static EffectSystem Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EffectSystem>();

                    if (instance == null)
                    {
                        GameObject go = new GameObject("EffectSystem");
                        instance = go.AddComponent<EffectSystem>();
                    }
                }
                return instance;
            }
        }

        /// <summary>
        /// 是否存在实例
        /// </summary>
        public static bool HasInstance => instance != null;

        #endregion

        public GameObject attackEff;

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

        [Header("震动设置")]
        [Tooltip("震动强度")]
        [SerializeField]
        private float shakeIntensity = 0.1f;

        [Tooltip("震动频率（每秒震动次数）")]
        [SerializeField]
        private float shakeFrequency = 30f;

        [Header("攻击特效设置")]
        [Tooltip("攻击特效移动时间（秒）")]
        [SerializeField]
        private float attackEffectMoveTime = 0.1f;

        [Tooltip("攻击特效存活时间（秒）")]
        [SerializeField]
        private float attackEffectLifeTime = 0.25f;

        [Header("测试设置")]
        [Tooltip("测试用SpriteRenderer列表")]
        [SerializeField]
        private List<SpriteRenderer> testSpriteRenderers = new List<SpriteRenderer>();

        [Tooltip("测试用不参与时停的GameObject列表")]
        [SerializeField]
        private List<GameObject> testExcludedObjects = new List<GameObject>();

        [Tooltip("测试持续时间（秒）")]
        [SerializeField]
        private float testDuration = 3f;

        [Tooltip("是否启用测试")]
        [SerializeField]
        private bool enableTest = true;

        [Header("攻击特效测试")]
        [Tooltip("攻击特效测试起始点")]
        [SerializeField]
        private Transform testAttackStartPoint;

        [Tooltip("攻击特效测试目标点")]
        [SerializeField]
        private Transform testAttackTargetPoint;

        /// <summary>
        /// 当前特效协程
        /// </summary>
        private Coroutine allScreenEffectCoroutine;

        /// <summary>
        /// 震动特效协程
        /// </summary>
        private Coroutine shakeEffectCoroutine;

        /// <summary>
        /// 时停特效协程
        /// </summary>
        private Coroutine timeStopCoroutine;

        /// <summary>
        /// 是否正在播放全屏特效
        /// </summary>
        private bool isPlayingAllScreenEffect = false;

        /// <summary>
        /// 是否正在播放震动特效
        /// </summary>
        private bool isPlayingShakeEffect = false;

        /// <summary>
        /// 是否正在时停
        /// </summary>
        private bool isTimeStopped = false;

        /// <summary>
        /// 时停前的时间缩放
        /// </summary>
        private float originalTimeScale = 1f;

        /// <summary>
        /// 不参与时停的物体及其原始更新模式
        /// </summary>
        private Dictionary<Animator, AnimatorUpdateMode> excludedAnimators = new Dictionary<Animator, AnimatorUpdateMode>();

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
        /// 缓存的位置数据
        /// </summary>
        private class PositionCache
        {
            public Transform transform;
            public Vector3 originalPosition;
        }

        #region IEffectSystem Properties

        /// <summary>
        /// 是否正在播放全屏特效
        /// </summary>
        public bool IsPlayingAllScreenEffect => isPlayingAllScreenEffect;

        /// <summary>
        /// 是否正在播放震动特效
        /// </summary>
        public bool IsPlayingShakeEffect => isPlayingShakeEffect;

        /// <summary>
        /// 是否正在时停
        /// </summary>
        public bool IsTimeStopped => isTimeStopped;

        #endregion

        #region IEffectSystem Events

        /// <summary>
        /// 全屏特效开始事件
        /// </summary>
        public event Action OnAllScreenEffectStart;

        /// <summary>
        /// 全屏特效结束事件
        /// </summary>
        public event Action OnAllScreenEffectEnd;

        /// <summary>
        /// 震动特效开始事件
        /// </summary>
        public event Action OnShakeEffectStart;

        /// <summary>
        /// 震动特效结束事件
        /// </summary>
        public event Action OnShakeEffectEnd;

        /// <summary>
        /// 时停开始事件
        /// </summary>
        public event Action OnTimeStopStart;

        /// <summary>
        /// 时停结束事件
        /// </summary>
        public event Action OnTimeStopEnd;

        #endregion

        #region Unity Lifecycle

        void Awake()
        {
            // 单例初始化
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogWarning("[EffectSystem] 已存在实例，销毁重复的EffectSystem");
                Destroy(gameObject);
                return;
            }
        }

        void Update()
        {
            // 按R键触发全屏特效测试
            if (enableTest && Input.GetKeyDown(KeyCode.R))
            {
                TestAllScreenEffect();
            }

            // 按T键触发震动特效测试
            if (enableTest && Input.GetKeyDown(KeyCode.T))
            {
                TestShakeEffect();
            }

            // 按Y键触发时停特效测试
            if (enableTest && Input.GetKeyDown(KeyCode.Y))
            {
                TestTimeStop();
            }

            // 按S键触发攻击特效测试
            if (enableTest && Input.GetKeyDown(KeyCode.S))
            {
                TestAttackEffect();
            }
        }

        void OnDestroy()
        {
            StopAllScreenEffect();
            StopShakeEffect();
            StopTimeStop();

            if (instance == this)
            {
                instance = null;
            }
        }

        #endregion

        #region All Screen Effect

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

        #endregion

        #region Shake Effect

        /// <summary>
        /// 播放震动特效
        /// </summary>
        /// <param name="spriteRenderers">要震动的SpriteRenderer数组</param>
        /// <param name="duration">持续时间（秒）</param>
        public void PlayShakeEffect(SpriteRenderer[] spriteRenderers, float duration)
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
            if (isPlayingShakeEffect)
            {
                StopShakeEffect();
            }

            shakeEffectCoroutine = StartCoroutine(ShakeEffectCoroutine(spriteRenderers, duration));
        }

        /// <summary>
        /// 停止震动特效
        /// </summary>
        public void StopShakeEffect()
        {
            if (shakeEffectCoroutine != null)
            {
                StopCoroutine(shakeEffectCoroutine);
                shakeEffectCoroutine = null;
            }

            isPlayingShakeEffect = false;
        }

        /// <summary>
        /// 震动特效协程
        /// </summary>
        private IEnumerator ShakeEffectCoroutine(SpriteRenderer[] spriteRenderers, float duration)
        {
            isPlayingShakeEffect = true;
            OnShakeEffectStart?.Invoke();

            // 缓存原始位置
            List<PositionCache> cacheList = new List<PositionCache>();

            foreach (var sr in spriteRenderers)
            {
                if (sr == null) continue;

                PositionCache cache = new PositionCache
                {
                    transform = sr.transform,
                    originalPosition = sr.transform.localPosition
                };
                cacheList.Add(cache);
            }

            // 开始震动
            float elapsed = 0f;
            float shakeInterval = 1f / shakeFrequency;

            while (elapsed < duration)
            {
                // 计算震动衰减（越接近结束震动越小）
                float remainingRatio = 1f - (elapsed / duration);
                float currentIntensity = shakeIntensity * remainingRatio;

                // 对每个物体应用随机偏移
                foreach (var cache in cacheList)
                {
                    if (cache.transform == null) continue;

                    Vector3 randomOffset = new Vector3(
                        UnityEngine.Random.Range(-currentIntensity, currentIntensity),
                        UnityEngine.Random.Range(-currentIntensity, currentIntensity),
                        0f
                    );

                    cache.transform.localPosition = cache.originalPosition + randomOffset;
                }

                yield return new WaitForSeconds(shakeInterval);
                elapsed += shakeInterval;
            }

            // 还原所有位置
            foreach (var cache in cacheList)
            {
                if (cache.transform == null) continue;
                cache.transform.localPosition = cache.originalPosition;
            }

            isPlayingShakeEffect = false;
            shakeEffectCoroutine = null;

            OnShakeEffectEnd?.Invoke();
        }

        #endregion

        #region Time Stop Effect

        /// <summary>
        /// 播放时停效果
        /// </summary>
        /// <param name="duration">时停持续时间（秒，真实时间）</param>
        /// <param name="excludedObjects">不参与时停的GameObject数组</param>
        public void PlayTimeStop(float duration, GameObject[] excludedObjects = null)
        {
            if (duration <= 0f)
            {
                Debug.LogWarning("[EffectSystem] 时停时间必须大于0");
                return;
            }

            // 如果正在时停，先停止
            if (isTimeStopped)
            {
                StopTimeStop();
            }

            timeStopCoroutine = StartCoroutine(TimeStopCoroutine(duration, excludedObjects));
        }

        /// <summary>
        /// 停止时停效果
        /// </summary>
        public void StopTimeStop()
        {
            if (timeStopCoroutine != null)
            {
                StopCoroutine(timeStopCoroutine);
                timeStopCoroutine = null;
            }

            // 恢复时间
            if (isTimeStopped)
            {
                RestoreTimeScale();
            }
        }

        /// <summary>
        /// 时停协程
        /// </summary>
        private IEnumerator TimeStopCoroutine(float duration, GameObject[] excludedObjects)
        {
            isTimeStopped = true;
            OnTimeStopStart?.Invoke();

            // 保存原始时间缩放
            originalTimeScale = Time.timeScale;

            // 设置不参与时停的物体
            SetupExcludedObjects(excludedObjects);

            // 停止时间
            Time.timeScale = 0f;

            Debug.Log($"[EffectSystem] 时停开始，持续 {duration} 秒");

            // 使用真实时间等待
            yield return new WaitForSecondsRealtime(duration);

            // 恢复时间
            RestoreTimeScale();

            Debug.Log("[EffectSystem] 时停结束");
        }

        /// <summary>
        /// 设置不参与时停的物体
        /// </summary>
        private void SetupExcludedObjects(GameObject[] excludedObjects)
        {
            excludedAnimators.Clear();

            if (excludedObjects == null) return;

            foreach (var obj in excludedObjects)
            {
                if (obj == null) continue;

                // 获取所有Animator组件（包括子物体）
                Animator[] animators = obj.GetComponentsInChildren<Animator>(true);
                foreach (var animator in animators)
                {
                    if (animator == null) continue;

                    // 保存原始更新模式
                    excludedAnimators[animator] = animator.updateMode;

                    // 设置为不受时间缩放影响
                    animator.updateMode = AnimatorUpdateMode.UnscaledTime;
                }
            }
        }

        /// <summary>
        /// 恢复时间缩放
        /// </summary>
        private void RestoreTimeScale()
        {
            // 恢复时间
            Time.timeScale = originalTimeScale;

            // 恢复Animator的更新模式
            foreach (var kvp in excludedAnimators)
            {
                if (kvp.Key != null)
                {
                    kvp.Key.updateMode = kvp.Value;
                }
            }
            excludedAnimators.Clear();

            isTimeStopped = false;
            timeStopCoroutine = null;

            OnTimeStopEnd?.Invoke();
        }

        #endregion

        #region Attack Effect

        /// <summary>
        /// 播放攻击特效
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="targetPosition">目标位置</param>
        public void PlayAttackEffect(Vector3 startPosition, Vector3 targetPosition)
        {
            PlayAttackEffect(startPosition, targetPosition, null);
        }

        /// <summary>
        /// 播放攻击特效（带回调）
        /// </summary>
        /// <param name="startPosition">起始位置</param>
        /// <param name="targetPosition">目标位置</param>
        /// <param name="onReachTarget">抵达目标点时的回调</param>
        public void PlayAttackEffect(Vector3 startPosition, Vector3 targetPosition, System.Action onReachTarget)
        {
            if (attackEff == null)
            {
                Debug.LogWarning("[EffectSystem] attackEff预制体未设置");
                return;
            }

            StartCoroutine(AttackEffectCoroutine(startPosition, targetPosition, onReachTarget));
        }

        /// <summary>
        /// 攻击特效协程
        /// </summary>
        private IEnumerator AttackEffectCoroutine(Vector3 startPosition, Vector3 targetPosition, System.Action onReachTarget = null)
        {
            // 在起始点生成特效
            GameObject effectInstance = Instantiate(attackEff, startPosition, Quaternion.identity);

            // 确保特效开启
            effectInstance.SetActive(true);

            // 计算方向并让Y轴朝向目标点
            Vector3 direction = (targetPosition - startPosition).normalized;
            if (direction != Vector3.zero)
            {
                // 计算让Y轴（上方向）朝向目标点的旋转
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
                effectInstance.transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }

            // 0.1s内移动到目标点
            float elapsed = 0f;
            while (elapsed < attackEffectMoveTime)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / attackEffectMoveTime);
                effectInstance.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                yield return null;
            }

            // 确保到达目标点
            effectInstance.transform.position = targetPosition;

            // 触发抵达目标点回调
            onReachTarget?.Invoke();

            // 等待剩余时间后销毁 (0.25s - 0.1s = 0.15s)
            float remainingTime = attackEffectLifeTime - attackEffectMoveTime;
            if (remainingTime > 0f)
            {
                yield return new WaitForSeconds(remainingTime);
            }

            // 销毁特效
            if (effectInstance != null)
            {
                Destroy(effectInstance);
            }
        }

        #endregion

        #region Test Methods

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

        /// <summary>
        /// 测试震动特效
        /// </summary>
        public void TestShakeEffect()
        {
            if (testSpriteRenderers == null || testSpriteRenderers.Count == 0)
            {
                Debug.LogWarning("[EffectSystem] 测试列表为空，请在Inspector中添加SpriteRenderer");
                return;
            }

            Debug.Log($"[EffectSystem] 测试震动特效，持续 {testDuration} 秒");
            PlayShakeEffect(testSpriteRenderers.ToArray(), testDuration);
        }

        /// <summary>
        /// 测试时停特效
        /// </summary>
        public void TestTimeStop()
        {
            Debug.Log($"[EffectSystem] 测试时停特效，持续 {testDuration} 秒");
            PlayTimeStop(testDuration, testExcludedObjects.ToArray());
        }

        /// <summary>
        /// 测试攻击特效
        /// </summary>
        public void TestAttackEffect()
        {
            if (testAttackStartPoint == null || testAttackTargetPoint == null)
            {
                Debug.LogWarning("[EffectSystem] 请在Inspector中设置攻击特效测试的起始点和目标点");
                return;
            }

            Debug.Log($"[EffectSystem] 测试攻击特效，从 {testAttackStartPoint.position} 到 {testAttackTargetPoint.position}");
            PlayAttackEffect(testAttackStartPoint.position, testAttackTargetPoint.position);
        }

        #endregion
    }
}
