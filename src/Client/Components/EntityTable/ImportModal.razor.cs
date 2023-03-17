using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.BlazorWebAssembly.Client.Shared;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;
using System.Diagnostics;

namespace FSH.BlazorWebAssembly.Client.Components.EntityTable;

public partial class ImportModal
{
    [Parameter]
    public string? ModelName { get; set; } = string.Empty;

    [Parameter]
    public Func<Task>? OnInitializedFunc { get; set; }

    [Parameter]
    public FileUploadRequest UploadModel { get; set; } = new();

    [Parameter]
    [EditorRequired]
    public Func<FileUploadRequest, Task> ImportFunc { get; set; } = default!;

    // public string? SuccessMessage { get; set; }
    private CustomValidation? _customValidation;
    [CascadingParameter]
    private MudDialogInstance MudDialog { get; set; } = default!;

    private IBrowserFile? _file;
    private bool _uploading;

    protected override Task OnInitializedAsync() =>
        OnInitializedFunc is not null
            ? OnInitializedFunc()
            : Task.CompletedTask;

    private async Task SaveAsync()
    {
        Stopwatch stopwatch = new Stopwatch();
        stopwatch.Start();
        _uploading = true;

        // if (await ApiHelper.ExecuteCallGuardedAsync(
        //   () => ImportFunc(UploadModel), Snackbar, _customValidation, SuccessMessage))
        // {
        //    _uploading = false;
        //    MudDialog.Close();
        // }

        if (await ApiHelper.ExecuteCallGuardedAsync(
           () => ImportFunc(UploadModel), Snackbar))
        {
            _uploading = false;
            MudDialog.Close();
        }

        _uploading = false;
        stopwatch.Stop();
        TimeSpan ts = stopwatch.Elapsed;
        Snackbar.Add(string.Format("Processing time is about {0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        _file = e.File;
        if (_file is not null)
        {
            if (_file.Size >= ApplicationConstants.MaxExcelFileSize)
            {
                Snackbar.Add(@L["File have size too big !"], Severity.Error);
                _file = null;
                return;
            }

            string? extension = Path.GetExtension(_file.Name);
            if (!ApplicationConstants.SupportedExcelFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add(@L["File Format Not Supported."], Severity.Error);
                return;
            }

            byte[]? buffer = new byte[_file.Size];
            await _file.OpenReadStream(_file.Size).ReadAsync(buffer);
            string? base64String = $"data:{ApplicationConstants.StandardExcelFormat};base64,{Convert.ToBase64String(buffer)}";

            UploadModel = new FileUploadRequest
            {
                Name = _file.Name,
                Extension = extension,
                Data = base64String,
            };
        }
    }
}