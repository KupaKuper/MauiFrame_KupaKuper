using KupaKuper_MauiControl.Resources;

using LocalizationResourceManager.Maui;

using System.Reflection;

namespace KupaKuper_MauiControl
{
    public static class LibraryStartup
    {
        public static MauiAppBuilder ConfigureHmiPageFrameLibrary(this MauiAppBuilder builder)
        {
            // 获取类库的程序集
            var assembly = typeof(LibraryStartup).GetTypeInfo().Assembly;

            // 添加本地化资源管理器
            builder.UseLocalizationResourceManager(settings =>
            {
                settings.AddResource(AppResources.ResourceManager);
                settings.RestoreLatestCulture(true);
            });

            return builder;
        }
    }
}
