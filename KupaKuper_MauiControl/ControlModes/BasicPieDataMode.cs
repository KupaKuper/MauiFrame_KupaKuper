using CommunityToolkit.Mvvm.ComponentModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public partial class BasicPieDataMode : ObservableObject
    {
        [ObservableProperty]
        private double value;

        [ObservableProperty]
        private string name;

        [ObservableProperty]
        private Color color;
    }
}