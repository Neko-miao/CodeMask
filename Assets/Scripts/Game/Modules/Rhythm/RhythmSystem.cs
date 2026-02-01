using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Events;

namespace Game
{
    public enum RhythmGenerationMode { Random, Sequence }

    /// <summary>
    /// 节奏系统 - 生成和管理节奏
    /// 使用纯距离计算，不依赖物理组件
    /// </summary>
    public class RhythmSystem : MonoBehaviour
    {
        public static RhythmSystem Instance { get; private set; }

        [Header("引用")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform endPoint;
        [SerializeField] private RhythmData rhythmData;

        [Header("序列配置")]
        [SerializeField] private RhythmSequenceConfig sequenceConfig;
        [SerializeField] private RhythmGenerationMode generationMode = RhythmGenerationMode.Sequence;

        [Header("设置")]
        [SerializeField, Min(0.1f)] private float beatsPerSecond = 1f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private bool autoStart = true;

        private Coroutine rhythmCoroutine;
        private int currentSequenceIndex;
        private MaskType currentMaskType = MaskType.None;
        private RhythmSequenceEntry currentSequenceEntry;
        private List<Rhythm> activeRhythms = new List<Rhythm>();

        public bool IsRunning { get; private set; }
        public float BeatsPerSecond { get => beatsPerSecond; set => beatsPerSecond = Mathf.Max(0.1f, value); }
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = Mathf.Max(0f, value); }

        public event Action<Rhythm> OnBeat;
        public event Action OnSequenceComplete;
        public event Action<Rhythm> OnRhythmMiss;

        /// <summary>
        /// 节奏创建事件（静态事件，供其他系统订阅）
        /// </summary>
        public static event Action<RhythmCreatedEvent> OnRhythmCreated;

        void Awake()
        {
            if (Instance == null) Instance = this;
            else if (Instance != this) { Destroy(gameObject); return; }
        }

        void Start()
        {
            if (autoStart) StartRhythm();
        }

        void Update()
        {
            CheckRhythmsAtEndPoint();
        }

        void OnDestroy()
        {
            StopRhythm();
            ClearAllActiveRhythms();
            if (Instance == this) Instance = null;
        }

        /// <summary>
        /// 获取所有活动的节奏（只读）
        /// </summary>
        public IReadOnlyList<Rhythm> GetActiveRhythms()
        {
            return activeRhythms;
        }

        /// <summary>
        /// 检查节奏是否超过判定区域（到达第二个Miss边界即左边界）
        /// </summary>
        private void CheckRhythmsAtEndPoint()
        {
            // 优先使用判定区域的左边界，如果没有则使用endPoint
            float missLeftX;
            if (RhythmTriggerZone.Instance != null)
            {
                missLeftX = RhythmTriggerZone.Instance.ZoneLeftX;
            }
            else if (endPoint != null)
            {
                missLeftX = endPoint.position.x;
            }
            else
            {
                return;
            }

            for (int i = activeRhythms.Count - 1; i >= 0; i--)
            {
                var rhythm = activeRhythms[i];
                if (rhythm == null) { activeRhythms.RemoveAt(i); continue; }

                // 节奏超过判定区域左边界（第二个Miss边界）
                if (rhythm.PositionX <= missLeftX)
                {
                    Debug.Log($"<color=red>[RhythmSystem] {rhythm.name} 超过判定区域，Miss!</color>");
                    OnRhythmMiss?.Invoke(rhythm);
                    RhythmTriggerZone.Instance?.OnRhythmMissed(rhythm);
                    activeRhythms.RemoveAt(i);
                    Destroy(rhythm.gameObject);
                }
            }
        }

        public void StartRhythm()
        {
            if (spawnPoint == null || endPoint == null)
            {
                Debug.LogError("[RhythmSystem] SpawnPoint或EndPoint未设置");
                return;
            }

            if (generationMode == RhythmGenerationMode.Random && (rhythmData == null || rhythmData.Count == 0))
            {
                Debug.LogError("[RhythmSystem] 随机模式：RhythmData未设置或为空");
                return;
            }

            if (generationMode == RhythmGenerationMode.Sequence && (sequenceConfig == null || !sequenceConfig.IsValid()))
            {
                Debug.LogError("[RhythmSystem] 序列模式：RhythmSequenceConfig未设置或无效");
                return;
            }

            StopRhythm();
            IsRunning = true;
            currentSequenceIndex = 0;

            if (currentMaskType != MaskType.None && sequenceConfig != null)
                currentSequenceEntry = sequenceConfig.GetEntryByMaskType(currentMaskType);

            rhythmCoroutine = StartCoroutine(RhythmCoroutine());
        }

        public void StopRhythm()
        {
            if (rhythmCoroutine != null)
            {
                StopCoroutine(rhythmCoroutine);
                rhythmCoroutine = null;
            }
            IsRunning = false;
        }

        public void SetMaskTypeAndStart(MaskType maskType)
        {
            currentMaskType = maskType;
            currentSequenceEntry = null;
            currentSequenceIndex = 0;
            if (maskType != MaskType.None && sequenceConfig != null)
                currentSequenceEntry = sequenceConfig.GetEntryByMaskType(maskType);
            StartRhythm();
        }

        /// <summary>
        /// 从活动列表中移除节奏
        /// </summary>
        public void RemoveFromActiveList(Rhythm rhythm)
        {
            activeRhythms.Remove(rhythm);
        }

        /// <summary>
        /// 从活动列表中移除并销毁节奏
        /// </summary>
        public void RemoveAndDestroyRhythm(Rhythm rhythm)
        {
            if (rhythm == null) return;
            activeRhythms.Remove(rhythm);
            Destroy(rhythm.gameObject);
        }

        public void ClearAllActiveRhythms()
        {
            foreach (var rhythm in activeRhythms)
                if (rhythm != null) Destroy(rhythm.gameObject);
            activeRhythms.Clear();
        }

        private IEnumerator RhythmCoroutine()
        {
            var wait = new WaitForSeconds(1f / beatsPerSecond);

            while (IsRunning)
            {
                bool spawned = generationMode == RhythmGenerationMode.Random 
                    ? SpawnRhythm(GetRandomConfig()) 
                    : SpawnRhythm(GetSequenceConfig());

                if (!spawned && generationMode == RhythmGenerationMode.Sequence && !sequenceConfig.Loop)
                {
                    OnSequenceComplete?.Invoke();
                    StopRhythm();
                    yield break;
                }

                yield return wait;
            }
        }

        private (RhythmConfig config, MaskType mask)? GetRandomConfig()
        {
            var config = rhythmData.GetConfig(UnityEngine.Random.Range(0, rhythmData.Count));
            return config?.prefab != null ? (config, MaskType.None) : null;
        }

        private (RhythmConfig config, MaskType mask)? GetSequenceConfig()
        {
            MaskType maskType;
            RhythmActionType actionType;

            if (currentSequenceEntry != null)
            {
                if (currentSequenceIndex >= currentSequenceEntry.ActionCount)
                {
                    if (sequenceConfig.Loop) currentSequenceIndex = 0;
                    else return null;
                }
                maskType = currentMaskType;
                actionType = currentSequenceEntry.GetAction(currentSequenceIndex);
            }
            else
            {
                if (!sequenceConfig.GetRhythmByGlobalIndex(currentSequenceIndex, out maskType, out actionType))
                    return null;
            }

            var config = rhythmData?.GetConfigByActionType(actionType);
            if (config?.prefab == null)
            {
                currentSequenceIndex++;
                return null;
            }

            currentSequenceIndex++;
            return (config, maskType);
        }

        private bool SpawnRhythm((RhythmConfig config, MaskType mask)? data)
        {
            if (data == null) return false;

            var (config, mask) = data.Value;
            var instance = Instantiate(config.prefab, spawnPoint.position, Quaternion.identity);
            if (instance == null) return false;

            var rhythm = instance.GetComponent<Rhythm>() ?? instance.AddComponent<Rhythm>();
            rhythm.Initialize(moveSpeed, config.actionType, mask);
            activeRhythms.Add(rhythm);
            OnBeat?.Invoke(rhythm);

            // 计算到达判定区域的时间并发布事件
            // 计算到达判定区域和完美区间的时间并发布事件
            float timeToReachZone = CalculateTimeToReachJudgmentZone();
            float timeToReachPerfect = CalculateTimeToReachPerfectZone();
            PublishRhythmCreatedEvent(rhythm, mask, config.actionType, timeToReachZone, timeToReachPerfect, spawnPoint.position);

            return true;
        }

        /// <summary>
        /// 计算节奏从生成点到达判定区域第一个未命中判定点的时间
        /// </summary>
        private float CalculateTimeToReachJudgmentZone()
        {
            if (spawnPoint == null || RhythmTriggerZone.Instance == null || moveSpeed <= 0f)
            {
                return 0f;
            }

            // 获取生成点X坐标
            float spawnX = spawnPoint.position.x;
            
            // 获取判定区域的右边界（第一个未命中判定点）
            // 节奏是向左移动的，所以需要到达判定区域的右边界
            float judgeRightX = RhythmTriggerZone.Instance.ZoneRightX;
            
            // 计算距离
            float distance = spawnX - judgeRightX;
            
            // 计算时间 = 距离 / 速度
            if (distance <= 0f)
            {
                return 0f;
            }
            
            return distance / moveSpeed;
        }

        /// <summary>
        /// 计算节奏从生成点到达完美判定区间中心的时间
        /// </summary>
        private float CalculateTimeToReachPerfectZone()
        {
            if (spawnPoint == null || RhythmTriggerZone.Instance == null || moveSpeed <= 0f)
            {
                return 0f;
            }

            // 获取生成点X坐标
            float spawnX = spawnPoint.position.x;
            
            // 获取完美区间中心X坐标
            float perfectCenterX = RhythmTriggerZone.Instance.PerfectZoneCenterX;
            
            // 计算距离
            float distance = spawnX - perfectCenterX;
            
            // 计算时间 = 距离 / 速度
            if (distance <= 0f)
            {
                return 0f;
            }
            
            return distance / moveSpeed;
        }

        /// <summary>
        /// 发布节奏创建事件
        /// </summary>
        private void PublishRhythmCreatedEvent(Rhythm rhythm, MaskType maskType, RhythmActionType actionType, float timeToReachZone, float timeToReachPerfect, Vector3 spawnPos)
        {
            var evt = new RhythmCreatedEvent(rhythm, maskType, actionType, timeToReachZone, timeToReachPerfect, spawnPos);
            OnRhythmCreated?.Invoke(evt);
            Debug.Log($"[RhythmSystem] 已发布 RhythmCreatedEvent: ActionType={actionType}, TimeToReachZone={timeToReachZone:F2}s, TimeToReachPerfect={timeToReachPerfect:F2}s");
        }
    }
}
