using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Views;

using AndroidX.Core.View;

namespace MauiHmiFrame_KupaKuper
{
    [Activity(Theme = "@style/AppTheme.NoActionBar", MainLauncher = true, ScreenOrientation = ScreenOrientation.Landscape, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // 设置全屏显示
            SetFullScreen();
        }

        protected override void OnResume()
        {
            base.OnResume();

            // 在恢复活动时重新应用全屏设置
            SetFullScreen();
        }

        private void SetFullScreen()
        {
            try
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    // Android 11 (API 30)及以上版本使用WindowInsetsController
                    if (Window != null)
                    {
                        Window.SetDecorFitsSystemWindows(false);
                        var controller = Window.InsetsController;
                        if (controller != null)
                        {
                            // 隐藏状态栏和导航栏
                            controller.Hide(WindowInsets.Type.StatusBars() | WindowInsets.Type.NavigationBars());
                            controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
                        }
                    }
                }
                else if (Build.VERSION.SdkInt >= BuildVersionCodes.P)
                {
                    // Android 9 (API 28)及以上版本
                    if (Window != null && Window.Attributes != null)
                    {
                        Window.Attributes.LayoutInDisplayCutoutMode = LayoutInDisplayCutoutMode.ShortEdges;
                    }

                    // 使用SystemUiFlags
                    if (Window?.DecorView != null)
                    {
                        var uiOptions = (int)Window.DecorView.SystemUiFlags;
                        uiOptions |= (int)SystemUiFlags.LayoutStable;
                        uiOptions |= (int)SystemUiFlags.LayoutFullscreen;
                        uiOptions |= (int)SystemUiFlags.HideNavigation;
                        uiOptions |= (int)SystemUiFlags.Fullscreen;
                        uiOptions |= (int)SystemUiFlags.ImmersiveSticky;

                        Window.DecorView.SystemUiFlags = (SystemUiFlags)uiOptions;
                    }
                }
                else
                {
                    // 较旧版本的Android
                    if (Window != null)
                    {
                        Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
                        Window.SetFlags(WindowManagerFlags.LayoutNoLimits, WindowManagerFlags.LayoutNoLimits);
                    }
                }

                // 设置状态栏和导航栏为透明（适用于所有版本）
                if (Window != null)
                {
                    Window.SetStatusBarColor(Android.Graphics.Color.Transparent);
                    Window.SetNavigationBarColor(Android.Graphics.Color.Transparent);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"设置全屏模式错误: {ex.Message}");

                // 出错时使用最基本的全屏方法
                try
                {
                    if (Window != null)
                    {
                        Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
                    }
                }
                catch
                {
                    // 忽略任何额外错误
                }
            }
        }

        // 处理系统UI可见性变化，确保保持全屏
        public override void OnWindowFocusChanged(bool hasFocus)
        {
            base.OnWindowFocusChanged(hasFocus);
            if (hasFocus)
            {
                SetFullScreen();
            }
        }
    }
}
