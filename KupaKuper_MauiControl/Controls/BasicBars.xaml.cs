using KupaKuper_MauiControl.ControlModes;

namespace KupaKuper_MauiControl.Controls
{
    public partial class BasicBars : ContentView
    {
        private readonly BasicBarsViewMode _viewModel;

        public BasicBars()
        {
            InitializeComponent();
            _viewModel = new BasicBarsViewMode(ChartView);
            ChartView.Drawable = _viewModel;

            // 监听主题变化
            Application.Current.RequestedThemeChanged += Current_RequestedThemeChanged;
            
            // 初始设置颜色
            UpdateThemeColors();
        }

        private void Current_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            UpdateThemeColors();
        }

        private void UpdateThemeColors()
        {
            var isDarkTheme = Application.Current.RequestedTheme == AppTheme.Dark;
            TextColor = isDarkTheme ? Colors.White : Colors.Black;
            
            // 更新 ViewModel
            if (_viewModel != null)
            {
                _viewModel.TextColor = TextColor;
                (ChartView as IGraphicsView)?.Invalidate();
            }
        }

        // 确保在控件被释放时取消事件订阅
        ~BasicBars()
        {
            if (Application.Current != null)
            {
                Application.Current.RequestedThemeChanged -= Current_RequestedThemeChanged;
            }
        }

        #region 数据源绑定属性
        public static readonly BindableProperty MaxValueProperty =
            BindableProperty.Create(nameof(MaxValue), typeof(double), typeof(BasicBars),
                defaultValue: 0.0,
                propertyChanged: OnLayoutPropertyChanged);

        public static readonly BindableProperty AutoScaleProperty =
            BindableProperty.Create(nameof(AutoScale), typeof(bool), typeof(BasicBars),
                defaultValue: true,
                propertyChanged: OnLayoutPropertyChanged);

        public static readonly BindableProperty YAxisStepsProperty =
            BindableProperty.Create(nameof(YAxisSteps), typeof(int), typeof(BasicBars),
                defaultValue: 10,
                propertyChanged: OnLayoutPropertyChanged);

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public bool AutoScale
        {
            get => (bool)GetValue(AutoScaleProperty);
            set => SetValue(AutoScaleProperty, value);
        }

        public int YAxisSteps
        {
            get => (int)GetValue(YAxisStepsProperty);
            set => SetValue(YAxisStepsProperty, value);
        }
        #endregion

        #region 数据源绑定属性
        public static readonly BindableProperty DataSeriesProperty =
            BindableProperty.Create(nameof(DataSeries), typeof(List<List<double>>), typeof(BasicBars),
                defaultValue: null,
                propertyChanged: OnDataChanged);

        public static readonly BindableProperty XLabelsProperty =
            BindableProperty.Create(nameof(XLabels), typeof(List<string>), typeof(BasicBars),
                defaultValue: null,
                propertyChanged: OnDataChanged);

        public static readonly BindableProperty SeriesColorsProperty =
            BindableProperty.Create(nameof(SeriesColors), typeof(List<Color>), typeof(BasicBars),
                defaultValue: null,
                propertyChanged: OnDataChanged);

        public static readonly BindableProperty LegendLabelsProperty = BindableProperty.Create(
            nameof(LegendLabels),
            typeof(List<string>),
            typeof(BasicBars),
            null,
            propertyChanged: OnDataChanged);

        public List<List<double>> DataSeries
        {
            get => (List<List<double>>)GetValue(DataSeriesProperty);
            set => SetValue(DataSeriesProperty, value);
        }

        public List<string> XLabels
        {
            get => (List<string>)GetValue(XLabelsProperty);
            set => SetValue(XLabelsProperty, value);
        }

        public List<Color> SeriesColors
        {
            get => (List<Color>)GetValue(SeriesColorsProperty);
            set => SetValue(SeriesColorsProperty, value);
        }

        public List<string> LegendLabels
        {
            get => (List<string>)GetValue(LegendLabelsProperty);
            set => SetValue(LegendLabelsProperty, value);
        }

        private static void OnDataChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BasicBars chart)
            {
                chart._viewModel.UpdateData(chart.DataSeries, chart.XLabels, chart.SeriesColors, chart.LegendLabels);
            }
        }
        #endregion

        #region 布局属性
        public static readonly BindableProperty BarWidthProperty =
            BindableProperty.Create(nameof(BarWidth), typeof(float), typeof(BasicBars),
                defaultValue: 50f,
                propertyChanged: OnLayoutPropertyChanged);

        public static readonly BindableProperty BarSpacingProperty =
            BindableProperty.Create(nameof(BarSpacing), typeof(float), typeof(BasicBars),
                defaultValue: 20f,
                propertyChanged: OnLayoutPropertyChanged);

        public static readonly BindableProperty ChartPaddingProperty =
            BindableProperty.Create(nameof(ChartPadding), typeof(float), typeof(BasicBars),
                defaultValue: 100f,
                propertyChanged: OnLayoutPropertyChanged);

        public float BarWidth
        {
            get => (float)GetValue(BarWidthProperty);
            set => SetValue(BarWidthProperty, value);
        }

        public float BarSpacing
        {
            get => (float)GetValue(BarSpacingProperty);
            set => SetValue(BarSpacingProperty, value);
        }

        public float ChartPadding
        {
            get => (float)GetValue(ChartPaddingProperty);
            set => SetValue(ChartPaddingProperty, value);
        }
        #endregion

        #region 颜色属性
        public static readonly BindableProperty OkBarColorProperty =
            BindableProperty.Create(nameof(OkBarColor), typeof(Color), typeof(BasicBars),
                defaultValue: Colors.DodgerBlue,
                propertyChanged: OnLayoutPropertyChanged);

        public static readonly BindableProperty NgBarColorProperty =
            BindableProperty.Create(nameof(NgBarColor), typeof(Color), typeof(BasicBars),
                defaultValue: Colors.Red,
                propertyChanged: OnLayoutPropertyChanged);

        public Color OkBarColor
        {
            get => (Color)GetValue(OkBarColorProperty);
            set => SetValue(OkBarColorProperty, value);
        }

        public Color NgBarColor
        {
            get => (Color)GetValue(NgBarColorProperty);
            set => SetValue(NgBarColorProperty, value);
        }
        #endregion

        #region 文本颜色属性
        public static readonly BindableProperty TextColorProperty =
            BindableProperty.Create(nameof(TextColor), typeof(Color), typeof(BasicBars),
                defaultValue: Colors.Black,
                propertyChanged: OnLayoutPropertyChanged);

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }
        #endregion

        private static void OnLayoutPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BasicBars chart && chart._viewModel != null)
            {
                // 立即更新 ViewModel 的 TextColor
                chart._viewModel.TextColor = chart.TextColor;
                
                // 触发重绘
                (chart.ChartView as IGraphicsView)?.Invalidate();
                
                // 更新其他属性
                chart._viewModel.UpdateLayout(
                    chart.BarWidth,
                    chart.BarSpacing,
                    chart.ChartPadding,
                    chart.OkBarColor,
                    chart.NgBarColor,
                    chart.MaxValue,
                    chart.AutoScale,
                    chart.YAxisSteps,
                    chart.TextColor
                );
            }
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            _viewModel.OnPanUpdated(sender, e);
        }

        private void OnBackgroundColorChanged()
        {
            // 处理背景色变化
        }
    }
}
