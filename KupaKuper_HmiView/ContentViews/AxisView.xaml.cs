using KupaKuper_HmiView.ContentViewModes;
using KupaKuper_HmiView.HelpVoid;
using KupaKuper_HmiView.Resources;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.ControlModes;
using KupaKuper_MauiControl.Controls;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class AxisView : ContentView
    {
        private readonly AxisViewVM _viewMode;

        public AxisView()
        {
            InitializeComponent();

            // 初始化ViewModel
            BindingContext = _viewMode = new AxisViewVM();
            //添加资源
            Resources.MergedDictionaries.Add(new MyStyles());
        }

        private void AxisView_Loaded(object? sender, EventArgs e)
        {
            try
            {
                // 确保有轴数据
                if (_viewMode.AxisNames.Count > 0)
                {
                    // 获取第一个轴的名称
                    string firstAxisName = _viewMode.AxisNames[0];

                    // 直接从视图树中查找第一个HmiButton
                    var buttons = this.GetVisualTreeDescendants()
                        .OfType<HmiButton>()
                        .Where(b => b.Text == firstAxisName)
                        .ToList();

                    if (buttons.Count > 0)
                    {
                        var firstButton = buttons.First();
                        // 直接调用按钮的点击事件
                        HmiButton_Clicked(firstButton, EventArgs.Empty);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化轴选择失败: {ex.Message}");
            }
            finally
            {
                // 移除事件处理器，因为只需要在首次加载时执行
                Loaded -= AxisView_Loaded;
            }
        }

        private AxisDataMode ShowAxisNow
        {
            get
            {
                return _ShowAxisNow;
            }
            set
            {
                _ShowAxisNow = value;
                ChangeAxisDataShow(_ShowAxisNow);
            }
        }

        /// <summary>
        /// 更改轴数据配置
        /// </summary>
        /// <param name="showAxisNow"></param>
        private void ChangeAxisDataShow(AxisDataMode showAxisNow)
        {
            _viewMode.AxisVar = showAxisNow;
        }

        /// <summary>
        /// 当前显示的轴数据
        /// </summary>
        private AxisDataMode _ShowAxisNow = new();

        /// <summary>
        /// 当前显示的轴名称按钮
        /// </summary>
        private HmiButton? _ShowAxisNameNow;

        /// <summary>
        /// 当前选择的点位显示按钮
        /// </summary>
        private Button? _ShowAxisPosition;

        private void HmiButton_Clicked(object sender, EventArgs e)
        {
            if (sender is HmiButton button)
            {
                AxisList_SelectionChanged(button);
                if (_viewMode.AxisVars.ContainsKey(button.Text))
                {
                    ShowAxisNow = _viewMode.AxisVars[button.Text];
                }
            }
        }

        /// <summary>
        /// 发送输入框数值的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tBox_RelPosition_EnterNumber(HmiNumberBox sender, EventArgs e)
        {
            sender.Text = sender.ValueNumber;
            PlcVar var = sender.Var;
            if (sender.ValueNumber != null) SendNumBoxValue(var, sender.ValueNumber);
        }

        /// <summary>
        /// 数值输入框触发方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void tBox_RelPosition_HmiNumberBox_Click(HmiNumberBox sender, EventArgs e)
        {
            sender.ValueNumber = await DisplayAlertHelp.TryDisplayPromptAsync("数值输入框", "输入当前值:", "OK", "Cancel", null, sender.ValueNumber ?? "", -1, Keyboard.Numeric);
        }

        /// <summary>
        /// 点位选择按钮触发方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Clicked(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            if (_ShowAxisPosition != null)
            {
                _ShowAxisPosition.Text = "";
                button.Text = "◀";
                _ShowAxisPosition = button;
                ChangeAbsNo(button.ZIndex);
            }
            else
            {
                button.Text = "◀";
                _ShowAxisPosition = button;
            }
        }

        /// <summary>
        /// 切换Abs动作的点位编号
        /// </summary>
        /// <param name="AbsNo"></param>
        private void ChangeAbsNo(int AbsNo)
        {
            _viewMode.AbsPosition = _viewMode.AxisPosition[AbsNo.ToString()].PositionVarValue.ToString();
        }

        /// <summary>
        /// 数值输入框发送输入值到PLC
        /// </summary>
        /// <param name="var"></param>
        /// <param name="value"></param>
        private void SendNumBoxValue(PlcVar var, string value)
        {
            PlcVarSend.NumberBos_SetValue(var, value);
        }

        /// <summary>
        /// 按钮按下触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_SetTrue_Clicked(object sender, EventArgs e)
        {
            var button = (HmiButton)sender;
            PlcVarSend.ButtonClicked_SetTrue(button.Var);
        }

        /// <summary>
        /// 按钮松开触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void but_SetFalse_Clicked(object sender, EventArgs e)
        {
            var button = (HmiButton)sender;
            PlcVarSend.ButtonClicked_SetFalse(button.Var);
        }
        /// <summary>
        /// 切换当前选中按钮的颜色
        /// </summary>
        /// <param name="button"></param>
        private void AxisList_SelectionChanged(HmiButton button)
        {
            var bluecolor = this.Resources.TryGetValue("blueColor", out object blue);
            if (_ShowAxisNameNow != null)
            {
                _ShowAxisNameNow.BorderColor = Colors.Transparent;
            }
            _ShowAxisNameNow = button;
            _ShowAxisNameNow.BorderColor = bluecolor ? (Color)blue : Color.FromArgb("00adb5");
        }
    }
}

