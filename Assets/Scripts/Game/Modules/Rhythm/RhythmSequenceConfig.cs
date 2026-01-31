using System;
using System.Collections.Generic;
using UnityEngine;
namespace Game
{
    /// <summary>
    /// 节奏序列条目 - 包含面具类型和对应的行为序列
    /// </summary>
    [Serializable]
    public class RhythmSequenceEntry
    {
        [Tooltip("面具类型")]
        public MaskType maskType;

        [Tooltip("该面具对应的节奏行为序列")]
        public List<RhythmActionType> actions = new List<RhythmActionType>();

        /// <summary>
        /// 行为数量
        /// </summary>
        public int ActionCount => actions?.Count ?? 0;

        /// <summary>
        /// 获取指定索引的行为类型
        /// </summary>
        public RhythmActionType GetAction(int index)
        {
            if (actions == null || actions.Count == 0)
                return RhythmActionType.Idle;

            // 循环获取
            return actions[index % actions.Count];
        }
    }

    /// <summary>
    /// 节奏序列配置 - ScriptableObject
    /// 用于配置按顺序生成的节奏序列，而非随机生成
    /// </summary>
    [CreateAssetMenu(fileName = "RhythmSequenceConfig", menuName = "Game/Rhythm/RhythmSequenceConfig", order = 2)]
    public class RhythmSequenceConfig : ScriptableObject
    {
        [Header("节奏序列配置")]
        [Tooltip("节奏序列列表，每个条目包含面具类型和对应的行为序列")]
        [SerializeField]
        private List<RhythmSequenceEntry> sequenceEntries = new List<RhythmSequenceEntry>();

        [Header("循环设置")]
        [Tooltip("序列播放完毕后是否循环")]
        [SerializeField]
        private bool loop = true;

        #region Properties

        /// <summary>
        /// 获取所有序列条目
        /// </summary>
        public IReadOnlyList<RhythmSequenceEntry> SequenceEntries => sequenceEntries;

        /// <summary>
        /// 序列条目数量
        /// </summary>
        public int EntryCount => sequenceEntries?.Count ?? 0;

        /// <summary>
        /// 是否循环
        /// </summary>
        public bool Loop => loop;

        /// <summary>
        /// 获取总节奏数量（所有条目的行为数量之和）
        /// </summary>
        public int TotalRhythmCount
        {
            get
            {
                int total = 0;
                if (sequenceEntries != null)
                {
                    foreach (var entry in sequenceEntries)
                    {
                        total += entry.ActionCount;
                    }
                }
                return total;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// 获取指定索引的序列条目
        /// </summary>
        public RhythmSequenceEntry GetEntry(int index)
        {
            if (sequenceEntries == null || sequenceEntries.Count == 0)
                return null;

            if (loop)
            {
                return sequenceEntries[index % sequenceEntries.Count];
            }
            else
            {
                if (index >= 0 && index < sequenceEntries.Count)
                    return sequenceEntries[index];
                return null;
            }
        }

        /// <summary>
        /// 根据面具类型获取序列条目
        /// </summary>
        public RhythmSequenceEntry GetEntryByMaskType(MaskType maskType)
        {
            if (sequenceEntries == null)
                return null;

            foreach (var entry in sequenceEntries)
            {
                if (entry.maskType == maskType)
                    return entry;
            }
            return null;
        }

        /// <summary>
        /// 根据全局索引获取节奏信息
        /// 返回对应的面具类型和行为类型
        /// </summary>
        /// <param name="globalIndex">全局索引</param>
        /// <param name="maskType">输出面具类型</param>
        /// <param name="actionType">输出行为类型</param>
        /// <returns>是否有效</returns>
        public bool GetRhythmByGlobalIndex(int globalIndex, out MaskType maskType, out RhythmActionType actionType)
        {
            maskType = MaskType.None;
            actionType = RhythmActionType.Idle;

            if (sequenceEntries == null || sequenceEntries.Count == 0)
                return false;

            int totalCount = TotalRhythmCount;
            if (totalCount == 0)
                return false;

            // 处理循环
            if (loop)
            {
                globalIndex = globalIndex % totalCount;
            }
            else if (globalIndex >= totalCount)
            {
                return false;
            }

            // 遍历找到对应的条目和行为
            int currentIndex = 0;
            foreach (var entry in sequenceEntries)
            {
                if (globalIndex < currentIndex + entry.ActionCount)
                {
                    maskType = entry.maskType;
                    actionType = entry.GetAction(globalIndex - currentIndex);
                    return true;
                }
                currentIndex += entry.ActionCount;
            }

            return false;
        }

        /// <summary>
        /// 验证配置是否有效
        /// </summary>
        public bool IsValid()
        {
            if (sequenceEntries == null || sequenceEntries.Count == 0)
                return false;

            foreach (var entry in sequenceEntries)
            {
                if (entry.ActionCount == 0)
                    return false;
            }
            return true;
        }

        #endregion
    }
}
