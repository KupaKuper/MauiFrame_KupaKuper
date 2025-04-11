using KupaKuper_HmiView.Resources;

using KupaKuper_MauiControl.ControlModes;

namespace KupaKuper_HmiView.ContentViewModes
{
    public partial class SystemSetViewMode : BaseViewMode
    {
        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("软件设置") ?? "软件设置"; set => throw new NotImplementedException(); }

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
