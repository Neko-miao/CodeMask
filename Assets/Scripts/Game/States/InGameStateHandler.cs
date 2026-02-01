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
        
        public void OnEnter()
        {
            Debug.Log("[InGameStateHandler] Entering InGame State");
            
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
            
            // 停止背景音乐
            StopBackgroundMusic();
            
            // 销毁主玩家
            DestroyMainPlayer();
            
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
                    // 生成主玩家
                    SpawnMainPlayer();
                    
                    // 播放背景音乐
                    PlayBackgroundMusic(config.StartLevelId);
                    
                    // 启动战斗
                    _battleController?.StartBattle(1);
                });
            }
            else
            {
                // 无Session时直接生成玩家并启动战斗
                SpawnMainPlayer();
                PlayBackgroundMusic(1);
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
    }
}
