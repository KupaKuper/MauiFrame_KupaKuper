using Microsoft.UI.Xaml;
using Microsoft.Maui.Handlers;

namespace MauiHmiFrame_KupaKuper.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();

            // 追加窗口映射逻辑，处理 DPI 变化
            WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                // 尝试将 PlatformView 转换为 Windows 平台的原生窗口
                var nativeWindow = handler.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow != null)
                {
                    // 监听 DPI 变化事件
                    nativeWindow.DpiChanged += (sender, args) =>
                    {
                        // 强制将窗口的光栅化缩放比例设置为 100%
                        nativeWindow.RasterizationScale = 1.0;
                    };
                    // 初始化时也设置为 100% 缩放
                    nativeWindow.RasterizationScale = 1.0;
                }
            });
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}