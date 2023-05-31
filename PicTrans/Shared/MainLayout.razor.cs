using MudBlazor;
using PicTrans.Models;

namespace PicTrans.Shared;

public partial class MainLayout
{
    private MudTheme theme = new();
    private bool isDarkMode = true;
    private SettingsModel settings;
    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        settings = await settingsService.GetSettings();
        if (settings is not null)
        {
            isDarkMode = settings.IsDarkMode;
        }
        else
        {
            isDarkMode = true;
        }
    }
}