using System.Collections.Generic;
using UnityEngine;
using Game.Events;

namespace Game
{
    /// <summary>
    /// Mask系统 - 管理场景中所有Mask单位
    /// </summary>
    public class MaskSystem : MonoBehaviour
    {
        public static MaskSystem Instance { get; private set; }

        [Header("Mask预制体")]
        [SerializeField] private GameObject maskPrefab;

        [Header("位置引用")]
        [SerializeField] private List<Transform> spawnPositions = new List<Transform>(3);
        [SerializeField] private Transform launchTargetPoint;      // Perfect发射目标点
        [SerializeField] private Transform wearingTargetPoint;     // 穿戴位置目标点

        [Header("选择UI")]
        [SerializeField] private GameObject selectionUIRoot;
        [SerializeField] private List<UnityEngine.UI.Button> selectionButtons = new List<UnityEngine.UI.Button>(4);

        [Header("Perfect设置")]
        [SerializeField] private int perfectCountForLaunch = 3;
        [SerializeField] private int perfectCountForCreate = 6;

        // 数据
        private List<Mask> maskQueue = new List<Mask>(3);
        private Dictionary<KeyCode, Mask> keyBindings = new Dictionary<KeyCode, Mask>();
        private Mask pendingMask;
        private Mask currentWearingMask;  // 当前穿戴中的Mask
        private int perfectLaunchCount = 0;
        private int perfectCreateCount = 0;

        private static readonly KeyCode[] MaskKeys = { KeyCode.Q, KeyCode.W, KeyCode.E };

        /// <summary>
        /// 当前穿戴中的Mask（只读）
        /// </summary>
        public Mask CurrentWearingMask => currentWearingMask;

        #region Unity生命周期

        void Awake()
        {
            if (Instance == null) Instance = this;
            else { Destroy(gameObject); return; }
        }

        void Start()
        {
            RhythmTriggerZone.OnRhythmTriggerEvent += OnRhythmTriggered;
            InitializeSelectionUI();
        }

        void Update() => HandleKeyInput();

        void OnDestroy()
        {
            RhythmTriggerZone.OnRhythmTriggerEvent -= OnRhythmTriggered;
            if (Instance == this) Instance = null;
        }

        #endregion

        #region 按键处理

        private void HandleKeyInput()
        {
            // Mask穿戴按键 Q/W/E
            foreach (var key in MaskKeys)
            {
                if (Input.GetKeyDown(key))
                    OnMaskKeyPressed(key);
            }

            // 测试按键
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                Debug.Log("[MaskSystem] 测试：按1创建Mask");
                CreateMask();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                Debug.Log("[MaskSystem] 测试：按2发射Mask");
                LaunchRandomActiveMask();
            }
        }

        /// <summary>
        /// 按键按下处理：
        /// 1. 先卸下当前穿戴的Mask（如果有）
        /// 2. 将按键绑定的Mask穿上（如果是MaskMode状态）
        /// </summary>
        private void OnMaskKeyPressed(KeyCode key)
        {
            Debug.Log($"[MaskSystem] 按键 {key} 按下");

            // 获取按键绑定的目标Mask
            Mask targetMask = GetMaskByKey(key);
            
            // 如果目标Mask不存在或不是MaskMode状态，不执行穿戴
            if (targetMask == null || targetMask.CurrentState != MaskState.MaskMode)
            {
                Debug.Log($"[MaskSystem] 按键 {key} 绑定的Mask不可穿戴（不存在或非MaskMode状态）");
                return;
            }

            // 步骤1：卸下当前穿戴的Mask
            UnwearCurrentMask();

            // 步骤2：穿上目标Mask
            WearMask(targetMask);
        }

        /// <summary>
        /// 卸下当前穿戴的Mask（变回Active状态）
        /// </summary>
        private void UnwearCurrentMask()
        {
            if (currentWearingMask != null)
            {
                Debug.Log("[MaskSystem] 卸下当前穿戴的Mask");
                currentWearingMask.ResetAndActivate();
                currentWearingMask = null;
            }
        }

        /// <summary>
        /// 穿上指定Mask（直接传送到穿戴位置）
        /// </summary>
        private void WearMask(Mask mask)
        {
            if (wearingTargetPoint == null)
            {
                Debug.LogWarning("[MaskSystem] wearingTargetPoint未设置");
                return;
            }

            Debug.Log($"[MaskSystem] 穿上Mask: {mask.name}");
            currentWearingMask = mask;
            mask.WearAt(wearingTargetPoint.position);
        }

        #endregion

        #region 公共接口

        public bool CreateMask()
        {
            if (maskPrefab == null)
            {
                Debug.LogError("[MaskSystem] Mask预制体未设置");
                return false;
            }

            int nextIndex = maskQueue.Count < 3 ? maskQueue.Count : -1;
            
            if (nextIndex < 0)
            {
                // 队列已满，弹出选择窗口
                pendingMask = InstantiateMask(-1);
                if (pendingMask != null)
                {
                    pendingMask.gameObject.SetActive(false);
                    ShowSelectionUI();
                }
                return false;
            }

            Mask newMask = InstantiateMask(nextIndex);
            if (newMask != null)
            {
                maskQueue.Add(newMask);
                BindMaskToKey(newMask);
                Debug.Log($"[MaskSystem] 创建Mask成功，当前数量: {maskQueue.Count}");
                return true;
            }
            return false;
        }

        public Mask GetMask(int index) => (index >= 0 && index < maskQueue.Count) ? maskQueue[index] : null;
        public int GetMaskCount() => maskQueue.Count;
        public IReadOnlyList<Mask> GetAllMasks() => maskQueue;
        public KeyCode GetMaskBoundKey(Mask mask)
        {
            foreach (var kvp in keyBindings)
                if (kvp.Value == mask) return kvp.Key;
            return KeyCode.None;
        }
        public Mask GetMaskByKey(KeyCode key) => keyBindings.TryGetValue(key, out var mask) ? mask : null;

        public void RemoveMask(Mask mask)
        {
            if (mask == null || !maskQueue.Contains(mask)) return;
            UnbindMask(mask);
            maskQueue.Remove(mask);
            RearrangeMasks();
        }

        public void ClearAllMasks()
        {
            foreach (var mask in maskQueue)
                if (mask != null) Destroy(mask.gameObject);
            maskQueue.Clear();
            keyBindings.Clear();
        }

        #endregion

        #region Perfect事件处理

        private void OnRhythmTriggered(RhythmTriggerEvent evt)
        {
            if (evt.Result != RhythmScoreGrade.Perfect)
            {
                perfectLaunchCount = 0;
                perfectCreateCount = 0;
                return;
            }

            // 发射计数
            if (++perfectLaunchCount >= perfectCountForLaunch)
            {
                perfectLaunchCount = 0;
                LaunchRandomActiveMask();
            }

            // 生成计数
            if (++perfectCreateCount >= perfectCountForCreate)
            {
                perfectCreateCount = 0;
                CreateMask();
            }
        }

        private void LaunchRandomActiveMask()
        {
            if (launchTargetPoint == null)
            {
                Debug.LogWarning("[MaskSystem] launchTargetPoint未设置，无法发射");
                return;
            }

            var activeMasks = maskQueue.FindAll(m => m != null && m.CurrentState == MaskState.Active);
            if (activeMasks.Count == 0)
            {
                Debug.Log("[MaskSystem] 没有Active状态的Mask可发射");
                return;
            }

            var selected = activeMasks[Random.Range(0, activeMasks.Count)];
            Debug.Log($"[MaskSystem] 发射Mask: {selected.name}");
            selected.ActivateAndFlyTo(launchTargetPoint.position);
        }

        #endregion

        #region 按键绑定

        private void BindMaskToKey(Mask mask)
        {
            foreach (var key in MaskKeys)
            {
                if (!keyBindings.ContainsKey(key) || keyBindings[key] == null)
                {
                    keyBindings[key] = mask;
                    Debug.Log($"[MaskSystem] Mask绑定到 {key}");
                    return;
                }
            }
        }

        private void UnbindMask(Mask mask)
        {
            KeyCode toRemove = KeyCode.None;
            foreach (var kvp in keyBindings)
                if (kvp.Value == mask) { toRemove = kvp.Key; break; }
            if (toRemove != KeyCode.None)
                keyBindings.Remove(toRemove);
        }

        #endregion

        #region 选择UI

        private void InitializeSelectionUI()
        {
            if (selectionUIRoot != null) selectionUIRoot.SetActive(false);
            for (int i = 0; i < selectionButtons.Count; i++)
            {
                int idx = i;
                if (selectionButtons[i] != null)
                    selectionButtons[i].onClick.AddListener(() => OnSelectionButtonClicked(idx));
            }
        }

        private void ShowSelectionUI()
        {
            if (selectionUIRoot == null) { OnSelectionButtonClicked(0); return; }
            selectionUIRoot.SetActive(true);
            Time.timeScale = 0f;
            for (int i = 0; i < selectionButtons.Count; i++)
            {
                if (selectionButtons[i] != null)
                    selectionButtons[i].gameObject.SetActive(i < 3 ? i < maskQueue.Count : pendingMask != null);
            }
        }

        private void HideSelectionUI()
        {
            if (selectionUIRoot != null) selectionUIRoot.SetActive(false);
            Time.timeScale = 1f;
        }

        private void OnSelectionButtonClicked(int idx)
        {
            if (idx < 3 && idx < maskQueue.Count)
            {
                var removed = maskQueue[idx];
                maskQueue.RemoveAt(idx);
                if (removed != null) { UnbindMask(removed); Destroy(removed.gameObject); }

                if (pendingMask != null)
                {
                    int newIdx = maskQueue.Count;
                    if (newIdx < spawnPositions.Count && spawnPositions[newIdx] != null)
                        pendingMask.transform.position = spawnPositions[newIdx].position;
                    pendingMask.gameObject.SetActive(true);
                    maskQueue.Add(pendingMask);
                    BindMaskToKey(pendingMask);
                    pendingMask = null;
                }
                RearrangeMasks();
            }
            else if (idx == 3 && pendingMask != null)
            {
                Destroy(pendingMask.gameObject);
                pendingMask = null;
            }
            HideSelectionUI();
        }

        private void RearrangeMasks()
        {
            for (int i = 0; i < maskQueue.Count; i++)
                if (maskQueue[i] != null && i < spawnPositions.Count && spawnPositions[i] != null)
                    maskQueue[i].transform.position = spawnPositions[i].position;
        }

        #endregion

        #region 辅助方法

        private Mask InstantiateMask(int posIdx)
        {
            Vector3 pos = (posIdx >= 0 && posIdx < spawnPositions.Count && spawnPositions[posIdx] != null)
                ? spawnPositions[posIdx].position : Vector3.zero;

            var obj = Instantiate(maskPrefab, pos, Quaternion.identity);
            var mask = obj.GetComponent<Mask>() ?? obj.AddComponent<Mask>();
            mask.SetState(MaskState.Active);
            return mask;
        }

        #endregion

#if UNITY_EDITOR
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < spawnPositions.Count; i++)
                if (spawnPositions[i] != null)
                {
                    Gizmos.DrawWireSphere(spawnPositions[i].position, 0.5f);
                    UnityEditor.Handles.Label(spawnPositions[i].position + Vector3.up * 0.7f, $"Slot {i + 1}");
                }
            if (wearingTargetPoint != null)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(wearingTargetPoint.position, 0.5f);
                UnityEditor.Handles.Label(wearingTargetPoint.position + Vector3.up * 0.7f, "Wearing Target");
            }
        }
#endif
    }
}
