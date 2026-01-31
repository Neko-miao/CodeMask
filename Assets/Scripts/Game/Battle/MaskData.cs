// ================================================
// Game - 面具数据
// ================================================

using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Battle
{
    /// <summary>
    /// 面具数据
    /// </summary>
    [Serializable]
    public class MaskData
    {
        public MaskType Type;
        public string Name;
        public MaskEffectType EffectType;
        public int AttackPower;
        public string Description;
        
        /// <summary>
        /// 克制的面具类型列表
        /// </summary>
        public List<MaskType> CounterTypes;
    }
    
    /// <summary>
    /// 面具配置表
    /// </summary>
    public static class MaskConfig
    {
        private static Dictionary<MaskType, MaskData> _masks;
        
        static MaskConfig()
        {
            _masks = new Dictionary<MaskType, MaskData>
            {
                {
                    MaskType.Cat, new MaskData
                    {
                        Type = MaskType.Cat,
                        Name = "猫面具",
                        EffectType = MaskEffectType.Dodge,
                        AttackPower = 1,
                        Description = "闪避型面具，一般卡点不受攻击伤害，完美卡点不受狂暴伤害",
                        CounterTypes = new List<MaskType> { MaskType.Bear }
                    }
                },
                {
                    MaskType.Snake, new MaskData
                    {
                        Type = MaskType.Snake,
                        Name = "蛇面具",
                        EffectType = MaskEffectType.Attack,
                        AttackPower = 1,
                        Description = "攻击型面具，对猫造成闪避效果",
                        CounterTypes = new List<MaskType> { MaskType.Cat }
                    }
                },
                {
                    MaskType.Bear, new MaskData
                    {
                        Type = MaskType.Bear,
                        Name = "熊面具",
                        EffectType = MaskEffectType.Block,
                        AttackPower = 2,
                        Description = "格挡型面具，格挡成功后下一卡点变成格挡反击",
                        CounterTypes = new List<MaskType> { MaskType.Snake }
                    }
                },
                {
                    MaskType.Horse, new MaskData
                    {
                        Type = MaskType.Horse,
                        Name = "马面具",
                        EffectType = MaskEffectType.Block,
                        AttackPower = 1,
                        Description = "防守型面具，面对马时即使一般卡点也会被其格挡",
                        CounterTypes = new List<MaskType> { MaskType.Bull }
                    }
                },
                {
                    MaskType.Bull, new MaskData
                    {
                        Type = MaskType.Bull,
                        Name = "牛面具",
                        EffectType = MaskEffectType.Attack,
                        AttackPower = 2,
                        Description = "破防型面具，五次攻击后进行破防攻击",
                        CounterTypes = new List<MaskType> { MaskType.Bear }
                    }
                },
                {
                    MaskType.Whale, new MaskData
                    {
                        Type = MaskType.Whale,
                        Name = "鲸鱼面具",
                        EffectType = MaskEffectType.Heal,
                        AttackPower = 1,
                        Description = "血厚型面具，完美卡点恢复体力",
                        CounterTypes = new List<MaskType>()
                    }
                },
                {
                    MaskType.Shark, new MaskData
                    {
                        Type = MaskType.Shark,
                        Name = "鲨鱼面具",
                        EffectType = MaskEffectType.Attack,
                        AttackPower = 3,
                        Description = "高攻击力面具",
                        CounterTypes = new List<MaskType>()
                    }
                },
                {
                    MaskType.Dragon, new MaskData
                    {
                        Type = MaskType.Dragon,
                        Name = "龙面具",
                        EffectType = MaskEffectType.Attack,
                        AttackPower = 5,
                        Description = "最终Boss面具，血量和攻击都超高",
                        CounterTypes = new List<MaskType>()
                    }
                }
            };
        }
        
        public static MaskData GetMaskData(MaskType type)
        {
            return _masks.TryGetValue(type, out var data) ? data : null;
        }
        
        public static bool IsCounter(MaskType attackerMask, MaskType defenderMask)
        {
            var data = GetMaskData(attackerMask);
            return data?.CounterTypes?.Contains(defenderMask) ?? false;
        }
    }
}
