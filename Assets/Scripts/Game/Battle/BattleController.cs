// ================================================
// Game - 战斗控制器
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using GameFramework.Core;

namespace Game.Battle
{
    /// <summary>
    /// 战斗状态
    /// </summary>
    public enum BattleState
    {
        None,
        Preparing,
        Fighting,
        Victory,
        Defeat,
        Paused
    }
    
    /// <summary>
    /// 战斗控制器 - 管理节奏战斗的核心逻辑
    /// </summary>
    public class BattleController
    {
        #region Properties
        
        /// <summary>
        /// 当前战斗状态
        /// </summary>
        public BattleState State { get; private set; }
        
        /// <summary>
        /// 玩家
        /// </summary>
        public PlayerFighter Player { get; private set; }
        
        /// <summary>
        /// 当前敌人
        /// </summary>
        public EnemyFighter CurrentEnemy { get; private set; }
        
        /// <summary>
        /// 当前关卡
        /// </summary>
        public int CurrentLevel { get; private set; }
        
        /// <summary>
        /// 战斗时间
        /// </summary>
        public float BattleTime { get; private set; }
        
        /// <summary>
        /// 节奏点列表
        /// </summary>
        public List<RhythmPoint> RhythmPoints { get; private set; }
        
        /// <summary>
        /// 当前节奏点索引
        /// </summary>
        public int CurrentRhythmIndex { get; private set; }
        
        /// <summary>
        /// 完美卡点连续次数
        /// </summary>
        public int PerfectCombo { get; private set; }
        
        #endregion
        
        #region Events
        
        public event Action<BattleState, BattleState> OnBattleStateChanged;
        public event Action<EnemyFighter> OnEnemySpawned;
        public event Action<EnemyFighter> OnEnemyDefeated;
        public event Action<RhythmPoint> OnRhythmPointReached;
        public event Action<HitResult, int> OnHitResult;
        public event Action<int> OnComboChanged;
        public event Action OnVictory;
        public event Action OnDefeat;
        
        #endregion
        
        #region Initialization
        
        public void Initialize()
        {
            State = BattleState.None;
            Player = new PlayerFighter();
            RhythmPoints = new List<RhythmPoint>();
            CurrentLevel = 1;
            
            Player.OnDeath += HandlePlayerDeath;
            
            Debug.Log("[BattleController] Initialized");
        }
        
        public void Shutdown()
        {
            if (Player != null)
            {
                Player.OnDeath -= HandlePlayerDeath;
            }
            
            State = BattleState.None;
            Player = null;
            CurrentEnemy = null;
            RhythmPoints?.Clear();
            
            Debug.Log("[BattleController] Shutdown");
        }
        
        #endregion
        
        #region Battle Control
        
        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartBattle(int level = 1)
        {
            CurrentLevel = level;
            BattleTime = 0f;
            PerfectCombo = 0;
            CurrentRhythmIndex = 0;
            
            Player.Reset();
            
            // 生成敌人
            SpawnEnemy();
            
            // 生成节奏点
            GenerateRhythmPoints();
            
            ChangeState(BattleState.Fighting);
            
            Debug.Log($"[BattleController] Battle started - Level {level}");
        }
        
        /// <summary>
        /// 暂停战斗
        /// </summary>
        public void PauseBattle()
        {
            if (State != BattleState.Fighting) return;
            
            ChangeState(BattleState.Paused);
        }
        
        /// <summary>
        /// 恢复战斗
        /// </summary>
        public void ResumeBattle()
        {
            if (State != BattleState.Paused) return;
            
            ChangeState(BattleState.Fighting);
        }
        
        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndBattle(bool victory)
        {
            ChangeState(victory ? BattleState.Victory : BattleState.Defeat);
            
            if (victory)
            {
                OnVictory?.Invoke();
            }
            else
            {
                OnDefeat?.Invoke();
            }
        }
        
        #endregion
        
        #region Update
        
        public void Update(float deltaTime)
        {
            if (State != BattleState.Fighting) return;
            
            BattleTime += deltaTime;
            
            // 检查节奏点
            CheckRhythmPoints();
            
            // 检查战斗结束条件
            CheckBattleEnd();
        }
        
        #endregion
        
        #region Input Handling
        
        /// <summary>
        /// 处理卡点输入 (Space)
        /// </summary>
        public void OnHitInput()
        {
            if (State != BattleState.Fighting) return;
            
            var nearestPoint = GetNearestRhythmPoint();
            if (nearestPoint == null || nearestPoint.IsProcessed)
            {
                // 没有可用的节奏点
                OnHitResult?.Invoke(HitResult.Miss, 0);
                ResetCombo();
                return;
            }
            
            var result = nearestPoint.GetHitResult(BattleTime);
            nearestPoint.IsProcessed = true;
            
            int damage = ProcessHitResult(result, nearestPoint);
            OnHitResult?.Invoke(result, damage);
        }
        
        /// <summary>
        /// 处理面具切换 (Q/W/E)
        /// </summary>
        public void OnMaskSwitch(int slot)
        {
            if (State != BattleState.Fighting) return;
            
            Player.SwitchToSlot(slot);
        }
        
        #endregion
        
        #region Private Methods
        
        private void ChangeState(BattleState newState)
        {
            if (State == newState) return;
            
            var oldState = State;
            State = newState;
            
            Debug.Log($"[BattleController] State: {oldState} -> {newState}");
            OnBattleStateChanged?.Invoke(oldState, newState);
        }
        
        private void SpawnEnemy()
        {
            // 根据关卡生成敌人
            switch (CurrentLevel)
            {
                case 1:
                    CurrentEnemy = new EnemyFighter("蛇", 3, MaskType.Snake, 1);
                    break;
                case 2:
                    CurrentEnemy = new EnemyFighter("猫", 3, MaskType.Cat, 2);
                    break;
                case 3:
                    CurrentEnemy = new EnemyFighter("熊", 4, MaskType.Bear, 2);
                    break;
                case 4:
                    CurrentEnemy = new EnemyFighter("牛", 5, MaskType.Bull, 2);
                    break;
                case 5:
                    CurrentEnemy = new EnemyFighter("鲸鱼", 6, MaskType.Whale, 1);
                    break;
                case 6:
                    CurrentEnemy = new EnemyFighter("鲨鱼", 4, MaskType.Shark, 3);
                    break;
                case 7:
                    CurrentEnemy = new EnemyFighter("龙", 10, MaskType.Dragon, 5);
                    break;
                default:
                    CurrentEnemy = new EnemyFighter("蛇", 3, MaskType.Snake, 1);
                    break;
            }
            
            CurrentEnemy.OnDeath += HandleEnemyDeath;
            OnEnemySpawned?.Invoke(CurrentEnemy);
            
            Debug.Log($"[BattleController] Enemy spawned: {CurrentEnemy.Name}");
        }
        
        private void GenerateRhythmPoints()
        {
            RhythmPoints.Clear();
            CurrentRhythmIndex = 0;
            
            // 根据关卡配置生成节奏点
            float baseInterval = 1.0f; // 基础间隔
            float startTime = 2.0f;    // 开始时间
            int pointCount = 10 + CurrentLevel * 2; // 节奏点数量
            
            for (int i = 0; i < pointCount; i++)
            {
                float time = startTime + i * baseInterval;
                var type = UnityEngine.Random.value > 0.3f 
                    ? RhythmPointType.Attack 
                    : RhythmPointType.Defend;
                
                RhythmPoints.Add(new RhythmPoint(type, time));
            }
            
            Debug.Log($"[BattleController] Generated {pointCount} rhythm points");
        }
        
        private void CheckRhythmPoints()
        {
            // 检查是否有节奏点到达
            while (CurrentRhythmIndex < RhythmPoints.Count)
            {
                var point = RhythmPoints[CurrentRhythmIndex];
                
                // 如果节奏点时间已过且未处理，算作miss
                if (BattleTime > point.Time + RhythmPoint.NORMAL_WINDOW && !point.IsProcessed)
                {
                    point.IsProcessed = true;
                    
                    // 敌人攻击
                    if (point.Type == RhythmPointType.Attack)
                    {
                        int damage = CurrentEnemy.PerformAttack();
                        Player.TakeDamage(damage);
                    }
                    
                    ResetCombo();
                    CurrentRhythmIndex++;
                }
                else if (BattleTime >= point.Time - RhythmPoint.NORMAL_WINDOW && !point.IsProcessed)
                {
                    // 节奏点进入可点击范围
                    OnRhythmPointReached?.Invoke(point);
                    break;
                }
                else
                {
                    break;
                }
            }
        }
        
        private RhythmPoint GetNearestRhythmPoint()
        {
            float minDist = float.MaxValue;
            RhythmPoint nearest = null;
            
            foreach (var point in RhythmPoints)
            {
                if (point.IsProcessed) continue;
                
                float dist = Mathf.Abs(point.Time - BattleTime);
                if (dist < minDist && dist <= RhythmPoint.NORMAL_WINDOW)
                {
                    minDist = dist;
                    nearest = point;
                }
            }
            
            return nearest;
        }
        
        private int ProcessHitResult(HitResult result, RhythmPoint point)
        {
            int damage = 0;
            var maskData = MaskConfig.GetMaskData(Player.CurrentMask);
            
            switch (result)
            {
                case HitResult.Perfect:
                    PerfectCombo++;
                    Player.RecordPerfectHit();
                    
                    // 完美卡点造成伤害
                    damage = maskData?.AttackPower ?? 1;
                    
                    // 克制关系检查
                    if (MaskConfig.IsCounter(Player.CurrentMask, CurrentEnemy.CurrentMask))
                    {
                        damage *= 2;
                    }
                    
                    CurrentEnemy.TakeDamage(damage);
                    
                    // 回血面具完美卡点回血
                    if (maskData?.EffectType == MaskEffectType.Heal)
                    {
                        Player.Heal(1);
                    }
                    
                    OnComboChanged?.Invoke(PerfectCombo);
                    break;
                    
                case HitResult.Normal:
                    // 一般卡点
                    damage = maskData?.AttackPower ?? 1;
                    CurrentEnemy.TakeDamage(damage);
                    
                    // 一般卡点也可能受到伤害（取决于面具类型）
                    if (maskData?.EffectType != MaskEffectType.Dodge)
                    {
                        // 非闪避面具受到部分伤害
                        if (point.Type == RhythmPointType.Attack)
                        {
                            Player.TakeDamage(1);
                        }
                    }
                    
                    ResetCombo();
                    break;
                    
                case HitResult.Miss:
                    // 未命中，受到敌人攻击
                    if (point.Type == RhythmPointType.Attack)
                    {
                        int enemyDamage = CurrentEnemy.PerformAttack();
                        Player.TakeDamage(enemyDamage);
                    }
                    ResetCombo();
                    break;
            }
            
            return damage;
        }
        
        private void ResetCombo()
        {
            if (PerfectCombo > 0)
            {
                PerfectCombo = 0;
                OnComboChanged?.Invoke(0);
            }
        }
        
        private void CheckBattleEnd()
        {
            if (!Player.IsAlive)
            {
                EndBattle(false);
            }
        }
        
        private void HandlePlayerDeath()
        {
            Debug.Log("[BattleController] Player died!");
            EndBattle(false);
        }
        
        private void HandleEnemyDeath()
        {
            Debug.Log($"[BattleController] Enemy {CurrentEnemy.Name} defeated!");
            
            // 获得敌人的面具
            Player.AddMask(CurrentEnemy.CurrentMask);
            
            OnEnemyDefeated?.Invoke(CurrentEnemy);
            
            // 进入下一关或胜利
            CurrentLevel++;
            if (CurrentLevel > 7) // 龙是最终Boss
            {
                EndBattle(true);
            }
            else
            {
                // 生成下一个敌人
                SpawnEnemy();
                GenerateRhythmPoints();
                BattleTime = 0f;
            }
        }
        
        #endregion
    }
}
