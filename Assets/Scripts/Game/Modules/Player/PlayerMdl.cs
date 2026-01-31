// ================================================
// Game - 玩家模块实现
// ================================================

using System;
using System.Collections.Generic;
using GameFramework.Core;
using GameFramework.Components;
using GameConfigs;
using UnityEngine;

namespace Game.Modules
{
    /// <summary>
    /// 玩家模块实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Module, Priority = 200, RequiredStates = new[] { GameState.Playing })]
    public class PlayerMdl : GameComponent, IPlayerMdl
    {
        private PlayerData _data;
        private IPlayerController _mainPlayer;
        private int _respawnCount;
        private int _currentModelId = -1;
        private GameObject _currentModel;
        
        public override string ComponentName => "PlayerMdl";
        public override ComponentType ComponentType => ComponentType.Module;
        public override int Priority => 200;
        
        #region 账户属性
        
        public int PlayerId => _data?.PlayerId ?? 0;
        public string PlayerName => _data?.PlayerName ?? "Player";
        public int Level => _data?.Level ?? 1;
        public int Exp => _data?.Exp ?? 0;
        public int ExpToNextLevel => _data?.ExpToNextLevel ?? 100;
        public long Currency => _data?.Currency ?? 0;
        
        #endregion
        
        #region 主玩家属性
        
        public IPlayerController MainPlayer => _mainPlayer;
        public bool HasMainPlayer => _mainPlayer != null && _mainPlayer.GameObject != null;
        public bool IsMainPlayerAlive => HasMainPlayer && _mainPlayer.IsAlive;
        public Vector3 MainPlayerPosition => HasMainPlayer ? _mainPlayer.Position : Vector3.zero;
        public int MainPlayerHealth => HasMainPlayer ? _mainPlayer.CurrentHealth : 0;
        public int MainPlayerMaxHealth => HasMainPlayer ? _mainPlayer.MaxHealth : 0;
        public int MainPlayerModelId => _currentModelId;
        public GameObject MainPlayerModel => _currentModel;
        
        #endregion
        
        #region 配置访问
        
        public PlayerConfig Config => ConfigManager.PlayerConfig;
        
        public PlayerCharacterData GetCharacterData(int characterId)
        {
            return Config?.GetCharacter(characterId);
        }
        
        public PlayerCharacterData SelectedCharacterData => GetCharacterData(_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
        
        #endregion
        
        #region 事件
        
        public event Action<int, int> OnExpChanged;
        public event Action<int> OnLevelUp;
        public event Action<long> OnCurrencyChanged;
        public event Action<IPlayerController> OnMainPlayerSpawned;
        public event Action OnMainPlayerDestroyed;
        public event Action OnMainPlayerDeath;
        public event Action<IPlayerController> OnMainPlayerRespawned;
        public event Action<int, int> OnMainPlayerHealthChanged;
        public event Action<int> OnCharacterSelected;
        public event Action<int, GameObject> OnMainPlayerModelChanged;
        
        #endregion
        
        #region 生命周期
        
        protected override void OnInit()
        {
            _data = new PlayerData
            {
                PlayerId = 1,
                PlayerName = "Player",
                Level = 1,
                Exp = 0,
                ExpToNextLevel = Config?.GetExpToNextLevel(1) ?? 100,
                Currency = 1000,
                VipLevel = 0,
                SelectedCharacterId = Config?.defaultCharacterId ?? 1
            };
            
            _respawnCount = 0;
            _currentModelId = -1;
            _currentModel = null;
        }
        
        protected override void OnShutdown()
        {
            DestroyMainPlayer();
            _currentModel = null;
        }
        
        #endregion
        
        #region 主玩家操作
        
        public IPlayerController SpawnMainPlayer(int characterId = -1, Vector3? spawnPosition = null)
        {
            // 如果已有主玩家，先销毁
            if (HasMainPlayer)
            {
                DestroyMainPlayer();
            }
            
            // 获取角色数据
            int charId = characterId >= 0 ? characterId : (_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
            var characterData = GetCharacterData(charId);
            
            if (characterData == null)
            {
                Debug.LogError($"[PlayerMdl] Character data not found: {charId}");
                return null;
            }
            
            // 确定生成位置
            Vector3 spawnPos = DetermineSpawnPosition(spawnPosition);
            
            // 创建玩家实例
            GameObject playerObj = CreatePlayerGameObject(characterData, spawnPos);
            if (playerObj == null)
            {
                Debug.LogError("[PlayerMdl] Failed to create player GameObject");
                return null;
            }
            
            // 获取并初始化控制器
            var controller = playerObj.GetComponent<IPlayerController>();
            if (controller == null)
            {
                // 如果没有控制器组件，添加一个
                controller = playerObj.AddComponent<PlayerController>();
            }
            
            controller.Initialize(characterData);
            controller.Activate();
            
            // 应用等级加成
            ApplyLevelBonusToPlayer(controller);
            
            // 设置生成后无敌
            if (Config?.spawnConfig != null && Config.spawnConfig.spawnInvincibleTime > 0)
            {
                controller.SetInvincible(true, Config.spawnConfig.spawnInvincibleTime);
            }
            
            _mainPlayer = controller;
            
            // 订阅事件
            SubscribePlayerEvents(controller);
            
            Debug.Log($"[PlayerMdl] Main player spawned at {spawnPos}, Character: {characterData.characterName}");
            
            OnMainPlayerSpawned?.Invoke(controller);
            
            // 发布事件
            GetComp<IEventMgr>()?.Publish(new MainPlayerSpawnedEvent { Player = controller });
            
            return controller;
        }
        
        public void DestroyMainPlayer()
        {
            if (_mainPlayer == null) return;
            
            // 取消订阅事件
            UnsubscribePlayerEvents(_mainPlayer);
            
            _mainPlayer.Destroy();
            _mainPlayer = null;
            
            Debug.Log("[PlayerMdl] Main player destroyed");
            
            OnMainPlayerDestroyed?.Invoke();
            
            // 发布事件
            GetComp<IEventMgr>()?.Publish(new MainPlayerDestroyedEvent());
        }
        
        public void RespawnMainPlayer(Vector3? spawnPosition = null)
        {
            var spawnConfig = Config?.spawnConfig;
            
            // 检查是否允许重生
            if (spawnConfig != null && !spawnConfig.allowRespawn)
            {
                Debug.Log("[PlayerMdl] Respawn not allowed");
                return;
            }
            
            // 检查重生次数
            if (spawnConfig != null && spawnConfig.maxRespawnCount >= 0 && _respawnCount >= spawnConfig.maxRespawnCount)
            {
                Debug.Log("[PlayerMdl] Max respawn count reached");
                return;
            }
            
            // 保存当前角色ID
            int characterId = _data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1;
            
            // 销毁当前玩家
            DestroyMainPlayer();
            
            // 重新生成
            var controller = SpawnMainPlayer(characterId, spawnPosition);
            if (controller != null)
            {
                _respawnCount++;
                
                Debug.Log($"[PlayerMdl] Main player respawned (count: {_respawnCount})");
                
                OnMainPlayerRespawned?.Invoke(controller);
                
                // 发布事件
                GetComp<IEventMgr>()?.Publish(new MainPlayerRespawnedEvent { Player = controller });
            }
        }
        
        private Vector3 DetermineSpawnPosition(Vector3? customPosition)
        {
            // 优先使用自定义位置
            if (customPosition.HasValue)
            {
                return customPosition.Value;
            }
            
            var spawnConfig = Config?.spawnConfig;
            
            // 尝试使用场景生成点
            if (spawnConfig != null && spawnConfig.useSceneSpawnPoint)
            {
                var spawnPoint = GameObject.FindGameObjectWithTag(spawnConfig.spawnPointTag);
                if (spawnPoint != null)
                {
                    return spawnPoint.transform.position;
                }
            }
            
            // 使用默认位置
            return spawnConfig?.defaultSpawnPosition ?? Vector3.zero;
        }
        
        private GameObject CreatePlayerGameObject(PlayerCharacterData characterData, Vector3 position)
        {
            GameObject playerObj = null;
            
            // 使用新版方法加载玩家预制体
            var prefab = characterData.LoadPlayerPrefab();
            if (prefab != null)
            {
                playerObj = UnityEngine.Object.Instantiate(prefab, position, Quaternion.identity);
            }
            
            // 如果都没有，创建一个基础对象
            if (playerObj == null)
            {
                playerObj = new GameObject($"Player_{characterData.characterName}");
                playerObj.transform.position = position;
                
                // 添加基础组件
                playerObj.AddComponent<SpriteRenderer>();
                var rb = playerObj.AddComponent<Rigidbody2D>();
                rb.gravityScale = 1f;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                playerObj.AddComponent<BoxCollider2D>();
                playerObj.AddComponent<PlayerController>();
            }
            
            playerObj.name = $"MainPlayer_{characterData.characterName}";
            playerObj.tag = "Player";
            
            // 加载默认模型
            var defaultModelData = characterData.GetDefaultModel();
            if (defaultModelData != null)
            {
                _currentModel = InstantiateModelInternal(defaultModelData, playerObj.transform);
                _currentModelId = defaultModelData.modelId;
            }
            
            return playerObj;
        }
        
        private void ApplyLevelBonusToPlayer(IPlayerController controller)
        {
            if (Config == null || controller == null) return;
            
            var levelData = Config.GetLevelData(Level);
            if (levelData != null)
            {
                // 应用等级加成到血量
                int bonusHealth = levelData.baseHealth - controller.CharacterData.baseStats.maxHealth;
                if (bonusHealth > 0)
                {
                    controller.SetMaxHealth(controller.MaxHealth + bonusHealth, true);
                }
                
                // 可以在这里添加其他等级加成
            }
        }
        
        private void SubscribePlayerEvents(IPlayerController controller)
        {
            if (controller == null) return;
            
            controller.OnHealthChanged += HandleMainPlayerHealthChanged;
            controller.OnDeath += HandleMainPlayerDeath;
        }
        
        private void UnsubscribePlayerEvents(IPlayerController controller)
        {
            if (controller == null) return;
            
            controller.OnHealthChanged -= HandleMainPlayerHealthChanged;
            controller.OnDeath -= HandleMainPlayerDeath;
        }
        
        private void HandleMainPlayerHealthChanged(int oldHealth, int newHealth)
        {
            OnMainPlayerHealthChanged?.Invoke(oldHealth, newHealth);
        }
        
        private void HandleMainPlayerDeath()
        {
            Debug.Log("[PlayerMdl] Main player died");
            
            OnMainPlayerDeath?.Invoke();
            
            // 发布死亡事件
            GetComp<IEventMgr>()?.Publish(new MainPlayerDeathEvent());
            
            // 检查是否自动重生
            var spawnConfig = Config?.spawnConfig;
            if (spawnConfig != null && spawnConfig.allowRespawn)
            {
                // 延迟重生
                if (spawnConfig.respawnDelay > 0)
                {
                    // 使用协程延迟重生（需要GameInstance支持）
                    GameInstance.Instance?.RunCoroutine(DelayedRespawnCoroutine(spawnConfig.respawnDelay));
                }
                else
                {
                    RespawnMainPlayer();
                }
            }
        }
        
        private System.Collections.IEnumerator DelayedRespawnCoroutine(float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            RespawnMainPlayer();
        }
        
        #endregion
        
        #region 玩家模型管理
        
        public GameObject LoadPlayerPrefab(int characterId = -1)
        {
            int charId = characterId >= 0 ? characterId : (_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
            var characterData = GetCharacterData(charId);
            
            if (characterData == null)
            {
                Debug.LogError($"[PlayerMdl] Character data not found: {charId}");
                return null;
            }
            
            return characterData.LoadPlayerPrefab();
        }
        
        public GameObject LoadPlayerModelPrefab(int characterId = -1, int modelId = -1)
        {
            int charId = characterId >= 0 ? characterId : (_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
            var characterData = GetCharacterData(charId);
            
            if (characterData == null)
            {
                Debug.LogError($"[PlayerMdl] Character data not found: {charId}");
                return null;
            }
            
            PlayerModelData modelData;
            if (modelId >= 0)
            {
                modelData = characterData.GetModel(modelId);
            }
            else
            {
                modelData = characterData.GetDefaultModel();
            }
            
            if (modelData == null)
            {
                Debug.LogWarning($"[PlayerMdl] Model data not found: character={charId}, model={modelId}");
                return null;
            }
            
            return modelData.LoadPrefab();
        }
        
        public GameObject InstantiatePlayerPrefab(int characterId = -1, Vector3? position = null, Quaternion? rotation = null, Transform parent = null)
        {
            var prefab = LoadPlayerPrefab(characterId);
            if (prefab == null)
            {
                Debug.LogError("[PlayerMdl] Failed to load player prefab");
                return null;
            }
            
            Vector3 pos = position ?? Vector3.zero;
            Quaternion rot = rotation ?? Quaternion.identity;
            
            GameObject instance;
            if (parent != null)
            {
                instance = UnityEngine.Object.Instantiate(prefab, parent);
                instance.transform.localPosition = pos;
                instance.transform.localRotation = rot;
            }
            else
            {
                instance = UnityEngine.Object.Instantiate(prefab, pos, rot);
            }
            
            return instance;
        }
        
        public GameObject InstantiatePlayerModel(int characterId = -1, int modelId = -1, Transform parent = null)
        {
            int charId = characterId >= 0 ? characterId : (_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
            var characterData = GetCharacterData(charId);
            
            if (characterData == null)
            {
                Debug.LogError($"[PlayerMdl] Character data not found: {charId}");
                return null;
            }
            
            PlayerModelData modelData;
            if (modelId >= 0)
            {
                modelData = characterData.GetModel(modelId);
            }
            else
            {
                modelData = characterData.GetDefaultModel();
            }
            
            if (modelData == null)
            {
                Debug.LogWarning($"[PlayerMdl] Model data not found: character={charId}, model={modelId}");
                return null;
            }
            
            return InstantiateModelInternal(modelData, parent);
        }
        
        public bool ChangeMainPlayerModel(int modelId)
        {
            if (!HasMainPlayer)
            {
                Debug.LogWarning("[PlayerMdl] No main player to change model");
                return false;
            }
            
            if (_currentModelId == modelId)
            {
                Debug.Log($"[PlayerMdl] Model {modelId} is already active");
                return true;
            }
            
            var characterData = SelectedCharacterData;
            if (characterData == null)
            {
                Debug.LogError("[PlayerMdl] Selected character data not found");
                return false;
            }
            
            var modelData = characterData.GetModel(modelId);
            if (modelData == null)
            {
                Debug.LogError($"[PlayerMdl] Model not found: {modelId}");
                return false;
            }
            
            // 销毁当前模型
            if (_currentModel != null)
            {
                UnityEngine.Object.Destroy(_currentModel);
                _currentModel = null;
            }
            
            // 实例化新模型
            _currentModel = InstantiateModelInternal(modelData, _mainPlayer.Transform);
            if (_currentModel == null)
            {
                Debug.LogError($"[PlayerMdl] Failed to instantiate model: {modelId}");
                return false;
            }
            
            int oldModelId = _currentModelId;
            _currentModelId = modelId;
            
            Debug.Log($"[PlayerMdl] Changed model from {oldModelId} to {modelId}");
            
            OnMainPlayerModelChanged?.Invoke(modelId, _currentModel);
            
            // 发布模型切换事件
            GetComp<IEventMgr>()?.Publish(new MainPlayerModelChangedEvent 
            { 
                OldModelId = oldModelId, 
                NewModelId = modelId, 
                Model = _currentModel 
            });
            
            return true;
        }
        
        public IReadOnlyList<PlayerModelData> GetAvailableModels(int characterId = -1)
        {
            int charId = characterId >= 0 ? characterId : (_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
            var characterData = GetCharacterData(charId);
            
            return characterData?.GetAllModels();
        }
        
        public PlayerModelData GetDefaultModel(int characterId = -1)
        {
            int charId = characterId >= 0 ? characterId : (_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
            var characterData = GetCharacterData(charId);
            
            return characterData?.GetDefaultModel();
        }
        
        public PlayerModelData GetModelData(int characterId, int modelId)
        {
            int charId = characterId >= 0 ? characterId : (_data?.SelectedCharacterId ?? Config?.defaultCharacterId ?? 1);
            var characterData = GetCharacterData(charId);
            
            return characterData?.GetModel(modelId);
        }
        
        private GameObject InstantiateModelInternal(PlayerModelData modelData, Transform parent)
        {
            if (modelData == null) return null;
            
            var prefab = modelData.LoadPrefab();
            if (prefab == null)
            {
                Debug.LogWarning($"[PlayerMdl] Model prefab not found: {modelData.modelName}");
                return null;
            }
            
            GameObject modelInstance;
            if (parent != null)
            {
                modelInstance = UnityEngine.Object.Instantiate(prefab, parent);
            }
            else
            {
                modelInstance = UnityEngine.Object.Instantiate(prefab);
            }
            
            modelInstance.name = $"Model_{modelData.modelName}";
            
            // 应用模型配置
            modelInstance.transform.localPosition = modelData.offset;
            modelInstance.transform.localRotation = Quaternion.Euler(modelData.rotationOffset);
            modelInstance.transform.localScale = modelData.scale;
            
            return modelInstance;
        }
        
        #endregion
        
        #region 账户操作
        
        public PlayerData GetPlayerData()
        {
            return _data;
        }
        
        public void SetPlayerData(PlayerData data)
        {
            _data = data ?? new PlayerData();
        }
        
        public void AddExp(int amount)
        {
            if (amount <= 0) return;
            
            int oldExp = _data.Exp;
            _data.Exp += amount;
            
            // 检查升级
            while (_data.Exp >= _data.ExpToNextLevel)
            {
                _data.Exp -= _data.ExpToNextLevel;
                _data.Level++;
                _data.ExpToNextLevel = Config?.GetExpToNextLevel(_data.Level) ?? CalculateExpToNextLevel(_data.Level);
                
                OnLevelUp?.Invoke(_data.Level);
                
                // 发布升级事件
                GetComp<IEventMgr>()?.Publish(new PlayerLevelUpEvent { Level = _data.Level });
                
                // 更新主玩家属性
                if (HasMainPlayer)
                {
                    ApplyLevelBonusToPlayer(_mainPlayer);
                }
            }
            
            OnExpChanged?.Invoke(oldExp, _data.Exp);
        }
        
        public void AddCurrency(long amount)
        {
            if (amount <= 0) return;
            
            _data.Currency += amount;
            OnCurrencyChanged?.Invoke(_data.Currency);
        }
        
        public bool ConsumeCurrency(long amount)
        {
            if (amount <= 0) return true;
            
            if (_data.Currency < amount)
            {
                Debug.LogWarning("[PlayerMdl] Not enough currency");
                return false;
            }
            
            _data.Currency -= amount;
            OnCurrencyChanged?.Invoke(_data.Currency);
            return true;
        }
        
        public void SetName(string name)
        {
            if (string.IsNullOrEmpty(name)) return;
            _data.PlayerName = name;
        }
        
        public void SelectCharacter(int characterId)
        {
            var characterData = GetCharacterData(characterId);
            if (characterData == null)
            {
                Debug.LogWarning($"[PlayerMdl] Character not found: {characterId}");
                return;
            }
            
            _data.SelectedCharacterId = characterId;
            
            OnCharacterSelected?.Invoke(characterId);
            
            Debug.Log($"[PlayerMdl] Character selected: {characterData.characterName}");
        }
        
        #endregion
        
        #region 私有方法
        
        private int CalculateExpToNextLevel(int level)
        {
            // 简单的经验公式: 100 * level^1.5
            return Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
        }
        
        #endregion
    }
    
    #region 事件定义
    
    /// <summary>
    /// 玩家升级事件
    /// </summary>
    public struct PlayerLevelUpEvent
    {
        public int Level;
    }
    
    /// <summary>
    /// 主玩家生成事件
    /// </summary>
    public struct MainPlayerSpawnedEvent
    {
        public IPlayerController Player;
    }
    
    /// <summary>
    /// 主玩家销毁事件
    /// </summary>
    public struct MainPlayerDestroyedEvent
    {
    }
    
    /// <summary>
    /// 主玩家死亡事件
    /// </summary>
    public struct MainPlayerDeathEvent
    {
    }
    
    /// <summary>
    /// 主玩家重生事件
    /// </summary>
    public struct MainPlayerRespawnedEvent
    {
        public IPlayerController Player;
    }
    
    /// <summary>
    /// 主玩家模型切换事件
    /// </summary>
    public struct MainPlayerModelChangedEvent
    {
        public int OldModelId;
        public int NewModelId;
        public GameObject Model;
    }
    
    #endregion
}
