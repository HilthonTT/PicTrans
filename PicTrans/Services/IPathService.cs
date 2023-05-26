using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Services;
public interface IPathService
{
    string GetFilePath(IBrowserFile file, string selectedPath, string selectedExtension);
}