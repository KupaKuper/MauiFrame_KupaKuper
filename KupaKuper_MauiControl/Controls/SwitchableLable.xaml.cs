using KupaKuper_MauiControl.ControlModes;

namespace KupaKuper_MauiControl.Controls
{
    public partial class SwitchableLable : ContentView
    {
        private readonly SwitchableLableViewMode _viewMode;
        private SwitchableLableGraphicsDrawable _graphicsDrawable;
        /// <summary>
        /// 指示器当前指示的标签序号
        /// </summary>
        public uint indicatorNum = 1;
        /// <summary>
        /// 带切换标签的ContentView控件,根据绑定的ContentView控件显示对应页面
        /// </summary>
        public SwitchableLable()
        {
            InitializeComponent();
            BindingContext = _viewMode = new();
            _graphicsDrawable = new (_viewMode, ViewsLable);
            ViewsLable.Drawable = _graphicsDrawable;
        }
        #region 属性
        #region 标签列表
        /// <summary>
        /// 标签数据源
        /// </summary>
        public static readonly BindableProperty ViewSourceProperty =
            BindableProperty.Create(nameof(ViewSource), typeof(List<string>), typeof(SwitchableLable),
                null, propertyChanged: OnViewSourceChanged);

        /// <summary>
        /// 页面列表属性改变时的处理方法
        /// </summary>
        private static void OnViewSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SwitchableLable views = (SwitchableLable)bindable;
            if (newValue != null)
            {
                views._viewMode.ChangeLableList((List<string>)newValue);
            }
        }

        /// <summary>
        /// 页面列表
        /// </summary>
        public List<string> ViewSource
        {
            get => (List<string>)GetValue(ViewSourceProperty);
            set
            {
                SetValue(ViewSourceProperty, value);
                Dispatcher.Dispatch(() =>
                {
                    // 触发重绘
                    ViewsLable.Invalidate();
                });
            }
        }
        #endregion
        #region 标签文字大小
        /// <summary>
        /// 标签文字大小
        /// </summary>
        public static readonly BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(float), typeof(SwitchableLable),
                null, propertyChanged: OnFontSizeChanged);

        /// <summary>
        /// 页面列表属性改变时的处理方法
        /// </summary>
        private static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SwitchableLable views = (SwitchableLable)bindable;
            if (newValue != null)
            {
                views._graphicsDrawable.textSize = views.FontSize;
                // 触发重绘
                views.ViewsLable.Invalidate();
            }
        }

        /// <summary>
        /// 标签文字大小
        /// </summary>
        public float FontSize
        {
            get => (float)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }
        #endregion
        #region 标签文字间距
        /// <summary>
        /// 标签文字横向间距
        /// </summary>
        public static readonly BindableProperty TextXSpringProperty =
            BindableProperty.Create(nameof(TextXSpring), typeof(float), typeof(SwitchableLable),
                null, propertyChanged: OnTextXSpringChanged);

        /// <summary>
        /// 页面列表属性改变时的处理方法
        /// </summary>
        private static void OnTextXSpringChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SwitchableLable views = (SwitchableLable)bindable;
            if (newValue != null)
            {
                views._graphicsDrawable.textSpring = new SizeF(views.TextXSpring, views.TextYSpring);
                // 触发重绘
                views.ViewsLable.Invalidate();
            }
        }

        /// <summary>
        /// 标签文字横向间距
        /// </summary>
        public float TextXSpring
        {
            get => (float)GetValue(TextXSpringProperty);
            set => SetValue(TextXSpringProperty, value);
        }

        /// <summary>
        /// 标签文字竖向间距
        /// </summary>
        public static readonly BindableProperty TextYSpringProperty =
            BindableProperty.Create(nameof(TextYSpring), typeof(float), typeof(SwitchableLable),
                null, propertyChanged: OnTextYSpringChanged);

        /// <summary>
        /// 页面列表属性改变时的处理方法
        /// </summary>
        private static void OnTextYSpringChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SwitchableLable views = (SwitchableLable)bindable;
            if (newValue != null)
            {
                views._graphicsDrawable.textSpring = new SizeF(views.TextXSpring,views.TextYSpring);
                // 触发重绘
                views.ViewsLable.Invalidate();
            }
        }

        /// <summary>
        /// 标签文字竖向间距
        /// </summary>
        public float TextYSpring
        {
            get => (float)GetValue(TextYSpringProperty);
            set => SetValue(TextYSpringProperty, value);
        }
        #endregion
        #region 标签间距
        /// <summary>
        /// 标签横向间距
        /// </summary>
        public static readonly BindableProperty LableXSpringProperty =
            BindableProperty.Create(nameof(LableXSpring), typeof(float), typeof(SwitchableLable),
                null, propertyChanged: OnLableXSpringChanged);

        /// <summary>
        /// 页面列表属性改变时的处理方法
        /// </summary>
        private static void OnLableXSpringChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SwitchableLable views = (SwitchableLable)bindable;
            if (newValue != null)
            {
                views._graphicsDrawable.lableSpring = new SizeF(views.LableXSpring, views.LableYSpring);
                // 触发重绘
                views.ViewsLable.Invalidate();
            }
        }

        /// <summary>
        /// 标签横向间距
        /// </summary>
        public float LableXSpring
        {
            get => (float)GetValue(LableXSpringProperty);
            set => SetValue(LableXSpringProperty, value);
        }

        /// <summary>
        /// 标签竖向间距
        /// </summary>
        public static readonly BindableProperty LableYSpringProperty =
            BindableProperty.Create(nameof(LableYSpring), typeof(float), typeof(SwitchableLable),
                null, propertyChanged: OnLableYSpringChanged);

        /// <summary>
        /// 页面列表属性改变时的处理方法
        /// </summary>
        private static void OnLableYSpringChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SwitchableLable views = (SwitchableLable)bindable;
            if (newValue != null)
            {
                views._graphicsDrawable.lableSpring = new SizeF(views.LableXSpring, views.LableYSpring);
                // 触发重绘
                views.ViewsLable.Invalidate();
            }
        }

        /// <summary>
        /// 标签竖向间距
        /// </summary>
        public float LableYSpring
        {
            get => (float)GetValue(TextYSpringProperty);
            set => SetValue(TextYSpringProperty, value);
        }
        #endregion
        public delegate void ChangeLable(uint viewIndex, string viewName);
        /// <summary>
        /// 切换页面时的触发事件
        /// </summary>
        public event ChangeLable? LableChanged;

        public void ChangeLableIndex(uint viewIndex)
        {
           LableChanged?.Invoke(viewIndex, _viewMode.ListViewName[(int)viewIndex - 1]);
        }

        #endregion 属性

        #region 点击和动画指示器
        /// <summary>
        /// 点击触发
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void OnLabelTapped(object sender, TappedEventArgs e)
        {
            var point = e.GetPosition(ViewsLable);
            for (int i = 0; i < _graphicsDrawable._labelRects.Count; i++)
            {
                if (_graphicsDrawable._labelRects[i].Contains(point.Value))
                {
                    ChangeLableIndex((uint)i + 1);
                    uint targetIndicatorNum = (uint)i + 1;
                    await AnimateIndicator(targetIndicatorNum);
                    ViewsLable.Invalidate();
                    break;
                }
            }
        }
        /// <summary>
        /// 动画指示器执行
        /// </summary>
        /// <param name="targetIndicatorNum"></param>
        /// <returns></returns>
        private async Task AnimateIndicator(uint targetIndicatorNum)
        {
            float start = _graphicsDrawable._labelRects[(int)_graphicsDrawable.indicatorNum - 1].X;
            float end = _graphicsDrawable._labelRects[(int)targetIndicatorNum - 1].X;
            double duration = 200; // 动画持续时间，单位：毫秒
            await _graphicsDrawable.AnimateIndicatorX(start, end, duration);
            _graphicsDrawable.indicatorNum = targetIndicatorNum;
        }
        #endregion
    }
}