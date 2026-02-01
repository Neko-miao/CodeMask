// ================================================
// Game - 游戏中状态处理器
// ================================================

using GameFramework.Core;
using GameFramework.UI;
using GameFramework.Session;
using GameFramework.Components;
using Game.UI;
using Game.Battle;
using Game.Modules;
using Game.Monsters;
using GameConfigs;
using UnityEngine;

namespace Game.States
{
    /// <summary>
    /// 游戏中状态处理器 - 管理单局战斗
    /// </summary>
    public class InGameStateHandler : IGameStateHandler
    {
        private BattleUIController _battleUIController;
        private BattleController _battleController;
        private MonsterSpawner _monsterSpawner;
        private QWEAudioPlayer _qweAudioPlayer;
        private int _currentLevelId = 1;
        
        public void OnEnter()
        {
            Debug.Log("[InGameStateHandler] Entering InGame State");
            
            // 创建QWE音频播放器
            CreateQWEAudioPlayer();
            
            // 初始化战斗控制器
            _battleController = new BattleController();
            _battleController.Initialize();
            
            // 打开战斗UI，传入战斗控制器
            var uiMgr = GameInstance.Instance.GetComp<IUIMgr>();
            if (uiMgr != null)
            {
                _battleUIController = uiMgr.Open<BattleUIController>(_battleController);
            }
            
            // 开始单局
            StartSession();
        }
        
        public void OnExit()
        {
            Debug.Log("[InGameStateHandler] Exiting InGame State");
            
            // 停止刷怪
            StopMonsterSpawning();
            
            // 停止背景音乐
            StopBackgroundMusic();
            
            // 销毁主玩家
            DestroyMainPlayer();
            
            // 销毁QWE音频播放器
            DestroyQWEAudioPlayer();
            
            // 关闭战斗UI
            var uiMgr = GameInstance.Instance.GetComp<IUIMgr>();
            if (uiMgr != null && _battleUIController != null)
            {
                uiMgr.Close(_battleUIController);
                _battleUIController = null;
            }
            
            // 清理战斗控制器
            _battleController?.Shutdown();
            _battleController = null;
        }
        
        public void OnUpdate(float deltaTime)
        {
            _battleController?.Update(deltaTime);
        }
        
        private void StartSession()
        {
            var session = GameInstance.Instance.GetComp<IGameSession>();
            if (session != null)
            {
                var config = new SessionConfig
                {
                    StartLevelId = 1,
                    AutoStartLevel = true
                };
                
                // 使用协程启动Session，完成后生成玩家并启动战斗
                session.StartSession(config, () =>
                {
                    _currentLevelId = config.StartLevelId;
                    
                    // 生成主玩家
                    SpawnMainPlayer();
                    
                    // 播放背景音乐
                    PlayBackgroundMusic(config.StartLevelId);
                    
                    // 开始刷怪
                    StartMonsterSpawning(config.StartLevelId);
                    
                    // 启动战斗
                    _battleController?.StartBattle(1);
                });
            }
            else
            {
                // 无Session时直接生成玩家并启动战斗
                _currentLevelId = 1;
                SpawnMainPlayer();
                PlayBackgroundMusic(1);
                StartMonsterSpawning(1);
                _battleController?.StartBattle(1);
            }
        }
        
        /// <summary>
        /// 生成主玩家
        /// </summary>
        private void SpawnMainPlayer()
        {
            var playerMdl = GameInstance.Instance.GetComp<IPlayerMdl>();
            if (playerMdl == null)
            {
                Debug.LogWarning("[InGameStateHandler] PlayerMdl not found");
                return;
            }
            
            // 根据PlayerConfig配置生成主玩家
            // 使用默认角色和配置中的生成位置
            var player = playerMdl.SpawnMainPlayer();
            
            if (player != null)
            {
                Debug.Log($"[InGameStateHandler] Main player spawned: {player.CharacterData?.characterName ?? "Unknown"}");
            }
            else
            {
                Debug.LogError("[InGameStateHandler] Failed to spawn main player");
            }
        }
        
        /// <summary>
        /// 销毁主玩家
        /// </summary>
        private void DestroyMainPlayer()
        {
            var playerMdl = GameInstance.Instance.GetComp<IPlayerMdl>();
            if (playerMdl != null && playerMdl.HasMainPlayer)
            {
                playerMdl.DestroyMainPlayer();
                Debug.Log("[InGameStateHandler] Main player destroyed");
            }
        }
        
        #region 背景音乐
        
        /// <summary>
        /// 播放关卡背景音乐
        /// </summary>
        private void PlayBackgroundMusic(int levelId)
        {
            // 使用 GameConfigs.ConfigManager 获取关卡数据
            var levelData = ConfigManager.GetLevel(levelId);
            if (levelData == null)
            {
                Debug.LogWarning($"[InGameStateHandler] Level {levelId} not found in LevelConfig");
                return;
            }
            
            if (!levelData.HasBackgroundMusic)
            {
                Debug.Log($"[InGameStateHandler] Level {levelId} has no background music configured");
                return;
            }
            
            // 获取音频管理器
            var audioMgr = GameInstance.Instance.GetComp<IAudioMgr>();
            if (audioMgr == null)
            {
                Debug.LogWarning("[InGameStateHandler] AudioMgr not found");
                return;
            }
            
            // 播放背景音乐
            var bgmClip = levelData.GetBackgroundMusic();
            if (bgmClip != null)
            {
                audioMgr.PlayBGM(bgmClip, levelData.loopBackgroundMusic, levelData.musicFadeInDuration);
                Debug.Log($"[InGameStateHandler] Playing background music: {bgmClip.name}");
            }
            else if (!string.IsNullOrEmpty(levelData.backgroundMusicName))
            {
                audioMgr.PlayBGM(levelData.backgroundMusicName, levelData.loopBackgroundMusic, levelData.musicFadeInDuration);
                Debug.Log($"[InGameStateHandler] Playing background music: {levelData.backgroundMusicName}");
            }
        }
        
        /// <summary>
        /// 停止背景音乐
        /// </summary>
        private void StopBackgroundMusic()
        {
            var audioMgr = GameInstance.Instance.GetComp<IAudioMgr>();
            if (audioMgr != null && audioMgr.IsBGMPlaying)
            {
                audioMgr.StopBGM(0.5f);
                Debug.Log("[InGameStateHandler] Background music stopped");
            }
        }
        
        #endregion
        
        #region QWE音频播放器
        
        /// <summary>
        /// 创建QWE音频播放器
        /// </summary>
        private void CreateQWEAudioPlayer()
        {
            // 检查是否已存在
            if (_qweAudioPlayer != null)
            {
                Debug.LogWarning("[InGameStateHandler] QWEAudioPlayer already exists");
                return;
            }
            
            // 创建GameObject并添加组件
            var audioPlayerGo = new GameObject("QWEAudioPlayer");
            _qweAudioPlayer = audioPlayerGo.AddComponent<QWEAudioPlayer>();
            
            Debug.Log("[InGameStateHandler] QWEAudioPlayer created");
        }
        
        /// <summary>
        /// 销毁QWE音频播放器
        /// </summary>
        private void DestroyQWEAudioPlayer()
        {
            if (_qweAudioPlayer != null)
            {
                Object.Destroy(_qweAudioPlayer.gameObject);
                _qweAudioPlayer = null;
                Debug.Log("[InGameStateHandler] QWEAudioPlayer destroyed");
            }
        }
        
        #endregion
        
        #region 刷怪控制
        
        /// <summary>
        /// 开始刷怪
        /// </summary>
        private void StartMonsterSpawning(int levelId)
        {
            // 获取关卡数据
            var levelData = ConfigManager.GetLevel(levelId);
            if (levelData == null || levelData.spawnConfig == null)
            {
                Debug.Log($"[InGameStateHandler] Level {levelId} has no spawn config");
                return;
            }
            
            // 创建或获取MonsterSpawner
            if (_monsterSpawner == null)
            {
                var spawnerGo = new GameObject("MonsterSpawner");
                _monsterSpawner = spawnerGo.AddComponent<MonsterSpawner>();
            }
            
            // 订阅刷怪事件
            _monsterSpawner.OnWaveStarted += OnWaveStarted;
            _monsterSpawner.OnWaveCompleted += OnWaveCompleted;
            _monsterSpawner.OnAllWavesCompleted += OnAllWavesCompleted;
            _monsterSpawner.OnMonsterSpawned += OnMonsterSpawned;
            
            // 开始刷怪
            _monsterSpawner.StartSpawning(levelData.spawnConfig);
            
            Debug.Log($"[InGameStateHandler] Monster spawning started for level {levelId}");
        }
        
        /// <summary>
        /// 停止刷怪
        /// </summary>
        private void StopMonsterSpawning()
        {
            if (_monsterSpawner != null)
            {
                // 取消订阅事件
                _monsterSpawner.OnWaveStarted -= OnWaveStarted;
                _monsterSpawner.OnWaveCompleted -= OnWaveCompleted;
                _monsterSpawner.OnAllWavesCompleted -= OnAllWavesCompleted;
                _monsterSpawner.OnMonsterSpawned -= OnMonsterSpawned;
                
                _monsterSpawner.StopSpawning();
                
                // 销毁MonsterSpawner
                Object.Destroy(_monsterSpawner.gameObject);
                _monsterSpawner = null;
            }
            
            // 销毁所有怪物
            var monsterMgr = GameInstance.Instance.GetComp<IMonsterMgr>();
            monsterMgr?.DestroyAllMonsters();
            
            Debug.Log("[InGameStateHandler] Monster spawning stopped");
        }
        
        /// <summary>
        /// 波次开始事件
        /// </summary>
        private void OnWaveStarted(int waveIndex, WaveConfig waveConfig)
        {
            Debug.Log($"[InGameStateHandler] Wave {waveIndex + 1} started: {waveConfig.waveName}");
        }
        
        /// <summary>
        /// 波次完成事件
        /// </summary>
        private void OnWaveCompleted(int waveIndex, WaveConfig waveConfig)
        {
            Debug.Log($"[InGameStateHandler] Wave {waveIndex + 1} completed: {waveConfig.waveName}");
        }
        
        /// <summary>
        /// 所有波次完成事件
        /// </summary>
        private void OnAllWavesCompleted()
        {
            Debug.Log("[InGameStateHandler] All waves completed! Level victory!");
            
            // 关卡胜利
            _battleController?.EndBattle(true);
        }
        
        /// <summary>
        /// 怪物生成事件
        /// </summary>
        private void OnMonsterSpawned(Monster monster, int waveIndex, int totalSpawned)
        {
            Debug.Log($"[InGameStateHandler] Monster spawned: {monster.MonsterName} (Wave: {waveIndex + 1}, Total: {totalSpawned})");
        }
        
        #endregion
    }
}
