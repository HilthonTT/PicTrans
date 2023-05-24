namespace PicTrans.Services;

public interface IDefaultDataService
{
    List<string> GetFolderPaths();
    List<string> GetPictureFormats();
}