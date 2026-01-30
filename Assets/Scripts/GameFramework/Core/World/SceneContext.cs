// ================================================
// GameFramework - 场景上下文实现
// ================================================

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GameFramework.World
{
    /// <summary>
    /// 场景上下文实现
    /// </summary>
    public class SceneContext : ISceneContext
    {
        private readonly Dictionary<int, SpawnPoint> _spawnPoints = new Dictionary<int, SpawnPoint>();
        private readonly Dictionary<SpawnPointType, List<SpawnPoint>> _spawnPointsByType = new Dictionary<SpawnPointType, List<SpawnPoint>>();
        
        private object _sceneData;
        
        public string CurrentSceneName => SceneManager.GetActiveScene().name;
        
        public object SceneData
        {
            get => _sceneData;
            set => _sceneData = value;
        }
        
        public SceneContext()
        {
            // 初始化类型列表
            foreach (SpawnPointType type in System.Enum.GetValues(typeof(SpawnPointType)))
            {
                _spawnPointsByType[type] = new List<SpawnPoint>();
            }
        }
        
        public SpawnPoint GetSpawnPoint(int id)
        {
            _spawnPoints.TryGetValue(id, out var point);
            return point;
        }
        
        public IReadOnlyList<SpawnPoint> GetSpawnPoints(SpawnPointType type)
        {
            if (_spawnPointsByType.TryGetValue(type, out var list))
            {
                return list;
            }
            return System.Array.Empty<SpawnPoint>();
        }
        
        public SpawnPoint GetRandomSpawnPoint(SpawnPointType type)
        {
            if (_spawnPointsByType.TryGetValue(type, out var list) && list.Count > 0)
            {
                return list[Random.Range(0, list.Count)];
            }
            return default;
        }
        
        public void RegisterSpawnPoint(SpawnPoint spawnPoint)
        {
            _spawnPoints[spawnPoint.Id] = spawnPoint;
            
            if (_spawnPointsByType.TryGetValue(spawnPoint.Type, out var list))
            {
                // 避免重复
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i].Id == spawnPoint.Id)
                    {
                        list[i] = spawnPoint;
                        return;
                    }
                }
                list.Add(spawnPoint);
            }
        }
        
        public void ClearSpawnPoints()
        {
            _spawnPoints.Clear();
            foreach (var list in _spawnPointsByType.Values)
            {
                list.Clear();
            }
            _sceneData = null;
        }
    }
}
