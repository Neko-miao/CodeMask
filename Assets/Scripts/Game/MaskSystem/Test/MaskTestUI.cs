// ================================================
// MaskSystem - 测试UI（IMGUI）
// ================================================

using System.Text;
using UnityEngine;

namespace Game.MaskSystem.Test
{
    /// <summary>
    /// 面具系统测试UI - 使用IMGUI显示实时状态
    /// </summary>
    public class MaskTestUI : MonoBehaviour
    {
        #region 私有字段

        private IMaskSystemAPI _api;
        private StringBuilder _logBuilder = new StringBuilder();
        private Vector2 _logScrollPos;
        private const int MAX_LOG_LINES = 20;
        private int _logLineCount = 0;

        // UI配置
        private Rect _statusRect = new Rect(10, 10, 400, 200);
        private Rect _controlRect = new Rect(10, 220, 400, 280);
        private Rect _logRect = new Rect(10, 510, 400, 200);

        #endregion

        #region 公开方法

        public void SetAPI(IMaskSystemAPI api)
        {
            _api = api;

            // 订阅事件以记录日志
            _api.OnMaskChanged += (o, n) => AddLog($"面具: {o} -> {n}");
            _api.OnPlayerHealthChanged += (o, n) => AddLog($"玩家血量: {o} -> {n}");
            _api.OnEnemyHealthChanged += (o, n) => AddLog($"敌人血量: {o} -> {n}");
            _api.OnMaskAcquired += (m) => AddLog($"获得面具: {m}");
            _api.OnEnemyDefeated += () => AddLog("敌人被击败!");
            _api.OnPlayerDefeated += () => AddLog("玩家被击败!");
            _api.OnEnemySpawned += (m) => AddLog($"敌人生成: {m}");
            _api.OnPlayerAttacked += (r) => AddLog($"玩家攻击: {r.Damage}伤害{(r.IsCounter ? "(克制)" : "")}");
            _api.OnEnemyAttacked += (r) => AddLog($"敌人攻击: {r.Damage}伤害");
        }

        #endregion

        #region Unity生命周期

        private void OnGUI()
        {
            if (_api == null) return;

            // 设置GUI样式
            GUI.skin.box.fontSize = 14;
            GUI.skin.label.fontSize = 14;
            GUI.skin.button.fontSize = 14;

            DrawStatusPanel();
            DrawControlPanel();
            DrawLogPanel();
        }

        #endregion

        #region UI绘制

        private void DrawStatusPanel()
        {
            GUI.Box(_statusRect, "");
            GUILayout.BeginArea(_statusRect);
            GUILayout.Label("<color=yellow><b>===  面具系统状态 ===</b></color>", CreateRichTextStyle());
            GUILayout.Space(5);

            // 玩家状态
            GUILayout.Label($"<color=cyan>【玩家】</color>", CreateRichTextStyle());
            GUILayout.Label($"  血量: {_api.GetPlayerHealth()} / {_api.GetPlayerMaxHealth()}");
            GUILayout.Label($"  当前面具: <color=lime>{_api.GetCurrentMask()}</color> (槽位 {_api.GetCurrentSlot()})", CreateRichTextStyle());

            // 拥有的面具
            var masks = _api.GetOwnedMasks();
            string maskList = masks.Count > 0 ? string.Join(", ", masks) : "无";
            GUILayout.Label($"  拥有面具: [{maskList}]");

            GUILayout.Space(10);

            // 敌人状态
            GUILayout.Label($"<color=red>【敌人】</color>", CreateRichTextStyle());
            if (_api.HasEnemy)
            {
                GUILayout.Label($"  名称: {_api.GetEnemyName()} [{_api.GetEnemyMask()}]");
                GUILayout.Label($"  血量: {_api.GetEnemyHealth()} / {_api.GetEnemyMaxHealth()}");
                GUILayout.Label($"  状态: {(_api.IsEnemyAlive ? "<color=lime>存活</color>" : "<color=red>死亡</color>")}", CreateRichTextStyle());
            }
            else
            {
                GUILayout.Label("  无敌人");
            }

            GUILayout.EndArea();
        }

        private void DrawControlPanel()
        {
            GUI.Box(_controlRect, "");
            GUILayout.BeginArea(_controlRect);
            GUILayout.Label("<color=yellow><b>=== 操作说明 ===</b></color>", CreateRichTextStyle());
            GUILayout.Space(5);

            GUILayout.Label("<color=cyan>面具切换:</color>", CreateRichTextStyle());
            GUILayout.Label("  Q - 切换到槽位0  |  W - 槽位1  |  E - 槽位2");

            GUILayout.Space(5);
            GUILayout.Label("<color=cyan>战斗操作:</color>", CreateRichTextStyle());
            GUILayout.Label("  Space - 玩家攻击  |  D - 敌人攻击");
            GUILayout.Label("  K - 击败敌人  |  H - 恢复血量  |  R - 重置");

            GUILayout.Space(5);
            GUILayout.Label("<color=cyan>生成敌人:</color>", CreateRichTextStyle());
            GUILayout.Label("  1-蛇 | 2-猫 | 3-熊 | 4-牛 | 5-鲸鱼 | 6-鲨鱼 | 7-龙");

            GUILayout.Space(10);

            // 按钮区域
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("玩家攻击", GUILayout.Height(30)))
            {
                _api.PlayerAttack();
            }
            if (GUILayout.Button("敌人攻击", GUILayout.Height(30)))
            {
                _api.EnemyAttack();
            }
            if (GUILayout.Button("击败敌人", GUILayout.Height(30)))
            {
                _api.DefeatCurrentEnemy();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Q-槽位0", GUILayout.Height(30)))
            {
                _api.SwitchMask(0);
            }
            if (GUILayout.Button("W-槽位1", GUILayout.Height(30)))
            {
                _api.SwitchMask(1);
            }
            if (GUILayout.Button("E-槽位2", GUILayout.Height(30)))
            {
                _api.SwitchMask(2);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("生成蛇", GUILayout.Height(25)))
            {
                _api.SpawnEnemyByType(MaskType.Snake);
            }
            if (GUILayout.Button("生成猫", GUILayout.Height(25)))
            {
                _api.SpawnEnemyByType(MaskType.Cat);
            }
            if (GUILayout.Button("生成熊", GUILayout.Height(25)))
            {
                _api.SpawnEnemyByType(MaskType.Bear);
            }
            if (GUILayout.Button("生成牛", GUILayout.Height(25)))
            {
                _api.SpawnEnemyByType(MaskType.Bull);
            }
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawLogPanel()
        {
            GUI.Box(_logRect, "");
            GUILayout.BeginArea(_logRect);
            GUILayout.Label("<color=yellow><b>=== 事件日志 ===</b></color>", CreateRichTextStyle());

            _logScrollPos = GUILayout.BeginScrollView(_logScrollPos, GUILayout.Height(160));
            GUILayout.Label(_logBuilder.ToString());
            GUILayout.EndScrollView();

            GUILayout.EndArea();
        }

        #endregion

        #region 辅助方法

        private GUIStyle CreateRichTextStyle()
        {
            var style = new GUIStyle(GUI.skin.label);
            style.richText = true;
            style.fontSize = 14;
            return style;
        }

        private void AddLog(string message)
        {
            string timestamp = System.DateTime.Now.ToString("HH:mm:ss");
            _logBuilder.Insert(0, $"[{timestamp}] {message}\n");

            _logLineCount++;
            if (_logLineCount > MAX_LOG_LINES)
            {
                // 移除最老的日志
                int lastNewline = _logBuilder.ToString().LastIndexOf('\n', _logBuilder.Length - 2);
                if (lastNewline > 0)
                {
                    _logBuilder.Remove(lastNewline, _logBuilder.Length - lastNewline);
                }
                _logLineCount--;
            }
        }

        #endregion
    }
}

