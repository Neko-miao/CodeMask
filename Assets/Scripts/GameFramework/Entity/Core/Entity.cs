// ================================================
// GameFramework - ECS实体实现
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Entity
{
    /// <summary>
    /// ECS实体实现 - 纯数据容器，只负责存储组件
    /// </summary>
    public class Entity : IEntity
    {
        private readonly Dictionary<int, IEntityComp> _comps = new Dictionary<int, IEntityComp>();
        private readonly List<IEntityComp> _compList = new List<IEntityComp>();
        
        private int _id;
        private int _version;
        private string _name;
        private EntityTag _tags;
        private bool _isActive = true;
        private bool _isDestroyed;
        private ulong _compSignature;
        
        #region Identity
        
        public int Id => _id;
        public int Version => _version;
        
        public string Name
        {
            get => _name;
            set => _name = value;
        }
        
        public EntityTag Tags
        {
            get => _tags;
            set => _tags = value;
        }
        
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }
        
        public bool IsDestroyed => _isDestroyed;
        
        public int CompCount => _comps.Count;
        
        public ulong CompSignature => _compSignature;
        
        #endregion
        
        #region Events
        
        public event Action<IEntity, IEntityComp> OnCompAdded;
        public event Action<IEntity, IEntityComp> OnCompRemoved;
        
        #endregion
        
        #region Constructor
        
        public Entity(int id)
        {
            _id = id;
            _version = 0;
            _name = $"Entity_{id}";
        }
        
        /// <summary>
        /// 内部初始化 (用于对象池复用)
        /// </summary>
        internal void Initialize(int id)
        {
            _id = id;
            _version++;
            _name = $"Entity_{id}";
            _tags = EntityTag.None;
            _isActive = true;
            _isDestroyed = false;
            _compSignature = 0;
        }
        
        #endregion
        
        #region Component Access
        
        public T AddComp<T>() where T : class, IEntityComp, new()
        {
            var typeId = CompType<T>.Id;
            
            if (_comps.ContainsKey(typeId))
            {
                return _comps[typeId] as T;
            }
            
            var comp = new T();
            AddCompInternal(typeId, comp);
            
            return comp;
        }
        
        public void AddComp(IEntityComp comp)
        {
            if (comp == null) return;
            
            var typeId = comp.CompTypeId;
            
            if (_comps.ContainsKey(typeId))
            {
                // 替换现有组件
                RemoveCompInternal(typeId);
            }
            
            AddCompInternal(typeId, comp);
        }
        
        public T GetComp<T>() where T : class, IEntityComp
        {
            var typeId = CompType<T>.Id;
            
            if (_comps.TryGetValue(typeId, out var comp))
            {
                return comp as T;
            }
            
            return null;
        }
        
        public bool TryGetComp<T>(out T comp) where T : class, IEntityComp
        {
            comp = GetComp<T>();
            return comp != null;
        }
        
        public bool HasComp<T>() where T : class, IEntityComp
        {
            return _comps.ContainsKey(CompType<T>.Id);
        }
        
        public bool HasComp(int compTypeId)
        {
            return _comps.ContainsKey(compTypeId);
        }
        
        public bool RemoveComp<T>() where T : class, IEntityComp
        {
            return RemoveComp(CompType<T>.Id);
        }
        
        public bool RemoveComp(int compTypeId)
        {
            if (!_comps.ContainsKey(compTypeId))
                return false;
            
            RemoveCompInternal(compTypeId);
            return true;
        }
        
        public IReadOnlyList<IEntityComp> GetAllComps()
        {
            return _compList;
        }
        
        #endregion
        
        #region Tag Operations
        
        public void AddTag(EntityTag tag)
        {
            _tags |= tag;
        }
        
        public void RemoveTag(EntityTag tag)
        {
            _tags &= ~tag;
        }
        
        public bool HasTag(EntityTag tag)
        {
            return (_tags & tag) == tag;
        }
        
        public bool HasAnyTag(EntityTag tags)
        {
            return (_tags & tags) != 0;
        }
        
        public bool HasAllTags(EntityTag tags)
        {
            return (_tags & tags) == tags;
        }
        
        #endregion
        
        #region Lifecycle
        
        public void Destroy()
        {
            _isDestroyed = true;
        }
        
        public void Reset()
        {
            // 移除所有组件
            foreach (var comp in _compList)
            {
                comp.Reset();
            }
            
            _comps.Clear();
            _compList.Clear();
            _compSignature = 0;
            _tags = EntityTag.None;
            _isActive = true;
            _isDestroyed = false;
            _version++;
        }
        
        #endregion
        
        #region Private Methods
        
        private void AddCompInternal(int typeId, IEntityComp comp)
        {
            comp.EntityId = _id;
            _comps[typeId] = comp;
            _compList.Add(comp);
            
            // 更新组件签名 (仅支持前64种组件类型)
            if (typeId < 64)
            {
                _compSignature |= (1UL << typeId);
            }
            
            OnCompAdded?.Invoke(this, comp);
        }
        
        private void RemoveCompInternal(int typeId)
        {
            if (_comps.TryGetValue(typeId, out var comp))
            {
                _comps.Remove(typeId);
                _compList.Remove(comp);
                
                // 更新组件签名
                if (typeId < 64)
                {
                    _compSignature &= ~(1UL << typeId);
                }
                
                OnCompRemoved?.Invoke(this, comp);
            }
        }
        
        #endregion
    }
}
