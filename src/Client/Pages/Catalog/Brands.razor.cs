using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Catalog;

public partial class Brands
{
    protected EntityServerTableContext<BrandDto, Guid, UpdateBrandRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Brand"],
            entityNamePlural: L["Brands"],
            entityResource: FSHResource.Brands,
            fields: new()
            {
               // new(brand => brand.Id, L["Id"], "Id"),
                new(brand => brand.Code, L["Code"], "Code"),
                new(brand => brand.Name, L["Name"], "Name"),
                new(brand => brand.Description, L["Description"], "Description"),
                new(brand => brand.IsActive, L["Active"], Type: typeof(bool)),
            },
            idFunc: brand => brand.Id,
            searchFunc: async filter => (await BrandsClient
                .SearchAsync(filter.Adapt<SearchBrandsRequest>()))
                .Adapt<PaginationResponse<BrandDto>>(),
            createFunc: async brand => await BrandsClient.CreateAsync(brand.Adapt<CreateBrandRequest>()),
            updateFunc: async (id, brand) => await BrandsClient.UpdateAsync(id, brand),
            deleteFunc: async id => await BrandsClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportBrandsRequest>();
                return await BrandsClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportBrandsRequest() { ExcelFile = FileUploadRequest };
                await BrandsClient.ImportAsync(request);
            }

            // exportAction: string.Empty
            );
}
