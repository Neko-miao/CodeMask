using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// Actor启动器，用于在游戏开始时实例化引用的Prefab
    /// </summary>
    public class ActorLauncher : MonoBehaviour
    {
        [Header("Prefab设置")]
        [Tooltip("要实例化的Prefab")]
        [SerializeField]
        private GameObject prefab;

        [Header("生成位置设置")]
        [Tooltip("是否使用自定义生成位置")]
        [SerializeField]
        private bool useCustomSpawnPosition = false;

        [Tooltip("自定义生成位置")]
        [SerializeField]
        private Vector3 spawnPosition = Vector3.zero;

        [Tooltip("自定义生成旋转")]
        [SerializeField]
        private Vector3 spawnRotation = Vector3.zero;

        [Header("父物体设置")]
        [Tooltip("是否将生成的物体设为当前物体的子物体")]
        [SerializeField]
        private bool setAsChild = false;

        /// <summary>
        /// 已实例化的Actor实例
        /// </summary>
        private GameObject spawnedInstance;

        /// <summary>
        /// 获取已生成的实例
        /// </summary>
        public GameObject SpawnedInstance => spawnedInstance;

        void Start()
        {
            SpawnActor();
        }

        /// <summary>
        /// 生成Actor
        /// </summary>
        private void SpawnActor()
        {
            if (prefab == null)
            {
                Debug.LogWarning($"[ActorLauncher] {gameObject.name}: Prefab未设置，无法生成Actor");
                return;
            }

            // 确定生成位置和旋转
            Vector3 position = useCustomSpawnPosition ? spawnPosition : transform.position;
            Quaternion rotation = useCustomSpawnPosition ? Quaternion.Euler(spawnRotation) : transform.rotation;

            // 确定父物体
            Transform parent = setAsChild ? transform : null;

            // 实例化Prefab
            spawnedInstance = Instantiate(prefab, position, rotation, parent);

            if (spawnedInstance != null)
            {
                Debug.Log($"[ActorLauncher] {gameObject.name}: 成功生成Actor '{spawnedInstance.name}'");
            }
        }

        /// <summary>
        /// 销毁已生成的Actor
        /// </summary>
        public void DestroySpawnedActor()
        {
            if (spawnedInstance != null)
            {
                Destroy(spawnedInstance);
                spawnedInstance = null;
                Debug.Log($"[ActorLauncher] {gameObject.name}: Actor已销毁");
            }
        }

        /// <summary>
        /// 重新生成Actor（先销毁旧的，再生成新的）
        /// </summary>
        public void RespawnActor()
        {
            DestroySpawnedActor();
            SpawnActor();
        }

        void OnDestroy()
        {
            // 如果设置为子物体，销毁时不需要手动清理，会随父物体一起销毁
            // 如果不是子物体，可以选择是否销毁生成的实例
            if (!setAsChild && spawnedInstance != null)
            {
                DestroySpawnedActor();
            }
        }
    }
}
