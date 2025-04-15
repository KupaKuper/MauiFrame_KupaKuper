using LocalizationResourceManager.Maui;

using System.Globalization;

namespace MauiHmiFrame_KupaKuper.Views
{
    public partial class SystemSetView : ContentView
    {
        private readonly ILocalizationResourceManager resourceManager;
        private readonly SystemSetViewMode _viewMode;
        public SystemSetView(ILocalizationResourceManager resourceManager)
        {
            InitializeComponent();
            BindingContext = _viewMode = new();
            this.resourceManager = resourceManager;
            ChangeBackColor.IsToggled = Application.Current?.RequestedTheme == AppTheme.Dark ? true : false;
            check_En.IsChecked = resourceManager.CurrentCulture.Name == "ch" ? false : true;
            check_En.IsEnabled = check_En.IsChecked ? false : true;
            check_Zh.IsChecked = resourceManager.CurrentCulture.Name == "ch" ? true : false;
            check_Zh.IsEnabled = check_Zh.IsChecked ? false : true;
        }

        private void check_Zh_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                check_En.IsChecked = !e.Value;
                check_Zh.IsEnabled = false;
                check_En.IsEnabled = true;
                resourceManager.CurrentCulture = new CultureInfo("ch");
            }
        }

        private void check_En_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (e.Value)
            {
                check_Zh.IsChecked = !e.Value;
                check_En.IsEnabled = false;
                check_Zh.IsEnabled = true;
                resourceManager.CurrentCulture = new CultureInfo("en-US");
            }
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = e.Value == true ? AppTheme.Dark : AppTheme.Light;
            }
        }
    }
}

