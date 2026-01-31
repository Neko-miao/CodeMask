// ================================================
// MaskSystem - 节奏系统配置
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem.Rhythm
{
    /// <summary>
    /// 单个音符配置
    /// </summary>
    [Serializable]
    public class NoteConfig
    {
        [Tooltip("音符类型")]
        public NoteType Type = NoteType.Attack;

        [Tooltip("伤害值")]
        [Range(1, 5)]
        public int Damage = 1;

        public NoteConfig() { }

        public NoteConfig(NoteType type, int damage = 1)
        {
            Type = type;
            Damage = damage;
        }
    }

    /// <summary>
    /// 音符模式（一组重复的音符序列）
    /// </summary>
    [Serializable]
    public class NotePattern
    {
        [Tooltip("模式名称")]
        public string PatternName = "Pattern";

        [Tooltip("音符序列")]
        public List<NoteConfig> Notes = new List<NoteConfig>();

        [Tooltip("重复次数")]
        [Range(1, 10)]
        public int RepeatCount = 1;
    }

    /// <summary>
    /// 关卡节奏配置
    /// </summary>
    [Serializable]
    public class LevelRhythmConfig
    {
        [Tooltip("关卡名称")]
        public string LevelName;

        [Tooltip("BPM")]
        [Range(60, 200)]
        public float BPM = 120f;

        [Tooltip("音符模式列表")]
        public List<NotePattern> Patterns = new List<NotePattern>();

        [Tooltip("难度 (0-1)")]
        [Range(0f, 1f)]
        public float Difficulty = 0.5f;
    }

    /// <summary>
    /// 节奏系统配置 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "RhythmConfig", menuName = "MaskSystem/Rhythm Config")]
    public class RhythmConfig : ScriptableObject
    {
        [Header("基础设置")]
        [Tooltip("每分钟节拍数")]
        [Range(60, 200)]
        public float BPM = 120f;

        [Tooltip("完美判定窗口（毫秒）")]
        [Range(20, 100)]
        public float PerfectWindowMs = 50f;

        [Tooltip("普通判定窗口（毫秒）")]
        [Range(100, 300)]
        public float NormalWindowMs = 150f;

        [Tooltip("音符移动时间（秒）")]
        [Range(1f, 4f)]
        public float NoteTravelTime = 2f;

        [Header("音符模式")]
        [Tooltip("音符模式列表")]
        public List<NotePattern> Patterns = new List<NotePattern>();

        [Header("关卡配置")]
        [Tooltip("各关卡节奏配置")]
        public List<LevelRhythmConfig> LevelConfigs = new List<LevelRhythmConfig>();

        [Header("视觉设置")]
        [Tooltip("判定线颜色")]
        public Color JudgeLineColor = new Color(1f, 1f, 1f, 0.8f);

        [Tooltip("完美判定特效颜色")]
        public Color PerfectColor = new Color(1f, 0.9f, 0.2f);

        [Tooltip("普通判定特效颜色")]
        public Color NormalColor = new Color(0.3f, 0.8f, 0.3f);

        [Tooltip("失误判定特效颜色")]
        public Color MissColor = new Color(0.8f, 0.2f, 0.2f);

        [Header("音效设置")]
        [Tooltip("节拍音效")]
        public AudioClip BeatSound;

        [Tooltip("完美判定音效")]
        public AudioClip PerfectSound;

        [Tooltip("普通判定音效")]
        public AudioClip NormalSound;

        [Tooltip("失误判定音效")]
        public AudioClip MissSound;

        /// <summary>
        /// 创建默认配置
        /// </summary>
        public static RhythmConfig CreateDefault()
        {
            var config = CreateInstance<RhythmConfig>();
            config.BPM = 120f;
            config.PerfectWindowMs = 50f;
            config.NormalWindowMs = 150f;
            config.NoteTravelTime = 2f;

            // 创建默认模式 - 基础攻击模式
            var basicPattern = new NotePattern
            {
                PatternName = "基础攻击",
                RepeatCount = 4,
                Notes = new List<NoteConfig>
                {
                    new NoteConfig(NoteType.Attack, 1),
                    new NoteConfig(NoteType.Attack, 1),
                    new NoteConfig(NoteType.Idle, 0),
                    new NoteConfig(NoteType.Attack, 1)
                }
            };
            config.Patterns.Add(basicPattern);

            // 防御模式
            var defensePattern = new NotePattern
            {
                PatternName = "防御反击",
                RepeatCount = 2,
                Notes = new List<NoteConfig>
                {
                    new NoteConfig(NoteType.Defense, 1),
                    new NoteConfig(NoteType.Attack, 1),
                    new NoteConfig(NoteType.Defense, 1),
                    new NoteConfig(NoteType.Attack, 2)
                }
            };
            config.Patterns.Add(defensePattern);

            // 狂暴模式
            var ragePattern = new NotePattern
            {
                PatternName = "狂暴攻击",
                RepeatCount = 1,
                Notes = new List<NoteConfig>
                {
                    new NoteConfig(NoteType.Rage, 2),
                    new NoteConfig(NoteType.Attack, 1),
                    new NoteConfig(NoteType.Rage, 2)
                }
            };
            config.Patterns.Add(ragePattern);

            // 创建关卡配置
            // 关卡1 - 快乐森林
            var level1 = new LevelRhythmConfig
            {
                LevelName = "快乐森林",
                BPM = 100f,
                Difficulty = 0.3f,
                Patterns = new List<NotePattern>
                {
                    new NotePattern
                    {
                        PatternName = "蛇的攻击",
                        RepeatCount = 3,
                        Notes = new List<NoteConfig>
                        {
                            new NoteConfig(NoteType.Attack, 1),
                            new NoteConfig(NoteType.Idle, 0),
                            new NoteConfig(NoteType.Attack, 1)
                        }
                    }
                }
            };
            config.LevelConfigs.Add(level1);

            // 关卡2 - 深海
            var level2 = new LevelRhythmConfig
            {
                LevelName = "深海",
                BPM = 110f,
                Difficulty = 0.5f,
                Patterns = new List<NotePattern>
                {
                    new NotePattern
                    {
                        PatternName = "鲸鱼的冲击",
                        RepeatCount = 4,
                        Notes = new List<NoteConfig>
                        {
                            new NoteConfig(NoteType.Attack, 1),
                            new NoteConfig(NoteType.Defense, 1),
                            new NoteConfig(NoteType.Attack, 2),
                            new NoteConfig(NoteType.Idle, 0)
                        }
                    }
                }
            };
            config.LevelConfigs.Add(level2);

            // 关卡3 - 天空
            var level3 = new LevelRhythmConfig
            {
                LevelName = "天空",
                BPM = 140f,
                Difficulty = 0.8f,
                Patterns = new List<NotePattern>
                {
                    new NotePattern
                    {
                        PatternName = "龙的怒火",
                        RepeatCount = 5,
                        Notes = new List<NoteConfig>
                        {
                            new NoteConfig(NoteType.Attack, 1),
                            new NoteConfig(NoteType.Rage, 2),
                            new NoteConfig(NoteType.Attack, 1),
                            new NoteConfig(NoteType.Rage, 3)
                        }
                    }
                }
            };
            config.LevelConfigs.Add(level3);

            return config;
        }

        /// <summary>
        /// 获取指定关卡的配置
        /// </summary>
        public LevelRhythmConfig GetLevelConfig(string levelName)
        {
            return LevelConfigs.Find(c => c.LevelName == levelName);
        }

        /// <summary>
        /// 根据关卡索引获取配置
        /// </summary>
        public LevelRhythmConfig GetLevelConfig(int index)
        {
            if (index >= 0 && index < LevelConfigs.Count)
                return LevelConfigs[index];
            return null;
        }
    }
}

