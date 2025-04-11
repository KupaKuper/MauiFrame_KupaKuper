namespace KupaKuper_IO.PlcConfig
{
    public class Axis
    {
        /// <summary>
        /// 轴控制变量
        /// </summary>
        public AxisControl AxisControl { get; set; } = new();

        /// <summary>
        /// 轴点位列表
        /// </summary>
        public List<AxisPosition> ListPosition { get; set; } = new List<AxisPosition>()
        {
            new(){PositionNo=0, Name="点位1"},
            new(){PositionNo=1,Name="点位2"},
            new(){PositionNo=2,Name="点位3"},
            new(){PositionNo=3,Name="点位4"},
            new(){PositionNo=4,Name="点位5"},
            new(){PositionNo=5,Name="点位6"},
            new(){PositionNo=6,Name="点位7"},
            new(){PositionNo=7,Name="点位8"},
            new(){PositionNo=8,Name="点位9"},
            new(){PositionNo=9,Name="点位10"},
            new(){PositionNo=10,Name="点位11"},
            new(){PositionNo=11,Name="点位12"},
            new(){PositionNo=12,Name="点位13"},
            new(){PositionNo=13,Name="点位14"},
            new(){PositionNo=14,Name="点位15"},
        };
    }
}