// ================================================
// Game - 玩家模块实现
// ================================================

using System;
using GameFramework.Core;
using GameFramework.Components;
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
        
        public override string ComponentName => "PlayerMdl";
        public override ComponentType ComponentType => ComponentType.Module;
        public override int Priority => 200;
        
        #region Properties
        
        public int PlayerId => _data?.PlayerId ?? 0;
        public string PlayerName => _data?.PlayerName ?? "Player";
        public int Level => _data?.Level ?? 1;
        public int Exp => _data?.Exp ?? 0;
        public int ExpToNextLevel => _data?.ExpToNextLevel ?? 100;
        public long Currency => _data?.Currency ?? 0;
        
        #endregion
        
        #region Events
        
        public event Action<int, int> OnExpChanged;
        public event Action<int> OnLevelUp;
        public event Action<long> OnCurrencyChanged;
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            _data = new PlayerData
            {
                PlayerId = 1,
                PlayerName = "Player",
                Level = 1,
                Exp = 0,
                ExpToNextLevel = 100,
                Currency = 1000,
                VipLevel = 0
            };
        }
        
        #endregion
        
        #region Public Methods
        
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
                _data.ExpToNextLevel = CalculateExpToNextLevel(_data.Level);
                
                OnLevelUp?.Invoke(_data.Level);
                
                // 发布升级事件
                GetComp<IEventMgr>()?.Publish(new PlayerLevelUpEvent { Level = _data.Level });
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
        
        #endregion
        
        #region Private Methods
        
        private int CalculateExpToNextLevel(int level)
        {
            // 简单的经验公式: 100 * level^1.5
            return Mathf.RoundToInt(100 * Mathf.Pow(level, 1.5f));
        }
        
        #endregion
    }
    
    /// <summary>
    /// 玩家升级事件
    /// </summary>
    public struct PlayerLevelUpEvent
    {
        public int Level;
    }
}

