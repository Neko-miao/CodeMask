// ================================================
// Game - 游戏状态管理器
// ================================================

using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace Game.States
{
    /// <summary>
    /// 游戏状态管理器 - 管理各种游戏状态的处理器
    /// </summary>
    public class GameStateMgr : GameComponent, IGameStateMgr
    {
        private Dictionary<GameState, IGameStateHandler> _handlers;
        private IGameStateHandler _currentHandler;
        private GameState _currentState;
        
        protected override void OnInit()
        {
            base.OnInit();
            
            _handlers = new Dictionary<GameState, IGameStateHandler>();
            
            // 注册状态处理器
            RegisterHandler(GameState.Menu, new MainMenuStateHandler());
            RegisterHandler(GameState.Playing, new InGameStateHandler());
            
            // 监听游戏状态变化
            GameInstance.Instance.OnStateChanged += HandleGameStateChanged;
            
            Debug.Log("[GameStateMgr] Initialized");
        }
        
        protected override void OnShutdown()
        {
            GameInstance.Instance.OnStateChanged -= HandleGameStateChanged;
            
            _currentHandler?.OnExit();
            _currentHandler = null;
            _handlers?.Clear();
            
            base.OnShutdown();
        }
        
        public void RegisterHandler(GameState state, IGameStateHandler handler)
        {
            _handlers[state] = handler;
        }
        
        public void UnregisterHandler(GameState state)
        {
            _handlers.Remove(state);
        }
        
        public IGameStateHandler GetCurrentHandler()
        {
            return _currentHandler;
        }
        
        protected override void OnTick(float deltaTime)
        {
            base.OnTick(deltaTime);
            _currentHandler?.OnUpdate(deltaTime);
        }
        
        private void HandleGameStateChanged(GameState oldState, GameState newState)
        {
            Debug.Log($"[GameStateMgr] State changed: {oldState} -> {newState}");
            
            // 退出旧状态
            if (_handlers.TryGetValue(oldState, out var oldHandler))
            {
                oldHandler.OnExit();
            }
            
            _currentState = newState;
            
            // 进入新状态
            if (_handlers.TryGetValue(newState, out var newHandler))
            {
                _currentHandler = newHandler;
                newHandler.OnEnter();
            }
            else
            {
                _currentHandler = null;
            }
        }
    }
}
