// ================================================
// GameFramework - 玩家上下文实现
// ================================================

using System.Collections.Generic;
using GameFramework.Entity;

namespace GameFramework.World
{
    /// <summary>
    /// 玩家上下文实现
    /// </summary>
    public class PlayerContext : IPlayerContext
    {
        private readonly Dictionary<int, IEntity> _players = new Dictionary<int, IEntity>();
        private readonly List<IEntity> _playerList = new List<IEntity>();
        
        private int _localPlayerId = -1;
        private IEntity _localPlayer;
        
        public int LocalPlayerId => _localPlayerId;
        public IEntity LocalPlayer => _localPlayer;
        public int PlayerCount => _players.Count;
        
        public bool IsLocalPlayer(IEntity entity)
        {
            return entity != null && entity == _localPlayer;
        }
        
        public bool IsLocalPlayer(int entityId)
        {
            return _localPlayer != null && _localPlayer.Id == entityId;
        }
        
        public IEntity GetPlayerEntity(int playerId)
        {
            _players.TryGetValue(playerId, out var entity);
            return entity;
        }
        
        public IReadOnlyList<IEntity> GetAllPlayerEntities()
        {
            return _playerList;
        }
        
        public void SetLocalPlayer(IEntity player)
        {
            _localPlayer = player;
            _localPlayerId = player?.Id ?? -1;
            
            if (player != null)
            {
                // 确保已注册
                if (!_players.ContainsKey(player.Id))
                {
                    RegisterPlayer(player.Id, player);
                }
                
                // 添加 Player 标签
                player.AddTag(EntityTag.Player);
            }
        }
        
        public void RegisterPlayer(int playerId, IEntity entity)
        {
            if (entity == null) return;
            
            _players[playerId] = entity;
            if (!_playerList.Contains(entity))
            {
                _playerList.Add(entity);
            }
            
            entity.AddTag(EntityTag.Player);
        }
        
        public void UnregisterPlayer(int playerId)
        {
            if (_players.TryGetValue(playerId, out var entity))
            {
                _players.Remove(playerId);
                _playerList.Remove(entity);
                
                if (_localPlayer == entity)
                {
                    _localPlayer = null;
                    _localPlayerId = -1;
                }
            }
        }
        
        public void Clear()
        {
            _players.Clear();
            _playerList.Clear();
            _localPlayer = null;
            _localPlayerId = -1;
        }
    }
}
