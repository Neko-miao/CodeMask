// ================================================
// Game - 主菜单数据模型
// ================================================

using GameFramework.UI;

namespace Game.UI
{
    /// <summary>
    /// 主菜单数据
    /// </summary>
    public class MainMenuData
    {
        public bool HasSaveData { get; set; }
        public bool SettingsEnabled { get; set; }
        public bool IsSettingsPanelOpen { get; set; }
    }
    
    /// <summary>
    /// 主菜单数据模型
    /// </summary>
    public class MainMenuModel : UIModelBase<MainMenuData>
    {
        public bool HasSaveData => Data?.HasSaveData ?? false;
        public bool SettingsEnabled => Data?.SettingsEnabled ?? false;
        public bool IsSettingsPanelOpen => Data?.IsSettingsPanelOpen ?? false;
        
        public override void Initialize()
        {
            SetData(new MainMenuData
            {
                HasSaveData = CheckSaveData(),
                SettingsEnabled = false,
                IsSettingsPanelOpen = false
            });
        }
        
        public void ToggleSettingsPanel()
        {
            if (Data == null) return;
            Data.IsSettingsPanelOpen = !Data.IsSettingsPanelOpen;
            NotifyDataChanged();
        }
        
        public void SetSettingsPanelOpen(bool isOpen)
        {
            if (Data == null) return;
            Data.IsSettingsPanelOpen = isOpen;
            NotifyDataChanged();
        }
        
        public override void Clear()
        {
            base.Clear();
        }
        
        private bool CheckSaveData()
        {
            return UnityEngine.PlayerPrefs.HasKey("SaveData");
        }
    }
}
