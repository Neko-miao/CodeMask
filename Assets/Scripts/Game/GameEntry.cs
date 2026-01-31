// ================================================
// Game - 游戏入口点
// ================================================

using UnityEngine;
using GameFramework.Core;

namespace Game
{
    /// <summary>
    /// 游戏入口点 - 创建并配置GameLauncher
    /// 将此脚本挂载到场景中的任意GameObject上即可启动游戏
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        [Header("启动设置")]
        [Tooltip("是否在Awake时自动初始化")]
        [SerializeField] private bool _autoInit = true;
        
        [Tooltip("初始游戏状态")]
        [SerializeField] private GameState _initialState = GameState.Menu;
        
        private GameLauncherExtended _launcher;
        
        private void Awake()
        {
            // 确保只有一个入口点
            if (FindObjectsOfType<GameEntry>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }
            
            DontDestroyOnLoad(gameObject);
            
            if (_autoInit)
            {
                InitializeGame();
            }
        }
        
        /// <summary>
        /// 初始化游戏
        /// </summary>
        public void InitializeGame()
        {
            if (_launcher != null)
            {
                Debug.LogWarning("[GameEntry] Game already initialized");
                return;
            }
            
            Debug.Log("[GameEntry] Initializing game...");
            
            // 创建GameLauncher
            var launcherGO = new GameObject("[GameLauncher]");
            launcherGO.transform.SetParent(transform);
            
            _launcher = launcherGO.AddComponent<GameLauncherExtended>();
            
            // GameLauncher会自动初始化，但我们需要等它完成后再设置状态
            // 因为它在Awake中会自动初始化到默认的Menu状态
            
            Debug.Log("[GameEntry] Game initialized successfully");
        }
        
        private void OnDestroy()
        {
            if (_launcher != null)
            {
                GameInstance.Instance?.Shutdown();
            }
        }
        
#if UNITY_EDITOR
        [ContextMenu("强制初始化")]
        private void ForceInit()
        {
            InitializeGame();
        }
        
        [ContextMenu("切换到主菜单")]
        private void SwitchToMenu()
        {
            GameInstance.Instance?.ChangeState(GameState.Menu);
        }
        
        [ContextMenu("切换到游戏")]
        private void SwitchToPlaying()
        {
            GameInstance.Instance?.ChangeState(GameState.Playing);
        }
#endif
    }
}
