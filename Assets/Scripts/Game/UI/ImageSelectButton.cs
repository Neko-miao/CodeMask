// ================================================
// Game - 图片选择按钮组件
// 根据传入的Type显示对应的Image
// ================================================

using System;
using UnityEngine;
using UnityEngine.UI;
using Game.Battle;

namespace Game.UI
{
    /// <summary>
    /// 图片选择按钮组件
    /// 根据传入的类型索引显示对应的Image，其他Image隐藏
    /// Image数组顺序对应 MaskType 枚举值（索引0=None, 1=Cat, 2=Snake, ...）
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ImageSelectButton : MonoBehaviour
    {
        [Header("Images")]
        [SerializeField] 
        [Tooltip("按MaskType枚举顺序放置Image：None(0), Cat(1), Snake(2), Bear(3), Horse(4), Bull(5), Whale(6), Shark(7), Dragon(8)")]
        private Image[] _images;
        
        [Header("Settings")]
        [SerializeField] 
        [Tooltip("当前显示的类型索引")]
        private int _currentType = 0;
        
        private Button _button;
        private MaskType _maskType = MaskType.None;
        private MaskData _maskData;
        
        /// <summary>
        /// 当前选中的类型索引
        /// </summary>
        public int CurrentType => _currentType;
        
        /// <summary>
        /// 当前的MaskType
        /// </summary>
        public MaskType MaskType => _maskType;
        
        /// <summary>
        /// 当前的MaskData
        /// </summary>
        public MaskData MaskData => _maskData;
        
        /// <summary>
        /// 类型数量
        /// </summary>
        public int TypeCount => _images?.Length ?? 0;
        
        /// <summary>
        /// 点击事件，参数为当前类型索引
        /// </summary>
        public event Action<int> OnClick;
        
        /// <summary>
        /// 点击事件，参数为MaskType
        /// </summary>
        public event Action<MaskType> OnMaskClick;
        
        /// <summary>
        /// 类型改变事件，参数为新的类型索引
        /// </summary>
        public event Action<int> OnTypeChanged;
        
        /// <summary>
        /// MaskType改变事件
        /// </summary>
        public event Action<MaskType> OnMaskTypeChanged;
        
        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(HandleClick);
            
            // 初始化显示
            UpdateDisplay();
        }
        
        private void OnDestroy()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }
        
        private void OnValidate()
        {
            // 编辑器中修改时更新显示
            if (_images != null && _images.Length > 0)
            {
                _currentType = Mathf.Clamp(_currentType, 0, _images.Length - 1);
                UpdateDisplay();
            }
        }
        
        /// <summary>
        /// 设置当前显示的类型
        /// </summary>
        /// <param name="type">类型索引（从0开始）</param>
        public void SetType(int type)
        {
            if (_images == null || _images.Length == 0)
            {
                Debug.LogWarning($"[ImageSelectButton] {gameObject.name}: No images assigned!");
                return;
            }
            
            if (type < 0 || type >= _images.Length)
            {
                Debug.LogWarning($"[ImageSelectButton] {gameObject.name}: Type {type} out of range (0-{_images.Length - 1})");
                return;
            }
            
            if (_currentType != type)
            {
                _currentType = type;
                _maskType = (MaskType)type;
                _maskData = MaskConfig.GetMaskData(_maskType);
                UpdateDisplay();
                OnTypeChanged?.Invoke(_currentType);
                OnMaskTypeChanged?.Invoke(_maskType);
            }
        }
        
        /// <summary>
        /// 设置MaskType并显示对应图片
        /// </summary>
        /// <param name="maskType">面具类型</param>
        public void SetMaskType(MaskType maskType)
        {
            int type = (int)maskType;
            SetType(type);
        }
        
        /// <summary>
        /// 初始化按钮数据（供MaskSystem调用）
        /// </summary>
        /// <param name="maskType">面具类型</param>
        /// <param name="maskData">面具数据（可选，如不传则自动从MaskConfig获取）</param>
        public void Initialize(MaskType maskType, MaskData maskData = null)
        {
            _maskType = maskType;
            _maskData = maskData ?? MaskConfig.GetMaskData(maskType);
            _currentType = (int)maskType;
            UpdateDisplay();
            
            Debug.Log($"[ImageSelectButton] 初始化: MaskType={maskType}, Name={_maskData?.Name ?? "Unknown"}");
        }
        
        /// <summary>
        /// 切换到下一个类型（循环）
        /// </summary>
        public void NextType()
        {
            if (_images == null || _images.Length == 0) return;
            
            int nextType = (_currentType + 1) % _images.Length;
            SetType(nextType);
        }
        
        /// <summary>
        /// 切换到上一个类型（循环）
        /// </summary>
        public void PreviousType()
        {
            if (_images == null || _images.Length == 0) return;
            
            int prevType = (_currentType - 1 + _images.Length) % _images.Length;
            SetType(prevType);
        }
        
        /// <summary>
        /// 获取当前显示的Image
        /// </summary>
        public Image GetCurrentImage()
        {
            if (_images == null || _currentType < 0 || _currentType >= _images.Length)
                return null;
            
            return _images[_currentType];
        }
        
        /// <summary>
        /// 获取指定类型的Image
        /// </summary>
        public Image GetImage(int type)
        {
            if (_images == null || type < 0 || type >= _images.Length)
                return null;
            
            return _images[type];
        }
        
        /// <summary>
        /// 设置按钮是否可交互
        /// </summary>
        public void SetInteractable(bool interactable)
        {
            if (_button != null)
            {
                _button.interactable = interactable;
            }
        }
        
        private void HandleClick()
        {
            OnClick?.Invoke(_currentType);
            OnMaskClick?.Invoke(_maskType);
        }
        
        private void UpdateDisplay()
        {
            if (_images == null) return;
            
            for (int i = 0; i < _images.Length; i++)
            {
                if (_images[i] != null)
                {
                    _images[i].gameObject.SetActive(i == _currentType);
                }
            }
        }
        
#if UNITY_EDITOR
        /// <summary>
        /// 编辑器工具：自动收集子物体的Image组件
        /// </summary>
        [ContextMenu("Auto Collect Child Images")]
        private void AutoCollectChildImages()
        {
            var childImages = GetComponentsInChildren<Image>(true);
            // 排除自身的Image（如果有）
            var selfImage = GetComponent<Image>();
            var imageList = new System.Collections.Generic.List<Image>();
            
            foreach (var img in childImages)
            {
                if (img != selfImage)
                {
                    imageList.Add(img);
                }
            }
            
            _images = imageList.ToArray();
            Debug.Log($"[ImageSelectButton] Collected {_images.Length} images from children");
        }
#endif
    }
}
