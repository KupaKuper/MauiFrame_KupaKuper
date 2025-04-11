using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_HmiView.HelpVoid;
using KupaKuper_HmiView.Resources;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.ControlModes;

namespace KupaKuper_HmiView.ContentViewModes
{
    /// <summary>
    /// 设备运行数据界面
    /// </summary>
    partial class FaultStatisticsViewVM : BaseViewMode
    {
        [ObservableProperty]
        private List<BasicPieDataMode> dataSeries = new();
        [ObservableProperty]
        private List<BasicPieDataMode> dataTimeSeries = new();

        private DateTime LastUpDataTime = DateTime.Now;

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("设备状态") ?? "设备状态"; set => throw new NotImplementedException(); }

        public FaultStatisticsViewVM()
        {
            UpdataView();
        }
        /// <summary>
        /// 获取设备数据
        /// </summary>
        private void UpData()
        {
            Task.Run(() => 
            {
                if (NowView != ViewIndex) return;
                if (DateTime.Now < LastUpDataTime.AddMinutes(1)) return;
                LastUpDataTime = DateTime.Now;
                var DataConfig = Device.dataConfig;
                var colors = new List<Color>
                {
                    Colors.DodgerBlue,
                    Colors.DarkOrange,
                    Colors.GreenYellow,
                    Colors.Gold,
                    Colors.Purple,
                    Colors.LimeGreen,
                    Colors.OrangeRed
                };
                try
                {
                    double _RunningTime = (double)PlcClient.ReadClient.Read(DataConfig.RunningTime.PlcVarAddress);
                    double _PauseTime = (double)PlcClient.ReadClient.Read(DataConfig.PauseTime.PlcVarAddress);
                    double _DownTime = (double)PlcClient.ReadClient.Read(DataConfig.DownTime.PlcVarAddress);
                    double _AlarmTime = (double)PlcClient.ReadClient.Read(DataConfig.AlarmTime.PlcVarAddress);
                    var _DataTimeSeries = new List<BasicPieDataMode>()
                {
                    new()
                    {
                        Value=_RunningTime,
                        Name="设备运行时间",
                        Color=colors[0]
                    },
                    new()
                    {
                        Value=_PauseTime,
                        Name="设备暂停时间",
                        Color=colors[1]
                    },
                    new()
                    {
                        Value=_DownTime,
                        Name="设备待料时间",
                        Color=colors[2]
                    },
                    new()
                    {
                        Value=_AlarmTime,
                        Name="设备报警时间",
                        Color=colors[3]
                    }
                };
                    DataTimeSeries = _DataTimeSeries;
                }
                catch
                {
                    PlcVarSend.ShowMessageErr($"{DataConfig.RunningTime.PlcVarName},{DataConfig.PauseTime.PlcVarName},{DataConfig.DownTime.PlcVarName},{DataConfig.AlarmTime.PlcVarName}", "读取失败,检查类型和变量配置");
                }
                try
                {
                    double _ProductionNumber = (double)PlcClient.ReadClient.Read(DataConfig.ProductionNumber.PlcVarAddress);
                    double _NgNumber = (double)PlcClient.ReadClient.Read(DataConfig.NgNumber.PlcVarAddress);
                    var _DataSeries = new List<BasicPieDataMode>()
                {
                    new()
                    {
                        Value=_ProductionNumber-_NgNumber,
                        Name="OK数量",
                        Color=colors[5]
                    },
                    new()
                    {
                        Value=_NgNumber,
                        Name="NG数量",
                        Color=colors[6]
                    }
                };
                    DataSeries = _DataSeries;
                }
                catch
                {
                    PlcVarSend.ShowMessageErr($"{DataConfig.ProductionNumber.PlcVarName},{DataConfig.NgNumber.PlcVarName}", "读取失败,检查类型和变量配置");
                }
            });
        }
        /// <summary>
        /// 生成随机数据
        /// </summary>
        public void FillTestData()
        {
            var _DataSeries = new List<BasicPieDataMode>();
            var _DataTimeSeries = new List<BasicPieDataMode>();
            // 使用预定义的颜色列表，确保颜色对比明显
            var colors = new List<Color>
            {
                Colors.DodgerBlue,
                Colors.DarkOrange,
                Colors.GreenYellow,
                Colors.Gold,
                Colors.Purple,
                Colors.LimeGreen,
                Colors.OrangeRed
            };

            var random = new Random();
            for (int i = 0; i < 5; i++)
            {
                _DataTimeSeries.Add(new BasicPieDataMode
                {
                    Value = random.Next(10, 100),  // 增大数值范围
                    Name = $"测试{i}",
                    Color = colors[i]              // 使用预定义颜色
                });
                if (i < 2) _DataSeries.Add(new BasicPieDataMode
                {
                    Value = random.Next(1, 1000),  // 增大数值范围
                    Name = i == 0 ? "OK数量" : "NG数量",
                    Color = colors[5+i]              // 使用预定义颜色
                });
            }
            DataSeries = _DataSeries;
            DataTimeSeries = _DataTimeSeries;
        }

        public override void UpdataView()
        {
            if (!IsDebug)
            {
                UpData();
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
