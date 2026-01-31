// ================================================
// MaskSystem - 关卡配置编辑器
// ================================================

using UnityEngine;
using UnityEditor;
using Game.MaskSystem;
using System.Collections.Generic;

namespace Game.MaskSystem.Editor
{
    /// <summary>
    /// 关卡配置自定义Inspector
    /// </summary>
    [CustomEditor(typeof(LevelConfig))]
    public class LevelConfigEditor : UnityEditor.Editor
    {
        private SerializedProperty _levelName;
        private SerializedProperty _description;
        private SerializedProperty _levelIndex;
        private SerializedProperty _playerInitialMask;
        private SerializedProperty _prepareTime;
        private SerializedProperty _waveInterval;
        private SerializedProperty _waves;

        private bool _showWavesFoldout = true;
        private List<bool> _waveFoldouts = new List<bool>();

        private void OnEnable()
        {
            _levelName = serializedObject.FindProperty("LevelName");
            _description = serializedObject.FindProperty("Description");
            _levelIndex = serializedObject.FindProperty("LevelIndex");
            _playerInitialMask = serializedObject.FindProperty("PlayerInitialMask");
            _prepareTime = serializedObject.FindProperty("PrepareTime");
            _waveInterval = serializedObject.FindProperty("WaveInterval");
            _waves = serializedObject.FindProperty("_waves");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawHeader();
            EditorGUILayout.Space(10);

            DrawLevelInfo();
            EditorGUILayout.Space(10);

            DrawLevelSettings();
            EditorGUILayout.Space(10);

            DrawWavesList();
            EditorGUILayout.Space(10);

            DrawQuickActions();
            EditorGUILayout.Space(10);

            DrawPreview();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("面具系统 - 关卡配置", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();
        }

        private void DrawLevelInfo()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("关卡信息", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_levelName, new GUIContent("关卡名称"));
            EditorGUILayout.PropertyField(_description, new GUIContent("关卡描述"));
            EditorGUILayout.PropertyField(_levelIndex, new GUIContent("关卡编号"));

            EditorGUILayout.EndVertical();
        }

        private void DrawLevelSettings()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("关卡设置", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(_playerInitialMask, new GUIContent("玩家初始面具"));
            EditorGUILayout.PropertyField(_prepareTime, new GUIContent("准备时间(秒)"));
            EditorGUILayout.PropertyField(_waveInterval, new GUIContent("波次间隔(秒)"));

            EditorGUILayout.EndVertical();
        }

        private void DrawWavesList()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 标题行
            EditorGUILayout.BeginHorizontal();
            _showWavesFoldout = EditorGUILayout.Foldout(_showWavesFoldout, $"敌人波次 ({_waves.arraySize})", true);
            
            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button("+", GUILayout.Width(25)))
            {
                AddNewWave();
            }
            EditorGUILayout.EndHorizontal();

            if (_showWavesFoldout)
            {
                EditorGUI.indentLevel++;

                // 确保foldout列表大小正确
                while (_waveFoldouts.Count < _waves.arraySize)
                    _waveFoldouts.Add(true);
                while (_waveFoldouts.Count > _waves.arraySize)
                    _waveFoldouts.RemoveAt(_waveFoldouts.Count - 1);

                for (int i = 0; i < _waves.arraySize; i++)
                {
                    DrawWaveItem(i);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawWaveItem(int index)
        {
            var wave = _waves.GetArrayElementAtIndex(index);
            var enemyType = wave.FindPropertyRelative("EnemyType");
            var enemyName = wave.FindPropertyRelative("EnemyName");
            var health = wave.FindPropertyRelative("EnemyHealth");
            var attackPower = wave.FindPropertyRelative("EnemyAttackPower");
            var minInterval = wave.FindPropertyRelative("MinAttackInterval");
            var maxInterval = wave.FindPropertyRelative("MaxAttackInterval");
            var warningTime = wave.FindPropertyRelative("AttackWarningTime");
            var description = wave.FindPropertyRelative("Description");

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            // 波次标题
            EditorGUILayout.BeginHorizontal();
            
            string displayName = !string.IsNullOrEmpty(enemyName.stringValue) 
                ? enemyName.stringValue 
                : ((MaskType)enemyType.enumValueIndex).ToString();
            
            _waveFoldouts[index] = EditorGUILayout.Foldout(_waveFoldouts[index], 
                $"波次 {index + 1}: {displayName}", true);

            GUILayout.FlexibleSpace();

            // 上移按钮
            EditorGUI.BeginDisabledGroup(index == 0);
            if (GUILayout.Button("↑", GUILayout.Width(25)))
            {
                _waves.MoveArrayElement(index, index - 1);
            }
            EditorGUI.EndDisabledGroup();

            // 下移按钮
            EditorGUI.BeginDisabledGroup(index == _waves.arraySize - 1);
            if (GUILayout.Button("↓", GUILayout.Width(25)))
            {
                _waves.MoveArrayElement(index, index + 1);
            }
            EditorGUI.EndDisabledGroup();

            // 删除按钮
            if (GUILayout.Button("X", GUILayout.Width(25)))
            {
                if (EditorUtility.DisplayDialog("删除波次", $"确定删除波次 {index + 1}?", "确定", "取消"))
                {
                    _waves.DeleteArrayElementAtIndex(index);
                    return;
                }
            }

            EditorGUILayout.EndHorizontal();

            // 波次详情
            if (_waveFoldouts[index])
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.PropertyField(enemyName, new GUIContent("敌人名称"));
                EditorGUILayout.PropertyField(enemyType, new GUIContent("面具类型"));
                
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("属性", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(health, new GUIContent("血量"));
                EditorGUILayout.PropertyField(attackPower, new GUIContent("攻击力"));

                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("攻击节奏", EditorStyles.miniBoldLabel);
                EditorGUILayout.PropertyField(minInterval, new GUIContent("最小间隔(秒)"));
                EditorGUILayout.PropertyField(maxInterval, new GUIContent("最大间隔(秒)"));
                EditorGUILayout.PropertyField(warningTime, new GUIContent("预警时间(秒)"));

                EditorGUILayout.Space(5);
                EditorGUILayout.PropertyField(description, new GUIContent("描述"));

                // 使用预设按钮
                if (GUILayout.Button("应用预设值"))
                {
                    ApplyPresetToWave(wave, (MaskType)enemyType.enumValueIndex);
                }

                EditorGUI.indentLevel--;
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawQuickActions()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("快速操作", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("添加蛇"))
                AddWaveWithType(MaskType.Snake);
            if (GUILayout.Button("添加猫"))
                AddWaveWithType(MaskType.Cat);
            if (GUILayout.Button("添加熊"))
                AddWaveWithType(MaskType.Bear);
            if (GUILayout.Button("添加牛"))
                AddWaveWithType(MaskType.Bull);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("添加鲸鱼"))
                AddWaveWithType(MaskType.Whale);
            if (GUILayout.Button("添加鲨鱼"))
                AddWaveWithType(MaskType.Shark);
            if (GUILayout.Button("添加龙"))
                AddWaveWithType(MaskType.Dragon);

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("加载预设: 快乐森林"))
            {
                LoadPreset(LevelConfig.CreateLevel1_HappyForest());
            }
            if (GUILayout.Button("加载预设: 深海"))
            {
                LoadPreset(LevelConfig.CreateLevel2_DeepSea());
            }
            if (GUILayout.Button("加载预设: 天空"))
            {
                LoadPreset(LevelConfig.CreateLevel3_Sky());
            }

            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button("清空所有波次"))
            {
                if (EditorUtility.DisplayDialog("清空波次", "确定清空所有波次?", "确定", "取消"))
                {
                    _waves.ClearArray();
                }
            }

            EditorGUILayout.EndVertical();
        }

        private void DrawPreview()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            GUILayout.Label("关卡预览", EditorStyles.boldLabel);

            var level = (LevelConfig)target;
            EditorGUILayout.LabelField(level.GetSummary(), EditorStyles.wordWrappedLabel);

            float totalTime = level.PrepareTime;
            for (int i = 0; i < level.WaveCount; i++)
            {
                var wave = level.GetWave(i);
                // 估算每波的平均时间
                float avgInterval = (wave.MinAttackInterval + wave.MaxAttackInterval) / 2f;
                float estimatedWaveTime = avgInterval * wave.EnemyHealth * 1.5f; // 粗略估算
                totalTime += estimatedWaveTime;

                if (i < level.WaveCount - 1)
                    totalTime += level.WaveInterval;
            }

            EditorGUILayout.LabelField($"预计游戏时长: {totalTime:F0}秒 ({totalTime/60:F1}分钟)");

            EditorGUILayout.EndVertical();
        }

        private void AddNewWave()
        {
            _waves.InsertArrayElementAtIndex(_waves.arraySize);
            var newWave = _waves.GetArrayElementAtIndex(_waves.arraySize - 1);
            
            // 设置默认值
            newWave.FindPropertyRelative("EnemyType").enumValueIndex = (int)MaskType.Snake;
            newWave.FindPropertyRelative("EnemyHealth").intValue = 3;
            newWave.FindPropertyRelative("EnemyAttackPower").intValue = 1;
            newWave.FindPropertyRelative("MinAttackInterval").floatValue = 1.5f;
            newWave.FindPropertyRelative("MaxAttackInterval").floatValue = 3f;
            newWave.FindPropertyRelative("AttackWarningTime").floatValue = 0.8f;
        }

        private void AddWaveWithType(MaskType type)
        {
            _waves.InsertArrayElementAtIndex(_waves.arraySize);
            var newWave = _waves.GetArrayElementAtIndex(_waves.arraySize - 1);
            ApplyPresetToWave(newWave, type);
        }

        private void ApplyPresetToWave(SerializedProperty wave, MaskType type)
        {
            var preset = WaveConfig.CreateDefault(type);
            
            wave.FindPropertyRelative("EnemyName").stringValue = "";
            wave.FindPropertyRelative("EnemyType").enumValueIndex = (int)type;
            wave.FindPropertyRelative("EnemyHealth").intValue = preset.EnemyHealth;
            wave.FindPropertyRelative("EnemyAttackPower").intValue = preset.EnemyAttackPower;
            wave.FindPropertyRelative("MinAttackInterval").floatValue = preset.MinAttackInterval;
            wave.FindPropertyRelative("MaxAttackInterval").floatValue = preset.MaxAttackInterval;
            wave.FindPropertyRelative("AttackWarningTime").floatValue = preset.AttackWarningTime;
        }

        private void LoadPreset(LevelConfig preset)
        {
            if (!EditorUtility.DisplayDialog("加载预设", $"是否加载预设关卡 '{preset.LevelName}'?\n这将覆盖当前配置。", "确定", "取消"))
            {
                return;
            }

            _levelName.stringValue = preset.LevelName;
            _description.stringValue = preset.Description;
            _levelIndex.intValue = preset.LevelIndex;
            _prepareTime.floatValue = preset.PrepareTime;
            _waveInterval.floatValue = preset.WaveInterval;

            _waves.ClearArray();
            foreach (var wave in preset.Waves)
            {
                _waves.InsertArrayElementAtIndex(_waves.arraySize);
                var newWave = _waves.GetArrayElementAtIndex(_waves.arraySize - 1);
                
                newWave.FindPropertyRelative("EnemyName").stringValue = wave.EnemyName;
                newWave.FindPropertyRelative("EnemyType").enumValueIndex = (int)wave.EnemyType;
                newWave.FindPropertyRelative("EnemyHealth").intValue = wave.EnemyHealth;
                newWave.FindPropertyRelative("EnemyAttackPower").intValue = wave.EnemyAttackPower;
                newWave.FindPropertyRelative("MinAttackInterval").floatValue = wave.MinAttackInterval;
                newWave.FindPropertyRelative("MaxAttackInterval").floatValue = wave.MaxAttackInterval;
                newWave.FindPropertyRelative("AttackWarningTime").floatValue = wave.AttackWarningTime;
                newWave.FindPropertyRelative("Description").stringValue = wave.Description;
            }

            // 销毁临时预设对象
            DestroyImmediate(preset);
        }
    }

    /// <summary>
    /// 关卡配置创建菜单
    /// </summary>
    public static class LevelConfigMenu
    {
        [MenuItem("MaskSystem/创建关卡配置")]
        public static void CreateLevelConfig()
        {
            var config = ScriptableObject.CreateInstance<LevelConfig>();
            
            string path = EditorUtility.SaveFilePanelInProject(
                "保存关卡配置", 
                "NewLevelConfig", 
                "asset", 
                "选择保存位置");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
            }
        }

        [MenuItem("MaskSystem/创建预设关卡/快乐森林")]
        public static void CreateLevel1()
        {
            SavePresetLevel(LevelConfig.CreateLevel1_HappyForest(), "Level1_HappyForest");
        }

        [MenuItem("MaskSystem/创建预设关卡/深海")]
        public static void CreateLevel2()
        {
            SavePresetLevel(LevelConfig.CreateLevel2_DeepSea(), "Level2_DeepSea");
        }

        [MenuItem("MaskSystem/创建预设关卡/天空")]
        public static void CreateLevel3()
        {
            SavePresetLevel(LevelConfig.CreateLevel3_Sky(), "Level3_Sky");
        }

        private static void SavePresetLevel(LevelConfig config, string defaultName)
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "保存关卡配置",
                defaultName,
                "asset",
                "选择保存位置");

            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                EditorUtility.FocusProjectWindow();
                Selection.activeObject = config;
            }
        }
    }
}

