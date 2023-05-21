using PicTrans.Models;
using SQLite;

namespace PicTrans.Services;
public class ImageData : IImageData
{
    private SQLiteAsyncConnection _connection;
    public ImageData()
    {
        SetUpDb();
    }

    private void SetUpDb()
    {
        if (_connection is null)
        {
            string path = Path.Combine(Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), "Image.db3");
            _connection = new SQLiteAsyncConnection(path);
            _connection.CreateTableAsync<ImageModel>();
        }
    }

    public async Task<List<ImageModel>> GetImagesAsync()
    {
        return await _connection.Table<ImageModel>().ToListAsync();
    }

    public async Task<int> AddImageAsync(ImageModel image)
    {
        return await _connection.InsertAsync(image);
    }

    public async Task<int> DeleteImage(ImageModel image)
    {
        return await _connection.DeleteAsync(image);
    }
}
