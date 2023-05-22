using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using PicTrans.Services;

namespace PicTrans;
public static class RegisterServices
{
    public static void ConfigureServices(this MauiAppBuilder builder)
    {
        builder.Services.AddMauiBlazorWebView();
        builder.Services.AddMudServices();
        builder.Services.AddSingleton<ISecureStorage, SecureStorageWrapper>();
        builder.Services.AddSingleton<IImageService, ImageService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
    }
}
