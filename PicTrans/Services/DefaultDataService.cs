namespace PicTrans.Services;
public class DefaultDataService : IDefaultDataService
{
    public List<string> GetPictureFormats()
    {
        return new List<string>()
        {
            ".jpg",
            ".jpeg",
            ".png",
            ".gif",
            ".bmp",
            ".tiff",
            ".webp",
        };
    }

    public List<string> GetFolderPaths()
    {
        return new List<string>()
        {
            "Download Folder",
            "Picture Folder",
            "Document Folder",
            "Video Folder",
            "Desktop",
        };
    }

    public List<string> GetFileFormats()
    {
        return new List<string>()
        {
            ".docx",
            ".pdf",
            ".txt",
            ".html",
        };
    }
}
