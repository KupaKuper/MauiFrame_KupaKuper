using MauiHmiFrame_KupaKuper.ViewModes;

namespace MauiHmiFrame_KupaKuper.Views;

public partial class HomeView : ContentView
{
	private readonly HomeViewMode _viewMode;
	public HomeView()
	{
		InitializeComponent();
		BindingContext = _viewMode = new();
	}
}