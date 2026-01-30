// ================================================
// GameFramework - UI管理器实现
// ================================================

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameFramework.Core;
using UnityEngine;

namespace GameFramework.UI
{
    /// <summary>
    /// UI管理器实现
    /// </summary>
    [ComponentInfo(Type = ComponentType.System, Priority = 100, 
        RequiredStates = new[] { GameState.Menu, GameState.Playing, GameState.Paused })]
    public class UIMgr : GameComponent, IUIMgr
    {
        private readonly Dictionary<Type, IUIView> _views = new Dictionary<Type, IUIView>();
        private readonly Dictionary<Type, IUIView> _cachedViews = new Dictionary<Type, IUIView>();
        private readonly Dictionary<UILayer, List<IUIView>> _layerViews = new Dictionary<UILayer, List<IUIView>>();
        private readonly Dictionary<UILayer, int> _layerOrders = new Dictionary<UILayer, int>();
        
        private Transform _uiRoot;
        private Canvas _rootCanvas;
        
        public override string ComponentName => "UIMgr";
        public override ComponentType ComponentType => ComponentType.System;
        public override int Priority => 100;
        
        #region Events
        
        public event Action<IUIView> OnViewOpened;
        public event Action<IUIView> OnViewClosed;
        
        #endregion
        
        #region Lifecycle
        
        protected override void OnInit()
        {
            CreateUIRoot();
            InitializeLayers();
        }
        
        protected override void OnShutdown()
        {
            CloseAll();
            
            if (_uiRoot != null)
            {
                UnityEngine.Object.Destroy(_uiRoot.gameObject);
            }
        }
        
        #endregion
        
        #region Open/Close
        
        public T Open<T>(object data = null) where T : class, IUIView
        {
            var type = typeof(T);
            
            // 检查是否已打开
            if (_views.TryGetValue(type, out var existingView))
            {
                existingView.Refresh();
                return existingView as T;
            }
            
            // 检查缓存
            if (!_cachedViews.TryGetValue(type, out var view))
            {
                view = LoadView<T>();
                if (view == null) return null;
            }
            else
            {
                _cachedViews.Remove(type);
            }
            
            // 初始化并打开
            view.Initialize();
            view.Open(data);
            
            // 注册
            _views[type] = view;
            AddToLayer(view);
            
            // 设置排序
            UpdateViewOrder(view);
            
            OnViewOpened?.Invoke(view);
            
            return view as T;
        }
        
        public async Task<T> OpenAsync<T>(object data = null) where T : class, IUIView
        {
            var type = typeof(T);
            
            // 检查缓存
            if (!_cachedViews.ContainsKey(type))
            {
                await PreloadAsync<T>();
            }
            
            return Open<T>(data);
        }
        
        public void Close<T>() where T : class, IUIView
        {
            var type = typeof(T);
            
            if (_views.TryGetValue(type, out var view))
            {
                CloseView(view);
            }
        }
        
        public void Close(IUIView view)
        {
            if (view == null) return;
            CloseView(view);
        }
        
        public void CloseAll(UILayer layer = UILayer.All)
        {
            if (layer == UILayer.All)
            {
                var viewsToClose = new List<IUIView>(_views.Values);
                foreach (var view in viewsToClose)
                {
                    CloseView(view);
                }
            }
            else
            {
                if (_layerViews.TryGetValue(layer, out var layerViewList))
                {
                    var viewsToClose = new List<IUIView>(layerViewList);
                    foreach (var view in viewsToClose)
                    {
                        CloseView(view);
                    }
                }
            }
        }
        
        #endregion
        
        #region Query
        
        public T Get<T>() where T : class, IUIView
        {
            _views.TryGetValue(typeof(T), out var view);
            return view as T;
        }
        
        public bool TryGet<T>(out T view) where T : class, IUIView
        {
            if (_views.TryGetValue(typeof(T), out var v))
            {
                view = v as T;
                return view != null;
            }
            view = null;
            return false;
        }
        
        public bool IsOpen<T>() where T : class, IUIView
        {
            return _views.ContainsKey(typeof(T));
        }
        
        public bool IsOpen(IUIView view)
        {
            return view != null && _views.ContainsValue(view);
        }
        
        public IReadOnlyList<IUIView> GetViewsByLayer(UILayer layer)
        {
            if (_layerViews.TryGetValue(layer, out var views))
            {
                return views;
            }
            return Array.Empty<IUIView>();
        }
        
        public IReadOnlyList<IUIView> GetAllOpenViews()
        {
            return new List<IUIView>(_views.Values);
        }
        
        #endregion
        
        #region Layer Management
        
        public void SetLayerOrder(UILayer layer, int order)
        {
            _layerOrders[layer] = order;
            
            // 更新该层级所有视图的排序
            if (_layerViews.TryGetValue(layer, out var views))
            {
                foreach (var view in views)
                {
                    UpdateViewOrder(view);
                }
            }
        }
        
        public int GetLayerOrder(UILayer layer)
        {
            return _layerOrders.TryGetValue(layer, out var order) ? order : (int)layer;
        }
        
        #endregion
        
        #region Preload
        
        public async Task PreloadAsync<T>() where T : class, IUIView
        {
            var type = typeof(T);
            
            if (_cachedViews.ContainsKey(type) || _views.ContainsKey(type))
                return;
            
            string path = GetViewPath<T>();
            await PreloadAsync(path);
        }
        
        public async Task PreloadAsync(string viewPath)
        {
            var resourceMgr = GetComp<Components.IResourceMgr>();
            if (resourceMgr != null)
            {
                var prefab = await resourceMgr.LoadAsync<GameObject>(viewPath);
                // 预加载完成后会在LoadView时使用
            }
        }
        
        #endregion
        
        #region Private Methods
        
        private void CreateUIRoot()
        {
            var rootGo = new GameObject("[UIRoot]");
            UnityEngine.Object.DontDestroyOnLoad(rootGo);
            _uiRoot = rootGo.transform;
            
            // 创建根Canvas
            _rootCanvas = rootGo.AddComponent<Canvas>();
            _rootCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _rootCanvas.sortingOrder = 0;
            
            // rootGo.AddComponent<UnityEngine.UI.CanvasScaler>();
            // rootGo.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        }
        
        private void InitializeLayers()
        {
            foreach (UILayer layer in Enum.GetValues(typeof(UILayer)))
            {
                if (layer == UILayer.All) continue;
                
                _layerViews[layer] = new List<IUIView>();
                _layerOrders[layer] = (int)layer;
            }
        }
        
        private T LoadView<T>() where T : class, IUIView
        {
            string path = GetViewPath<T>();
            
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
            var view = go.GetComponent<T>();
            
            if (view == null)
            {
                Debug.LogError($"[UIMgr] View component not found on prefab: {path}");
                UnityEngine.Object.Destroy(go);
                return null;
            }
            
            return view;
        }
        
        private string GetViewPath<T>() where T : class, IUIView
        {
            var type = typeof(T);
            // 默认路径规则: UI/{ViewName}
            return $"UI/{type.Name}";
        }
        
        private void CloseView(IUIView view)
        {
            if (view == null) return;
            
            var type = view.GetType();
            
            view.Close();
            
            _views.Remove(type);
            RemoveFromLayer(view);
            
            // 缓存视图以便复用
            _cachedViews[type] = view;
            
            OnViewClosed?.Invoke(view);
        }
        
        private void AddToLayer(IUIView view)
        {
            if (_layerViews.TryGetValue(view.Layer, out var views))
            {
                if (!views.Contains(view))
                {
                    views.Add(view);
                }
            }
        }
        
        private void RemoveFromLayer(IUIView view)
        {
            if (_layerViews.TryGetValue(view.Layer, out var views))
            {
                views.Remove(view);
            }
        }
        
        private void UpdateViewOrder(IUIView view)
        {
            if (view?.Canvas == null) return;
            
            int baseOrder = GetLayerOrder(view.Layer);
            int indexInLayer = 0;
            
            if (_layerViews.TryGetValue(view.Layer, out var views))
            {
                indexInLayer = views.IndexOf(view);
            }
            
            view.Canvas.sortingOrder = baseOrder + indexInLayer;
        }
        
        #endregion
    }
}

