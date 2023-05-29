using Microsoft.Extensions.Logging;
using MudBlazor.Services;
using PicTrans.Helpers;
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
        builder.Services.AddSingleton<IDefaultDataService, DefaultDataService>();
        builder.Services.AddSingleton<IPathService, PathService>();
        builder.Services.AddSingleton<IFileService, FileService>();
        builder.Services.AddSingleton<ISettingsService, SettingsService>();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
    }
}
