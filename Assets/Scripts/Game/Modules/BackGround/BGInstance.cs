using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game
{
    /// <summary>
    /// 视差背景控制器（单例）
    /// 实现前景、中景、背景不同速度的循环滚动效果
    /// </summary>
    public class BGInstance : MonoBehaviour, IBGController
    {
        #region 单例

        private static BGInstance _instance;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static BGInstance Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BGInstance>();
                }
                return _instance;
            }
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
        }

        #endregion

        [Header("背景层级引用")]
        [Tooltip("远景层（背景）- 移动最慢")]
        [SerializeField] private Transform distantViewGroup;
        
        [Tooltip("中景层 - 移动速度中等")]
        [SerializeField] private Transform midViewGroup;
        
        [Tooltip("近景层（前景）- 移动最快")]
        [SerializeField] private Transform localViewGroup;

        [Header("移动速度设置")]
        [Tooltip("中景移动速度（基准速度）")]
        [SerializeField] private float midViewSpeed = 1f;
        
        [Tooltip("远景相对中景的速度倍率")]
        [SerializeField] private float distantSpeedRatio = 0.2f;
        
        [Tooltip("近景相对中景的速度倍率")]
        [SerializeField] private float localSpeedRatio = 2f;

        [Header("循环设置")]
        [Tooltip("背景宽度（用于循环计算）")]
        [SerializeField] private float backgroundWidth = 20f;
        
        [Tooltip("是否启用自动滚动")]
        [SerializeField] private bool autoScroll = true;
        
        [Tooltip("滚动方向（1=向左，-1=向右）")]
        [SerializeField] private int scrollDirection = 1;

        // 各层级的子物体列表
        private List<Transform> distantChildren = new List<Transform>();
        private List<Transform> midChildren = new List<Transform>();
        private List<Transform> localChildren = new List<Transform>();

        // 各层级的初始位置
        private Dictionary<Transform, Vector3> initialPositions = new Dictionary<Transform, Vector3>();

        // 是否暂停
        private bool isPaused = false;

        private void Start()
        {
            InitializeLayers();
        }

        private void Update()
        {
            if (!isPaused && autoScroll)
            {
                UpdateAllLayers();
            }
        }

        /// <summary>
        /// 初始化各层级
        /// </summary>
        private void InitializeLayers()
        {
            // 自动查找子层级（如果未手动指定）
            if (distantViewGroup == null)
                distantViewGroup = transform.Find("DistantViewGroup");
            if (midViewGroup == null)
                midViewGroup = transform.Find("MidViewGroup");
            if (localViewGroup == null)
                localViewGroup = transform.Find("LocalViewGroup");

            // 收集各层级的子物体
            CollectChildren(distantViewGroup, distantChildren);
            CollectChildren(midViewGroup, midChildren);
            CollectChildren(localViewGroup, localChildren);
        }

        /// <summary>
        /// 收集层级下的所有子物体
        /// </summary>
        private void CollectChildren(Transform parent, List<Transform> childList)
        {
            if (parent == null) return;

            childList.Clear();
            foreach (Transform child in parent)
            {
                childList.Add(child);
                initialPositions[child] = child.localPosition;
            }
        }

        /// <summary>
        /// 更新所有层级
        /// </summary>
        private void UpdateAllLayers()
        {
            float deltaTime = Time.deltaTime;
            
            // 更新远景层（相对中景最慢）
            UpdateLayer(distantChildren, midViewSpeed * distantSpeedRatio * deltaTime);
            
            // 更新中景层（基准速度）
            UpdateLayer(midChildren, midViewSpeed * deltaTime);
            
            // 更新近景层（相对中景最快）
            UpdateLayer(localChildren, midViewSpeed * localSpeedRatio * deltaTime);
        }

        /// <summary>
        /// 更新单个层级的所有子物体
        /// </summary>
        private void UpdateLayer(List<Transform> children, float speed)
        {
            foreach (var child in children)
            {
                if (child == null) continue;

                // 移动物体
                Vector3 pos = child.localPosition;
                pos.x -= speed * scrollDirection;
                child.localPosition = pos;

                // 检查并处理循环
                CheckAndLoopObject(child);
            }
        }

        /// <summary>
        /// 检查并循环物体位置
        /// </summary>
        private void CheckAndLoopObject(Transform obj)
        {
            Vector3 pos = obj.localPosition;
            
            // 当物体移出左边界时，将其移到右边
            if (scrollDirection > 0 && pos.x < -backgroundWidth)
            {
                pos.x += backgroundWidth * 2;
                obj.localPosition = pos;
            }
            // 当物体移出右边界时，将其移到左边
            else if (scrollDirection < 0 && pos.x > backgroundWidth)
            {
                pos.x -= backgroundWidth * 2;
                obj.localPosition = pos;
            }
        }

        #region 公共接口

        /// <summary>
        /// 是否正在播放
        /// </summary>
        public bool IsPlaying => !isPaused;

        /// <summary>
        /// 中景移动速度（其他景别速度相对于此）
        /// </summary>
        public float MidViewSpeed
        {
            get => midViewSpeed;
            set => midViewSpeed = value;
        }

        /// <summary>
        /// 播放背景滚动
        /// </summary>
        public void Play()
        {
            isPaused = false;
        }

        /// <summary>
        /// 暂停背景滚动
        /// </summary>
        public void Pause()
        {
            isPaused = true;
        }

        #endregion

#if UNITY_EDITOR
        /// <summary>
        /// 在编辑器中绘制边界辅助线
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Vector3 center = transform.position;
            
            // 绘制循环边界
            Gizmos.DrawLine(
                new Vector3(center.x - backgroundWidth, center.y - 10, center.z),
                new Vector3(center.x - backgroundWidth, center.y + 10, center.z)
            );
            Gizmos.DrawLine(
                new Vector3(center.x + backgroundWidth, center.y - 10, center.z),
                new Vector3(center.x + backgroundWidth, center.y + 10, center.z)
            );
        }
#endif
    }
}
