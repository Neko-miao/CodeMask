using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Game.MaskSystem.Visual.Placeholder;
using Game.MaskSystem.Rhythm;

namespace Game.MaskSystem.Editor
{
    /// <summary>
    /// 快速创建游戏场景的编辑器工具
    /// </summary>
    public class QuickStartSceneCreator : EditorWindow
    {
        [MenuItem("MaskSystem/快速开始游戏 _F5", false, 0)]
        public static void QuickStartGame()
        {
            // 如果正在运行，先停止
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // 创建并保存场景
            CreateQuickStartScene();

            // 开始游戏
            EditorApplication.isPlaying = true;
        }

        [MenuItem("MaskSystem/快速开始节奏游戏 _F6", false, 1)]
        public static void QuickStartRhythmGame()
        {
            // 如果正在运行，先停止
            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
                return;
            }

            // 创建节奏战斗场景
            CreateRhythmBattleScene();

            // 开始游戏
            EditorApplication.isPlaying = true;
        }

        [MenuItem("MaskSystem/创建快速开始场景", false, 10)]
        public static void CreateQuickStartScene()
        {
            // 询问是否保存当前场景
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建游戏管理器对象
            GameObject gameManager = new GameObject("GameManager");
            SimpleBattleScene battleScene = gameManager.AddComponent<SimpleBattleScene>();

            // 保存场景
            string scenePath = "Assets/Scenes/QuickStartScene.unity";
            EnsureDirectoryExists("Assets/Scenes");
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"[QuickStartSceneCreator] 快速开始场景已创建: {scenePath}");
            Debug.Log("[QuickStartSceneCreator] 按 F5 或点击菜单 'MaskSystem/快速开始游戏' 来运行游戏");
        }

        [MenuItem("MaskSystem/创建节奏战斗场景", false, 11)]
        public static void CreateRhythmBattleScene()
        {
            // 询问是否保存当前场景
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            // 创建新场景
            Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 创建节奏战斗管理器对象
            GameObject gameManager = new GameObject("RhythmGameManager");
            RhythmBattleScene rhythmScene = gameManager.AddComponent<RhythmBattleScene>();

            // 保存场景
            string scenePath = "Assets/Scenes/RhythmBattleScene.unity";
            EnsureDirectoryExists("Assets/Scenes");
            EditorSceneManager.SaveScene(newScene, scenePath);

            Debug.Log($"[QuickStartSceneCreator] 节奏战斗场景已创建: {scenePath}");
            Debug.Log("[QuickStartSceneCreator] 按 F6 或点击菜单 'MaskSystem/快速开始节奏游戏' 来运行游戏");
        }

        [MenuItem("MaskSystem/打开快速开始场景", false, 20)]
        public static void OpenQuickStartScene()
        {
            string scenePath = "Assets/Scenes/QuickStartScene.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                if (EditorUtility.DisplayDialog("场景不存在", "快速开始场景不存在，是否创建？", "创建", "取消"))
                {
                    CreateQuickStartScene();
                }
            }
        }

        [MenuItem("MaskSystem/打开节奏战斗场景", false, 21)]
        public static void OpenRhythmBattleScene()
        {
            string scenePath = "Assets/Scenes/RhythmBattleScene.unity";
            if (System.IO.File.Exists(scenePath))
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                if (EditorUtility.DisplayDialog("场景不存在", "节奏战斗场景不存在，是否创建？", "创建", "取消"))
                {
                    CreateRhythmBattleScene();
                }
            }
        }

        [MenuItem("MaskSystem/游戏操作说明", false, 100)]
        public static void ShowGameInstructions()
        {
            EditorUtility.DisplayDialog("游戏操作说明",
                "【节奏模式操作】\n" +
                "空格键 - 卡点判定（保持当前面具）\n" +
                "Q/W/E - 切换面具并卡点判定\n" +
                "R - 重新开始游戏\n" +
                "P - 暂停/继续游戏\n" +
                "B - 手动开始节奏战斗\n\n" +
                "【调试功能】\n" +
                "N - 跳过当前敌人\n" +
                "1/2/3 - 直接跳转到对应关卡\n\n" +
                "【节奏战斗规则】\n" +
                "• 音符从右向左滚动\n" +
                "• 在音符到达判定线时按键\n" +
                "• Perfect(完美): ±50ms，敌人受伤\n" +
                "• Normal(普通): ±150ms，双方都受伤\n" +
                "• Miss(失误): 玩家受伤\n" +
                "• 连续3次Perfect触发追加攻击！\n" +
                "• 不同面具有不同效果（攻击/闪避/格挡/回血）",
                "知道了");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string newPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = newPath;
                }
            }
        }
    }

    /// <summary>
    /// 初始化设置和自动化工具
    /// </summary>
    public static class MaskSystemSetup
    {
        [MenuItem("MaskSystem/初始化系统配置", false, 50)]
        public static void InitializeSystem()
        {
            // 确保 Resources 文件夹存在
            EnsureDirectoryExists("Assets/Resources");

            // 创建 MaskSystemConfig
            CreateMaskSystemConfigIfNeeded();

            Debug.Log("[MaskSystemSetup] 系统配置初始化完成！");
            EditorUtility.DisplayDialog("初始化完成",
                "MaskSystem 配置已初始化！\n\n" +
                "现在可以使用 F5 或菜单 'MaskSystem/快速开始游戏' 来启动游戏。",
                "确定");
        }

        private static void CreateMaskSystemConfigIfNeeded()
        {
            string configPath = "Assets/Resources/MaskSystemConfig.asset";

            if (AssetDatabase.LoadAssetAtPath<Game.MaskSystem.MaskSystemConfig>(configPath) != null)
            {
                Debug.Log("[MaskSystemSetup] MaskSystemConfig 已存在");
                return;
            }

            // 使用默认配置创建
            var config = Game.MaskSystem.MaskSystemConfig.CreateDefault();

            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            Debug.Log("[MaskSystemSetup] 已创建 MaskSystemConfig");
        }

        private static void EnsureDirectoryExists(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] parts = path.Split('/');
                string currentPath = parts[0];
                for (int i = 1; i < parts.Length; i++)
                {
                    string newPath = currentPath + "/" + parts[i];
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, parts[i]);
                    }
                    currentPath = newPath;
                }
            }
        }
    }
}

