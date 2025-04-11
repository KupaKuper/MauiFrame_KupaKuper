using LocalizationResourceManager.Maui;
using System.Diagnostics;
using KupaKuper_HmiView.ContentViewModes;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class VisionImangView : ContentView
    {
        private readonly VisionImangViewVM _viewMode;

        private Image imageControl;

        public VisionImangView()
        {
            InitializeComponent();
            BindingContext = _viewMode = new();

            imageControl = this.FindByName<Image>("ImageControl");
        }

        private void OnViewLoaded(object sender, EventArgs e)
        {
            try
            {
                // 可以在这里添加其他初始化逻辑
                Debug.WriteLine("View loaded successfully");

            }
            catch (Exception ex)
            {
                Debug.WriteLine($"OnViewLoaded error: {ex.Message}");
            }
        }
    }
}

