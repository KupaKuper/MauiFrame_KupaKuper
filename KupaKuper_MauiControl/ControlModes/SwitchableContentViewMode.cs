using CommunityToolkit.Mvvm.ComponentModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public partial class SwitchableContentViewMode : ObservableObject
    {
        /// <summary>
        /// 当前显示的页面
        /// </summary>
        [ObservableProperty]
        private ContentView currentView = new();
    }
}