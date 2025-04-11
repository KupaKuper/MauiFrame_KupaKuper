using KupaKuper_HmiView.ContentViewModes;
using KupaKuper_HmiView.HelpVoid;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.Controls;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class OtherView : ContentView
    {
        private readonly OtherViewVM _viewMode;

        private Button? _currentSelectedButton;

        public OtherView()
        {
            InitializeComponent();

            // 设置 BindingContext
            BindingContext = _viewMode = new OtherViewVM();

            // 添加加载完成事件处理
            this.Loaded += OtherView_Loaded;
        }

        private void OtherView_Loaded(object? sender, EventArgs e)
        {
            try
            {
                if (BindingContext is OtherViewVM viewModel && viewModel.ParameterListName.Any())
                {
                    // 获取第一个参数组名称
                    string firstGroupName = viewModel.ParameterListName[0];

                    // 在ContentView中查找对应的按钮
                    var contentView = this.GetVisualTreeDescendants()
                        .OfType<ContentView>()
                        .FirstOrDefault();

                    if (contentView != null)
                    {
                        var buttons = contentView.GetVisualTreeDescendants()
                            .OfType<Button>()
                            .Where(b => b.Text == firstGroupName)
                            .ToList();
                        if (buttons.Any())
                        {
                            var firstButton = buttons.First();
                            // 确保视觉上的选中状态
                            var bluecolor = this.Resources.TryGetValue("blueColor", out object blue);
                            _currentSelectedButton = firstButton;
                            _currentSelectedButton.BorderColor = bluecolor ? (Color)blue : Color.FromArgb("00adb5");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"初始化其他设置选择失败: {ex.Message}");
            }
            finally
            {
                Loaded -= OtherView_Loaded;
            }
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button)
            {
                // 重置之前选中按钮的样式
                if (_currentSelectedButton != null)
                {
                    _currentSelectedButton.BorderColor = Colors.Transparent;
                }

                // 设置当前按钮的样式
                var bluecolor = this.Resources.TryGetValue("blueColor", out object blue);
                _currentSelectedButton = button;
                _currentSelectedButton.BorderColor = bluecolor ? (Color)blue : Color.FromArgb("00adb5");

                // 切换参数组
                if (BindingContext is OtherViewVM viewModel)
                {
                    viewModel.SwitchGroupCommand.Execute(button.Text);
                }
            }
        }

        private void Switch_Toggled(object sender, ToggledEventArgs e)
        {
            if (sender is Switch switchControl &&
                switchControl.BindingContext is ParameterData parameter &&
                BindingContext is OtherViewVM viewModel)
            {
                if (parameter.Value)
                {
                    PlcVarSend.ButtonClicked_SetTrue(parameter.Config.plcVar);
                }
                else
                {
                    PlcVarSend.ButtonClicked_SetFalse(parameter.Config.plcVar);
                }
            }
        }

        private async void tBox_RelPosition_HmiNumberBox_Click(HmiNumberBox sender, EventArgs e)
        {
            sender.ValueNumber = await DisplayAlertHelp.TryDisplayPromptAsync("数值输入框", "输入当前值:", "OK", "Cancel", null, sender.ValueNumber ?? "", -1, Keyboard.Numeric);
        }

        private void HmiNumberBox_ValueChanged(HmiNumberBox sender, EventArgs e)
        {
            sender.Text = sender.ValueNumber;
            PlcVar var = sender.Var;
            if (sender.ValueNumber != null) SendNumBoxValue(var, sender.ValueNumber);
            System.Diagnostics.Debug.WriteLine($"参数 {var.PlcVarName} 的值更改为: {sender.ValueNumber}");
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

        protected override void OnParentSet()
        {
            base.OnParentSet();

            if (Parent != null)
            {
                // 视图被添加到父容器时触发
                _viewMode.OnPageAppearing();
            }
            else
            {
                // 视图从父容器移除时触发
                _viewMode.OnPageDisappearing();
            }
        }
    }
}

