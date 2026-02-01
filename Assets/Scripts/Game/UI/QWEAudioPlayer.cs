// ================================================
// Game - QWE键音频播放器
// ================================================

using UnityEngine;
using GameFramework.Core;
using GameFramework.Components;

namespace Game.UI
{
    /// <summary>
    /// QWE键音频播放器
    /// 按下Q、W、E键时播放对应的音频
    /// </summary>
    public class QWEAudioPlayer : MonoBehaviour
    {
        [Header("音频配置")]
        [Tooltip("Q键音频文件名（放在Resources/Audio/SFX/目录下）")]
        [SerializeField]
        private string _qKeySound = "Q";
        
        [Tooltip("W键音频文件名（放在Resources/Audio/SFX/目录下）")]
        [SerializeField]
        private string _wKeySound = "W";
        
        [Tooltip("E键音频文件名（放在Resources/Audio/SFX/目录下）")]
        [SerializeField]
        private string _eKeySound = "E";
        
        [Header("播放设置")]
        [Tooltip("是否启用")]
        [SerializeField]
        private bool _isEnabled = true;
        
        [Tooltip("Q键音频播放起始时间（秒），从该时间点开始播放")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _qKeyStartTime = 0.8f;
        
        [Tooltip("W键音频播放起始时间（秒），从该时间点开始播放")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _wKeyStartTime = 0.8f;
        
        [Tooltip("E键音频播放起始时间（秒），从该时间点开始播放")]
        [SerializeField]
        [Range(0f, 10f)]
        private float _eKeyStartTime = 0.8f;
        
        private IAudioMgr _audioMgr;
        private AudioSource _audioSource;
        
        private void Start()
        {
            // 获取音频管理器
            _audioMgr = GameInstance.Instance?.GetComp<IAudioMgr>();
            
            if (_audioMgr == null)
            {
                Debug.LogWarning("[QWEAudioPlayer] 音频管理器未找到，音频播放功能将不可用");
            }
            
            // 创建本地AudioSource用于精确控制播放时间
            _audioSource = gameObject.AddComponent<AudioSource>();
            _audioSource.playOnAwake = false;
        }
        
        private void Update()
        {
            if (!_isEnabled || _audioMgr == null) return;
            
            // 检测Q键按下
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PlaySound(_qKeySound, _qKeyStartTime);
            }
            
            // 检测W键按下
            if (Input.GetKeyDown(KeyCode.W))
            {
                PlaySound(_wKeySound, _wKeyStartTime);
            }
            
            // 检测E键按下
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlaySound(_eKeySound, _eKeyStartTime);
            }
        }
        
        /// <summary>
        /// 播放音频
        /// </summary>
        /// <param name="soundName">音频文件名</param>
        /// <param name="startTime">播放起始时间（秒）</param>
        private void PlaySound(string soundName, float startTime = 0f)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogWarning($"[QWEAudioPlayer] 音频名称为空");
                return;
            }
            
            if (_audioSource == null)
            {
                Debug.LogWarning("[QWEAudioPlayer] AudioSource未初始化");
                return;
            }
            
            // 加载音频剪辑
            var clip = Resources.Load<AudioClip>($"Audio/SFX/{soundName}");
            if (clip == null)
            {
                Debug.LogWarning($"[QWEAudioPlayer] 音频文件未找到: {soundName}");
                return;
            }
            
            // 设置音频剪辑
            _audioSource.clip = clip;
            
            // 设置播放起始时间（确保不超过音频长度）
            float actualStartTime = Mathf.Clamp(startTime, 0f, clip.length);
            _audioSource.time = actualStartTime;
            
            // 设置音量（使用音频管理器的SFX音量）
            if (_audioMgr != null)
            {
                _audioSource.volume = _audioMgr.GetSFXVolume();
            }
            
            // 播放音频
            _audioSource.Play();
        }
        
        /// <summary>
        /// 设置是否启用
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _isEnabled = enabled;
        }
        
        /// <summary>
        /// 设置Q键音频
        /// </summary>
        public void SetQKeySound(string soundName)
        {
            _qKeySound = soundName;
        }
        
        /// <summary>
        /// 设置W键音频
        /// </summary>
        public void SetWKeySound(string soundName)
        {
            _wKeySound = soundName;
        }
        
        /// <summary>
        /// 设置E键音频
        /// </summary>
        public void SetEKeySound(string soundName)
        {
            _eKeySound = soundName;
        }
        
        /// <summary>
        /// 设置Q键音频播放起始时间
        /// </summary>
        public void SetQKeyStartTime(float startTime)
        {
            _qKeyStartTime = Mathf.Max(0f, startTime);
        }
        
        /// <summary>
        /// 设置W键音频播放起始时间
        /// </summary>
        public void SetWKeyStartTime(float startTime)
        {
            _wKeyStartTime = Mathf.Max(0f, startTime);
        }
        
        /// <summary>
        /// 设置E键音频播放起始时间
        /// </summary>
        public void SetEKeyStartTime(float startTime)
        {
            _eKeyStartTime = Mathf.Max(0f, startTime);
        }
    }
}

