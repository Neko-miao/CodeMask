// ================================================
// MaskSystem - 关卡测试场景
// ================================================

using System.Text;
using UnityEngine;

namespace Game.MaskSystem
{
    /// <summary>
    /// 关卡测试场景组件 - 可玩的关卡测试
    /// </summary>
    public class LevelTestScene : MonoBehaviour
    {
        #region 配置

        [Header("游戏模式")]
        [Tooltip("启用战役模式（连续三关卡）")]
        [SerializeField] private bool campaignMode = true;

        [Header("关卡配置（单关卡模式）")]
        [Tooltip("关卡配置文件（可选，留空使用默认关卡）")]
        [SerializeField] private LevelConfig levelConfig;

        [Tooltip("使用预设关卡")]
        [SerializeField] private PresetLevel presetLevel = PresetLevel.Level1_HappyForest;

        [Header("调试选项")]
        [Tooltip("启用控制台日志")]
        [SerializeField] private bool enableLogging = true;

        [Tooltip("自动开始")]
        [SerializeField] private bool autoStart = true;

        #endregion

        #region 枚举

        public enum PresetLevel
        {
            Custom,           // 使用自定义配置
            Level1_HappyForest,
            Level2_DeepSea,
            Level3_Sky,
            QuickTest         // 快速测试（单个蛇敌人）
        }

        #endregion

        #region 私有字段

        private CampaignManager _campaignManager;
        private LevelManager _levelManager;
        private IMaskSystemAPI _api;
        private StringBuilder _logBuilder = new StringBuilder();
        private Vector2 _logScrollPos;
        private const int MAX_LOG_LINES = 15;
        private int _logLineCount = 0;

        // UI区域
        private Rect _statusRect = new Rect(10, 10, 450, 220);
        private Rect _battleRect = new Rect(10, 240, 450, 180);
        private Rect _controlRect = new Rect(10, 430, 450, 150);
        private Rect _logRect = new Rect(10, 590, 450, 180);

        // 预警显示
        private float _warningFlashTime;
        private bool _showWarningFlash;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 创建API
            _api = MaskSystemFacade.CreateNew();

            if (campaignMode)
            {
                // 战役模式：创建战役管理器
                _campaignManager = new CampaignManager(_api);
                _levelManager = _campaignManager.LevelManager;
            }
            else
            {
                // 单关卡模式：创建关卡管理器
                _levelManager = new LevelManager(_api);
            }

            // 订阅事件
            SubscribeEvents();

            Log(campaignMode ? "战役测试场景初始化完成" : "关卡测试场景初始化完成");
        }

        private void Start()
        {
            if (campaignMode)
            {
                // 战役模式：使用默认三关卡
                _campaignManager.UseDefaultCampaign();
                Log("加载默认战役: 快乐森林 -> 深海 -> 天空");

                if (autoStart)
                {
                    _campaignManager.StartCampaign();
                    Log("战役自动开始");
                }
            }
            else
            {
                // 单关卡模式
                LevelConfig config = GetLevelConfig();
                _levelManager.LoadLevel(config);
                Log($"加载关卡: {config.LevelName}");

                if (autoStart)
                {
                    _levelManager.StartLevel();
                    Log("关卡自动开始");
                }
            }
        }

        private void Update()
        {
            // 更新
            if (campaignMode)
            {
                _campaignManager?.Update(Time.deltaTime);
            }
            else
            {
                _levelManager?.Update(Time.deltaTime);
            }

            // 处理输入
            HandleInput();

            // 更新预警闪烁
            UpdateWarningFlash();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            
            if (campaignMode)
            {
                _campaignManager?.Dispose();
            }
            else
            {
                _levelManager?.Dispose();
            }
            
            (_api as MaskSystemFacade)?.Dispose();
        }

        private void OnGUI()
        {
            DrawStatusPanel();
            DrawBattlePanel();
            DrawControlPanel();
            DrawLogPanel();
        }

        #endregion

        #region 输入处理

        private void HandleInput()
        {
            // Space - 反击/攻击
            if (Input.GetKeyDown(KeyCode.Space))
            {
                bool success;
                if (campaignMode)
                {
                    success = _campaignManager.TryCounter();
                }
                else
                {
                    success = _levelManager.State == LevelState.Playing && _levelManager.TryCounter();
                }

                if (success)
                {
                    Log("反击成功!");
                }
                else
                {
                    Log("反击失败 - 不在反击窗口内");
                }
            }

            // Q/W/E - 切换面具
            if (Input.GetKeyDown(KeyCode.Q))
            {
                if (campaignMode) _campaignManager.SwitchMask(0);
                else _levelManager.SwitchMask(0);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                if (campaignMode) _campaignManager.SwitchMask(1);
                else _levelManager.SwitchMask(1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                if (campaignMode) _campaignManager.SwitchMask(2);
                else _levelManager.SwitchMask(2);
            }

            // R - 重新开始
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (campaignMode)
                {
                    // 战役模式：根据状态决定重新开始当前关卡还是整个战役
                    if (_campaignManager.State == CampaignState.GameOver || 
                        _campaignManager.State == CampaignState.GameComplete)
                    {
                        _campaignManager.RestartCampaign();
                        Log("重新开始战役");
                    }
                    else
                    {
                        _campaignManager.RestartCurrentLevel();
                        Log("重新开始当前关卡");
                    }
                }
                else
                {
                    _levelManager.Restart();
                    Log("重新开始关卡");
                }
            }

            // P - 暂停/恢复
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_levelManager.AutoBattle?.IsPaused ?? false)
                {
                    if (campaignMode) _campaignManager.Resume();
                    else _levelManager.Resume();
                    Log("恢复");
                }
                else
                {
                    if (campaignMode) _campaignManager.Pause();
                    else _levelManager.Pause();
                    Log("暂停");
                }
            }

            // 数字键 - 切换关卡（仅单关卡模式）
            if (!campaignMode)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1))
                {
                    LoadAndStartPreset(PresetLevel.Level1_HappyForest);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    LoadAndStartPreset(PresetLevel.Level2_DeepSea);
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    LoadAndStartPreset(PresetLevel.Level3_Sky);
                }
            }
            
            // 战役模式：N - 跳过当前关卡（调试用）
            if (campaignMode && Input.GetKeyDown(KeyCode.N))
            {
                _api.DefeatCurrentEnemy();
                Log("调试: 跳过当前敌人");
            }
        }

        #endregion

        #region UI绘制

        private void DrawStatusPanel()
        {
            GUI.Box(_statusRect, "");
            GUILayout.BeginArea(_statusRect);

            if (campaignMode)
            {
                GUILayout.Label("<color=yellow><b>=== 战役模式 ===</b></color>", CreateRichTextStyle());
                GUILayout.Label($"战役进度: <color=cyan>关卡 {_campaignManager.CurrentLevelIndex + 1} / {_campaignManager.TotalLevels}</color>", CreateRichTextStyle());
                GUILayout.Label($"战役状态: <color=lime>{_campaignManager.State}</color>", CreateRichTextStyle());
            }
            else
            {
                GUILayout.Label("<color=yellow><b>=== 关卡状态 ===</b></color>", CreateRichTextStyle());
            }

            var level = _levelManager.CurrentLevel;
            var state = _levelManager.State;

            GUILayout.Label($"当前关卡: <color=cyan>{level?.LevelName ?? "无"}</color>", CreateRichTextStyle());
            GUILayout.Label($"关卡状态: <color=lime>{state}</color>", CreateRichTextStyle());
            GUILayout.Label($"波次: {_levelManager.CurrentWaveIndex + 1} / {_levelManager.TotalWaves}");

            GUILayout.Space(10);

            // 玩家状态
            GUILayout.Label("<color=cyan>【玩家】</color>", CreateRichTextStyle());
            int hp = _api.GetPlayerHealth();
            int maxHp = _api.GetPlayerMaxHealth();
            string hpBar = GetHealthBar(hp, maxHp);
            GUILayout.Label($"  血量: {hpBar} {hp}/{maxHp}");
            GUILayout.Label($"  面具: <color=lime>{_api.GetCurrentMask()}</color> (槽位 {_api.GetCurrentSlot()})", CreateRichTextStyle());

            var masks = _api.GetOwnedMasks();
            string maskList = masks.Count > 0 ? string.Join(", ", masks) : "无";
            GUILayout.Label($"  拥有: [{maskList}]");

            GUILayout.Space(5);

            // 敌人状态
            GUILayout.Label("<color=red>【敌人】</color>", CreateRichTextStyle());
            if (_api.HasEnemy)
            {
                int enemyHp = _api.GetEnemyHealth();
                int enemyMaxHp = _api.GetEnemyMaxHealth();
                string enemyHpBar = GetHealthBar(enemyHp, enemyMaxHp);
                GUILayout.Label($"  {_api.GetEnemyName()} [{_api.GetEnemyMask()}]");
                GUILayout.Label($"  血量: {enemyHpBar} {enemyHp}/{enemyMaxHp}");
            }
            else
            {
                GUILayout.Label("  无敌人");
            }

            GUILayout.EndArea();
        }

        private void DrawBattlePanel()
        {
            // 预警时使用红色背景
            Color bgColor = _showWarningFlash ? new Color(0.8f, 0.2f, 0.2f, 0.5f) : GUI.backgroundColor;
            GUI.backgroundColor = bgColor;
            GUI.Box(_battleRect, "");
            GUI.backgroundColor = Color.white;

            GUILayout.BeginArea(_battleRect);

            GUILayout.Label("<color=yellow><b>=== 战斗 ===</b></color>", CreateRichTextStyle());

            var autoBattle = _levelManager.AutoBattle;
            if (autoBattle != null && _levelManager.State == LevelState.Playing)
            {
                GUILayout.Label($"阶段: <color=cyan>{autoBattle.CurrentPhase}</color>", CreateRichTextStyle());

                switch (autoBattle.CurrentPhase)
                {
                    case BattlePhase.Idle:
                        GUILayout.Label($"下次攻击: {autoBattle.NextAttackTimer:F1}秒");
                        DrawProgressBar(1 - (autoBattle.NextAttackTimer / 3f), "等待中...", Color.gray);
                        break;

                    case BattlePhase.Warning:
                        GUILayout.Label($"<color=red><b>!! 攻击预警 !!</b></color>", CreateRichTextStyle());
                        GUILayout.Label($"反击窗口: {autoBattle.WarningTimer:F2}秒");
                        DrawProgressBar(autoBattle.WarningProgress, "按 SPACE 反击!", Color.red);
                        GUILayout.Label("<color=yellow><size=20>>>> 按 SPACE 反击! <<<</size></color>", CreateRichTextStyle());
                        break;

                    case BattlePhase.Cooldown:
                        GUILayout.Label("冷却中...");
                        break;
                }
            }
            else if (_levelManager.State == LevelState.Preparing)
            {
                GUILayout.Label($"准备中... {_levelManager.TransitionTimer:F1}秒");
                DrawProgressBar(1 - (_levelManager.TransitionTimer / (_levelManager.CurrentLevel?.PrepareTime ?? 2f)), "准备中", Color.yellow);
            }
            else if (_levelManager.State == LevelState.WaveTransition)
            {
                GUILayout.Label($"下一波即将开始... {_levelManager.TransitionTimer:F1}秒");
                DrawProgressBar(1 - (_levelManager.TransitionTimer / (_levelManager.CurrentLevel?.WaveInterval ?? 1.5f)), "波次切换", Color.cyan);
            }
            else if (_levelManager.State == LevelState.Victory)
            {
                if (campaignMode && _campaignManager.State == CampaignState.LevelTransition)
                {
                    GUILayout.Label("<color=lime><size=20>*** 关卡完成! ***</size></color>", CreateRichTextStyle());
                    GUILayout.Label($"下一关倒计时: {_campaignManager.LevelTransitionTimer:F1}秒");
                    DrawProgressBar(1 - (_campaignManager.LevelTransitionTimer / _campaignManager.LevelTransitionTime), "进入下一关...", Color.cyan);
                }
                else if (campaignMode && _campaignManager.State == CampaignState.GameComplete)
                {
                    GUILayout.Label("<color=lime><size=24>*** 恭喜通关! ***</size></color>", CreateRichTextStyle());
                    GUILayout.Label("<color=yellow>你成功击败了所有敌人!</color>", CreateRichTextStyle());
                    GUILayout.Label("按 R 重新挑战");
                }
                else
                {
                    GUILayout.Label("<color=lime><size=24>*** 胜利! ***</size></color>", CreateRichTextStyle());
                    GUILayout.Label("按 R 重新开始");
                }
            }
            else if (_levelManager.State == LevelState.Defeat)
            {
                if (campaignMode)
                {
                    GUILayout.Label("<color=red><size=24>*** 游戏结束 ***</size></color>", CreateRichTextStyle());
                    GUILayout.Label($"止步于: 关卡 {_campaignManager.CurrentLevelIndex + 1} - {_levelManager.CurrentLevel?.LevelName}");
                }
                else
                {
                    GUILayout.Label("<color=red><size=24>*** 失败 ***</size></color>", CreateRichTextStyle());
                }
                GUILayout.Label("按 R 重新开始");
            }
            else
            {
                GUILayout.Label("等待开始...");
            }

            GUILayout.EndArea();
        }

        private void DrawControlPanel()
        {
            GUI.Box(_controlRect, "");
            GUILayout.BeginArea(_controlRect);

            GUILayout.Label("<color=yellow><b>=== 操作 ===</b></color>", CreateRichTextStyle());
            GUILayout.Label("Space - 反击  |  Q/W/E - 切换面具");
            GUILayout.Label("R - 重新开始  |  P - 暂停/恢复");
            
            if (campaignMode)
            {
                GUILayout.Label("N - 跳过当前敌人（调试）");
            }
            else
            {
                GUILayout.Label("1/2/3 - 切换关卡（森林/深海/天空）");
            }

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("反击 (Space)", GUILayout.Height(35)))
            {
                if (campaignMode) _campaignManager.TryCounter();
                else _levelManager.TryCounter();
            }
            if (GUILayout.Button("重新开始 (R)", GUILayout.Height(35)))
            {
                if (campaignMode) _campaignManager.RestartCampaign();
                else _levelManager.Restart();
            }
            GUILayout.EndHorizontal();

            if (campaignMode)
            {
                // 战役模式按钮
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("重新开始战役", GUILayout.Height(30)))
                {
                    _campaignManager.RestartCampaign();
                    Log("重新开始战役");
                }
                if (GUILayout.Button("跳过敌人 (N)", GUILayout.Height(30)))
                {
                    _api.DefeatCurrentEnemy();
                }
                GUILayout.EndHorizontal();
            }
            else
            {
                // 单关卡模式按钮
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("关卡1:森林", GUILayout.Height(30)))
                {
                    LoadAndStartPreset(PresetLevel.Level1_HappyForest);
                }
                if (GUILayout.Button("关卡2:深海", GUILayout.Height(30)))
                {
                    LoadAndStartPreset(PresetLevel.Level2_DeepSea);
                }
                if (GUILayout.Button("关卡3:天空", GUILayout.Height(30)))
                {
                    LoadAndStartPreset(PresetLevel.Level3_Sky);
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.EndArea();
        }

        private void DrawLogPanel()
        {
            GUI.Box(_logRect, "");
            GUILayout.BeginArea(_logRect);

            GUILayout.Label("<color=yellow><b>=== 日志 ===</b></color>", CreateRichTextStyle());

            _logScrollPos = GUILayout.BeginScrollView(_logScrollPos, GUILayout.Height(140));
            GUILayout.Label(_logBuilder.ToString(), CreateRichTextStyle());
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        private void DrawProgressBar(float progress, string label, Color color)
        {
            Rect rect = GUILayoutUtility.GetRect(0, 25, GUILayout.ExpandWidth(true));
            rect.width -= 10;

            // 背景
            GUI.color = Color.gray;
            GUI.Box(rect, "");

            // 进度条
            GUI.color = color;
            Rect progressRect = rect;
            progressRect.width *= Mathf.Clamp01(progress);
            GUI.Box(progressRect, "");

            // 标签
            GUI.color = Color.white;
            GUI.Label(rect, label, new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter });
        }

        #endregion

        #region 事件订阅

        private void SubscribeEvents()
        {
            // 关卡管理器事件
            _levelManager.OnStateChanged += OnStateChanged;
            _levelManager.OnWaveStart += OnWaveStart;
            _levelManager.OnWaveComplete += OnWaveComplete;
            _levelManager.OnLevelVictory += OnLevelVictory;
            _levelManager.OnLevelDefeat += OnLevelDefeat;

            _levelManager.AutoBattle.OnWarningStart += OnWarningStart;
            _levelManager.AutoBattle.OnEnemyAttack += OnEnemyAttack;
            _levelManager.AutoBattle.OnPlayerCounter += OnPlayerCounter;
            _levelManager.AutoBattle.OnCounterFailed += OnCounterFailed;

            // API事件
            _api.OnMaskChanged += OnMaskChanged;
            _api.OnMaskAcquired += OnMaskAcquired;

            // 战役事件
            if (campaignMode && _campaignManager != null)
            {
                _campaignManager.OnCampaignStateChanged += OnCampaignStateChanged;
                _campaignManager.OnLevelStart += OnCampaignLevelStart;
                _campaignManager.OnLevelComplete += OnCampaignLevelComplete;
                _campaignManager.OnGameComplete += OnGameComplete;
                _campaignManager.OnGameOver += OnGameOver;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_levelManager != null)
            {
                _levelManager.OnStateChanged -= OnStateChanged;
                _levelManager.OnWaveStart -= OnWaveStart;
                _levelManager.OnWaveComplete -= OnWaveComplete;
                _levelManager.OnLevelVictory -= OnLevelVictory;
                _levelManager.OnLevelDefeat -= OnLevelDefeat;

                if (_levelManager.AutoBattle != null)
                {
                    _levelManager.AutoBattle.OnWarningStart -= OnWarningStart;
                    _levelManager.AutoBattle.OnEnemyAttack -= OnEnemyAttack;
                    _levelManager.AutoBattle.OnPlayerCounter -= OnPlayerCounter;
                    _levelManager.AutoBattle.OnCounterFailed -= OnCounterFailed;
                }
            }

            if (_api != null)
            {
                _api.OnMaskChanged -= OnMaskChanged;
                _api.OnMaskAcquired -= OnMaskAcquired;
            }

            if (_campaignManager != null)
            {
                _campaignManager.OnCampaignStateChanged -= OnCampaignStateChanged;
                _campaignManager.OnLevelStart -= OnCampaignLevelStart;
                _campaignManager.OnLevelComplete -= OnCampaignLevelComplete;
                _campaignManager.OnGameComplete -= OnGameComplete;
                _campaignManager.OnGameOver -= OnGameOver;
            }
        }

        #endregion

        #region 事件处理

        private void OnStateChanged(LevelState oldState, LevelState newState)
        {
            Log($"状态变化: {oldState} -> {newState}");
        }

        private void OnWaveStart(int waveIndex, WaveConfig wave)
        {
            Log($"<color=cyan>波次 {waveIndex + 1} 开始: {wave.GetDisplayName()}</color>");
        }

        private void OnWaveComplete(int waveIndex)
        {
            Log($"<color=lime>波次 {waveIndex + 1} 完成!</color>");
        }

        private void OnLevelVictory()
        {
            Log("<color=lime>*** 关卡胜利! ***</color>");
        }

        private void OnLevelDefeat()
        {
            Log("<color=red>*** 关卡失败 ***</color>");
        }

        private void OnWarningStart()
        {
            _showWarningFlash = true;
            _warningFlashTime = 0;
            Log("<color=red>!! 敌人即将攻击 - 按Space反击 !!</color>");
        }

        private void OnEnemyAttack(CombatResult result)
        {
            _showWarningFlash = false;
            Log($"<color=red>敌人攻击! 伤害: {result.Damage}</color>");
        }

        private void OnPlayerCounter(CombatResult result)
        {
            _showWarningFlash = false;
            string counterInfo = result.IsCounter ? " (克制!)" : "";
            Log($"<color=lime>反击成功! 伤害: {result.Damage}{counterInfo}</color>");
        }

        private void OnCounterFailed()
        {
            Log("<color=yellow>反击失败 - 时机不对</color>");
        }

        private void OnMaskChanged(MaskType oldMask, MaskType newMask)
        {
            Log($"面具切换: {oldMask} -> {newMask}");
        }

        private void OnMaskAcquired(MaskType mask)
        {
            Log($"<color=cyan>获得面具: {mask}</color>");
        }

        // 战役事件
        private void OnCampaignStateChanged(CampaignState oldState, CampaignState newState)
        {
            Log($"战役状态: {oldState} -> {newState}");
        }

        private void OnCampaignLevelStart(int levelIndex, LevelConfig level)
        {
            Log($"<color=yellow>===== 开始关卡 {levelIndex + 1}: {level.LevelName} =====</color>");
        }

        private void OnCampaignLevelComplete(int levelIndex, LevelConfig level)
        {
            Log($"<color=lime>关卡 {levelIndex + 1} 完成: {level.LevelName}</color>");
        }

        private void OnGameComplete()
        {
            Log("<color=lime><b>*** 恭喜通关! 所有关卡完成! ***</b></color>");
        }

        private void OnGameOver()
        {
            Log("<color=red><b>*** 游戏结束 ***</b></color>");
        }

        #endregion

        #region 辅助方法

        private LevelConfig GetLevelConfig()
        {
            if (levelConfig != null && presetLevel == PresetLevel.Custom)
            {
                return levelConfig;
            }

            switch (presetLevel)
            {
                case PresetLevel.Level1_HappyForest:
                    return LevelConfig.CreateLevel1_HappyForest();
                case PresetLevel.Level2_DeepSea:
                    return LevelConfig.CreateLevel2_DeepSea();
                case PresetLevel.Level3_Sky:
                    return LevelConfig.CreateLevel3_Sky();
                case PresetLevel.QuickTest:
                    return LevelConfig.CreateWithEnemies("快速测试", MaskType.Snake);
                default:
                    return LevelConfig.CreateDefault();
            }
        }

        private void LoadAndStartPreset(PresetLevel preset)
        {
            LevelConfig config;
            switch (preset)
            {
                case PresetLevel.Level1_HappyForest:
                    config = LevelConfig.CreateLevel1_HappyForest();
                    break;
                case PresetLevel.Level2_DeepSea:
                    config = LevelConfig.CreateLevel2_DeepSea();
                    break;
                case PresetLevel.Level3_Sky:
                    config = LevelConfig.CreateLevel3_Sky();
                    break;
                default:
                    config = LevelConfig.CreateDefault();
                    break;
            }

            _levelManager.QuickStart(config);
            Log($"加载并开始关卡: {config.LevelName}");
        }

        private void UpdateWarningFlash()
        {
            if (_showWarningFlash)
            {
                _warningFlashTime += Time.deltaTime;
                // 闪烁效果
                _showWarningFlash = Mathf.Sin(_warningFlashTime * 10) > 0;
            }
        }

        private string GetHealthBar(int current, int max)
        {
            int filled = Mathf.RoundToInt((float)current / max * 10);
            int empty = 10 - filled;
            return "[" + new string('█', filled) + new string('░', empty) + "]";
        }

        private GUIStyle CreateRichTextStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.fontSize = 14;
            return style;
        }

        private void Log(string message)
        {
            if (!enableLogging) return;

            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            _logBuilder.Insert(0, $"[{timestamp}] {message}\n");

            _logLineCount++;
            if (_logLineCount > MAX_LOG_LINES)
            {
                int lastNewline = _logBuilder.ToString().LastIndexOf('\n', _logBuilder.Length - 2);
                if (lastNewline > 0)
                {
                    _logBuilder.Remove(lastNewline, _logBuilder.Length - lastNewline);
                }
                _logLineCount--;
            }

            Debug.Log($"[LevelTest] {message}");
        }

        #endregion
    }
}

