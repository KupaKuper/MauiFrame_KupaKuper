using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_HmiView.Resources;

using KupaKuper_MauiControl.ControlModes;

namespace KupaKuper_HmiView.ContentViewModes
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
        public override string ViewName { get => AppResources.ResourceManager?.GetString("登入界面") ?? "登入界面"; set => throw new NotImplementedException(); }

        public override void CloseViewVisible()
        {
            throw new NotImplementedException();
        }

        public override void OnViewVisible()
        {
            base.NowView = this.ViewIndex;
        }

        public override void UpdataView()
        {
            throw new NotImplementedException();
        }
    }
}
