using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Property;

public partial class AssetHistorys
{
    public IBrowserFile? UploadFile { get; set; }
    protected EntityServerTableContext<AssetHistoryDto, Guid, AssetHistoryViewModel> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["AssetHistory"],
            entityNamePlural: L["AssetHistorys"],
            entityResource: FSHResource.AssetHistorys,
            fields: new()
            {
                // new(AssetHistory => AssetHistory.Id, L["Id"], "Id"),
                new(AssetHistory => AssetHistory.AssetName, L["Asset Name"], "Asset.Name"),
                new(AssetHistory => AssetHistory.PreviousQualityStatusName, L["Prev Quality Status"], "PreviousQualityStatus.Name"),
                new(AssetHistory => AssetHistory.QualityStatusName, L["Quality Status"], "QualityStatus.Name" ),
                new(AssetHistory => AssetHistory.PreviousUsingStatusName, L["Prev Using Status"], "PreviousUsingStatus.Name"),
                new(AssetHistory => AssetHistory.UsingStatusName, L["Using Status"], "UsingStatus.Name" ),
                new(AssetHistory => AssetHistory.DoccumentLink, L["DoccumentLink"], "DoccumentLink"),
                new(AssetHistory => AssetHistory.Note, L["Note"], "Note"),
                new(AssetHistory => AssetHistory.EmployeeFirstName, L["Owner.F.Name"], "Employee.FirstName"),
                new(AssetHistory => AssetHistory.EmployeeLastName, L["Owner.L.Name"], "Employee.LastName"),
                new(AssetHistory => AssetHistory.LastModifiedOn, L["Date"], "LastModifiedOn"),
            },
            idFunc: AssetHistory => AssetHistory.Id,
            searchFunc: async filter => (await AssetHistorysClient
                .SearchAsync(filter.Adapt<SearchAssetHistorysRequest>()))
                .Adapt<PaginationResponse<AssetHistoryDto>>(),

            createFunc: async AssetHistory =>
            {
                if (!string.IsNullOrEmpty(AssetHistory.DoccumentInBytes))
                {
                    AssetHistory.Doccument = new FileUploadRequest()
                    {
                        Data = AssetHistory.DoccumentInBytes,
                        Extension = AssetHistory.DoccumentExtension ?? string.Empty,
                        Name = $"{AssetHistory.AssetId}_{Guid.NewGuid():N}"
                    };
                }

                await AssetHistorysClient.CreateAsync(AssetHistory.Adapt<CreateAssetHistoryRequest>());
                AssetHistory.DoccumentInBytes = string.Empty;
            },

            updateFunc: async (id, AssetHistory) =>
            {
                if (!string.IsNullOrEmpty(AssetHistory.DoccumentInBytes))
                {
                    AssetHistory.DeleteCurrentDoccument = true;
                    AssetHistory.Doccument = new FileUploadRequest()
                    {
                        Data = AssetHistory.DoccumentInBytes,
                        Extension = AssetHistory.DoccumentExtension ?? string.Empty,
                        Name = $"{AssetHistory.AssetId}_{Guid.NewGuid():N}"
                    };
                }

                await AssetHistorysClient.UpdateAsync(id, AssetHistory.Adapt<UpdateAssetHistoryRequest>());
                AssetHistory.DoccumentInBytes = string.Empty;
            },

            deleteFunc: async id => await AssetHistorysClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportAssetHistorysRequest>();
                return await AssetHistorysClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportAssetHistorysRequest() { ExcelFile = FileUploadRequest };
                await AssetHistorysClient.ImportAsync(request);
            }
            );

    public void ClearDoccumentInBytes()
    {
        UploadFile = null;
        Context.AddEditModal.RequestModel.DoccumentInBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    public void SetDeleteCurrentDoccumentFlag()
    {
        Context.AddEditModal.RequestModel.DoccumentInBytes = string.Empty;
        Context.AddEditModal.RequestModel.DoccumentPath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentDoccument = true;
        Context.AddEditModal.ForceRender();
    }

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            UploadFile = e.File;
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedDoccumentFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Doccument Format Not Supported.", Severity.Error);
                UploadFile = null;
                return;
            }

            Context.AddEditModal.RequestModel.DoccumentExtension = extension;
            byte[]? buffer = new byte[UploadFile.Size];
            await UploadFile.OpenReadStream(ApplicationConstants.MaxDoccumentFileSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.DoccumentInBytes = $"data:{ApplicationConstants.StandardDoccumentFormat};base64,{Convert.ToBase64String(buffer)}";

            Context.AddEditModal.ForceRender();
        }
    }
}

public class AssetHistoryViewModel : UpdateAssetHistoryRequest
{
    public string? DoccumentPath { get; set; }
    public string? DoccumentInBytes { get; set; }
    public string? DoccumentExtension { get; set; }
}
