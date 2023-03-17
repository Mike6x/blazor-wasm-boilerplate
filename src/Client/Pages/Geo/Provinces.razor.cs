using FSH.BlazorWebAssembly.Client.Components.EntityTable;
using FSH.BlazorWebAssembly.Client.Infrastructure.ApiClient;
using FSH.WebApi.Shared.Authorization;
using Mapster;

namespace FSH.BlazorWebAssembly.Client.Pages.Geo;

public partial class Provinces
{
    protected EntityServerTableContext<ProvinceDto, Guid, UpdateProvinceRequest> Context { get; set; } = default!;

    protected override void OnInitialized() =>
        Context = new(
            entityName: L["Province"],
            entityNamePlural: L["Provinces"],
            entityResource: FSHResource.Provinces,
            fields: new()
            {
                new(Province => Province.Order, L["Order"], "Order"),
                new(Province => Province.NumericCode, L["Numeric"], "NumericCode"),
                new(Province => Province.Code, L["Code"], "Code"),
                new(Province => Province.Name, L["Name"], "Name"),
                new(Province => Province.TypeName, L["Type"], "Type" ),
                new(Province => Province.NativeName, L["Navtive Name"], "NativeName"),
                new(Province => Province.TypeNativeName, L["Type"], "Type" ),
                new(Province => Province.StateName, L["State"], "State"),
                new(Province => Province.StateNativeName, L["State"], "StateNativeName"),

                // new(Province => Province.Metropolis, L["Metropolis"], "Metropolis"),
                // new(Province => Province.Description, L["Description"], "Description"),
            },
            idFunc: Province => Province.Id,
            searchFunc: async filter => (await ProvincesClient
                .SearchAsync(filter.Adapt<SearchProvincesRequest>()))
                .Adapt<PaginationResponse<ProvinceDto>>(),
            createFunc: async Province => await ProvincesClient.CreateAsync(Province.Adapt<CreateProvinceRequest>()),
            updateFunc: async (id, Province) => await ProvincesClient.UpdateAsync(id, Province),
            deleteFunc: async id => await ProvincesClient.DeleteAsync(id),
            exportFunc: async filter =>
            {
                var exportFilter = filter.Adapt<ExportProvincesRequest>();
                return await ProvincesClient.ExportAsync(exportFilter);
            },
            importFunc: async FileUploadRequest =>
            {
                var request = new ImportProvincesRequest() { ExcelFile = FileUploadRequest };
                await ProvincesClient.ImportAsync(request);
            }
            );
}