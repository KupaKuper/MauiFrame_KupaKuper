using KupaKuper_HmiView.ContentViewModes;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class HistoryProductionView : ContentView
    {
        private readonly HistoryProductionViewVM _viewMode;

        /// <summary>
        /// 历史产能数据界面
        /// </summary>
        public HistoryProductionView()
        {
            InitializeComponent();
            BindingContext = _viewMode = new();
        }

        private void OnMonthViewToggled(object sender, ToggledEventArgs e)
        {
            _viewMode.LoadData();
        }
    }
}

