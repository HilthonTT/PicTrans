using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Converters;
public interface IFileChecker
{
    string GetDocumentExtension(IBrowserFile inputFile);
}