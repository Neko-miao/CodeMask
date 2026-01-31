// ================================================
// MaskSystem - 面具定义（独立模块，与Battle系统解耦）
// ================================================

using System;
using System.Collections.Generic;

namespace Game.MaskSystem
{
    /// <summary>
    /// 面具类型枚举
    /// </summary>
    public enum MaskType
    {
        None = 0,
        Cat = 1,      // 猫 - 闪避型
        Snake = 2,    // 蛇 - 攻击型
        Bear = 3,     // 熊 - 格挡型
        Horse = 4,    // 马 - 防守型
        Bull = 5,     // 牛 - 破防型
        Whale = 6,    // 鲸鱼 - 血厚
        Shark = 7,    // 鲨鱼 - 高攻击
        Dragon = 8    // 龙 - 最终Boss
    }

    /// <summary>
    /// 面具效果类型
    /// </summary>
    public enum MaskEffectType
    {
        Attack,  // 攻击 - 克制时双倍伤害
        Dodge,   // 闪避 - 不受普通攻击伤害
        Block,   // 格挡 - 格挡后反击
        Heal     // 回血 - 完美时回复体力
    }

    /// <summary>
    /// 面具定义数据
    /// </summary>
    [Serializable]
    public class MaskDefinition
    {
        public MaskType Type;
        public string Name;
        public MaskEffectType EffectType;
        public int AttackPower;
        public string Description;
        public List<MaskType> CounterTypes;

        public MaskDefinition(MaskType type, string name, MaskEffectType effectType, int attackPower, string description, List<MaskType> counterTypes = null)
        {
            Type = type;
            Name = name;
            EffectType = effectType;
            AttackPower = attackPower;
            Description = description;
            CounterTypes = counterTypes ?? new List<MaskType>();
        }
    }

    /// <summary>
    /// 面具配置管理器 - 静态配置表
    /// </summary>
    public static class MaskRegistry
    {
        private static Dictionary<MaskType, MaskDefinition> _masks;

        static MaskRegistry()
        {
            _masks = new Dictionary<MaskType, MaskDefinition>
            {
                {
                    MaskType.Cat, new MaskDefinition(
                        MaskType.Cat,
                        "猫面具",
                        MaskEffectType.Dodge,
                        1,
                        "闪避型面具，一般卡点不受攻击伤害",
                        new List<MaskType> { MaskType.Bear }
                    )
                },
                {
                    MaskType.Snake, new MaskDefinition(
                        MaskType.Snake,
                        "蛇面具",
                        MaskEffectType.Attack,
                        1,
                        "攻击型面具，对猫造成闪避效果",
                        new List<MaskType> { MaskType.Cat }
                    )
                },
                {
                    MaskType.Bear, new MaskDefinition(
                        MaskType.Bear,
                        "熊面具",
                        MaskEffectType.Block,
                        2,
                        "格挡型面具，格挡成功后反击",
                        new List<MaskType> { MaskType.Snake }
                    )
                },
                {
                    MaskType.Horse, new MaskDefinition(
                        MaskType.Horse,
                        "马面具",
                        MaskEffectType.Block,
                        1,
                        "防守型面具，面对马时即使一般卡点也会被格挡",
                        new List<MaskType> { MaskType.Bull }
                    )
                },
                {
                    MaskType.Bull, new MaskDefinition(
                        MaskType.Bull,
                        "牛面具",
                        MaskEffectType.Attack,
                        2,
                        "破防型面具，五次攻击后进行破防攻击",
                        new List<MaskType> { MaskType.Bear }
                    )
                },
                {
                    MaskType.Whale, new MaskDefinition(
                        MaskType.Whale,
                        "鲸鱼面具",
                        MaskEffectType.Heal,
                        1,
                        "血厚型面具，完美卡点恢复体力",
                        new List<MaskType>()
                    )
                },
                {
                    MaskType.Shark, new MaskDefinition(
                        MaskType.Shark,
                        "鲨鱼面具",
                        MaskEffectType.Attack,
                        3,
                        "高攻击力面具",
                        new List<MaskType>()
                    )
                },
                {
                    MaskType.Dragon, new MaskDefinition(
                        MaskType.Dragon,
                        "龙面具",
                        MaskEffectType.Attack,
                        5,
                        "最终Boss面具，血量和攻击都超高",
                        new List<MaskType>()
                    )
                }
            };
        }

        /// <summary>
        /// 获取面具定义
        /// </summary>
        public static MaskDefinition GetMask(MaskType type)
        {
            return _masks.TryGetValue(type, out var data) ? data : null;
        }

        /// <summary>
        /// 检查是否克制
        /// </summary>
        public static bool IsCounter(MaskType attacker, MaskType defender)
        {
            var data = GetMask(attacker);
            return data?.CounterTypes?.Contains(defender) ?? false;
        }

        /// <summary>
        /// 获取所有面具类型
        /// </summary>
        public static IEnumerable<MaskType> GetAllMaskTypes()
        {
            return _masks.Keys;
        }
    }
}

