using KupaKuper_IO.Ethernet;

using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace KupaKuper_MauiControl.Controls
{
    public partial class HmiCylinderList : ScrollGraphicsView
    {
        #region 绑定属性
        /// <summary>
        /// 气缸项目集合的绑定属性
        /// </summary>
        public static readonly BindableProperty CylinderItemsProperty = BindableProperty.Create(
            nameof(CylinderItems),
            typeof(ObservableCollection<CylinderDataMode>),
            typeof(HmiCylinderList),
            null,
            propertyChanged: OnCylinderItemsChanged);

        public ObservableCollection<CylinderDataMode> CylinderItems
        {
            get => (ObservableCollection<CylinderDataMode>)GetValue(CylinderItemsProperty);
            set => SetValue(CylinderItemsProperty, value);
        }
        #endregion

        #region 属性更新机制
        // 添加此字段跟踪当前订阅的集合
        private ObservableCollection<CylinderDataMode>? _currentCollection;

        // 存储已订阅属性更改的对象
        private HashSet<CylinderDataMode> _subscribedItems = new HashSet<CylinderDataMode>();

        /// <summary>
        /// 当项目集合变化时更新基类的Items属性
        /// </summary>
        private static void OnCylinderItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is HmiCylinderList control)
            {
                // 取消订阅旧集合的事件
                if (oldValue is ObservableCollection<CylinderDataMode> oldCollection)
                {
                    oldCollection.CollectionChanged -= control.OnCollectionChanged;
                    control._currentCollection = null;

                    // 取消订阅旧集合中每个项目的属性变化
                    control.UnsubscribeAllCylinders();
                }

                // 订阅新集合的事件
                if (newValue is ObservableCollection<CylinderDataMode> newCollection)
                {
                    newCollection.CollectionChanged += control.OnCollectionChanged;
                    control._currentCollection = newCollection;

                    // 订阅新集合中每个项目的属性变化
                    control.SubscribeAllCylinders(newCollection);

                    // 初始更新
                    control.UpdateItemsFromCollection(newCollection);
                }
            }
        }

        // 订阅所有项目的属性变化
        private void SubscribeAllCylinders(ObservableCollection<CylinderDataMode> items)
        {
            foreach (var item in items)
            {
                SubscribeCylinderPropertyChanged(item);
            }
        }

        // 取消订阅所有项目的属性变化
        private void UnsubscribeAllCylinders()
        {
            foreach (var item in _subscribedItems)
            {
                UnsubscribeCylinderPropertyChanged(item);
            }
            _subscribedItems.Clear();
        }

        // 订阅单个项目的属性变化
        private void SubscribeCylinderPropertyChanged(CylinderDataMode item)
        {
            if (!_subscribedItems.Contains(item) && item is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnCylinderPropertyChanged;
                _subscribedItems.Add(item);
            }
        }

        // 取消订阅单个的属性变化
        private void UnsubscribeCylinderPropertyChanged(CylinderDataMode item)
        {
            if (_subscribedItems.Contains(item) && item is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged -= OnCylinderPropertyChanged;
                _subscribedItems.Remove(item);
            }
        }

        // 处理项目属性变化
        private void OnCylinderPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            // 当项目属性变化时强制重绘
            this.Invalidate();
        }

        // 处理集合变化，需要更新订阅
        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // 处理新增项
            if (e.NewItems != null)
            {
                foreach (var item in e.NewItems)
                {
                    if (item is CylinderDataMode itemType)
                    {
                        SubscribeCylinderPropertyChanged(itemType);
                    }
                }
            }

            // 处理删除项
            if (e.OldItems != null)
            {
                foreach (var item in e.OldItems)
                {
                    if (item is CylinderDataMode itemType)
                    {
                        UnsubscribeCylinderPropertyChanged(itemType);
                    }
                }
            }

            // 当集合内容变化时更新Items
            if (_currentCollection != null)
            {
                UpdateItemsFromCollection(_currentCollection);
            }
        }

        // 从集合更新Items的辅助方法
        private void UpdateItemsFromCollection(ObservableCollection<CylinderDataMode> collection)
        {
            var objectItems = new ObservableCollection<object>();
            foreach (var item in collection)
            {
                objectItems.Add(item);
            }

            // 更新基类的Items属性
            this.Items = objectItems;

            // 强制刷新绘制
            this.Invalidate();
        }
        #endregion

        public delegate void CylinderButtonClick(PlcVar? plcVar);
        public event CylinderButtonClick? CylinderClick;
        /// <summary>
        /// 绘制气缸组的控件
        /// </summary>
        public HmiCylinderList()
        {
            // 先设置委托
            buttonClick = (item, buttonIndex) => {
                CylinderButton_Click(item, buttonIndex);
            };
            
            // 设置默认值
            ItemWidth = 270;
            ItemHeight = 100;
            
            // 设置边距
            ItemMarginX = 10;
            ItemMarginY = 10;
            
            // 确保布局更新
            if (graphicsDrawable != null)
            {
                graphicsDrawable.UpdateLayout(Width > 0 ? (float)Width : 800);
                this.Invalidate();
            }
        }
        #region 绘制气缸需要使用的颜色
        private Color backgroundColor = new();
        private Color orangeColor = new();
        private Color buttonColor = new();
        private Color blueColor = new();
        private Color textColor = new();
        private LinearGradientPaint linearGradientPaint = new();
        #endregion
        /// <summary>
        /// 触发按钮
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="buttonIndex"></param>
        private void CylinderButton_Click(object Item, int buttonIndex)
        {
            var cylinder = (CylinderDataMode)Item;
            if (buttonIndex == 0)
            {
                if (cylinder?.Cylinder.Home != null)
                {
                    CylinderClick?.Invoke(cylinder?.Cylinder.Home);
                }
            }
            else if (buttonIndex == 1)
            {
                if (cylinder?.Cylinder.Work != null)
                {
                    CylinderClick?.Invoke(cylinder?.Cylinder.Work);
                }
            }
        }
        /// <summary>
        /// 绘制前先从字典读取需要的颜色
        /// </summary>
        public override void GraphicsDrawable_BeforeDrawItem()
        {
            // 从资源字典获取背景颜色
            if (Application.Current?.RequestedTheme == AppTheme.Dark)
            {
                var darkColor = new object();
                // 使用DarkLineUpColor
                if ((bool)(Application.Current?.Resources.TryGetValue("DarkLineUpColor", out darkColor)))
                {
                    backgroundColor = (Color)darkColor;
                }
                else
                {
                    backgroundColor = Color.FromArgb("#404040"); // 默认暗色
                }
            }
            else
            {
                var lightColor = new object();
                // 使用LightLineUpColor
                if ((bool)(Application.Current?.Resources.TryGetValue("LightLineUpColor", out lightColor)))
                {
                    backgroundColor = (Color)lightColor;
                }
                else
                {
                    backgroundColor = Color.FromArgb("#EAEAEA"); // 默认亮色
                }
            }

            // 获取指示灯颜色
            var orange = new object();
            // 使用orangeColor
            if ((bool)(Application.Current?.Resources.TryGetValue("orangeColor", out orange)))
            {
                orangeColor = (Color)orange;
            }
            else
            {
                orangeColor = Colors.Orange; // 默认橙色
            }

            // 获取蓝色
            var blue = new object();
            // 使用orangeColor
            if ((bool)(Application.Current?.Resources.TryGetValue("blueColor", out blue)))
            {
                blueColor = (Color)blue;
            }
            else
            {
                blueColor = Colors.DarkBlue; // 默认橙色
            }

            // 获取按钮颜色
            if (Application.Current?.RequestedTheme == AppTheme.Dark)
            {
                var darkColor = new object();
                // 使用DarkButtonColor
                if ((bool)(Application.Current?.Resources.TryGetValue("DarkButtonColor", out darkColor)))
                {
                    buttonColor = (Color)darkColor;
                }
                else
                {
                    buttonColor = Color.FromArgb("#404040"); // 默认暗色
                }
            }
            else
            {
                var lightColor = new object();
                // 使用LightButtonColor
                if ((bool)(Application.Current?.Resources.TryGetValue("LightButtonColor", out lightColor)))
                {
                    buttonColor = (Color)lightColor;
                }
                else
                {
                    buttonColor = Color.FromArgb("#EAEAEA"); // 默认亮色
                }
            }

            // 获取渐变色
            if (Application.Current?.RequestedTheme == AppTheme.Dark)
            {
                var gradientPaint = new object();
                Color upColor = ((Color)((bool)(Application.Current?.Resources.TryGetValue("DarkLineUpColor", out gradientPaint)) ? gradientPaint : Colors.White));
                Color downColor = ((Color)((bool)(Application.Current?.Resources.TryGetValue("DarkLineDownColor", out gradientPaint)) ? gradientPaint : Colors.Gray));
                linearGradientPaint = new()
                {
                    StartColor = upColor,
                    EndColor = downColor,
                    StartPoint = new Point(0, 0.9),
                    EndPoint = new Point(0, 1)
                };
            }
            else
            {
                var gradientPaint = new object();
                Color upColor = ((Color)((bool)(Application.Current?.Resources.TryGetValue("LightLineUpColor", out gradientPaint)) ? gradientPaint : Colors.White));
                Color downColor = ((Color)((bool)(Application.Current?.Resources.TryGetValue("LightLineDownColor", out gradientPaint)) ? gradientPaint : Colors.Gray));
                linearGradientPaint = new()
                {
                    StartColor = upColor,
                    EndColor = downColor,
                    StartPoint = new Point(0, 0.9),
                    EndPoint = new Point(0, 1)
                };
            }


            // 获取文本颜色
            if (Application.Current?.RequestedTheme == AppTheme.Dark)
            {
                var darkTextColor = new object();
                // 使用DarkTextColor
                if ((bool)(Application.Current?.Resources.TryGetValue("DarkTextColor", out darkTextColor)))
                {
                    textColor = (Color)darkTextColor;
                }
                else
                {
                    textColor = Colors.White; // 默认白色
                }
            }
            else
            {
                var lightTextColor = new object();
                // 使用LightTextColor
                if ((bool)(Application.Current?.Resources.TryGetValue("LightTextColor", out lightTextColor)))
                {
                    textColor = (Color)lightTextColor;
                }
                else
                {
                    textColor = Colors.Black; // 默认黑色
                }
            }
        }

        /// <summary>
        /// 气缸控件绘制
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void GraphicsDrawable_DrawItem(ICanvas canvas, float x, float y, float Width, float Height, object item, int index)
        {            
            if (item is CylinderDataMode cylinder)
            {
                try
                {
                    // 绘制气缸项背景
                    var itemRect = new RectF(x, y, Width, Height);
                    canvas.SetFillPaint(linearGradientPaint, itemRect);
                    canvas.FillRoundedRectangle(itemRect, 10);
                    // 绘制圆角矩形边框
                    canvas.StrokeSize = 1;
                    canvas.DrawRoundedRectangle(x, y, Width, Height + 0.5f, 10);

                    // 绘制气缸名称
                    canvas.FontColor = textColor;
                    canvas.FontSize = 14;
                    canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                    canvas.DrawString(cylinder.Cylinder.Name,
                        x + 10, y + 5,
                        Width - 20, 25,
                        HorizontalAlignment.Center, VerticalAlignment.Center);

                    //绘制横线
                    canvas.FillColor = Colors.Gray;
                    canvas.DrawLine(x + 15, y + 30, x + Width - 30, y + 30);

                    // 绘制状态指示灯
                    float indicatorSize = 7;
                    float[] indicatorX = { x + 20f + indicatorSize, x + Width - indicatorSize - 20f };
                    float[] indicatorY = { y + 38f + indicatorSize, y + 74f + indicatorSize };

                    // 绘制指示灯边框
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(indicatorX[0], indicatorY[0], (float)indicatorSize + 2);
                    // 缩回状态指示灯
                    canvas.FillColor = cylinder.HomeInputValue ? orangeColor : Colors.Transparent;
                    canvas.FillCircle(indicatorX[0], indicatorY[0], indicatorSize);

                    // 绘制指示灯边框
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(indicatorX[0], indicatorY[1], (float)indicatorSize + 2);
                    // 缩回完成状态指示灯
                    canvas.FillColor = cylinder.HomeDownValue ? orangeColor : Colors.Transparent;
                    canvas.FillCircle(indicatorX[0], indicatorY[1], indicatorSize);

                    // 绘制指示灯边框
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(indicatorX[1], indicatorY[0], (float)indicatorSize + 2);
                    // 伸出状态指示灯
                    canvas.FillColor = cylinder.WorkInputValue ? orangeColor : Colors.Transparent;
                    canvas.FillCircle(indicatorX[1], indicatorY[0], indicatorSize);

                    // 绘制指示灯边框
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(indicatorX[1], indicatorY[1], (float)indicatorSize + 2);
                    // 伸出完成状态指示灯
                    canvas.FillColor = cylinder.WorkDownValue ? orangeColor : Colors.Transparent;
                    canvas.FillCircle(indicatorX[1], indicatorY[1], indicatorSize);

                    //绘制气缸报警提示
                    if (cylinder.ErrorValue)
                    {
                        canvas.FontColor = Colors.Red;
                        canvas.FontSize = 14;
                        canvas.DrawString("⚠️", x + 5, y + 5, 20, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
                    }

                    // 绘制控制按钮
                    float buttonWidth = 80;
                    float buttonHeight = 50;
                    float[] buttonX = { x + Width / 2 - buttonWidth - 10, x + Width / 2 + 10 };
                    float buttonY = indicatorY[0] - 5;

                    // 缩回按钮 - 添加按下效果
                    bool homeButtonPressed = base.graphicsDrawable.IsButtonPressed(index, 0);
                    var homeButtonRect = new RectF(
                        buttonX[0] + (homeButtonPressed ? 2 : 0),
                        buttonY + (homeButtonPressed ? 2 : 0),
                        buttonWidth - (homeButtonPressed ? 4 : 0),
                        buttonHeight - (homeButtonPressed ? 4 : 0));
                    canvas.FillColor = cylinder.HomeValue ? blueColor : buttonColor; // 按下时收缩
                    canvas.FillRoundedRectangle(homeButtonRect, 4);
                    //按钮边框
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1;
                    canvas.DrawRoundedRectangle(homeButtonRect, 4);
                    //按钮名称
                    canvas.FontColor = textColor;
                    canvas.FontSize = 14;
                    canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                    canvas.DrawString(cylinder.Cylinder.HomeButtonName, homeButtonRect, HorizontalAlignment.Center, VerticalAlignment.Center);
                    //绘制lock锁
                    if (cylinder.HomeLockValue || cylinder.LockValue)
                    {
                        canvas.FontColor = Colors.Orange;
                        canvas.FontSize = 14;
                        canvas.DrawString("🔒", homeButtonRect.X, homeButtonRect.Y, 20, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
                    }

                    // 伸出按钮 - 添加按下效果
                    bool workButtonPressed = base.graphicsDrawable.IsButtonPressed(index, 1);
                    var workButtonRect = new RectF(
                        buttonX[1] + (workButtonPressed ? 2 : 0),
                        buttonY + (workButtonPressed ? 2 : 0),
                        buttonWidth - (workButtonPressed ? 4 : 0),
                        buttonHeight - (workButtonPressed ? 4 : 0));
                    canvas.FillColor = cylinder.WorkValue ? blueColor : buttonColor; // 按下时收缩
                    canvas.FillRoundedRectangle(workButtonRect, 4);
                    //按钮边框
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1;
                    canvas.DrawRoundedRectangle(workButtonRect, 4);
                    //按钮名称
                    canvas.FontColor = textColor;
                    canvas.FontSize = 14;
                    canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                    canvas.DrawString(cylinder.Cylinder.WorkButtonName, workButtonRect, HorizontalAlignment.Center, VerticalAlignment.Center);
                    //绘制lock锁
                    if (cylinder.WorkLockValue || cylinder.LockValue)
                    {
                        canvas.FontColor = Colors.Orange;
                        canvas.FontSize = 14;
                        canvas.DrawString("🔒", workButtonRect.X, workButtonRect.Y, 20, 20, HorizontalAlignment.Center, VerticalAlignment.Center);
                    }

                    // 存储按钮区域，用于点击检测 - 关键修改：存储实际屏幕位置
                    base.graphicsDrawable.AddButtonRectF(index, new() { homeButtonRect, workButtonRect });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"绘制气缸项错误: {ex.Message}");
                }
            }
        }
    }
}
