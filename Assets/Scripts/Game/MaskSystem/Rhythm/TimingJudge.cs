// ================================================
// MaskSystem - 节奏判定系统
// ================================================

using System;
using UnityEngine;

namespace Game.MaskSystem.Rhythm
{
    /// <summary>
    /// 判定结果数据
    /// </summary>
    public struct JudgeResultData
    {
        public BeatNote Note;
        public JudgeResult Result;
        public float TimingOffset;  // 与目标时间的偏差（毫秒），负数=提前，正数=延迟
        public bool WasMaskSwitch;  // 是否是切换面具触发的
        public int MaskSlot;        // 如果是切换面具，使用的槽位

        public string GetResultText()
        {
            switch (Result)
            {
                case JudgeResult.Perfect: return "PERFECT!";
                case JudgeResult.Normal: return "GOOD";
                case JudgeResult.Miss: return "MISS";
                default: return "";
            }
        }

        public Color GetResultColor()
        {
            switch (Result)
            {
                case JudgeResult.Perfect: return new Color(1f, 0.9f, 0.2f); // 金色
                case JudgeResult.Normal: return new Color(0.3f, 0.8f, 0.3f); // 绿色
                case JudgeResult.Miss: return new Color(0.8f, 0.2f, 0.2f); // 红色
                default: return Color.white;
            }
        }
    }

    /// <summary>
    /// 节奏判定系统 - 处理玩家输入与音符的时间判定
    /// </summary>
    public class TimingJudge
    {
        #region 判定窗口配置

        /// <summary>
        /// 完美判定窗口（毫秒）
        /// </summary>
        public float PerfectWindowMs { get; set; } = 50f;

        /// <summary>
        /// 普通判定窗口（毫秒）
        /// </summary>
        public float NormalWindowMs { get; set; } = 150f;

        /// <summary>
        /// 失误窗口（超过此时间未按则判定为Miss）
        /// </summary>
        public float MissWindowMs { get; set; } = 200f;

        #endregion

        #region 事件

        /// <summary>
        /// 判定完成事件
        /// </summary>
        public event Action<JudgeResultData> OnJudgeComplete;

        /// <summary>
        /// 音符进入判定区事件
        /// </summary>
        public event Action<BeatNote> OnNoteEnterJudgeZone;

        /// <summary>
        /// 音符离开判定区事件（未判定）
        /// </summary>
        public event Action<BeatNote> OnNoteExitJudgeZone;

        #endregion

        #region 状态

        /// <summary>
        /// 当前在判定区的音符
        /// </summary>
        public BeatNote CurrentNoteInZone { get; private set; }

        /// <summary>
        /// 是否有音符在判定区
        /// </summary>
        public bool HasNoteInZone => CurrentNoteInZone != null && CurrentNoteInZone.State == NoteState.InJudgeZone;

        #endregion

        #region 判定方法

        /// <summary>
        /// 更新音符状态（每帧调用）
        /// </summary>
        /// <param name="note">音符</param>
        /// <param name="currentTime">当前时间</param>
        public void UpdateNoteState(BeatNote note, float currentTime)
        {
            if (note.State == NoteState.Judged || note.State == NoteState.Passed)
                return;

            float timeDiff = (note.TargetTime - currentTime) * 1000f; // 转换为毫秒

            // 检查是否进入判定区
            if (note.State == NoteState.Approaching && timeDiff <= NormalWindowMs)
            {
                note.State = NoteState.InJudgeZone;
                CurrentNoteInZone = note;
                OnNoteEnterJudgeZone?.Invoke(note);
            }

            // 检查是否错过（超出判定窗口后方）
            if (note.State == NoteState.InJudgeZone && timeDiff < -MissWindowMs)
            {
                // 自动判定为Miss
                JudgeNote(note, currentTime, false, -1);
            }
        }

        /// <summary>
        /// 玩家按键时判定
        /// </summary>
        /// <param name="note">要判定的音符</param>
        /// <param name="currentTime">按键时间</param>
        /// <param name="wasMaskSwitch">是否是切换面具</param>
        /// <param name="maskSlot">面具槽位（如果是切换）</param>
        /// <returns>判定结果</returns>
        public JudgeResultData JudgeNote(BeatNote note, float currentTime, bool wasMaskSwitch = false, int maskSlot = -1)
        {
            float timeDiffMs = (currentTime - note.TargetTime) * 1000f; // 毫秒，正数=晚，负数=早
            float absTimeDiff = Mathf.Abs(timeDiffMs);

            JudgeResult result;

            if (absTimeDiff <= PerfectWindowMs)
            {
                result = JudgeResult.Perfect;
            }
            else if (absTimeDiff <= NormalWindowMs)
            {
                result = JudgeResult.Normal;
            }
            else
            {
                result = JudgeResult.Miss;
            }

            // 更新音符状态
            note.State = NoteState.Judged;
            note.Result = result;

            // 清除当前判定区音符
            if (CurrentNoteInZone == note)
            {
                CurrentNoteInZone = null;
            }

            // 构建结果数据
            var resultData = new JudgeResultData
            {
                Note = note,
                Result = result,
                TimingOffset = timeDiffMs,
                WasMaskSwitch = wasMaskSwitch,
                MaskSlot = maskSlot
            };

            OnJudgeComplete?.Invoke(resultData);

            Debug.Log($"[TimingJudge] {result} | 偏差: {timeDiffMs:F1}ms | 类型: {note.Type} | 切换面具: {wasMaskSwitch}");

            return resultData;
        }

        /// <summary>
        /// 尝试判定当前在判定区的音符
        /// </summary>
        /// <param name="currentTime">当前时间</param>
        /// <param name="wasMaskSwitch">是否是切换面具</param>
        /// <param name="maskSlot">面具槽位</param>
        /// <returns>判定结果，如果没有可判定音符返回null</returns>
        public JudgeResultData? TryJudgeCurrent(float currentTime, bool wasMaskSwitch = false, int maskSlot = -1)
        {
            if (!HasNoteInZone)
            {
                Debug.Log("[TimingJudge] 没有可判定的音符");
                return null;
            }

            return JudgeNote(CurrentNoteInZone, currentTime, wasMaskSwitch, maskSlot);
        }

        /// <summary>
        /// 设置当前判定区音符（由NoteTrack调用）
        /// </summary>
        public void SetCurrentNote(BeatNote note)
        {
            CurrentNoteInZone = note;
        }

        /// <summary>
        /// 清除当前判定区
        /// </summary>
        public void ClearCurrentNote()
        {
            if (CurrentNoteInZone != null && CurrentNoteInZone.State == NoteState.InJudgeZone)
            {
                CurrentNoteInZone.State = NoteState.Passed;
                CurrentNoteInZone.Result = JudgeResult.Miss;
                OnNoteExitJudgeZone?.Invoke(CurrentNoteInZone);
            }
            CurrentNoteInZone = null;
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 获取判定等级描述
        /// </summary>
        public static string GetJudgeDescription(JudgeResult result)
        {
            switch (result)
            {
                case JudgeResult.Perfect:
                    return "完美！敌人受到伤害";
                case JudgeResult.Normal:
                    return "普通，双方都受到伤害";
                case JudgeResult.Miss:
                    return "失误！你受到伤害";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 计算判定分数（用于评分系统）
        /// </summary>
        public static int GetJudgeScore(JudgeResult result)
        {
            switch (result)
            {
                case JudgeResult.Perfect: return 100;
                case JudgeResult.Normal: return 50;
                case JudgeResult.Miss: return 0;
                default: return 0;
            }
        }

        #endregion
    }
}

