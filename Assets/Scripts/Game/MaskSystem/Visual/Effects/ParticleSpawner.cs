// ================================================
// MaskSystem Visual - 粒子特效生成器
// ================================================

using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 粒子特效生成器 - 管理和复用粒子特效
    /// </summary>
    public class ParticleSpawner : MonoBehaviour
    {
        #region 特效预制体

        [Header("攻击特效")]
        [SerializeField] private GameObject slashEffect;
        [SerializeField] private GameObject impactEffect;
        [SerializeField] private GameObject criticalEffect;

        [Header("状态特效")]
        [SerializeField] private GameObject healEffect;
        [SerializeField] private GameObject buffEffect;
        [SerializeField] private GameObject debuffEffect;

        [Header("环境特效")]
        [SerializeField] private GameObject dustEffect;
        [SerializeField] private GameObject sparkEffect;

        [Header("对象池设置")]
        [SerializeField] private int poolSize = 10;

        #endregion

        #region 私有字段

        private Dictionary<GameObject, Queue<GameObject>> _pools = new Dictionary<GameObject, Queue<GameObject>>();
        private Transform _poolContainer;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 创建对象池容器
            _poolContainer = new GameObject("ParticlePool").transform;
            _poolContainer.SetParent(transform);

            // 预热对象池
            PrewarmPool(slashEffect);
            PrewarmPool(impactEffect);
            PrewarmPool(healEffect);
        }

        #endregion

        #region 对象池

        private void PrewarmPool(GameObject prefab)
        {
            if (prefab == null) return;

            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new Queue<GameObject>();
            }

            for (int i = 0; i < poolSize; i++)
            {
                var obj = Instantiate(prefab, _poolContainer);
                obj.SetActive(false);
                _pools[prefab].Enqueue(obj);
            }
        }

        private GameObject GetFromPool(GameObject prefab)
        {
            if (prefab == null) return null;

            if (!_pools.ContainsKey(prefab))
            {
                _pools[prefab] = new Queue<GameObject>();
            }

            GameObject obj;
            if (_pools[prefab].Count > 0)
            {
                obj = _pools[prefab].Dequeue();
            }
            else
            {
                obj = Instantiate(prefab, _poolContainer);
            }

            obj.SetActive(true);
            return obj;
        }

        private void ReturnToPool(GameObject prefab, GameObject obj, float delay)
        {
            StartCoroutine(ReturnToPoolDelayed(prefab, obj, delay));
        }

        private System.Collections.IEnumerator ReturnToPoolDelayed(GameObject prefab, GameObject obj, float delay)
        {
            yield return new WaitForSeconds(delay);

            if (obj != null)
            {
                obj.SetActive(false);
                obj.transform.SetParent(_poolContainer);

                if (_pools.ContainsKey(prefab))
                {
                    _pools[prefab].Enqueue(obj);
                }
            }
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 生成斩击特效
        /// </summary>
        public void SpawnSlash(Vector3 position, Quaternion rotation = default)
        {
            SpawnEffect(slashEffect, position, rotation, 0.5f);
        }

        /// <summary>
        /// 生成冲击特效
        /// </summary>
        public void SpawnImpact(Vector3 position)
        {
            SpawnEffect(impactEffect, position, Quaternion.identity, 0.5f);
        }

        /// <summary>
        /// 生成暴击特效
        /// </summary>
        public void SpawnCritical(Vector3 position)
        {
            SpawnEffect(criticalEffect, position, Quaternion.identity, 1f);
        }

        /// <summary>
        /// 生成治疗特效
        /// </summary>
        public void SpawnHeal(Vector3 position)
        {
            SpawnEffect(healEffect, position, Quaternion.identity, 1f);
        }

        /// <summary>
        /// 生成增益特效
        /// </summary>
        public void SpawnBuff(Vector3 position)
        {
            SpawnEffect(buffEffect, position, Quaternion.identity, 1f);
        }

        /// <summary>
        /// 生成灰尘特效
        /// </summary>
        public void SpawnDust(Vector3 position)
        {
            SpawnEffect(dustEffect, position, Quaternion.identity, 0.5f);
        }

        /// <summary>
        /// 生成火花特效
        /// </summary>
        public void SpawnSpark(Vector3 position)
        {
            SpawnEffect(sparkEffect, position, Quaternion.identity, 0.3f);
        }

        /// <summary>
        /// 生成自定义特效
        /// </summary>
        public void SpawnEffect(GameObject prefab, Vector3 position, Quaternion rotation, float duration)
        {
            if (prefab == null) return;

            var obj = GetFromPool(prefab);
            if (obj != null)
            {
                obj.transform.position = position;
                obj.transform.rotation = rotation;

                // 重新播放粒子系统
                var particles = obj.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in particles)
                {
                    ps.Clear();
                    ps.Play();
                }

                ReturnToPool(prefab, obj, duration);
            }
        }

        /// <summary>
        /// 在两点之间生成特效
        /// </summary>
        public void SpawnEffectBetween(GameObject prefab, Vector3 from, Vector3 to, float duration)
        {
            if (prefab == null) return;

            Vector3 position = (from + to) / 2f;
            Vector3 direction = to - from;
            Quaternion rotation = Quaternion.LookRotation(Vector3.forward, direction);

            SpawnEffect(prefab, position, rotation, duration);
        }

        #endregion

        #region 组合效果

        /// <summary>
        /// 攻击命中效果组合
        /// </summary>
        public void PlayHitCombo(Vector3 position, bool isCritical = false)
        {
            SpawnImpact(position);
            SpawnSpark(position + Random.insideUnitSphere * 0.5f);

            if (isCritical)
            {
                SpawnCritical(position);
            }
        }

        /// <summary>
        /// 死亡效果组合
        /// </summary>
        public void PlayDeathCombo(Vector3 position)
        {
            SpawnImpact(position);
            SpawnDust(position);

            // 多个火花
            for (int i = 0; i < 3; i++)
            {
                SpawnSpark(position + Random.insideUnitSphere * 1f);
            }
        }

        #endregion
    }
}

