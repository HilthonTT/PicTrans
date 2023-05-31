using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Converters;
public class FileChecker : IFileChecker
{
    public string GetDocumentExtension(IBrowserFile inputFile)
    {
        return Path.GetExtension(inputFile.Name);
    }
}
