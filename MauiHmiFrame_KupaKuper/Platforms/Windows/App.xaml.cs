using Microsoft.UI.Xaml;
using Microsoft.Maui.Handlers;

namespace MauiHmiFrame_KupaKuper.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}
