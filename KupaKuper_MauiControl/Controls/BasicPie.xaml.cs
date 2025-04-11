using KupaKuper_MauiControl.ControlModes;

namespace KupaKuper_MauiControl.Controls
{
    public partial class BasicPie : ContentView
    {
        private readonly BasicPieViewMode _viewModel;

        public BasicPie()
        {
            InitializeComponent();
            _viewModel = new BasicPieViewMode(chartView);
            chartView.Drawable = _viewModel;
            
            // 设置默认背景色，确保视图可见
            BackgroundColor = Colors.Transparent;
            chartView.BackgroundColor = Colors.Transparent;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);
            
            // 确保在尺寸改变时重新绘制
            if (width > 0 && height > 0)
            {
                (_viewModel as IDrawable)?.Draw(null, new RectF(0, 0, (float)width, (float)height));
            }
        }

        #region 数据绑定
        public static readonly BindableProperty DataSeriesProperty =
            BindableProperty.Create(nameof(DataSeries), typeof(List<BasicPieDataMode>), typeof(BasicPie),
                defaultValue: null,
                propertyChanged: OnDataChanged);

        public List<BasicPieDataMode> DataSeries
        {
            get => (List<BasicPieDataMode>)GetValue(DataSeriesProperty);
            set => SetValue(DataSeriesProperty, value);
        }

        private static void OnDataChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BasicPie chart)
            {
                // 确保在数据为null时也能正确处理
                var newData = newValue as List<BasicPieDataMode>;
                chart._viewModel.UpdateData(newData ?? new List<BasicPieDataMode>());
            }
        }
        #endregion

        #region 属性绑定
        public static readonly BindableProperty LabelColorProperty =
            BindableProperty.Create(nameof(LabelColor), typeof(Color), typeof(BasicPie),
                Colors.Black, propertyChanged: OnLabelColorChanged);

        public static readonly BindableProperty StrokeColorProperty =
            BindableProperty.Create(nameof(StrokeColor), typeof(Color), typeof(BasicPie),
                Colors.White, propertyChanged: OnStrokeColorChanged);

        public Color LabelColor
        {
            get => (Color)GetValue(LabelColorProperty);
            set => SetValue(LabelColorProperty, value);
        }

        public Color StrokeColor
        {
            get => (Color)GetValue(StrokeColorProperty);
            set => SetValue(StrokeColorProperty, value);
        }

        private static void OnLabelColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BasicPie chart && newValue is Color color)
            {
                chart._viewModel.LabelColor = color;
            }
        }

        private static void OnStrokeColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BasicPie chart && newValue is Color color)
            {
                chart._viewModel.StrokeColor = color;
            }
        }
        #endregion

        protected override void OnHandlerChanged()
        {
            base.OnHandlerChanged();
            UpdateColors();
        }

        private void UpdateColors()
        {
            // 根据当前主题设置颜色
            Application.Current.RequestedThemeChanged += (s, e) =>
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    var isDark = Application.Current.RequestedTheme == AppTheme.Dark;
                    LabelColor = isDark ? Colors.White : Colors.Black;
                    StrokeColor = isDark ? Colors.DarkGray : Colors.White;
                });
            };

            // 初始设置
            var isDark = Application.Current.RequestedTheme == AppTheme.Dark;
            LabelColor = isDark ? Colors.White : Colors.Black;
            StrokeColor = isDark ? Colors.Black : Colors.White;
        }

        private void PanGestureRecognizer_PanUpdated(object sender, PanUpdatedEventArgs e)
        {

        }
    }
}