// ================================================
// GameFramework - 渲染系统
// ================================================

using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 渲染系统 - 处理实体的渲染同步
    /// 需要: TransformComp, RenderComp
    /// </summary>
    public class RenderSystem : EntitySystem
    {
        private Transform _renderRoot;
        private readonly Dictionary<int, GameObject> _instantiated = new Dictionary<int, GameObject>();
        
        public override string SystemName => "RenderSystem";
        public override int Priority => 900;
        public override SystemPhase Phase => SystemPhase.LateUpdate;
        
        protected override void ConfigureRequirements()
        {
            Require<TransformComp>();
            Require<RenderComp>();
        }
        
        public override void OnInit(IEntityMgr entityMgr)
        {
            base.OnInit(entityMgr);
            
            // 创建渲染根节点
            var rootGo = new GameObject("[EntityRenderRoot]");
            Object.DontDestroyOnLoad(rootGo);
            _renderRoot = rootGo.transform;
        }
        
        public override void OnDestroy()
        {
            // 清理所有实例化的对象
            foreach (var kvp in _instantiated)
            {
                if (kvp.Value != null)
                {
                    Object.Destroy(kvp.Value);
                }
            }
            _instantiated.Clear();
            
            if (_renderRoot != null)
            {
                Object.Destroy(_renderRoot.gameObject);
            }
            
            base.OnDestroy();
        }
        
        public override void OnEntityEnter(IEntity entity)
        {
            var render = entity.GetComp<RenderComp>();
            if (render != null && !render.IsInstantiated && !string.IsNullOrEmpty(render.PrefabPath))
            {
                InstantiateRenderObject(entity, render);
            }
        }
        
        public override void OnEntityExit(IEntity entity)
        {
            var render = entity.GetComp<RenderComp>();
            if (render != null && render.IsInstantiated)
            {
                DestroyRenderObject(entity.Id, render);
            }
        }
        
        public override void ProcessEntity(IEntity entity, float deltaTime)
        {
            var transform = entity.GetComp<TransformComp>();
            var render = entity.GetComp<RenderComp>();
            
            if (transform == null || render == null) return;
            if (!render.IsEnabled) return;
            
            // 确保已实例化
            if (!render.IsInstantiated && !string.IsNullOrEmpty(render.PrefabPath))
            {
                InstantiateRenderObject(entity, render);
            }
            
            if (!render.IsInstantiated) return;
            
            // 同步Transform
            if (render.SyncTransform && render.Transform != null)
            {
                render.Transform.position = transform.Position;
                render.Transform.rotation = transform.RotationQuat;
                render.Transform.localScale = transform.Scale;
            }
            
            // 同步可见性
            if (render.GameObject != null && render.GameObject.activeSelf != render.IsVisible)
            {
                render.GameObject.SetActive(render.IsVisible);
            }
        }
        
        #region Private Methods
        
        private void InstantiateRenderObject(IEntity entity, RenderComp render)
        {
            var prefab = Resources.Load<GameObject>(render.PrefabPath);
            if (prefab == null)
            {
                Debug.LogWarning($"[RenderSystem] Prefab not found: {render.PrefabPath}");
                return;
            }
            
            var go = Object.Instantiate(prefab, _renderRoot);
            go.name = $"{entity.Name}_{entity.Id}";
            
            render.GameObject = go;
            render.Transform = go.transform;
            render.Renderer = go.GetComponentInChildren<Renderer>();
            render.Animator = go.GetComponentInChildren<Animator>();
            
            _instantiated[entity.Id] = go;
            
            // 初始同步位置
            var transform = entity.GetComp<TransformComp>();
            if (transform != null)
            {
                render.Transform.position = transform.Position;
                render.Transform.rotation = transform.RotationQuat;
                render.Transform.localScale = transform.Scale;
            }
        }
        
        private void DestroyRenderObject(int entityId, RenderComp render)
        {
            if (render.GameObject != null)
            {
                Object.Destroy(render.GameObject);
            }
            
            render.GameObject = null;
            render.Transform = null;
            render.Renderer = null;
            render.Animator = null;
            
            _instantiated.Remove(entityId);
        }
        
        #endregion
        
        #region Public Methods
        
        /// <summary>
        /// 手动实例化渲染对象
        /// </summary>
        public void Instantiate(int entityId, string prefabPath)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var render = entity.GetComp<RenderComp>();
            if (render == null)
            {
                render = entity.AddComp<RenderComp>();
            }
            
            render.PrefabPath = prefabPath;
            InstantiateRenderObject(entity, render);
        }
        
        /// <summary>
        /// 手动实例化渲染对象 (使用已有GameObject)
        /// </summary>
        public void SetGameObject(int entityId, GameObject gameObject)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var render = entity.GetComp<RenderComp>();
            if (render == null)
            {
                render = entity.AddComp<RenderComp>();
            }
            
            render.GameObject = gameObject;
            render.Transform = gameObject.transform;
            render.Renderer = gameObject.GetComponentInChildren<Renderer>();
            render.Animator = gameObject.GetComponentInChildren<Animator>();
            
            _instantiated[entityId] = gameObject;
        }
        
        /// <summary>
        /// 播放动画
        /// </summary>
        public void PlayAnimation(int entityId, string animationName, float crossFade = 0.1f)
        {
            var entity = GetEntity(entityId);
            if (entity == null) return;
            
            var render = entity.GetComp<RenderComp>();
            if (render?.Animator == null) return;
            
            render.Animator.CrossFade(animationName, crossFade);
        }
        
        /// <summary>
        /// 设置动画参数
        /// </summary>
        public void SetAnimatorFloat(int entityId, string paramName, float value)
        {
            var entity = GetEntity(entityId);
            var render = entity?.GetComp<RenderComp>();
            render?.Animator?.SetFloat(paramName, value);
        }
        
        public void SetAnimatorBool(int entityId, string paramName, bool value)
        {
            var entity = GetEntity(entityId);
            var render = entity?.GetComp<RenderComp>();
            render?.Animator?.SetBool(paramName, value);
        }
        
        public void SetAnimatorTrigger(int entityId, string paramName)
        {
            var entity = GetEntity(entityId);
            var render = entity?.GetComp<RenderComp>();
            render?.Animator?.SetTrigger(paramName);
        }
        
        #endregion
    }
}

