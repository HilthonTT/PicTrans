using Microsoft.AspNetCore.Components.Forms;

namespace PicTrans.Converters;
public interface ITxtConverter
{
    Task<byte[]> ConvertToTxtAsync(IBrowserFile inputFile);
}