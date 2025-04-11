using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;

namespace KupaKuper_MauiControl.Controls
{
    public partial class HmiIoList : ScrollGraphicsView
    {
        #region 绑定属性

        /// <summary>
        /// IO项目集合的绑定属性
        /// </summary>
        public static readonly BindableProperty IoItemsProperty = BindableProperty.Create(
            nameof(IoItems),
            typeof(ObservableCollection<IoDataMode>),
            typeof(HmiIoList),
            null,
            propertyChanged: OnCylinderItemsChanged);

        /// <summary>
        /// IO数据的绑定属性
        /// </summary>
        public ObservableCollection<IoDataMode> IoItems
        {
            get => (ObservableCollection<IoDataMode>)GetValue(IoItemsProperty);
            set => SetValue(IoItemsProperty, value);
        }

        #endregion 绑定属性

        #region 属性更新机制

        // 添加此字段跟踪当前订阅的集合
        private ObservableCollection<IoDataMode>? _currentCollection;

        // 存储已订阅属性更改的对象
        private HashSet<IoDataMode> _subscribedItems = new HashSet<IoDataMode>();

        /// <summary>
        /// 当项目集合变化时更新基类的Items属性
        /// </summary>
        private static void OnCylinderItemsChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is HmiIoList control)
            {
                // 取消订阅旧集合的事件
                if (oldValue is ObservableCollection<IoDataMode> oldCollection)
                {
                    oldCollection.CollectionChanged -= control.OnCollectionChanged;
                    control._currentCollection = null;

                    // 取消订阅旧集合中每个项目的属性变化
                    control.UnsubscribeAllCylinders();
                }

                // 订阅新集合的事件
                if (newValue is ObservableCollection<IoDataMode> newCollection)
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
        private void SubscribeAllCylinders(ObservableCollection<IoDataMode> items)
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
        private void SubscribeCylinderPropertyChanged(IoDataMode item)
        {
            if (!_subscribedItems.Contains(item) && item is INotifyPropertyChanged notifyPropertyChanged)
            {
                notifyPropertyChanged.PropertyChanged += OnCylinderPropertyChanged;
                _subscribedItems.Add(item);
            }
        }

        // 取消订阅单个的属性变化
        private void UnsubscribeCylinderPropertyChanged(IoDataMode item)
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
                    if (item is IoDataMode itemType)
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
                    if (item is IoDataMode itemType)
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
        private void UpdateItemsFromCollection(ObservableCollection<IoDataMode> collection)
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

        #endregion 属性更新机制

        /// <summary>
        /// 绘制项目组的控件
        /// </summary>
        public HmiIoList()
        {
            // 设置默认值
            ItemWidth = 277;
            ItemHeight = 33;

            // 设置边距
            ItemMarginX = 10;
            ItemMarginY = 10;

            // 确保布局更新
            if (graphicsDrawable != null)
            {
                graphicsDrawable.CalculateLayout = ScrollGraphicsDrawable.CalculateLayoutType.Width;
                graphicsDrawable.UpdateLayout(Width > 0 ? (float)Width : 800);
                this.Invalidate();
            }
        }

        #region 绘制IO需要使用的颜色

        private Color backgroundColor = new();
        private Color orangeColor = new();
        private Color buttonColor = new();
        private Color blueColor = new();
        private Color textColor = new();
        private LinearGradientPaint linearGradientPaint = new();

        #endregion 绘制IO需要使用的颜色

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
        /// 指示灯离边框的间距
        /// </summary>
        public float IoItemPadding { get; set; } = 6;

        /// <summary>
        /// 指示灯大小(半径)
        /// </summary>
        public float IndicatorSize { get; set; } = 7;

        /// <summary>
        /// 项目控件绘制
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="item"></param>
        /// <param name="index"></param>
        public override void GraphicsDrawable_DrawItem(ICanvas canvas, float x, float y, float Width, float Height, object item, int index)
        {
            if (item is IoDataMode Io)
            {
                try
                {
                    // 绘制圆角矩形背景
                    RectF linearRectangle = new RectF(x, y, (float)Width, (float)Height);
                    canvas.SetFillPaint(linearGradientPaint, linearRectangle);
                    canvas.FillRoundedRectangle(linearRectangle, 10);

                    // 绘制圆角矩形边框
                    canvas.StrokeSize = 1;
                    canvas.DrawRoundedRectangle(x, y, (float)Width, (float)Height + 0.5f, 10);

                    // 绘制指示灯
                    float indicatorX = x + IoItemPadding + IndicatorSize;
                    float indicatorY = y + Height / 2;

                    // 绘制指示灯边框
                    canvas.StrokeColor = Colors.Gray;
                    canvas.StrokeSize = 1;
                    canvas.DrawCircle(indicatorX, indicatorY, IndicatorSize + 2);

                    // 绘制指示灯状态
                    canvas.FillColor = Io.IoValue ? orangeColor : Colors.Transparent;
                    canvas.FillCircle(indicatorX, indicatorY, IndicatorSize);

                    // 绘制IO名称
                    canvas.FontColor = textColor;
                    canvas.FontSize = 14;
                    canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;

                    float textX = indicatorX + (float)IndicatorSize + 8;
                    float textY = y;
                    float textWidth = (float)Width - (float)IoItemPadding * 2 - (float)IndicatorSize - 8;
                    float textHeight = (float)Height;

                    canvas.DrawString(
                        Io.IoName,
                        textX, textY,
                        textWidth, textHeight,
                        HorizontalAlignment.Left,
                        VerticalAlignment.Center);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"绘制项目项错误: {ex.Message}");
                }
            }
        }
    }
}