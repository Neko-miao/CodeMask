// ================================================
// GameFramework - 场景上下文接口
// ================================================

using System.Collections.Generic;
using UnityEngine;

namespace GameFramework.World
{
    /// <summary>
    /// 生成点类型
    /// </summary>
    public enum SpawnPointType
    {
        Default = 0,
        Player = 1,
        Enemy = 2,
        Item = 3,
        Boss = 4
    }
    
    /// <summary>
    /// 生成点数据
    /// </summary>
    public struct SpawnPoint
    {
        public int Id;
        public SpawnPointType Type;
        public Vector3 Position;
        public Quaternion Rotation;
        public string Tag;
        
        public SpawnPoint(int id, SpawnPointType type, Vector3 position, Quaternion rotation = default, string tag = null)
        {
            Id = id;
            Type = type;
            Position = position;
            Rotation = rotation == default ? Quaternion.identity : rotation;
            Tag = tag;
        }
    }
    
    /// <summary>
    /// 场景上下文接口
    /// </summary>
    public interface ISceneContext
    {
        /// <summary>
        /// 当前场景名称
        /// </summary>
        string CurrentSceneName { get; }
        
        /// <summary>
        /// 场景数据
        /// </summary>
        object SceneData { get; set; }
        
        /// <summary>
        /// 获取生成点
        /// </summary>
        SpawnPoint GetSpawnPoint(int id);
        
        /// <summary>
        /// 获取指定类型的所有生成点
        /// </summary>
        IReadOnlyList<SpawnPoint> GetSpawnPoints(SpawnPointType type);
        
        /// <summary>
        /// 获取随机生成点
        /// </summary>
        SpawnPoint GetRandomSpawnPoint(SpawnPointType type);
        
        /// <summary>
        /// 注册生成点
        /// </summary>
        void RegisterSpawnPoint(SpawnPoint spawnPoint);
        
        /// <summary>
        /// 清空生成点
        /// </summary>
        void ClearSpawnPoints();
    }
}
