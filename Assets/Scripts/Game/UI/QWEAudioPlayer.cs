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
        
        [Header("设置")]
        [Tooltip("是否启用")]
        [SerializeField]
        private bool _isEnabled = true;
        
        private IAudioMgr _audioMgr;
        
        private void Start()
        {
            // 获取音频管理器
            _audioMgr = GameInstance.Instance?.GetComp<IAudioMgr>();
            
            if (_audioMgr == null)
            {
                Debug.LogWarning("[QWEAudioPlayer] 音频管理器未找到，音频播放功能将不可用");
            }
        }
        
        private void Update()
        {
            if (!_isEnabled || _audioMgr == null) return;
            
            // 检测Q键按下
            if (Input.GetKeyDown(KeyCode.Q))
            {
                PlaySound(_qKeySound);
            }
            
            // 检测W键按下
            if (Input.GetKeyDown(KeyCode.W))
            {
                PlaySound(_wKeySound);
            }
            
            // 检测E键按下
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlaySound(_eKeySound);
            }
        }
        
        /// <summary>
        /// 播放音频
        /// </summary>
        private void PlaySound(string soundName)
        {
            if (string.IsNullOrEmpty(soundName))
            {
                Debug.LogWarning($"[QWEAudioPlayer] 音频名称为空");
                return;
            }
            
            _audioMgr.PlaySFX(soundName);
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
    }
}

