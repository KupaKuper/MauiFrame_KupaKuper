using KupaKuper_HmiView.ContentViewModes;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class IoView : ContentView
    {
        private readonly IoViewVM _viewMode;

        public IoView()
        {
            InitializeComponent();
            // 延迟初始化ViewModel
            BindingContext = _viewMode = new IoViewVM();
        }
    }
}