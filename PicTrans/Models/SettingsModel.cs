using SQLite;

namespace PicTrans.Models;
public class SettingsModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public bool IsDarkMode { get; set; } = true;
}
