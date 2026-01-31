// ================================================
// MaskSystem - 节奏战斗场景
// 集成节奏系统的完整战斗场景
// ================================================

using UnityEngine;
using System.Collections.Generic;
using Game.MaskSystem.Rhythm.Visual;
using Game.MaskSystem.Visual.Placeholder;

namespace Game.MaskSystem.Rhythm
{
    /// <summary>
    /// 节奏战斗场景 - 使用节奏判定系统的战斗场景
    /// </summary>
    public class RhythmBattleScene : MonoBehaviour
    {
        [Header("游戏设置")]
        [SerializeField] private bool campaignMode = true;
        [SerializeField] private int startLevelIndex = 0;

        [Header("节奏设置")]
        [SerializeField] private float bpm = 120f;
        [SerializeField] private int notesPerWave = 12;

        [Header("视觉设置")]
        [SerializeField] private float characterScale = 2f;
        [SerializeField] private float playerXPosition = -3f;
        [SerializeField] private float enemyXPosition = 3f;
        [SerializeField] private float trackYPosition = -3f;

        // 系统引用
        private IMaskSystemAPI _api;
        private CampaignManager _campaignManager;
        private LevelManager _levelManager => _campaignManager?.LevelManager;
        private RhythmManager _rhythmManager;
        private RhythmCombatBridge _combatBridge;
        private RhythmVisualManager _visualManager;
        private List<LevelConfig> _campaignLevels;

        // 视觉对象
        private Camera _mainCamera;
        private GameObject _backgroundObject;
        private MeshRenderer _backgroundRenderer;
        private Material _backgroundMaterial;
        private GameObject _playerObject;
        private GameObject _enemyObject;
        private SpriteRenderer _playerSprite;
        private SpriteRenderer _enemySprite;

        // 状态
        private string _currentLevelName = "";
        private MaskType _currentEnemyMask = MaskType.None;
        private string _lastMessage = "";
        private float _messageTimer = 0f;
        private bool _rhythmBattleActive = false;

        // 动画状态
        private Vector3 _playerBasePos;
        private Vector3 _enemyBasePos;
        private float _playerAnimTimer = 0f;
        private float _enemyAnimTimer = 0f;
        private bool _playerHit = false;
        private bool _enemyHit = false;

        // 战斗效果显示
        private CombatEffectResult? _lastCombatEffect;
        private float _combatEffectTimer = 0f;

        #region 生命周期

        void Awake()
        {
            InitializeSystem();
            CreateVisualObjects();
            SetupCamera();
            SetupRhythmVisualManager();
        }

        void Start()
        {
            if (campaignMode)
            {
                if (startLevelIndex > 0 && startLevelIndex < _campaignLevels.Count)
                {
                    _campaignManager.StartFromLevel(startLevelIndex);
                }
                else
                {
                    _campaignManager.StartCampaign();
                }
            }
        }

        void Update()
        {
            // 更新战役/关卡
            if (campaignMode)
            {
                _campaignManager?.Update(Time.deltaTime);
            }
            else
            {
                _levelManager?.Update(Time.deltaTime);
            }

            // 更新节奏系统
            if (_rhythmBattleActive && _rhythmManager.IsRunning)
            {
                _rhythmManager.Update(Time.deltaTime);
            }

            // 处理输入
            HandleInput();

            // 更新视觉
            UpdateVisuals();
            UpdateAnimations();

            // 更新计时器
            if (_messageTimer > 0) _messageTimer -= Time.deltaTime;
            if (_combatEffectTimer > 0) _combatEffectTimer -= Time.deltaTime;
        }

        void OnGUI()
        {
            DrawWorldSpaceUI();
            DrawRhythmUI();
            DrawMainUI();
        }

        void OnDestroy()
        {
            _combatBridge?.Dispose();
            _campaignManager?.Dispose();
            
            if (_backgroundMaterial != null)
                Destroy(_backgroundMaterial);
        }

        #endregion

        #region 初始化

        private void InitializeSystem()
        {
            _api = MaskSystemFacade.Instance;

            // 创建关卡配置
            _campaignLevels = new List<LevelConfig>
            {
                LevelConfig.CreateLevel1_HappyForest(),
                LevelConfig.CreateLevel2_DeepSea(),
                LevelConfig.CreateLevel3_Sky()
            };

            // 初始化战役管理器
            _campaignManager = new CampaignManager(_api);
            _campaignManager.SetLevels(_campaignLevels.ToArray());

            // 初始化节奏系统
            InitializeRhythmSystem();

            // 订阅事件
            SubscribeEvents();
        }

        private void InitializeRhythmSystem()
        {
            _rhythmManager = new RhythmManager();
            
            // 设置默认配置
            var config = RhythmConfig.CreateDefault();
            config.BPM = bpm;
            _rhythmManager.SetConfig(config);

            // 设置轨道参数
            _rhythmManager.Track.TrackY = trackYPosition;
            _rhythmManager.Track.JudgeLineX = playerXPosition + 1f;
            _rhythmManager.Track.SpawnX = enemyXPosition + 2f;

            // 获取战斗管理器并创建桥接
            var combat = (_api as MaskSystemFacade)?.GetCombatManager();
            if (combat != null)
            {
                _combatBridge = new RhythmCombatBridge(combat, _rhythmManager);
                _combatBridge.OnCombatEffect += OnCombatEffect;
                _combatBridge.OnTriplePerfectAttack += OnTriplePerfectAttack;
                _combatBridge.OnBlockCounterReady += OnBlockCounterReady;
                _combatBridge.OnFirstSwitchBonus += OnFirstSwitchBonus;
            }

            // 订阅节奏事件
            _rhythmManager.OnJudge += OnRhythmJudge;
            _rhythmManager.OnAllNotesComplete += OnWaveNotesComplete;

            Debug.Log("[RhythmBattleScene] 节奏系统初始化完成");
        }

        private void SubscribeEvents()
        {
            _campaignManager.OnGameComplete += () => ShowMessage("恭喜通关！按R重新开始");
            _campaignManager.OnGameOver += () => ShowMessage("游戏结束！按R重新开始");
            _campaignManager.OnLevelStart += OnLevelStart;

            _api.OnMaskAcquired += (mask) => ShowMessage($"获得新面具: {GetMaskName(mask)}!");
            _api.OnPlayerDefeated += () => { ShowMessage("你被击败了!"); StopRhythmBattle(); };
            _api.OnEnemyDefeated += OnEnemyDefeated;
        }

        private void SetupRhythmVisualManager()
        {
            _visualManager = gameObject.AddComponent<RhythmVisualManager>();
            _visualManager.TrackY = trackYPosition;
            _visualManager.JudgeLineX = playerXPosition + 1f;
            _visualManager.SpawnX = enemyXPosition + 2f;
            _visualManager.Initialize(_rhythmManager);
        }

        private void CreateVisualObjects()
        {
            // 创建程序化背景
            CreateProceduralBackground();

            // 创建玩家
            _playerObject = new GameObject("Player");
            _playerObject.transform.SetParent(transform);
            _playerObject.transform.position = new Vector3(playerXPosition, 0, 0);
            _playerSprite = _playerObject.AddComponent<SpriteRenderer>();
            _playerSprite.sortingOrder = 10;
            _playerObject.transform.localScale = Vector3.one * characterScale;
            _playerBasePos = _playerObject.transform.position;

            // 创建敌人
            _enemyObject = new GameObject("Enemy");
            _enemyObject.transform.SetParent(transform);
            _enemyObject.transform.position = new Vector3(enemyXPosition, 0, 0);
            _enemySprite = _enemyObject.AddComponent<SpriteRenderer>();
            _enemySprite.sortingOrder = 10;
            _enemyObject.transform.localScale = Vector3.one * characterScale;
            _enemyBasePos = _enemyObject.transform.position;

            UpdatePlayerVisual();
        }

        private void CreateProceduralBackground()
        {
            _backgroundObject = new GameObject("ProceduralBackground");
            _backgroundObject.transform.SetParent(transform);
            _backgroundObject.transform.position = new Vector3(0, 0, 10);
            
            MeshFilter meshFilter = _backgroundObject.AddComponent<MeshFilter>();
            _backgroundRenderer = _backgroundObject.AddComponent<MeshRenderer>();
            
            Mesh quadMesh = new Mesh();
            float height = 12f;
            float width = height * (16f / 9f);
            
            quadMesh.vertices = new Vector3[]
            {
                new Vector3(-width / 2, -height / 2, 0),
                new Vector3(width / 2, -height / 2, 0),
                new Vector3(-width / 2, height / 2, 0),
                new Vector3(width / 2, height / 2, 0)
            };
            quadMesh.uv = new Vector2[] { new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 1), new Vector2(1, 1) };
            quadMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            quadMesh.RecalculateNormals();
            meshFilter.mesh = quadMesh;
            
            Shader bgShader = Shader.Find("MaskSystem/ProceduralBackground");
            if (bgShader == null) bgShader = Shader.Find("Unlit/Color");
            
            _backgroundMaterial = new Material(bgShader);
            _backgroundRenderer.material = _backgroundMaterial;
            _backgroundRenderer.sortingOrder = -100;
            
            SetBackgroundTheme("快乐森林");
        }

        private void SetupCamera()
        {
            _mainCamera = Camera.main;
            if (_mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                _mainCamera = camObj.AddComponent<Camera>();
                _mainCamera.tag = "MainCamera";
            }

            _mainCamera.orthographic = true;
            _mainCamera.orthographicSize = 5;
            _mainCamera.backgroundColor = Color.black;
            _mainCamera.transform.position = new Vector3(0, 0, -10);
        }

        #endregion

        #region 输入处理

        private void HandleInput()
        {
            // 节奏输入（Space保持卡点）
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_rhythmBattleActive && _rhythmManager.IsRunning)
                {
                    var result = _rhythmManager.PlayerInput();
                    if (result.HasValue)
                    {
                        TriggerPlayerAction(result.Value.Result);
                    }
                }
                else if (_levelManager != null && _levelManager.TryCounter())
                {
                    TriggerPlayerAttack();
                }
            }

            // 切换面具（QWE同时触发节奏判定）
            if (Input.GetKeyDown(KeyCode.Q)) HandleMaskSwitchInput(0);
            if (Input.GetKeyDown(KeyCode.W)) HandleMaskSwitchInput(1);
            if (Input.GetKeyDown(KeyCode.E)) HandleMaskSwitchInput(2);

            // 重新开始
            if (Input.GetKeyDown(KeyCode.R)) RestartGame();

            // 暂停
            if (Input.GetKeyDown(KeyCode.P)) TogglePause();

            // 调试
            if (Input.GetKeyDown(KeyCode.N)) _api.DefeatCurrentEnemy();
            if (Input.GetKeyDown(KeyCode.Alpha1)) _campaignManager?.StartFromLevel(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) _campaignManager?.StartFromLevel(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) _campaignManager?.StartFromLevel(2);

            // 手动触发节奏战斗（测试用）
            if (Input.GetKeyDown(KeyCode.B)) StartRhythmBattle();
        }

        private void HandleMaskSwitchInput(int slot)
        {
            if (_rhythmBattleActive && _rhythmManager.IsRunning)
            {
                // 节奏模式下，切换面具同时触发判定
                var result = _rhythmManager.PlayerInputWithMask(true, slot);
                if (result.HasValue)
                {
                    TriggerPlayerAction(result.Value.Result);
                }
            }
            else
            {
                // 非节奏模式，直接切换
                SwitchMask(slot);
            }
        }

        private void SwitchMask(int slot)
        {
            if (_api.SwitchMask(slot))
            {
                UpdatePlayerVisual();
                ShowMessage($"切换到面具: {GetMaskName(_api.GetCurrentMask())}");
            }
        }

        private void TogglePause()
        {
            if (_rhythmManager.IsRunning)
            {
                _rhythmManager.Pause();
                ShowMessage("暂停");
            }
            else if (_rhythmBattleActive)
            {
                _rhythmManager.Resume();
                ShowMessage("继续");
            }
        }

        private void RestartGame()
        {
            StopRhythmBattle();
            _campaignManager?.RestartCampaign();
            ShowMessage("游戏重新开始!");
        }

        #endregion

        #region 节奏战斗控制

        /// <summary>
        /// 开始节奏战斗
        /// </summary>
        private void StartRhythmBattle()
        {
            if (_rhythmBattleActive || !_api.IsEnemyAlive) return;

            // 根据当前敌人生成音符序列
            var enemyMask = _api.GetEnemyMask();
            var levelConfig = _rhythmManager.Track.CompletedNotes.Count > 0 ? null 
                : GetCurrentRhythmConfig();

            float difficulty = GetCurrentDifficulty();
            var notes = _rhythmManager.GenerateRandomNotes(enemyMask, notesPerWave, difficulty);

            _rhythmManager.Start(notes);
            _combatBridge?.ResetBattleState();
            _rhythmBattleActive = true;

            ShowMessage("节奏战斗开始!");
            Debug.Log($"[RhythmBattleScene] 开始节奏战斗，敌人: {enemyMask}, 音符数: {notes.Count}");
        }

        /// <summary>
        /// 停止节奏战斗
        /// </summary>
        private void StopRhythmBattle()
        {
            _rhythmManager.Stop();
            _rhythmBattleActive = false;
            _visualManager?.ClearAll();
        }

        private LevelRhythmConfig GetCurrentRhythmConfig()
        {
            if (_levelManager?.CurrentLevel == null) return null;
            
            // 这里可以从RhythmConfig中获取对应关卡的配置
            return null;
        }

        private float GetCurrentDifficulty()
        {
            int levelIndex = _campaignManager?.CurrentLevelIndex ?? 0;
            return 0.3f + levelIndex * 0.25f; // 0.3, 0.55, 0.8
        }

        #endregion

        #region 事件处理

        private void OnLevelStart(int levelIndex, LevelConfig config)
        {
            _currentLevelName = "";
            _currentEnemyMask = MaskType.None;
            ShowMessage($"进入关卡: {config.LevelName}");
            
            // 调整BPM
            switch (levelIndex)
            {
                case 0: bpm = 100f; break;
                case 1: bpm = 110f; break;
                case 2: bpm = 130f; break;
            }
        }

        private void OnEnemyDefeated()
        {
            ShowMessage("敌人被击败!");
            TriggerEnemyHit();
            StopRhythmBattle();
        }

        private void OnRhythmJudge(JudgeResultData result)
        {
            // 视觉反馈已由RhythmVisualManager处理
            Debug.Log($"[RhythmBattleScene] 判定: {result.Result} | 偏差: {result.TimingOffset:F1}ms");
        }

        private void OnCombatEffect(CombatEffectResult effect)
        {
            _lastCombatEffect = effect;
            _combatEffectTimer = 1.5f;

            if (effect.PlayerDamage > 0)
                TriggerPlayerHit();
            if (effect.EnemyDamage > 0)
                TriggerEnemyHit();

            ShowMessage(effect.Message);
        }

        private void OnTriplePerfectAttack(int damage)
        {
            ShowMessage($"★ 三连完美! ★ 追加攻击 {damage} 伤害!");
            TriggerEnemyHit();
        }

        private void OnBlockCounterReady()
        {
            ShowMessage("格挡成功! 下一击将反击!");
        }

        private void OnFirstSwitchBonus(MaskType mask, string bonus)
        {
            ShowMessage(bonus);
        }

        private void OnWaveNotesComplete()
        {
            ShowMessage("本波节奏完成!");
            
            // 如果敌人还活着，继续下一波
            if (_api.IsEnemyAlive)
            {
                // 延迟后开始下一波
                Invoke(nameof(StartRhythmBattle), 1.5f);
            }
        }

        #endregion

        #region 视觉更新

        private void UpdateVisuals()
        {
            // 更新背景
            if (_levelManager?.CurrentLevel != null && _currentLevelName != _levelManager.CurrentLevel.LevelName)
            {
                _currentLevelName = _levelManager.CurrentLevel.LevelName;
                SetBackgroundTheme(_currentLevelName);
            }

            // 更新敌人
            if (_api.IsEnemyAlive && _currentEnemyMask != _api.GetEnemyMask())
            {
                _currentEnemyMask = _api.GetEnemyMask();
                UpdateEnemyVisual();

                // 新敌人出现时开始节奏战斗
                if (!_rhythmBattleActive)
                {
                    Invoke(nameof(StartRhythmBattle), 1f);
                }
            }

            _enemyObject.SetActive(_api.IsEnemyAlive);
        }

        private void UpdatePlayerVisual()
        {
            _playerSprite.sprite = PlaceholderAssets.CreateCharacterSprite(true, _api.GetCurrentMask());
        }

        private void UpdateEnemyVisual()
        {
            _enemySprite.sprite = PlaceholderAssets.CreateCharacterSprite(false, _api.GetEnemyMask());
        }

        private void UpdateAnimations()
        {
            // 玩家动画
            if (_playerHit)
            {
                _playerAnimTimer += Time.deltaTime * 10f;
                float offset = Mathf.Sin(_playerAnimTimer * Mathf.PI) * 0.3f;
                _playerObject.transform.position = _playerBasePos + new Vector3(-offset, 0, 0);
                _playerSprite.color = Color.Lerp(Color.red, Color.white, _playerAnimTimer);

                if (_playerAnimTimer >= 1f)
                {
                    _playerHit = false;
                    _playerAnimTimer = 0f;
                    _playerObject.transform.position = _playerBasePos;
                    _playerSprite.color = Color.white;
                }
            }

            // 敌人动画
            if (_enemyHit)
            {
                _enemyAnimTimer += Time.deltaTime * 10f;
                float offset = Mathf.Sin(_enemyAnimTimer * Mathf.PI) * 0.3f;
                _enemyObject.transform.position = _enemyBasePos + new Vector3(offset, 0, 0);
                _enemySprite.color = Color.Lerp(Color.yellow, Color.white, _enemyAnimTimer);

                if (_enemyAnimTimer >= 1f)
                {
                    _enemyHit = false;
                    _enemyAnimTimer = 0f;
                    _enemyObject.transform.position = _enemyBasePos;
                    _enemySprite.color = Color.white;
                }
            }
        }

        private void TriggerPlayerHit()
        {
            _playerHit = true;
            _playerAnimTimer = 0f;
        }

        private void TriggerEnemyHit()
        {
            _enemyHit = true;
            _enemyAnimTimer = 0f;
        }

        private void TriggerPlayerAction(JudgeResult result)
        {
            // 根据判定结果播放不同动画
            if (result == JudgeResult.Perfect || result == JudgeResult.Normal)
            {
                TriggerPlayerAttack();
            }
        }

        private void TriggerPlayerAttack()
        {
            // 攻击动画
        }

        private void SetBackgroundTheme(string levelName)
        {
            if (_backgroundMaterial == null) return;
            
            switch (levelName)
            {
                case "快乐森林":
                    _backgroundMaterial.SetFloat("_Theme", 0f);
                    _backgroundMaterial.SetFloat("_ScrollSpeed", 1.0f);
                    _backgroundMaterial.SetColor("_SkyColorTop", new Color(0.4f, 0.6f, 0.9f));
                    _backgroundMaterial.SetColor("_SkyColorBottom", new Color(0.9f, 0.7f, 0.5f));
                    _backgroundMaterial.SetColor("_FarColor", new Color(0.2f, 0.35f, 0.25f));
                    _backgroundMaterial.SetColor("_MidColor", new Color(0.15f, 0.28f, 0.18f));
                    _backgroundMaterial.SetColor("_NearColor", new Color(0.1f, 0.2f, 0.12f));
                    _backgroundMaterial.SetColor("_AccentColor", new Color(1f, 0.9f, 0.4f));
                    break;
                case "深海":
                    _backgroundMaterial.SetFloat("_Theme", 1f);
                    _backgroundMaterial.SetFloat("_ScrollSpeed", 0.7f);
                    _backgroundMaterial.SetColor("_SkyColorTop", new Color(0.05f, 0.15f, 0.3f));
                    _backgroundMaterial.SetColor("_SkyColorBottom", new Color(0.0f, 0.05f, 0.15f));
                    _backgroundMaterial.SetColor("_FarColor", new Color(0.1f, 0.25f, 0.35f));
                    _backgroundMaterial.SetColor("_MidColor", new Color(0.08f, 0.2f, 0.3f));
                    _backgroundMaterial.SetColor("_NearColor", new Color(0.15f, 0.25f, 0.2f));
                    _backgroundMaterial.SetColor("_AccentColor", new Color(0.4f, 0.8f, 1f));
                    break;
                case "天空":
                    _backgroundMaterial.SetFloat("_Theme", 2f);
                    _backgroundMaterial.SetFloat("_ScrollSpeed", 1.2f);
                    _backgroundMaterial.SetColor("_SkyColorTop", new Color(0.05f, 0.05f, 0.2f));
                    _backgroundMaterial.SetColor("_SkyColorBottom", new Color(0.3f, 0.4f, 0.7f));
                    _backgroundMaterial.SetColor("_FarColor", new Color(0.6f, 0.65f, 0.8f));
                    _backgroundMaterial.SetColor("_MidColor", new Color(0.7f, 0.75f, 0.9f));
                    _backgroundMaterial.SetColor("_NearColor", new Color(0.8f, 0.85f, 0.95f));
                    _backgroundMaterial.SetColor("_AccentColor", new Color(1f, 0.95f, 0.7f));
                    break;
            }
        }

        private void ShowMessage(string message)
        {
            _lastMessage = message;
            _messageTimer = 2f;
            Debug.Log($"[RhythmBattleScene] {message}");
        }

        #endregion

        #region UI绘制

        private void DrawWorldSpaceUI()
        {
            if (_mainCamera == null) return;

            GUIStyle faceTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle healthTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            healthTextStyle.normal.textColor = Color.white;

            // 玩家UI
            if (_playerObject != null)
            {
                Vector3 playerScreenPos = _mainCamera.WorldToScreenPoint(_playerObject.transform.position);
                playerScreenPos.y = Screen.height - playerScreenPos.y;

                // 面具名称
                string playerMaskName = GetMaskName(_api.GetCurrentMask());
                Color maskColor = PlaceholderAssets.GetMaskColor(_api.GetCurrentMask());
                
                GUI.color = new Color(0, 0, 0, 0.7f);
                GUI.DrawTexture(new Rect(playerScreenPos.x - 50, playerScreenPos.y - 20, 100, 40), Texture2D.whiteTexture);
                GUI.color = maskColor;
                faceTextStyle.normal.textColor = Color.white;
                GUI.Label(new Rect(playerScreenPos.x - 50, playerScreenPos.y - 20, 100, 40), playerMaskName, faceTextStyle);
                GUI.color = Color.white;

                // 血条
                float healthBarY = playerScreenPos.y - 100;
                DrawHealthBar(playerScreenPos.x, healthBarY, 120, 16, 
                    _api.GetPlayerHealth(), _api.GetPlayerMaxHealth(), 
                    new Color(0.2f, 0.8f, 0.2f), healthTextStyle);

                // 面具槽位
                DrawMaskSlots(playerScreenPos.x - 80, playerScreenPos.y + 80);
            }

            // 敌人UI
            if (_enemyObject != null && _enemyObject.activeSelf && _api.IsEnemyAlive)
            {
                Vector3 enemyScreenPos = _mainCamera.WorldToScreenPoint(_enemyObject.transform.position);
                enemyScreenPos.y = Screen.height - enemyScreenPos.y;

                string enemyMaskName = GetMaskName(_api.GetEnemyMask());
                Color enemyColor = PlaceholderAssets.GetMaskColor(_api.GetEnemyMask());
                
                GUI.color = new Color(0, 0, 0, 0.7f);
                GUI.DrawTexture(new Rect(enemyScreenPos.x - 60, enemyScreenPos.y - 25, 120, 50), Texture2D.whiteTexture);
                faceTextStyle.normal.textColor = enemyColor;
                GUI.Label(new Rect(enemyScreenPos.x - 60, enemyScreenPos.y - 25, 120, 30), enemyMaskName, faceTextStyle);
                GUI.color = Color.white;

                // 血条
                float healthBarY = enemyScreenPos.y - 100;
                int enemyMaxHealth = _levelManager?.CurrentWave?.EnemyHealth ?? 3;
                DrawHealthBar(enemyScreenPos.x, healthBarY, 120, 16, 
                    _api.GetEnemyHealth(), enemyMaxHealth, 
                    new Color(0.9f, 0.2f, 0.2f), healthTextStyle);
            }
        }

        private void DrawMaskSlots(float startX, float startY)
        {
            GUIStyle slotStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            var ownedMasks = _api.GetOwnedMasks();
            
            for (int i = 0; i < 3; i++)
            {
                float slotX = startX + i * 55;
                string keyLabel = i == 0 ? "Q" : (i == 1 ? "W" : "E");
                
                if (i < ownedMasks.Count)
                {
                    MaskType mask = ownedMasks[i];
                    bool isCurrent = mask == _api.GetCurrentMask();
                    Color slotColor = PlaceholderAssets.GetMaskColor(mask);
                    
                    GUI.color = isCurrent ? Color.yellow : new Color(0.3f, 0.3f, 0.3f, 0.8f);
                    GUI.DrawTexture(new Rect(slotX, startY, 50, 60), Texture2D.whiteTexture);
                    
                    GUI.color = slotColor;
                    GUI.DrawTexture(new Rect(slotX + 5, startY + 5, 40, 30), Texture2D.whiteTexture);
                    
                    GUI.color = Color.white;
                    slotStyle.normal.textColor = isCurrent ? Color.black : Color.white;
                    GUI.Label(new Rect(slotX, startY + 35, 50, 25), $"[{keyLabel}]", slotStyle);
                }
                else
                {
                    GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                    GUI.DrawTexture(new Rect(slotX, startY, 50, 60), Texture2D.whiteTexture);
                    GUI.color = new Color(0.5f, 0.5f, 0.5f);
                    GUI.Label(new Rect(slotX, startY + 35, 50, 25), $"[{keyLabel}]", slotStyle);
                }
            }
            GUI.color = Color.white;
        }

        private void DrawHealthBar(float centerX, float y, float width, float height, int current, int max, Color fillColor, GUIStyle textStyle)
        {
            float startX = centerX - width / 2;
            
            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            GUI.DrawTexture(new Rect(startX - 2, y - 2, width + 4, height + 4), Texture2D.whiteTexture);
            
            GUI.color = new Color(0.3f, 0.3f, 0.3f);
            GUI.DrawTexture(new Rect(startX, y, width, height), Texture2D.whiteTexture);
            
            float ratio = max > 0 ? (float)current / max : 0;
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(startX, y, width * ratio, height), Texture2D.whiteTexture);
            
            GUI.color = Color.white;
            GUI.Label(new Rect(startX, y - 2, width, height + 4), $"{current}/{max}", textStyle);
        }

        private void DrawRhythmUI()
        {
            if (!_rhythmBattleActive) return;

            // 节奏状态面板
            GUIStyle rhythmStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };
            rhythmStyle.normal.textColor = Color.cyan;

            GUI.Box(new Rect(Screen.width - 210, 10, 200, 120), "");
            GUILayout.BeginArea(new Rect(Screen.width - 200, 20, 180, 100));
            
            GUILayout.Label($"BPM: {bpm}", rhythmStyle);
            GUILayout.Label($"连击: {_rhythmManager.TotalCombo}", rhythmStyle);
            GUILayout.Label($"★完美: {_rhythmManager.PerfectCombo}/3", rhythmStyle);
            GUILayout.Label($"分数: {_rhythmManager.Score}", rhythmStyle);
            
            GUILayout.EndArea();

            // 面具效果提示
            if (_combatBridge != null)
            {
                string effectDesc = _combatBridge.GetMaskEffectDescription();
                GUIStyle effectStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 14,
                    alignment = TextAnchor.MiddleCenter
                };
                effectStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(0, Screen.height * 0.7f, Screen.width, 25), effectDesc, effectStyle);
            }

            // 格挡反击提示
            if (_combatBridge?.IsBlockCounterReady == true)
            {
                GUIStyle counterStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 24,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                float flash = Mathf.PingPong(Time.time * 4f, 1f);
                counterStyle.normal.textColor = new Color(0f, 1f, flash);
                GUI.Label(new Rect(0, Screen.height * 0.35f, Screen.width, 40), "⚔ 格挡反击准备! ⚔", counterStyle);
            }
        }

        private void DrawMainUI()
        {
            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.white;

            GUIStyle infoStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                alignment = TextAnchor.MiddleLeft
            };
            infoStyle.normal.textColor = Color.white;

            // 信息面板
            GUI.Box(new Rect(10, 10, 300, 130), "");
            GUILayout.BeginArea(new Rect(20, 20, 280, 110));

            if (_levelManager?.CurrentLevel != null)
            {
                GUILayout.Label($"关卡: {_levelManager.CurrentLevel.LevelName}", titleStyle);
                GUILayout.Label($"波次: {_levelManager.CurrentWaveIndex + 1}/{_levelManager.TotalWaves}", infoStyle);
            }

            if (campaignMode && _campaignManager != null)
            {
                GUILayout.Label($"战役: {_campaignManager.CurrentLevelIndex + 1}/{_campaignManager.TotalLevels}", infoStyle);
            }

            GUILayout.Label($"节奏战斗: {(_rhythmBattleActive ? "进行中" : "待机")}", infoStyle);

            GUILayout.EndArea();

            // 消息提示
            if (_messageTimer > 0)
            {
                GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                messageStyle.normal.textColor = Color.yellow;
                GUI.Label(new Rect(0, Screen.height / 2 + 80, Screen.width, 40), _lastMessage, messageStyle);
            }

            // 战斗效果显示
            if (_combatEffectTimer > 0 && _lastCombatEffect.HasValue)
            {
                DrawCombatEffectDisplay();
            }

            // 操作提示
            GUIStyle helpStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            helpStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(new Rect(0, Screen.height - 30, Screen.width, 25),
                "[空格]卡点  [Q/W/E]切换面具+卡点  [R]重新开始  [P]暂停  [B]开始节奏战斗", helpStyle);
        }

        private void DrawCombatEffectDisplay()
        {
            var effect = _lastCombatEffect.Value;
            
            GUIStyle effectStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            Color effectColor = Color.white;
            switch (effect.JudgeResult)
            {
                case JudgeResult.Perfect: effectColor = new Color(1f, 0.9f, 0.2f); break;
                case JudgeResult.Normal: effectColor = new Color(0.3f, 0.8f, 0.3f); break;
                case JudgeResult.Miss: effectColor = new Color(0.8f, 0.2f, 0.2f); break;
            }
            effectStyle.normal.textColor = effectColor;

            string effectText = effect.JudgeResult.ToString();
            if (effect.EnemyDamage > 0) effectText += $" → 敌人-{effect.EnemyDamage}";
            if (effect.PlayerDamage > 0) effectText += $" → 你-{effect.PlayerDamage}";
            if (effect.PlayerHealed) effectText += " → 回复!";
            if (effect.CounterAttack) effectText += " → 反击!";
            if (effect.Dodged) effectText += " → 闪避!";
            if (effect.Blocked) effectText += " → 格挡!";

            GUI.Label(new Rect(0, Screen.height / 2 - 20, Screen.width, 40), effectText, effectStyle);
        }

        private string GetMaskName(MaskType maskType)
        {
            switch (maskType)
            {
                case MaskType.None: return "无";
                case MaskType.Cat: return "猫";
                case MaskType.Snake: return "蛇";
                case MaskType.Bear: return "熊";
                case MaskType.Horse: return "马";
                case MaskType.Bull: return "牛";
                case MaskType.Whale: return "鲸";
                case MaskType.Shark: return "鲨";
                case MaskType.Dragon: return "龙";
                default: return maskType.ToString();
            }
        }

        #endregion
    }
}

