using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        /// 检查节奏是否到达终点（纯距离计算）
        /// </summary>
        private void CheckRhythmsAtEndPoint()
        {
            if (endPoint == null) return;
            float endX = endPoint.position.x;

            for (int i = activeRhythms.Count - 1; i >= 0; i--)
            {
                var rhythm = activeRhythms[i];
                if (rhythm == null) { activeRhythms.RemoveAt(i); continue; }

                if (rhythm.PositionX <= endX)
                {
                    Debug.Log($"<color=red>[RhythmSystem] {rhythm.name} Miss</color>");
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

            return true;
        }
    }
}
