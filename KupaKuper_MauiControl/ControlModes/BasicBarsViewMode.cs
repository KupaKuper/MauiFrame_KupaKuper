using CommunityToolkit.Mvvm.ComponentModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public partial class BasicBarsViewMode : ObservableObject, IDrawable
    {
        #region 数据源属性
        /// <summary>
        /// 多系列数据集合
        /// </summary>
        [ObservableProperty]
        private List<List<double>> dataSeries = new();
        /// <summary>
        /// X轴标签集合
        /// </summary>
        [ObservableProperty]
        private List<string> xLabels = new();
        /// <summary>
        /// 每个系列对应的颜色
        /// </summary>
        [ObservableProperty]
        private List<Color> seriesColors = new();
        /// <summary>
        /// 图例标签集合
        /// </summary>
        [ObservableProperty]
        private List<string> legendLabels = new();
        /// <summary>
        /// 文本颜色
        /// </summary>
        [ObservableProperty]
        private Color textColor = Application.Current.RequestedTheme == AppTheme.Dark ? 
            Colors.White : Colors.Black;
        #endregion

        #region 滚动相关属性
        /// <summary>
        /// 当前滚动偏移量
        /// </summary>
        private float _scrollOffset = 0;
        /// <summary>
        /// 最小滚动偏移量（左边界）
        /// </summary>
        private float _minScrollOffset;
        /// <summary>
        /// 最大滚动偏移量（右边界）
        /// </summary>
        private float _maxScrollOffset;
        /// <summary>
        /// 上一次平移位置
        /// </summary>
        private float _lastPanX;
        /// <summary>
        /// 平移速度
        /// </summary>
        private float _panVelocity;
        /// <summary>
        /// 是否正在平移
        /// </summary>
        private bool _isPanning;
        /// <summary>
        /// 是否正在动画
        /// </summary>
        private bool _isAnimating;
        #endregion

        #region 图表布局属性
        /// <summary>
        /// 柱形宽度
        /// </summary>
        private float _barWidth = 30f;
        /// <summary>
        /// 柱形间距
        /// </summary>
        private float _barSpacing;
        /// <summary>
        /// 图表内边距
        /// </summary>
        private float _chartPadding = 20f;
        /// <summary>
        /// 正常状态柱形颜色
        /// </summary>
        private Color _okBarColor = Colors.DodgerBlue;
        /// <summary>
        /// 异常状态柱形颜色
        /// </summary>
        private Color _ngBarColor = Colors.Red;
        /// <summary>
        /// 图表最大值
        /// </summary>
        private double _maxValue;
        /// <summary>
        /// 是否自动缩放
        /// </summary>
        private bool _autoScale = true;
        /// <summary>
        /// Y轴刻度数量
        /// </summary>
        private int _yAxisSteps = 10;
        #endregion

        #region 常量定义
        /// <summary>
        /// 图表边距比例
        /// </summary>
        private const float CHART_PADDING_RATIO = 0.05f;
        /// <summary>
        /// Y轴标签宽度
        /// </summary>
        private const float Y_AXIS_LABEL_WIDTH = 50f;
        /// <summary>
        /// 边缘缓冲区大小
        /// </summary>
        private const float EDGE_BUFFER = 15;
        /// <summary>
        /// 滑动摩擦系数
        /// </summary>
        private const float FRICTION = 0.95f;
        /// <summary>
        /// 最小速度阈值
        /// </summary>
        private const float MIN_VELOCITY = 0.1f;
        #endregion

        #region 私有字段
        /// <summary>
        /// 图形视图引用
        /// </summary>
        private readonly IView _graphicsView;
        /// <summary>
        /// 动画计时器
        /// </summary>
        private IDispatcherTimer _animationTimer;
        #endregion

        // 构造函数
        public BasicBarsViewMode(IView graphicsView)
        {
            _graphicsView = graphicsView;   // 保存图表控件引用
            
            // 监听视图大小变化事件
            if (_graphicsView is Microsoft.Maui.Controls.View view)
            {
                view.SizeChanged += (s, e) =>
                {
                    UpdateScrollRange();    // 重新计算滚动范围
                    (_graphicsView as IGraphicsView)?.Invalidate();  // 触发重绘
                };
            }

            // 初始化动画计时器
            _animationTimer = Application.Current.Dispatcher.CreateTimer();  // 创建计时器
            _animationTimer.Interval = TimeSpan.FromMilliseconds(16);       // 设置刷新间隔（约60fps）
            _animationTimer.Tick += OnAnimationTick;                        // 注册计时器回调
        }

        // 更新图表数据
        // 注意：此方法需要确保 _graphicsView 已经完成初始化并且具有有效的宽度
        // 如果在视图加载完成之前调用此方法，UpdateScrollRange 将无法正确计算滚动范围
        // 在其他地方调用这个方法来切换数据
        public void UpdateData(List<List<double>> dataSeries, List<string> xLabels, List<Color> colors, List<string> legendLabels)
        {
            DataSeries = dataSeries;
            XLabels = xLabels;
            SeriesColors = colors;
            LegendLabels = legendLabels;
            
            // 检查视图是否已正确初始化
            if ((_graphicsView as Microsoft.Maui.Controls.View)?.Width > 0)
            {
                UpdateScrollRange();
                (_graphicsView as IGraphicsView)?.Invalidate();
            }
            else
            {
                // 如果视图尚未初始化，等待 SizeChanged 事件
                if (_graphicsView is Microsoft.Maui.Controls.View view)
                {
                    view.SizeChanged += (s, e) =>
                    {
                        if (view.Width > 0)
                        {
                            UpdateScrollRange();
                            (_graphicsView as IGraphicsView)?.Invalidate();
                        }
                    };
                }
            }
        }

        private void UpdateScrollRange()
        {
            // 检查数据和视图是否有效
            if (DataSeries.Count == 0 || XLabels.Count == 0 || _graphicsView == null) return;

            // 获取数据点数量和系列数量
            int dataPoints = XLabels.Count;
            int seriesCount = DataSeries.Count;

            // 计算柱形组的布局参数
            float groupSpacing = _barWidth;                                     // 组间距等于柱宽
            float barSpacing = _barWidth * 0.2f;                               // 组内柱间距为柱宽的20%
            float groupWidth = (seriesCount * _barWidth) + 
                             ((seriesCount - 1) * barSpacing);                 // 计算每组的总宽度
            
            // 计算实际内容总宽度
            float totalContentWidth = (dataPoints * groupWidth) + 
                                   ((dataPoints - 1) * groupSpacing);          // 计算所有数据的总宽度
            
            // 计算可用的显示宽度（减去边距和Y轴标签宽度）
            float visibleWidth = (float)(_graphicsView as Microsoft.Maui.Controls.View)?.Bounds.Width - 
                               (2 * _chartPadding) - Y_AXIS_LABEL_WIDTH;       // 计算可视区域宽度
            
            // 只有当内容宽度大于可视区域时才允许滚动
            if (totalContentWidth > visibleWidth)
            {
                _minScrollOffset = -(totalContentWidth - visibleWidth);        // 设置最小偏移量（左边界）
                _maxScrollOffset = 0;                                          // 设置最大偏移量（右边界）
            }
            else
            {
                _minScrollOffset = 0;                                          // 内容不足时不允许滚动
                _maxScrollOffset = 0;
            }

            // 确保当前偏移量在有效范围内
            _scrollOffset = Math.Clamp(_scrollOffset, _minScrollOffset, _maxScrollOffset);
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            // 如果没有在进行动画，直接返回
            if (!_isAnimating) return;

            // 根据当前速度计算新的偏移位置
            float newOffset = _scrollOffset + _panVelocity;

            // 处理边界情况
            if (newOffset > -EDGE_BUFFER && newOffset < 0)
            {
                _scrollOffset = 0;                  // 如果接近右边界，直接贴边
                StopAnimation();                    // 停止动画
            }
            else if (newOffset < _minScrollOffset + EDGE_BUFFER && newOffset > _minScrollOffset)
            {
                _scrollOffset = _minScrollOffset;   // 如果接近左边界，直接贴边
                StopAnimation();                    // 停止动画
            }
            else
            {
                // 确保偏移量在有效范围内
                _scrollOffset = Math.Clamp(newOffset, _minScrollOffset, _maxScrollOffset);
            }

            // 应用摩擦力，减小速度
            _panVelocity *= FRICTION;

            // 当速度小于阈值时停止动画
            if (Math.Abs(_panVelocity) < MIN_VELOCITY)
            {
                StopAnimation();
            }

            // 触发重绘
            (_graphicsView as IGraphicsView)?.Invalidate();
        }

        private void StopAnimation()
        {
            _isAnimating = false;          // 标记动画结束
            _animationTimer.Stop();        // 停止计时器
            _panVelocity = 0;             // 重置速度
        }

        public void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _isPanning = true;              // 标记开始滑动
                    _lastPanX = 0;                  // 重置上次位置
                    StopAnimation();                // 停止当前动画
                    break;

                case GestureStatus.Running:
                    // 如果已经在边缘，直接返回，防止抖动
                    if ((_scrollOffset >= 0 && e.TotalX > 0) || 
                        (_scrollOffset <= _minScrollOffset && e.TotalX < 0))
                    {
                        return;
                    }

                    // 计算滑动距离
                    float deltaX = (float)e.TotalX - _lastPanX;
                    _lastPanX = (float)e.TotalX;

                    // 计算新的偏移位置（乘以1.5使滑动更灵敏）
                    float newOffset = _scrollOffset + deltaX * 1.5f;

                    // 确保偏移量在有效范围内
                    _scrollOffset = Math.Clamp(newOffset, _minScrollOffset, _maxScrollOffset);

                    // 更新速度用于惯性滚动
                    _panVelocity = Math.Abs(deltaX * 1.5f) > 5 ? deltaX * 1.5f : 0;

                    // 触发重绘
                    (_graphicsView as IGraphicsView)?.Invalidate();
                    break;

                case GestureStatus.Completed:
                    _isPanning = false;             // 标记滑动结束
                    // 如果速度足够大，开始惯性动画
                    if (Math.Abs(_panVelocity) > MIN_VELOCITY)
                    {
                        _isAnimating = true;
                        _animationTimer.Start();
                    }
                    break;

                case GestureStatus.Canceled:
                    _isPanning = false;             // 标记滑动取消
                    StopAnimation();                // 停止动画
                    break;
            }
        }

        public void UpdateLayout(float barWidth, float barSpacing, float chartPadding,
                               Color okBarColor, Color ngBarColor,
                               double maxValue, bool autoScale, int yAxisSteps,
                               Color textColor)
        {
            _barWidth = barWidth;
            _barSpacing = barSpacing;
            _chartPadding = chartPadding;
            _okBarColor = okBarColor;
            _ngBarColor = ngBarColor;
            _maxValue = maxValue;
            _autoScale = autoScale;
            _yAxisSteps = yAxisSteps;
            TextColor = textColor;

            UpdateScrollRange();
            (_graphicsView as IGraphicsView)?.Invalidate();
        }

        private void DrawBackgrounds(ICanvas canvas, RectF dirtyRect,
            float yAxisX, float xAxisEndX, float yAxisStartY, float yAxisEndY,
            int dataPoints, float groupWidth, float groupSpacing)
        {
            // 创建渐变色
            var gradientStart = Colors.LightGray.WithAlpha(0.1f);  // 顶部更浅的颜色
            var gradientEnd = Colors.LightGray.WithAlpha(0.3f);    // 底部稍深的颜色

            // 遍历每个数据点绘制背景
            for (int i = 0; i < dataPoints; i++)
            {
                // 计算当前组的起始X坐标
                float groupStartX = yAxisX + _scrollOffset + (i * (groupWidth + groupSpacing));
                
                // 只绘制可见区域内的背景
                if (groupStartX + groupWidth <= yAxisX || groupStartX >= xAxisEndX)
                    continue;

                // 确保不超出坐标系边界
                float drawStartX = Math.Max(groupStartX, yAxisX);
                float drawEndX = Math.Min(groupStartX + groupWidth, xAxisEndX);
                float drawWidth = drawEndX - drawStartX;

                // 创建渐变背景
                var gradient = new LinearGradientPaint
                {
                    StartColor = gradientStart,
                    EndColor = gradientEnd,
                    StartPoint = new Point(0, 0),    // 渐变起点（顶部）
                    EndPoint = new Point(0, 1)       // 渐变终点（底部）
                };

                // 绘制背景矩形
                canvas.SetFillPaint(gradient, new RectF(drawStartX, yAxisStartY, drawWidth, yAxisEndY - yAxisStartY));
                canvas.FillRectangle(drawStartX, yAxisStartY, drawWidth, yAxisEndY - yAxisStartY);
            }
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {            
            // 基本检查
            if (DataSeries == null || DataSeries.Count == 0 || XLabels == null || XLabels.Count == 0)
            {
                return;
            }

            // 保存画布状态
            canvas.SaveState();

            // 设置基本参数
            _chartPadding = 20;

            float chartWidth = dirtyRect.Width - (2 * _chartPadding) - Y_AXIS_LABEL_WIDTH;
            
            // 计算Y轴最大值
            float maxValue;
            if (_autoScale && DataSeries.Any())
            {
                maxValue = (float)DataSeries.SelectMany(series => series).Max();
                maxValue = (float)Math.Ceiling(maxValue / 100) * 100;
            }
            else
            {
                maxValue = (float)_maxValue;
            }

            // 修改坐标系的高度，为图例留出空间
            float legendHeight = 30f;  // 图例所需的高度
            float chartHeight = dirtyRect.Height - (2 * _chartPadding) - legendHeight;  // 减去图例高度
            
            // 计算坐标系的边界（注意Y轴方向）
            float yAxisStartY = dirtyRect.Height - _chartPadding - legendHeight;  // Y轴起点（底部）
            float yAxisEndY = _chartPadding;                                      // Y轴终点（顶部）
            float yAxisX = _chartPadding + Y_AXIS_LABEL_WIDTH;
            float xAxisEndX = dirtyRect.Width - _chartPadding;

            int dataPoints = XLabels.Count;
            int seriesCount = DataSeries.Count;
            float groupSpacing = _barWidth;
            float barSpacing = _barWidth * 0.2f;
            float groupWidth = (seriesCount * _barWidth) + ((seriesCount - 1) * barSpacing);

            // 先绘制背景
            DrawBackgrounds(canvas, dirtyRect, yAxisX, xAxisEndX, yAxisStartY, yAxisEndY,
                dataPoints, groupWidth, groupSpacing);

            // 绘制网格
            DrawGrid(canvas, dirtyRect, yAxisX, xAxisEndX, yAxisStartY, yAxisEndY, maxValue);

            // 绘制坐标轴
            canvas.StrokeColor = TextColor;  // 使用 TextColor 绘制坐标轴
            canvas.StrokeSize = 2;

            // Y轴
            canvas.DrawLine(yAxisX, yAxisStartY, yAxisX, yAxisEndY);
            // X轴
            canvas.DrawLine(yAxisX, yAxisStartY, xAxisEndX, yAxisStartY);

            // 绘制坐标轴末端圆点
            canvas.FillColor = TextColor;  // 使用 TextColor 绘制端点
            float dotRadius = 4f;
            canvas.FillCircle(yAxisX, yAxisEndY, dotRadius);
            canvas.FillCircle(xAxisEndX, yAxisStartY, dotRadius);

            // 绘制网格线
            canvas.StrokeColor = TextColor.WithAlpha(0.2f);  // 使用半透明的 TextColor 绘制网格线
            canvas.StrokeSize = 1;

            // 绘制Y轴刻度和标签
            canvas.StrokeColor = TextColor;
            canvas.FontColor = TextColor;
            for (int i = 0; i <= _yAxisSteps; i++)
            {
                float y = yAxisStartY - (i * chartHeight / _yAxisSteps);
                float value = maxValue * i / _yAxisSteps;
                
                // 刻度线
                canvas.DrawLine(yAxisX - 5, y, yAxisX, y);
                
                canvas.DrawString(
                    $"{value:F0}",
                    yAxisX - 8,
                    y - 6,
                    HorizontalAlignment.Right
                );
            }

            // 绘制数据柱子
            for (int i = 0; i < dataPoints; i++)
            {
                float groupStartX = yAxisX + _scrollOffset + (i * (groupWidth + groupSpacing));

                // 检查整个组是否完全在可视区域内
                if (groupStartX + groupWidth <= yAxisX || groupStartX >= xAxisEndX)
                    continue;

                for (int j = 0; j < seriesCount; j++)
                {
                    float barX = groupStartX + (j * (_barWidth + barSpacing));
                    float value = (float)DataSeries[j][i];
                    float barHeight = value * chartHeight / maxValue;

                    // 确保柱子完全在可视区域内
                    if (barX < yAxisX || barX + _barWidth > xAxisEndX)
                        continue;

                    float barTop = yAxisStartY - barHeight;
                    float cornerRadius = Math.Min(_barWidth * 0.2f, barHeight * 0.2f);

                    // 创建从底到顶的渐变
                    var gradient = new LinearGradientPaint
                    {
                        StartColor = SeriesColors[j],                    // 底部颜色更深
                        EndColor = SeriesColors[j].WithAlpha(0.6f),     // 顶部颜色更浅
                        StartPoint = new Point(0, 1),                    // 底部
                        EndPoint = new Point(0, 0)                       // 顶部
                    };

                    // 绘制主体部分
                    canvas.SetFillPaint(gradient, new RectF(barX, barTop, _barWidth, barHeight));

                    // 先绘制主体矩形
                    canvas.FillRectangle(barX, barTop + cornerRadius, _barWidth, barHeight - cornerRadius);

                    // 绘制顶部圆角部分
                    canvas.FillEllipse(barX, barTop, _barWidth, cornerRadius * 2);

                    // 在柱子上方显示数值时使用正确的颜色
                    canvas.FontColor = TextColor;
                    if (barX + _barWidth / 2 >= yAxisX && barX + _barWidth / 2 <= xAxisEndX)
                    {
                        canvas.DrawString(
                            $"{value:F0}",
                            barX + _barWidth / 2,
                            yAxisStartY - barHeight - 15,
                            HorizontalAlignment.Center
                        );
                    }
                }

                // X轴标签也使用正确的颜色
                canvas.FontColor = TextColor;
                if (groupStartX + groupWidth/2 >= yAxisX && groupStartX + groupWidth/2 <= xAxisEndX)
                {
                    canvas.DrawString(
                        XLabels[i],
                        groupStartX + groupWidth / 2,
                        yAxisStartY + 15,
                        HorizontalAlignment.Center
                    );
                }
            }

            // 修改图例绘制方法
            DrawLegend(canvas, dirtyRect, yAxisX, xAxisEndX);

            // 确保在绘制完成后恢复状态
            canvas.RestoreState();
        }

        private void DrawGrid(ICanvas canvas, RectF dirtyRect, 
            float yAxisX, float xAxisEndX, float yAxisStartY, float yAxisEndY, float maxValue)
        {
            // 设置网格线样式
            canvas.StrokeColor = TextColor.WithAlpha(0.2f);  // 使用半透明的 TextColor
            canvas.StrokeSize = 1;
            float chartHeight = yAxisStartY - yAxisEndY;

            // 绘制水平网格线（与Y轴刻度对应）
            for (int i = 1; i < _yAxisSteps; i++)
            {
                float y = yAxisStartY - (i * chartHeight / _yAxisSteps);
                canvas.DrawLine(yAxisX, y, xAxisEndX, y);
            }

            // 绘制垂直网格线（跟随柱子）
            int dataPoints = XLabels.Count;
            int seriesCount = DataSeries.Count;
            float groupSpacing = _barWidth;
            float barSpacing = _barWidth * 0.2f;
            float groupWidth = (seriesCount * _barWidth) + ((seriesCount - 1) * barSpacing);

            for (int i = 0; i < dataPoints; i++)
            {
                float groupStartX = yAxisX + _scrollOffset + (i * (groupWidth + groupSpacing));
                
                // 只绘制可见区域内的垂直网格线
                if (groupStartX >= yAxisX && groupStartX <= xAxisEndX)
                {
                    canvas.DrawLine(
                        groupStartX,
                        yAxisStartY,
                        groupStartX,
                        yAxisEndY
                    );
                }

                // 在每组数据中间也画一条网格线
                float groupMiddleX = groupStartX + groupWidth;
                if (groupMiddleX >= yAxisX && groupMiddleX <= xAxisEndX)
                {
                    canvas.DrawLine(
                        groupMiddleX,
                        yAxisStartY,
                        groupMiddleX,
                        yAxisEndY
                    );
                }
            }
        }

        // 修改图例绘制方法
        private void DrawLegend(ICanvas canvas, RectF dirtyRect, float yAxisX, float xAxisEndX)
        {
            if (LegendLabels == null || LegendLabels.Count == 0) return;

            float legendY = dirtyRect.Height - _chartPadding;     // 图例Y坐标
            float circleRadius = 8f;                              // 增大圆圈半径
            float textOffset = 14f;                               // 增加文字与圆圈的间距
            float itemSpacing = 100f;                            // 增加图例项之间的间距

            // 计算所有图例项的总宽度
            float totalWidth = (LegendLabels.Count * itemSpacing) - (itemSpacing - textOffset);

            // 计算起始X坐标，使图例居中显示
            float startX = yAxisX + ((xAxisEndX - yAxisX) - totalWidth) / 2;

            // 设置文字样式
            canvas.FontSize = 14;
            canvas.FontColor = TextColor;  // 使用正确的文字颜色

            // 绘制每个图例项
            for (int i = 0; i < Math.Min(LegendLabels.Count, SeriesColors.Count); i++)
            {
                float x = startX + (i * itemSpacing);
                
                // 绘制颜色圆圈
                canvas.FillColor = SeriesColors[i];
                canvas.FillCircle(x, legendY, circleRadius);

                Rect rect = new() { X = x + textOffset, Y = legendY- circleRadius, Size=new(itemSpacing, 14) };
                // 绘制文字标签（与圆心垂直对齐）
                canvas.DrawString(
                    LegendLabels[i],
                    rect,
                    HorizontalAlignment.Left,
                    VerticalAlignment.Center
                );
            }
        }

        partial void OnTextColorChanged(Color value)
        {
            (_graphicsView as IGraphicsView)?.Invalidate();
        }
    }
}
