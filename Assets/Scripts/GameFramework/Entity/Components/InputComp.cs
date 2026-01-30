// ================================================
// GameFramework - 输入组件 (纯数据)
// ================================================

using System;
using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 输入组件 - 存储实体的输入状态
    /// </summary>
    [Serializable]
    public class InputComp : EntityComp<InputComp>
    {
        /// <summary>
        /// 移动方向 (归一化)
        /// </summary>
        public Vector2 MoveDirection;
        
        /// <summary>
        /// 视角方向 (鼠标/摇杆)
        /// </summary>
        public Vector2 LookDirection;
        
        /// <summary>
        /// 鼠标位置
        /// </summary>
        public Vector2 MousePosition;
        
        /// <summary>
        /// 世界空间中的瞄准点
        /// </summary>
        public Vector3 AimPoint;
        
        /// <summary>
        /// 主按钮 (攻击/确认)
        /// </summary>
        public bool Primary;
        
        /// <summary>
        /// 副按钮 (副武器/取消)
        /// </summary>
        public bool Secondary;
        
        /// <summary>
        /// 跳跃
        /// </summary>
        public bool Jump;
        
        /// <summary>
        /// 冲刺
        /// </summary>
        public bool Sprint;
        
        /// <summary>
        /// 蹲下
        /// </summary>
        public bool Crouch;
        
        /// <summary>
        /// 交互
        /// </summary>
        public bool Interact;
        
        /// <summary>
        /// 重装/切换
        /// </summary>
        public bool Reload;
        
        /// <summary>
        /// 技能 (1-4)
        /// </summary>
        public bool Skill1;
        public bool Skill2;
        public bool Skill3;
        public bool Skill4;
        
        /// <summary>
        /// 菜单/暂停
        /// </summary>
        public bool Menu;
        
        /// <summary>
        /// 是否有移动输入
        /// </summary>
        public bool HasMoveInput => MoveDirection.sqrMagnitude > 0.01f;
        
        /// <summary>
        /// 移动输入大小
        /// </summary>
        public float MoveMagnitude => MoveDirection.magnitude;
        
        public override void Reset()
        {
            MoveDirection = Vector2.zero;
            LookDirection = Vector2.zero;
            MousePosition = Vector2.zero;
            AimPoint = Vector3.zero;
            Primary = false;
            Secondary = false;
            Jump = false;
            Sprint = false;
            Crouch = false;
            Interact = false;
            Reload = false;
            Skill1 = false;
            Skill2 = false;
            Skill3 = false;
            Skill4 = false;
            Menu = false;
        }
        
        public override IEntityComp Clone()
        {
            return new InputComp
            {
                EntityId = EntityId,
                IsEnabled = IsEnabled,
                MoveDirection = MoveDirection,
                LookDirection = LookDirection,
                MousePosition = MousePosition,
                AimPoint = AimPoint,
                Primary = Primary,
                Secondary = Secondary,
                Jump = Jump,
                Sprint = Sprint,
                Crouch = Crouch,
                Interact = Interact,
                Reload = Reload,
                Skill1 = Skill1,
                Skill2 = Skill2,
                Skill3 = Skill3,
                Skill4 = Skill4,
                Menu = Menu
            };
        }
        
        /// <summary>
        /// 清除所有按钮状态
        /// </summary>
        public void ClearButtons()
        {
            Primary = false;
            Secondary = false;
            Jump = false;
            Sprint = false;
            Crouch = false;
            Interact = false;
            Reload = false;
            Skill1 = false;
            Skill2 = false;
            Skill3 = false;
            Skill4 = false;
            Menu = false;
        }
    }
}

