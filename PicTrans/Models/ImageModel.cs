using SQLite;

namespace PicTrans.Models;
public class ImageModel
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }
    public string FileName { get; set; }
    public long FileSize { get; set; }
    public MemoryStream FileData { get; set; }
    public DateTime DateConverted { get; set; } = DateTime.UtcNow;
}
