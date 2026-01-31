// ================================================
// MaskSystem Visual - 战斗场景管理器
// ================================================

using System;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 战斗场景管理器 - 管理整个战斗场景的视觉表现
    /// </summary>
    public class BattleSceneManager : MonoBehaviour
    {
        #region 组件引用

        [Header("资源配置")]
        [Tooltip("游戏资源配置")]
        [SerializeField] private GameAssetsConfig assetsConfig;

        [Header("场景元素")]
        [Tooltip("背景图片")]
        [SerializeField] private Image backgroundImage;

        [Tooltip("环境光覆盖层")]
        [SerializeField] private Image ambientOverlay;

        [Tooltip("前景装饰")]
        [SerializeField] private Image foregroundDecor;

        [Header("角色视图")]
        [Tooltip("玩家视图")]
        [SerializeField] private PlayerView playerView;

        [Tooltip("敌人视图")]
        [SerializeField] private EnemyView enemyView;

        [Header("UI组件")]
        [Tooltip("战斗HUD")]
        [SerializeField] private BattleHUD battleHUD;

        [Tooltip("关卡标题文本")]
        [SerializeField] private Text levelTitleText;

        [Tooltip("波次文本")]
        [SerializeField] private Text waveText;

        [Header("特效")]
        [Tooltip("屏幕震动组件")]
        [SerializeField] private ScreenShake screenShake;

        [Tooltip("屏幕闪烁组件")]
        [SerializeField] private HitFlash hitFlash;

        [Header("音频")]
        [Tooltip("背景音乐播放器")]
        [SerializeField] private AudioSource bgmSource;

        [Tooltip("音效播放器")]
        [SerializeField] private AudioSource sfxSource;

        [Header("战役模式")]
        [Tooltip("启用战役模式（连续三关卡）")]
        [SerializeField] private bool campaignMode = true;

        [Tooltip("自动开始")]
        [SerializeField] private bool autoStart = true;

        #endregion

        #region 私有字段

        private CampaignManager _campaignManager;
        private LevelManager _levelManager;
        private IMaskSystemAPI _api;
        private LevelVisualData _currentLevelVisual;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 创建API
            _api = MaskSystemFacade.CreateNew();

            // 初始化管理器
            if (campaignMode)
            {
                _campaignManager = new CampaignManager(_api);
                _levelManager = _campaignManager.LevelManager;
            }
            else
            {
                _levelManager = new LevelManager(_api);
            }

            // 初始化视图
            InitializeViews();

            // 订阅事件
            SubscribeEvents();

            Debug.Log("[BattleSceneManager] 初始化完成");
        }

        private void Start()
        {
            if (campaignMode)
            {
                _campaignManager.UseDefaultCampaign();

                if (autoStart)
                {
                    _campaignManager.StartCampaign();
                }
            }
            else
            {
                var level = LevelConfig.CreateDefault();
                _levelManager.LoadLevel(level);

                if (autoStart)
                {
                    _levelManager.StartLevel();
                }
            }
        }

        private void Update()
        {
            // 更新管理器
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

            // 更新预警显示
            UpdateWarningDisplay();
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

        #endregion

        #region 初始化

        private void InitializeViews()
        {
            // 创建默认配置（如果没有）
            if (assetsConfig == null)
            {
                assetsConfig = GameAssetsConfig.CreateDefault();
            }

            // 初始化玩家视图
            if (playerView != null)
            {
                playerView.Initialize(assetsConfig);
            }

            // 初始化敌人视图
            if (enemyView != null)
            {
                enemyView.Initialize(assetsConfig);
                enemyView.SetVisible(false);
            }

            // 初始化HUD
            if (battleHUD != null)
            {
                battleHUD.Initialize(assetsConfig);
            }
        }

        #endregion

        #region 输入处理

        private void HandleInput()
        {
            // Space - 反击
            if (Input.GetKeyDown(KeyCode.Space))
            {
                bool success;
                if (campaignMode)
                {
                    success = _campaignManager.TryCounter();
                }
                else
                {
                    success = _levelManager.TryCounter();
                }

                if (success)
                {
                    OnPlayerCounter();
                }
            }

            // Q/W/E - 切换面具
            if (Input.GetKeyDown(KeyCode.Q)) SwitchMask(0);
            else if (Input.GetKeyDown(KeyCode.W)) SwitchMask(1);
            else if (Input.GetKeyDown(KeyCode.E)) SwitchMask(2);

            // R - 重新开始
            if (Input.GetKeyDown(KeyCode.R))
            {
                if (campaignMode)
                {
                    if (_campaignManager.State == CampaignState.GameOver ||
                        _campaignManager.State == CampaignState.GameComplete)
                    {
                        _campaignManager.RestartCampaign();
                    }
                    else
                    {
                        _campaignManager.RestartCurrentLevel();
                    }
                }
                else
                {
                    _levelManager.Restart();
                }
            }

            // P - 暂停
            if (Input.GetKeyDown(KeyCode.P))
            {
                if (_levelManager.AutoBattle?.IsPaused ?? false)
                {
                    if (campaignMode) _campaignManager.Resume();
                    else _levelManager.Resume();
                }
                else
                {
                    if (campaignMode) _campaignManager.Pause();
                    else _levelManager.Pause();
                }
            }

            // N - 调试：跳过敌人
            if (Input.GetKeyDown(KeyCode.N))
            {
                _api.DefeatCurrentEnemy();
            }
        }

        private void SwitchMask(int slot)
        {
            bool success;
            if (campaignMode)
            {
                success = _campaignManager.SwitchMask(slot);
            }
            else
            {
                success = _levelManager.SwitchMask(slot);
            }

            if (success && playerView != null)
            {
                playerView.SelectSlot(slot);
                playerView.PlayMaskSwitchAnimation(_api.GetCurrentMask());
                PlaySound(assetsConfig?.SwitchMaskSound);
            }
        }

        #endregion

        #region 视图更新

        private void UpdateWarningDisplay()
        {
            var autoBattle = _levelManager?.AutoBattle;
            if (autoBattle == null || enemyView == null) return;

            if (autoBattle.CurrentPhase == BattlePhase.Warning)
            {
                if (!autoBattle.IsPaused)
                {
                    enemyView.UpdateWarningProgress(autoBattle.WarningProgress);
                }
            }
        }

        private void UpdateLevelVisuals(string levelName)
        {
            if (assetsConfig == null) return;

            _currentLevelVisual = assetsConfig.GetLevelVisual(levelName);
            if (_currentLevelVisual == null)
            {
                // 尝试根据关卡名创建默认视觉
                if (levelName.Contains("森林"))
                    _currentLevelVisual = LevelVisualData.CreateHappyForest();
                else if (levelName.Contains("海") || levelName.Contains("深"))
                    _currentLevelVisual = LevelVisualData.CreateDeepSea();
                else if (levelName.Contains("天空") || levelName.Contains("龙"))
                    _currentLevelVisual = LevelVisualData.CreateSky();
            }

            if (_currentLevelVisual != null)
            {
                // 更新背景
                if (backgroundImage != null)
                {
                    if (_currentLevelVisual.Background != null)
                    {
                        backgroundImage.sprite = _currentLevelVisual.Background;
                        backgroundImage.color = Color.white;
                    }
                    else
                    {
                        backgroundImage.sprite = null;
                        backgroundImage.color = _currentLevelVisual.BackgroundColor;
                    }
                }

                // 更新环境光
                if (ambientOverlay != null)
                {
                    ambientOverlay.color = new Color(
                        _currentLevelVisual.AmbientColor.r,
                        _currentLevelVisual.AmbientColor.g,
                        _currentLevelVisual.AmbientColor.b,
                        0.1f
                    );
                }

                // 更新标题
                if (levelTitleText != null)
                {
                    levelTitleText.text = _currentLevelVisual.LevelName;
                }

                // 播放BGM
                if (bgmSource != null && _currentLevelVisual.BGM != null)
                {
                    bgmSource.clip = _currentLevelVisual.BGM;
                    bgmSource.volume = _currentLevelVisual.BGMVolume;
                    bgmSource.Play();
                }
            }
        }

        private void UpdatePlayerView()
        {
            if (playerView == null || _api == null) return;

            // 更新面具槽位
            playerView.SetOwnedMasks(_api.GetOwnedMasks());

            // 更新血量
            playerView.SetHealth(_api.GetPlayerHealth(), _api.GetPlayerMaxHealth());

            // 更新当前面具
            playerView.SetMask(_api.GetCurrentMask(), assetsConfig);
        }

        private void UpdateEnemyView()
        {
            if (enemyView == null || _api == null) return;

            if (_api.HasEnemy)
            {
                enemyView.SetEnemy(
                    _api.GetEnemyName(),
                    _api.GetEnemyMask(),
                    _api.GetEnemyHealth(),
                    _api.GetEnemyMaxHealth()
                );
            }
        }

        #endregion

        #region 事件处理

        private void SubscribeEvents()
        {
            // API事件
            _api.OnMaskChanged += OnMaskChanged;
            _api.OnPlayerHealthChanged += OnPlayerHealthChanged;
            _api.OnEnemyHealthChanged += OnEnemyHealthChanged;
            _api.OnMaskAcquired += OnMaskAcquired;
            _api.OnEnemyDefeated += OnEnemyDefeated;
            _api.OnPlayerDefeated += OnPlayerDefeated;
            _api.OnEnemySpawned += OnEnemySpawned;

            // 关卡事件
            _levelManager.OnWaveStart += OnWaveStart;
            _levelManager.AutoBattle.OnWarningStart += OnWarningStart;
            _levelManager.AutoBattle.OnEnemyAttack += OnEnemyAttack;
            _levelManager.AutoBattle.OnPlayerCounter += OnPlayerCounterSuccess;

            // 战役事件
            if (campaignMode && _campaignManager != null)
            {
                _campaignManager.OnLevelStart += OnCampaignLevelStart;
                _campaignManager.OnGameComplete += OnGameComplete;
                _campaignManager.OnGameOver += OnGameOver;
            }
        }

        private void UnsubscribeEvents()
        {
            if (_api != null)
            {
                _api.OnMaskChanged -= OnMaskChanged;
                _api.OnPlayerHealthChanged -= OnPlayerHealthChanged;
                _api.OnEnemyHealthChanged -= OnEnemyHealthChanged;
                _api.OnMaskAcquired -= OnMaskAcquired;
                _api.OnEnemyDefeated -= OnEnemyDefeated;
                _api.OnPlayerDefeated -= OnPlayerDefeated;
                _api.OnEnemySpawned -= OnEnemySpawned;
            }

            if (_levelManager != null)
            {
                _levelManager.OnWaveStart -= OnWaveStart;

                if (_levelManager.AutoBattle != null)
                {
                    _levelManager.AutoBattle.OnWarningStart -= OnWarningStart;
                    _levelManager.AutoBattle.OnEnemyAttack -= OnEnemyAttack;
                    _levelManager.AutoBattle.OnPlayerCounter -= OnPlayerCounterSuccess;
                }
            }

            if (_campaignManager != null)
            {
                _campaignManager.OnLevelStart -= OnCampaignLevelStart;
                _campaignManager.OnGameComplete -= OnGameComplete;
                _campaignManager.OnGameOver -= OnGameOver;
            }
        }

        private void OnMaskChanged(MaskType oldMask, MaskType newMask)
        {
            if (playerView != null)
            {
                playerView.SetMask(newMask, assetsConfig);
            }
        }

        private void OnPlayerHealthChanged(int oldHealth, int newHealth)
        {
            if (playerView != null)
            {
                playerView.SetHealth(newHealth, _api.GetPlayerMaxHealth());
            }

            if (battleHUD != null)
            {
                battleHUD.UpdatePlayerHealth(newHealth, _api.GetPlayerMaxHealth());
            }

            // 受伤效果
            if (newHealth < oldHealth)
            {
                playerView?.PlayHitAnimation();
                screenShake?.Shake(0.3f, 10f);
                hitFlash?.Flash(Color.red, 0.2f);
                PlaySound(assetsConfig?.HitSound);
            }
        }

        private void OnEnemyHealthChanged(int oldHealth, int newHealth)
        {
            if (enemyView != null)
            {
                enemyView.SetHealth(newHealth, _api.GetEnemyMaxHealth());
            }

            if (battleHUD != null)
            {
                battleHUD.UpdateEnemyHealth(newHealth, _api.GetEnemyMaxHealth());
            }

            // 敌人受伤效果
            if (newHealth < oldHealth)
            {
                enemyView?.PlayHitAnimation();

                var visual = assetsConfig?.GetMaskVisual(_api.GetCurrentMask());
                if (visual?.AttackEffect != null)
                {
                    enemyView?.SpawnEffect(visual.AttackEffect);
                }
            }
        }

        private void OnMaskAcquired(MaskType mask)
        {
            if (playerView != null)
            {
                playerView.AddMask(mask);
            }

            // 获得面具特效
            if (assetsConfig?.LevelUpEffect != null)
            {
                var effect = Instantiate(assetsConfig.LevelUpEffect, playerView.transform.position, Quaternion.identity);
                Destroy(effect, 2f);
            }
        }

        private void OnEnemyDefeated()
        {
            enemyView?.PlayDefeatAnimation();
            PlaySound(assetsConfig?.VictorySound);
        }

        private void OnPlayerDefeated()
        {
            playerView?.PlayDeathAnimation();
            hitFlash?.Flash(Color.red, 1f);
            PlaySound(assetsConfig?.DefeatSound);

            if (battleHUD != null)
            {
                battleHUD.ShowGameOver();
            }
        }

        private void OnEnemySpawned(MaskType enemyMask)
        {
            if (enemyView != null)
            {
                enemyView.ResetView();
                enemyView.SetVisible(true);
                UpdateEnemyView();
                enemyView.PlayEntranceAnimation();
            }

            if (battleHUD != null)
            {
                battleHUD.UpdateEnemyHealth(_api.GetEnemyHealth(), _api.GetEnemyMaxHealth());
            }
        }

        private void OnWaveStart(int waveIndex, WaveConfig wave)
        {
            if (waveText != null)
            {
                waveText.text = $"波次 {waveIndex + 1}/{_levelManager.TotalWaves}";
            }

            UpdatePlayerView();
        }

        private void OnWarningStart()
        {
            enemyView?.ShowWarning();

            if (battleHUD != null)
            {
                battleHUD.ShowWarning();
            }
        }

        private void OnEnemyAttack(CombatResult result)
        {
            enemyView?.HideWarning();
            enemyView?.PlayAttackAnimation(Vector3.left);

            if (battleHUD != null)
            {
                battleHUD.HideWarning();
            }

            PlaySound(assetsConfig?.AttackSound);
        }

        private void OnPlayerCounter()
        {
            playerView?.PlayAttackAnimation(Vector3.right);
            PlaySound(assetsConfig?.AttackSound);
        }

        private void OnPlayerCounterSuccess(CombatResult result)
        {
            enemyView?.HideWarning();

            if (battleHUD != null)
            {
                battleHUD.HideWarning();
                battleHUD.ShowCounterSuccess();
            }
        }

        private void OnCampaignLevelStart(int levelIndex, LevelConfig level)
        {
            UpdateLevelVisuals(level.LevelName);
            UpdatePlayerView();

            if (battleHUD != null)
            {
                battleHUD.ShowLevelTitle(level.LevelName, level.Description);
            }
        }

        private void OnGameComplete()
        {
            if (battleHUD != null)
            {
                battleHUD.ShowVictory();
            }

            PlaySound(assetsConfig?.VictorySound);
        }

        private void OnGameOver()
        {
            if (battleHUD != null)
            {
                battleHUD.ShowGameOver();
            }
        }

        #endregion

        #region 音频

        private void PlaySound(AudioClip clip)
        {
            if (sfxSource != null && clip != null)
            {
                sfxSource.PlayOneShot(clip);
            }
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 设置资源配置
        /// </summary>
        public void SetAssetsConfig(GameAssetsConfig config)
        {
            assetsConfig = config;
            InitializeViews();
        }

        /// <summary>
        /// 获取API接口
        /// </summary>
        public IMaskSystemAPI GetAPI()
        {
            return _api;
        }

        #endregion
    }
}

