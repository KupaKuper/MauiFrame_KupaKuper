using CommunityToolkit.Mvvm.ComponentModel;

using KupaKuper_IO.PlcConfig;

namespace KupaKuper_MauiControl.ControlModes
{
    /// <summary>
    /// 轴绑定数据模型
    /// </summary>
    public partial class AxisDataMode : ObservableObject
    {
        [ObservableProperty]
        public AxisControl axis = new();
        /// <summary>
        /// 轴当前位置的值
        /// </summary>
        [ObservableProperty]
        public string currentPositionValue = "0000.000";

        /// <summary>
        /// 轴使能状态值
        /// </summary>
        [ObservableProperty]
        public bool powerValue = false;

        /// <summary>
        /// 轴忙碌状态值
        /// </summary>
        [ObservableProperty]
        public bool busyValue = false;

        /// <summary>
        /// 轴记忆位的值
        /// </summary>
        [ObservableProperty]
        public string memoryPositionValue = "0000.000";

        /// <summary>
        /// 轴报错状态值
        /// </summary>
        [ObservableProperty]
        public bool errorValue = false;

        /// <summary>
        /// 定位编号的值
        /// </summary>
        [ObservableProperty]
        public int absNumberValue = 0;

        /// <summary>
        /// 点动速度的值
        /// </summary>
        [ObservableProperty]
        public string jogVelocityValue = "000";

        /// <summary>
        /// 轴正极限状态值
        /// </summary>
        [ObservableProperty]
        public bool posLimitValue = false;

        /// <summary>
        /// 轴负极限状态值
        /// </summary>
        [ObservableProperty]
        public bool negLimitValue = false;

        /// <summary>
        /// 轴原点状态值
        /// </summary>
        [ObservableProperty]
        public bool originValue = false;

        /// <summary>
        /// 回原点完成状态值
        /// </summary>
        [ObservableProperty]
        public bool homeDoneValue = false;

        /// <summary>
        /// 绝对定位完成状态值
        /// </summary>
        [ObservableProperty]
        public bool movAbsDoneValue = false;

        /// <summary>
        /// 相对定位完成状态值
        /// </summary>
        [ObservableProperty]
        public bool movRelDoneValue = false;

        /// <summary>
        /// 轴点位列表
        /// </summary>
        [ObservableProperty]
        public List<AxisPositionMode> listPosition = new()
        {
            new(){AxisPosition=new(){PositionNo=0 }},
            new(){AxisPosition=new(){PositionNo=1 }},
            new(){AxisPosition=new(){PositionNo=2 }},
            new(){AxisPosition=new(){PositionNo=3 }},
            new(){AxisPosition=new(){PositionNo=4 }},
            new(){AxisPosition=new(){PositionNo=5 }},
            new(){AxisPosition=new(){PositionNo=6 }},
            new(){AxisPosition=new(){PositionNo=7 }},
            new(){AxisPosition=new(){PositionNo=8 }},
            new(){AxisPosition=new(){PositionNo=9 }},
            new(){AxisPosition=new(){PositionNo=10 }},
            new(){AxisPosition=new(){PositionNo=11 }},
            new(){AxisPosition=new(){PositionNo=12 }},
            new(){AxisPosition=new(){PositionNo=13 }},
            new(){AxisPosition=new(){PositionNo=14 }},
        };
    }
    /// <summary>
    /// 轴点位绑定数据模型
    /// </summary>
    public partial class AxisPositionMode : ObservableObject
    {
        /// <summary>
        /// 轴点位变量
        /// </summary>
        [ObservableProperty]
        public AxisPosition axisPosition = new();

        /// <summary>
        /// 当前点位置值
        /// </summary>
        [ObservableProperty]
        public string positionVarValue = "0000.000";

        /// <summary>
        /// 当前点位速度值
        /// </summary>
        [ObservableProperty]
        public string velocityVarValue = "000";
    }
}
