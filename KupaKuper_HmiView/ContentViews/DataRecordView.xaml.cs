using KupaKuper_HmiView.ContentViewModes;

using KupaKuper_MauiControl.Controls;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class DataRecordView : ContentView
    {
        private readonly DataRecordViewVM _viewMode;
        /// <summary>
        /// 产品数据显示
        /// </summary>
        public DataRecordView()
        {
            InitializeComponent();
            BindingContext = _viewMode = new();

            // 添加加载完成事件处理
            Loaded += OnViewLoaded;
        }

        private void OnViewLoaded(object? sender, EventArgs e)
        {
            // 延迟一帧执行，确保绑定已完成
            Dispatcher.Dispatch(() => {
                // 强制刷新表格数据
                if (FindByName("csvTable") is CsvTable csvTable)
                {
                    csvTable.ForceReloadData();
                }
            });
        }

        private void CsvTable_FileSelected(string filePath)
        {
            _viewMode.ChangeCsvAdr(filePath);
        }
    }
}

