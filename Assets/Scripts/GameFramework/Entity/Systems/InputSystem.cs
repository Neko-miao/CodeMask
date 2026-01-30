// ================================================
// GameFramework - 输入系统
// ================================================

using UnityEngine;

namespace GameFramework.Entity
{
    /// <summary>
    /// 输入系统 - 将玩家输入同步到实体的InputComp和VelocityComp
    /// 需要: InputComp
    /// </summary>
    public class InputSystem : EntitySystem
    {
        public override string SystemName => "InputSystem";
        public override int Priority => 10;
        public override SystemPhase Phase => SystemPhase.EarlyUpdate;
        
        /// <summary>
        /// 被控制的实体ID (-1表示无)
        /// </summary>
        public int ControlledEntityId { get; private set; } = -1;
        
        protected override void ConfigureRequirements()
        {
            Require<InputComp>();
            RequireTag(EntityTag.Player);
        }
        
        public override void ProcessEntity(IEntity entity, float deltaTime)
        {
            // 只处理被控制的实体
            if (entity.Id != ControlledEntityId) return;
            
            var input = entity.GetComp<InputComp>();
            if (input == null || !input.IsEnabled) return;
            
            // 读取输入
            ReadInput(input);
            
            // 同步到VelocityComp
            SyncToVelocity(entity, input);
        }
        
        protected virtual void ReadInput(InputComp input)
        {
            // 移动输入
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            input.MoveDirection = new Vector2(h, v).normalized;
            
            // 鼠标位置
            input.MousePosition = Input.mousePosition;
            
            // 按钮输入
            input.Primary = Input.GetButton("Fire1");
            input.Secondary = Input.GetButton("Fire2");
            input.Jump = Input.GetButton("Jump");
            input.Sprint = Input.GetKey(KeyCode.LeftShift);
            input.Crouch = Input.GetKey(KeyCode.LeftControl);
            input.Interact = Input.GetKeyDown(KeyCode.E);
            input.Reload = Input.GetKeyDown(KeyCode.R);
            input.Skill1 = Input.GetKeyDown(KeyCode.Alpha1);
            input.Skill2 = Input.GetKeyDown(KeyCode.Alpha2);
            input.Skill3 = Input.GetKeyDown(KeyCode.Alpha3);
            input.Skill4 = Input.GetKeyDown(KeyCode.Alpha4);
            input.Menu = Input.GetKeyDown(KeyCode.Escape);
        }
        
        protected virtual void SyncToVelocity(IEntity entity, InputComp input)
        {
            var velocity = entity.GetComp<VelocityComp>();
            if (velocity == null) return;
            
            // 转换2D输入到3D移动方向
            velocity.MoveInput = new Vector3(input.MoveDirection.x, 0, input.MoveDirection.y);
        }
        
        #region Public Methods
        
        /// <summary>
        /// 设置被控制的实体
        /// </summary>
        public void SetControlledEntity(int entityId)
        {
            ControlledEntityId = entityId;
        }
        
        /// <summary>
        /// 设置被控制的实体
        /// </summary>
        public void SetControlledEntity(IEntity entity)
        {
            ControlledEntityId = entity?.Id ?? -1;
        }
        
        /// <summary>
        /// 清除控制
        /// </summary>
        public void ClearControl()
        {
            ControlledEntityId = -1;
        }
        
        #endregion
    }
}

