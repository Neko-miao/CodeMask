// ================================================
// MaskSystem - 节奏视觉管理器
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.MaskSystem.Rhythm.Visual
{
    /// <summary>
    /// 音符视觉对象
    /// </summary>
    public class NoteVisual
    {
        public BeatNote Note;
        public GameObject GameObject;
        public MeshRenderer Renderer;
        public Material Material;
        public float DestroyTime;
    }

    /// <summary>
    /// 打击特效对象
    /// </summary>
    public class HitEffect
    {
        public GameObject GameObject;
        public MeshRenderer Renderer;
        public Material Material;
        public float StartTime;
        public float Duration;
    }

    /// <summary>
    /// 节奏视觉管理器 - 管理音符、判定线、特效的显示
    /// </summary>
    public class RhythmVisualManager : MonoBehaviour
    {
        #region 配置

        [Header("轨道设置")]
        [Tooltip("轨道Y位置")]
        public float TrackY = -3f;

        [Tooltip("判定线X位置")]
        public float JudgeLineX = -3f;

        [Tooltip("生成点X位置")]
        public float SpawnX = 7f;

        [Tooltip("音符大小")]
        public float NoteSize = 0.8f;

        [Header("判定线设置")]
        [Tooltip("判定线宽度")]
        public float JudgeLineWidth = 0.1f;

        [Tooltip("判定线高度")]
        public float JudgeLineHeight = 2f;

        [Tooltip("判定线颜色")]
        public Color JudgeLineColor = new Color(1f, 1f, 1f, 0.8f);

        [Header("特效设置")]
        [Tooltip("特效持续时间")]
        public float EffectDuration = 0.5f;

        [Tooltip("特效大小")]
        public float EffectSize = 1.5f;

        #endregion

        #region 私有字段

        private RhythmManager _rhythmManager;
        private Dictionary<int, NoteVisual> _noteVisuals = new Dictionary<int, NoteVisual>();
        private List<HitEffect> _activeEffects = new List<HitEffect>();
        private List<HitEffect> _effectPool = new List<HitEffect>();

        private GameObject _judgeLineObject;
        private Material _noteShaderMaterial;
        private Material _hitEffectMaterial;

        private Mesh _quadMesh;

        // 判定结果显示
        private string _lastJudgeText = "";
        private Color _lastJudgeColor = Color.white;
        private float _judgeTextTimer = 0f;
        private const float JudgeTextDuration = 0.5f;

        // 连击显示
        private int _currentCombo = 0;
        private int _perfectCombo = 0;

        #endregion

        #region 事件

        public event Action<JudgeResultData> OnJudgeVisualized;

        #endregion

        #region 初始化

        private void Awake()
        {
            CreateQuadMesh();
            LoadShaders();
        }

        public void Initialize(RhythmManager rhythmManager)
        {
            _rhythmManager = rhythmManager;

            // 绑定事件
            _rhythmManager.OnNoteSpawned += OnNoteSpawned;
            _rhythmManager.OnNoteDestroyed += OnNoteDestroyed;
            _rhythmManager.OnJudge += OnJudge;
            _rhythmManager.OnComboUpdate += OnComboUpdate;
            _rhythmManager.Track.OnNotePositionUpdated += OnNotePositionUpdated;

            // 同步轨道设置
            TrackY = _rhythmManager.Track.TrackY;
            JudgeLineX = _rhythmManager.Track.JudgeLineX;
            SpawnX = _rhythmManager.Track.SpawnX;

            CreateJudgeLine();

            Debug.Log("[RhythmVisualManager] 初始化完成");
        }

        private void OnDestroy()
        {
            if (_rhythmManager != null)
            {
                _rhythmManager.OnNoteSpawned -= OnNoteSpawned;
                _rhythmManager.OnNoteDestroyed -= OnNoteDestroyed;
                _rhythmManager.OnJudge -= OnJudge;
                _rhythmManager.OnComboUpdate -= OnComboUpdate;
                
                if (_rhythmManager.Track != null)
                {
                    _rhythmManager.Track.OnNotePositionUpdated -= OnNotePositionUpdated;
                }
            }

            // 清理资源
            foreach (var visual in _noteVisuals.Values)
            {
                if (visual.GameObject != null)
                    Destroy(visual.GameObject);
            }
            _noteVisuals.Clear();

            foreach (var effect in _activeEffects)
            {
                if (effect.GameObject != null)
                    Destroy(effect.GameObject);
            }
            _activeEffects.Clear();

            foreach (var effect in _effectPool)
            {
                if (effect.GameObject != null)
                    Destroy(effect.GameObject);
            }
            _effectPool.Clear();
        }

        private void CreateQuadMesh()
        {
            _quadMesh = new Mesh();
            _quadMesh.vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
                new Vector3(0.5f, 0.5f, 0)
            };
            _quadMesh.uv = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(1, 1)
            };
            _quadMesh.triangles = new int[] { 0, 2, 1, 2, 3, 1 };
            _quadMesh.RecalculateNormals();
        }

        private void LoadShaders()
        {
            // 加载音符着色器
            Shader noteShader = Shader.Find("MaskSystem/RhythmNote");
            if (noteShader == null)
            {
                Debug.LogWarning("[RhythmVisualManager] RhythmNote shader not found, using fallback");
                noteShader = Shader.Find("Sprites/Default");
            }
            _noteShaderMaterial = new Material(noteShader);

            // 加载打击特效着色器
            Shader hitShader = Shader.Find("MaskSystem/HitFeedback");
            if (hitShader == null)
            {
                Debug.LogWarning("[RhythmVisualManager] HitFeedback shader not found, using fallback");
                hitShader = Shader.Find("Sprites/Default");
            }
            _hitEffectMaterial = new Material(hitShader);
        }

        private void CreateJudgeLine()
        {
            _judgeLineObject = new GameObject("JudgeLine");
            _judgeLineObject.transform.SetParent(transform);
            _judgeLineObject.transform.position = new Vector3(JudgeLineX, TrackY, -1f);
            _judgeLineObject.transform.localScale = new Vector3(JudgeLineWidth, JudgeLineHeight, 1f);

            var meshFilter = _judgeLineObject.AddComponent<MeshFilter>();
            meshFilter.mesh = _quadMesh;

            var meshRenderer = _judgeLineObject.AddComponent<MeshRenderer>();
            
            // 使用简单的不透明材质
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.color = JudgeLineColor;
            meshRenderer.material = mat;
            meshRenderer.sortingOrder = 5;
        }

        #endregion

        #region 更新

        private void Update()
        {
            UpdateEffects();
            UpdateJudgeText();
            UpdateNoteStates();
        }

        private void UpdateEffects()
        {
            float currentTime = Time.time;

            for (int i = _activeEffects.Count - 1; i >= 0; i--)
            {
                var effect = _activeEffects[i];
                float elapsed = currentTime - effect.StartTime;
                float progress = elapsed / effect.Duration;

                if (progress >= 1f)
                {
                    // 回收到对象池
                    effect.GameObject.SetActive(false);
                    _effectPool.Add(effect);
                    _activeEffects.RemoveAt(i);
                }
                else
                {
                    // 更新进度
                    effect.Material.SetFloat("_Progress", progress);
                }
            }
        }

        private void UpdateJudgeText()
        {
            if (_judgeTextTimer > 0)
            {
                _judgeTextTimer -= Time.deltaTime;
            }
        }

        private void UpdateNoteStates()
        {
            foreach (var kvp in _noteVisuals)
            {
                var visual = kvp.Value;
                if (visual.Note.State == NoteState.InJudgeZone)
                {
                    visual.Material.SetFloat("_InJudgeZone", 1f);
                }
                else
                {
                    visual.Material.SetFloat("_InJudgeZone", 0f);
                }
            }
        }

        #endregion

        #region 事件处理

        private void OnNoteSpawned(BeatNote note)
        {
            var visual = CreateNoteVisual(note);
            _noteVisuals[note.Id] = visual;
        }

        private void OnNoteDestroyed(BeatNote note)
        {
            if (_noteVisuals.TryGetValue(note.Id, out var visual))
            {
                if (visual.GameObject != null)
                {
                    Destroy(visual.GameObject);
                }
                _noteVisuals.Remove(note.Id);
            }
        }

        private void OnNotePositionUpdated(BeatNote note, Vector2 position)
        {
            if (_noteVisuals.TryGetValue(note.Id, out var visual))
            {
                visual.GameObject.transform.position = new Vector3(position.x, position.y, 0f);
            }
        }

        private void OnJudge(JudgeResultData result)
        {
            // 显示判定文字
            _lastJudgeText = result.GetResultText();
            _lastJudgeColor = result.GetResultColor();
            _judgeTextTimer = JudgeTextDuration;

            // 更新音符视觉
            if (_noteVisuals.TryGetValue(result.Note.Id, out var visual))
            {
                visual.Material.SetFloat("_JudgeResult", (float)result.Result);
            }

            // 生成打击特效
            SpawnHitEffect(result);

            OnJudgeVisualized?.Invoke(result);
        }

        private void OnComboUpdate(int totalCombo, int perfectCombo)
        {
            _currentCombo = totalCombo;
            _perfectCombo = perfectCombo;
        }

        #endregion

        #region 视觉创建

        private NoteVisual CreateNoteVisual(BeatNote note)
        {
            var go = new GameObject($"Note_{note.Id}_{note.Type}");
            go.transform.SetParent(transform);
            go.transform.position = new Vector3(SpawnX, TrackY, 0f);
            go.transform.localScale = new Vector3(NoteSize, NoteSize, 1f);

            var meshFilter = go.AddComponent<MeshFilter>();
            meshFilter.mesh = _quadMesh;

            var meshRenderer = go.AddComponent<MeshRenderer>();
            var mat = new Material(_noteShaderMaterial);
            
            // 设置音符类型
            mat.SetFloat("_NoteType", (float)note.Type);
            mat.SetColor("_MainColor", note.GetColor());
            
            meshRenderer.material = mat;
            meshRenderer.sortingOrder = 10;

            return new NoteVisual
            {
                Note = note,
                GameObject = go,
                Renderer = meshRenderer,
                Material = mat
            };
        }

        private void SpawnHitEffect(JudgeResultData result)
        {
            HitEffect effect;

            // 尝试从对象池获取
            if (_effectPool.Count > 0)
            {
                effect = _effectPool[_effectPool.Count - 1];
                _effectPool.RemoveAt(_effectPool.Count - 1);
                effect.GameObject.SetActive(true);
            }
            else
            {
                // 创建新的特效对象
                var go = new GameObject("HitEffect");
                go.transform.SetParent(transform);
                go.transform.localScale = new Vector3(EffectSize, EffectSize, 1f);

                var meshFilter = go.AddComponent<MeshFilter>();
                meshFilter.mesh = _quadMesh;

                var meshRenderer = go.AddComponent<MeshRenderer>();
                var mat = new Material(_hitEffectMaterial);
                meshRenderer.material = mat;
                meshRenderer.sortingOrder = 20;

                effect = new HitEffect
                {
                    GameObject = go,
                    Renderer = meshRenderer,
                    Material = mat
                };
            }

            // 设置位置（在判定线位置）
            effect.GameObject.transform.position = new Vector3(JudgeLineX, TrackY, -0.5f);

            // 设置特效类型
            float effectType = 0;
            switch (result.Result)
            {
                case JudgeResult.Perfect: effectType = 0; break;
                case JudgeResult.Normal: effectType = 1; break;
                case JudgeResult.Miss: effectType = 2; break;
            }
            effect.Material.SetFloat("_EffectType", effectType);
            effect.Material.SetFloat("_Progress", 0f);
            effect.Material.SetFloat("_Intensity", result.Result == JudgeResult.Perfect ? 1.5f : 1f);

            effect.StartTime = Time.time;
            effect.Duration = EffectDuration;

            _activeEffects.Add(effect);
        }

        #endregion

        #region UI 绘制

        private void OnGUI()
        {
            if (_rhythmManager == null || !_rhythmManager.IsRunning) return;

            DrawTrackUI();
            DrawJudgeResult();
            DrawCombo();
            DrawInputHint();
        }

        private void DrawTrackUI()
        {
            // 轨道背景
            float trackScreenY = Screen.height - (Screen.height * 0.2f);
            float trackHeight = 80f;
            
            GUI.color = new Color(0, 0, 0, 0.5f);
            GUI.DrawTexture(new Rect(0, trackScreenY - trackHeight / 2, Screen.width, trackHeight), Texture2D.whiteTexture);
            
            // 判定线指示
            float judgeScreenX = Screen.width * 0.25f;
            GUI.color = JudgeLineColor;
            GUI.DrawTexture(new Rect(judgeScreenX - 2, trackScreenY - trackHeight / 2, 4, trackHeight), Texture2D.whiteTexture);
            
            GUI.color = Color.white;
        }

        private void DrawJudgeResult()
        {
            if (_judgeTextTimer <= 0) return;

            float alpha = Mathf.Clamp01(_judgeTextTimer / JudgeTextDuration);
            float scale = 1f + (1f - alpha) * 0.5f;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = (int)(48 * scale);
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            Color col = _lastJudgeColor;
            col.a = alpha;
            style.normal.textColor = col;

            float y = Screen.height * 0.4f - (1f - alpha) * 50f;
            GUI.Label(new Rect(Screen.width / 2 - 200, y, 400, 60), _lastJudgeText, style);
        }

        private void DrawCombo()
        {
            if (_currentCombo <= 0) return;

            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 32;
            style.fontStyle = FontStyle.Bold;
            style.alignment = TextAnchor.MiddleCenter;

            // 连击数
            style.normal.textColor = Color.white;
            GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height * 0.5f, 200, 40), $"COMBO: {_currentCombo}", style);

            // 完美连击
            if (_perfectCombo > 0)
            {
                style.fontSize = 24;
                style.normal.textColor = new Color(1f, 0.9f, 0.2f);
                GUI.Label(new Rect(Screen.width / 2 - 100, Screen.height * 0.5f + 40, 200, 30), $"★ Perfect x{_perfectCombo}", style);
            }
        }

        private void DrawInputHint()
        {
            // 判定区内有音符时显示提示
            if (_rhythmManager.HasNoteInJudgeZone())
            {
                var note = _rhythmManager.GetCurrentNote();
                if (note != null)
                {
                    GUIStyle style = new GUIStyle(GUI.skin.label);
                    style.fontSize = 28;
                    style.fontStyle = FontStyle.Bold;
                    style.alignment = TextAnchor.MiddleCenter;

                    // 闪烁效果
                    float flash = Mathf.Abs(Mathf.Sin(Time.time * 8f));
                    Color hintColor = note.GetColor();
                    hintColor.a = 0.5f + flash * 0.5f;
                    style.normal.textColor = hintColor;

                    string hint = note.Type == NoteType.Defense ? "格挡!" : "按键!";
                    float judgeScreenX = Screen.width * 0.25f;
                    float trackScreenY = Screen.height * 0.8f;

                    GUI.Label(new Rect(judgeScreenX - 50, trackScreenY - 60, 100, 40), hint, style);
                }
            }
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// 设置轨道参数
        /// </summary>
        public void SetTrackParams(float trackY, float judgeLineX, float spawnX)
        {
            TrackY = trackY;
            JudgeLineX = judgeLineX;
            SpawnX = spawnX;

            if (_judgeLineObject != null)
            {
                _judgeLineObject.transform.position = new Vector3(JudgeLineX, TrackY, -1f);
            }
        }

        /// <summary>
        /// 清理所有视觉元素
        /// </summary>
        public void ClearAll()
        {
            foreach (var visual in _noteVisuals.Values)
            {
                if (visual.GameObject != null)
                    Destroy(visual.GameObject);
            }
            _noteVisuals.Clear();

            foreach (var effect in _activeEffects)
            {
                if (effect.GameObject != null)
                    effect.GameObject.SetActive(false);
                _effectPool.Add(effect);
            }
            _activeEffects.Clear();

            _currentCombo = 0;
            _perfectCombo = 0;
            _judgeTextTimer = 0;
        }

        /// <summary>
        /// 获取当前显示的音符数量
        /// </summary>
        public int GetActiveNoteCount()
        {
            return _noteVisuals.Count;
        }

        #endregion
    }
}

