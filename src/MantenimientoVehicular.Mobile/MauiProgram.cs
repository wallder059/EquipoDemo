using MantenimientoVehicular.Mobile.Services;
using MantenimientoVehicular.Mobile.ViewModels;

namespace MantenimientoVehicular.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>().ConfigureFonts(fonts =>
        {
            fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
        });
#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri(ApiSettings.BaseUrl),
            Timeout = TimeSpan.FromSeconds(15),
        });
        builder.Services.AddSingleton<IVehiculoApiService, VehiculoApiService>();
        builder.Services.AddTransient<HistorialMantenimientoViewModel>();
        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}
