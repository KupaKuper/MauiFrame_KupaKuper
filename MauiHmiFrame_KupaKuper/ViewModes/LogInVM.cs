using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_MauiControl.ControlModes;

using MauiHmiFrame_KupaKuper.Resources;

namespace MauiHmiFrame_KupaKuper.ViewModes
{
    public partial class LogInVM : BaseViewMode
    {
        [ObservableProperty]
        public string password = "1120";
        [ObservableProperty]
        public string titlText = "";

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => _viewName; set => _viewName = value; }
        private string _viewName = AppResources.ResourceManager?.GetString("登入界面") ?? "登入界面";

        public override void CloseViewVisible()
        {
            
        }

        public override void OnViewVisible()
        {
            NowView = ViewIndex;
        }

        public override void UpdataView()
        {
            
        }
    }
}
