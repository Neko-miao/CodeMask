// ================================================
// GameFramework - ECS系统接口
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Entity
{
    /// <summary>
    /// 系统执行阶段
    /// </summary>
    public enum SystemPhase
    {
        /// <summary>
        /// 早期更新 (输入处理等)
        /// </summary>
        EarlyUpdate = 0,
        
        /// <summary>
        /// 主更新 (逻辑处理)
        /// </summary>
        Update = 1,
        
        /// <summary>
        /// 后期更新 (相机跟随等)
        /// </summary>
        LateUpdate = 2,
        
        /// <summary>
        /// 固定更新 (物理处理)
        /// </summary>
        FixedUpdate = 3,
        
        /// <summary>
        /// 渲染前
        /// </summary>
        PreRender = 4,
        
        /// <summary>
        /// 渲染后
        /// </summary>
        PostRender = 5
    }
    
    /// <summary>
    /// ECS系统接口 - 包含逻辑，处理符合条件的实体
    /// System只包含逻辑，通过组件签名过滤需要处理的实体
    /// </summary>
    public interface IEntitySystem
    {
        #region Properties
        
        /// <summary>
        /// 系统名称
        /// </summary>
        string SystemName { get; }
        
        /// <summary>
        /// 执行优先级 (数值越小越先执行)
        /// </summary>
        int Priority { get; }
        
        /// <summary>
        /// 执行阶段
        /// </summary>
        SystemPhase Phase { get; }
        
        /// <summary>
        /// 是否启用
        /// </summary>
        bool IsEnabled { get; set; }
        
        /// <summary>
        /// 是否已初始化
        /// </summary>
        bool IsInitialized { get; }
        
        /// <summary>
        /// 需要的组件签名 (实体必须拥有这些组件才会被此系统处理)
        /// </summary>
        ulong RequiredSignature { get; }
        
        /// <summary>
        /// 排除的组件签名 (实体如果拥有这些组件则不会被处理)
        /// </summary>
        ulong ExcludedSignature { get; }
        
        /// <summary>
        /// 需要的标签
        /// </summary>
        EntityTag RequiredTags { get; }
        
        /// <summary>
        /// 排除的标签
        /// </summary>
        EntityTag ExcludedTags { get; }
        
        #endregion
        
        #region Lifecycle
        
        /// <summary>
        /// 初始化
        /// </summary>
        void OnInit(IEntityMgr entityMgr);
        
        /// <summary>
        /// 销毁
        /// </summary>
        void OnDestroy();
        
        /// <summary>
        /// 启用时
        /// </summary>
        void OnEnable();
        
        /// <summary>
        /// 禁用时
        /// </summary>
        void OnDisable();
        
        #endregion
        
        #region Entity Events
        
        /// <summary>
        /// 实体进入此系统 (首次满足条件)
        /// </summary>
        void OnEntityEnter(IEntity entity);
        
        /// <summary>
        /// 实体离开此系统 (不再满足条件)
        /// </summary>
        void OnEntityExit(IEntity entity);
        
        #endregion
        
        #region Update
        
        /// <summary>
        /// 更新 (处理所有符合条件的实体)
        /// </summary>
        void OnUpdate(float deltaTime, IReadOnlyList<IEntity> entities);
        
        /// <summary>
        /// 处理单个实体
        /// </summary>
        void ProcessEntity(IEntity entity, float deltaTime);
        
        #endregion
        
        #region Query
        
        /// <summary>
        /// 检查实体是否满足此系统的处理条件
        /// </summary>
        bool MatchEntity(IEntity entity);
        
        #endregion
    }
}

