using System;
using System.Collections.Generic;
using UnityEngine;
using Game.Events;

namespace Game
{
    /// <summary>
    /// 节奏评分等级（三档：Perfect、Great、Miss）
    /// </summary>
    public enum RhythmScoreGrade
    {
        /// <summary>
        /// 完美 - 距离中心点最近
        /// </summary>
        Perfect,

        /// <summary>
        /// 优秀 - 距离中心点较近
        /// </summary>
        Great,

        /// <summary>
        /// 未命中 - 距离中心点较远或未触发
        /// </summary>
        Miss
    }

    /// <summary>
    /// 节奏触发区域 - 使用纯距离计算检测节奏并根据距离中心点的远近计算评分
    /// 不依赖任何物理组件（BoxCollider2D、Rigidbody2D）
    /// </summary>
    public class RhythmTriggerZone : MonoBehaviour
    {
        #region Singleton

        private static RhythmTriggerZone _instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static RhythmTriggerZone Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<RhythmTriggerZone>();
                }
                return _instance;
            }
        }

        #endregion

        [Header("触发区域设置")]
        [Tooltip("触发区域的总宽度")]
        [SerializeField]
        private float zoneWidth = 3f;

        [Header("评分区域设置（相对于区域宽度的百分比）")]
        [Tooltip("Perfect区域占中心的百分比（0-1）")]
        [SerializeField]
        [Range(0f, 1f)]
        private float perfectZonePercent = 0.33f;

        [Tooltip("Great区域占中心的百分比（0-1）")]
        [SerializeField]
        [Range(0f, 1f)]
        private float greatZonePercent = 0.66f;

        [Header("输入设置")]
        [Tooltip("触发判定的按键")]
        [SerializeField]
        private KeyCode triggerKey = KeyCode.Space;

        [Header("Debug设置")]
        [Tooltip("是否在Scene视图中显示评分区域")]
        [SerializeField]
        private bool showDebugGizmos = true;

        #region Events

        /// <summary>
        /// 节奏被触发事件（评分等级，节奏物体）
        /// </summary>
        public event Action<RhythmScoreGrade, Rhythm> OnRhythmTriggered;

        /// <summary>
        /// 节奏未命中事件
        /// </summary>
        public event Action<Rhythm> OnRhythmMiss;

        /// <summary>
        /// 全局节奏触发事件（静态事件，供其他系统订阅）
        /// </summary>
        public static event Action<RhythmTriggerEvent> OnRhythmTriggerEvent;

        #endregion

        #region Properties

        /// <summary>
        /// 触发区域中心点（世界坐标）
        /// </summary>
        public Vector2 ZoneCenter => (Vector2)transform.position;

        /// <summary>
        /// 触发区域宽度
        /// </summary>
        public float ZoneWidth => zoneWidth;

        /// <summary>
        /// 触发区域左边界X坐标
        /// </summary>
        public float ZoneLeftX => transform.position.x - zoneWidth / 2f;

        /// <summary>
        /// 触发区域右边界X坐标
        /// </summary>
        public float ZoneRightX => transform.position.x + zoneWidth / 2f;

        #endregion

        void Awake()
        {
            // 单例初始化
            if (_instance == null)
            {
                _instance = this;
            }
            else if (_instance != this)
            {
                Debug.LogWarning("[RhythmTriggerZone] 场景中存在多个RhythmTriggerZone实例");
            }
        }

        /// <summary>
        /// 检查节奏是否在触发区域内（纯距离计算）
        /// </summary>
        /// <param name="rhythm">要检查的节奏</param>
        /// <returns>是否在区域内</returns>
        public bool IsRhythmInZone(Rhythm rhythm)
        {
            if (rhythm == null) return false;
            
            float rhythmX = rhythm.PositionX;
            return rhythmX >= ZoneLeftX && rhythmX <= ZoneRightX;
        }

        /// <summary>
        /// 获取节奏到区域中心的距离
        /// </summary>
        /// <param name="rhythm">节奏物体</param>
        /// <returns>距离（绝对值）</returns>
        public float GetDistanceToCenter(Rhythm rhythm)
        {
            if (rhythm == null) return float.MaxValue;
            return Mathf.Abs(rhythm.PositionX - ZoneCenter.x);
        }

        /// <summary>
        /// 尝试触发当前区域内最近中心点的节奏
        /// </summary>
        /// <returns>是否成功触发</returns>
        public bool TryTriggerRhythm()
        {
            if (RhythmSystem.Instance == null)
            {
                Debug.Log("[RhythmTriggerZone] 触发失败：RhythmSystem不存在");
                return false;
            }

            var activeRhythms = RhythmSystem.Instance.GetActiveRhythms();
            if (activeRhythms == null || activeRhythms.Count == 0)
            {
                Debug.Log("[RhythmTriggerZone] 触发失败：没有活动的节奏");
                return false;
            }

            // 找到在区域内且距离中心点最近的节奏
            Rhythm closestRhythm = null;
            float closestDistance = float.MaxValue;

            foreach (var rhythm in activeRhythms)
            {
                if (rhythm == null) continue;
                if (!IsRhythmInZone(rhythm)) continue;

                float distance = GetDistanceToCenter(rhythm);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRhythm = rhythm;
                }
            }

            if (closestRhythm == null)
            {
                Debug.Log("[RhythmTriggerZone] 触发失败：区域内没有节奏");
                return false;
            }

            // 计算评分
            RhythmScoreGrade grade = CalculateScoreGrade(closestDistance);

            // Debug输出
            Debug.Log($"<color=yellow>[RhythmTriggerZone] 触发节奏: {closestRhythm.gameObject.name}</color>");
            Debug.Log($"<color=cyan>  距离中心: {closestDistance:F3}</color>");
            Debug.Log($"<color=green>  评分等级: {GetGradeDisplayName(grade)}</color>");

            // 触发事件
            OnRhythmTriggered?.Invoke(grade, closestRhythm);

            // 发布全局事件
            PublishRhythmTriggerEvent(closestRhythm.MaskType, closestRhythm.ActionType, grade);

            // 通知RhythmSystem移除并销毁
            RhythmSystem.Instance.RemoveAndDestroyRhythm(closestRhythm);

            return true;
        }

        /// <summary>
        /// 尝试触发指定行为类型的节奏
        /// </summary>
        /// <param name="actionType">行为类型</param>
        /// <returns>是否成功触发</returns>
        public bool TryTriggerRhythmByActionType(RhythmActionType actionType)
        {
            if (RhythmSystem.Instance == null)
            {
                Debug.Log("[RhythmTriggerZone] 触发失败：RhythmSystem不存在");
                return false;
            }

            var activeRhythms = RhythmSystem.Instance.GetActiveRhythms();
            if (activeRhythms == null || activeRhythms.Count == 0)
            {
                Debug.Log("[RhythmTriggerZone] 触发失败：没有活动的节奏");
                return false;
            }

            // 找到指定类型且在区域内且距离中心点最近的节奏
            Rhythm closestRhythm = null;
            float closestDistance = float.MaxValue;

            foreach (var rhythm in activeRhythms)
            {
                if (rhythm == null) continue;
                if (rhythm.ActionType != actionType) continue;
                if (!IsRhythmInZone(rhythm)) continue;

                float distance = GetDistanceToCenter(rhythm);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestRhythm = rhythm;
                }
            }

            if (closestRhythm == null)
            {
                Debug.Log($"[RhythmTriggerZone] 触发失败：区域内没有类型为 {actionType} 的节奏");
                return false;
            }

            // 计算评分
            RhythmScoreGrade grade = CalculateScoreGrade(closestDistance);

            // Debug输出
            Debug.Log($"<color=yellow>[RhythmTriggerZone] 触发节奏: {closestRhythm.gameObject.name} (类型: {actionType})</color>");
            Debug.Log($"<color=cyan>  距离中心: {closestDistance:F3}</color>");
            Debug.Log($"<color=green>  评分等级: {GetGradeDisplayName(grade)}</color>");

            // 触发事件
            OnRhythmTriggered?.Invoke(grade, closestRhythm);

            // 发布全局事件
            PublishRhythmTriggerEvent(closestRhythm.MaskType, closestRhythm.ActionType, grade);

            // 通知RhythmSystem移除并销毁
            RhythmSystem.Instance.RemoveAndDestroyRhythm(closestRhythm);

            return true;
        }

        /// <summary>
        /// 根据距离计算评分等级（三档：Perfect、Great、Miss）
        /// </summary>
        /// <param name="distance">距离中心点的距离</param>
        /// <returns>评分等级</returns>
        private RhythmScoreGrade CalculateScoreGrade(float distance)
        {
            float halfWidth = zoneWidth / 2f;
            float perfectThreshold = halfWidth * perfectZonePercent;
            float greatThreshold = halfWidth * greatZonePercent;

            if (distance <= perfectThreshold)
            {
                return RhythmScoreGrade.Perfect;
            }
            else if (distance <= greatThreshold)
            {
                return RhythmScoreGrade.Great;
            }
            else
            {
                // 在区域内但距离较远，判定为Miss
                return RhythmScoreGrade.Miss;
            }
        }

        /// <summary>
        /// 获取评分等级的显示名称
        /// </summary>
        private string GetGradeDisplayName(RhythmScoreGrade grade)
        {
            switch (grade)
            {
                case RhythmScoreGrade.Perfect:
                    return "★ Perfect ★";
                case RhythmScoreGrade.Great:
                    return "☆ Great ☆";
                case RhythmScoreGrade.Miss:
                    return "Miss...";
                default:
                    return grade.ToString();
            }
        }

        /// <summary>
        /// 当节奏未被触发到达终点时调用（由RhythmSystem调用）
        /// </summary>
        public void OnRhythmMissed(Rhythm rhythm)
        {
            if (rhythm == null) return;

            Debug.Log($"<color=red>[RhythmTriggerZone] 未命中: {rhythm.gameObject.name}</color>");
            Debug.Log($"<color=red>  评分等级: {GetGradeDisplayName(RhythmScoreGrade.Miss)}</color>");

            // 触发未命中事件
            OnRhythmMiss?.Invoke(rhythm);

            // 发布全局事件
            PublishRhythmTriggerEvent(rhythm.MaskType, rhythm.ActionType, RhythmScoreGrade.Miss);
        }

        /// <summary>
        /// 发布节奏触发全局事件
        /// </summary>
        /// <param name="maskType">面具类型</param>
        /// <param name="actionType">行为类型</param>
        /// <param name="grade">评分等级</param>
        private void PublishRhythmTriggerEvent(MaskType maskType, RhythmActionType actionType, RhythmScoreGrade grade)
        {
            var evt = new RhythmTriggerEvent(maskType, actionType, grade);
            OnRhythmTriggerEvent?.Invoke(evt);
            Debug.Log($"[RhythmTriggerZone] 已发布 RhythmTriggerEvent: MaskType={maskType}, ActionType={actionType}, Grade={grade}");
        }

        /// <summary>
        /// 处理输入
        /// </summary>
        void Update()
        {
            // 检测按键输入
            if (Input.GetKeyDown(triggerKey))
            {
                TryTriggerRhythm();
            }
        }

        void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// 在Scene视图中绘制评分区域（三档）
        /// </summary>
        void OnDrawGizmos()
        {
            if (!showDebugGizmos) return;

            Vector2 center = (Vector2)transform.position;
            float width = zoneWidth;
            float height = 2f; // 显示用的高度
            float halfWidth = width / 2f;

            // Miss区域（整个触发区域外圈）- 红色
            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawCube(center, new Vector3(width, height, 0.1f));

            // Great区域 - 黄色
            float greatWidth = width * greatZonePercent;
            Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
            Gizmos.DrawCube(center, new Vector3(greatWidth, height, 0.1f));

            // Perfect区域 - 绿色
            float perfectWidth = width * perfectZonePercent;
            Gizmos.color = new Color(0f, 1f, 0f, 0.4f);
            Gizmos.DrawCube(center, new Vector3(perfectWidth, height, 0.1f));

            // 中心线 - 白色
            Gizmos.color = Color.white;
            Gizmos.DrawLine(
                new Vector3(center.x, center.y - height / 2, 0),
                new Vector3(center.x, center.y + height / 2, 0)
            );

            // 绘制边界线
            // Perfect边界 - 绿色
            Gizmos.color = Color.green;
            float perfectHalf = halfWidth * perfectZonePercent;
            Gizmos.DrawLine(
                new Vector3(center.x - perfectHalf, center.y - height / 2, 0),
                new Vector3(center.x - perfectHalf, center.y + height / 2, 0)
            );
            Gizmos.DrawLine(
                new Vector3(center.x + perfectHalf, center.y - height / 2, 0),
                new Vector3(center.x + perfectHalf, center.y + height / 2, 0)
            );

            // Great边界 - 黄色
            Gizmos.color = Color.yellow;
            float greatHalf = halfWidth * greatZonePercent;
            Gizmos.DrawLine(
                new Vector3(center.x - greatHalf, center.y - height / 2, 0),
                new Vector3(center.x - greatHalf, center.y + height / 2, 0)
            );
            Gizmos.DrawLine(
                new Vector3(center.x + greatHalf, center.y - height / 2, 0),
                new Vector3(center.x + greatHalf, center.y + height / 2, 0)
            );

            // 外边界 - 红色
            Gizmos.color = Color.red;
            Gizmos.DrawLine(
                new Vector3(center.x - halfWidth, center.y - height / 2, 0),
                new Vector3(center.x - halfWidth, center.y + height / 2, 0)
            );
            Gizmos.DrawLine(
                new Vector3(center.x + halfWidth, center.y - height / 2, 0),
                new Vector3(center.x + halfWidth, center.y + height / 2, 0)
            );
        }
#endif
    }
}
