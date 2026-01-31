using System;
using System.Collections;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 节奏系统 - 根据节奏设置驱动生成节奏
    /// </summary>
    public class RhythmSystem : MonoBehaviour
    {
        [Header("引用")]
        [Tooltip("用于生成Prefab的ActorLauncher")]
        [SerializeField]
        private ActorLauncher actorLauncher;

        [Tooltip("节奏数据配置")]
        [SerializeField]
        private RhythmData rhythmData;

        [Header("节奏设置")]
        [Tooltip("每秒生成次数")]
        [SerializeField]
        [Min(0.1f)]
        private float beatsPerSecond = 1f;

        [Header("移动设置")]
        [Tooltip("向左移动速度")]
        [SerializeField]
        private float moveSpeed = 5f;

        [Header("自动开始")]
        [Tooltip("是否在Start时自动开始")]
        [SerializeField]
        private bool autoStart = true;

        /// <summary>
        /// 节奏协程
        /// </summary>
        private Coroutine rhythmCoroutine;

        /// <summary>
        /// 是否正在运行
        /// </summary>
        private bool isRunning = false;

        #region Properties

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning => isRunning;

        /// <summary>
        /// 每秒生成次数
        /// </summary>
        public float BeatsPerSecond
        {
            get => beatsPerSecond;
            set => beatsPerSecond = Mathf.Max(0.1f, value);
        }

        /// <summary>
        /// 移动速度
        /// </summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// 节拍事件（生成的Rhythm组件）
        /// </summary>
        public event Action<Rhythm> OnBeat;

        #endregion

        void Start()
        {
            if (autoStart)
            {
                StartRhythm();
            }
        }

        /// <summary>
        /// 开始生成节奏
        /// </summary>
        public void StartRhythm()
        {
            if (actorLauncher == null)
            {
                Debug.LogError("[RhythmSystem] ActorLauncher未设置");
                return;
            }

            if (rhythmData == null || rhythmData.Count == 0)
            {
                Debug.LogError("[RhythmSystem] RhythmData未设置或为空");
                return;
            }

            StopRhythm();

            isRunning = true;
            rhythmCoroutine = StartCoroutine(RhythmCoroutine());
            Debug.Log($"[RhythmSystem] 开始生成节奏，每秒 {beatsPerSecond} 次");
        }

        /// <summary>
        /// 停止生成节奏
        /// </summary>
        public void StopRhythm()
        {
            if (rhythmCoroutine != null)
            {
                StopCoroutine(rhythmCoroutine);
                rhythmCoroutine = null;
            }

            isRunning = false;
        }

        /// <summary>
        /// 节奏生成协程
        /// </summary>
        private IEnumerator RhythmCoroutine()
        {
            float interval = 1f / beatsPerSecond;

            while (isRunning)
            {
                SpawnRandomRhythm();
                yield return new WaitForSeconds(interval);
            }
        }

        /// <summary>
        /// 随机生成一个节奏
        /// </summary>
        private void SpawnRandomRhythm()
        {
            // 从RhythmData随机获取一个配置
            int randomIndex = UnityEngine.Random.Range(0, rhythmData.Count);
            RhythmConfig config = rhythmData.GetConfig(randomIndex);

            if (config == null || config.prefab == null) return;

            // 使用ActorLauncher生成Prefab
            actorLauncher.SetPrefabAndRespawn(config.prefab);

            if (actorLauncher.SpawnedInstance == null) return;

            // 获取或添加Rhythm组件
            Rhythm rhythm = actorLauncher.SpawnedInstance.GetComponent<Rhythm>();
            if (rhythm == null)
            {
                rhythm = actorLauncher.SpawnedInstance.AddComponent<Rhythm>();
            }

            // 初始化
            rhythm.Initialize(moveSpeed, config.actionType);

            // 触发事件
            OnBeat?.Invoke(rhythm);
        }

        void OnDestroy()
        {
            StopRhythm();
        }
    }
}
