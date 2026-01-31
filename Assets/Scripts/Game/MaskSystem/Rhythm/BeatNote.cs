// ================================================
// MaskSystem - 节奏音符数据
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem.Rhythm
{
    /// <summary>
    /// 音符类型
    /// </summary>
    public enum NoteType
    {
        Attack,     // 攻击 - 需要玩家按键反击
        Defense,    // 防御 - 需要玩家格挡
        Idle,       // 空闲 - 不产生效果的间隔
        Rage        // 狂暴 - 高伤害攻击，需要完美卡点
    }

    /// <summary>
    /// 判定结果
    /// </summary>
    public enum JudgeResult
    {
        None,       // 未判定
        Perfect,    // 完美 ±50ms
        Normal,     // 普通 ±150ms
        Miss        // 失误 >150ms 或未按
    }

    /// <summary>
    /// 音符状态
    /// </summary>
    public enum NoteState
    {
        Approaching,    // 接近中
        InJudgeZone,    // 在判定区域内
        Judged,         // 已判定
        Passed          // 已通过（未按）
    }

    /// <summary>
    /// 节拍音符数据
    /// </summary>
    [Serializable]
    public class BeatNote
    {
        /// <summary>
        /// 唯一ID
        /// </summary>
        public int Id { get; private set; }

        /// <summary>
        /// 音符类型
        /// </summary>
        public NoteType Type { get; private set; }

        /// <summary>
        /// 目标时间（秒）- 玩家应该在此时按键
        /// </summary>
        public float TargetTime { get; private set; }

        /// <summary>
        /// 当前状态
        /// </summary>
        public NoteState State { get; set; }

        /// <summary>
        /// 判定结果
        /// </summary>
        public JudgeResult Result { get; set; }

        /// <summary>
        /// 生成时间
        /// </summary>
        public float SpawnTime { get; private set; }

        /// <summary>
        /// 音符在轨道上的位置 (0-1, 0=判定线, 1=生成点)
        /// </summary>
        public float TrackPosition { get; set; }

        /// <summary>
        /// 关联的伤害值
        /// </summary>
        public int Damage { get; set; }

        /// <summary>
        /// 是否是连击的一部分
        /// </summary>
        public bool IsComboNote { get; set; }

        private static int _nextId = 0;

        public BeatNote(NoteType type, float targetTime, float spawnTime, int damage = 1)
        {
            Id = _nextId++;
            Type = type;
            TargetTime = targetTime;
            SpawnTime = spawnTime;
            Damage = damage;
            State = NoteState.Approaching;
            Result = JudgeResult.None;
            TrackPosition = 1f;
            IsComboNote = false;
        }

        /// <summary>
        /// 获取音符显示颜色
        /// </summary>
        public Color GetColor()
        {
            switch (Type)
            {
                case NoteType.Attack:
                    return new Color(1f, 0.3f, 0.3f); // 红色
                case NoteType.Defense:
                    return new Color(0.3f, 0.5f, 1f); // 蓝色
                case NoteType.Idle:
                    return new Color(0.5f, 0.5f, 0.5f); // 灰色
                case NoteType.Rage:
                    return new Color(1f, 0.5f, 0f); // 橙色
                default:
                    return Color.white;
            }
        }

        /// <summary>
        /// 获取音符类型名称
        /// </summary>
        public string GetTypeName()
        {
            switch (Type)
            {
                case NoteType.Attack: return "攻击";
                case NoteType.Defense: return "防御";
                case NoteType.Idle: return "空闲";
                case NoteType.Rage: return "狂暴";
                default: return Type.ToString();
            }
        }

        /// <summary>
        /// 重置ID计数器（用于新关卡开始时）
        /// </summary>
        public static void ResetIdCounter()
        {
            _nextId = 0;
        }
    }
}

