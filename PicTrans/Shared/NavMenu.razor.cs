namespace PicTrans.Shared;

public partial class NavMenu
{
    private void LoadHomePage()
    {
        navManager.NavigateTo("/");
    }

    private void LoadFileConverterPage()
    {
        navManager.NavigateTo("/FileConverter");
    }

    private void LoadAuthorGithubPage()
    {
        navManager.NavigateTo("https://github.com/HilthonTT");
    }

    private void LoadSettingsPage()
    {
        navManager.NavigateTo("/Settings");
    }
}