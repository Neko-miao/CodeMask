// ================================================
// MaskSystem - 节奏系统主管理器
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem.Rhythm
{
    /// <summary>
    /// 节奏系统主管理器 - 控制节拍、音符生成和整体流程
    /// </summary>
    public class RhythmManager
    {
        #region 配置

        /// <summary>
        /// 每分钟节拍数
        /// </summary>
        public float BPM { get; private set; } = 120f;

        /// <summary>
        /// 每拍时间（秒）
        /// </summary>
        public float BeatInterval => 60f / BPM;

        /// <summary>
        /// 音符序列配置
        /// </summary>
        private RhythmConfig _config;

        #endregion

        #region 组件

        /// <summary>
        /// 判定系统
        /// </summary>
        public TimingJudge Judge { get; private set; }

        /// <summary>
        /// 音符轨道
        /// </summary>
        public NoteTrack Track { get; private set; }

        #endregion

        #region 状态

        /// <summary>
        /// 是否正在运行
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// 当前节拍计数
        /// </summary>
        public int CurrentBeat { get; private set; }

        /// <summary>
        /// 连续完美次数
        /// </summary>
        public int PerfectCombo { get; private set; }

        /// <summary>
        /// 总连击数
        /// </summary>
        public int TotalCombo { get; private set; }

        /// <summary>
        /// 最大连击数
        /// </summary>
        public int MaxCombo { get; private set; }

        /// <summary>
        /// 当前分数
        /// </summary>
        public int Score { get; private set; }

        /// <summary>
        /// 上次节拍时间
        /// </summary>
        private float _lastBeatTime;

        /// <summary>
        /// 当前时间
        /// </summary>
        public float CurrentTime => Track?.CurrentTime ?? 0f;

        #endregion

        #region 事件

        /// <summary>
        /// 节拍事件（每个节拍触发）
        /// </summary>
        public event Action<int> OnBeat;

        /// <summary>
        /// 判定完成事件
        /// </summary>
        public event Action<JudgeResultData> OnJudge;

        /// <summary>
        /// 连击更新事件
        /// </summary>
        public event Action<int, int> OnComboUpdate; // (currentCombo, perfectCombo)

        /// <summary>
        /// 三连完美触发攻击事件
        /// </summary>
        public event Action OnPerfectTripleAttack;

        /// <summary>
        /// 音符生成事件
        /// </summary>
        public event Action<BeatNote> OnNoteSpawned;

        /// <summary>
        /// 音符销毁事件
        /// </summary>
        public event Action<BeatNote> OnNoteDestroyed;

        /// <summary>
        /// 所有音符完成事件
        /// </summary>
        public event Action OnAllNotesComplete;

        #endregion

        #region 初始化

        public RhythmManager()
        {
            Judge = new TimingJudge();
            Track = new NoteTrack(Judge);

            // 绑定事件
            Judge.OnJudgeComplete += HandleJudgeComplete;
            Track.OnNoteSpawned += (note) => OnNoteSpawned?.Invoke(note);
            Track.OnNoteDestroyed += (note) => OnNoteDestroyed?.Invoke(note);
        }

        /// <summary>
        /// 设置节奏配置
        /// </summary>
        public void SetConfig(RhythmConfig config)
        {
            _config = config;
            if (config != null)
            {
                BPM = config.BPM;
                Judge.PerfectWindowMs = config.PerfectWindowMs;
                Judge.NormalWindowMs = config.NormalWindowMs;
                Track.TravelTime = config.NoteTravelTime;
            }
        }

        #endregion

        #region 控制方法

        /// <summary>
        /// 开始节奏战斗
        /// </summary>
        /// <param name="noteSequence">音符序列（目标时间排序）</param>
        public void Start(List<BeatNote> noteSequence = null)
        {
            Reset();

            if (noteSequence != null && noteSequence.Count > 0)
            {
                Track.AddNotes(noteSequence);
            }
            else if (_config != null)
            {
                // 从配置生成音符
                var notes = GenerateNotesFromConfig();
                Track.AddNotes(notes);
            }

            Track.Start();
            IsRunning = true;
            _lastBeatTime = 0f;

            Debug.Log($"[RhythmManager] 开始 | BPM: {BPM} | 节拍间隔: {BeatInterval:F3}s");
        }

        /// <summary>
        /// 暂停
        /// </summary>
        public void Pause()
        {
            IsRunning = false;
            Track.Stop();
        }

        /// <summary>
        /// 继续
        /// </summary>
        public void Resume()
        {
            IsRunning = true;
            Track.Start();
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            IsRunning = false;
            Track.Stop();
        }

        /// <summary>
        /// 重置
        /// </summary>
        public void Reset()
        {
            Track.Reset();
            CurrentBeat = 0;
            PerfectCombo = 0;
            TotalCombo = 0;
            MaxCombo = 0;
            Score = 0;
            _lastBeatTime = 0f;
        }

        #endregion

        #region 更新

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Update(float deltaTime)
        {
            if (!IsRunning) return;

            Track.Update(deltaTime);

            // 检查节拍
            CheckBeat();

            // 检查是否所有音符都已完成
            CheckAllNotesComplete();
        }

        private void CheckBeat()
        {
            float currentTime = Track.CurrentTime;
            float timeSinceLastBeat = currentTime - _lastBeatTime;

            if (timeSinceLastBeat >= BeatInterval)
            {
                CurrentBeat++;
                _lastBeatTime = currentTime;
                OnBeat?.Invoke(CurrentBeat);
            }
        }

        private void CheckAllNotesComplete()
        {
            // 如果轨道上没有活跃音符且待生成队列为空
            if (Track.ActiveNotes.Count == 0 && Track.CompletedNotes.Count > 0)
            {
                // 给一点缓冲时间
                if (Track.CurrentTime > Track.CompletedNotes[Track.CompletedNotes.Count - 1].TargetTime + 1f)
                {
                    OnAllNotesComplete?.Invoke();
                    Stop();
                }
            }
        }

        #endregion

        #region 输入处理

        /// <summary>
        /// 玩家按键（Space保持）
        /// </summary>
        public JudgeResultData? PlayerInput()
        {
            return PlayerInputWithMask(false, -1);
        }

        /// <summary>
        /// 玩家按键（带面具切换）
        /// </summary>
        /// <param name="isMaskSwitch">是否是切换面具</param>
        /// <param name="maskSlot">面具槽位 (0=Q, 1=W, 2=E)</param>
        public JudgeResultData? PlayerInputWithMask(bool isMaskSwitch, int maskSlot)
        {
            if (!IsRunning) return null;

            var result = Judge.TryJudgeCurrent(Track.CurrentTime, isMaskSwitch, maskSlot);
            return result;
        }

        #endregion

        #region 判定处理

        private void HandleJudgeComplete(JudgeResultData result)
        {
            // 更新连击
            if (result.Result != JudgeResult.Miss)
            {
                TotalCombo++;
                if (TotalCombo > MaxCombo)
                    MaxCombo = TotalCombo;

                if (result.Result == JudgeResult.Perfect)
                {
                    PerfectCombo++;

                    // 检查三连完美
                    if (PerfectCombo >= 3)
                    {
                        OnPerfectTripleAttack?.Invoke();
                        PerfectCombo = 0; // 重置完美计数
                        Debug.Log("[RhythmManager] 三连完美！触发攻击！");
                    }
                }
            }
            else
            {
                // Miss 重置连击
                TotalCombo = 0;
                PerfectCombo = 0;
            }

            // 更新分数
            Score += TimingJudge.GetJudgeScore(result.Result) * (1 + TotalCombo / 10);

            OnComboUpdate?.Invoke(TotalCombo, PerfectCombo);
            OnJudge?.Invoke(result);
        }

        #endregion

        #region 音符生成

        /// <summary>
        /// 从配置生成音符序列
        /// </summary>
        private List<BeatNote> GenerateNotesFromConfig()
        {
            var notes = new List<BeatNote>();

            if (_config == null) return notes;

            float startTime = Track.TravelTime + 0.5f; // 给玩家准备时间

            foreach (var pattern in _config.Patterns)
            {
                for (int i = 0; i < pattern.RepeatCount; i++)
                {
                    float patternStartTime = startTime + i * pattern.Notes.Count * BeatInterval;

                    for (int j = 0; j < pattern.Notes.Count; j++)
                    {
                        var noteConfig = pattern.Notes[j];
                        float targetTime = patternStartTime + j * BeatInterval;

                        var note = new BeatNote(
                            noteConfig.Type,
                            targetTime,
                            targetTime - Track.TravelTime,
                            noteConfig.Damage
                        );

                        notes.Add(note);
                    }
                }

                startTime += pattern.RepeatCount * pattern.Notes.Count * BeatInterval;
            }

            return notes;
        }

        /// <summary>
        /// 根据敌人类型生成随机音符序列
        /// </summary>
        /// <param name="enemyType">敌人类型</param>
        /// <param name="totalBeats">总节拍数</param>
        /// <param name="difficulty">难度 (0-1)</param>
        public List<BeatNote> GenerateRandomNotes(MaskType enemyType, int totalBeats, float difficulty = 0.5f)
        {
            var notes = new List<BeatNote>();
            float startTime = Track.TravelTime + 0.5f;

            // 根据敌人类型设置音符类型概率
            var probabilities = GetNoteTypeProbabilities(enemyType, difficulty);

            for (int i = 0; i < totalBeats; i++)
            {
                float targetTime = startTime + i * BeatInterval;

                // 随机决定音符类型
                NoteType type = GetRandomNoteType(probabilities);

                // 空闲音符有概率跳过
                if (type == NoteType.Idle && UnityEngine.Random.value < 0.3f)
                    continue;

                int damage = type == NoteType.Rage ? 2 : 1;

                var note = new BeatNote(type, targetTime, targetTime - Track.TravelTime, damage);
                notes.Add(note);
            }

            return notes;
        }

        private Dictionary<NoteType, float> GetNoteTypeProbabilities(MaskType enemyType, float difficulty)
        {
            // 基础概率
            var probs = new Dictionary<NoteType, float>
            {
                { NoteType.Attack, 0.5f },
                { NoteType.Defense, 0.2f },
                { NoteType.Idle, 0.2f },
                { NoteType.Rage, 0.1f * difficulty }
            };

            // 根据敌人类型调整
            switch (enemyType)
            {
                case MaskType.Snake:
                    probs[NoteType.Attack] = 0.6f;
                    probs[NoteType.Defense] = 0.1f;
                    break;
                case MaskType.Bear:
                    probs[NoteType.Attack] = 0.4f;
                    probs[NoteType.Rage] = 0.2f * difficulty;
                    break;
                case MaskType.Bull:
                    probs[NoteType.Rage] = 0.25f * difficulty;
                    break;
                case MaskType.Shark:
                    probs[NoteType.Attack] = 0.7f;
                    probs[NoteType.Rage] = 0.15f * difficulty;
                    break;
                case MaskType.Dragon:
                    probs[NoteType.Attack] = 0.5f;
                    probs[NoteType.Rage] = 0.3f * difficulty;
                    probs[NoteType.Defense] = 0.1f;
                    break;
            }

            return probs;
        }

        private NoteType GetRandomNoteType(Dictionary<NoteType, float> probabilities)
        {
            float total = 0f;
            foreach (var p in probabilities.Values) total += p;

            float roll = UnityEngine.Random.value * total;
            float cumulative = 0f;

            foreach (var kvp in probabilities)
            {
                cumulative += kvp.Value;
                if (roll <= cumulative)
                    return kvp.Key;
            }

            return NoteType.Attack;
        }

        #endregion

        #region 查询

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public (int perfect, int normal, int miss, int score, int maxCombo) GetStatistics()
        {
            var (perfect, normal, miss) = Track.GetStatistics();
            return (perfect, normal, miss, Score, MaxCombo);
        }

        /// <summary>
        /// 获取当前在判定区的音符
        /// </summary>
        public BeatNote GetCurrentNote()
        {
            return Judge.CurrentNoteInZone;
        }

        /// <summary>
        /// 是否有音符在判定区
        /// </summary>
        public bool HasNoteInJudgeZone()
        {
            return Judge.HasNoteInZone;
        }

        #endregion
    }
}

