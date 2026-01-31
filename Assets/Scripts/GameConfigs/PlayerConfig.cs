// ================================================
// GameConfigs - 玩家配置
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConfigs
{
    /// <summary>
    /// 玩家动画类型
    /// </summary>
    public enum PlayerAnimationType
    {
        Idle,
        Walk,
        Run,
        Jump,
        Fall,
        Attack,
        Hurt,
        Die,
        Skill1,
        Skill2,
        Skill3,
        Victory,
        Defeat
    }

    /// <summary>
    /// 2D精灵动画数据
    /// </summary>
    [Serializable]
    public class SpriteAnimationData
    {
        [Tooltip("动画类型")]
        public PlayerAnimationType animationType = PlayerAnimationType.Idle;

        [Tooltip("动画帧")]
        public List<Sprite> frames = new List<Sprite>();

        [Tooltip("帧率")]
        [Range(1, 60)]
        public int frameRate = 12;

        [Tooltip("是否循环")]
        public bool loop = true;

        [Tooltip("动画播放完成后切换到的动画类型（非循环动画使用）")]
        public PlayerAnimationType nextAnimation = PlayerAnimationType.Idle;
    }

    /// <summary>
    /// 玩家等级数据
    /// </summary>
    [Serializable]
    public class PlayerLevelData
    {
        [Tooltip("等级")]
        public int level = 1;

        [Tooltip("升级所需经验")]
        public int expRequired = 100;

        [Tooltip("该等级基础生命值")]
        public int baseHealth = 100;

        [Tooltip("该等级基础攻击力")]
        public int baseAttack = 10;

        [Tooltip("该等级基础防御力")]
        public int baseDefense = 5;

        [Tooltip("该等级基础移动速度")]
        public float baseMoveSpeed = 5f;
    }

    /// <summary>
    /// 玩家基础属性
    /// </summary>
    [Serializable]
    public class PlayerBaseStats
    {
        [Header("生命值")]
        [Tooltip("初始最大生命值")]
        [Range(1, 10000)]
        public int maxHealth = 100;

        [Tooltip("生命回复速度（每秒）")]
        [Range(0, 100)]
        public float healthRegen = 0f;

        [Header("攻击")]
        [Tooltip("基础攻击力")]
        [Range(1, 1000)]
        public int attackPower = 10;

        [Tooltip("攻击速度（次/秒）")]
        [Range(0.1f, 10f)]
        public float attackSpeed = 1f;

        [Tooltip("攻击范围")]
        [Range(0.1f, 20f)]
        public float attackRange = 1.5f;

        [Header("防御")]
        [Tooltip("基础防御力")]
        [Range(0, 1000)]
        public int defense = 5;

        [Header("移动")]
        [Tooltip("移动速度")]
        [Range(0.1f, 50f)]
        public float moveSpeed = 5f;

        [Tooltip("跳跃力")]
        [Range(0f, 50f)]
        public float jumpForce = 8f;

        [Tooltip("冲刺速度倍率")]
        [Range(1f, 5f)]
        public float dashSpeedMultiplier = 2f;

        [Tooltip("冲刺持续时间")]
        [Range(0.1f, 2f)]
        public float dashDuration = 0.2f;

        [Header("无敌")]
        [Tooltip("受伤后无敌时间")]
        [Range(0f, 5f)]
        public float invincibleDuration = 1f;
    }

    /// <summary>
    /// 玩家模型数据
    /// </summary>
    [Serializable]
    public class PlayerModelData
    {
        [Tooltip("模型ID")]
        public int modelId = 1;

        [Tooltip("模型名称")]
        public string modelName = "Default";

        [Tooltip("模型预制体（直接引用，推荐）")]
        public GameObject prefab;

        [Tooltip("模型预制体路径（Resources下的路径，备用）")]
        public string prefabPath = "";

        [Tooltip("模型缩放")]
        public Vector3 scale = Vector3.one;

        [Tooltip("模型偏移（相对于玩家根节点）")]
        public Vector3 offset = Vector3.zero;

        [Tooltip("模型旋转偏移")]
        public Vector3 rotationOffset = Vector3.zero;

        [Tooltip("模型预览图")]
        public Sprite previewSprite;

        [Tooltip("是否默认模型")]
        public bool isDefault = false;

        [Tooltip("解锁条件描述")]
        public string unlockDescription = "";

        /// <summary>
        /// 加载模型预制体
        /// </summary>
        public GameObject LoadPrefab()
        {
            if (prefab != null)
            {
                return prefab;
            }

            if (!string.IsNullOrEmpty(prefabPath))
            {
                return Resources.Load<GameObject>(prefabPath);
            }

            return null;
        }
    }

    /// <summary>
    /// 单个玩家角色数据
    /// </summary>
    [Serializable]
    public class PlayerCharacterData
    {
        [Header("基本信息")]
        [Tooltip("角色ID")]
        public int characterId = 1;

        [Tooltip("角色名称")]
        public string characterName = "Hero";

        [Tooltip("角色描述")]
        [TextArea(2, 4)]
        public string description = "";

        [Tooltip("角色图标")]
        public Sprite icon;

        [Header("玩家预制体")]
        [Tooltip("玩家控制器预制体（包含完整的玩家组件，直接引用）")]
        public GameObject playerPrefab;

        [Tooltip("玩家控制器预制体路径（Resources下的路径，备用）")]
        public string playerPrefabPath = "Prefabs/Player/PlayerCharacter";

        [Header("模型配置")]
        [Tooltip("默认模型ID")]
        public int defaultModelId = 1;

        [Tooltip("可用模型列表")]
        public List<PlayerModelData> models = new List<PlayerModelData>();

        [Header("兼容旧版 - 直接预制体引用")]
        [Tooltip("角色预制体路径（Resources下的路径）- 旧版兼容")]
        public string prefabPath = "Prefabs/Player/PlayerCharacter";

        [Tooltip("角色预制体（直接引用）- 旧版兼容")]
        public GameObject prefab;

        [Header("基础属性")]
        [Tooltip("角色基础属性")]
        public PlayerBaseStats baseStats = new PlayerBaseStats();

        [Header("2D动画配置")]
        [Tooltip("是否使用2D精灵动画")]
        public bool use2DSpriteAnimation = true;

        [Tooltip("精灵动画列表")]
        public List<SpriteAnimationData> spriteAnimations = new List<SpriteAnimationData>();

        [Header("Animator配置")]
        [Tooltip("Animator控制器（不使用精灵动画时）")]
        public RuntimeAnimatorController animatorController;

        [Header("音效")]
        [Tooltip("攻击音效路径")]
        public string attackSoundPath;

        [Tooltip("受伤音效路径")]
        public string hurtSoundPath;

        [Tooltip("死亡音效路径")]
        public string dieSoundPath;

        [Tooltip("跳跃音效路径")]
        public string jumpSoundPath;

        /// <summary>
        /// 获取指定类型的精灵动画数据
        /// </summary>
        public SpriteAnimationData GetSpriteAnimation(PlayerAnimationType type)
        {
            return spriteAnimations.Find(a => a.animationType == type);
        }

        /// <summary>
        /// 加载玩家预制体（优先使用新版配置）
        /// </summary>
        public GameObject LoadPlayerPrefab()
        {
            // 优先使用新版玩家预制体
            if (playerPrefab != null)
            {
                return playerPrefab;
            }

            if (!string.IsNullOrEmpty(playerPrefabPath))
            {
                var loaded = Resources.Load<GameObject>(playerPrefabPath);
                if (loaded != null) return loaded;
            }

            // 回退到旧版预制体
            if (prefab != null)
            {
                return prefab;
            }

            if (!string.IsNullOrEmpty(prefabPath))
            {
                return Resources.Load<GameObject>(prefabPath);
            }

            return null;
        }

        /// <summary>
        /// 获取默认模型数据
        /// </summary>
        public PlayerModelData GetDefaultModel()
        {
            if (models == null || models.Count == 0)
            {
                return null;
            }

            // 先尝试通过默认模型ID获取
            var model = models.Find(m => m.modelId == defaultModelId);
            if (model != null) return model;

            // 再尝试查找标记为默认的模型
            model = models.Find(m => m.isDefault);
            if (model != null) return model;

            // 返回第一个模型
            return models[0];
        }

        /// <summary>
        /// 根据模型ID获取模型数据
        /// </summary>
        public PlayerModelData GetModel(int modelId)
        {
            return models?.Find(m => m.modelId == modelId);
        }

        /// <summary>
        /// 获取所有模型
        /// </summary>
        public IReadOnlyList<PlayerModelData> GetAllModels()
        {
            return models;
        }
    }

    /// <summary>
    /// 玩家生成配置
    /// </summary>
    [Serializable]
    public class PlayerSpawnConfig
    {
        [Tooltip("默认生成位置")]
        public Vector3 defaultSpawnPosition = Vector3.zero;

        [Tooltip("默认生成旋转")]
        public Vector3 defaultSpawnRotation = Vector3.zero;

        [Tooltip("是否使用场景中的生成点")]
        public bool useSceneSpawnPoint = true;

        [Tooltip("场景生成点标签")]
        public string spawnPointTag = "PlayerSpawnPoint";

        [Tooltip("生成后无敌时间")]
        [Range(0f, 10f)]
        public float spawnInvincibleTime = 2f;

        [Tooltip("重生延迟时间")]
        [Range(0f, 10f)]
        public float respawnDelay = 3f;

        [Tooltip("是否允许重生")]
        public bool allowRespawn = true;

        [Tooltip("最大重生次数，-1为无限")]
        public int maxRespawnCount = -1;
    }

    /// <summary>
    /// 玩家配置数据库 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerConfig", menuName = "GameConfigs/Player Config", order = 1)]
    public class PlayerConfig : ScriptableObject
    {
        [Header("默认设置")]
        [Tooltip("默认角色ID")]
        public int defaultCharacterId = 1;

        [Header("生成配置")]
        [Tooltip("玩家生成配置")]
        public PlayerSpawnConfig spawnConfig = new PlayerSpawnConfig();

        [Header("角色列表")]
        [Tooltip("所有可用角色")]
        public List<PlayerCharacterData> characters = new List<PlayerCharacterData>();

        [Header("等级配置")]
        [Tooltip("等级数据列表")]
        public List<PlayerLevelData> levelDataList = new List<PlayerLevelData>();

        /// <summary>
        /// 角色字典缓存
        /// </summary>
        private Dictionary<int, PlayerCharacterData> _characterDict;

        /// <summary>
        /// 等级字典缓存
        /// </summary>
        private Dictionary<int, PlayerLevelData> _levelDict;

        /// <summary>
        /// 获取默认角色
        /// </summary>
        public PlayerCharacterData DefaultCharacter => GetCharacter(defaultCharacterId);

        /// <summary>
        /// 根据角色ID获取角色数据
        /// </summary>
        public PlayerCharacterData GetCharacter(int characterId)
        {
            if (_characterDict == null)
            {
                BuildCharacterDictionary();
            }

            _characterDict.TryGetValue(characterId, out var data);
            return data;
        }

        /// <summary>
        /// 根据等级获取等级数据
        /// </summary>
        public PlayerLevelData GetLevelData(int level)
        {
            if (_levelDict == null)
            {
                BuildLevelDictionary();
            }

            if (_levelDict.TryGetValue(level, out var data))
            {
                return data;
            }

            // 如果没有找到精确等级，返回最高等级数据
            if (levelDataList.Count > 0)
            {
                return levelDataList[levelDataList.Count - 1];
            }

            return null;
        }

        /// <summary>
        /// 计算指定等级升级所需经验
        /// </summary>
        public int GetExpToNextLevel(int level)
        {
            var levelData = GetLevelData(level);
            if (levelData != null)
            {
                return levelData.expRequired;
            }

            // 默认公式: 100 * level^1.5
            return Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
        }

        /// <summary>
        /// 获取所有角色
        /// </summary>
        public IReadOnlyList<PlayerCharacterData> GetAllCharacters()
        {
            return characters;
        }

        /// <summary>
        /// 构建角色字典缓存
        /// </summary>
        private void BuildCharacterDictionary()
        {
            _characterDict = new Dictionary<int, PlayerCharacterData>();
            foreach (var character in characters)
            {
                if (!_characterDict.ContainsKey(character.characterId))
                {
                    _characterDict[character.characterId] = character;
                }
            }
        }

        /// <summary>
        /// 构建等级字典缓存
        /// </summary>
        private void BuildLevelDictionary()
        {
            _levelDict = new Dictionary<int, PlayerLevelData>();
            foreach (var levelData in levelDataList)
            {
                if (!_levelDict.ContainsKey(levelData.level))
                {
                    _levelDict[levelData.level] = levelData;
                }
            }
        }

        /// <summary>
        /// 清除缓存
        /// </summary>
        public void ClearCache()
        {
            _characterDict = null;
            _levelDict = null;
        }

        private void OnValidate()
        {
            ClearCache();
        }
    }
}
