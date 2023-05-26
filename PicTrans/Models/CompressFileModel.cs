using System.ComponentModel.DataAnnotations;

namespace PicTrans.Models;
public class CompressFileModel
{
    [Required]
    [Display(Name = "File Extension")]
    public string FileExtension { get; set; } = ".png";

    [Required]
    [Display(Name = "Selected Path")]
    public string SelectedPath { get; set; } = "Download Folder";

    [Required]
    [Display(Name = "Max Width")]
    public int MaxWidth { get; set; } = 800;

    [Required]
    [Display(Name = "Max Height")]
    public int MaxHeight { get; set; } = 800;
}
