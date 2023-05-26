using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Services;
public class PathService : IPathService
{
    public string GetFilePath(
        IBrowserFile file,
        string selectedPath,
        string selectedExtension)
    {
        return selectedPath switch
        {
            "Download Folder" => GetDefaultDownloadPath(file, selectedExtension),
            _ => GetSelectedPath(file, selectedExtension, selectedPath),
        };
    }

    private static string GetDefaultDownloadPath(IBrowserFile file, string selectedExtension)
    {
        string downloadsFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string fileName = Path.GetFileNameWithoutExtension(file.Name) + selectedExtension;
        return Path.Combine(downloadsFolder, "Downloads", fileName);
    }

    private static string GetSelectedPath(IBrowserFile file, string selectedExtension, string path)
    {
        string downloadsFolder = Environment.GetFolderPath(GetFolder(path));
        string fileName = Path.GetFileNameWithoutExtension(file.Name) + selectedExtension;
        return Path.Combine(downloadsFolder, fileName);
    }

    private static Environment.SpecialFolder GetFolder(string path)
    {
        return path switch
        {
            "Picture Folder" => Environment.SpecialFolder.MyPictures,
            "Document Folder" => Environment.SpecialFolder.MyDocuments,
            "Video Folder" => Environment.SpecialFolder.MyVideos,
            "Desktop" => Environment.SpecialFolder.Desktop,
            _ => throw new NotImplementedException(),
        };
    }
}
