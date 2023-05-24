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
}
