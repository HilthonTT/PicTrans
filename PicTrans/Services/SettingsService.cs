using PicTrans.Models;
using SQLite;

namespace PicTrans.Services;
public class SettingsService : ISettingsService
{
    private SQLiteAsyncConnection _cnn;

    public SettingsService()
    {
        SetUpDb();
    }

    private void SetUpDb()
    {
        if (_cnn is null)
        {
            string dbPath = Path.Combine(
                Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData),
                "Settings.db3");
            _cnn = new SQLiteAsyncConnection(dbPath);
            _cnn.CreateTableAsync<SettingsModel>();
        }
    }

    public async Task<int> SaveSettings(SettingsModel settings)
    {
        var existingSettings = await _cnn.Table<SettingsModel>().FirstOrDefaultAsync();
        if (existingSettings is not null)
        {
            settings.Id = existingSettings.Id;
            return await _cnn.UpdateAsync(settings);
        }
        else
        {
            return await _cnn.InsertAsync(settings);
        }
    }

    public async Task<SettingsModel> GetSettings()
    {
        return await _cnn.Table<SettingsModel>().FirstOrDefaultAsync();
    }
}
