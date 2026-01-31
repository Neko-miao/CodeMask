// ================================================
// Game - 战斗界面控制器
// ================================================

using UnityEngine;
using GameFramework.Core;
using GameFramework.UI;
using Game.Battle;

namespace Game.UI
{
    /// <summary>
    /// 战斗界面控制器 - MVC核心，持有Model和View
    /// </summary>
    public class BattleUIController : UIControllerBase<BattleView, BattleModel>
    {
        private BattleController _battleController;
        
        public void SetBattleController(BattleController battleController)
        {
            // 解绑旧的
            UnbindBattleEvents();
            
            _battleController = battleController;
            
            // 绑定新的
            if (_battleController != null)
            {
                BindBattleEvents();
                InitializeFromBattleController();
            }
        }
        
        protected override void OnModelInit(object data)
        {
            Model.Initialize();
            
            // 如果传入了 BattleController，则绑定
            if (data is BattleController bc)
            {
                SetBattleController(bc);
            }
        }
        
        protected override void OnBindEvents()
        {
            // View 事件
            View.OnPauseClick += HandlePause;
            View.OnQuitClick += HandleQuit;
            View.OnHitInput += HandleHitInput;
            View.OnMaskSwitch += HandleMaskSwitch;
        }
        
        protected override void OnUnbindEvents()
        {
            View.OnPauseClick -= HandlePause;
            View.OnQuitClick -= HandleQuit;
            View.OnHitInput -= HandleHitInput;
            View.OnMaskSwitch -= HandleMaskSwitch;
            
            UnbindBattleEvents();
        }
        
        protected override void OnUpdateView()
        {
            View.UpdatePlayerHealthUI(Model.Player?.CurrentHealth ?? 0, Model.Player?.MaxHealth ?? 1);
            View.UpdatePlayerMaskUI(Model.Player?.CurrentMask ?? MaskType.None);
            
            if (Model.Enemy != null)
            {
                View.UpdateEnemyUI(Model.Enemy.Name, Model.Enemy.CurrentHealth, Model.Enemy.MaxHealth);
            }
            
            View.UpdateComboUI(Model.Combo);
            View.UpdateLevelUI(Model.CurrentLevel);
            View.UpdateResultUI(Model.IsVictory, Model.IsDefeat, Model.ResultMessage);
        }
        
        protected override void OnDestroy()
        {
            UnbindBattleEvents();
            _battleController = null;
        }
        
        #region Battle Event Binding
        
        private void BindBattleEvents()
        {
            if (_battleController == null) return;
            
            _battleController.OnBattleStateChanged += OnBattleStateChanged;
            _battleController.OnEnemySpawned += OnEnemySpawned;
            _battleController.OnEnemyDefeated += OnEnemyDefeated;
            _battleController.OnHitResult += OnHitResult;
            _battleController.OnComboChanged += OnComboChanged;
            _battleController.OnVictory += OnVictory;
            _battleController.OnDefeat += OnDefeat;
            
            if (_battleController.Player != null)
            {
                _battleController.Player.OnHealthChanged += OnPlayerHealthChanged;
                _battleController.Player.OnMaskChanged += OnPlayerMaskChanged;
            }
        }
        
        private void UnbindBattleEvents()
        {
            if (_battleController == null) return;
            
            _battleController.OnBattleStateChanged -= OnBattleStateChanged;
            _battleController.OnEnemySpawned -= OnEnemySpawned;
            _battleController.OnEnemyDefeated -= OnEnemyDefeated;
            _battleController.OnHitResult -= OnHitResult;
            _battleController.OnComboChanged -= OnComboChanged;
            _battleController.OnVictory -= OnVictory;
            _battleController.OnDefeat -= OnDefeat;
            
            if (_battleController.Player != null)
            {
                _battleController.Player.OnHealthChanged -= OnPlayerHealthChanged;
                _battleController.Player.OnMaskChanged -= OnPlayerMaskChanged;
            }
        }
        
        private void InitializeFromBattleController()
        {
            if (_battleController == null) return;
            
            Model.UpdateBattleState(_battleController.State, _battleController.CurrentLevel);
            
            if (_battleController.Player != null)
            {
                Model.UpdatePlayerHealth(_battleController.Player.CurrentHealth, _battleController.Player.MaxHealth);
                Model.UpdatePlayerMask(_battleController.Player.CurrentMask);
            }
            
            if (_battleController.CurrentEnemy != null)
            {
                Model.UpdateEnemy(_battleController.CurrentEnemy);
            }
        }
        
        #endregion
        
        #region View Event Handlers
        
        private void HandlePause()
        {
            if (_battleController == null) return;
            
            if (_battleController.State == BattleState.Fighting)
            {
                _battleController.PauseBattle();
                GameInstance.Instance.Pause();
            }
            else if (_battleController.State == BattleState.Paused)
            {
                _battleController.ResumeBattle();
                GameInstance.Instance.Resume();
            }
        }
        
        private void HandleQuit()
        {
            GameInstance.Instance.ChangeState(GameState.Menu);
        }
        
        private void HandleHitInput()
        {
            if (_battleController == null || _battleController.State != BattleState.Fighting) return;
            _battleController.OnHitInput();
        }
        
        private void HandleMaskSwitch(int index)
        {
            if (_battleController == null || _battleController.State != BattleState.Fighting) return;
            _battleController.OnMaskSwitch(index);
        }
        
        #endregion
        
        #region Battle Event Handlers
        
        private void OnBattleStateChanged(BattleState oldState, BattleState newState)
        {
            Model.UpdateBattleState(newState, _battleController.CurrentLevel);
        }
        
        private void OnEnemySpawned(EnemyFighter enemy)
        {
            Model.UpdateEnemy(enemy);
            enemy.OnHealthChanged += OnEnemyHealthChanged;
        }
        
        private void OnEnemyDefeated(EnemyFighter enemy)
        {
            enemy.OnHealthChanged -= OnEnemyHealthChanged;
            Model.SetEnemyDefeatedMessage(enemy.Name, enemy.CurrentMask);
        }
        
        private void OnHitResult(HitResult result, int damage)
        {
            Model.UpdateHitResult(result, damage);
            View.ShowHitResult(result, damage);
        }
        
        private void OnComboChanged(int combo)
        {
            Model.UpdateCombo(combo);
        }
        
        private void OnVictory()
        {
            Model.SetVictory();
        }
        
        private void OnDefeat()
        {
            Model.SetDefeat();
        }
        
        private void OnPlayerHealthChanged(int oldHealth, int newHealth)
        {
            if (_battleController?.Player == null) return;
            Model.UpdatePlayerHealth(newHealth, _battleController.Player.MaxHealth);
        }
        
        private void OnPlayerMaskChanged(MaskType oldMask, MaskType newMask)
        {
            Model.UpdatePlayerMask(newMask);
        }
        
        private void OnEnemyHealthChanged(int oldHealth, int newHealth)
        {
            if (_battleController?.CurrentEnemy == null) return;
            Model.UpdateEnemyHealth(newHealth, _battleController.CurrentEnemy.MaxHealth);
        }
        
        #endregion
    }
}
