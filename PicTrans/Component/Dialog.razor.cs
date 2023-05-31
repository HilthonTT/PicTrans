using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace PicTrans.Component;

public partial class Dialog
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; }

    void Submit() => MudDialog.Close(DialogResult.Ok(true));
    void Cancel() => MudDialog.Cancel();
}