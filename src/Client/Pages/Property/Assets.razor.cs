using FSH.BlazorWebAssembly.Client.Components.Common;
using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.BlazorWebAssembly.Client.Infrastructure.Common;
using FSH.WebApi.Shared.Authorization;
using Mapster;
using Microsoft.AspNetCore.Components.Forms;
using MudBlazor;

namespace FSH.BlazorWebAssembly.Client.Pages.Property;

public partial class Assets
{
    public IBrowserFile? File2Upload { get; set; }
    protected EntityServerTableContext<AssetDto, Guid, AssetViewModel> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Asset"],
            entityNamePlural: L["Assets"],
            entityResource: FSHResource.Assets,
            fields: new()
            {
                // new(Asset => Asset.Id, L["Id"], "Id"),
                new(asset => asset.Code, L["Code"], "Code"),
                new(asset => asset.Name, L["Name"], "Name"),
                new(asset => asset.Serial, L["Serial"], "Serial"),
                new(asset => asset.Description, L["Description"], "Description"),
                new(asset => asset.QualityStatusName, L["Quality"], "Quality"),
                new(asset => asset.UsingStatusName, L["Status"], "Status"),
                new(asset => asset.EmployeeFirstName, L["FirstName"], "FirstName"),
                new(asset => asset.EmployeeLastName, L["LastName"], "LastName"),
            },
            idFunc: asset => asset.Id,
            searchFunc: async filter => (await AssetsClient
                .SearchAsync(filter.Adapt<SearchAssetsRequest>()))
                .Adapt<PaginationResponse<AssetDto>>(),
            createFunc: async asset =>
            {
                if (!string.IsNullOrEmpty(asset.ImageInBytes))
                {
                    asset.Image = new FileUploadRequest() { Data = asset.ImageInBytes, Extension = asset.ImageExtension ?? string.Empty, Name = $"{asset.Name}_{Guid.NewGuid():N}" };
                }

                await AssetsClient.CreateAsync(asset.Adapt<CreateAssetRequest>());
                asset.ImageInBytes = string.Empty;
            },
            updateFunc: async (id, asset) =>
            {
                if (!string.IsNullOrEmpty(asset.ImageInBytes))
                {
                    asset.DeleteCurrentImage = true;
                    asset.Image = new FileUploadRequest() { Data = asset.ImageInBytes, Extension = asset.ImageExtension ?? string.Empty, Name = $"{asset.Name}_{Guid.NewGuid():N}" };
                }

                await AssetsClient.UpdateAsync(id, asset);
                asset.ImageInBytes = string.Empty;

            },
            deleteFunc: async id => await AssetsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportAssetsRequest>();
                return await AssetsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportAssetsRequest() { ExcelFile = FileUploadRequest };
                await AssetsClient.ImportAsync(request);
            }
            );

    private async Task UploadFiles(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedImageFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Image Format Not Supported.", Severity.Error);
                return;
            }

            Context.AddEditModal.RequestModel.ImageExtension = extension;
            var imageFile = await e.File.RequestImageFileAsync(ApplicationConstants.StandardImageFormat, ApplicationConstants.MaxImageWidth, ApplicationConstants.MaxImageHeight);
            byte[]? buffer = new byte[imageFile.Size];
            await imageFile.OpenReadStream(ApplicationConstants.MaxImageFileSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.ImageInBytes = $"data:{ApplicationConstants.StandardImageFormat};base64,{Convert.ToBase64String(buffer)}";
            Context.AddEditModal.ForceRender();
        }
    }

    public void ClearImageInBytes()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.ForceRender();
    }

    public void SetDeleteCurrentImageFlag()
    {
        Context.AddEditModal.RequestModel.ImageInBytes = string.Empty;
        Context.AddEditModal.RequestModel.ImagePath = string.Empty;
        Context.AddEditModal.RequestModel.DeleteCurrentImage = true;
        Context.AddEditModal.ForceRender();
    }

    private async Task UploadDoccumentFiles(InputFileChangeEventArgs e)
    {
        if (e.File != null)
        {
            File2Upload = e.File;
            string? extension = Path.GetExtension(e.File.Name);
            if (!ApplicationConstants.SupportedDoccumentFormats.Contains(extension.ToLower()))
            {
                Snackbar.Add("Doccument Format Not Supported.", Severity.Error);
                File2Upload = null;
                return;
            }

            Context.AddEditModal.RequestModel.DoccumentExtension = extension;
            byte[]? buffer = new byte[File2Upload.Size];
            await File2Upload.OpenReadStream(ApplicationConstants.MaxDoccumentFileSize).ReadAsync(buffer);
            Context.AddEditModal.RequestModel.DoccumentInBytes = $"data:{ApplicationConstants.StandardDoccumentFormat};base64,{Convert.ToBase64String(buffer)}";

            Context.AddEditModal.ForceRender();
        }
    }

    public void ClearDoccumentInBytes()
    {
        File2Upload = null;
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

}

public class AssetViewModel : UpdateAssetRequest
{
    public string? ImagePath { get; set; }
    public string? ImageInBytes { get; set; }
    public string? ImageExtension { get; set; }

    public string QRCodeImageInBytes => QRCodeHelper.GenerateQRCode(Barcode ?? string.Empty, 20);

    public string? DoccumentPath { get; set; }
    public string? DoccumentInBytes { get; set; }
    public string? DoccumentExtension { get; set; }
}