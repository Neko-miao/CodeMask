// ================================================
// MaskSystem - 测试场景组件
// ================================================

using UnityEngine;

namespace Game.MaskSystem.Test
{
    /// <summary>
    /// 面具系统测试场景组件
    /// 挂载到场景中的GameObject上，用于快速测试面具系统功能
    /// </summary>
    public class MaskSystemTestScene : MonoBehaviour
    {
        #region 配置

        [Header("系统配置")]
        [Tooltip("面具系统配置（可选，留空使用默认配置）")]
        [SerializeField] private MaskSystemConfig config;

        [Header("初始敌人")]
        [Tooltip("初始敌人类型")]
        [SerializeField] private MaskType initialEnemy = MaskType.Snake;

        [Header("调试选项")]
        [Tooltip("启用控制台日志")]
        [SerializeField] private bool enableLogging = true;

        [Tooltip("显示IMGUI调试界面")]
        [SerializeField] private bool showDebugUI = true;

        #endregion

        #region 私有字段

        private IMaskSystemAPI _api;
        private MaskTestUI _testUI;

        #endregion

        #region Unity生命周期

        private void Awake()
        {
            // 创建面具系统实例
            _api = MaskSystemFacade.CreateNew(config);

            // 创建测试UI
            if (showDebugUI)
            {
                _testUI = gameObject.AddComponent<MaskTestUI>();
                _testUI.SetAPI(_api);
            }

            Log("测试场景初始化完成");
        }

        private void Start()
        {
            // 生成初始敌人
            _api.SpawnEnemyByType(initialEnemy);

            // 订阅事件
            SubscribeEvents();

            Log("初始敌人已生成: " + initialEnemy);
        }

        private void Update()
        {
            HandleInput();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
            (_api as MaskSystemFacade)?.Dispose();
            Log("测试场景已销毁");
        }

        #endregion

        #region 输入处理

        private void HandleInput()
        {
            // Q/W/E 切换面具
            if (Input.GetKeyDown(KeyCode.Q))
            {
                _api.SwitchMask(0);
                Log("按下Q - 切换到槽位0");
            }
            else if (Input.GetKeyDown(KeyCode.W))
            {
                _api.SwitchMask(1);
                Log("按下W - 切换到槽位1");
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                _api.SwitchMask(2);
                Log("按下E - 切换到槽位2");
            }

            // Space 玩家攻击
            if (Input.GetKeyDown(KeyCode.Space))
            {
                var result = _api.PlayerAttack();
                Log($"按下Space - 玩家攻击: {result}");
            }

            // D 敌人攻击
            if (Input.GetKeyDown(KeyCode.D))
            {
                var result = _api.EnemyAttack();
                Log($"按下D - 敌人攻击: {result}");
            }

            // K 击败敌人
            if (Input.GetKeyDown(KeyCode.K))
            {
                _api.DefeatCurrentEnemy();
                Log("按下K - 击败当前敌人");
            }

            // R 重置玩家
            if (Input.GetKeyDown(KeyCode.R))
            {
                _api.ResetPlayer();
                Log("按下R - 重置玩家");
            }

            // 数字键 1-7 生成不同敌人
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                _api.SpawnEnemyByType(MaskType.Snake);
                Log("按下1 - 生成蛇");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                _api.SpawnEnemyByType(MaskType.Cat);
                Log("按下2 - 生成猫");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                _api.SpawnEnemyByType(MaskType.Bear);
                Log("按下3 - 生成熊");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                _api.SpawnEnemyByType(MaskType.Bull);
                Log("按下4 - 生成牛");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                _api.SpawnEnemyByType(MaskType.Whale);
                Log("按下5 - 生成鲸鱼");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                _api.SpawnEnemyByType(MaskType.Shark);
                Log("按下6 - 生成鲨鱼");
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                _api.SpawnEnemyByType(MaskType.Dragon);
                Log("按下7 - 生成龙");
            }

            // H 恢复血量
            if (Input.GetKeyDown(KeyCode.H))
            {
                _api.HealPlayer(1);
                Log("按下H - 恢复1点血量");
            }
        }

        #endregion

        #region 事件订阅

        private void SubscribeEvents()
        {
            _api.OnMaskChanged += OnMaskChanged;
            _api.OnPlayerHealthChanged += OnPlayerHealthChanged;
            _api.OnEnemyHealthChanged += OnEnemyHealthChanged;
            _api.OnMaskAcquired += OnMaskAcquired;
            _api.OnEnemyDefeated += OnEnemyDefeated;
            _api.OnPlayerDefeated += OnPlayerDefeated;
            _api.OnEnemySpawned += OnEnemySpawned;
        }

        private void UnsubscribeEvents()
        {
            _api.OnMaskChanged -= OnMaskChanged;
            _api.OnPlayerHealthChanged -= OnPlayerHealthChanged;
            _api.OnEnemyHealthChanged -= OnEnemyHealthChanged;
            _api.OnMaskAcquired -= OnMaskAcquired;
            _api.OnEnemyDefeated -= OnEnemyDefeated;
            _api.OnPlayerDefeated -= OnPlayerDefeated;
            _api.OnEnemySpawned -= OnEnemySpawned;
        }

        #endregion

        #region 事件处理

        private void OnMaskChanged(MaskType oldMask, MaskType newMask)
        {
            Log($"[事件] 面具切换: {oldMask} -> {newMask}");
        }

        private void OnPlayerHealthChanged(int oldHealth, int newHealth)
        {
            Log($"[事件] 玩家血量: {oldHealth} -> {newHealth}");
        }

        private void OnEnemyHealthChanged(int oldHealth, int newHealth)
        {
            Log($"[事件] 敌人血量: {oldHealth} -> {newHealth}");
        }

        private void OnMaskAcquired(MaskType mask)
        {
            Log($"[事件] 获得面具: {mask}");
        }

        private void OnEnemyDefeated()
        {
            Log("[事件] 敌人被击败!");
        }

        private void OnPlayerDefeated()
        {
            Log("[事件] 玩家被击败!");
        }

        private void OnEnemySpawned(MaskType enemyMask)
        {
            Log($"[事件] 敌人生成: {enemyMask}");
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 获取API接口（供外部使用）
        /// </summary>
        public IMaskSystemAPI GetAPI()
        {
            return _api;
        }

        #endregion

        #region 辅助方法

        private void Log(string message)
        {
            if (enableLogging)
            {
                Debug.Log($"[MaskSystemTest] {message}");
            }
        }

        #endregion
    }
}

