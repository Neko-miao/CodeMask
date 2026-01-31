// ================================================
// GameFramework - UI管理器实现
// ================================================

using System;
using System.Collections;
using System.Collections.Generic;
using GameFramework.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace GameFramework.UI
{
    /// <summary>
    /// UI管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.System, Priority = 100, 
        RequiredStates = new[] { GameState.Menu, GameState.Playing, GameState.Paused })]
    public class UIMgr : GameComponent, IUIMgr
    {
        private readonly Dictionary<Type, IUIController> _controllers = new Dictionary<Type, IUIController>();
        private readonly Dictionary<Type, IUIController> _cachedControllers = new Dictionary<Type, IUIController>();
        private readonly Dictionary<UILayer, List<IUIController>> _layerControllers = new Dictionary<UILayer, List<IUIController>>();
        private readonly Dictionary<UILayer, int> _layerOrders = new Dictionary<UILayer, int>();
        
        private Transform _uiRoot;
        private Canvas _rootCanvas;
        private Camera _uiCamera;
        private EventSystem _eventSystem;
        
        // UI Camera 配置
        private const int UI_CAMERA_DEPTH = 100;
        private const float UI_CAMERA_PLANE_DISTANCE = 100f;
        
        /// <summary>
        /// UI 相机
        /// </summary>
        public Camera UICamera => _uiCamera;
        
        /// <summary>
        /// UI 根节点
        /// </summary>
        public Transform UIRoot => _uiRoot;
        
        /// <summary>
        /// 根 Canvas
        /// </summary>
        public Canvas RootCanvas => _rootCanvas;
        
        public override string ComponentName => "UIMgr";
        public override ComponentType ComponentType => ComponentType.System;
        public override int Priority => 100;
        
        #region Events
        
        public event Action<IUIController> OnControllerOpened;
        public event Action<IUIController> OnControllerClosed;
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            CreateUICamera();
            CreateUIRoot();
            CreateEventSystem();
            InitializeLayers();
        }
        
        protected override void OnShutdown()
        {
            CloseAll();
            
            if (_uiRoot != null)
            {
                UnityEngine.Object.Destroy(_uiRoot.gameObject);
            }
            
            if (_uiCamera != null)
            {
                UnityEngine.Object.Destroy(_uiCamera.gameObject);
            }
            
            if (_eventSystem != null)
            {
                UnityEngine.Object.Destroy(_eventSystem.gameObject);
            }
        }
        
        #endregion
        
        #region Open/Close
        
        public TController Open<TController>(object data = null) where TController : class, IUIController, new()
        {
            var type = typeof(TController);
            
            // 检查是否已打开
            if (_controllers.TryGetValue(type, out var existingController))
            {
                existingController.Refresh();
                return existingController as TController;
            }
            
            // 检查缓存
            TController controller;
            if (_cachedControllers.TryGetValue(type, out var cachedController))
            {
                controller = cachedController as TController;
                _cachedControllers.Remove(type);
            }
            else
            {
                // 创建新的 Controller
                controller = new TController();
                
                // 加载 View
                var view = LoadView(controller.ViewPath);
                if (view == null) return null;
                
                // 初始化 Controller
                controller.Initialize(view);
            }
            
            // 打开
            controller.Open(data);
            
            // 注册
            _controllers[type] = controller;
            AddToLayer(controller);
            
            // 设置排序
            UpdateControllerOrder(controller);
            
            OnControllerOpened?.Invoke(controller);
            
            return controller;
        }
        
        public Coroutine OpenAsync<TController>(Action<TController> onComplete, object data = null) where TController : class, IUIController, new()
        {
            return GameInstance.Instance.RunCoroutine(OpenAsyncCoroutine<TController>(onComplete, data));
        }
        
        private IEnumerator OpenAsyncCoroutine<TController>(Action<TController> onComplete, object data) where TController : class, IUIController, new()
        {
            var type = typeof(TController);
            
            // 检查缓存，如果没有则预加载
            if (!_cachedControllers.ContainsKey(type) && !_controllers.ContainsKey(type))
            {
                bool isLoaded = false;
                PreloadAsync<TController>(() => isLoaded = true);
                
                while (!isLoaded)
                {
                    yield return null;
                }
            }
            
            var controller = Open<TController>(data);
            onComplete?.Invoke(controller);
        }
        
        public void Close<TController>() where TController : class, IUIController
        {
            var type = typeof(TController);
            
            if (_controllers.TryGetValue(type, out var controller))
            {
                CloseController(controller);
            }
        }
        
        public void Close(IUIController controller)
        {
            if (controller == null) return;
            CloseController(controller);
        }
        
        public void CloseByView(IUIView view)
        {
            if (view == null) return;
            
            // 查找拥有该 View 的 Controller
            foreach (var controller in _controllers.Values)
            {
                if (controller.View == view)
                {
                    CloseController(controller);
                    return;
                }
            }
        }
        
        public void CloseAll(UILayer layer = UILayer.All)
        {
            if (layer == UILayer.All)
            {
                var controllersToClose = new List<IUIController>(_controllers.Values);
                foreach (var controller in controllersToClose)
                {
                    CloseController(controller);
                }
            }
            else
            {
                if (_layerControllers.TryGetValue(layer, out var layerControllerList))
                {
                    var controllersToClose = new List<IUIController>(layerControllerList);
                    foreach (var controller in controllersToClose)
                    {
                        CloseController(controller);
                    }
                }
            }
        }
        
        #endregion
        
        #region Query
        
        public TController Get<TController>() where TController : class, IUIController
        {
            _controllers.TryGetValue(typeof(TController), out var controller);
            return controller as TController;
        }
        
        public bool TryGet<TController>(out TController controller) where TController : class, IUIController
        {
            if (_controllers.TryGetValue(typeof(TController), out var c))
            {
                controller = c as TController;
                return controller != null;
            }
            controller = null;
            return false;
        }
        
        public bool IsOpen<TController>() where TController : class, IUIController
        {
            return _controllers.ContainsKey(typeof(TController));
        }
        
        public bool IsOpen(IUIController controller)
        {
            return controller != null && _controllers.ContainsValue(controller);
        }
        
        public IReadOnlyList<IUIController> GetControllersByLayer(UILayer layer)
        {
            if (_layerControllers.TryGetValue(layer, out var controllers))
            {
                return controllers;
            }
            return Array.Empty<IUIController>();
        }
        
        public IReadOnlyList<IUIController> GetAllOpenControllers()
        {
            return new List<IUIController>(_controllers.Values);
        }
        
        #endregion
        
        #region Layer Management
        
        public void SetLayerOrder(UILayer layer, int order)
        {
            _layerOrders[layer] = order;
            
            // 更新该层级所有 Controller 的排序
            if (_layerControllers.TryGetValue(layer, out var controllers))
            {
                foreach (var controller in controllers)
                {
                    UpdateControllerOrder(controller);
                }
            }
        }
        
        public int GetLayerOrder(UILayer layer)
        {
            return _layerOrders.TryGetValue(layer, out var order) ? order : (int)layer;
        }
        
        #endregion
        
        #region Preload
        
        public Coroutine PreloadAsync<TController>(Action onComplete = null) where TController : class, IUIController, new()
        {
            return GameInstance.Instance.RunCoroutine(PreloadAsyncCoroutine<TController>(onComplete));
        }
        
        private IEnumerator PreloadAsyncCoroutine<TController>(Action onComplete) where TController : class, IUIController, new()
        {
            var type = typeof(TController);
            
            if (_cachedControllers.ContainsKey(type) || _controllers.ContainsKey(type))
            {
                onComplete?.Invoke();
                yield break;
            }
            
            // 创建 Controller 获取 ViewPath
            var tempController = new TController();
            string path = tempController.ViewPath;
            
            bool isLoaded = false;
            PreloadAsync(path, () => isLoaded = true);
            
            while (!isLoaded)
            {
                yield return null;
            }
            
            onComplete?.Invoke();
        }
        
        public Coroutine PreloadAsync(string viewPath, Action onComplete = null)
        {
            return GameInstance.Instance.RunCoroutine(PreloadPathAsyncCoroutine(viewPath, onComplete));
        }
        
        private IEnumerator PreloadPathAsyncCoroutine(string viewPath, Action onComplete)
        {
            var resourceMgr = GetComp<Components.IResourceMgr>();
            if (resourceMgr != null)
            {
                bool isLoaded = false;
                resourceMgr.LoadAsync<GameObject>(viewPath, _ => isLoaded = true);
                
                while (!isLoaded)
                {
                    yield return null;
                }
            }
            
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Private Methods
        
        private void CreateUICamera()
        {
            var cameraGo = new GameObject("[UICamera]");
            UnityEngine.Object.DontDestroyOnLoad(cameraGo);
            
            _uiCamera = cameraGo.AddComponent<Camera>();
            
            // 配置 UI 相机
            _uiCamera.clearFlags = CameraClearFlags.Depth;
            _uiCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");
            _uiCamera.orthographic = true;
            _uiCamera.orthographicSize = 5f;
            _uiCamera.nearClipPlane = 0.1f;
            _uiCamera.farClipPlane = 1000f;
            _uiCamera.depth = UI_CAMERA_DEPTH;
            _uiCamera.allowHDR = false;
            _uiCamera.allowMSAA = false;
            
            // 设置相机位置
            cameraGo.transform.position = new Vector3(0, 0, -UI_CAMERA_PLANE_DISTANCE);
            
            Debug.Log("[UIMgr] UICamera created");
        }
        
        private void CreateUIRoot()
        {
            var rootGo = new GameObject("[UIRoot]");
            UnityEngine.Object.DontDestroyOnLoad(rootGo);
            
            // 设置 UI Layer
            rootGo.layer = LayerMask.NameToLayer("UI");
            
            // 创建根 Canvas，使用 ScreenSpaceCamera 模式
            _rootCanvas = rootGo.AddComponent<Canvas>();
            _rootCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            _rootCanvas.worldCamera = _uiCamera;
            _rootCanvas.planeDistance = UI_CAMERA_PLANE_DISTANCE;
            _rootCanvas.sortingOrder = 0;
            
            // 配置 CanvasScaler
            var canvasScaler = rootGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = UnityEngine.UI.CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
            
            rootGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            
            _uiRoot = rootGo.transform;
            
            Debug.Log("[UIMgr] UIRoot created with ScreenSpaceCamera mode");
        }
        
        private void CreateEventSystem()
        {
            // 检查场景中是否已存在 EventSystem
            _eventSystem = UnityEngine.Object.FindObjectOfType<EventSystem>();
            
            if (_eventSystem == null)
            {
                var eventSystemGo = new GameObject("[EventSystem]");
                UnityEngine.Object.DontDestroyOnLoad(eventSystemGo);
                
                _eventSystem = eventSystemGo.AddComponent<EventSystem>();
                eventSystemGo.AddComponent<StandaloneInputModule>();
                
                Debug.Log("[UIMgr] EventSystem created");
            }
            else
            {
                Debug.Log("[UIMgr] EventSystem already exists in scene");
            }
        }
        
        private void InitializeLayers()
        {
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                if (layer == UILayer.All) continue;
                
                _layerControllers[layer] = new List<IUIController>();
                _layerOrders[layer] = (int)layer;
            }
        }
        
        private IUIView LoadView(string path)
        {
            var resourceMgr = GetComp<Components.IResourceMgr>();
            if (resourceMgr == null)
            {
                Debug.LogError("[UIMgr] ResourceMgr not found");
                return null;
            }
            
            var prefab = resourceMgr.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[UIMgr] View prefab not found: {path}");
                return null;
            }
            
            var go = UnityEngine.Object.Instantiate(prefab, _uiRoot);
            
            // 使用 UIViewBase 获取组件，因为 GetComponent 不能直接获取接口类型
            var view = go.GetComponent<UIViewBase>() as IUIView;
            
            if (view == null)
            {
                Debug.LogError($"[UIMgr] UIViewBase component not found on prefab: {path}");
                UnityEngine.Object.Destroy(go);
                return null;
            }
            
            // 初始化 View
            view.Initialize();
            
            return view;
        }
        
        private void CloseController(IUIController controller)
        {
            if (controller == null) return;
            
            var type = controller.GetType();
            
            controller.Close();
            
            _controllers.Remove(type);
            RemoveFromLayer(controller);
            
            // 缓存 Controller 以便复用
            _cachedControllers[type] = controller;
            
            OnControllerClosed?.Invoke(controller);
        }
        
        private void AddToLayer(IUIController controller)
        {
            var view = controller.View;
            if (view == null) return;
            
            if (_layerControllers.TryGetValue(view.Layer, out var controllers))
            {
                if (!controllers.Contains(controller))
                {
                    controllers.Add(controller);
                }
            }
        }
        
        private void RemoveFromLayer(IUIController controller)
        {
            var view = controller.View;
            if (view == null) return;
            
            if (_layerControllers.TryGetValue(view.Layer, out var controllers))
            {
                controllers.Remove(controller);
            }
        }
        
        private void UpdateControllerOrder(IUIController controller)
        {
            var view = controller.View;
            if (view?.Canvas == null) return;
            
            int baseOrder = GetLayerOrder(view.Layer);
            int indexInLayer = 0;
            
            if (_layerControllers.TryGetValue(view.Layer, out var controllers))
            {
                indexInLayer = controllers.IndexOf(controller);
            }
            
            view.Canvas.sortingOrder = baseOrder + indexInLayer;
        }
        
        #endregion
    }
}
