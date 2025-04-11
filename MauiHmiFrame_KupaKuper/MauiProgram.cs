using KupaKuper_HmiView;

using LocalizationResourceManager.Maui;

using MauiHmiFrame_KupaKuper.Resources;

using Microsoft.Extensions.Logging;
using Microsoft.Maui.Platform;

using MyControl = KupaKuper_MauiControl.ControlModes;

namespace MauiHmiFrame_KupaKuper
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("iconfont.ttf", "iconfont");
                });
            builder
               .UseMauiApp<App>()
               .ConfigureKupaKuper_HmiViewLibrary(); // 调用类库的配置方法
            builder
            .UseLocalizationResourceManager(settings =>
            {
                settings.AddResource(AppResources.ResourceManager);
                settings.RestoreLatestCulture(true);
            });
            //Add Views
            builder.Services.AddSingleton<MainPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

#if WINDOWS
        builder.Services.AddTransient<IWindow>(services =>
        {
            var window = new Window
            {
                Width = 1024,
                Height = 768,
                Title = "HMI App"
            };
            return window;
        });
#endif
#if ANDROID
            Microsoft.Maui.Handlers.GraphicsViewHandler.Mapper.AppendToMapping("AndroidClipping", (handler, view) =>
            {
                // 只为滚动视图类型启用裁剪，避免影响图表控件
                if ((view.Drawable is MyControl.ScrollGraphicsDrawable ||
                    (view.Drawable is MyControl.CsvTableGraphicsDrawable))
                && handler.PlatformView is Android.Views.View androidView)
                {
                    androidView.SetClipToOutline(true);
                }

                // 对于图表控件，使用不同的裁剪设置
                if ((view is IGraphicsView gv &&
                (gv.Drawable is MyControl.BasicBarsViewMode || gv.Drawable is MyControl.BasicPieViewMode)) &&
                handler.PlatformView is Android.Views.View chartView)
                {
                    // 为图表禁用严格裁剪
                    chartView.SetClipToOutline(false);
                    // 可以设置其他属性来实现适当的裁剪行为
                }
            });
#endif

            return builder.Build();
        }
    }
}
