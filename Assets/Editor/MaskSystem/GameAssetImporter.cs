// ================================================
// MaskSystem Editor - 游戏资源导入工具
// ================================================

using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using Game.MaskSystem;
using Game.MaskSystem.Visual;

namespace Game.MaskSystem.Editor
{
    /// <summary>
    /// 游戏资源导入向导窗口
    /// </summary>
    public class GameAssetImporter : EditorWindow
    {
        #region 窗口

        [MenuItem("MaskSystem/资源导入工具")]
        public static void ShowWindow()
        {
            var window = GetWindow<GameAssetImporter>("资源导入工具");
            window.minSize = new Vector2(500, 600);
        }

        #endregion

        #region 私有字段

        private GameAssetsConfig _config;
        private Rhythm.RhythmConfig _rhythmConfig;
        private Vector2 _scrollPos;
        private int _selectedTab = 0;
        private string[] _tabNames = { "面具资源", "关卡资源", "节奏资源", "通用资源", "快速设置" };

        // 面具导入
        private MaskType _selectedMaskType = MaskType.Cat;
        private Sprite _maskIcon;
        private Sprite _maskCharacter;
        private Sprite _maskSprite;
        private Color _maskThemeColor = Color.white;

        // 关卡导入
        private int _selectedLevelIndex = 0;
        private Sprite _levelBackground;
        private AudioClip _levelBGM;
        private Color _levelAmbientColor = Color.white;

        // 节奏资源
        private AudioClip _beatSound;
        private AudioClip _perfectSound;
        private AudioClip _normalSound;
        private AudioClip _missSound;
        private Sprite _noteSprite;
        private Sprite _judgeLineSprite;
        private bool _useShaderEffects = true;

        // 批量导入
        private string _importFolderPath = "";
        private List<string> _foundFiles = new List<string>();

        #endregion

        #region GUI

        private void OnGUI()
        {
            EditorGUILayout.Space(10);

            // 配置文件选择
            DrawConfigSelection();

            EditorGUILayout.Space(10);

            // 标签页
            _selectedTab = GUILayout.Toolbar(_selectedTab, _tabNames);

            EditorGUILayout.Space(10);

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);

            switch (_selectedTab)
            {
                case 0:
                    DrawMaskAssetsTab();
                    break;
                case 1:
                    DrawLevelAssetsTab();
                    break;
                case 2:
                    DrawRhythmAssetsTab();
                    break;
                case 3:
                    DrawCommonAssetsTab();
                    break;
                case 4:
                    DrawQuickSetupTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawConfigSelection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("配置文件", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _config = (GameAssetsConfig)EditorGUILayout.ObjectField("GameAssetsConfig", _config, typeof(GameAssetsConfig), false);

            if (GUILayout.Button("新建", GUILayout.Width(60)))
            {
                CreateNewConfig();
            }

            if (_config == null && GUILayout.Button("查找", GUILayout.Width(60)))
            {
                FindExistingConfig();
            }
            EditorGUILayout.EndHorizontal();

            if (_config == null)
            {
                EditorGUILayout.HelpBox("请选择或创建一个 GameAssetsConfig 配置文件", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();
        }

        #endregion

        #region 面具资源标签页

        private void DrawMaskAssetsTab()
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox("请先选择配置文件", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("面具资源配置", EditorStyles.boldLabel);

            // 面具类型选择
            _selectedMaskType = (MaskType)EditorGUILayout.EnumPopup("面具类型", _selectedMaskType);

            EditorGUILayout.Space(10);

            // 加载当前配置
            var existingVisual = _config.GetMaskVisual(_selectedMaskType);
            if (existingVisual != null)
            {
                _maskIcon = existingVisual.Icon;
                _maskCharacter = existingVisual.CharacterSprite;
                _maskSprite = existingVisual.MaskSprite;
                _maskThemeColor = existingVisual.ThemeColor;
            }

            // 图片资源
            EditorGUILayout.LabelField("图片资源", EditorStyles.miniBoldLabel);
            _maskIcon = (Sprite)EditorGUILayout.ObjectField("图标 (Icon)", _maskIcon, typeof(Sprite), false);
            _maskCharacter = (Sprite)EditorGUILayout.ObjectField("角色图 (Character)", _maskCharacter, typeof(Sprite), false);
            _maskSprite = (Sprite)EditorGUILayout.ObjectField("面具图 (Mask)", _maskSprite, typeof(Sprite), false);

            EditorGUILayout.Space(5);

            // 颜色
            _maskThemeColor = EditorGUILayout.ColorField("主题色", _maskThemeColor);

            EditorGUILayout.Space(10);

            // 预览
            DrawMaskPreview();

            EditorGUILayout.Space(10);

            // 保存按钮
            if (GUILayout.Button("保存面具配置", GUILayout.Height(30)))
            {
                SaveMaskConfig();
            }

            EditorGUILayout.EndVertical();

            // 已配置列表
            DrawConfiguredMasksList();
        }

        private void DrawMaskPreview()
        {
            EditorGUILayout.LabelField("预览", EditorStyles.miniBoldLabel);

            EditorGUILayout.BeginHorizontal();

            // 图标预览
            if (_maskIcon != null)
            {
                GUILayout.Box(_maskIcon.texture, GUILayout.Width(64), GUILayout.Height(64));
            }
            else
            {
                GUILayout.Box("无图标", GUILayout.Width(64), GUILayout.Height(64));
            }

            // 角色预览
            if (_maskCharacter != null)
            {
                GUILayout.Box(_maskCharacter.texture, GUILayout.Width(100), GUILayout.Height(100));
            }
            else
            {
                GUILayout.Box("无角色图", GUILayout.Width(100), GUILayout.Height(100));
            }

            // 颜色预览
            EditorGUI.DrawRect(GUILayoutUtility.GetRect(50, 50), _maskThemeColor);

            EditorGUILayout.EndHorizontal();
        }

        private void DrawConfiguredMasksList()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("已配置面具列表", EditorStyles.boldLabel);

            foreach (var visual in _config.MaskVisuals)
            {
                EditorGUILayout.BeginHorizontal();

                // 图标
                if (visual.Icon != null)
                {
                    GUILayout.Box(visual.Icon.texture, GUILayout.Width(32), GUILayout.Height(32));
                }
                else
                {
                    GUILayout.Box("", GUILayout.Width(32), GUILayout.Height(32));
                }

                // 信息
                EditorGUILayout.LabelField(visual.Type.ToString(), GUILayout.Width(80));
                EditorGUILayout.LabelField(visual.GetDisplayName());

                // 编辑按钮
                if (GUILayout.Button("编辑", GUILayout.Width(50)))
                {
                    _selectedMaskType = visual.Type;
                }

                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndVertical();
        }

        private void SaveMaskConfig()
        {
            var visual = new MaskVisualData
            {
                Type = _selectedMaskType,
                Icon = _maskIcon,
                CharacterSprite = _maskCharacter,
                MaskSprite = _maskSprite,
                ThemeColor = _maskThemeColor,
                GlowColor = _maskThemeColor
            };

            _config.AddMaskVisual(visual);
            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();

            Debug.Log($"[GameAssetImporter] 保存面具配置: {_selectedMaskType}");
        }

        #endregion

        #region 关卡资源标签页

        private void DrawLevelAssetsTab()
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox("请先选择配置文件", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("关卡资源配置", EditorStyles.boldLabel);

            // 关卡选择
            string[] levelNames = new string[_config.LevelVisuals.Count];
            for (int i = 0; i < levelNames.Length; i++)
            {
                levelNames[i] = _config.LevelVisuals[i].LevelName;
            }

            if (levelNames.Length > 0)
            {
                _selectedLevelIndex = EditorGUILayout.Popup("选择关卡", _selectedLevelIndex, levelNames);

                var level = _config.GetLevelVisual(_selectedLevelIndex);
                if (level != null)
                {
                    _levelBackground = level.Background;
                    _levelBGM = level.BGM;
                    _levelAmbientColor = level.AmbientColor;
                }

                EditorGUILayout.Space(10);

                // 资源配置
                EditorGUILayout.LabelField("关卡资源", EditorStyles.miniBoldLabel);
                _levelBackground = (Sprite)EditorGUILayout.ObjectField("背景图", _levelBackground, typeof(Sprite), false);
                _levelBGM = (AudioClip)EditorGUILayout.ObjectField("背景音乐", _levelBGM, typeof(AudioClip), false);
                _levelAmbientColor = EditorGUILayout.ColorField("环境光颜色", _levelAmbientColor);

                EditorGUILayout.Space(10);

                // 预览背景
                if (_levelBackground != null)
                {
                    EditorGUILayout.LabelField("背景预览");
                    GUILayout.Box(_levelBackground.texture, GUILayout.Height(150), GUILayout.ExpandWidth(true));
                }

                EditorGUILayout.Space(10);

                if (GUILayout.Button("保存关卡配置", GUILayout.Height(30)))
                {
                    SaveLevelConfig();
                }
            }
            else
            {
                EditorGUILayout.HelpBox("配置中没有关卡数据", MessageType.Info);
            }

            EditorGUILayout.EndVertical();
        }

        private void SaveLevelConfig()
        {
            var level = _config.GetLevelVisual(_selectedLevelIndex);
            if (level != null)
            {
                level.Background = _levelBackground;
                level.BGM = _levelBGM;
                level.AmbientColor = _levelAmbientColor;

                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();

                Debug.Log($"[GameAssetImporter] 保存关卡配置: {level.LevelName}");
            }
        }

        #endregion

        #region 节奏资源标签页

        private void DrawRhythmAssetsTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("节奏配置文件", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _rhythmConfig = (Rhythm.RhythmConfig)EditorGUILayout.ObjectField("RhythmConfig", _rhythmConfig, typeof(Rhythm.RhythmConfig), false);

            if (GUILayout.Button("新建", GUILayout.Width(60)))
            {
                CreateNewRhythmConfig();
            }

            if (_rhythmConfig == null && GUILayout.Button("查找", GUILayout.Width(60)))
            {
                FindExistingRhythmConfig();
            }
            EditorGUILayout.EndHorizontal();

            if (_rhythmConfig == null)
            {
                EditorGUILayout.HelpBox("请选择或创建一个 RhythmConfig 配置文件", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 音效设置
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("节奏音效", EditorStyles.boldLabel);

            if (_rhythmConfig != null)
            {
                _rhythmConfig.BeatSound = (AudioClip)EditorGUILayout.ObjectField("节拍音效", _rhythmConfig.BeatSound, typeof(AudioClip), false);
                _rhythmConfig.PerfectSound = (AudioClip)EditorGUILayout.ObjectField("完美判定音效", _rhythmConfig.PerfectSound, typeof(AudioClip), false);
                _rhythmConfig.NormalSound = (AudioClip)EditorGUILayout.ObjectField("普通判定音效", _rhythmConfig.NormalSound, typeof(AudioClip), false);
                _rhythmConfig.MissSound = (AudioClip)EditorGUILayout.ObjectField("失误判定音效", _rhythmConfig.MissSound, typeof(AudioClip), false);
            }
            else
            {
                _beatSound = (AudioClip)EditorGUILayout.ObjectField("节拍音效", _beatSound, typeof(AudioClip), false);
                _perfectSound = (AudioClip)EditorGUILayout.ObjectField("完美判定音效", _perfectSound, typeof(AudioClip), false);
                _normalSound = (AudioClip)EditorGUILayout.ObjectField("普通判定音效", _normalSound, typeof(AudioClip), false);
                _missSound = (AudioClip)EditorGUILayout.ObjectField("失误判定音效", _missSound, typeof(AudioClip), false);
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 视觉设置
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("节奏视觉效果", EditorStyles.boldLabel);

            _useShaderEffects = EditorGUILayout.Toggle("使用Shader特效", _useShaderEffects);

            if (!_useShaderEffects)
            {
                EditorGUILayout.HelpBox("不使用Shader时需要提供图片资源", MessageType.Info);
                _noteSprite = (Sprite)EditorGUILayout.ObjectField("音符图片", _noteSprite, typeof(Sprite), false);
                _judgeLineSprite = (Sprite)EditorGUILayout.ObjectField("判定线图片", _judgeLineSprite, typeof(Sprite), false);
            }
            else
            {
                EditorGUILayout.HelpBox("将使用程序化Shader生成音符和特效:\n" +
                    "• RhythmNoteShader - 发光脉动音符\n" +
                    "• HitFeedbackShader - 打击反馈特效\n" +
                    "无需额外图片资源！", MessageType.Info);

                // Shader预览按钮
                if (GUILayout.Button("预览音符Shader"))
                {
                    ShowShaderPreview("MaskSystem/RhythmNote");
                }

                if (GUILayout.Button("预览打击特效Shader"))
                {
                    ShowShaderPreview("MaskSystem/HitFeedback");
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            // 判定参数设置
            if (_rhythmConfig != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("判定参数", EditorStyles.boldLabel);

                _rhythmConfig.BPM = EditorGUILayout.Slider("BPM", _rhythmConfig.BPM, 60, 200);
                _rhythmConfig.PerfectWindowMs = EditorGUILayout.Slider("完美窗口 (ms)", _rhythmConfig.PerfectWindowMs, 20, 100);
                _rhythmConfig.NormalWindowMs = EditorGUILayout.Slider("普通窗口 (ms)", _rhythmConfig.NormalWindowMs, 100, 300);
                _rhythmConfig.NoteTravelTime = EditorGUILayout.Slider("音符移动时间 (s)", _rhythmConfig.NoteTravelTime, 1f, 4f);

                EditorGUILayout.Space(5);

                // 颜色设置
                EditorGUILayout.LabelField("判定颜色", EditorStyles.miniBoldLabel);
                _rhythmConfig.JudgeLineColor = EditorGUILayout.ColorField("判定线颜色", _rhythmConfig.JudgeLineColor);
                _rhythmConfig.PerfectColor = EditorGUILayout.ColorField("完美颜色", _rhythmConfig.PerfectColor);
                _rhythmConfig.NormalColor = EditorGUILayout.ColorField("普通颜色", _rhythmConfig.NormalColor);
                _rhythmConfig.MissColor = EditorGUILayout.ColorField("失误颜色", _rhythmConfig.MissColor);

                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(10);

            // 保存按钮
            if (GUILayout.Button("保存节奏配置", GUILayout.Height(30)))
            {
                SaveRhythmConfig();
            }

            // 快速测试
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("快速测试", EditorStyles.boldLabel);

            if (GUILayout.Button("创建并运行节奏战斗场景"))
            {
                QuickStartSceneCreator.QuickStartRhythmGame();
            }

            EditorGUILayout.HelpBox(
                "节奏战斗操作说明:\n" +
                "• 空格键 - 卡点判定（保持当前面具）\n" +
                "• Q/W/E - 切换面具并卡点判定\n" +
                "• 音符从右向左滚动\n" +
                "• Perfect: ±50ms, Normal: ±150ms, Miss: >150ms\n" +
                "• 3次连续Perfect触发追加攻击！",
                MessageType.None);

            EditorGUILayout.EndVertical();
        }

        private void CreateNewRhythmConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "创建节奏配置文件",
                "RhythmConfig",
                "asset",
                "选择保存位置");

            if (!string.IsNullOrEmpty(path))
            {
                var config = Rhythm.RhythmConfig.CreateDefault();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                _rhythmConfig = config;
                Selection.activeObject = config;
            }
        }

        private void FindExistingRhythmConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:RhythmConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _rhythmConfig = AssetDatabase.LoadAssetAtPath<Rhythm.RhythmConfig>(path);
            }
            else
            {
                EditorUtility.DisplayDialog("未找到", "项目中没有找到 RhythmConfig 文件", "确定");
            }
        }

        private void ShowShaderPreview(string shaderName)
        {
            Shader shader = Shader.Find(shaderName);
            if (shader != null)
            {
                Selection.activeObject = shader;
                EditorGUIUtility.PingObject(shader);
            }
            else
            {
                EditorUtility.DisplayDialog("未找到", $"Shader '{shaderName}' 未找到", "确定");
            }
        }

        private void SaveRhythmConfig()
        {
            if (_rhythmConfig != null)
            {
                EditorUtility.SetDirty(_rhythmConfig);
                AssetDatabase.SaveAssets();
                Debug.Log("[GameAssetImporter] 保存节奏配置");
            }
            else
            {
                EditorUtility.DisplayDialog("错误", "请先选择或创建节奏配置文件", "确定");
            }
        }

        #endregion

        #region 通用资源标签页

        private void DrawCommonAssetsTab()
        {
            if (_config == null)
            {
                EditorGUILayout.HelpBox("请先选择配置文件", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("通用UI资源", EditorStyles.boldLabel);

            _config.HealthBarFill = (Sprite)EditorGUILayout.ObjectField("血条填充", _config.HealthBarFill, typeof(Sprite), false);
            _config.HealthBarBackground = (Sprite)EditorGUILayout.ObjectField("血条背景", _config.HealthBarBackground, typeof(Sprite), false);
            _config.WarningBarFill = (Sprite)EditorGUILayout.ObjectField("预警条填充", _config.WarningBarFill, typeof(Sprite), false);
            _config.MaskSlotBackground = (Sprite)EditorGUILayout.ObjectField("面具槽背景", _config.MaskSlotBackground, typeof(Sprite), false);
            _config.MaskSlotSelected = (Sprite)EditorGUILayout.ObjectField("面具槽选中框", _config.MaskSlotSelected, typeof(Sprite), false);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("通用特效", EditorStyles.boldLabel);

            _config.DefaultHitEffect = (GameObject)EditorGUILayout.ObjectField("受击特效", _config.DefaultHitEffect, typeof(GameObject), false);
            _config.HealEffect = (GameObject)EditorGUILayout.ObjectField("治疗特效", _config.HealEffect, typeof(GameObject), false);
            _config.LevelUpEffect = (GameObject)EditorGUILayout.ObjectField("升级特效", _config.LevelUpEffect, typeof(GameObject), false);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("通用音效", EditorStyles.boldLabel);

            _config.AttackSound = (AudioClip)EditorGUILayout.ObjectField("攻击音效", _config.AttackSound, typeof(AudioClip), false);
            _config.HitSound = (AudioClip)EditorGUILayout.ObjectField("受击音效", _config.HitSound, typeof(AudioClip), false);
            _config.SwitchMaskSound = (AudioClip)EditorGUILayout.ObjectField("切换面具音效", _config.SwitchMaskSound, typeof(AudioClip), false);
            _config.VictorySound = (AudioClip)EditorGUILayout.ObjectField("胜利音效", _config.VictorySound, typeof(AudioClip), false);
            _config.DefeatSound = (AudioClip)EditorGUILayout.ObjectField("失败音效", _config.DefeatSound, typeof(AudioClip), false);

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            if (GUILayout.Button("保存配置", GUILayout.Height(30)))
            {
                EditorUtility.SetDirty(_config);
                AssetDatabase.SaveAssets();
                Debug.Log("[GameAssetImporter] 保存通用资源配置");
            }
        }

        #endregion

        #region 快速设置标签页

        private void DrawQuickSetupTab()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("批量导入", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            _importFolderPath = EditorGUILayout.TextField("资源文件夹", _importFolderPath);
            if (GUILayout.Button("选择", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("选择资源文件夹", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    _importFolderPath = path.Replace(Application.dataPath, "Assets");
                    ScanFolder();
                }
            }
            EditorGUILayout.EndHorizontal();

            if (_foundFiles.Count > 0)
            {
                EditorGUILayout.HelpBox($"找到 {_foundFiles.Count} 个文件", MessageType.Info);

                if (GUILayout.Button("自动匹配并导入"))
                {
                    AutoImportFromFolder();
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("快速创建", EditorStyles.boldLabel);

            if (GUILayout.Button("创建战斗场景预制体"))
            {
                CreateBattleScenePrefab();
            }

            if (GUILayout.Button("创建测试场景"))
            {
                CreateTestScene();
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("使用说明", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "1. 创建或选择 GameAssetsConfig 配置文件\n" +
                "2. 在「面具资源」标签配置各面具的图片和颜色\n" +
                "3. 在「关卡资源」标签配置背景图和BGM\n" +
                "4. 在「通用资源」标签配置UI图片和音效\n" +
                "5. 点击「创建战斗场景预制体」生成可用的场景\n" +
                "6. 将预制体拖入场景即可开始游戏",
                MessageType.None);

            EditorGUILayout.EndVertical();
        }

        private void ScanFolder()
        {
            _foundFiles.Clear();

            if (string.IsNullOrEmpty(_importFolderPath)) return;

            string[] files = Directory.GetFiles(_importFolderPath, "*.*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string ext = Path.GetExtension(file).ToLower();
                if (ext == ".png" || ext == ".jpg" || ext == ".psd" || ext == ".mp3" || ext == ".wav" || ext == ".ogg")
                {
                    _foundFiles.Add(file.Replace("\\", "/"));
                }
            }
        }

        private void AutoImportFromFolder()
        {
            if (_config == null)
            {
                EditorUtility.DisplayDialog("错误", "请先选择配置文件", "确定");
                return;
            }

            int imported = 0;

            foreach (var file in _foundFiles)
            {
                string assetPath = file;
                string fileName = Path.GetFileNameWithoutExtension(file).ToLower();

                // 尝试匹配面具
                foreach (MaskType maskType in System.Enum.GetValues(typeof(MaskType)))
                {
                    if (maskType == MaskType.None) continue;

                    string maskName = maskType.ToString().ToLower();
                    if (fileName.Contains(maskName))
                    {
                        var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                        if (sprite != null)
                        {
                            var visual = _config.GetMaskVisual(maskType) ?? MaskVisualData.CreateDefault(maskType);

                            if (fileName.Contains("icon"))
                                visual.Icon = sprite;
                            else if (fileName.Contains("char") || fileName.Contains("character"))
                                visual.CharacterSprite = sprite;
                            else
                                visual.MaskSprite = sprite;

                            _config.AddMaskVisual(visual);
                            imported++;
                        }
                    }
                }

                // 尝试匹配关卡背景
                if (fileName.Contains("bg") || fileName.Contains("background"))
                {
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
                    if (sprite != null)
                    {
                        if (fileName.Contains("forest") || fileName.Contains("1"))
                        {
                            var level = _config.GetLevelVisual("快乐森林");
                            if (level != null) level.Background = sprite;
                        }
                        else if (fileName.Contains("sea") || fileName.Contains("2"))
                        {
                            var level = _config.GetLevelVisual("深海");
                            if (level != null) level.Background = sprite;
                        }
                        else if (fileName.Contains("sky") || fileName.Contains("3"))
                        {
                            var level = _config.GetLevelVisual("天空");
                            if (level != null) level.Background = sprite;
                        }
                        imported++;
                    }
                }
            }

            EditorUtility.SetDirty(_config);
            AssetDatabase.SaveAssets();

            EditorUtility.DisplayDialog("导入完成", $"成功导入 {imported} 个资源", "确定");
        }

        #endregion

        #region 辅助方法

        private void CreateNewConfig()
        {
            string path = EditorUtility.SaveFilePanelInProject(
                "创建配置文件",
                "GameAssetsConfig",
                "asset",
                "选择保存位置");

            if (!string.IsNullOrEmpty(path))
            {
                var config = GameAssetsConfig.CreateDefault();
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                _config = config;
                Selection.activeObject = config;
            }
        }

        private void FindExistingConfig()
        {
            string[] guids = AssetDatabase.FindAssets("t:GameAssetsConfig");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                _config = AssetDatabase.LoadAssetAtPath<GameAssetsConfig>(path);
            }
            else
            {
                EditorUtility.DisplayDialog("未找到", "项目中没有找到 GameAssetsConfig 文件", "确定");
            }
        }

        private void CreateBattleScenePrefab()
        {
            // 创建根物体
            var root = new GameObject("BattleScene");

            // 添加场景管理器
            var sceneManager = root.AddComponent<BattleSceneManager>();

            // 创建Canvas
            var canvas = new GameObject("Canvas");
            canvas.transform.SetParent(root.transform);
            var canvasComp = canvas.AddComponent<Canvas>();
            canvasComp.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvas.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            // 创建背景
            var background = new GameObject("Background");
            background.transform.SetParent(canvas.transform);
            var bgImage = background.AddComponent<UnityEngine.UI.Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.2f);
            var bgRect = background.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            // 保存预制体
            string prefabPath = "Assets/Prefabs/MaskSystem/BattleScene.prefab";
            string dir = Path.GetDirectoryName(prefabPath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            DestroyImmediate(root);

            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("创建完成", $"预制体已保存到: {prefabPath}", "确定");
        }

        private void CreateTestScene()
        {
            // 创建新场景
            var scene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
                UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects,
                UnityEditor.SceneManagement.NewSceneMode.Single);

            // 添加BattleSceneManager
            var root = new GameObject("BattleSceneManager");
            root.AddComponent<BattleSceneManager>();

            // 保存场景
            string scenePath = "Assets/Scenes/MaskSystemTest.unity";
            string dir = Path.GetDirectoryName(scenePath);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            UnityEditor.SceneManagement.EditorSceneManager.SaveScene(scene, scenePath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog("创建完成", $"测试场景已保存到: {scenePath}", "确定");
        }

        #endregion
    }
}

