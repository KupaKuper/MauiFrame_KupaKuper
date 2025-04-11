using KupaKuper_IO.Ethernet;

namespace KupaKuper_MauiControl.Controls
{
    public class HmiNumberBox : Label
    {
        public HmiNumberBox()
        {
            TapGestureRecognizer tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += (s, e) =>
            {
                if (this.HmiNumberBox_Click != null) this.HmiNumberBox_Click.Invoke(this, e);
            };
            this.GestureRecognizers.Add(tapGestureRecognizer);
            this.TextDecorations = TextDecorations.Underline;
        }
        /// <summary>
        /// PLC变量的绑定属性
        /// </summary>
        public static readonly BindableProperty VarProperty = BindableProperty.Create(
            nameof(Var),
            typeof(PlcVar),
            typeof(HmiNumberBox),
            null);

        /// <summary>
        /// 获取或设置PLC变量
        /// </summary>
        public PlcVar Var
        {
            get => (PlcVar)GetValue(VarProperty);
            set => SetValue(VarProperty, value);
        }
        /// <summary>
        /// 输入的数值
        /// </summary>
        public string? ValueNumber
        {
            get
            {
                return _valueNumber;
            }
            set
            {
                if (EnterNumber != null && value != _valueNumber && value != null)
                {
                    _valueNumber = value;
                    EventArgs e = new EventArgs();
                    this.EnterNumber.Invoke(this, e);
                }
            }
        }
        private string? _valueNumber = "0";

        public delegate void EnterNumberEventHandler(HmiNumberBox sender, EventArgs e);
        /// <summary>
        /// 输入数值后执行的方法
        /// </summary>
        public event EnterNumberEventHandler? EnterNumber;
        public delegate void HmiNumberBoxEventHandler(HmiNumberBox sender, EventArgs e);
        /// <summary>
        /// 单击控件时执行
        /// </summary>
        public event HmiNumberBoxEventHandler? HmiNumberBox_Click;


    }
}

