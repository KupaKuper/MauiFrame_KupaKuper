using Microsoft.UI.Xaml;
using Microsoft.Maui.Handlers;

namespace MauiHmiFrame_KupaKuper.WinUI
{
    public partial class App : MauiWinUIApplication
    {
        public App()
        {
            this.InitializeComponent();

            // ׷�Ӵ���ӳ���߼������� DPI �仯
            WindowHandler.Mapper.AppendToMapping(nameof(IWindow), (handler, view) =>
            {
                // ���Խ� PlatformView ת��Ϊ Windows ƽ̨��ԭ������
                var nativeWindow = handler.PlatformView as Microsoft.UI.Xaml.Window;
                if (nativeWindow != null)
                {
                    // ���� DPI �仯�¼�
                    nativeWindow.DpiChanged += (sender, args) =>
                    {
                        // ǿ�ƽ����ڵĹ�դ�����ű�������Ϊ 100%
                        nativeWindow.RasterizationScale = 1.0;
                    };
                    // ��ʼ��ʱҲ����Ϊ 100% ����
                    nativeWindow.RasterizationScale = 1.0;
                }
            });
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
    }
}