// ================================================
// Game - 战斗界面 (MVC - View)
// ================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using GameFramework.UI;
using Game.Battle;

namespace Game.UI
{
    /// <summary>
    /// 战斗界面 - 只负责UI元素绑定和显示，不持有Controller引用
    /// </summary>
    public class BattleView : UIViewBase
    {
        [Header("Player Info")]
        [SerializeField] private Slider _playerHealthBar;
        [SerializeField] private Text _playerHealthText;
        [SerializeField] private Image[] _maskSlots;
        [SerializeField] private Text _currentMaskText;
        
        [Header("Enemy Info")]
        [SerializeField] private Slider _enemyHealthBar;
        [SerializeField] private Text _enemyHealthText;
        [SerializeField] private Text _enemyNameText;
        [SerializeField] private Image _enemyMaskIcon;
        
        [Header("Battle Info")]
        [SerializeField] private Text _comboText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _resultText;
        
        [Header("Rhythm")]
        [SerializeField] private RectTransform _rhythmContainer;
        [SerializeField] private Image _hitZone;
        [SerializeField] private Text _hitResultText;
        
        [Header("Buttons")]
        [SerializeField] private Button _pauseBtn;
        [SerializeField] private Button _quitBtn;
        
        #region Events (供 Controller 绑定)
        
        public event Action OnPauseClick;
        public event Action OnQuitClick;
        public event Action OnHitInput;
        public event Action<int> OnMaskSwitch;
        
        #endregion
        
        protected override void OnInit()
        {
            base.OnInit();
            
            // 绑定按钮事件
            if (_pauseBtn != null)
                _pauseBtn.onClick.AddListener(() => OnPauseClick?.Invoke());
            
            if (_quitBtn != null)
                _quitBtn.onClick.AddListener(() => OnQuitClick?.Invoke());
            
            Debug.Log("[BattleView] Initialized");
        }
        
        protected override void OnOpen(object data)
        {
            base.OnOpen(data);
            
            // 初始隐藏结果
            if (_resultText != null)
                _resultText.gameObject.SetActive(false);
            
            if (_hitResultText != null)
                _hitResultText.text = "";
        }
        
        protected override void OnDestroyInternal()
        {
            if (_pauseBtn != null)
                _pauseBtn.onClick.RemoveAllListeners();
            
            if (_quitBtn != null)
                _quitBtn.onClick.RemoveAllListeners();
            
            base.OnDestroyInternal();
        }
        
        private void Update()
        {
            if (!IsVisible) return;
            HandleInput();
        }
        
        #region Input Handling
        
        private void HandleInput()
        {
            // Space - 卡点
            if (Input.GetKeyDown(KeyCode.Space))
            {
                OnHitInput?.Invoke();
            }
            
            // Q/W/E - 切换面具
            if (Input.GetKeyDown(KeyCode.Q))
            {
                OnMaskSwitch?.Invoke(0);
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                OnMaskSwitch?.Invoke(1);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                OnMaskSwitch?.Invoke(2);
            }
            
            // Escape - 暂停
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                OnPauseClick?.Invoke();
            }
        }
        
        #endregion
        
        #region UI Update Methods (供 Controller 调用)
        
        public void UpdatePlayerHealthUI(int current, int max)
        {
            if (_playerHealthBar != null)
            {
                _playerHealthBar.maxValue = max;
                _playerHealthBar.value = current;
            }
            
            if (_playerHealthText != null)
            {
                _playerHealthText.text = $"{current}/{max}";
            }
        }
        
        public void UpdatePlayerMaskUI(MaskType mask)
        {
            if (_currentMaskText != null)
            {
                var maskData = MaskConfig.GetMaskData(mask);
                _currentMaskText.text = maskData?.Name ?? "无面具";
            }
        }
        
        public void UpdateEnemyUI(string name, int currentHealth, int maxHealth)
        {
            if (_enemyNameText != null)
            {
                _enemyNameText.text = name;
            }
            
            if (_enemyHealthBar != null)
            {
                _enemyHealthBar.maxValue = maxHealth;
                _enemyHealthBar.value = currentHealth;
            }
            
            if (_enemyHealthText != null)
            {
                _enemyHealthText.text = $"{currentHealth}/{maxHealth}";
            }
        }
        
        public void UpdateComboUI(int combo)
        {
            if (_comboText != null)
            {
                _comboText.text = combo > 0 ? $"Combo x{combo}" : "";
            }
        }
        
        public void UpdateLevelUI(int level)
        {
            if (_levelText != null)
            {
                _levelText.text = $"关卡 {level}";
            }
        }
        
        public void UpdateResultUI(bool isVictory, bool isDefeat, string message)
        {
            if (_resultText == null) return;
            
            if (isVictory)
            {
                _resultText.gameObject.SetActive(true);
                _resultText.text = message;
                _resultText.color = Color.yellow;
            }
            else if (isDefeat)
            {
                _resultText.gameObject.SetActive(true);
                _resultText.text = message;
                _resultText.color = Color.red;
            }
            else
            {
                _resultText.gameObject.SetActive(false);
            }
        }
        
        public void ShowHitResult(HitResult result, int damage)
        {
            if (_hitResultText == null) return;
            
            switch (result)
            {
                case HitResult.Perfect:
                    _hitResultText.text = $"完美! -{damage}";
                    _hitResultText.color = Color.yellow;
                    break;
                case HitResult.Normal:
                    _hitResultText.text = $"不错 -{damage}";
                    _hitResultText.color = Color.white;
                    break;
                case HitResult.Miss:
                    _hitResultText.text = "Miss!";
                    _hitResultText.color = Color.red;
                    break;
            }
            
            // 延迟清除
            CancelInvoke(nameof(ClearHitResultText));
            Invoke(nameof(ClearHitResultText), 0.5f);
        }
        
        private void ClearHitResultText()
        {
            if (_hitResultText != null)
                _hitResultText.text = "";
        }
        
        #endregion
    }
}
