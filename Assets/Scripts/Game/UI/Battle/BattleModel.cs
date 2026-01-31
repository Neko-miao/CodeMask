// ================================================
// Game - 战斗界面数据模型
// ================================================

using System;
using GameFramework.UI;
using Game.Battle;

namespace Game.UI
{
    /// <summary>
    /// 玩家UI数据
    /// </summary>
    public class PlayerUIData
    {
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public MaskType CurrentMask { get; set; }
        public MaskType[] OwnedMasks { get; set; }
    }
    
    /// <summary>
    /// 敌人UI数据
    /// </summary>
    public class EnemyUIData
    {
        public string Name { get; set; }
        public int CurrentHealth { get; set; }
        public int MaxHealth { get; set; }
        public MaskType Mask { get; set; }
    }
    
    /// <summary>
    /// 战斗UI数据
    /// </summary>
    public class BattleUIData
    {
        public BattleState State { get; set; }
        public int CurrentLevel { get; set; }
        public int Combo { get; set; }
        public HitResult LastHitResult { get; set; }
        public int LastHitDamage { get; set; }
        public bool IsVictory { get; set; }
        public bool IsDefeat { get; set; }
        public string ResultMessage { get; set; }
        
        public PlayerUIData Player { get; set; }
        public EnemyUIData Enemy { get; set; }
    }
    
    /// <summary>
    /// 战斗界面数据模型
    /// </summary>
    public class BattleModel : UIModelBase<BattleUIData>
    {
        public BattleState State => Data?.State ?? BattleState.None;
        public int CurrentLevel => Data?.CurrentLevel ?? 0;
        public int Combo => Data?.Combo ?? 0;
        public PlayerUIData Player => Data?.Player;
        public EnemyUIData Enemy => Data?.Enemy;
        public bool IsVictory => Data?.IsVictory ?? false;
        public bool IsDefeat => Data?.IsDefeat ?? false;
        public string ResultMessage => Data?.ResultMessage ?? "";
        public HitResult LastHitResult => Data?.LastHitResult ?? HitResult.Miss;
        public int LastHitDamage => Data?.LastHitDamage ?? 0;
        
        public override void Initialize()
        {
            SetData(new BattleUIData
            {
                State = BattleState.None,
                CurrentLevel = 0,
                Combo = 0,
                IsVictory = false,
                IsDefeat = false,
                Player = new PlayerUIData(),
                Enemy = null
            });
        }
        
        public void UpdateBattleState(BattleState state, int level)
        {
            if (Data == null) return;
            Data.State = state;
            Data.CurrentLevel = level;
            NotifyDataChanged();
        }
        
        public void UpdatePlayerHealth(int current, int max)
        {
            if (Data?.Player == null) return;
            Data.Player.CurrentHealth = current;
            Data.Player.MaxHealth = max;
            NotifyDataChanged();
        }
        
        public void UpdatePlayerMask(MaskType mask)
        {
            if (Data?.Player == null) return;
            Data.Player.CurrentMask = mask;
            NotifyDataChanged();
        }
        
        public void UpdateEnemy(EnemyFighter enemy)
        {
            if (Data == null) return;
            
            if (enemy == null)
            {
                Data.Enemy = null;
            }
            else
            {
                Data.Enemy = new EnemyUIData
                {
                    Name = enemy.Name,
                    CurrentHealth = enemy.CurrentHealth,
                    MaxHealth = enemy.MaxHealth,
                    Mask = enemy.CurrentMask
                };
            }
            NotifyDataChanged();
        }
        
        public void UpdateEnemyHealth(int current, int max)
        {
            if (Data?.Enemy == null) return;
            Data.Enemy.CurrentHealth = current;
            Data.Enemy.MaxHealth = max;
            NotifyDataChanged();
        }
        
        public void UpdateCombo(int combo)
        {
            if (Data == null) return;
            Data.Combo = combo;
            NotifyDataChanged();
        }
        
        public void UpdateHitResult(HitResult result, int damage)
        {
            if (Data == null) return;
            Data.LastHitResult = result;
            Data.LastHitDamage = damage;
            NotifyDataChanged();
        }
        
        public void SetVictory()
        {
            if (Data == null) return;
            Data.IsVictory = true;
            Data.IsDefeat = false;
            Data.ResultMessage = "胜利!";
            NotifyDataChanged();
        }
        
        public void SetDefeat()
        {
            if (Data == null) return;
            Data.IsVictory = false;
            Data.IsDefeat = true;
            Data.ResultMessage = "失败...";
            NotifyDataChanged();
        }
        
        public void SetEnemyDefeatedMessage(string enemyName, MaskType mask)
        {
            if (Data == null) return;
            Data.ResultMessage = $"击败 {enemyName}！获得 {mask} 面具";
            NotifyDataChanged();
        }
        
        public override void Clear()
        {
            base.Clear();
        }
    }
}
