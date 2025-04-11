using KupaKuper_MauiControl.ControlModes;

using System.Collections.ObjectModel;

using BeforeDraw = KupaKuper_MauiControl.ControlModes.ScrollGraphicsDrawable.BeforeDraw;
using delDraw = KupaKuper_MauiControl.ControlModes.ScrollGraphicsDrawable.delDraw;

namespace KupaKuper_MauiControl.Controls
{
    public abstract partial class ScrollGraphicsView : GraphicsView
    {
        #region 必要属性
        /// <summary>
        /// 项目集合
        /// </summary>
        private ObservableCollection<object>? _items;
        public ObservableCollection<object> Items
        {
            get => _items ?? new ObservableCollection<object>();
            set
            {
                if (_items != value)
                {
                    _items = value;
                    
                    // 直接更新 graphicsDrawable.Items
                    if (graphicsDrawable != null)
                    {
                        graphicsDrawable.Items = value;
                        
                        // 更新布局并重绘
                        graphicsDrawable.UpdateLayout(Width > 0 ? (float)Width : 800);
                        this.Invalidate();
                    }
                }
            }
        }
        /// <summary>
        /// 项目宽度
        /// </summary>
        private float _itemWidth;
        public float ItemWidth
        {
            get => _itemWidth;
            set
            {
                if (_itemWidth != value)
                {
                    _itemWidth = value;
                    // 更新绘制对象中的值
                    if (graphicsDrawable != null)
                    {
                        graphicsDrawable.ItemWidth = value;
                        // 重新计算布局
                        graphicsDrawable.UpdateLayout(Width > 0 ? (float)Width : 800);
                        this.Invalidate();
                    }
                }
            }
        }
        /// <summary>
        /// 项目高度
        /// </summary>
        private float _itemHeight;
        public float ItemHeight
        {
            get => _itemHeight;
            set
            {
                if (_itemHeight != value)
                {
                    _itemHeight = value;
                    // 更新绘制对象中的值
                    if (graphicsDrawable != null)
                    {
                        graphicsDrawable.ItemHeight = value;
                        this.Invalidate();
                    }
                }
            }
        }
        /// <summary>
        /// 绘制前的准备方法,不用可以为空
        /// </summary>
        public abstract void GraphicsDrawable_BeforeDrawItem();
        /// <summary>
        /// 绘制单独一项的布局,使用IsButtonPressed获取项按钮状态,使用AddButtonRectF在绘制按钮后更新按钮位置
        /// </summary>
        /// <param name="canvas"> 绘制实现的接口 </param>
        /// <param name="x"> 绘制的起点x </param>
        /// <param name="y"> 绘制的起点y </param>
        /// <param name="item"> 绘制的项 </param>
        /// <param name="index"> 绘制的第几项数据 </param>
        public abstract void GraphicsDrawable_DrawItem(ICanvas canvas, float x, float y, float Width, float Height, object item, int index);
        #endregion

        #region 属性实现
        /// <summary>
        /// 项目横向间隔
        /// </summary>
        private float _itemMarginX;
        public float ItemMarginX
        {
            get => _itemMarginX;
            set
            {
                if (_itemMarginX != value)
                {
                    _itemMarginX = value;
                    if (graphicsDrawable != null)
                    {
                        graphicsDrawable.ItemMarginX = value;
                        this.Invalidate();
                    }
                }
            }
        }
        /// <summary>
        /// 项目竖向间隔
        /// </summary>
        private float _itemMarginY;
        public float ItemMarginY
        {
            get => _itemMarginY;
            set
            {
                if (_itemMarginY != value)
                {
                    _itemMarginY = value;
                    if (graphicsDrawable != null)
                    {
                        graphicsDrawable.ItemMarginY = value;
                        this.Invalidate();
                    }
                }
            }
        }
        /// <summary>
        /// 实现绘制效果的绘制类
        /// </summary>
        public ScrollGraphicsDrawable graphicsDrawable { get; set; }
        /// <summary>
        /// 有按钮触发时执行的方法
        /// </summary>
        public required ButtonClick? buttonClick { get; set; }
        #endregion
       

        public ScrollGraphicsView()
        {
            // 创建委托实例
            _drawItemDelegate = new ScrollGraphicsDrawable.delDraw(InternalDrawItem);
            _beforeDrawItemDelegate = new ScrollGraphicsDrawable.BeforeDraw(InternalBeforeDrawItem);
            
            // 初始化绘图对象
            graphicsDrawable = new()
            {
                Items = new ObservableCollection<object>(),
                ViewportWidth = this.Width > 0 ? this.Width : 800,
                ViewportHeight = this.Height > 0 ? this.Height : 600,
                ItemWidth = this.ItemWidth,
                ItemHeight = this.ItemHeight,
                ItemMarginX = this.ItemMarginX,
                ItemMarginY = this.ItemMarginY
            };
            
            // 使用强引用委托注册事件
            graphicsDrawable.DrawItem += _drawItemDelegate;
            graphicsDrawable.BeforeDrawItem += _beforeDrawItemDelegate;
            //方法绑定
            this.SizeChanged += ScrollGraphicsView_SizeChanged;
            this.StartInteraction += OnGraphicsViewStartInteraction;
            this.DragInteraction += OnGraphicsViewDragInteraction;
            this.EndInteraction += OnGraphicsViewEndInteraction;
            this.Drawable = graphicsDrawable;
            // 初始化滚动计时器
            _scrollTimer = Dispatcher.CreateTimer();
            _scrollTimer.Interval = TimeSpan.FromMilliseconds(16); // 约60fps
            _scrollTimer.Tick += OnScrollTimerTick;
            // 添加鼠标滚轮事件处理
            this.HandlerChanged += OnGraphicsViewHandlerChanged;
        }
        
        // 内部委托方法
        private void InternalBeforeDrawItem()
        {
            GraphicsDrawable_BeforeDrawItem();
        }
        
        private void InternalDrawItem(ICanvas canvas, float x, float y, float Width, float Height, object item, int index)
        {
            GraphicsDrawable_DrawItem(canvas, x, y, Width, Height, item, index);
        }

        /// <summary>
        /// 绘制区域改变时需要更新绘制
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollGraphicsView_SizeChanged(object? sender, EventArgs e)
        {
            // 延迟加载避免cylinderGraphicsView实际尺寸读取错误
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(100), () =>
            {
                try
                {
                    // 确保视图尺寸有效
                    if (Width <= 0 || Height <= 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"警告: 无效的尺寸 Width={Width}, Height={Height}");
                        return;
                    }

                    // 获取GraphicsView的实际尺寸
                    double viewWidth = this.Width;
                    double viewHeight = this.Height;

                    // 如果GraphicsView尺寸无效，使用ContentView的尺寸
                    if (viewWidth <= 0 || viewHeight <= 0)
                    {
                        // 考虑到Grid的列定义，右侧区域宽度约为总宽度减去左侧菜单宽度
                        viewWidth = Width - 160; // 假设左侧菜单宽度约为160
                        viewHeight = Height;
                    }

                    // 更新视口尺寸
                    graphicsDrawable.ViewportWidth = viewWidth;
                    graphicsDrawable.ViewportHeight = viewHeight;

                    // 更新布局
                    graphicsDrawable.UpdateLayout((float)viewWidth);

                    // 强制重绘
                    this.Invalidate();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"尺寸变化处理错误: {ex.Message}");
                }
            });
        }

        /// <summary>
        /// 按钮按下触发的方法
        /// </summary>
        /// <param name="Item"> 对应按钮触发的所属项 </param>
        /// /// <param name="buttonIndex"> 触发的按钮在项中的序号 </param>
        public delegate void ButtonClick(object Item,int buttonIndex);
        #region 鼠标滚动处理(Windows)
        // 鼠标滚轮滚动速度
        private const double WheelScrollSpeed = 40.0;
        /// <summary>
        /// windows系统使用的鼠标滚轮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGraphicsViewHandlerChanged(object? sender, EventArgs e)
        {
            // 确保在Windows平台上添加鼠标滚轮事件
        #if WINDOWS
            if (this.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement element)
            {
                element.PointerWheelChanged += OnPointerWheelChanged;
            }
        #endif
        }
#if WINDOWS
            private void OnPointerWheelChanged(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
            {
                if(graphicsDrawable.Items==null || graphicsDrawable.Items.Count == 0)
                {
                    return;
                }
                // 获取鼠标滚轮增量
                var pointerPoint = e.GetCurrentPoint(sender as Microsoft.UI.Xaml.UIElement);
                var delta = pointerPoint.Properties.MouseWheelDelta;
        
                // 计算滚动量（滚轮向上为正，向下为负）
                double scrollDelta = delta / 120.0 * WheelScrollSpeed;
        
                // 更新滚动位置
                _scrollY -= scrollDelta;
                _scrollY = Math.Max(0, _scrollY);
        
                // 计算最大滚动位置
                double totalItemHeight = graphicsDrawable.ItemHeight + graphicsDrawable.ItemMarginY * 2;
                int itemsPerRow = graphicsDrawable.Columns;
                int totalRows = (int)Math.Ceiling((double)graphicsDrawable.Items.Count / itemsPerRow) - (int)(graphicsDrawable.ViewportHeight / totalItemHeight);
                double contentHeight = totalRows * totalItemHeight;
                double maxScrollY = Math.Max(0, contentHeight);
                _scrollY = Math.Min(_scrollY, maxScrollY);
        
                // 更新绘图
                graphicsDrawable.ScrollY = _scrollY;
                this.Invalidate();
        
                // 标记事件已处理
                e.Handled = true;
            }
#endif
        #endregion
        #region 滚动处理及点击处理
        // 滚动相关变量
        private bool _isDragging = false;
        private double _lastTouchY = 0;
        private double _scrollY = 0;
        private double _scrollVelocity = 0;
        private IDispatcherTimer _scrollTimer;
        private delDraw _drawItemDelegate;
        private BeforeDraw _beforeDrawItemDelegate;

        private void OnGraphicsViewStartInteraction(object? sender, TouchEventArgs e)
        {
            if (graphicsDrawable.Items == null || graphicsDrawable.Items.Count == 0)
            {
                return;
            }
            try
            {
                if (e.Touches.Count() == 0) return;

                // 获取点击位置
                var touchPoint = e.Touches[0];
                var point = new Point(touchPoint.X, touchPoint.Y);

                // 执行点击测试
                var (itemIndex, buttonIndex) = graphicsDrawable.HitTest(point);

                // 重置所有按钮状态，设置按下状态
                if (itemIndex >= 0 && buttonIndex >= 0)
                {
                    graphicsDrawable.SetButtonPressed(itemIndex, buttonIndex, true);
                    this.Invalidate(); // 重绘以显示按下效果
                    // 如果点击到了按钮
                    if (itemIndex < graphicsDrawable.Items.Count)
                    {
                        var cylinder = graphicsDrawable.Items[itemIndex];
                        // 根据按钮索引执行相应操作
                        buttonClick?.Invoke(cylinder, buttonIndex);
                    }
                }

                // 继续处理滚动
                _isDragging = true;
                _lastTouchY = touchPoint.Y;
                _scrollVelocity = 0;
                _scrollTimer.Stop();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"按钮按下处理错误: {ex.Message}");
            }
        }

        private void OnGraphicsViewDragInteraction(object? sender, TouchEventArgs e)
        {
            if (graphicsDrawable.Items == null || graphicsDrawable.Items.Count == 0)
            {
                return;
            }
            if (!_isDragging || e.Touches.Count() == 0) return;

            double currentY = e.Touches[0].Y;
            double deltaY = currentY - _lastTouchY;

            // 更新滚动位置
            _scrollY -= deltaY;
            _scrollY = Math.Max(0, _scrollY);

            // 计算最大滚动位置（内容高度减去视口高度）
            double totalItemHeight = graphicsDrawable.ItemHeight + graphicsDrawable.ItemMarginY * 2;
            int itemsPerRow = graphicsDrawable.Columns;
            int totalRows = (int)Math.Ceiling((double)graphicsDrawable.Items.Count / itemsPerRow) - (int)(graphicsDrawable.ViewportHeight / totalItemHeight);
            double contentHeight = totalRows * totalItemHeight;
            double maxScrollY = Math.Max(0, contentHeight);
            _scrollY = Math.Min(_scrollY, maxScrollY);
            if (_scrollY >= maxScrollY) _scrollVelocity = 0;
            // 更新滚动速度
            _scrollVelocity = deltaY;

            // 更新绘图
            graphicsDrawable.ScrollY = _scrollY;
            this.Invalidate();

            _lastTouchY = currentY;
        }

        private void OnGraphicsViewEndInteraction(object? sender, TouchEventArgs e)
        {
            if (graphicsDrawable.Items == null || graphicsDrawable.Items.Count == 0)
            {
                return;
            }
            // 重置所有按钮状态
            for (int i = 0; i < graphicsDrawable.Items.Count; i++)
            {
                graphicsDrawable.SetButtonPressed(i, 0, false);
                graphicsDrawable.SetButtonPressed(i, 1, false);
            }
            this.Invalidate();

            _isDragging = false;

            // 如果有滚动速度，启动惯性滚动
            if (Math.Abs(_scrollVelocity) > 1)
            {
                _scrollTimer.Start();
            }
        }

        private void OnScrollTimerTick(object? sender, EventArgs e)
        {
            if(graphicsDrawable.Items==null || graphicsDrawable.Items.Count == 0)
            {
                return;
            }
            // 应用惯性滚动
            _scrollY -= _scrollVelocity;
            _scrollY = Math.Max(0, _scrollY);

            // 计算最大滚动位置
            double totalItemHeight = graphicsDrawable.ItemHeight + graphicsDrawable.ItemMarginY * 2;
            int itemsPerRow = graphicsDrawable.Columns;
            int totalRows = (int)Math.Ceiling((double)graphicsDrawable.Items.Count / itemsPerRow) - (int)(graphicsDrawable.ViewportHeight / totalItemHeight);
            double contentHeight = totalRows * totalItemHeight;
            double maxScrollY = Math.Max(0, contentHeight);
            _scrollY = Math.Min(_scrollY, maxScrollY);
            if (_scrollY >= maxScrollY) _scrollTimer.Stop();
            // 减小滚动速度（摩擦力）
            if (!(Math.Abs(_scrollVelocity) < 2)) _scrollVelocity *= 0.95;

            // 更新绘图
            graphicsDrawable.ScrollY = _scrollY;
            this.Invalidate();

            // 减小滚动速度（摩擦力）
            if (!(Math.Abs(_scrollVelocity) < 4)) _scrollVelocity *= 0.95;

            // 当速度足够小时停止滚动
            if (Math.Abs(_scrollVelocity) < 4)
            {
                int agern = Math.Min(totalRows, _scrollY % totalItemHeight > 0 ? (int)(_scrollY / totalItemHeight) + 1 : (int)(_scrollY / totalItemHeight));
                double d = agern * totalItemHeight;
                if (Math.Abs(_scrollY - d) < 2)
                {
                    _scrollTimer.Stop();
                    _scrollY = d;
                }
                else if (Math.Abs(_scrollY - d) < 6)
                {
                    if (!(Math.Abs(_scrollVelocity) < 3)) _scrollVelocity *= 0.7;
                }
            }
        }
        #endregion

    }
}
