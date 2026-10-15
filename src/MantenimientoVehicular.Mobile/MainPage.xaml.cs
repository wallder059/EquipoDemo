using MantenimientoVehicular.Mobile.ViewModels;

namespace MantenimientoVehicular.Mobile;

public partial class MainPage : ContentPage
{
    public MainPage(HistorialMantenimientoViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is HistorialMantenimientoViewModel viewModel)
        {
            await viewModel.LoadAsync();
        }
    }
}
