using KupaKuper_MauiControl.ControlModes;

using MauiHmiFrame_KupaKuper.Resources;

namespace MauiHmiFrame_KupaKuper.Views
{
    public partial class SystemSetViewMode : BaseViewMode
    {
        public override uint ViewIndex { get; set; }
        public override string ViewName { get => _viewName; set => _viewName = value; }
        private string _viewName = AppResources.ResourceManager?.GetString("软件设置") ?? "软件设置";

        public override void CloseViewVisible()
        {
            ;
        }

        public override void OnViewVisible()
        {
            base.NowView = this.ViewIndex;
        }

        public override void UpdataView()
        {
            ;
        }
    }
}
