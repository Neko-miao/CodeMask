// ================================================
// Editor - 玩家配置编辑器
// ================================================

using UnityEngine;
using UnityEditor;
using GameConfigs;

namespace GameEditor
{
    /// <summary>
    /// PlayerConfig 自定义编辑器
    /// </summary>
    [CustomEditor(typeof(PlayerConfig))]
    public class PlayerConfigEditor : Editor
    {
        private PlayerConfig _config;
        private bool _showCharacters = true;
        private bool _showLevels = true;
        private bool _showSpawnConfig = true;

        private void OnEnable()
        {
            _config = (PlayerConfig)target;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("玩家配置", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // 快捷操作
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("添加默认角色", GUILayout.Height(25)))
            {
                AddDefaultCharacter();
            }
            if (GUILayout.Button("添加默认等级数据", GUILayout.Height(25)))
            {
                AddDefaultLevelData();
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            // 绘制默认Inspector
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();

            // 验证信息
            EditorGUILayout.Space(10);
            DrawValidationInfo();
        }

        private void AddDefaultCharacter()
        {
            Undo.RecordObject(_config, "Add Default Character");

            var character = new PlayerCharacterData
            {
                characterId = _config.characters.Count + 1,
                characterName = $"Character_{_config.characters.Count + 1}",
                description = "新角色",
                playerPrefabPath = "Players/AKAKI",
                baseStats = new PlayerBaseStats
                {
                    maxHealth = 100,
                    attackPower = 10,
                    defense = 5,
                    moveSpeed = 5f,
                    jumpForce = 8f,
                    dashSpeedMultiplier = 2f,
                    dashDuration = 0.2f,
                    invincibleDuration = 1f
                }
            };

            // 尝试加载默认预制体
            var prefab = Resources.Load<GameObject>("Players/AKAKI");
            if (prefab != null)
            {
                character.playerPrefab = prefab;
            }

            _config.characters.Add(character);
            EditorUtility.SetDirty(_config);

            Debug.Log($"[PlayerConfigEditor] Added default character: {character.characterName}");
        }

        private void AddDefaultLevelData()
        {
            Undo.RecordObject(_config, "Add Default Level Data");

            // 添加1-10级的默认数据
            for (int i = 1; i <= 10; i++)
            {
                if (_config.levelDataList.Exists(l => l.level == i))
                    continue;

                var levelData = new PlayerLevelData
                {
                    level = i,
                    expRequired = Mathf.RoundToInt(100 * Mathf.Pow(i, 1.5f)),
                    baseHealth = 100 + (i - 1) * 20,
                    baseAttack = 10 + (i - 1) * 2,
                    baseDefense = 5 + (i - 1) * 1,
                    baseMoveSpeed = 5f + (i - 1) * 0.1f
                };

                _config.levelDataList.Add(levelData);
            }

            // 按等级排序
            _config.levelDataList.Sort((a, b) => a.level.CompareTo(b.level));

            EditorUtility.SetDirty(_config);
            Debug.Log("[PlayerConfigEditor] Added default level data (1-10)");
        }

        private void DrawValidationInfo()
        {
            EditorGUILayout.LabelField("配置验证", EditorStyles.boldLabel);

            var style = new GUIStyle(EditorStyles.helpBox);

            // 检查角色配置
            if (_config.characters.Count == 0)
            {
                EditorGUILayout.HelpBox("警告: 没有配置任何角色，请点击\"添加默认角色\"按钮添加角色", MessageType.Warning);
            }
            else
            {
                // 检查默认角色
                var defaultChar = _config.GetCharacter(_config.defaultCharacterId);
                if (defaultChar == null)
                {
                    EditorGUILayout.HelpBox($"警告: 默认角色ID({_config.defaultCharacterId})不存在", MessageType.Warning);
                }
                else
                {
                    // 检查预制体
                    bool hasPrefab = defaultChar.playerPrefab != null || 
                                     defaultChar.prefab != null ||
                                     !string.IsNullOrEmpty(defaultChar.playerPrefabPath) ||
                                     !string.IsNullOrEmpty(defaultChar.prefabPath);

                    if (!hasPrefab)
                    {
                        EditorGUILayout.HelpBox($"警告: 默认角色\"{defaultChar.characterName}\"没有配置预制体", MessageType.Warning);
                    }
                    else
                    {
                        EditorGUILayout.HelpBox($"✓ 默认角色: {defaultChar.characterName} (ID: {defaultChar.characterId})", MessageType.Info);
                    }
                }

                EditorGUILayout.LabelField($"角色数量: {_config.characters.Count}");
            }

            // 检查等级配置
            if (_config.levelDataList.Count == 0)
            {
                EditorGUILayout.HelpBox("提示: 没有配置等级数据，将使用默认公式计算", MessageType.Info);
            }
            else
            {
                EditorGUILayout.LabelField($"等级配置数量: {_config.levelDataList.Count}");
            }

            // 显示生成配置
            EditorGUILayout.LabelField($"生成位置: {_config.spawnConfig.defaultSpawnPosition}");
            EditorGUILayout.LabelField($"允许重生: {_config.spawnConfig.allowRespawn}");
        }
    }

    /// <summary>
    /// PlayerConfig 菜单工具
    /// </summary>
    public static class PlayerConfigMenu
    {
        [MenuItem("GameTools/打开玩家配置")]
        public static void OpenPlayerConfig()
        {
            var config = Resources.Load<PlayerConfig>("Configs/PlayerConfig");
            if (config != null)
            {
                Selection.activeObject = config;
                EditorGUIUtility.PingObject(config);
            }
            else
            {
                Debug.LogWarning("PlayerConfig not found at Resources/Configs/PlayerConfig");
            }
        }

        [MenuItem("GameTools/创建玩家配置")]
        public static void CreatePlayerConfig()
        {
            // 确保目录存在
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Configs"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Configs");
            }

            // 检查是否已存在
            var existing = Resources.Load<PlayerConfig>("Configs/PlayerConfig");
            if (existing != null)
            {
                Debug.Log("PlayerConfig already exists");
                Selection.activeObject = existing;
                return;
            }

            // 创建配置
            var config = ScriptableObject.CreateInstance<PlayerConfig>();
            
            // 添加默认角色
            var defaultCharacter = new PlayerCharacterData
            {
                characterId = 1,
                characterName = "Default Hero",
                description = "默认英雄角色",
                playerPrefabPath = "Players/AKAKI",
                baseStats = new PlayerBaseStats
                {
                    maxHealth = 100,
                    attackPower = 10,
                    defense = 5,
                    moveSpeed = 5f,
                    jumpForce = 8f
                }
            };

            // 尝试加载预制体
            var prefab = Resources.Load<GameObject>("Players/AKAKI");
            if (prefab != null)
            {
                defaultCharacter.playerPrefab = prefab;
            }

            config.characters.Add(defaultCharacter);
            config.defaultCharacterId = 1;

            // 添加默认等级数据
            for (int i = 1; i <= 10; i++)
            {
                config.levelDataList.Add(new PlayerLevelData
                {
                    level = i,
                    expRequired = Mathf.RoundToInt(100 * Mathf.Pow(i, 1.5f)),
                    baseHealth = 100 + (i - 1) * 20,
                    baseAttack = 10 + (i - 1) * 2,
                    baseDefense = 5 + (i - 1) * 1,
                    baseMoveSpeed = 5f + (i - 1) * 0.1f
                });
            }

            AssetDatabase.CreateAsset(config, "Assets/Resources/Configs/PlayerConfig.asset");
            AssetDatabase.SaveAssets();

            Selection.activeObject = config;
            Debug.Log("PlayerConfig created at Assets/Resources/Configs/PlayerConfig.asset");
        }

        [MenuItem("GameTools/验证玩家配置")]
        public static void ValidatePlayerConfig()
        {
            var config = Resources.Load<PlayerConfig>("Configs/PlayerConfig");
            if (config == null)
            {
                Debug.LogError("PlayerConfig not found!");
                return;
            }

            Debug.Log("=== PlayerConfig Validation ===");
            Debug.Log($"Characters: {config.characters.Count}");
            Debug.Log($"Default Character ID: {config.defaultCharacterId}");

            var defaultChar = config.GetCharacter(config.defaultCharacterId);
            if (defaultChar != null)
            {
                Debug.Log($"Default Character: {defaultChar.characterName}");
                
                var prefab = defaultChar.LoadPlayerPrefab();
                if (prefab != null)
                {
                    Debug.Log($"Prefab: {prefab.name} ✓");
                }
                else
                {
                    Debug.LogWarning("Prefab: NOT FOUND!");
                }
            }
            else
            {
                Debug.LogError($"Default character (ID: {config.defaultCharacterId}) not found!");
            }

            Debug.Log($"Level Data: {config.levelDataList.Count} entries");
            Debug.Log($"Spawn Position: {config.spawnConfig.defaultSpawnPosition}");
            Debug.Log("=== Validation Complete ===");
        }
    }
}
