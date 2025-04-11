using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_HmiView.ContentViews;

using KupaKuper_MauiControl.Controls;

namespace MauiHmiFrame_KupaKuper
{
    public partial class MainPageMode : ObservableRecipient
    {
        [ObservableProperty]
        public Dictionary<uint, ContentView> currentViews;
        [ObservableProperty]
        public string message;

        public IoView io;
        public AxisView axis;
        public CylinderView cylinder;
        public OtherView other;
        public MainPageMode()
        {
            io = new();
            axis = new();
            cylinder = new();
            other = new();
        }
        public void ChangeUpPage(SwitchableContentView switchableContentView)
        {
            var v = new Dictionary<uint,ContentView>()
            {
                {1,io },
                {2,axis },
                {3,cylinder },
                {4,other }
            };
            switchableContentView.ViewSource = v;
        }
    }
}
