// ================================================
// GameFramework - 标签组件 (纯数据)
// ================================================

using System;
using System.Collections.Generic;

namespace GameFramework.Entity
{
    /// <summary>
    /// 标签组件 - 存储字符串标签用于更灵活的分类
    /// </summary>
    [Serializable]
    public class TagComp : EntityComp<TagComp>
    {
        /// <summary>
        /// 主标签
        /// </summary>
        public string PrimaryTag;
        
        /// <summary>
        /// 附加标签列表
        /// </summary>
        public List<string> Tags = new List<string>();
        
        /// <summary>
        /// 分组ID
        /// </summary>
        public int GroupId;
        
        /// <summary>
        /// 队伍/阵营ID
        /// </summary>
        public int TeamId;
        
        /// <summary>
        /// 优先级
        /// </summary>
        public int Priority;
        
        /// <summary>
        /// 是否有标签
        /// </summary>
        public bool HasTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return false;
            
            if (PrimaryTag == tag) return true;
            return Tags.Contains(tag);
        }
        
        /// <summary>
        /// 添加标签
        /// </summary>
        public void AddTag(string tag)
        {
            if (string.IsNullOrEmpty(tag)) return;
            if (!Tags.Contains(tag))
            {
                Tags.Add(tag);
            }
        }
        
        /// <summary>
        /// 移除标签
        /// </summary>
        public bool RemoveTag(string tag)
        {
            return Tags.Remove(tag);
        }
        
        public override void Reset()
        {
            PrimaryTag = null;
            Tags.Clear();
            GroupId = 0;
            TeamId = 0;
            Priority = 0;
        }
        
        public override IEntityComp Clone()
        {
            var clone = new TagComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                PrimaryTag = PrimaryTag,
                GroupId = GroupId,
                TeamId = TeamId,
                Priority = Priority
            };
            clone.Tags.AddRange(Tags);
            return clone;
        }
    }
}

