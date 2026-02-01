// ================================================
// GameConfigs - 关卡配置
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConfigs
{
    /// <summary>
    /// 难度等级
    /// </summary>
    public enum DifficultyLevel
    {
        Easy = 0,
        Normal = 1,
        Hard = 2,
        Nightmare = 3
    }

    /// <summary>
    /// 关卡奖励配置
    /// </summary>
    [Serializable]
    public class LevelReward
    {
        [Tooltip("通关获得金币")]
        [Range(0, 100000)]
        public int gold = 100;

        [Tooltip("通关获得经验")]
        [Range(0, 100000)]
        public int exp = 50;

        [Tooltip("首次通关额外奖励金币")]
        [Range(0, 100000)]
        public int firstClearBonusGold = 200;

        [Tooltip("三星通关额外奖励")]
        [Range(0, 100000)]
        public int perfectClearBonus = 300;
    }

    /// <summary>
    /// 关卡星级评价条件
    /// </summary>
    [Serializable]
    public class StarCondition
    {
        [Tooltip("一星条件：通关")]
        public bool requireComplete = true;

        [Tooltip("二星条件：时间限制（秒），0表示无限制")]
        [Range(0, 3600)]
        public int timeLimitForTwoStar = 180;

        [Tooltip("三星条件：时间限制（秒），0表示无限制")]
        [Range(0, 3600)]
        public int timeLimitForThreeStar = 120;

        [Tooltip("三星条件：最大允许受伤次数，-1表示无限制")]
        public int maxDamageCountForThreeStar = 3;
    }

    /// <summary>
    /// 单个关卡数据
    /// </summary>
    [Serializable]
    public class LevelData
    {
        [Header("基本信息")]
        [Tooltip("关卡ID")]
        public int levelId;

        [Tooltip("关卡名称")]
        public string levelName;

        [Tooltip("关卡描述")]
        [TextArea(2, 4)]
        public string description;

        [Tooltip("关卡图标路径")]
        public string iconPath;

        [Header("场景信息")]
        [Tooltip("场景路径（Build Settings中的场景路径）")]
        public string scenePath;

        [Tooltip("场景名称")]
        public string sceneName;

        [Header("关卡属性")]
        [Tooltip("所属章节")]
        [Range(1, 100)]
        public int chapter = 1;

        [Tooltip("章节内序号")]
        [Range(1, 100)]
        public int orderInChapter = 1;

        [Tooltip("难度等级")]
        public DifficultyLevel difficulty = DifficultyLevel.Normal;

        [Tooltip("推荐等级")]
        [Range(1, 100)]
        public int recommendedLevel = 1;

        [Tooltip("是否已解锁（默认状态）")]
        public bool isUnlockedByDefault = false;

        [Tooltip("解锁所需前置关卡ID，0表示无需前置")]
        public int prerequisiteLevelId = 0;

        [Header("刷怪配置")]
        [Tooltip("关卡刷怪配置")]
        public LevelSpawnConfig spawnConfig;

        [Header("奖励配置")]
        [Tooltip("关卡奖励")]
        public LevelReward reward;

        [Header("评价条件")]
        [Tooltip("星级评价条件")]
        public StarCondition starCondition;

        [Header("限制条件")]
        [Tooltip("时间限制（秒），0表示无限制")]
        [Range(0, 3600)]
        public int timeLimit = 0;

        [Tooltip("生命限制，0表示无限制")]
        [Range(0, 100)]
        public int lifeLimit = 3;

        [Header("音频设置")]
        [Tooltip("背景音乐")]
        public AudioClip backgroundMusic;

        [Tooltip("背景音乐名称（如果不指定AudioClip，则使用此名称从Resources/Audio/BGM加载）")]
        public string backgroundMusicName;

        [Tooltip("背景音乐是否循环播放")]
        public bool loopBackgroundMusic = true;

        [Tooltip("背景音乐淡入时间（秒）")]
        [Range(0f, 5f)]
        public float musicFadeInDuration = 0.5f;

        /// <summary>
        /// 获取背景音乐（优先使用AudioClip，其次使用名称从Resources加载）
        /// </summary>
        public AudioClip GetBackgroundMusic()
        {
            if (backgroundMusic != null)
            {
                return backgroundMusic;
            }
            
            if (!string.IsNullOrEmpty(backgroundMusicName))
            {
                return Resources.Load<AudioClip>($"Audio/BGM/{backgroundMusicName}");
            }
            
            return null;
        }

        /// <summary>
        /// 是否有背景音乐配置
        /// </summary>
        public bool HasBackgroundMusic => backgroundMusic != null || !string.IsNullOrEmpty(backgroundMusicName);
    }

    /// <summary>
    /// 关卡配置数据库 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "GameConfigs/Level Config", order = 0)]
    public class LevelConfig : ScriptableObject
    {
        [Tooltip("所有关卡数据")]
        public List<LevelData> levels = new List<LevelData>();

        /// <summary>
        /// 关卡字典缓存
        /// </summary>
        private Dictionary<int, LevelData> _levelDict;

        /// <summary>
        /// 根据关卡ID获取关卡数据
        /// </summary>
        public LevelData GetLevel(int levelId)
        {
            if (_levelDict == null)
            {
                BuildDictionary();
            }

            _levelDict.TryGetValue(levelId, out var data);
            return data;
        }

        /// <summary>
        /// 获取指定章节的所有关卡
        /// </summary>
        public List<LevelData> GetLevelsByChapter(int chapter)
        {
            var result = new List<LevelData>();
            foreach (var level in levels)
            {
                if (level.chapter == chapter)
                {
                    result.Add(level);
                }
            }
            result.Sort((a, b) => a.orderInChapter.CompareTo(b.orderInChapter));
            return result;
        }

        /// <summary>
        /// 获取所有章节编号
        /// </summary>
        public List<int> GetAllChapters()
        {
            var chapters = new HashSet<int>();
            foreach (var level in levels)
            {
                chapters.Add(level.chapter);
            }
            var result = new List<int>(chapters);
            result.Sort();
            return result;
        }

        /// <summary>
        /// 构建字典缓存
        /// </summary>
        private void BuildDictionary()
        {
            _levelDict = new Dictionary<int, LevelData>();
            foreach (var level in levels)
            {
                if (!_levelDict.ContainsKey(level.levelId))
                {
                    _levelDict[level.levelId] = level;
                }
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _levelDict = null;
        }

        private void OnValidate()
        {
            ClearCache();
        }
    }
}
