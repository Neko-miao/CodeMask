// ================================================
// GameConfigs - 怪物配置
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameConfigs
{
    /// <summary>
    /// 单个怪物数据
    /// </summary>
    [Serializable]
    public class MonsterData
    {
        [Header("基本信息")]
        [Tooltip("怪物类型")]
        public MonsterType monsterType;

        [Tooltip("怪物名称")]
        public string monsterName;

        [Tooltip("怪物描述")]
        [TextArea(2, 4)]
        public string description;

        [Header("资源设置")]
        [Tooltip("怪物预制体引用（优先使用）")]
        public GameObject prefab;

        [Tooltip("怪物预制体路径（如果prefab为空，则使用此路径从Resources加载）")]
        public string prefabPath;

        [Tooltip("怪物图标路径（相对于Resources）")]
        public string iconPath;

        /// <summary>
        /// 加载怪物预制体（优先使用prefab引用，其次从路径加载）
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

        /// <summary>
        /// 是否有有效的预制体配置
        /// </summary>
        public bool HasPrefab => prefab != null || !string.IsNullOrEmpty(prefabPath);

        [Header("基础属性")]
        [Tooltip("生命值")]
        [Range(1, 100000)]
        public int health = 100;

        [Tooltip("攻击力")]
        [Range(0, 10000)]
        public int attack = 10;

        [Tooltip("防御力")]
        [Range(0, 10000)]
        public int defense = 5;

        [Tooltip("移动速度")]
        [Range(0f, 50f)]
        public float moveSpeed = 5f;

        [Tooltip("攻击间隔（秒）")]
        [Range(0.1f, 10f)]
        public float attackInterval = 1f;

        [Tooltip("攻击范围")]
        [Range(0.1f, 50f)]
        public float attackRange = 2f;

        [Header("战斗属性")]
        [Tooltip("是否为Boss")]
        public bool isBoss = false;

        [Tooltip("击杀获得分数")]
        [Range(0, 10000)]
        public int scoreReward = 10;

        [Tooltip("击杀获得金币")]
        [Range(0, 10000)]
        public int goldReward = 5;

        [Tooltip("击杀获得经验")]
        [Range(0, 10000)]
        public int expReward = 10;
    }

    /// <summary>
    /// 怪物配置数据库 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "MonsterConfig", menuName = "GameConfigs/Monster Config", order = 1)]
    public class MonsterConfig : ScriptableObject
    {
        [Tooltip("所有怪物数据")]
        public List<MonsterData> monsters = new List<MonsterData>();

        /// <summary>
        /// 怪物字典缓存
        /// </summary>
        private Dictionary<MonsterType, MonsterData> _monsterDict;

        /// <summary>
        /// 根据怪物类型获取怪物数据
        /// </summary>
        public MonsterData GetMonster(MonsterType type)
        {
            if (_monsterDict == null)
            {
                BuildDictionary();
            }

            _monsterDict.TryGetValue(type, out var data);
            return data;
        }

        /// <summary>
        /// 构建字典缓存
        /// </summary>
        private void BuildDictionary()
        {
            _monsterDict = new Dictionary<MonsterType, MonsterData>();
            foreach (var monster in monsters)
            {
                if (!_monsterDict.ContainsKey(monster.monsterType))
                {
                    _monsterDict[monster.monsterType] = monster;
                }
            }
        }

        /// <summary>
        /// 清除缓存（编辑器中修改后调用）
        /// </summary>
        public void ClearCache()
        {
            _monsterDict = null;
        }

        private void OnValidate()
        {
            ClearCache();
        }
    }
}
