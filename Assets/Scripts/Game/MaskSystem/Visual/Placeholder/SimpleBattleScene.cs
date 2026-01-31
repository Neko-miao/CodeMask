using UnityEngine;
using System.Collections.Generic;
using Game.MaskSystem;

namespace Game.MaskSystem.Visual.Placeholder
{
    /// <summary>
    /// 简化版战斗场景 - 使用占位符资源，可立即开始游戏
    /// 将此脚本挂载到空物体上即可运行
    /// </summary>
    public class SimpleBattleScene : MonoBehaviour
    {
        [Header("游戏设置")]
        [SerializeField] private bool campaignMode = true;
        [SerializeField] private int startLevelIndex = 0;

        [Header("视觉设置")]
        [SerializeField] private float characterScale = 2f;
        [SerializeField] private float playerXPosition = -3f;
        [SerializeField] private float enemyXPosition = 3f;

        // 内部引用
        private IMaskSystemAPI _api;
        private CampaignManager _campaignManager;
        private LevelManager _levelManager => _campaignManager?.LevelManager;
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
        private bool _isWarning = false;
        private float _warningTimer = 0f;
        private string _lastMessage = "";
        private float _messageTimer = 0f;

        // 动画状态
        private Vector3 _playerBasePos;
        private Vector3 _enemyBasePos;
        private float _playerAnimTimer = 0f;
        private float _enemyAnimTimer = 0f;
        private bool _playerHit = false;
        private bool _enemyHit = false;

        void Awake()
        {
            InitializeSystem();
            CreateVisualObjects();
            SetupCamera();
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
            // 更新游戏逻辑
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

            // 更新视觉
            UpdateVisuals();

            // 更新动画
            UpdateAnimations();

            // 更新消息计时器
            if (_messageTimer > 0)
            {
                _messageTimer -= Time.deltaTime;
            }
        }

        void OnGUI()
        {
            DrawWorldSpaceUI();
            DrawUI();
        }

        void OnDestroy()
        {
            _campaignManager?.Dispose();
            
            // Clean up dynamically created material
            if (_backgroundMaterial != null)
            {
                Destroy(_backgroundMaterial);
            }
        }

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

            _campaignManager = new CampaignManager(_api);
            _campaignManager.SetLevels(_campaignLevels.ToArray());

            // 订阅事件
            _campaignManager.OnGameComplete += () => ShowMessage("恭喜通关！按R重新开始");
            _campaignManager.OnGameOver += () => ShowMessage("游戏结束！按R重新开始");
            _campaignManager.OnLevelStart += (idx, config) => OnLevelChanged(idx);

            _api.OnMaskAcquired += (mask) => ShowMessage($"获得新面具: {mask}!");
            _api.OnPlayerDefeated += () => ShowMessage("你被击败了!");
            _api.OnEnemyDefeated += () => ShowMessage("敌人被击败!");

            SubscribeAutoBattleEvents();
        }

        private void SubscribeAutoBattleEvents()
        {
            if (_levelManager?.AutoBattle != null)
            {
                var autoBattle = _levelManager.AutoBattle;
                autoBattle.OnWarningStart += OnWarningStart;
                autoBattle.OnWarningUpdate += OnWarningUpdate;
                autoBattle.OnPlayerCounter += (result) => { ShowMessage($"反击成功! 伤害: {result.Damage}"); TriggerEnemyHit(); };
                autoBattle.OnCounterFailed += () => ShowMessage("反击失败!");
                autoBattle.OnEnemyAttack += (result) => { ShowMessage($"被攻击! 伤害: {result.Damage}"); TriggerPlayerHit(); };
            }
        }

        private void OnWarningStart()
        {
            _isWarning = true;
            _warningTimer = _levelManager?.AutoBattle?.CurrentWave?.AttackWarningTime ?? 0.8f;
        }

        private void OnWarningUpdate(float remainingTime)
        {
            _warningTimer = remainingTime;
            if (remainingTime <= 0)
            {
                _isWarning = false;
            }
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

            // 初始化玩家外观
            UpdatePlayerVisual();
        }

        private void CreateProceduralBackground()
        {
            _backgroundObject = new GameObject("ProceduralBackground");
            _backgroundObject.transform.SetParent(transform);
            _backgroundObject.transform.position = new Vector3(0, 0, 10); // Behind everything
            
            // Create a quad mesh
            MeshFilter meshFilter = _backgroundObject.AddComponent<MeshFilter>();
            _backgroundRenderer = _backgroundObject.AddComponent<MeshRenderer>();
            
            // Create quad mesh programmatically
            Mesh quadMesh = new Mesh();
            quadMesh.name = "BackgroundQuad";
            
            // Vertices for a quad that fills the camera view
            float height = 12f;
            float width = height * (16f / 9f); // Assume 16:9 aspect
            
            quadMesh.vertices = new Vector3[]
            {
                new Vector3(-width / 2, -height / 2, 0),
                new Vector3(width / 2, -height / 2, 0),
                new Vector3(-width / 2, height / 2, 0),
                new Vector3(width / 2, height / 2, 0)
            };
            
            quadMesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            
            quadMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            quadMesh.RecalculateNormals();
            
            meshFilter.mesh = quadMesh;
            
            // Create material with the procedural shader
            Shader bgShader = Shader.Find("MaskSystem/ProceduralBackground");
            if (bgShader == null)
            {
                Debug.LogWarning("[SimpleBattleScene] ProceduralBackground shader not found! Using fallback.");
                bgShader = Shader.Find("Unlit/Color");
            }
            
            _backgroundMaterial = new Material(bgShader);
            _backgroundRenderer.material = _backgroundMaterial;
            _backgroundRenderer.sortingOrder = -100;
            
            // Set default theme (forest)
            SetBackgroundTheme("快乐森林");
        }

        private void SetBackgroundTheme(string levelName)
        {
            if (_backgroundMaterial == null) return;
            
            switch (levelName)
            {
                case "快乐森林":
                    _backgroundMaterial.SetFloat("_Theme", 0f);
                    _backgroundMaterial.SetFloat("_ScrollSpeed", 1.0f);
                    // Sky colors - warm sunset forest
                    _backgroundMaterial.SetColor("_SkyColorTop", new Color(0.4f, 0.6f, 0.9f));
                    _backgroundMaterial.SetColor("_SkyColorBottom", new Color(0.9f, 0.7f, 0.5f));
                    // Layer colors - greens and browns
                    _backgroundMaterial.SetColor("_FarColor", new Color(0.2f, 0.35f, 0.25f));
                    _backgroundMaterial.SetColor("_MidColor", new Color(0.15f, 0.28f, 0.18f));
                    _backgroundMaterial.SetColor("_NearColor", new Color(0.1f, 0.2f, 0.12f));
                    _backgroundMaterial.SetColor("_AccentColor", new Color(1f, 0.9f, 0.4f));
                    break;
                    
                case "深海":
                    _backgroundMaterial.SetFloat("_Theme", 1f);
                    _backgroundMaterial.SetFloat("_ScrollSpeed", 0.7f);
                    // Deep ocean colors
                    _backgroundMaterial.SetColor("_SkyColorTop", new Color(0.05f, 0.15f, 0.3f));
                    _backgroundMaterial.SetColor("_SkyColorBottom", new Color(0.0f, 0.05f, 0.15f));
                    // Layer colors - blues and teals
                    _backgroundMaterial.SetColor("_FarColor", new Color(0.1f, 0.25f, 0.35f));
                    _backgroundMaterial.SetColor("_MidColor", new Color(0.08f, 0.2f, 0.3f));
                    _backgroundMaterial.SetColor("_NearColor", new Color(0.15f, 0.25f, 0.2f));
                    _backgroundMaterial.SetColor("_AccentColor", new Color(0.4f, 0.8f, 1f));
                    break;
                    
                case "天空":
                    _backgroundMaterial.SetFloat("_Theme", 2f);
                    _backgroundMaterial.SetFloat("_ScrollSpeed", 1.2f);
                    // Sky/celestial colors
                    _backgroundMaterial.SetColor("_SkyColorTop", new Color(0.05f, 0.05f, 0.2f));
                    _backgroundMaterial.SetColor("_SkyColorBottom", new Color(0.3f, 0.4f, 0.7f));
                    // Layer colors - whites and light blues
                    _backgroundMaterial.SetColor("_FarColor", new Color(0.6f, 0.65f, 0.8f));
                    _backgroundMaterial.SetColor("_MidColor", new Color(0.7f, 0.75f, 0.9f));
                    _backgroundMaterial.SetColor("_NearColor", new Color(0.8f, 0.85f, 0.95f));
                    _backgroundMaterial.SetColor("_AccentColor", new Color(1f, 0.95f, 0.7f));
                    break;
                    
                default:
                    // Default to forest
                    _backgroundMaterial.SetFloat("_Theme", 0f);
                    break;
            }
            
            Debug.Log($"[SimpleBattleScene] 背景主题切换: {levelName}");
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
            // 反击
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (_levelManager != null && _levelManager.TryCounter())
                {
                    TriggerPlayerAttack();
                }
            }

            // 切换面具
            if (Input.GetKeyDown(KeyCode.Q)) SwitchMask(0);
            if (Input.GetKeyDown(KeyCode.W)) SwitchMask(1);
            if (Input.GetKeyDown(KeyCode.E)) SwitchMask(2);

            // 重新开始
            if (Input.GetKeyDown(KeyCode.R))
            {
                RestartGame();
            }

            // 暂停/继续
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_levelManager != null)
                {
                    if (_levelManager.State == LevelState.Playing)
                    {
                        _levelManager.Pause();
                        ShowMessage("游戏暂停");
                    }
                    else
                    {
                        _levelManager.Resume();
                        ShowMessage("游戏继续");
                    }
                }
            }

            // 调试：跳过当前敌人
            if (Input.GetKeyDown(KeyCode.N))
            {
                _api.DefeatCurrentEnemy();
            }

            // 调试：选择关卡
            if (Input.GetKeyDown(KeyCode.Alpha1)) _campaignManager?.StartFromLevel(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) _campaignManager?.StartFromLevel(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) _campaignManager?.StartFromLevel(2);
        }

        private void SwitchMask(int slot)
        {
            if (_levelManager != null && _levelManager.SwitchMask(slot))
            {
                UpdatePlayerVisual();
                ShowMessage($"切换到面具槽位 {slot + 1}");
            }
        }

        private void RestartGame()
        {
            if (campaignMode)
            {
                _campaignManager.RestartCampaign();
                // 重新订阅事件
                SubscribeAutoBattleEvents();
            }
            else
            {
                _levelManager?.Restart();
            }
            ShowMessage("游戏重新开始!");
        }

        #endregion

        #region 视觉更新

        private void UpdateVisuals()
        {
            // 更新关卡背景主题
            if (_levelManager?.CurrentLevel != null && _currentLevelName != _levelManager.CurrentLevel.LevelName)
            {
                _currentLevelName = _levelManager.CurrentLevel.LevelName;
                SetBackgroundTheme(_currentLevelName);
            }

            // 更新敌人外观
            if (_api.IsEnemyAlive && _currentEnemyMask != _api.GetEnemyMask())
            {
                _currentEnemyMask = _api.GetEnemyMask();
                UpdateEnemyVisual();
            }

            // 更新预警状态
            if (_levelManager?.AutoBattle != null)
            {
                _isWarning = _levelManager.AutoBattle.IsInCounterWindow;
            }

            // 敌人死亡时隐藏
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

            // 预警时敌人闪烁
            if (_isWarning && !_enemyHit)
            {
                float flash = Mathf.PingPong(Time.time * 8f, 1f);
                _enemySprite.color = Color.Lerp(Color.white, Color.red, flash);
            }
            else if (!_enemyHit)
            {
                _enemySprite.color = Color.white;
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

        private void TriggerPlayerAttack()
        {
            // 玩家攻击动画（向前冲刺）
            // 简单实现，可以后续扩展
        }

        #endregion

        #region 事件处理

        private void OnLevelChanged(int levelIndex)
        {
            if (levelIndex < _campaignLevels.Count)
            {
                ShowMessage($"进入关卡: {_campaignLevels[levelIndex].LevelName}");
                _currentLevelName = "";  // 强制更新背景
                _currentEnemyMask = MaskType.None;  // 强制更新敌人
                _isWarning = false;

                // 重新订阅战斗事件
                SubscribeAutoBattleEvents();
            }
        }

        private void ShowMessage(string message)
        {
            _lastMessage = message;
            _messageTimer = 2f;
            Debug.Log($"[SimpleBattleScene] {message}");
        }

        #endregion

        #region UI绘制

        /// <summary>
        /// 绘制世界空间UI（角色头顶血条、面具名称等）
        /// </summary>
        private void DrawWorldSpaceUI()
        {
            if (_mainCamera == null) return;

            // 样式定义
            GUIStyle faceTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle maskSlotStyle = new GUIStyle(GUI.skin.box)
            {
                fontSize = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };

            GUIStyle healthTextStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            healthTextStyle.normal.textColor = Color.white;

            // ============ 玩家UI ============
            if (_playerObject != null)
            {
                Vector3 playerScreenPos = _mainCamera.WorldToScreenPoint(_playerObject.transform.position);
                playerScreenPos.y = Screen.height - playerScreenPos.y; // 翻转Y轴

                // 玩家脸上显示当前面具名称
                string playerMaskName = GetMaskDisplayName(_api.GetCurrentMask());
                Color maskColor = PlaceholderAssets.GetMaskColor(_api.GetCurrentMask());
                faceTextStyle.normal.textColor = Color.white;
                
                // 面具名称背景
                GUI.color = new Color(0, 0, 0, 0.7f);
                GUI.DrawTexture(new Rect(playerScreenPos.x - 50, playerScreenPos.y - 20, 100, 40), Texture2D.whiteTexture);
                GUI.color = maskColor;
                GUI.Label(new Rect(playerScreenPos.x - 50, playerScreenPos.y - 20, 100, 40), playerMaskName, faceTextStyle);
                GUI.color = Color.white;

                // 玩家头顶血条
                float healthBarY = playerScreenPos.y - 100;
                DrawWorldHealthBar(playerScreenPos.x, healthBarY, 120, 16, 
                    _api.GetPlayerHealth(), _api.GetPlayerMaxHealth(), 
                    new Color(0.2f, 0.8f, 0.2f), healthTextStyle);

                // 玩家身边的面具槽位
                float slotStartX = playerScreenPos.x - 80;
                float slotY = playerScreenPos.y + 80;
                var ownedMasks = _api.GetOwnedMasks();
                
                for (int i = 0; i < 3; i++)
                {
                    float slotX = slotStartX + i * 55;
                    string keyLabel = i == 0 ? "Q" : (i == 1 ? "W" : "E");
                    
                    if (i < ownedMasks.Count)
                    {
                        MaskType mask = ownedMasks[i];
                        bool isCurrent = mask == _api.GetCurrentMask();
                        Color slotColor = PlaceholderAssets.GetMaskColor(mask);
                        
                        // 槽位背景
                        GUI.color = isCurrent ? Color.yellow : new Color(0.3f, 0.3f, 0.3f, 0.8f);
                        GUI.DrawTexture(new Rect(slotX, slotY, 50, 60), Texture2D.whiteTexture);
                        
                        // 面具颜色指示
                        GUI.color = slotColor;
                        GUI.DrawTexture(new Rect(slotX + 5, slotY + 5, 40, 30), Texture2D.whiteTexture);
                        
                        // 按键和名称
                        GUI.color = Color.white;
                        maskSlotStyle.normal.textColor = isCurrent ? Color.black : Color.white;
                        GUI.Label(new Rect(slotX, slotY + 35, 50, 25), $"[{keyLabel}]", maskSlotStyle);
                    }
                    else
                    {
                        // 空槽位
                        GUI.color = new Color(0.2f, 0.2f, 0.2f, 0.5f);
                        GUI.DrawTexture(new Rect(slotX, slotY, 50, 60), Texture2D.whiteTexture);
                        GUI.color = new Color(0.5f, 0.5f, 0.5f);
                        GUI.Label(new Rect(slotX, slotY + 35, 50, 25), $"[{keyLabel}]", maskSlotStyle);
                    }
                }
                GUI.color = Color.white;
            }

            // ============ 敌人UI ============
            if (_enemyObject != null && _enemyObject.activeSelf && _api.IsEnemyAlive)
            {
                Vector3 enemyScreenPos = _mainCamera.WorldToScreenPoint(_enemyObject.transform.position);
                enemyScreenPos.y = Screen.height - enemyScreenPos.y;

                // 敌人脸上显示怪物名称
                string enemyName = _api.GetEnemyName();
                string enemyMaskName = GetMaskDisplayName(_api.GetEnemyMask());
                Color enemyColor = PlaceholderAssets.GetMaskColor(_api.GetEnemyMask());
                
                // 怪物名称背景
                GUI.color = new Color(0, 0, 0, 0.7f);
                GUI.DrawTexture(new Rect(enemyScreenPos.x - 60, enemyScreenPos.y - 25, 120, 50), Texture2D.whiteTexture);
                
                // 怪物类型
                faceTextStyle.normal.textColor = enemyColor;
                GUI.Label(new Rect(enemyScreenPos.x - 60, enemyScreenPos.y - 25, 120, 30), enemyMaskName, faceTextStyle);
                
                // 怪物名称（小字）
                faceTextStyle.fontSize = 18;
                faceTextStyle.normal.textColor = Color.gray;
                GUI.Label(new Rect(enemyScreenPos.x - 60, enemyScreenPos.y + 5, 120, 20), $"({enemyName})", faceTextStyle);
                faceTextStyle.fontSize = 28;
                
                GUI.color = Color.white;

                // 敌人头顶血条
                float healthBarY = enemyScreenPos.y - 100;
                int enemyMaxHealth = _levelManager?.CurrentWave?.EnemyHealth ?? 1;
                DrawWorldHealthBar(enemyScreenPos.x, healthBarY, 120, 16, 
                    _api.GetEnemyHealth(), enemyMaxHealth, 
                    new Color(0.9f, 0.2f, 0.2f), healthTextStyle);

                // 预警时显示警告图标
                if (_isWarning)
                {
                    GUIStyle warningIconStyle = new GUIStyle(GUI.skin.label)
                    {
                        fontSize = 36,
                        fontStyle = FontStyle.Bold,
                        alignment = TextAnchor.MiddleCenter
                    };
                    float flash = Mathf.PingPong(Time.time * 6f, 1f);
                    warningIconStyle.normal.textColor = new Color(1f, flash, 0f);
                    GUI.Label(new Rect(enemyScreenPos.x - 30, enemyScreenPos.y - 150, 60, 40), "⚡", warningIconStyle);
                }
            }
        }

        /// <summary>
        /// 绘制世界空间血条
        /// </summary>
        private void DrawWorldHealthBar(float centerX, float y, float width, float height, int current, int max, Color fillColor, GUIStyle textStyle)
        {
            float startX = centerX - width / 2;
            
            // 背景
            GUI.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            GUI.DrawTexture(new Rect(startX - 2, y - 2, width + 4, height + 4), Texture2D.whiteTexture);
            
            // 空血条
            GUI.color = new Color(0.3f, 0.3f, 0.3f);
            GUI.DrawTexture(new Rect(startX, y, width, height), Texture2D.whiteTexture);
            
            // 填充
            float ratio = max > 0 ? (float)current / max : 0;
            GUI.color = fillColor;
            GUI.DrawTexture(new Rect(startX, y, width * ratio, height), Texture2D.whiteTexture);
            
            // 血量文字
            GUI.color = Color.white;
            GUI.Label(new Rect(startX, y - 2, width, height + 4), $"{current}/{max}", textStyle);
        }

        /// <summary>
        /// 获取面具显示名称
        /// </summary>
        private string GetMaskDisplayName(MaskType maskType)
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

        private void DrawUI()
        {
            // 设置样式
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

            GUIStyle warningStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            warningStyle.normal.textColor = Color.red;

            GUIStyle messageStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            messageStyle.normal.textColor = Color.yellow;

            // 顶部信息栏
            GUI.Box(new Rect(10, 10, 300, 180), "");
            GUILayout.BeginArea(new Rect(20, 20, 280, 160));

            // 关卡信息
            if (_levelManager?.CurrentLevel != null)
            {
                GUILayout.Label($"关卡: {_levelManager.CurrentLevel.LevelName}", titleStyle);
                GUILayout.Label($"波次: {_levelManager.CurrentWaveIndex + 1}/{_levelManager.TotalWaves}", infoStyle);
            }

            // 战役进度
            if (campaignMode && _campaignManager != null)
            {
                GUILayout.Label($"战役进度: {_campaignManager.CurrentLevelIndex + 1}/{_campaignManager.TotalLevels}", infoStyle);
            }

            // 状态
            if (_levelManager != null)
            {
                GUILayout.Label($"状态: {GetStateText(_levelManager.State)}", infoStyle);
            }

            GUILayout.EndArea();

            // 预警提示（中央）
            if (_isWarning)
            {
                float flash = Mathf.PingPong(Time.time * 4f, 1f);
                GUI.color = new Color(1f, flash, flash, 1f);
                GUI.Label(new Rect(0, Screen.height / 2 - 50, Screen.width, 60), "⚠ 按空格反击! ⚠", warningStyle);
                GUI.color = Color.white;
            }

            // 消息提示
            if (_messageTimer > 0)
            {
                GUI.Label(new Rect(0, Screen.height / 2 + 50, Screen.width, 40), _lastMessage, messageStyle);
            }

            // 操作提示（底部中央）
            GUIStyle helpStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                alignment = TextAnchor.MiddleCenter
            };
            helpStyle.normal.textColor = new Color(0.7f, 0.7f, 0.7f);
            GUI.Label(new Rect(0, Screen.height - 30, Screen.width, 25),
                "[空格]反击  [Q/W/E]切换面具  [R]重新开始  [P]暂停  [1/2/3]选关", helpStyle);

            // 过渡状态显示
            if (_levelManager?.State == LevelState.Preparing)
            {
                DrawCenterMessage($"准备中... {_levelManager.TransitionTimer:F1}s");
            }
            else if (_levelManager?.State == LevelState.WaveTransition)
            {
                DrawCenterMessage($"下一波 {_levelManager.TransitionTimer:F1}s");
            }
            else if (_levelManager?.State == LevelState.Victory)
            {
                DrawCenterMessage("关卡胜利!");
            }
            else if (_levelManager?.State == LevelState.Defeat)
            {
                DrawCenterMessage("关卡失败!");
            }

            // 战役完成状态
            if (_campaignManager?.State == CampaignState.GameComplete)
            {
                DrawCenterMessage("🎉 恭喜通关! 按R重新开始 🎉", Color.yellow);
            }
            else if (_campaignManager?.State == CampaignState.GameOver)
            {
                DrawCenterMessage("💀 游戏结束! 按R重新开始 💀", Color.red);
            }
            else if (_campaignManager?.State == CampaignState.LevelTransition)
            {
                DrawCenterMessage($"进入下一关...");
            }
        }

        private void DrawCenterMessage(string message, Color? color = null)
        {
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 28,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = color ?? Color.white;

            // 背景
            GUI.color = new Color(0, 0, 0, 0.7f);
            GUI.DrawTexture(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 80), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(Screen.width / 2 - 200, Screen.height / 2 - 100, 400, 80), message, style);
        }

        private string GetStateText(LevelState state)
        {
            switch (state)
            {
                case LevelState.None: return "未开始";
                case LevelState.Preparing: return "准备中";
                case LevelState.Playing: return "战斗中";
                case LevelState.WaveTransition: return "波次过渡";
                case LevelState.Victory: return "胜利";
                case LevelState.Defeat: return "失败";
                default: return state.ToString();
            }
        }

        private int GetEnemyMaxHealth()
        {
            if (_levelManager?.CurrentWave != null)
            {
                return _levelManager.CurrentWave.EnemyHealth;
            }
            return 1;
        }

        #endregion
    }
}

