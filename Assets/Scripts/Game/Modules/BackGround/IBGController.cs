namespace Game
{
    /// <summary>
    /// 背景控制器接口
    /// </summary>
    public interface IBGController
    {
        /// <summary>
        /// 是否正在播放
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// 中景移动速度（其他景别速度相对于此）
        /// </summary>
        float MidViewSpeed { get; set; }

        /// <summary>
        /// 播放背景滚动
        /// </summary>
        void Play();

        /// <summary>
        /// 暂停背景滚动
        /// </summary>
        void Pause();
    }
}
