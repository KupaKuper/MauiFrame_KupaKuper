using KupaKuper_IO.Ethernet;

namespace KupaKuper_IO.PlcConfig
{
    public class AxisControl
    {
        /// <summary>
        /// 轴名称
        /// </summary>
        public string? Name { get; set; } = "测试使用轴" + DateTime.Now.ToString("dd:hh:mm:ss");

        /// <summary>
        /// 轴当前位置
        /// </summary>
        public PlcVar CurrentPosition { get; set; } = new PlcVar() { VarInfo = "轴当前位置" };

        /// <summary>
        /// 轴使能
        /// </summary>
        public PlcVar Power { get; set; } = new PlcVar() { VarInfo = "轴使能" };

        /// <summary>
        /// 轴忙碌
        /// </summary>
        public PlcVar Busy { get; set; } = new PlcVar() { VarInfo = "轴忙碌" };

        /// <summary>
        /// 上使能
        /// </summary>
        public PlcVar OpenPower { get; set; } = new PlcVar() { VarInfo = "上使能" };

        /// <summary>
        /// 点动正
        /// </summary>
        public PlcVar JogP { get; set; } = new PlcVar() { VarInfo = "点动正" };

        /// <summary>
        /// 点动负
        /// </summary>
        public PlcVar JogN { get; set; } = new PlcVar() { VarInfo = "点动负" };

        /// <summary>
        /// 轴停止
        /// </summary>
        public PlcVar Stop { get; set; } = new PlcVar() { VarInfo = "轴停止" };

        /// <summary>
        /// 轴回原点
        /// </summary>
        public PlcVar GoHome { get; set; } = new PlcVar() { VarInfo = "轴回原点" };

        /// <summary>
        /// 轴回原点完成
        /// </summary>
        public PlcVar HomeDown { get; set; } = new PlcVar() { VarInfo = "轴回原点完成" };

        /// <summary>
        /// 轴复位
        /// </summary>
        public PlcVar Reset { get; set; } = new PlcVar() { VarInfo = "轴复位" };

        /// <summary>
        /// 点位示教
        /// </summary>
        public PlcVar Teach { get; set; } = new PlcVar() { VarInfo = "点位示教" };

        /// <summary>
        /// 绝对定位触发
        /// </summary>
        public PlcVar MoveAbs { get; set; } = new PlcVar() { VarInfo = "绝对定位触发" };

        /// <summary>
        /// 相对定位触发
        /// </summary>
        public PlcVar MoveRel { get; set; } = new PlcVar() { VarInfo = "相对定位触发" };

        /// <summary>
        /// 相对位移
        /// </summary>
        public PlcVar RelativePosition { get; set; } = new() { VarInfo = "相对位移" };

        /// <summary>
        /// 轴回记忆位
        /// </summary>
        public PlcVar MoveMemory { get; set; } = new PlcVar() { VarInfo = "轴回记忆位" };

        /// <summary>
        /// 轴记忆位
        /// </summary>
        public PlcVar MemoryPosition { get; set; } = new PlcVar() { VarInfo = "轴记忆位" };

        /// <summary>
        /// 轴报错
        /// </summary>
        public PlcVar Error { get; set; } = new PlcVar() { VarInfo = "轴报错" };

        /// <summary>
        /// 定位编号
        /// </summary>
        public PlcVar AbsNumber { get; set; } = new PlcVar() { VarInfo = "定位编号" };

        /// <summary>
        /// 点动速度
        /// </summary>
        public PlcVar JogVelocity { get; set; } = new PlcVar() { VarInfo = "点动速度" };

        /// <summary>
        /// 轴正极限
        /// </summary>
        public PlcVar PosLimit { get; set; } = new PlcVar() { VarInfo = "轴正极限" };

        /// <summary>
        /// 轴负极限
        /// </summary>
        public PlcVar NegLimit { get; set; } = new PlcVar() { VarInfo = "轴负极限" };

        /// <summary>
        /// 轴原点
        /// </summary>
        public PlcVar Origin { get; set; } = new PlcVar() { VarInfo = "轴原点" };

        /// <summary>
        /// 绝对定位完成
        /// </summary>
        public PlcVar MovAbsDone { get; set; } = new PlcVar() { VarInfo = "绝对定位完成" };

        /// <summary>
        /// 相对定位完成
        /// </summary>
        public PlcVar MovRelDone { get; set; } = new PlcVar() { VarInfo = "相对定位完成" };
    }
}