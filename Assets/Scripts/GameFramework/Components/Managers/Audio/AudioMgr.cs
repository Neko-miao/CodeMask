// ================================================
// GameFramework - 音频管理器实现
// ================================================

using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 音频管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.Core, Priority = 40, RequiredStates = new[] { GameState.Global })]
    public class AudioMgr : GameComponent, IAudioMgr
    {
        private AudioSource _bgmSource;
        private readonly List<AudioSource> _sfxSources = new List<AudioSource>();
        private readonly Dictionary<int, AudioSource> _activeSfx = new Dictionary<int, AudioSource>();
        private Transform _audioRoot;
        
        private float _masterVolume = 1f;
        private float _bgmVolume = 1f;
        private float _sfxVolume = 1f;
        private bool _isMuted = false;
        private string _currentBGM = string.Empty;
        private int _nextSfxId = 1;
        
        private const int MAX_SFX_SOURCES = 16;
        
        public override string ComponentName => "AudioMgr";
        public override ComponentType ComponentType => ComponentType.Core;
        public override int Priority => 40;
        
        #region Properties
        
        public bool IsBGMPlaying => _bgmSource != null && _bgmSource.isPlaying;
        public string CurrentBGM => _currentBGM;
        public bool IsMuted => _isMuted;
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            // 创建音频根节点
            var go = new GameObject("[AudioMgr]");
            Object.DontDestroyOnLoad(go);
            _audioRoot = go.transform;
            
            // 创建BGM音源
            var bgmGo = new GameObject("BGM");
            bgmGo.transform.SetParent(_audioRoot);
            _bgmSource = bgmGo.AddComponent<AudioSource>();
            _bgmSource.playOnAwake = false;
            
            // 预创建SFX音源池
            for (int i = 0; i < MAX_SFX_SOURCES; i++)
            {
                CreateSfxSource();
            }
        }
        
        protected override void OnShutdown()
        {
            StopBGM();
            StopAllSFX();
            
            if (_audioRoot != null)
            {
                Object.Destroy(_audioRoot.gameObject);
            }
        }
        
        #endregion
        
        #region BGM
        
        public void PlayBGM(string name, bool loop = true, float fadeInDuration = 0f)
        {
            if (string.IsNullOrEmpty(name)) return;
            
            var clip = Resources.Load<AudioClip>($"Audio/BGM/{name}");
            if (clip != null)
            {
                PlayBGM(clip, loop, fadeInDuration);
                _currentBGM = name;
            }
            else
            {
                Debug.LogWarning($"[AudioMgr] BGM not found: {name}");
            }
        }
        
        public void PlayBGM(AudioClip clip, bool loop = true, float fadeInDuration = 0f)
        {
            if (clip == null || _bgmSource == null) return;
            
            _bgmSource.clip = clip;
            _bgmSource.loop = loop;
            _bgmSource.volume = _isMuted ? 0f : _bgmVolume * _masterVolume;
            _bgmSource.Play();
        }
        
        public void StopBGM(float fadeOutDuration = 0f)
        {
            if (_bgmSource == null) return;
            
            _bgmSource.Stop();
            _currentBGM = string.Empty;
        }
        
        public void PauseBGM()
        {
            _bgmSource?.Pause();
        }
        
        public void ResumeBGM()
        {
            _bgmSource?.UnPause();
        }
        
        public void SetBGMVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp01(volume);
            if (_bgmSource != null && !_isMuted)
            {
                _bgmSource.volume = _bgmVolume * _masterVolume;
            }
        }
        
        public float GetBGMVolume() => _bgmVolume;
        
        #endregion
        
        #region SFX
        
        public int PlaySFX(string name, float volume = 1f)
        {
            if (string.IsNullOrEmpty(name)) return -1;
            
            var clip = Resources.Load<AudioClip>($"Audio/SFX/{name}");
            if (clip != null)
            {
                return PlaySFX(clip, volume);
            }
            
            Debug.LogWarning($"[AudioMgr] SFX not found: {name}");
            return -1;
        }
        
        public int PlaySFX(AudioClip clip, float volume = 1f)
        {
            if (clip == null) return -1;
            
            var source = GetAvailableSfxSource();
            if (source == null) return -1;
            
            int id = _nextSfxId++;
            _activeSfx[id] = source;
            
            source.clip = clip;
            source.volume = _isMuted ? 0f : volume * _sfxVolume * _masterVolume;
            source.spatialBlend = 0f;
            source.Play();
            
            return id;
        }
        
        public int PlaySFX3D(string name, Vector3 position, float volume = 1f)
        {
            if (string.IsNullOrEmpty(name)) return -1;
            
            var clip = Resources.Load<AudioClip>($"Audio/SFX/{name}");
            if (clip != null)
            {
                return PlaySFX3D(clip, position, volume);
            }
            
            return -1;
        }
        
        public int PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f)
        {
            if (clip == null) return -1;
            
            var source = GetAvailableSfxSource();
            if (source == null) return -1;
            
            int id = _nextSfxId++;
            _activeSfx[id] = source;
            
            source.transform.position = position;
            source.clip = clip;
            source.volume = _isMuted ? 0f : volume * _sfxVolume * _masterVolume;
            source.spatialBlend = 1f;
            source.Play();
            
            return id;
        }
        
        public void StopSFX(int sfxId)
        {
            if (_activeSfx.TryGetValue(sfxId, out var source))
            {
                source.Stop();
                _activeSfx.Remove(sfxId);
            }
        }
        
        public void StopAllSFX()
        {
            foreach (var source in _sfxSources)
            {
                source.Stop();
            }
            _activeSfx.Clear();
        }
        
        public void SetSFXVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp01(volume);
        }
        
        public float GetSFXVolume() => _sfxVolume;
        
        #endregion
        
        #region Global
        
        public void SetMasterVolume(float volume)
        {
            _masterVolume = Mathf.Clamp01(volume);
            UpdateAllVolumes();
        }
        
        public float GetMasterVolume() => _masterVolume;
        
        public void SetMute(bool mute)
        {
            _isMuted = mute;
            UpdateAllVolumes();
        }
        
        public void PauseAll()
        {
            _bgmSource?.Pause();
            foreach (var source in _sfxSources)
            {
                source.Pause();
            }
        }
        
        public void ResumeAll()
        {
            _bgmSource?.UnPause();
            foreach (var source in _sfxSources)
            {
                source.UnPause();
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private AudioSource CreateSfxSource()
        {
            var go = new GameObject($"SFX_{_sfxSources.Count}");
            go.transform.SetParent(_audioRoot);
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            _sfxSources.Add(source);
            return source;
        }
        
        private AudioSource GetAvailableSfxSource()
        {
            foreach (var source in _sfxSources)
            {
                if (!source.isPlaying)
                {
                    return source;
                }
            }
            
            // 如果没有可用的，复用最老的
            return _sfxSources.Count > 0 ? _sfxSources[0] : null;
        }
        
        private void UpdateAllVolumes()
        {
            if (_bgmSource != null)
            {
                _bgmSource.volume = _isMuted ? 0f : _bgmVolume * _masterVolume;
            }
        }
        
        #endregion
    }
}

