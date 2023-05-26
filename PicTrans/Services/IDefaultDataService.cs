namespace PicTrans.Services;

public interface IDefaultDataService
{
    List<string> GetFileFormats();
    List<string> GetFolderPaths();
    List<string> GetPictureFormats();
}