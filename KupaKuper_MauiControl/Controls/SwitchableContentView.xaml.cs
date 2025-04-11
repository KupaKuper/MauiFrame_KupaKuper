using KupaKuper_MauiControl.ControlModes;

namespace KupaKuper_MauiControl.Controls
{
    public partial class SwitchableContentView : ContentView
    {
        public readonly SwitchableContentViewMode _viewMode;

        /// <summary>
        /// 带切换标签的ContentView控件,根据绑定的ContentView控件显示对应页面
        /// </summary>
        public SwitchableContentView()
        {
            InitializeComponent();
            BindingContext = _viewMode = new();
        }
        #region 属性
        private List<string> viewNames = new();

        /// <summary>
        /// 页面列表属性,用于绑定数据源
        /// </summary>
        public static readonly BindableProperty ViewSourceProperty =
            BindableProperty.Create(nameof(ViewSource), typeof(Dictionary<uint, ContentView>), typeof(SwitchableContentView),
                null, propertyChanged: OnViewSourceChanged);

        /// <summary>
        /// 页面列表属性改变时的处理方法
        /// </summary>
        private static void OnViewSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            SwitchableContentView views = (SwitchableContentView)bindable;
            if (newValue != null)
            {
                foreach (var item in views.ViewSource)
                {
                    if (item.Value.BindingContext is BaseViewMode view)
                    {
                        view.ViewIndex = item.Key;
                        views.viewNames.Add(view.ViewName);
                    }
                }
                if (views.ViewSource.Count > 0)
                {
                    views.ChangeViewShow(1);
                }
            }
        }

        /// <summary>
        /// 页面列表
        /// </summary>
        public Dictionary<uint, ContentView> ViewSource
        {
            get => (Dictionary<uint, ContentView>)GetValue(ViewSourceProperty);
            set
            {
                SetValue(ViewSourceProperty, value);
                Dispatcher.Dispatch(() =>
                {
                    lables.ViewSource = viewNames;
                });
            }
        }

        /// <summary>
        /// 当前显示的页面的序号
        /// </summary>
        public uint CurrentViewIndex { get; private set; } = 0;

        public delegate void ChangeView(uint ViewIndex);
        /// <summary>
        /// 页面切换时触发
        /// </summary>
        public event ChangeView CurrentViewChanged;
        #endregion 属性

        private void SwitchableLable_LableChanged(uint viewIndex, string viewName)
        {
            ChangeViewShow(ViewSource.Keys.ToList()[(int)viewIndex - 1]);
        }
        /// <summary>
        /// 切换页面显示
        /// </summary>
        /// <param name="viewIndex"></param>
        public async void ChangeViewShow(uint viewIndex)
        {
            if (CurrentViewIndex != viewIndex)
            {
                CurrentViewIndex = viewIndex;
                if (ViewSource.TryGetValue(viewIndex, out var view))
                {
                    if (_viewMode.CurrentView.BindingContext is IViewVisibleAware oldView)
                    {
                        oldView.CloseViewVisible();
                    }
                    //淡出动画
                    await Task.WhenAny
                        (
                            _viewMode.CurrentView.TranslateTo(-30, 0, 250),
                            _viewMode.CurrentView.FadeTo(0, 250)
                        );
                    _viewMode.CurrentView = view;
                    //淡入动画
                    await Task.WhenAny
                        (
                           view.TranslateTo(0, 0, 250),
                           view.FadeTo(1, 250)
                        );
                    if (view.BindingContext is IViewVisibleAware newView)
                    {
                        newView.OnViewVisible();
                    }
                    CurrentViewChanged.Invoke(CurrentViewIndex);
                }
            }
            else
            {
                if (ViewSource.TryGetValue(viewIndex, out var view))
                {
                    if (view.BindingContext is BaseViewMode oldView)
                    {
                        oldView.UpdataView();
                    }
                }
            }
        }

    }
}