using KupaKuper_HmiView.ContentViewModes;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class DailyProductionView : ContentView
    {
        private readonly DailyProductionViewVM _viewMode;

        /// <summary>
        /// 今日产能界面数据类
        /// </summary>
        public DailyProductionView()
        {
            BindingContext = _viewMode = new DailyProductionViewVM();
            InitializeComponent();
        }
    }
}

