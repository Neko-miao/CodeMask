// ================================================
// MaskSystem Visual - 玩家视图
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Game.MaskSystem.Visual
{
    /// <summary>
    /// 玩家视图 - 处理玩家的视觉表现
    /// </summary>
    public class PlayerView : CharacterView
    {
        #region 额外组件

        [Header("玩家专属")]
        [Tooltip("面具槽位容器")]
        [SerializeField] private Transform maskSlotsContainer;

        [Tooltip("面具槽位预制体")]
        [SerializeField] private GameObject maskSlotPrefab;

        [Tooltip("当前选中指示器")]
        [SerializeField] private Image selectedIndicator;

        #endregion

        #region 私有字段

        private List<MaskSlotView> _maskSlots = new List<MaskSlotView>();
        private GameAssetsConfig _assets;
        private int _selectedSlot = 0;

        #endregion

        #region 事件

        public event Action<MaskType, MaskType> OnMaskSwitched;

        #endregion

        #region 初始化

        public override void Initialize(GameAssetsConfig assets)
        {
            base.Initialize(assets);
            _assets = assets;

            // 清空现有槽位
            ClearMaskSlots();
        }

        #endregion

        #region 面具槽位

        /// <summary>
        /// 设置拥有的面具
        /// </summary>
        public void SetOwnedMasks(IReadOnlyList<MaskType> masks)
        {
            ClearMaskSlots();

            if (maskSlotsContainer == null || maskSlotPrefab == null)
            {
                Debug.LogWarning("[PlayerView] 面具槽位容器或预制体未设置");
                return;
            }

            for (int i = 0; i < masks.Count && i < 3; i++)
            {
                var slotObj = Instantiate(maskSlotPrefab, maskSlotsContainer);
                var slotView = slotObj.GetComponent<MaskSlotView>();

                if (slotView == null)
                {
                    slotView = slotObj.AddComponent<MaskSlotView>();
                }

                slotView.Initialize(i, masks[i], _assets);
                slotView.SetKeyHint(GetKeyHint(i));
                _maskSlots.Add(slotView);
            }

            // 选中第一个
            if (_maskSlots.Count > 0)
            {
                SelectSlot(0);
            }
        }

        /// <summary>
        /// 添加面具到槽位
        /// </summary>
        public void AddMask(MaskType mask)
        {
            if (_maskSlots.Count >= 3)
            {
                Debug.LogWarning("[PlayerView] 面具槽位已满");
                return;
            }

            if (maskSlotsContainer != null && maskSlotPrefab != null)
            {
                var slotObj = Instantiate(maskSlotPrefab, maskSlotsContainer);
                var slotView = slotObj.GetComponent<MaskSlotView>();

                if (slotView == null)
                {
                    slotView = slotObj.AddComponent<MaskSlotView>();
                }

                int index = _maskSlots.Count;
                slotView.Initialize(index, mask, _assets);
                slotView.SetKeyHint(GetKeyHint(index));
                _maskSlots.Add(slotView);

                // 播放获得动画
                slotView.PlayAcquireAnimation();
            }
        }

        /// <summary>
        /// 选择槽位
        /// </summary>
        public void SelectSlot(int slot)
        {
            if (slot < 0 || slot >= _maskSlots.Count) return;

            // 取消之前的选中
            if (_selectedSlot >= 0 && _selectedSlot < _maskSlots.Count)
            {
                _maskSlots[_selectedSlot].SetSelected(false);
            }

            MaskType oldMask = _selectedSlot >= 0 && _selectedSlot < _maskSlots.Count 
                ? _maskSlots[_selectedSlot].MaskType 
                : MaskType.None;

            _selectedSlot = slot;
            _maskSlots[slot].SetSelected(true);

            // 更新角色面具显示
            SetMask(_maskSlots[slot].MaskType, _assets);

            // 更新选中指示器位置
            if (selectedIndicator != null)
            {
                selectedIndicator.rectTransform.position = _maskSlots[slot].transform.position;
            }

            OnMaskSwitched?.Invoke(oldMask, _maskSlots[slot].MaskType);
        }

        /// <summary>
        /// 播放面具切换动画
        /// </summary>
        public void PlayMaskSwitchAnimation(MaskType newMask)
        {
            // 切换效果
            if (_assets != null)
            {
                var visual = _assets.GetMaskVisual(newMask);
                if (visual?.SwitchEffect != null)
                {
                    SpawnEffect(visual.SwitchEffect);
                }
            }

            // 缩放弹跳效果
            if (characterRoot != null)
            {
                StartCoroutine(SwitchBounceAnimation());
            }
        }

        private System.Collections.IEnumerator SwitchBounceAnimation()
        {
            Vector3 originalScale = Vector3.one;
            float duration = 0.2f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float scale = 1f + Mathf.Sin(t * Mathf.PI) * 0.2f;
                characterRoot.localScale = originalScale * scale;
                yield return null;
            }

            characterRoot.localScale = originalScale;
        }

        private void ClearMaskSlots()
        {
            foreach (var slot in _maskSlots)
            {
                if (slot != null)
                    Destroy(slot.gameObject);
            }
            _maskSlots.Clear();
        }

        private string GetKeyHint(int index)
        {
            switch (index)
            {
                case 0: return "Q";
                case 1: return "W";
                case 2: return "E";
                default: return "";
            }
        }

        #endregion

        #region 动画重写

        public override void PlayAttackAnimation(Vector3 targetDirection)
        {
            // 玩家攻击向右
            base.PlayAttackAnimation(Vector3.right);
        }

        #endregion

        #region 清理

        private void OnDestroy()
        {
            ClearMaskSlots();
        }

        #endregion
    }

    /// <summary>
    /// 面具槽位视图
    /// </summary>
    public class MaskSlotView : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Image selectedFrame;
        [SerializeField] private Text keyHintText;

        public int SlotIndex { get; private set; }
        public MaskType MaskType { get; private set; }

        public void Initialize(int index, MaskType maskType, GameAssetsConfig assets)
        {
            SlotIndex = index;
            MaskType = maskType;

            if (assets != null)
            {
                var visual = assets.GetMaskVisual(maskType);
                if (visual != null)
                {
                    if (iconImage != null && visual.Icon != null)
                    {
                        iconImage.sprite = visual.Icon;
                        iconImage.color = Color.white;
                    }

                    if (backgroundImage != null)
                    {
                        backgroundImage.color = visual.BackgroundColor;
                    }
                }
            }

            SetSelected(false);
        }

        public void SetKeyHint(string hint)
        {
            if (keyHintText != null)
            {
                keyHintText.text = hint;
            }
        }

        public void SetSelected(bool selected)
        {
            if (selectedFrame != null)
            {
                selectedFrame.gameObject.SetActive(selected);
            }

            // 缩放效果
            transform.localScale = selected ? Vector3.one * 1.1f : Vector3.one;
        }

        public void PlayAcquireAnimation()
        {
            StartCoroutine(AcquireAnimationCoroutine());
        }

        private System.Collections.IEnumerator AcquireAnimationCoroutine()
        {
            transform.localScale = Vector3.zero;
            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                // 弹性效果
                float scale = 1f + Mathf.Sin(t * Mathf.PI * 2) * (1 - t) * 0.3f;
                transform.localScale = Vector3.one * scale * t;
                yield return null;
            }

            transform.localScale = Vector3.one;
        }
    }
}

