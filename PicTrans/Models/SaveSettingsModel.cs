using System.ComponentModel.DataAnnotations;

namespace PicTrans.Models;
internal class SaveSettingsModel
{
    [Display(Name = "Dark Mode")]
    [Required(ErrorMessage = "You must set your dark mode settings.")]
    public bool IsDarkMode { get; set; } = true;
}
