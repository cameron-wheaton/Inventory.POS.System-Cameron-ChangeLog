using Microsoft.Extensions.Logging;
using INVApp.Services;
using INVApp.ViewModels;
using INVApp.Views;
using ZXing.Net.Maui.Controls;
using CommunityToolkit.Maui;

namespace INVApp
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseBarcodeReader()
                .UseMauiCommunityToolkit()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("Lato-Regular.ttf", "LatoRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");

                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<SoundService>();

            builder.Services.AddTransient<StockOverviewPage>();
            builder.Services.AddTransient<StockIntakePage>();
            builder.Services.AddTransient<POSPage>();
            builder.Services.AddTransient<SettingsPage>();


            builder.Services.AddTransient<StockOverviewViewModel>();
            builder.Services.AddTransient<StockIntakeViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
