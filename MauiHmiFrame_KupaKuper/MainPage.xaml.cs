using LocalizationResourceManager.Maui;

namespace MauiHmiFrame_KupaKuper
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageMode _pageMode;
        public MainPage(ILocalizationResourceManager resourceManager)
        {
            InitializeComponent();
            BindingContext = _pageMode = new();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            _pageMode.ChangeUpPage(switchViews);
        }

        private void switchViews_CurrentViewChanged(uint ViewIndex)
        {
            _pageMode.Message = ViewIndex.ToString();
        }
    }

}
