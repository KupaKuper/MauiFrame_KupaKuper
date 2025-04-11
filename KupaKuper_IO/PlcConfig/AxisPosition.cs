using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class AxisPosition
    {
        /// <summary>
        /// 当前点位编号
        /// </summary>
        public int PositionNo { get; set; }

        /// <summary>
        /// 当前点位置
        /// </summary>
        public PlcVar PositionVar { get; set; } = new PlcVar() { VarInfo = "点位置" };

        /// <summary>
        /// 当前点位速度
        /// </summary>
        public PlcVar VelocityVar { get; set; } = new PlcVar() { VarInfo = "点位速度" };

        /// <summary>
        /// 当前点位名称
        /// </summary>
        public string Name { get; set; }

        public AxisPosition()
        {
            Name = "点位_" + PositionNo;
        }
    }
}