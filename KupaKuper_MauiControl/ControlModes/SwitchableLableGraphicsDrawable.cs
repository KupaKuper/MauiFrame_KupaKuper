namespace KupaKuper_MauiControl.ControlModes
{
    public class SwitchableLableGraphicsDrawable : IDrawable
    {
        private readonly SwitchableLableViewMode _viewMode;
        public List<RectF> _labelRects = new List<RectF>();
        private GraphicsView _viewsLabel;
        /// <summary>
        /// 指示器的位置
        /// </summary>
        public float indicatorX = 0;
        /// <summary>
        /// 指示器当前指示的标签序号
        /// </summary>
        public uint indicatorNum = 1;

        public SwitchableLableGraphicsDrawable(SwitchableLableViewMode viewMode, GraphicsView viewsLabel)
        {
            _viewMode = viewMode;
            _viewsLabel = viewsLabel;
            GraphicsDrawable_BeforeDrawItem();
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            DrawLabels(canvas, dirtyRect);
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
        /// 绘制前先从字典读取需要的颜色
        /// </summary>
        public void GraphicsDrawable_BeforeDrawItem()
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

        #region 绘制参数
        public float textSize { get; set; } = 14;
        public SizeF textSpring { get; set; } = new(5f, 10f);
        public SizeF lableSpring { get; set; } = new SizeF(5, 6);
        #endregion
        /// <summary>
        /// 绘制标签栏
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="dirtyRect"></param>
        private void DrawLabels(ICanvas canvas, RectF dirtyRect)
        {
            _labelRects.Clear();
            float x = lableSpring.Width;
            float y = 0;
            foreach (var item in _viewMode.ListViewName)
            {
                var labelSize = canvas.GetStringSize(item, Microsoft.Maui.Graphics.Font.Default, textSize) + textSpring;
                var rect = new RectF(x, lableSpring.Width/2, labelSize.Width, labelSize.Height);
                _labelRects.Add(rect);
                x += rect.Width + lableSpring.Width;
                y = rect.Height + lableSpring.Height;
            }
            _viewMode.LableHeight = y;
            // 绘制项背景
            var rectA = new RectF(0, 0, x, y);
            canvas.SetFillPaint(linearGradientPaint, rectA);
            canvas.FillRoundedRectangle(rectA, 8);
            // 绘制圆角矩形边框
            canvas.StrokeColor = Colors.Gray;
            canvas.StrokeSize = 1;
            canvas.DrawRoundedRectangle(rectA, 8);
            // 绘制指示器
            canvas.FillColor = blueColor;
            canvas.FillRoundedRectangle(indicatorX==0?lableSpring.Width:indicatorX, lableSpring.Width/2, _labelRects[(int)indicatorNum - 1].Width, _labelRects[(int)indicatorNum - 1].Height, 6);
            // 绘制标签的逻辑
            for (int i = 0; i < _labelRects.Count; i++)
            {
                var viewName = _viewMode.ListViewName[i];
                var rect = _labelRects[i];
                canvas.FontColor = textColor;
                canvas.FontSize = textSize;
                if (indicatorNum - 1 == i) canvas.Font = Microsoft.Maui.Graphics.Font.DefaultBold;
                canvas.DrawString(viewName, rect, HorizontalAlignment.Center, VerticalAlignment.Center);
            }
        }
        public async Task AnimateIndicatorX(float start, float end, double duration)
        {
            double elapsed = 0;
            while (elapsed < duration)
            {
                await Task.Delay(16);
                elapsed += 16;
                float progress = (float)Math.Min(1, elapsed / duration);
                indicatorX = (float)(start + (end - start) * Easing.CubicInOut.Ease(progress));
                _viewsLabel.Invalidate();
            }
            indicatorX = end;
            _viewsLabel.Invalidate();
        }
    }
}
