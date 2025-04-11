using CommunityToolkit.Mvvm.ComponentModel;

namespace KupaKuper_MauiControl.ControlModes
{
    public partial class BasicPieViewMode : ObservableObject, IDrawable
    {
        private List<BasicPieDataMode> _dataSeries = new();
        private float _totalValue;
        private float _currentAnimationProgress = 0;  // 动画进度(0-1)
        private IDispatcherTimer _animationTimer;

        private const float StartAngle = 0f;        // 起始角度
        private const float LabelOffset = 20f;        // 标签偏移
        private const float Depth3D = 20f;            // 3D深度效果
        private const float AnimationDuration = 500f; // 动画持续时间(ms)
        private const float AnimationInterval = 16f;   // 动画刷新间隔(ms)

        private static readonly Random _rand = new();
        private static Color RandomColor =>
            Color.FromRgb(_rand.Next(256), _rand.Next(256), _rand.Next(256));

        private readonly IView _graphicsView;

        private Color _labelColor = Colors.Black;
        private Color _strokeColor = Colors.White;

        public Color LabelColor
        {
            get => _labelColor;
            set
            {
                if (_labelColor != value)
                {
                    _labelColor = value;
                    (_graphicsView as IGraphicsView)?.Invalidate();
                }
            }
        }

        public Color StrokeColor
        {
            get => _strokeColor;
            set
            {
                if (_strokeColor != value)
                {
                    _strokeColor = value;
                    (_graphicsView as IGraphicsView)?.Invalidate();
                }
            }
        }

        public BasicPieViewMode(IView graphicsView)
        {
            _graphicsView = graphicsView;

            //初始化动画计时器
           _animationTimer = Application.Current.Dispatcher.CreateTimer();
            _animationTimer.Interval = TimeSpan.FromMilliseconds(AnimationInterval);
            _animationTimer.Tick += OnAnimationTick;

            // 监听视图大小变化事件
            if (_graphicsView is Microsoft.Maui.Controls.View view)
            {
                view.SizeChanged += (s, e) =>
                {
                    if (view.Width > 0)
                    {
                        (_graphicsView as IGraphicsView)?.Invalidate();
                    }
                };
            }
        }

        public void UpdateData(List<BasicPieDataMode> dataSeries)
        {
            _dataSeries = dataSeries ?? new List<BasicPieDataMode>();
            
            // 为没有设置颜色的数据项设置随机颜色
            foreach (var item in _dataSeries)
            {
                if (item.Color == null)
                {
                    item.Color = RandomColor;
                }
            }
            
            // 重置动画状态
            _currentAnimationProgress = 0;
            
            // 如果有数据，启动动画
            if (_dataSeries.Any())
            {
                _animationTimer.Start();
            }
            
            // 检查视图是否已正确初始化
            if (_graphicsView is Microsoft.Maui.Controls.View view && view.Width > 0)
            {
                (_graphicsView as IGraphicsView)?.Invalidate();
            }
            else
            {
                // 如果视图尚未初始化，等待 SizeChanged 事件
                if (_graphicsView is Microsoft.Maui.Controls.View v)
                {
                    v.SizeChanged += OnFirstSizeChanged;
                }
            }
        }

        private void OnFirstSizeChanged(object sender, EventArgs e)
        {
            if (sender is Microsoft.Maui.Controls.View view)
            {
                if (view.Width > 0)
                {
                    view.SizeChanged -= OnFirstSizeChanged;  // 移除事件处理器
                    (_graphicsView as IGraphicsView)?.Invalidate();
                }
            }
        }

        private void OnAnimationTick(object sender, EventArgs e)
        {
            // 更新动画进度
            _currentAnimationProgress += AnimationInterval / AnimationDuration;
            if (_currentAnimationProgress >= 1)
            {
                _currentAnimationProgress = 1;
                _animationTimer.Stop();
            }

            // 触发重绘
            (_graphicsView as IGraphicsView)?.Invalidate();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            // 添加 canvas 空检查
            if (canvas == null) return;

            // 基本检查
            if (_dataSeries == null || _dataSeries.Count == 0)
            {
                return;
            }

            // 计算总值
            _totalValue = (float)_dataSeries.Sum(x => x.Value);
            if (_totalValue <= 0) return;  // 只在总值为0或负数时返回

            // 保存画布状态
            canvas.SaveState();

            // 1. 增加饼图显示边界空白
            float extraPadding = 40; // 额外的空白区域
            float padding = 40 + extraPadding;
            float availableWidth = dirtyRect.Width - (padding * 2);
            float availableHeight = dirtyRect.Height - (padding * 2);
            float diameter = Math.Min(availableWidth, availableHeight);
            float radius = diameter / 2;
        
            RectF pieRect = new RectF(
                dirtyRect.Width / 2 - radius,   // 左边
                dirtyRect.Height / 2 - radius,  // 顶部
                diameter,                       // 宽度
                diameter                        // 高度
            );
        
            float currentAngle = StartAngle;

            // 3. 绘制所有扇形
            foreach (var item in _dataSeries)
            {
                // 确保每个数据项都有颜色
                if (item.Color == null)
                {
                    item.Color = RandomColor;
                }

                float percentage = (float)item.Value / _totalValue;
                float sweepAngle = (percentage * 360f) * _currentAnimationProgress;

                //绘制扇形区域
                PathF pathF = new PathF();
                pathF.MoveTo(pieRect.Center.X, pieRect.Center.Y);
                pathF.AddArc(pieRect.Left, pieRect.Top, pieRect.Right, pieRect.Bottom, currentAngle, Math.Min(currentAngle + sweepAngle,359.5f), false);
                pathF.MoveTo(pieRect.Center.X, pieRect.Center.Y);
                pathF.Close();

                //填充颜色
                canvas.FillColor = item.Color;
                canvas.FillPath(pathF);

                // 绘制边线
                canvas.StrokeColor = _strokeColor;
                canvas.StrokeSize = 3;             // 边线宽度
                canvas.DrawPath(pathF);

                // 计算标签位置
                float midAngle = 360f - (currentAngle + sweepAngle / 2);
                DrawLabel(canvas, pieRect.Center.X, pieRect.Center.Y, radius, midAngle, item, dirtyRect);
                pathF.Dispose();
                currentAngle += sweepAngle;
            }
            if (radius > 30)
            {
                canvas.FillColor = _strokeColor;
                canvas.FillEllipse(pieRect.Center.X - 25, pieRect.Center.Y - 25, 50, 50);
            }
            // 确保在绘制完成后恢复状态
            canvas.RestoreState();

        }

        private void DrawLabel(ICanvas canvas, float centerX, float centerY, float radius, 
                              float angle, BasicPieDataMode item, RectF dirtyRect)
        {
            double radians = angle * Math.PI / 180;
            float labelOffset = radius * 0.2f;

            float labelX = centerX + (radius + labelOffset) * (float)Math.Cos(radians);
            float labelY = centerY + (radius + labelOffset) * (float)Math.Sin(radians);

            // 检查标签位置是否超出控件区域
            bool isOutside = labelX < dirtyRect.Left || labelX > dirtyRect.Right ||
                             labelY < dirtyRect.Top || labelY > dirtyRect.Bottom;

            // 绘制引导线
            float innerX = centerX + radius * 0.8f * (float)Math.Cos(radians);
            float innerY = centerY + radius * 0.8f * (float)Math.Sin(radians);

            if (isOutside)
            {
                // 如果超出区域，使用折线绘制引导线
                float midX = innerX + (labelX - innerX) / 2;
                float midY = innerY + (labelY - innerY) / 2;

                // 调整折线的终点，使其不超出边界
                if (labelX < dirtyRect.Left) labelX = dirtyRect.Left;
                if (labelX > dirtyRect.Right) labelX = dirtyRect.Right;
                if (labelY < dirtyRect.Top) labelY = dirtyRect.Top;
                if (labelY > dirtyRect.Bottom) labelY = dirtyRect.Bottom;

                canvas.StrokeColor = Colors.Gray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(innerX, innerY, midX, midY);
                canvas.DrawLine(midX, midY, labelX, labelY);
            }
            else
            {
                canvas.StrokeColor = Colors.Gray;
                canvas.StrokeSize = 1;
                canvas.DrawLine(innerX, innerY, labelX, labelY);
            }

            // 绘制标签文本
            canvas.FontColor = _labelColor;
            canvas.FontSize = Math.Max(radius * 0.08f, 12);

            string percentage = $"{(item.Value / _totalValue * 100):F1}%";
            string label = $"{item.Name}\n{item.Value}({percentage})";

            HorizontalAlignment hAlign = labelX > centerX ? 
                HorizontalAlignment.Left : HorizontalAlignment.Right;

            canvas.DrawString(label, labelX, labelY, hAlign);
        }
    }
}
