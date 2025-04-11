using KupaKuper_HmiView.ContentViewModes;
using KupaKuper_HmiView.HelpVoid;

using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.Controls;

namespace KupaKuper_HmiView.ContentViews
{
    public partial class CylinderView : ContentView
    {
        private readonly CylinderViewVM _viewMode;

        public CylinderView()
        {
            try
            {
                _viewMode = new();
                BindingContext = _viewMode;
                InitializeComponent();

                // 添加加载完成事件处理
                this.Loaded += CylinderView_Loaded;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"CylinderView 初始化错误: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Stack Trace: {ex.StackTrace}");
            }
        }
        /// <summary>
        /// 气缸界面加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CylinderView_Loaded(object? sender, EventArgs e)
        {
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(200), () =>
            {
                try
                {
                    // 确保有气缸组数据
                    if (_viewMode.GroupNames?.Count > 0)
                    {
                        // 获取第一个气缸组的名称
                        string firstCylinderName = _viewMode.GroupNames[0];

                        // 查找对应的按钮
                        var buttons = this.GetVisualTreeDescendants()
                            .OfType<HmiButton>()
                            .Where(b => b.Text == firstCylinderName)
                            .ToList();

                        if (buttons.Count > 0)
                        {
                            // 模拟点击第一个按钮
                            CylinderList_SelectionChanged(buttons[0]);
                            _viewMode.CurrentGroupKey = firstCylinderName;
                        }
                    }

                    System.Diagnostics.Debug.WriteLine($"当前页面项目数: {_viewMode?.CurrentPageCylinderNames?.Count}");
                    System.Diagnostics.Debug.WriteLine($"总数据项目数: {_viewMode?._allIoNames?.Count}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"初始化气缸组选择失败: {ex.Message}");
                }
                finally
                {
                    // 移除事件处理器，因为只需要在首次加载时执行
                    Loaded -= CylinderView_Loaded;
                }
            });
        }
        /// <summary>
        /// 当前显示气缸组按钮
        /// </summary>
        private HmiButton? _ShowCylindersNow;

        private void HmiButton_Clicked(object sender, EventArgs e)
        {
            HmiButton button = (HmiButton)sender;
            CylinderList_SelectionChanged(button);
        }

        /// <summary>
        /// 切换当前选中按钮的颜色
        /// </summary>
        /// <param name="button"></param>
        private void CylinderList_SelectionChanged(HmiButton button)
        {
            var bluecolor = this.Resources.TryGetValue("blueColor", out object blue);
            if (_ShowCylindersNow != null)
            {
                _ShowCylindersNow.BorderColor = Colors.Transparent;
            }
            _ShowCylindersNow = button;
            _ShowCylindersNow.BorderColor = bluecolor ? (Color)blue : Color.FromArgb("00adb5");
        }
        /// <summary>
        /// 气缸控件按钮触发执行方法
        /// </summary>
        /// <param name="plcVar"></param>
        private void HmiCylinderList_CylinderClick(PlcVar plcVar)
        {
            PlcVarSend.ButtonClicked_SetTrue(plcVar);
        }
    }
}