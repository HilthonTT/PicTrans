using PicTrans.Models;

namespace PicTrans.Services;
public interface ISettingsService
{
    Task<SettingsModel> GetSettings();
    Task<int> SaveSettings(SettingsModel settings);
}