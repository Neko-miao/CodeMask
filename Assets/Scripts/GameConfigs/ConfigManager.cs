// ================================================
// GameConfigs - 配置管理器
// ================================================

using UnityEngine;

namespace GameConfigs
{
    /// <summary>
    /// 配置管理器
    /// 负责加载和管理所有游戏配置
    /// </summary>
    public static class ConfigManager
    {
        private const string LEVEL_CONFIG_PATH = "Configs/LevelConfig";
        private const string MONSTER_CONFIG_PATH = "Configs/MonsterConfig";
        private const string PLAYER_CONFIG_PATH = "Configs/PlayerConfig";

        private static LevelConfig _levelConfig;
        private static MonsterConfig _monsterConfig;
        private static PlayerConfig _playerConfig;

        /// <summary>
        /// 关卡配置
        /// </summary>
        public static LevelConfig LevelConfig
        {
            get
            {
                if (_levelConfig == null)
                {
                    _levelConfig = Resources.Load<LevelConfig>(LEVEL_CONFIG_PATH);
                    if (_levelConfig == null)
                    {
                        Debug.LogError($"[ConfigManager] 无法加载关卡配置：{LEVEL_CONFIG_PATH}");
                    }
                }
                return _levelConfig;
            }
        }

        /// <summary>
        /// 怪物配置
        /// </summary>
        public static MonsterConfig MonsterConfig
        {
            get
            {
                if (_monsterConfig == null)
                {
                    _monsterConfig = Resources.Load<MonsterConfig>(MONSTER_CONFIG_PATH);
                    if (_monsterConfig == null)
                    {
                        Debug.LogError($"[ConfigManager] 无法加载怪物配置：{MONSTER_CONFIG_PATH}");
                    }
                }
                return _monsterConfig;
            }
        }

        /// <summary>
        /// 玩家配置
        /// </summary>
        public static PlayerConfig PlayerConfig
        {
            get
            {
                if (_playerConfig == null)
                {
                    _playerConfig = Resources.Load<PlayerConfig>(PLAYER_CONFIG_PATH);
                    if (_playerConfig == null)
                    {
                        Debug.LogError($"[ConfigManager] 无法加载玩家配置：{PLAYER_CONFIG_PATH}");
                    }
                }
                return _playerConfig;
            }
        }

        /// <summary>
        /// 根据关卡ID获取关卡数据
        /// </summary>
        public static LevelData GetLevel(int levelId)
        {
            return LevelConfig?.GetLevel(levelId);
        }

        /// <summary>
        /// 根据怪物类型获取怪物数据
        /// </summary>
        public static MonsterData GetMonster(MonsterType monsterType)
        {
            return MonsterConfig?.GetMonster(monsterType);
        }

        /// <summary>
        /// 获取指定章节的所有关卡
        /// </summary>
        public static System.Collections.Generic.List<LevelData> GetLevelsByChapter(int chapter)
        {
            return LevelConfig?.GetLevelsByChapter(chapter);
        }

        /// <summary>
        /// 获取所有章节编号
        /// </summary>
        public static System.Collections.Generic.List<int> GetAllChapters()
        {
            return LevelConfig?.GetAllChapters();
        }

        /// <summary>
        /// 根据角色ID获取角色数据
        /// </summary>
        public static PlayerCharacterData GetPlayerCharacter(int characterId)
        {
            return PlayerConfig?.GetCharacter(characterId);
        }

        /// <summary>
        /// 获取默认玩家角色数据
        /// </summary>
        public static PlayerCharacterData GetDefaultPlayerCharacter()
        {
            return PlayerConfig?.DefaultCharacter;
        }

        /// <summary>
        /// 获取玩家等级数据
        /// </summary>
        public static PlayerLevelData GetPlayerLevelData(int level)
        {
            return PlayerConfig?.GetLevelData(level);
        }

        /// <summary>
        /// 加载玩家预制体
        /// </summary>
        /// <param name="characterId">角色ID，-1使用默认角色</param>
        /// <returns>玩家预制体</returns>
        public static GameObject LoadPlayerPrefab(int characterId = -1)
        {
            var characterData = characterId >= 0 
                ? GetPlayerCharacter(characterId) 
                : GetDefaultPlayerCharacter();
            
            return characterData?.LoadPlayerPrefab();
        }

        /// <summary>
        /// 加载玩家模型预制体
        /// </summary>
        /// <param name="characterId">角色ID，-1使用默认角色</param>
        /// <param name="modelId">模型ID，-1使用默认模型</param>
        /// <returns>模型预制体</returns>
        public static GameObject LoadPlayerModelPrefab(int characterId = -1, int modelId = -1)
        {
            var characterData = characterId >= 0 
                ? GetPlayerCharacter(characterId) 
                : GetDefaultPlayerCharacter();
            
            if (characterData == null) return null;

            var modelData = modelId >= 0 
                ? characterData.GetModel(modelId) 
                : characterData.GetDefaultModel();
            
            return modelData?.LoadPrefab();
        }

        /// <summary>
        /// 获取玩家模型数据
        /// </summary>
        /// <param name="characterId">角色ID</param>
        /// <param name="modelId">模型ID，-1使用默认模型</param>
        /// <returns>模型数据</returns>
        public static PlayerModelData GetPlayerModel(int characterId, int modelId = -1)
        {
            var characterData = GetPlayerCharacter(characterId);
            if (characterData == null) return null;

            return modelId >= 0 
                ? characterData.GetModel(modelId) 
                : characterData.GetDefaultModel();
        }

        /// <summary>
        /// 预加载所有配置
        /// </summary>
        public static void PreloadAll()
        {
            var _ = LevelConfig;
            var __ = MonsterConfig;
            var ___ = PlayerConfig;
            Debug.Log("[ConfigManager] 所有配置预加载完成");
        }

        /// <summary>
        /// 清除所有配置缓存
        /// </summary>
        public static void ClearCache()
        {
            _levelConfig?.ClearCache();
            _monsterConfig?.ClearCache();
            _playerConfig?.ClearCache();
            _levelConfig = null;
            _monsterConfig = null;
            _playerConfig = null;
        }
    }
}
