using PicTrans.Models;

namespace PicTrans.Services;
public interface IImageData
{
    Task<int> AddImageAsync(ImageModel image);
    Task<int> DeleteImage(ImageModel image);
    Task<List<ImageModel>> GetImagesAsync();
}