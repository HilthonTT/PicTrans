using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using PicTrans.Data;
using PicTrans.Services;

namespace PicTrans;
public static class RegisterServices
{
    public static void ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<ISecureStorage, SecureStorageWrapper>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<WeatherForecastService>();
    }
}
