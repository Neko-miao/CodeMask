// ================================================
// MaskSystem - 音符轨道
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem.Rhythm
{
    /// <summary>
    /// 音符轨道 - 管理音符的生成、移动和生命周期
    /// </summary>
    public class NoteTrack
    {
        #region 配置

        /// <summary>
        /// 音符从生成到判定线的时间（秒）
        /// </summary>
        public float TravelTime { get; set; } = 2.0f;

        /// <summary>
        /// 轨道长度（世界坐标）
        /// </summary>
        public float TrackLength { get; set; } = 10f;

        /// <summary>
        /// 判定线位置（世界坐标X）
        /// </summary>
        public float JudgeLineX { get; set; } = -3f;

        /// <summary>
        /// 生成点位置（世界坐标X）
        /// </summary>
        public float SpawnX { get; set; } = 7f;

        /// <summary>
        /// 轨道Y位置
        /// </summary>
        public float TrackY { get; set; } = -3f;

        #endregion

        #region 状态

        /// <summary>
        /// 当前活跃的音符列表
        /// </summary>
        public List<BeatNote> ActiveNotes { get; private set; } = new List<BeatNote>();

        /// <summary>
        /// 即将生成的音符队列
        /// </summary>
        private Queue<BeatNote> _pendingNotes = new Queue<BeatNote>();

        /// <summary>
        /// 已完成的音符（用于统计）
        /// </summary>
        public List<BeatNote> CompletedNotes { get; private set; } = new List<BeatNote>();

        /// <summary>
        /// 当前时间
        /// </summary>
        public float CurrentTime { get; private set; }

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 关联的判定系统
        /// </summary>
        private TimingJudge _judge;

        #endregion

        #region 事件

        /// <summary>
        /// 音符生成事件
        /// </summary>
        public event Action<BeatNote> OnNoteSpawned;

        /// <summary>
        /// 音符销毁事件
        /// </summary>
        public event Action<BeatNote> OnNoteDestroyed;

        /// <summary>
        /// 音符位置更新事件
        /// </summary>
        public event Action<BeatNote, Vector2> OnNotePositionUpdated;

        #endregion

        #region 构造函数

        public NoteTrack(TimingJudge judge)
        {
            _judge = judge;
            TrackLength = SpawnX - JudgeLineX;
        }

        #endregion

        #region 控制方法

        /// <summary>
        /// 开始轨道
        /// </summary>
        public void Start()
        {
            IsRunning = true;
            CurrentTime = 0f;
            Debug.Log("[NoteTrack] 轨道开始");
        }

        /// <summary>
        /// 停止轨道
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            Debug.Log("[NoteTrack] 轨道停止");
        }

        /// <summary>
        /// 重置轨道
        /// </summary>
        public void Reset()
        {
            ActiveNotes.Clear();
            CompletedNotes.Clear();
            _pendingNotes.Clear();
            CurrentTime = 0f;
            BeatNote.ResetIdCounter();
            Debug.Log("[NoteTrack] 轨道重置");
        }

        /// <summary>
        /// 添加待生成的音符
        /// </summary>
        public void AddNote(BeatNote note)
        {
            _pendingNotes.Enqueue(note);
        }

        /// <summary>
        /// 批量添加音符
        /// </summary>
        public void AddNotes(IEnumerable<BeatNote> notes)
        {
            foreach (var note in notes)
            {
                _pendingNotes.Enqueue(note);
            }
        }

        #endregion

        #region 更新

        /// <summary>
        /// 更新轨道（每帧调用）
        /// </summary>
        /// <param name="deltaTime">帧时间</param>
        public void Update(float deltaTime)
        {
            if (!IsRunning) return;

            CurrentTime += deltaTime;

            // 检查并生成新音符
            SpawnPendingNotes();

            // 更新活跃音符
            UpdateActiveNotes();

            // 清理已完成的音符
            CleanupCompletedNotes();
        }

        private void SpawnPendingNotes()
        {
            // 检查待生成队列中是否有应该生成的音符
            while (_pendingNotes.Count > 0)
            {
                var note = _pendingNotes.Peek();
                
                // 计算音符应该生成的时间（目标时间减去移动时间）
                float spawnTime = note.TargetTime - TravelTime;
                
                if (CurrentTime >= spawnTime)
                {
                    _pendingNotes.Dequeue();
                    SpawnNote(note);
                }
                else
                {
                    break; // 队列是按时间排序的，后面的肯定也还没到时间
                }
            }
        }

        private void SpawnNote(BeatNote note)
        {
            note.TrackPosition = 1f;
            note.State = NoteState.Approaching;
            ActiveNotes.Add(note);
            OnNoteSpawned?.Invoke(note);
            Debug.Log($"[NoteTrack] 生成音符: {note.Type} @ {note.TargetTime:F2}s");
        }

        private void UpdateActiveNotes()
        {
            foreach (var note in ActiveNotes)
            {
                if (note.State == NoteState.Judged || note.State == NoteState.Passed)
                    continue;

                // 计算音符位置 (0=判定线, 1=生成点)
                float timeToTarget = note.TargetTime - CurrentTime;
                note.TrackPosition = Mathf.Clamp01(timeToTarget / TravelTime);

                // 计算世界坐标
                float worldX = Mathf.Lerp(JudgeLineX, SpawnX, note.TrackPosition);
                Vector2 worldPos = new Vector2(worldX, TrackY);
                OnNotePositionUpdated?.Invoke(note, worldPos);

                // 更新判定状态
                _judge.UpdateNoteState(note, CurrentTime);
            }
        }

        private void CleanupCompletedNotes()
        {
            for (int i = ActiveNotes.Count - 1; i >= 0; i--)
            {
                var note = ActiveNotes[i];
                
                // 音符已经判定完成或已经完全通过判定线
                if (note.State == NoteState.Judged || note.State == NoteState.Passed)
                {
                    // 给一点时间显示判定结果
                    if (note.TrackPosition < -0.2f)
                    {
                        CompletedNotes.Add(note);
                        ActiveNotes.RemoveAt(i);
                        OnNoteDestroyed?.Invoke(note);
                    }
                }
            }
        }

        #endregion

        #region 查询方法

        /// <summary>
        /// 获取最接近判定线的未判定音符
        /// </summary>
        public BeatNote GetClosestNote()
        {
            BeatNote closest = null;
            float closestDist = float.MaxValue;

            foreach (var note in ActiveNotes)
            {
                if (note.State == NoteState.Judged || note.State == NoteState.Passed)
                    continue;

                float dist = Mathf.Abs(note.TrackPosition);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = note;
                }
            }

            return closest;
        }

        /// <summary>
        /// 获取在判定区内的音符
        /// </summary>
        public BeatNote GetNoteInJudgeZone()
        {
            foreach (var note in ActiveNotes)
            {
                if (note.State == NoteState.InJudgeZone)
                    return note;
            }
            return null;
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public (int perfect, int normal, int miss) GetStatistics()
        {
            int perfect = 0, normal = 0, miss = 0;

            foreach (var note in CompletedNotes)
            {
                switch (note.Result)
                {
                    case JudgeResult.Perfect: perfect++; break;
                    case JudgeResult.Normal: normal++; break;
                    case JudgeResult.Miss: miss++; break;
                }
            }

            return (perfect, normal, miss);
        }

        #endregion
    }
}

