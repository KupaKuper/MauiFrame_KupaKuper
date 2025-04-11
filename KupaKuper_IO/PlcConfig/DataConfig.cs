using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class DataConfig
    {
        /// <summary>
        /// 生产总数
        /// </summary>
        public PlcVar ProductionNumber { get; set; } = new() { VarInfo = "设备生产总数" };

        /// <summary>
        /// NG数量
        /// </summary>
        public PlcVar NgNumber { get; set; } = new() { VarInfo = "生产NG数量" };

        /// <summary>
        /// 设备运行时间
        /// </summary>
        public PlcVar RunningTime { get; set; } = new() { VarInfo = "设备运行时间" };

        /// <summary>
        /// 设备暂停时间
        /// </summary>
        public PlcVar PauseTime { get; set; } = new() { VarInfo = "设备暂停时间" };

        /// <summary>
        /// 设备报警时间
        /// </summary>
        public PlcVar AlarmTime { get; set; } = new() { VarInfo = "设备报警时间" };

        /// <summary>
        /// 设备待料时间
        /// </summary>
        public PlcVar DownTime { get; set; } = new() { VarInfo = "设备待料时间" };

        /// <summary>
        /// 产能数据保存的文加夹地址
        /// </summary>
        public string DailyProductAdr { get; set; } = @".\ProductData";

        /// <summary>
        /// 需要读取的Csv表格地址
        /// </summary>
        public List<string> ReadCsvAddress { get; set; } = new() { "显示到产品数据界面的文件地址" };

        /// <summary>
        /// 产品图片读取地址
        /// </summary>
        public string PictureAdr { get; set; } = @".\ProductPicture";
    }
}