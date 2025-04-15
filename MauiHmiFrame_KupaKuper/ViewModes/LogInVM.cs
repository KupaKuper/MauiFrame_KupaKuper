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
        [ObservableProperty]
        public ImageSource pictureAdr = "C:\\Users\\26060\\Pictures\\Camera Roll\\HJ.png";
        [ObservableProperty]
        public ImageSource pictureBackgroud = "C:\\Users\\26060\\Pictures\\背景2.png";

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => _viewName; set => _viewName = value; }
        private string _viewName = AppResources.ResourceManager?.GetString("登入界面") ?? "登入界面";

        public override void CloseViewVisible()
        {
            ;
        }

        public override void OnViewVisible()
        {
            NowView = ViewIndex;
        }

        public override void UpdataView()
        {
            ;
        }
    }
}
