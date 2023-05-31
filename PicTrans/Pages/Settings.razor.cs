using PicTrans.Models;

namespace PicTrans.Pages;

public partial class Settings
{
    private SaveSettingsModel model = new();
    private SettingsModel settings;
    private string errorMessage = "";
    protected override async Task OnInitializedAsync()
    {
        await LoadSettings();
    }

    private async Task LoadSettings()
    {
        settings = await settingsService.GetSettings();
        if (settings is not null)
        {
            model.IsDarkMode = settings.IsDarkMode;
        }
        else
        {
            settings.IsDarkMode = true;
        }
    }

    private async Task SaveSettings()
    {
        try
        {
            errorMessage = "";
            var s = new SettingsModel
            {
                IsDarkMode = model.IsDarkMode,
            };
            if (settings is not null)
            {
                s.Id = settings.Id;
            }

            await settingsService.SaveSettings(s);
            navManager.NavigateTo("/Settings", true);
        }
        catch
        {
            errorMessage = "Failed to save settings";
        }
    }
}