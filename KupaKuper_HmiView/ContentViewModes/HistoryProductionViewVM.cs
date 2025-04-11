using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using KupaKuper_HmiView.Resources;

using KupaKuper_IO.HelpVoid;

using KupaKuper_MauiControl.ControlModes;

using Color = Microsoft.Maui.Graphics.Color;
using Colors = Microsoft.Maui.Graphics.Colors;

namespace KupaKuper_HmiView.ContentViewModes
{
    /// <summary>
    /// 历史产能数据界面
    /// </summary>
    partial class HistoryProductionViewVM : BaseViewMode
    {
        [ObservableProperty]
        private List<List<double>> dataSeries = new() { new(24), new(24) };
        [ObservableProperty]
        private List<string> xLabels;
        [ObservableProperty]
        private List<Color> seriesColors = new() { Colors.DodgerBlue, Colors.OrangeRed };
        [ObservableProperty]
        private List<string> legendLabels = new() { "Ok", "NG" };
        [ObservableProperty]
        private Double maxValue = 1000;
        [ObservableProperty]
        private DateTime selectedDate = DateTime.Now;
        [ObservableProperty]
        private string monthIndx = DateTime.Now.Month+"月";
        private DateTime SelectedDateMonth = DateTime.Now;

        public string BasePath { get; private set; } = @"./";
        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("历史生产") ?? "历史生产"; set => throw new NotImplementedException(); }


        public HistoryProductionViewVM()
        {
            LoadDateData(DateTime.Now);
        }
        partial void OnSelectedDateChanged(DateTime value)
        {
            LoadData();
        }

        public void LoadData()
        {
            LoadDateData(SelectedDate);
        }

        private void LoadDateData(DateTime date)
        {
            string DataAdr = Path.Combine(Device.dataConfig.DailyProductAdr, $"{date.ToString("yyyy_MM")}月", $"ProductData_{date.ToString("yyyy_MM_dd")}.csv");
            if (File.Exists(DataAdr))
            {
                var data_time = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "时间").Result;
                var data_OK = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "OK").Result;
                var data_NG = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "NG").Result; // 修正为 "NG"
                DataSeries = new List<List<double>>
                {
                StringToData.StringTo<double>(data_OK),  // 第一个数据系列
                StringToData.StringTo<double>(data_NG)  // 第二个数据系列
                };
                MaxValue = 1000;
                XLabels = data_time;
                SeriesColors = new List<Color>
                {
                Colors.DodgerBlue,
                Colors.OrangeRed
                };
                LegendLabels = new() { "Ok", "NG" };
            }
            else if (IsDebug)
            {
                // 随机生成测试数据
                FillTestData();
            }
        }

        private void LoadMonthlyData(DateTime Month)
        {
            string DataAdr = Path.Combine(Device.dataConfig.DailyProductAdr, $"{Month.ToString("yyyy_MM")}月", $"ProductData_{Month.ToString("yyyy_MM")}.csv");
            if (File.Exists(DataAdr))
            {
                var data_time = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "时间").Result;
                var data_OK = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "OK").Result;
                var data_NG = CsvFileHelper.ReadSpecificColumnAsync(DataAdr, 0, "NG").Result; // 修正为 "NG"
                DataSeries = new List<List<double>>
                {
                StringToData.StringTo<double>(data_OK),  // 第一个数据系列
                StringToData.StringTo<double>(data_NG)  // 第二个数据系列
                };
                MaxValue = 24000;
                XLabels = data_time;
                SeriesColors = new List<Color>
                {
                Colors.DodgerBlue,
                Colors.OrangeRed
                };
                LegendLabels = new() { "Ok", "NG" };
            }
            else if (IsDebug)
            {
                // 随机生成测试数据
                FillTestMonthData(Month);
            }
        }

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
            MaxValue = 1000;
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
        /// 生成随机数据
        /// </summary>
        private void FillTestMonthData(DateTime date)
        {
            var random = new Random();
            var month = date.Month;
            LegendLabels = new() { "Ok", "NG" };
            List<double> doubles1 = new();
            List<double> doubles2 = new();
            List<string> strings = new();
            for (int i = 1; i < 31; i++)
            {
                var r = random.Next(15000, 24000);
                doubles1.Add(r);
                doubles2.Add(24000 - r);
                strings.Add($"{month:D2}月{i:D2}号");
            }
            MaxValue = 24000;
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

        // 新增方法，用于处理月份切换
        private void ChangeMonth(int offset)
        {
            SelectedDateMonth = SelectedDateMonth.AddMonths(offset);
            MonthIndx = SelectedDateMonth.Month + "月";
            OnPropertyChanged(nameof(MonthIndx)); // 通知视图更新月份显示
            LoadMonthlyData(SelectedDateMonth); ; // 触发数据加载
        }

        /// <summary>
        /// 上一个月
        /// </summary>
        [RelayCommand]
        private void LastMonth()
        {
            ChangeMonth(-1);
        }

        /// <summary>
        /// 下一个月
        /// </summary>
        [RelayCommand]
        private void NextMonth()
        {
            ChangeMonth(1);
        }

        public override void UpdataView()
        {
            throw new NotImplementedException();
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
