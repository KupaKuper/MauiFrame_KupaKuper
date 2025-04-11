using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_HmiView.Resources;

using KupaKuper_IO.HelpVoid;

using KupaKuper_MauiControl.ControlModes;

using System.Diagnostics;

namespace KupaKuper_HmiView.ContentViewModes
{
    /// <summary>
    /// 产品mes数据界面
    /// </summary>
    public partial class DataRecordViewVM: BaseViewMode
    {
        [ObservableProperty]
        private CsvTableDataMode dataSource = new();
        [ObservableProperty]
        private List<string> csvName = new();
        [ObservableProperty]
        private string nowCsvAdr = "";

        private FileHelp fileHelp = new();

        public override uint ViewIndex { get; set; }
        public override string ViewName { get => AppResources.ResourceManager?.GetString("产品数据") ?? "产品数据"; set => throw new NotImplementedException(); }

        public DataRecordViewVM()
        {
            UpdataView();
        }
        private bool AddCsvAdr()
        {
            bool r = false;
            if (Device.dataConfig.ReadCsvAddress.Count < 1) return false;
            foreach (var item in Device.dataConfig.ReadCsvAddress)
            {
                if (File.Exists(item) && Path.GetExtension(item) == ".csv")
                {
                    CsvName.Add(item);
                    FileChangeHelp.FileChanged(item);
                    r = true;
                }
            }
            return r;
        }
        /// <summary>
        /// 数据变化
        /// </summary>
        private async void DataWatch()
        {
            if (NowView != ViewIndex) return;
            string DataAdr = NowCsvAdr;
            if (!File.Exists(DataAdr)) return;
            if (!FileChangeHelp.FileChanged(DataAdr)) return;
            await fileHelp.FileDisposeTask(DataAdr);
        }
        private void FileHelp_FileDispose(string filePath)
        {
            Debug.WriteLine($"文件已更改: {filePath}");
            // 处理文件变更
            LoadData(filePath);
        }
        /// <summary>
        /// 读取本地生产数据
        /// </summary>
        /// <param name="DataAdr"></param>
        private void LoadData(string DataAdr)
        {
            if (!File.Exists(DataAdr)) return;
            var Data = CsvFileHelper.ReadAllLinesAndSplitAsync(DataAdr).Result;
            if (Data.Count < 2) return;
            var header = Data.First();
            Data.RemoveAt(0);
            var random = new Random();
            var csvTableDataMode = new CsvTableDataMode
            {
                Headers = header,
                Rows = Data
            };
            DataSource = csvTableDataMode;
        }
        /// <summary>
        /// 生成随机数据
        /// </summary>
        public void FillTestData()
        {
            var random = new Random();
            var head = new string[8];
            var _row = new List<string[]>();
            
            // 设置列标题
            for (int j = 0; j < head.Length; j++)
            {
                head[j] = $"列{j}";
            }
            
            // 生成测试数据
            for (int i = 0; i < 100; i++)
            {
                List<string> s = new();
                s.Add(i.ToString());
                for (int j = 1; j < head.Length; j++)
                {
                    s.Add(random.Next(100, 1000).ToString());
                }
                _row.Add(s.ToArray());      
            }

            // 设置数据源并通知UI更新
            DataSource.Headers = head;
            if (DataSource.Rows.Count >= 100)
            {
                DataSource.Rows.AddRange(_row);
            }
            else
            {
                DataSource.Rows = _row;
            }
        }

        /// <summary>
        /// 修改读取的Csv文件
        /// </summary>
        /// <param name="NewAdr"></param>
        public void ChangeCsvAdr(string NewAdr)
        {
            if (FileChangeHelp.FileChanged(NewAdr)) return;
            Task.Run(() => LoadData(NewAdr));
        }

        public override void UpdataView()
        {
            if (!IsDebug)
            {
                if (AddCsvAdr())
                {
                    // 其他初始化...
                    fileHelp.FileDispose += FileHelp_FileDispose;
                    DataWatch();
                }
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
