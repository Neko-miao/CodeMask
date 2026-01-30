// ================================================
// GameFramework - 音频管理器接口
// ================================================

using System;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.Components
{
    /// <summary>
    /// 音频管理器接口
    /// </summary>
    public interface IAudioMgr : IGameComponent
    {
        #region BGM
        
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        void PlayBGM(string name, bool loop = true, float fadeInDuration = 0f);
        
        /// <summary>
        /// 播放背景音乐
        /// </summary>
        void PlayBGM(AudioClip clip, bool loop = true, float fadeInDuration = 0f);
        
        /// <summary>
        /// 停止背景音乐
        /// </summary>
        void StopBGM(float fadeOutDuration = 0f);
        
        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        void PauseBGM();
        
        /// <summary>
        /// 恢复背景音乐
        /// </summary>
        void ResumeBGM();
        
        /// <summary>
        /// 设置BGM音量
        /// </summary>
        void SetBGMVolume(float volume);
        
        /// <summary>
        /// 获取BGM音量
        /// </summary>
        float GetBGMVolume();
        
        /// <summary>
        /// BGM是否正在播放
        /// </summary>
        bool IsBGMPlaying { get; }
        
        /// <summary>
        /// 当前BGM名称
        /// </summary>
        string CurrentBGM { get; }
        
        #endregion
        
        #region SFX
        
        /// <summary>
        /// 播放音效
        /// </summary>
        int PlaySFX(string name, float volume = 1f);
        
        /// <summary>
        /// 播放音效
        /// </summary>
        int PlaySFX(AudioClip clip, float volume = 1f);
        
        /// <summary>
        /// 播放3D音效
        /// </summary>
        int PlaySFX3D(string name, Vector3 position, float volume = 1f);
        
        /// <summary>
        /// 播放3D音效
        /// </summary>
        int PlaySFX3D(AudioClip clip, Vector3 position, float volume = 1f);
        
        /// <summary>
        /// 停止音效
        /// </summary>
        void StopSFX(int sfxId);
        
        /// <summary>
        /// 停止所有音效
        /// </summary>
        void StopAllSFX();
        
        /// <summary>
        /// 设置音效音量
        /// </summary>
        void SetSFXVolume(float volume);
        
        /// <summary>
        /// 获取音效音量
        /// </summary>
        float GetSFXVolume();
        
        #endregion
        
        #region Global
        
        /// <summary>
        /// 设置主音量
        /// </summary>
        void SetMasterVolume(float volume);
        
        /// <summary>
        /// 获取主音量
        /// </summary>
        float GetMasterVolume();
        
        /// <summary>
        /// 静音/取消静音
        /// </summary>
        void SetMute(bool mute);
        
        /// <summary>
        /// 是否静音
        /// </summary>
        bool IsMuted { get; }
        
        /// <summary>
        /// 暂停所有音频
        /// </summary>
        void PauseAll();
        
        /// <summary>
        /// 恢复所有音频
        /// </summary>
        void ResumeAll();
        
        #endregion
    }
}

