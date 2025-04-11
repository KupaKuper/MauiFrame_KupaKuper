using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_HmiView.Resources;

using KupaKuper_IO.HelpVoid;

using KupaKuper_MauiControl.ControlModes;

using System.Diagnostics;

namespace KupaKuper_HmiView.ContentViewModes
{
    /// <summary>
    /// 今日产能数据界面
    /// </summary>
    public partial class DailyProductionViewVM : BaseViewMode
    {
        // 使用示例
        [ObservableProperty]
        List<List<double>> dataSeries;
        [ObservableProperty]
        List<string> xLabels;
        [ObservableProperty]
        List<Color> seriesColors;
        [ObservableProperty]
        List<string> legendLabels;

        private FileHelp fileHelp = new();

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("今日生产") ?? "今日生产"; set => throw new NotImplementedException(); }

        public DailyProductionViewVM()
        {
            UpdataView();
        }
        /// <summary>
        /// 文件更改触发的执行方法
        /// </summary>
        /// <param name="filePath"></param>
        private void FileHelp_FileDispose(string filePath)
        {
            Debug.WriteLine($"文件已更改: {filePath}");
            // 处理文件变更
            LoadData(filePath);
        }
        /// <summary>
        /// 生成随机数据
        /// </summary>
        private void FillTestData()
        {
            var random = new Random();
            LegendLabels = new() { "Ok", "NG" };
            List<double> doubles1 = new();
            List<double> doubles2 = new();
            List<string> strings = new();
            for (int i = 0; i < 24; i++)
            {
                var r = random.Next(500, 1000);
                doubles1.Add(r);
                doubles2.Add(1000 - r);
                strings.Add(i == 23 ? $"{i:D2}:00-00:00" : $"{i:D2}:00-{(i + 1):D2}:00");
            }
            DataSeries = new List<List<double>>
            {
                doubles1,  // 第一个数据系列
                doubles2   // 第二个数据系列
            };
            XLabels = strings;
            SeriesColors = new List<Color>
            {
                Colors.DodgerBlue,
                Colors.OrangeRed
            };
        }

        /// <summary>
        /// 读取本地生产数据
        /// </summary>
        /// <param name="DataAdr"></param>
        private void LoadData(string DataAdr)
        {
            if (!File.Exists(DataAdr)) return;
            var data_time = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "时间").Result;
            var data_OK = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "OK").Result;
            var data_NG = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "NG").Result;

            DataSeries = new List<List<double>>
            {
                StringToData.StringTo<double>(data_OK),  // 第一个数据系列
                StringToData.StringTo<double>(data_NG)  // 第二个数据系列
            };
            XLabels = data_time;
            SeriesColors = new List<Color>
            {
                Colors.DodgerBlue,
                Colors.OrangeRed
            };
            LegendLabels = new() { "Ok", "NG" };
        }

        private async void DataWatch()
        {
            if (NowView != ViewIndex) return;
            var data = DateTime.Now;
            string DataAdr = Path.Combine(Device.dataConfig.DailyProductAdr, $"{data.ToString("yyyy_MM")}月", $"ProductData_{data.ToString("yyyy_MM_dd")}.csv");
            if (!File.Exists(DataAdr)) return;
            if (!FileChangeHelp.FileChanged(DataAdr)) return;
            await fileHelp.FileDisposeTask(DataAdr);
        }

        public override void UpdataView()
        {
            if (!IsDebug)
            {
                fileHelp.FileDispose += FileHelp_FileDispose;
                // 加载本地数据
                DataWatch();
            }
            else
            {
                FillTestData();
            }
        }

        public override void OnViewVisible()
        {
            base.NowView = this.ViewIndex;
        }

        public override void CloseViewVisible()
        {
            throw new NotImplementedException();
        }
    }
}
