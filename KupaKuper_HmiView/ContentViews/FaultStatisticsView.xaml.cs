using KupaKuper_HmiView.ContentViewModes;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class FaultStatisticsView : ContentView
    {
        private readonly FaultStatisticsViewVM _viewMode;
        /// <summary>
        /// 设备运行数据
        /// </summary>
        public FaultStatisticsView()
        {
            InitializeComponent();
            BindingContext = _viewMode = new FaultStatisticsViewVM();
        }
    }
}

